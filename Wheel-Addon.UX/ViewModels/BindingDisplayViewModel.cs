using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using OpenTabletDriver.Desktop.Reflection;
using ReactiveUI;

namespace WheelAddon.UX.ViewModels;

public class BindingDisplayViewModel : ViewModelBase
{
    private string? _description;
    private PluginSettingStore? _store;

    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public PluginSettingStore? Store
    {
        get => _store;
        set => this.RaiseAndSetIfChanged(ref _store, value);
    }
    
    public event EventHandler<BindingDisplayViewModel>? OnShowBindingEditorDialog;
    public event EventHandler<BindingDisplayViewModel>? OnShowAdvancedBindingEditorDialog;

    public void OnShowBindingEditorDialogEvent()
    {
        OnShowBindingEditorDialog?.Invoke(this, this);
    }

    public void OnShowAdvancedBindingEditorDialogEvent()
    {
        OnShowAdvancedBindingEditorDialog?.Invoke(this, this);
    }
}
