using System.Drawing;
using ReactiveUI;

namespace WheelAddon.UX.ViewModels;

public class WheelBindingDisplayViewModel : BindingDisplayViewModel
{
    private int _start = 0;
    private int _end = 0;
    private int _max = 0;

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
}
