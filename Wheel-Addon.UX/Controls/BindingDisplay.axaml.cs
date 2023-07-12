using System;
using Avalonia;
using Avalonia.Controls;
using OpenTabletDriver.Desktop.Reflection;
using WheelAddon.UX.ViewModels;
using WheelAddon.UX.Views;

namespace WheelAddon.UX.Controls
{
    public partial class BindingDisplay : UserControl
    {
        private string? _description;
        private PluginSettingStore? _store;

        // --------------------------------- Description --------------------------------- //

        public static readonly DirectProperty<BindingDisplay, string?> DesciptionProperty =
            AvaloniaProperty.RegisterDirect<BindingDisplay, string?>(
                nameof(Description),
                o => o.Description,
                (o, v) => o.Description = v);

        public string? Description
        {
            get => _description;
            set => this.SetAndRaise(DesciptionProperty, ref _description, value);
        }

        // --------------------------------- Store --------------------------------- //

        public static readonly DirectProperty<BindingDisplay, PluginSettingStore?> StoreProperty =
            AvaloniaProperty.RegisterDirect<BindingDisplay, PluginSettingStore?>(
                nameof(Store),
                o => o.Store,
                (o, v) => o.Store = v);

        public PluginSettingStore? Store
        {
            get => _store;
            set => this.SetAndRaise(StoreProperty, ref _store, value);
        }

        // --------------------------------- Constructor --------------------------------- //

        public BindingDisplay()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is BindingDisplayViewModel vm)
            {
                vm.OnShowBindingEditorDialog += ShowBindingEditorDialog;
                vm.OnShowAdvancedBindingEditorDialog += ShowAdvancedBindingEditorDialog;
            }
        }

        private void ShowBindingEditorDialog(object? sender, BindingDisplayViewModel e)
        {
            if (this.DataContext is BindingDisplayViewModel vm)
            {
                if (TopLevel.GetTopLevel(this) is MainWindow window)
                {
                    window.ShowBindingEditorDialog(sender, vm);
                }
            }
        }

        private void ShowAdvancedBindingEditorDialog(object? sender, BindingDisplayViewModel e)
        {
            if (this.DataContext is BindingDisplayViewModel vm)
            {
                if (TopLevel.GetTopLevel(this) is MainWindow window)
                {
                    window.ShowAdvancedBindingEditorDialog(sender, vm);
                }
            }
        }
    }
}