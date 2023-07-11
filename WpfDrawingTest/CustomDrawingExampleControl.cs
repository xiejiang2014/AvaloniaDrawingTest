using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfDrawingTest
{
    internal class CustomDrawingExampleControl : Control
    {
        private static Random s_random = new();

        private Point _cursorPoint;

        private Pen _pen;

        private Geometry _smileGeometry;

        private System.Diagnostics.Stopwatch _timeKeeper = System.Diagnostics.Stopwatch.StartNew();

        public CustomDrawingExampleControl()
        {
            _pen = new Pen(new SolidColorBrush(Colors.Black), 1);

            StreamGeometry sg = new StreamGeometry();
            using (var cntx = sg.Open())
            {
                cntx.BeginFigure(new Point(-25.0d, -10.0d), false, false);
                cntx.ArcTo(new Point(25.0d, -10.0d),
                           new Size(10.0d, 10.0d),
                           0.0d,
                           false,
                           SweepDirection.Clockwise, true, true);
            }

            _smileGeometry = sg.Clone();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            InvalidateVisual();
        }

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(
                                        nameof(Scale),
                                        typeof(double),
                                        typeof(CustomDrawingExampleControl),
                                        new PropertyMetadata(1.0d, null)
                                       );


        public double Rotation
        {
            get => (double)GetValue(RotationProperty);
            set => SetValue(RotationProperty, value);
        }

        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register(
                                        nameof(Rotation),
                                        typeof(double),
                                        typeof(CustomDrawingExampleControl),
                                        new PropertyMetadata(default(double), null, (_, val) => (double)val % (Math.PI * 2))
                                       );


        public double ViewportCenterY
        {
            get => (double)GetValue(ViewportCenterYProperty);
            set => SetValue(ViewportCenterYProperty, value);
        }

        public static readonly DependencyProperty ViewportCenterYProperty =
            DependencyProperty.Register(
                                        nameof(ViewportCenterY),
                                        typeof(double),
                                        typeof(CustomDrawingExampleControl),
                                        new PropertyMetadata(default(double))
                                       );


        public double ViewportCenterX
        {
            get => (double)GetValue(ViewportCenterXProperty);
            set => SetValue(ViewportCenterXProperty, value);
        }

        public static readonly DependencyProperty ViewportCenterXProperty =
            DependencyProperty.Register(
                                        nameof(ViewportCenterX),
                                        typeof(double),
                                        typeof(CustomDrawingExampleControl),
                                        new PropertyMetadata(default(double))
                                       );


        protected override void OnRender(DrawingContext dc)
        {
            var localBounds = new Rect(new Size(ActualWidth, ActualHeight));
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));


            dc.DrawRectangle(Brushes.White, _pen, localBounds);

            var halfWidth  = ActualWidth  / 2.0d;
            var halfHeight = ActualHeight / 2.0d;

            // 0,0 refers to the top-left of the control now. It is not prime time to draw gui stuff because it'll be under the world 
            dc.PushTransform(new TranslateTransform(halfWidth, halfHeight));

            dc.PushTransform(new RotateTransform(Rotation));

            // everything is rotated but not scaled 

            dc.PushTransform(new ScaleTransform(Scale, -Scale));
            dc.PushTransform(new TranslateTransform(-ViewportCenterX, -ViewportCenterY)  );

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

            // this is prime time to draw gui stuff 
            dc.DrawLine(_pen, _cursorPoint + new Vector(-20, 0),   _cursorPoint + new Vector(20, 0));
            dc.DrawLine(_pen, _cursorPoint + new Vector(0,   -20), _cursorPoint + new Vector(0,  20));

            //// oh and draw again when you can, no rush, right? 
            //Dispatcher.Invoke(InvalidateVisual, DispatcherPriority.Background);
        }


        private Point UIPointToWorldPoint(Point inPoint, double viewportCenterX, double viewportCenterY, double scale, double rotation)
        {
            Point workingPoint = new Point(inPoint.X, -inPoint.Y);
            workingPoint += new Vector(-this.ActualWidth / 2.0d, this.ActualHeight / 2.0d);
            workingPoint =  new Point(workingPoint.X     / scale, workingPoint.Y              / scale) ;

            workingPoint = (new RotateTransform(rotation)).Transform(workingPoint);
            
            workingPoint += new Vector(viewportCenterX, viewportCenterY);

            return workingPoint;
        }
    }
}