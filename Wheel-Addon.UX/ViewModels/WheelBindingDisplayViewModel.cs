using System.Drawing;
using OpenTabletDriver.Desktop.Reflection;
using ReactiveUI;
using WheelAddon.Lib.Serializables;
using WheelAddon.Lib.Serializables.Bindings;
using WheelAddon.UX.Extensions;

namespace WheelAddon.UX.ViewModels;

public class WheelBindingDisplayViewModel : BindingDisplayViewModel
{
    private string _sliceColor = Color.Black.ToHex();
    private int _start = 0;
    private int _end = 0;
    private int _max = 0;

    public WheelBindingDisplayViewModel()
    {
        Description = "PlaceHolder";
        PluginProperty = null;
    }

    public WheelBindingDisplayViewModel(SerializablePluginProperty? pluginProperty)
    {
        PluginProperty = pluginProperty;
    }

    public WheelBindingDisplayViewModel(string description, SerializablePluginProperty? pluginProperty)
    {
        Description = description;
        PluginProperty = pluginProperty;
    }

    public WheelBindingDisplayViewModel(string description, SerializablePluginProperty? pluginProperty, string color, int start, int end, int max)
    {
        Description = description;
        PluginProperty = pluginProperty;
        SliceColor = color;
        Start = start;
        End = end;
        Max = max;
    }

    /// <summary>
    ///     Color of the slice
    /// </summary>
    public string SliceColor
    {
        get => _sliceColor;
        set => this.RaiseAndSetIfChanged(ref _sliceColor, value);
    }

    /// <summary>
    ///     Start value of slice, not in degrees nor radians
    /// </summary>
    public int Start
    {
        get => _start;
        set => this.RaiseAndSetIfChanged(ref _start, value);
    }

    /// <summary>
    ///     End value of slice, not in degrees nor radians
    /// </summary>
    public int End
    {
        get => _end;
        set => this.RaiseAndSetIfChanged(ref _end, value);
    }

    /// <summary>
    ///     Max possible value of the wheel, not in degrees nor radians
    /// </summary>
    public int Max
    {
        get => _max;
        set => this.RaiseAndSetIfChanged(ref _max, value);
    }

    public SerializableRangedWheelBinding ToAdvancedWheelBinding()
    {
        return new SerializableRangedWheelBinding
        {
            PluginProperty = PluginProperty,
            Start = Start,
            End = End,
        };
    }

    public static WheelBindingDisplayViewModel Default => new("Slice 1", null, ColorExtensions.RandomColor().ToHex(), 0, 0, 71);
}
