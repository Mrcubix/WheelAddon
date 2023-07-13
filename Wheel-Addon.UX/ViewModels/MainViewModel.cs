using System;
using System.Collections.ObjectModel;
using Avalonia.Media;
using WheelAddon.Lib.RPC;
using ReactiveUI;
using WheelAddon.Lib.Contracts;
using WheelAddon.Lib.Serializables;
using WheelAddon.UX.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using WheelAddon.Lib.Serializables.Bindings;
using WheelAddon.Lib.Serializables.Modes;
using Avalonia.Threading;
using Avalonia.Logging;
using System.Runtime.InteropServices.JavaScript;
using System.IO;

namespace WheelAddon.UX.ViewModels;

public class MainViewModel : ViewModelBase
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

    public IBrush SeparatorColor { get; } = Brush.Parse("#66FFFFFF");

    #region Simple Bindings

    public BindingDisplayViewModel ClockWiseBindingDisplay
    {
        get => _clockWiseBindingDisplay;
        set => this.RaiseAndSetIfChanged(ref _clockWiseBindingDisplay, value);
    }

    public BindingDisplayViewModel CounterClockWiseBindingDisplay
    {
        get => _counterClockWiseBindingDisplay;
        set => this.RaiseAndSetIfChanged(ref _counterClockWiseBindingDisplay, value);
    }

    #endregion

    #region Ranged Bindings

    public ObservableCollection<WheelBindingDisplayViewModel> Displays
    {
        get => _displays;
        set => this.RaiseAndSetIfChanged(ref _displays, value);
    }

    public ObservableCollection<SliceDisplayViewModel> Slices
    {
        get => _slices;
        set => this.RaiseAndSetIfChanged(ref _slices, value);
    }

    public int LastIndex 
    {
        get => _lastIndex;
        set => this.RaiseAndSetIfChanged(ref _lastIndex, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => this.RaiseAndSetIfChanged(ref _isEmpty, value);
    }


    #endregion

    #region RPC

    public RpcClient<IWheelDaemon> Client
    {
        get => _rpcClient;
        set => this.RaiseAndSetIfChanged(ref _rpcClient, value);
    }

    public string ConnectionStateText
    {
        get => _connectionStateText;
        set => this.RaiseAndSetIfChanged(ref _connectionStateText, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => this.RaiseAndSetIfChanged(ref _isConnected, value);
    }

    #endregion

    #region Settings

    public SerializableSettings Settings
    {
        get => _settings;
        set => this.RaiseAndSetIfChanged(ref _settings, value);
    }

    public ObservableCollection<SerializablePlugin> Plugins
    {
        get => _plugins;
        set => this.RaiseAndSetIfChanged(ref _plugins, value);
    }

    #endregion

    public MainViewModel() : this(SerializableSettings.Default)
    {
        /*// define events
        OnSliceAdded = null!;
        OnSliceRemoved = null!;

        // simple mode binding displays
        ClockWiseBindingDisplay = new BindingDisplayViewModel("Clockwise Rotation", null!);
        CounterClockWiseBindingDisplay = new BindingDisplayViewModel("Counter Clockwise Rotation", null!);

        // wheel binding displays

        _displays = new ObservableCollection<WheelBindingDisplayViewModel>();

        var color = ColorExtensions.RandomColor().ToHex();

        var defaultDisplay = new WheelBindingDisplayViewModel
        {
            Description = "Slice 1",
            PluginProperty = null!,
            SliceColor = color,
            Start = 0,
            End = 20,
            Max = 71
        };

        _displays.Add(defaultDisplay);

        // wheel slices

        _slices = new ObservableCollection<SliceDisplayViewModel>();

        var defaultSlice = new SliceDisplayViewModel(defaultDisplay)
        {
            Color = color
        };

        _slices.Add(defaultSlice);*/
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

    public void OnSliceAddedEvent()
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

    public void OnSliceRemovedEvent(int index)
    {
        if (index < 1 || index >= _displays.Count)
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
