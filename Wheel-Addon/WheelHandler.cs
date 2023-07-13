using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OTD.EnhancedOutputMode.Lib.Interface;
using OTD.Backport.Parsers.Tablet;
using System;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;
using System.Threading.Tasks;
using System.Threading;
using WheelAddon.RPC;

namespace WheelAddon.Filter
{
    [PluginName(PLUGIN_NAME)]
    public class WheelHandler : IFilter, IAuxFilter, IDisposable
    {
        public const string PLUGIN_NAME = "Wheel Addon";

        private RpcServer<WheelDaemon> rpcServer;
        private Settings settings = Settings.Default;

        private CancellationTokenSource debounceToken = new();

        public static event EventHandler<IWheelReport> OnWheelUsage = null!;
        public static event EventHandler<IWheelReport> OnWheelFingerUp = null!;
        public static event EventHandler<bool> OnWheelAction = null!;

        public static event EventHandler<LogMessage> OnLog = null!;

        public bool Touching { get; set; }
        public int Delta { get; set; }
        public int LastValue { get; set; } = -1;
        public int LastNonNullValue { get; set; } = -1;
        public int RemainingTouchesDebounce { get; set; } = -1;

        public WheelHandler()
        {
            // define events
            OnWheelUsage += OnWheelUsed;
            OnWheelFingerUp += HandleAdvancedMode;
            OnWheelAction += HandleSimpleModeAction;

            OnLog += OnLogSent;

            // start the RPC server
            rpcServer = new RpcServer<WheelDaemon>("WheelDaemon");

            rpcServer.Instance.OnSettingsChanged += (_, s) =>
            {
                this.settings = s;

                Log.Debug(PLUGIN_NAME, "Settings updated");
            };

            rpcServer.Instance.Initialize();

            _ = Task.Run(() => rpcServer.MainAsync());
        }

        public Vector2 Filter(Vector2 input) => input;

        public IAuxReport AuxFilter(IAuxReport report)
        {
            if (report is IWheelReport wheelReport)
            {
                if (rpcServer.Instance.IsReady)
                    OnWheelUsage?.Invoke(this, wheelReport);
            }

            return report;
        }

        public void OnWheelUsed(object? sender, IWheelReport report)
        {
            if (rpcServer.Instance.IsCalibrating || rpcServer.Instance.HasErrored)
                return;

            if (report.Wheel == -1)
            {
                if (RemainingTouchesDebounce-- > 0)
                {
                    LastValue = -1;

                    _ = Task.Run(() =>
                    {
                        // cancel the debounce token
                        debounceToken.Cancel();

                        // create a new token
                        debounceToken = new CancellationTokenSource();

                        // start the debounce, call HandleUpDebounce after the debounce time
                        try
                        {
                            _ = Task.Delay(DebounceAfterTouch, debounceToken.Token)
                                .ContinueWith(_ => HandleUpDebounceAsync(report), TaskScheduler.Default);
                        }
                        catch(OperationCanceledException)
                        {}
                    });

                    return;
                }

                Touching = false;

                OnWheelFingerUp?.Invoke(this, report);
            }
            else
            {
                //if (LastValue == -1 && RemainingTouchesDebounce < 0)
                    //Log.Debug(PLUGIN_NAME, "Wheel: Finger Down");

                Touching = true;

                if (ModeToggle == false)
                    HandleSimpleMode(report);

                // reset the debounce
                RemainingTouchesDebounce = DebounceDuringTouch;
                LastNonNullValue = report.Wheel;
            }

            LastValue = report.Wheel;
        }

        public Task HandleUpDebounceAsync(IWheelReport report)
        {
            if (LastValue == -1)
            {
                //Log.Debug(PLUGIN_NAME, "Wheel: Finger Up");

                OnWheelFingerUp?.Invoke(this, report);

                RemainingTouchesDebounce = -1;

                Delta = 0;

                Touching = false;
            }

            return Task.CompletedTask;
        }

        #region Simple Mode

        public void HandleSimpleMode(IWheelReport report)
        {
            if (LastValue == -1)
                return;

            // handle the case where the wheel goes from the max value to the min value, using the LargeRotationThreshold
            if (Math.Abs(LastValue - report.Wheel) > LargeRotationThreshold)
            {
                if (LastValue > report.Wheel)
                {
                    // going from max to min
                    Delta += settings.MaxWheelValue - LastValue + report.Wheel;
                }
                else
                {
                    // going from min to max
                    Delta += settings.MaxWheelValue - report.Wheel + LastValue;
                }
            }
            else
            {
                Delta += report.Wheel - LastValue;
            }

            //Log.Debug(PLUGIN_NAME, $"Wheel: Delta: {Delta}");

            // in the case Delta is greater than the RotationThreshold, we need to send the events
            var actionsToExecute = Math.Abs(Delta) / RotationThreshold;

            if (actionsToExecute > 0)
            {
                //Log.Debug(PLUGIN_NAME, $"Wheel: Was rotated {actionsToExecute} times {((Delta > 0) ? "clockwise" : "counter-clockwise")}");

                for (var i = 0; i < actionsToExecute; i++)
                {
                    // send the event
                    OnWheelAction?.Invoke(this, Delta > 0);

                    // decrement the delta
                    if (Delta > 0)
                        Delta -= RotationThreshold;
                    else
                        Delta += RotationThreshold;
                }
            }
        }

