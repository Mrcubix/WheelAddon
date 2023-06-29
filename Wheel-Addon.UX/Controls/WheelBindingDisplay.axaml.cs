using Avalonia;

namespace WheelAddon.UX.Controls
{
    public partial class WheelBindingDisplay : BindingDisplay
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
    }
}