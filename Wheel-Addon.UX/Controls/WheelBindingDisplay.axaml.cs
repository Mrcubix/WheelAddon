using System;
using Avalonia;
using Avalonia.Controls;
using OpenTabletDriver.External.Avalonia.ViewModels;
using OpenTabletDriver.External.Avalonia.Views;

namespace WheelAddon.UX.Controls
{
    public partial class WheelBindingDisplay : UserControl
    {
        private int _start = 0;
        private int _end = 0;

        // --------------------------------- Start --------------------------------- //

        public static readonly DirectProperty<WheelBindingDisplay, int> StartProperty =
            AvaloniaProperty.RegisterDirect<WheelBindingDisplay, int>(
                nameof(Start),
                o => o.Start,
                (o, v) => o.Start = v);

        public int Start
        {
            get => _start;
            set => this.SetAndRaise(StartProperty, ref _start, value);
        }

        // --------------------------------- End --------------------------------- //

        public static readonly DirectProperty<WheelBindingDisplay, int> EndProperty =
            AvaloniaProperty.RegisterDirect<WheelBindingDisplay, int>(
                nameof(End),
                o => o.End,
                (o, v) => o.End = v);

        public int End
        {
            get => _end;
            set => this.SetAndRaise(EndProperty, ref _end, value);
        }

        // --------------------------------- Constructor --------------------------------- //

        public WheelBindingDisplay()
        {
            InitializeComponent();
        }

        // --------------------------------- Methods --------------------------------- //

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is BindingDisplayViewModel vm)
            {
                vm.ShowBindingEditorDialogRequested += ShowBindingEditorDialog;
                vm.ShowAdvancedBindingEditorDialogRequested += ShowAdvancedBindingEditorDialog;
            }
        }

        private void ShowBindingEditorDialog(object? sender, BindingDisplayViewModel e)
        {
            if (DataContext is BindingDisplayViewModel vm)
                if (TopLevel.GetTopLevel(this) is AppMainWindow window)
                    window.ShowBindingEditorDialog(sender, vm);
        }

        private void ShowAdvancedBindingEditorDialog(object? sender, BindingDisplayViewModel e)
        {
            if (DataContext is BindingDisplayViewModel vm)
                if (TopLevel.GetTopLevel(this) is AppMainWindow window)
                    window.ShowAdvancedBindingEditorDialog(sender, vm);
        }
    }
}