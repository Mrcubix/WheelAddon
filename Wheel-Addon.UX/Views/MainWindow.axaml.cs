using System;
using Avalonia.Controls;
using Avalonia.Threading;
using WheelAddon.Lib.Serializables;
using WheelAddon.UX.Dialogs;
using WheelAddon.UX.ViewModels;

namespace WheelAddon.UX.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void ShowBindingEditorDialog(object? sender, BindingDisplayViewModel e)
    {
        // instantiate and show dialog on UI thread
        Dispatcher.UIThread.Post(async () =>
        {
            if (DataContext is MainViewModel vm)
            {
                var dialog = new BindingEditorDialog()
                {
                    DataContext = new BindingEditorDialogViewModel(),
                    Plugins = vm.Plugins
                };

                var res = await dialog.ShowDialog<SerializablePluginProperty>(this);

                if (res == null)
                    return;

                e.PluginProperty = res;
                e.Content = vm.GetFriendlyContentFromProperty(res);
            }
        });
    }

    public void ShowAdvancedBindingEditorDialog(object? sender, BindingDisplayViewModel e)
    {
        // not implemented
        throw new NotImplementedException();
    }
}