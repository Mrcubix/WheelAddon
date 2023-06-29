using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using OpenTabletDriver.Desktop.Reflection;
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
            var dialog = new BindingEditorDialog
            {
                DataContext = e
            };

            var res = await dialog.ShowDialog<BindingEditorDialog>(this);

            // do stuff
        });
    }

    public void ShowAdvancedBindingEditorDialog(object? sender, BindingDisplayViewModel e)
    {
        // not implemented
        throw new NotImplementedException();
    }
}