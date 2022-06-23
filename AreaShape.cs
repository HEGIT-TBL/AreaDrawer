using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AreaDrawer
{
    public class AreaShape : Shape
    {
        public ObservableCollection<Thumb> Thumbs
        {
            get => (ObservableCollection<Thumb>)GetValue(PointsProperty);
            set => SetValue(PointsProperty, value);
        }
        public static readonly DependencyProperty PointsProperty = DependencyProperty
            .Register("PointsProperty", typeof(ObservableCollection<Thumb>), typeof(AreaShape),
            new FrameworkPropertyMetadata(new ObservableCollection<Thumb>(),
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty
            .Register("IsSelectedProperty", typeof(bool), typeof(AreaShape), new PropertyMetadata(false));

        public PointCollection Points
        {
            get => _points; set
            {
                _points = value;
                Thumbs.Clear();
                AddThumbsFromPoints(_points, ThumbStyle);
            }
        }
        private PointCollection _points;

        public Style ThumbStyle
        {
            get => _thumbStyle; set
            {
                _thumbStyle = value ?? _defaultThumbStyle;
                foreach (var thumb in Thumbs)
                {
                    thumb.Style = value;
                }
            }
        }
        private Style _thumbStyle;
        private readonly Style _defaultThumbStyle;

        public AreaShape()
        {
            Thumbs = new ObservableCollection<Thumb>();
            Thumbs.CollectionChanged += Thumbs_CollectionChanged;
            Panel.SetZIndex(this, -100);

            _defaultThumbStyle = new Style(typeof(Thumb));
            _defaultThumbStyle.Setters.Add(new Setter(HeightProperty, 10.0));
            _defaultThumbStyle.Setters.Add(new Setter(WidthProperty, 10.0));
            _defaultThumbStyle.Setters.Add(new Setter(CursorProperty, Cursors.Hand));
            _defaultThumbStyle.Setters.Add(new Setter(FillProperty, Brushes.AliceBlue));
            ThumbStyle = _defaultThumbStyle;
        }

        public void AddThumbsFromPoints(IEnumerable<Point> points, Style style)
        {
            ThumbStyle = style;
            foreach (var point in points)
            {
                AddThumb(point, style);
            }
        }

        public void AddThumb(Point point, Style style)
        {
            var thumb = new Thumb
            {
                RenderTransform = new TranslateTransform(),
                Style = ThumbStyle,
            };

            thumb.DragDelta += Thumb_DragDelta;

            Canvas.SetLeft(thumb, point.X);
            Canvas.SetTop(thumb, point.Y);
            Thumbs.Add(thumb);
            Panel.SetZIndex(thumb, 100);
            thumb.InvalidateVisual();
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;
            var left = Canvas.GetLeft(thumb) + e.HorizontalChange;
            var top = Canvas.GetTop(thumb) + e.VerticalChange;
            Canvas.SetLeft(thumb, left);
            Canvas.SetTop(thumb, top);
            InvalidateMeasure();

            RenderSize = new Size(RenderSize.Width + 1, RenderSize.Height + 1);
        }

        public void AddToCanvasChildren(Canvas canvas)
        {
            //Thumbs first to set their Actual height/width
            //for the shape to follow on draw
            foreach (var thumb in Thumbs)
            {
                canvas.Children.Add(thumb);
            }
            canvas.Children.Add(this);
            InvalidateVisual();
            HideThumbs();
        }

        public void RemoveFromCanvasChildren(Canvas canvas)
        {
            foreach (var thumb in Thumbs)
            {
                canvas.Children.Remove(thumb);
            }
            canvas.Children.Remove(this);
        }

        public void HideThumbs()
        {
            foreach (var thumb in Thumbs)
            {
                thumb.Opacity = 0;
                thumb.IsEnabled = false;
            }
        }

        public void ShowThumbs()
        {
            foreach (var thumb in Thumbs)
            {
                thumb.Opacity = 1;
                thumb.IsEnabled = true;
            }
        }

        private void Thumbs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateMeasure();
        }

        private void GetGeometry(StreamGeometryContext context)
        {
            context.BeginFigure(PointFromThumb(Thumbs[0]), isFilled: true, isClosed: false);

            foreach (var thumb in Thumbs)
            {
                context.LineTo(PointFromThumb(thumb), isStroked: true, isSmoothJoin: true);
            }
            context.LineTo(PointFromThumb(Thumbs[0]), isStroked: true, isSmoothJoin: true);
        }

        private Point PointFromThumb(Thumb thumb)
        {
            return new Point(Canvas.GetLeft(thumb) + thumb.ActualWidth / 2,
                Canvas.GetTop(thumb) + thumb.ActualHeight / 2);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var minX = Thumbs.Min(thumb => Canvas.GetLeft(thumb));
            var minY = Thumbs.Min(thumb => Canvas.GetTop(thumb));
            var maxX = Thumbs.Max(thumb => Canvas.GetLeft(thumb));
            var maxY = Thumbs.Max(thumb => Canvas.GetTop(thumb));
            return new Size(maxX - minX, maxY - minY);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var geometry = new StreamGeometry { FillRule = FillRule.Nonzero };
                using (var context = geometry.Open())
                {
                    GetGeometry(context);
                }

                geometry.Freeze();
                return geometry;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }
    }
}
