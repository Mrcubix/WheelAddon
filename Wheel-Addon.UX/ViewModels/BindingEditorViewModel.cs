using System;
using ReactiveUI;
using WheelAddon.Lib.Serializables;

namespace WheelAddon.UX.ViewModels
{
    public class BindingEditorDialogViewModel : ViewModelBase
    {
        private SerializablePluginProperty? _property = null!; 

        public event EventHandler CloseRequested = null!;

        public SerializablePluginProperty? Property
        {
            get => _property;
            set => this.RaiseAndSetIfChanged(ref _property, value);
        }

        public void Clear()
        {
            _property = null!;

            CloseRequested?.Invoke(this, null!);
        }
    }
}