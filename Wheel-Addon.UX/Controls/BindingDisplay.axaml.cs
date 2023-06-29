using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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

        protected override void OnLoaded()
        {
            base.OnLoaded();

            var window = TopLevel.GetTopLevel(this) as MainWindow;

            if (window != null && this.DataContext is BindingDisplayViewModel vm)
            {
                vm.OnShowBindingEditorDialog += (sender, e) => window.ShowBindingEditorDialog(sender, e);
                vm.OnShowAdvancedBindingEditorDialog += (sender, e) => window.ShowAdvancedBindingEditorDialog(sender, e);
            }
        }
    }
}