using System;
using ReactiveUI;
using WheelAddon.Lib.Serializables;
using WheelAddon.Lib.Serializables.Bindings;

namespace WheelAddon.UX.ViewModels;

public class BindingDisplayViewModel : ViewModelBase
{
    private string? _description;
    private string? _content;
    private SerializablePluginSettings? _pluginProperty;

    public BindingDisplayViewModel()
    {
        Description = "PlaceHolder";
        Content = "";
        PluginProperty = null;
    }

    public BindingDisplayViewModel(SerializablePluginSettings? pluginProperty)
    {
        PluginProperty = pluginProperty;
    }

    public BindingDisplayViewModel(string description, string content, SerializablePluginSettings? pluginProperty)
    {
        Description = description;
        Content = content;
        PluginProperty = pluginProperty;
    }

    public string? Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public string? Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    public SerializablePluginSettings? PluginProperty
    {
        get => _pluginProperty;
        set => this.RaiseAndSetIfChanged(ref _pluginProperty, value);
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

    public SerializableWheelBinding ToSimpleWheelBinding()
    {
        return new SerializableWheelBinding
        {
            PluginProperty = PluginProperty
        };
    }
}
