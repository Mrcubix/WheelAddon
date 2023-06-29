using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using OpenTabletDriver.Desktop.Reflection;
using ReactiveUI;

namespace WheelAddon.UX.ViewModels;

public class WheelBindingDisplayViewModel : BindingDisplayViewModel
{
    private string? _description;
    private PluginSettingStore? _store;
    private int _start = 0;
    private int _end = 0;

    public int Start
    {
        get => _start;
        set => this.RaiseAndSetIfChanged(ref _start, value);
    }

    public int End
    {
        get => _end;
        set => this.RaiseAndSetIfChanged(ref _end, value);
    }
}
