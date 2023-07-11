using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace AvaloniaDrawingTest
{
    internal class CustomDrawingExampleControl : Control
    {
        private static Random s_random = new();

        private        Point  _cursorPoint;

        private IPen _pen;

        private Geometry _smileGeometry;

        private System.Diagnostics.Stopwatch _timeKeeper = System.Diagnostics.Stopwatch.StartNew();

        public CustomDrawingExampleControl()
        {
            _pen = new Pen(new SolidColorBrush(Colors.Black), lineCap: PenLineCap.Round);
            StreamGeometry sg = new StreamGeometry();
            using (var cntx = sg.Open())
            {
                cntx.BeginFigure(new Point(-25.0d, -10.0d), false);
                cntx.ArcTo(new Point(25.0d,        -10.0d), new Size(10.0d, 10.0d), 0.0d, false, SweepDirection.Clockwise);
                cntx.EndFigure(true);
            }
            _smileGeometry = sg.Clone();
        }


        public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<CustomDrawingExampleControl, double>(nameof(Scale), 1.0d);

        public double Scale
        {
            get => GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly StyledProperty<double> RotationProperty = AvaloniaProperty.Register<CustomDrawingExampleControl, double>(nameof(Rotation),
         coerce: (_, val) => val % (Math.PI * 2));

        /// <summary>
        /// Rotation, measured in Radians!
        /// </summary>
        public double Rotation
        {
            get => GetValue(RotationProperty);
            set => SetValue(RotationProperty, value);
        }

        public static readonly StyledProperty<double> ViewportCenterYProperty = AvaloniaProperty.Register<CustomDrawingExampleControl, double>(nameof(ViewportCenterY), 0.0d);

        public double ViewportCenterY
        {
            get => GetValue(ViewportCenterYProperty);
            set => SetValue(ViewportCenterYProperty, value);
        }

        public static readonly StyledProperty<double> ViewportCenterXProperty = AvaloniaProperty.Register<CustomDrawingExampleControl, double>(nameof(ViewportCenterX), 0.0d);

        public double ViewportCenterX
        {
            get => GetValue(ViewportCenterXProperty);
            set => SetValue(ViewportCenterXProperty, value);
        }

        public override void Render(DrawingContext dc)
        {
            var localBounds = new Rect(new Size(this.Bounds.Width, this.Bounds.Height));
            var clip        = dc.PushClip(this.Bounds);
            dc.DrawRectangle(Brushes.White, _pen, localBounds, 1.0d);

            //var halfMax = Math.Max(this.Bounds.Width / 2.0d, this.Bounds.Height / 2.0d) * Math.Sqrt(2.0d);
            //var halfMin = Math.Min(this.Bounds.Width / 2.0d, this.Bounds.Height / 2.0d) / 1.3d;
            var halfWidth  = this.Bounds.Width  / 2.0d;
            var halfHeight = this.Bounds.Height / 2.0d;

            // 0,0 refers to the top-left of the control now. It is not prime time to draw gui stuff because it'll be under the world 

            var translateModifier = dc.PushTransform(Avalonia.Matrix.CreateTranslation(new Avalonia.Vector(halfWidth, halfHeight)));

            // now 0,0 refers to the ViewportCenter(X,Y). 
            var rotationMatrix   = Avalonia.Matrix.CreateRotation(Rotation);
            var rotationModifier = dc.PushTransform(rotationMatrix);

            // everything is rotated but not scaled 

            var scaleModifier = dc.PushTransform(Avalonia.Matrix.CreateScale(Scale, -Scale));

            var mapPositionModifier = dc.PushTransform(Matrix.CreateTranslation(new Vector(-ViewportCenterX, -ViewportCenterY)));

            // now everything is rotated and scaled, and at the right position, now we're drawing strictly in world coordinates

            dc.DrawEllipse(Brushes.White, _pen, new Point(0.0d, 0.0d), 50.0d, 50.0d);
            dc.DrawLine(_pen, new Point(-25.0d,                 -5.0d), new Point(-25.0d, 15.0d));
            dc.DrawLine(_pen, new Point(25.0d,                  -5.0d), new Point(25.0d,  15.0d));
            dc.DrawGeometry(null, _pen, _smileGeometry);

            Point cursorInWorldPoint = UIPointToWorldPoint(_cursorPoint, ViewportCenterX, ViewportCenterY, Scale, Rotation);
            dc.DrawEllipse(Brushes.Gray, _pen, cursorInWorldPoint, 20.0d, 20.0d);


            for (int i = 0; i < 10000; i++)
            {
                double orbitRadius = i * 100 + 200;
                var    orbitInput  = ((_timeKeeper.Elapsed.TotalMilliseconds + 987654d) / orbitRadius) / 10.0d;
                if (i % 3 == 0)
                    orbitInput *= -1;
                Point orbitPosition = new Point(Math.Sin(orbitInput) * s_random.NextDouble() * 300, Math.Cos(orbitInput) * s_random.NextDouble() * 300);
                dc.DrawEllipse(Brushes.Gray, _pen, orbitPosition, 20.0d, 20.0d);
            }


            // end drawing the world 

            mapPositionModifier.Dispose();

            scaleModifier.Dispose();

            rotationModifier.Dispose();
            translateModifier.Dispose();

            // this is prime time to draw gui stuff 

            dc.DrawLine(_pen, _cursorPoint + new Vector(-20, 0),   _cursorPoint + new Vector(20, 0));
            dc.DrawLine(_pen, _cursorPoint + new Vector(0,   -20), _cursorPoint + new Vector(0,  20));

            clip.Dispose();

            // oh and draw again when you can, no rush, right? 
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        }


        private Point UIPointToWorldPoint(Point inPoint, double viewportCenterX, double viewportCenterY, double scale, double rotation)
        {
            Point workingPoint = new Point(inPoint.X, -inPoint.Y);
            workingPoint += new Vector(-this.Bounds.Width / 2.0d, this.Bounds.Height / 2.0d);
            workingPoint /= scale;

            workingPoint = Matrix.CreateRotation(rotation).Transform(workingPoint);

            workingPoint += new Vector(viewportCenterX, viewportCenterY);

            return workingPoint;
        }
    }
}