using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using WheelAddon.Lib.Serializables;
using WheelAddon.UX.ViewModels;
using System.Linq;

namespace WheelAddon.UX.Dialogs
{
    public partial class AdvancedBindingEditorDialog : Window
    {
        protected ObservableCollection<SerializablePlugin> _plugins = null!;

        public AdvancedBindingEditorDialog()
        {
            InitializeComponent();
        }

        public ObservableCollection<SerializablePlugin> Plugins
        {
            get => _plugins;
            set => _plugins = value;
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is AdvancedBindingEditorDialogViewModel vm)
            {
                vm.CloseRequested += (s, e) => Close(new SerializablePluginSettings()
                {
                    Identifier = -1,
                    Value = "None"
                });

                vm.ApplyRequested += (s, e) => Close(new SerializablePluginSettings()
                {
                    Identifier = Plugins.FirstOrDefault(p => p.PluginName == vm.SelectedType)?.Identifier ?? -1,
                    Value = vm.SelectedProperty
                });

                TypesComboBox.SelectionChanged += (s, e) =>
                {
                    if (Plugins == null)
                        return;

                    var plugin = Plugins.FirstOrDefault(p => p.PluginName == (string?)(TypesComboBox.SelectedItem));

                    if (plugin != null)
                    {
                        PropertiesComboBox.ItemsSource = plugin.ValidProperties;
                    }
                };
            }
        }
    }
}