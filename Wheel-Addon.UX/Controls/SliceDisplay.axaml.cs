using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using WheelAddon.UX.ViewModels;

namespace WheelAddon.UX.Controls
{
    public partial class SliceDisplay : Control
    {
        private IDisposable? _subscription;
        private IBrush _brush = null!;

        public SliceDisplay()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is SliceDisplayViewModel viewModel)
            {
                _subscription = viewModel.Changed
                    .Subscribe(_ => InvalidateVisual());

                _brush = Brush.Parse(viewModel.Color);
            }
            else
            {
                _subscription?.Dispose();
                _subscription = null;
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (DataContext is not SliceDisplayViewModel viewModel)
                return;

            var origin = viewModel.Origin;
            var radius = viewModel.Radius;

            var start = viewModel.Start;
            var end = viewModel.End;
            var max = viewModel.Max;

            // Convert start and end to degrees
            var startAngle = start / max * 360;
            var endAngle = end / max * 360;
            
            // Convert degrees to radians
            var startAngleRad = startAngle * Math.PI / 180;
            var endAngleRad = endAngle * Math.PI / 180;

            // Calculate start and end points
            var startPoint = new Point
            (
                origin.X + radius * Math.Cos(startAngleRad),
                origin.Y + radius * Math.Sin(startAngleRad)
            );

            var endPoint = new Point
            (
                origin.X + radius * Math.Cos(endAngleRad),
                origin.Y + radius * Math.Sin(endAngleRad)
            );

            var isLargeArc = endAngle - startAngle > 180;

            var stream = new StreamGeometry();
            
            using (var ctx = stream.Open())
            {
                ctx.BeginFigure(origin, true);
                ctx.LineTo(startPoint);
                ctx.ArcTo(endPoint, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise);
                ctx.LineTo(origin);
            }

            context.DrawGeometry(_brush, null, stream);
        }
    }
}