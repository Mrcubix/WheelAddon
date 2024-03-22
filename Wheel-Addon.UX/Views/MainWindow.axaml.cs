using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        _ = Dispatcher.UIThread.InvokeAsync(() => ShowBindingEditorDialogCore(e));
    }

    private async Task ShowBindingEditorDialogCore(BindingDisplayViewModel e)
    {
        if (DataContext is MainViewModel vm)
        {
            var dialog = new BindingEditorDialog()
            {
                DataContext = new BindingEditorDialogViewModel(),
                Plugins = vm.Plugins
            };

            var res = await dialog.ShowDialog<SerializablePluginSettings>(this);

            if (res == null)
                return;

            if (res.Identifier == -1 || res.Value == "None")
            {
                e.PluginProperty = null;
                e.Content = "";
            }
            else
            {
                e.PluginProperty = res;
                e.Content = vm.GetFriendlyContentFromProperty(res);
            }
        }
    }

    public void ShowAdvancedBindingEditorDialog(object? sender, BindingDisplayViewModel e)
    {
        // instantiate and show dialog on UI thread
        _ = Dispatcher.UIThread.InvokeAsync(() => ShowAdvancedBindingEditorDialogCore(e));
    }

    private async Task ShowAdvancedBindingEditorDialogCore(BindingDisplayViewModel e)
    {
        if (DataContext is MainViewModel vm)
        {
            var types = vm.Plugins.Select(p => p.PluginName ?? p.FullName ?? "Unknown").ToList();

            var currentPlugin = vm.Plugins.FirstOrDefault(p => p.Identifier == e.PluginProperty?.Identifier);
            var selectedType = currentPlugin?.PluginName ?? currentPlugin?.FullName ?? "Unknown";

            var validProperties = currentPlugin?.ValidProperties ?? new string[0];
            var selectedProperty = e.PluginProperty?.Value ?? "";

            var dialogVM = new AdvancedBindingEditorDialogViewModel()
            {
                Types = new ObservableCollection<string>(types),
                SelectedType = selectedType,
                ValidProperties = new ObservableCollection<string>(validProperties),
                SelectedProperty = selectedProperty
            };

            var dialog = new AdvancedBindingEditorDialog()
            {
                DataContext = dialogVM,
                Plugins = vm.Plugins
            };

            var res = await dialog.ShowDialog<SerializablePluginSettings>(this);

            if (res == null)
                return;

            if (res.Identifier == -1 || res.Value == "None")
            {
                e.PluginProperty = null;
                e.Content = "";
            }
            else
            {
                e.PluginProperty = res;
                e.Content = vm.GetFriendlyContentFromProperty(res);
            }
        }
    }
}