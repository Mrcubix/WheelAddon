using System;
using Avalonia;

namespace WheelAddon.UX.ViewModels
{
    public partial class SliceDisplayViewModel : ViewModelBase
    {
        private readonly WheelBindingDisplayViewModel _wheelBindingDisplayVM;

        private Point _origin = new(100, 100);
        private int _radius = 100;

        private string _color = "#FF0000";

        private string _data = "";

        public WheelBindingDisplayViewModel WheelBindingDisplayVM => _wheelBindingDisplayVM;

        public Point Origin
        {
            get => _origin;
            set => SetProperty(ref _origin, value);
        }

        public int Radius
        {
            get => _radius;
            set => SetProperty(ref _radius, value);
        }

        /// <summary>
        ///     Start value of slice, not in degrees nor radians
        /// </summary>
        public double Start => _wheelBindingDisplayVM.Start;

        /// <summary>
        ///     End value of slice, not in degrees nor radians
        /// </summary>
        public double End => _wheelBindingDisplayVM.End;

        /// <summary>
        ///     Max possible value of the wheel, not in degrees nor radians
        /// </summary>
        public double Max => _wheelBindingDisplayVM.Max;

        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        public string Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        public SliceDisplayViewModel(WheelBindingDisplayViewModel vm)
        {
            _wheelBindingDisplayVM = vm;

            // expect an observer as a parameter
            vm.PropertyChanged += OnWheelBindingDisplayChanged;
            
            UpdateData();
        }

        private void OnWheelBindingDisplayChanged(object? sender, EventArgs e) => UpdateData();

        private void UpdateData()
        {
            // calculate the point on the perimeter for the start and end values
            
            // convert start and end to degrees using the max value
            var startAngleDegrees = (Start / Max) * 360;
            var endAngleDegrees = (End / Max) * 360;

            var startAngle = ToRadians(startAngleDegrees);
            var endAngle = ToRadians(endAngleDegrees);

            // calculate the point on the perimeter for the start value
            var startPoint = new Point
            (
                Origin.X + Radius * (float) Math.Cos(startAngle),
                Origin.Y + Radius * (float) Math.Sin(startAngle)
            );

            // calculate the point on the perimeter for the end value
            var endPoint = new Point
            (
                Origin.X + Radius * (float) Math.Cos(endAngle),
                Origin.Y + Radius * (float) Math.Sin(endAngle)
            );

            // formula to draw a circle at 100 100 of radius 100 in desmos: (x-100)^2 + (y-100)^2 = 100^2 {x>0, y>0}

            var isLargeArc = endAngleDegrees - startAngleDegrees > 180 ? 1 : 0;

            Data = $"M {Origin.X},{Origin.Y} L {startPoint.X},{startPoint.Y} A {Radius},{Radius} 0 {isLargeArc} 1 {endPoint.X},{endPoint.Y} Z";

            Console.WriteLine(Data);
        }

        private static double ToRadians(double angle)
        {
            return (double) (Math.PI / 180) * angle;
        }

    }
}