        public void HandleSimpleModeAction(object? sender, bool isClockwise)
        {
            var simpleMode = settings.SimpleMode;

            IBinding? binding = isClockwise ? simpleMode.Clockwise.Binding : simpleMode.CounterClockwise.Binding;

            if (binding == null)
                return;

            // press the binding
            PressMomentarily(binding);
        }

        #endregion

        #region Advanced Mode

        public void HandleAdvancedMode(object? sender, IWheelReport report)
        {
            if (ModeToggle != true)
                return;

            // find the slices in question in which the wheel is currently in

            var advancedMode = settings.AdvancedMode;

            var foundAtIndex = -1;

            for (var i = 0; i < advancedMode.Count; i++)
            {
                var slice = advancedMode[i];

                if (slice.End >= LastNonNullValue && LastNonNullValue >= slice.Start)
                {
                    // check if there is already a binding defined
                    if (foundAtIndex != -1)
                    {
                        // slice x overlaps with slice y
                        HandlerLog(this, PLUGIN_NAME, $"Slice {foundAtIndex + 1} overlaps with slice {i + 1}", LogLevel.Error);
                        return;
                    }
                    else
                    {
                        foundAtIndex = i;
                    }
                }
            }

            if (foundAtIndex != -1)
            {
                HandleAdvancedModeAction(foundAtIndex);
            }
        }

        public void HandleAdvancedModeAction(int index)
        {
            var advancedMode = settings.AdvancedMode;

            // get the binding
            var binding = advancedMode[index].Binding;

            if (binding == null)
                return;

            // press the binding
            PressMomentarily(binding);
        }

        public void PressMomentarily(IBinding binding)
        {
            _ = Task.Run(async () =>
            {
                binding.Press();
                await Task.Delay(15);
                binding.Release();
            });
        }

        #endregion

        #region Logging

        public void OnLogSent(object? sender, LogMessage message)
        {
            Log.Write(message.Group, message.Message, message.Level);
        }

        public static void HandlerLog(object? sender, string group, string message, LogLevel level = LogLevel.Info)
        {
            OnLog?.Invoke(sender, new LogMessage(group, message, level));
        }

        public static void HandlerLogSpacer(object? sender, string group)
        {
            OnLog?.Invoke(sender, new LogMessage(group, "----------------------------------------", LogLevel.Info));
        }

        public void Dispose()
        {
            // unsubscribe from the events
            OnLog -= OnLogSent;
            OnWheelAction -= HandleSimpleModeAction;
            OnWheelFingerUp -= HandleAdvancedMode;

            // dispose of the settings
            settings = null!;

            // dispose of the rpc server
            rpcServer?.Dispose();
            rpcServer = null!;
        }

        #endregion

        public FilterStage FilterStage => FilterStage.PreTranspose;

        [BooleanProperty("Toggle Mode", ""),
         DefaultPropertyValue(false),
         ToolTip("Wheel Addon:\n\n" +
                 "OFF: Simple Mode\n" +
                 "ON: Advanced Mode\n\n" +
                 "The state of this checkbox determines which mode is used.")
        ]
        public bool ModeToggle { set; get; }

        [Property("Rotation Threshold"),
         Unit("au"),
         DefaultPropertyValue(20),
         ToolTip("Wheel Addon:\n\n" +
                 "/!\\ Only Available in Simple Mode /!\\ \n\n" +
                 "The threshold before the designated action is performed.")
        ]
        public int RotationThreshold { get; set; }

        [Property("Large Rotation Threshold"),
         Unit("au"),
         DefaultPropertyValue(30),
         ToolTip("Wheel Addon:\n\n" +
                 "/!\\ Only Available in Simple Mode /!\\ \n\n" +
                 "The threshold at which a rotation is considered large. \n\n" +
                 "Modifying this value could solve issues with sudden direction changes.")
        ]
        public int LargeRotationThreshold { get; set; }

        [Property("Debounce During Touch"),
         Unit("packets"),
         DefaultPropertyValue(1),
         ToolTip("Wheel Addon:\n\n" +
                 "The number packets to debounce after releasing the wheel. \n\n" +
                 "Modifying this value could solve issues with unwanted actions.")
        ]
        public int DebounceDuringTouch { get; set; }

        [Property("Debounce After Touch"),
         Unit("ms"),
         DefaultPropertyValue(30),
         ToolTip("Wheel Addon:\n\n" +
                 "The number ms to debounce after touching the wheel. \n\n" +
                 "Modifying this value could solve issues with unwanted actions.")
        ]
        public int DebounceAfterTouch { get; set; }


    }
}
