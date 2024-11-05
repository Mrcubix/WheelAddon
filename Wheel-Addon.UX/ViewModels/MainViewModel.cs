using System;
using System.Collections.ObjectModel;
using Avalonia.Media;
using WheelAddon.Lib.RPC;
using WheelAddon.Lib.Contracts;
using WheelAddon.Lib.Serializables;
using WheelAddon.UX.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using WheelAddon.Lib.Serializables.Bindings;
using WheelAddon.Lib.Serializables.Modes;
using Avalonia.Threading;
using System.IO;
using System.Threading;
using OpenTabletDriver.External.Common.Serializables;
using OpenTabletDriver.External.Avalonia.ViewModels;

namespace WheelAddon.UX.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private RpcClient<IWheelDaemon> _rpcClient = null!;
    private string _connectionStateText = "Disconnected";

    private ObservableCollection<WheelBindingDisplayViewModel> _displays;
    private ObservableCollection<SliceDisplayViewModel> _slices = null!;
    private int _lastIndex = 0;
    private bool _isEmpty = true;

    private BindingDisplayViewModel _clockWiseBindingDisplay = null!;
    private BindingDisplayViewModel _counterClockWiseBindingDisplay = null!;

    private SerializableSettings _settings = null!;
    private ObservableCollection<SerializablePlugin> _plugins = null!;

    private bool _isConnected = false;

    private bool _isCalibrating = false;
    private string _calibrationButtonText = "Calibrate";
    private CancellationTokenSource _calibrationCancellationTokenSource = null!;
    private int _currentMax = 0;

    public IBrush SeparatorColor { get; } = Brush.Parse("#66FFFFFF");

    #region Simple Bindings

    public BindingDisplayViewModel ClockWiseBindingDisplay
    {
        get => _clockWiseBindingDisplay;
        set => SetProperty(ref _clockWiseBindingDisplay, value);
    }

    public BindingDisplayViewModel CounterClockWiseBindingDisplay
    {
        get => _counterClockWiseBindingDisplay;
        set => SetProperty(ref _counterClockWiseBindingDisplay, value);
    }

    #endregion

    #region Ranged Bindings

    public ObservableCollection<WheelBindingDisplayViewModel> Displays
    {
        get => _displays;
        set => SetProperty(ref _displays, value);
    }

    public ObservableCollection<SliceDisplayViewModel> Slices
    {
        get => _slices;
        set => SetProperty(ref _slices, value);
    }

    public int LastIndex 
    {
        get => _lastIndex;
        set => SetProperty(ref _lastIndex, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    #endregion

    #region RPC

    public RpcClient<IWheelDaemon> Client
    {
        get => _rpcClient;
        set => SetProperty(ref _rpcClient, value);
    }

    public string ConnectionStateText
    {
        get => _connectionStateText;
        set => SetProperty(ref _connectionStateText, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    #endregion

    #region Settings

    public SerializableSettings Settings
    {
        get => _settings;
        set => SetProperty(ref _settings, value);
    }

    public ObservableCollection<SerializablePlugin> Plugins
    {
        get => _plugins;
        set => SetProperty(ref _plugins, value);
    }

    #endregion

    #region Calibration

    public bool IsCalibrating
    {
        get => _isCalibrating;
        set => SetProperty(ref _isCalibrating, value);
    }

    public string CalibrationButtonText
    {
        get => _calibrationButtonText;
        set => SetProperty(ref _calibrationButtonText, value);
    }

    public int CurrentMax
    {
        get => _currentMax;
        set => SetProperty(ref _currentMax, value);
    }

    #endregion

    public MainViewModel() : this(SerializableSettings.Default)
    {
    }

    public MainViewModel(SerializableSettings settings)
    {
        // Instantiate RPC client
        Client = new RpcClient<IWheelDaemon>("WheelDaemon");

        Client.Connected += (sender, args) =>
        {
            ConnectionStateText = "Connected";
        };

        Client.Attached += (sender, args) => Task.Run(async () =>
        {
            var tempPlugins = await FetchPluginsAsync();

            if (tempPlugins != null)
                Plugins = new(tempPlugins);

            OnPluginChanged?.Invoke(this, Plugins);

            var temp = await FetchSettingsAsync();

            if (temp != null)
            {
                Settings = temp;
                CurrentMax = Settings.MaxWheelValue;

                // build the interface on the UI thread
                Dispatcher.UIThread.Post(() => BuildInterface(Settings));
            }

            IsConnected = true;
        });

        Client.Connecting += (sender, args) =>
        {
            ConnectionStateText = "Connecting...";
        };

        Client.Disconnected += (sender, args) =>
        {
            ConnectionStateText = "Disconnected";
            IsConnected = false;

            CalibrationButtonText = "Calibrate";
            IsCalibrating = false;
        };

        _ = Task.Run(() => Client.ConnectAsync());

        // Set settings
        Settings = settings;

        // define events
        OnPluginChanged = null!;
        OnSliceAdded = null!;
        OnSliceRemoved = null!;

        _displays = new ObservableCollection<WheelBindingDisplayViewModel>();
        _slices = new ObservableCollection<SliceDisplayViewModel>();

        // build interface
        BuildInterface(settings);
    }

    public void BuildInterface(SerializableSettings settings)
    {
        // simple mode binding displays
        var simpleModeSettings = settings.SimpleMode;

        ClockWiseBindingDisplay = new BindingDisplayViewModel(  "Clockwise Rotation", 
                                                                GetFriendlyContentFromProperty(simpleModeSettings?.Clockwise.PluginProperty),
                                                                simpleModeSettings?.Clockwise.PluginProperty);

        CounterClockWiseBindingDisplay = new BindingDisplayViewModel("Counter Clockwise Rotation", 
                                                                     GetFriendlyContentFromProperty(simpleModeSettings?.CounterClockwise.PluginProperty), 
                                                                     simpleModeSettings?.CounterClockwise.PluginProperty);

        // wheel binding displays

        // clear existing displays
        Displays.Clear();
        Slices.Clear();

        var advancedModeSettings = settings.AdvancedMode;

        if (advancedModeSettings.Count == 0)
        {
            IsEmpty = true;
            return;
        }
        
        for (var i = 0; i < advancedModeSettings.Count; i++)
        {
            var advancedSettings = advancedModeSettings[i];

            // wheel binding displays

            var display = new WheelBindingDisplayViewModel
            {
                Description = $"Slice {i + 1}",
                Content = GetFriendlyContentFromProperty(advancedSettings.PluginProperty),
                PluginProperty = advancedSettings.PluginProperty,
                SliceColor = ColorExtensions.RandomColor().ToHex(),
                Start = advancedSettings.Start,
                End = advancedSettings.End,
                Max = Settings.MaxWheelValue
            };

            _displays.Add(display);

            // wheel slices

            var slice = new SliceDisplayViewModel(display)
            {
                Color = display.SliceColor
            };

            _slices.Add(slice);

            IsEmpty = false;
        }
    }

    public event EventHandler<ObservableCollection<SerializablePlugin>> OnPluginChanged;
    public event EventHandler<SliceDisplayViewModel> OnSliceAdded;
    public event EventHandler<int> OnSliceRemoved;

    public void AddSlice()
    {
        WheelBindingDisplayViewModel lastSlice = null!;

        var end = Settings.MaxWheelValue;
        var max = Settings.MaxWheelValue;

        // get the last slice
        if (_displays.Count != 0)
            lastSlice = _displays[LastIndex];
        else
        {
            lastSlice = WheelBindingDisplayViewModel.Default;
            end = 20;
        }

        var color = ColorExtensions.RandomColor().ToHex();

        // generate a new slice
        var display = new WheelBindingDisplayViewModel
        {
            Description = "Slice " + (_displays.Count + 1),
            PluginProperty = null!,
            SliceColor = color,
            Start = lastSlice.End,
            End = end,
            Max = max
        };

        _displays.Add(display);

        var slice = new SliceDisplayViewModel(display)
        {
            Color = color
        };

        _slices.Add(slice);

        OnSliceAdded?.Invoke(this, slice);

        LastIndex = _displays.Count - 1;
        IsEmpty = false;
    }

    public void RemoveSlice(int index)
    {
        if (index < 0 || index >= _displays.Count)
            return;

        // remove the slice
        _displays.RemoveAt(index);

        _slices.RemoveAt(index);

        OnSliceRemoved?.Invoke(this, index);

        LastIndex = _displays.Count - 1;
        IsEmpty = _displays.Count == 0;
    }

    #region Commands

    public void AttemptReconnect()
    {
        _ = Task.Run(() => ConnectRpcAsync());
    }

    private async Task ConnectRpcAsync()
    {
        if (!Client.IsConnected)
        {
            try
            {
                await Client.ConnectAsync();
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }
    }

    public void OnApplyEvent()
    {
        if (!Client.IsConnected)
            return;

        // simple mode settings

        Settings.SimpleMode = new SerializableSimpleModeWheelBindings
        {
            Clockwise = new SerializableWheelBinding
            {
                PluginProperty = ClockWiseBindingDisplay.PluginProperty
            },

            CounterClockwise = new SerializableWheelBinding
            {
                PluginProperty = CounterClockWiseBindingDisplay.PluginProperty
            }
        };

        Settings.AdvancedMode = Displays.Select(x => new SerializableRangedWheelBinding
        {
            PluginProperty = x.PluginProperty,
            Start = x.Start,
            End = x.End
        }).ToList();

        // generate the serializable settings

        try
        {
            _ = Client.Instance?.ApplyWheelBindings(Settings);
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    public void OnSaveEvent()
    {
        if (!Client.IsConnected)
            return;

        try
        {
            _ = Client.Instance?.SaveWheelBindings();
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    private async Task<List<SerializablePlugin>?> FetchPluginsAsync()
    {
        if (!Client.IsConnected)
            return null;

        try 
        { 
            return await Client.Instance.GetPlugins(); 
        }
        catch (Exception e)
        {
            HandleException(e);
        }

        return null;
    }

    private async Task<SerializableSettings?> FetchSettingsAsync()
    {
        if (!Client.IsConnected)
            return null;

        try
        {
            return await Client.Instance.GetWheelBindings();
        }
        catch (Exception e)
        {
            HandleException(e);
        }
        
        return null;
    }

    public void ToggleCalibration()
    {
        if (!Client.IsConnected)
            return;

        try
        {
            if (!IsCalibrating)
                _ = Task.Run(StartCalibrationAsync);
            else
                _ = Task.Run(StopCalibrationAsync);
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    public async Task StartCalibrationAsync()
    {
        if (!Client.IsConnected)
            return;

        if (IsCalibrating)
            return;

        try
        {
            var res = await Client.Instance.StartCalibration();

            if (res == true)
            {
                IsCalibrating = true;
                CalibrationButtonText = "Stop Calibration";

                _calibrationCancellationTokenSource = new CancellationTokenSource();

                _ = Task.Run(PeriodicCalibrationTaskAsync);
            }
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    } 

    public async Task PeriodicCalibrationTaskAsync()
    {
        while (IsConnected && IsCalibrating)
        {
            _calibrationCancellationTokenSource.Token.ThrowIfCancellationRequested();

            var res = await Client.Instance.GetMaxWheelValue();

            if (res != -1)
                CurrentMax = res;

            await Task.Delay(150);
        }
    }

    public async Task StopCalibrationAsync()
    {
        if (!Client.IsConnected)
            return;

        if (!IsCalibrating)
            return;

        if (_calibrationCancellationTokenSource == null)
            return;

        await _calibrationCancellationTokenSource.CancelAsync();

        int res = -1;

        try
        {
            res = await Client.Instance.StopCalibration();
        }
        catch (Exception e)
        {
            HandleException(e);
        }

        IsCalibrating = false;
        CalibrationButtonText = "Calibrate";

        if (res == -1 || res == 0)
            return;

        // update the settings
        Dispatcher.UIThread.Post(() =>
        {
            var oldMax = Settings.MaxWheelValue;

            Settings.MaxWheelValue = res;
            CurrentMax = res;
            
            foreach (var display in Displays)
            {
                display.Max = res;

                if (display.End == oldMax)
                    display.End = res;
            }
        });
    }

    #endregion

    #region Binding display miscs

    private string? GetPluginNameFromIdentifier(int identifier)
    {
        if (Plugins == null)
            return null;

        return Plugins.FirstOrDefault(x => x.Identifier == identifier)?.PluginName ?? "Unknown";
    }

    public string GetFriendlyContentFromProperty(SerializablePluginSettings? property)
    {
        if (property == null || property.Identifier == 0)
            return "";

        var pluginName = GetPluginNameFromIdentifier(property.Identifier);

        return $"{pluginName} : {property.Value}";
    }

    #endregion

    #region Exception Handling

    private void HandleException(Exception e)
    {
        switch(e)
        {
            case StreamJsonRpc.RemoteRpcException re: 
                Console.WriteLine($"An Error occured while attempting to connect to the RPC server: {re.Message}");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("This error could have occured due to an different version of WheelAddon being used with this Interface.");

                ConnectionStateText = "Disconnected";
                break;
            case OperationCanceledException _:
                break;
            default:
                Console.WriteLine($"An unhanded exception occured: {e.Message}");

                // write the exception to a file
                File.WriteAllText("exception.txt", e.ToString());

                Console.WriteLine("The exception has been written to exception.txt");

                break;
        }
    }

    #endregion
}
