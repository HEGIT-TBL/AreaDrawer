using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AreaDrawer
{
    public partial class AreaDrawer : UserControl
    {
        public CanvasState State { get; set; }
        public double PreviewLineStrokeThickness { get; set; }
        public SolidColorBrush PreviewLineBrush { get; set; }
        public Style AreaSelectedStyle { get; set; }
        public Style AreaUnselectedStyle { get; set; }
        public Style ThumbStyle { get; set; }
        public Style ThumbUnselectedStyle { get; set; }     //unused

        public event Action<AreaShape> OnDeleteAreaShapeCommandExecuted;
        public event Action<AreaShape> OnFinishDrawingShapeCommandExecuted;
        public event Action<AreaShape> OnSelectionChanged;

        public ICommand AddAreaShapeCommand => _addAreaShapeCommand ??= new RelayCommand(() =>
        {
            AreasCanvas.Cursor = Cursors.Cross;
            State = CanvasState.Draw;
        });

        public ICommand FinishDrawingShapeCommand => _finishDrawingShapeCommand ??= new RelayCommand(() =>
        {
            var finishedAreaShape = FinishDrawing();
            OnFinishDrawingShapeCommandExecuted?.Invoke(finishedAreaShape);
        });

        public ICommand DeleteAreaShapeCommand => _deleteAreaShapeCommand ??= new RelayCommand(() =>
        {
            if (SelectedAreaShape == null)
            {
                MessageBox.Show("Выберете область!", "Error", MessageBoxButton.OK);
                return;
            }
            SelectedAreaShape.RemoveFromCanvasChildren(AreasCanvas);
            AreaShapeCollection.Remove(SelectedAreaShape);
            OnDeleteAreaShapeCommandExecuted?.Invoke(SelectedAreaShape);
            SelectedAreaShape = null;
        });

        public AreaShape SelectedAreaShape
        {
            get => (AreaShape)GetValue(SelectedAreaShapeProperty);
            set
            {
                if (value != null)
                {
                    value.IsSelected = true;
                }
                SetValue(SelectedAreaShapeProperty, value);
                OnSelectionChanged?.Invoke(value);
            }
        }
        public static readonly DependencyProperty SelectedAreaShapeProperty =
            DependencyProperty.Register("SelectedAreaShapeProperty", typeof(AreaShape), typeof(AreaDrawer), new PropertyMetadata(null));

        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("StretchProperty", typeof(Stretch), typeof(AreaDrawer), new PropertyMetadata(Stretch.None));

        public ObservableCollection<AreaShape> AreaShapeCollection
        {
            get => (ObservableCollection<AreaShape>)GetValue(AreaShapeCollectionProperty);
            set => SetValue(AreaShapeCollectionProperty, value);
        }
        public static readonly DependencyProperty AreaShapeCollectionProperty = DependencyProperty
            .Register("AreaShapeCollectionProperty", typeof(ObservableCollection<AreaShape>), typeof(AreaDrawer),
            new FrameworkPropertyMetadata(new ObservableCollection<AreaShape>(),
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        private ICommand _addAreaShapeCommand;
        private ICommand _finishDrawingShapeCommand;
        private ICommand _deleteAreaShapeCommand;
        private Point _previousMousePosition;
        private Polyline _areaPreview;
        private readonly Polyline _nextLinePreview;

        public AreaDrawer()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += AreaDrawer_Loaded;
            Unloaded += AreaDrawer_Unloaded;

            State = CanvasState.Transform;
            _previousMousePosition = new Point(0, 0);

            ThumbStyle = Resources["ThumbStyleSelected"] as Style;
            ThumbUnselectedStyle = Resources["ThumbStyleUnselected"] as Style;
            AreaSelectedStyle = Resources["AreaShapeStyleSelected"] as Style;
            AreaUnselectedStyle = Resources["AreaShapeStyleUnselected"] as Style;
            PreviewLineBrush = Resources["StrokeBrushUnselected"] as SolidColorBrush;
            PreviewLineStrokeThickness = (double)Resources["StrokeThickness"];
            _nextLinePreview = new Polyline()
            {
                Stroke = Resources["StrokeBrushSelected"] as SolidColorBrush,
                StrokeThickness = PreviewLineStrokeThickness,
            };
        }

        public void UnselectAllThenSelectClicked(object sender, MouseButtonEventArgs e)
        {
            var areaShape = sender as AreaShape;
            foreach (var shape in AreaShapeCollection)
            {
                SetUnselectedStyle(shape);
            }
            SelectedAreaShape = AreaShapeCollection
                .Where(aShape => aShape == areaShape)
                .Single();
            SetSelectedStyle(SelectedAreaShape);
        }

        public void SetSelectedStyle(AreaShape areaShape)
        {
            if (areaShape == null)
            {
                return;
            }
            Canvas.SetZIndex(areaShape, 99);
            areaShape.Style = AreaSelectedStyle;
            areaShape.ShowThumbs();
            SelectedAreaShape = areaShape;
        }

        public void SetUnselectedStyle(AreaShape areaShape)
        {
            if (areaShape == null)
            {
                return;
            }
            Canvas.SetZIndex(areaShape, -100);

            areaShape.IsSelected = false;
            areaShape.Style = AreaUnselectedStyle;
            areaShape.HideThumbs();

            if (AreaShapeCollection.All(areaS => !areaS.IsSelected))
            {
                SelectedAreaShape = null;
            }
        }

        private void AreasCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (State == CanvasState.Draw)
            {
                var point = e.GetPosition(AreasCanvas);
                if (_areaPreview == null)
                {
                    _areaPreview = new Polyline
                    {
                        Stroke = PreviewLineBrush,
                        StrokeThickness = PreviewLineStrokeThickness,
                    };
                    AreasCanvas.Children.Add(_areaPreview);
                    AreasCanvas.Children.Add(_nextLinePreview);
                }
                else
                {
                    _nextLinePreview.Points.Clear();
                }

                _areaPreview.Points.Add(point);
                _nextLinePreview.Points.Add(point);
                _nextLinePreview.Points.Add(point);
            }
            else
            {
                if (!(e.OriginalSource is AreaShape))
                {
                    foreach (var shape in AreaShapeCollection)
                    {
                        SetUnselectedStyle(shape);
                    }
                }
            }
        }

        private void AreasCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (State == CanvasState.Draw && _areaPreview != null)
            {
                _nextLinePreview.Points[1] = e.GetPosition(AreasCanvas);
            }
        }

        private void AreaShape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var areaShape = sender as AreaShape;
            areaShape.CaptureMouse();
            var shapeTransform = areaShape.RenderTransform as TranslateTransform;
            _previousMousePosition.X = e.GetPosition(AreasCanvas).X - shapeTransform.X;
            _previousMousePosition.Y = e.GetPosition(AreasCanvas).Y - shapeTransform.Y;

            UnselectAllThenSelectClicked(sender, e);
        }

        private void AreaShape_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is AreaShape areaShape && areaShape.IsMouseCaptured)
            {
                var mousePosition = e.GetPosition(AreasCanvas);
                var deltaX = mousePosition.X - _previousMousePosition.X;
                var deltaY = mousePosition.Y - _previousMousePosition.Y;

                foreach (var thumb in areaShape.Thumbs)
                {
                    var thumbTransform = thumb.RenderTransform as TranslateTransform;
                    thumbTransform.X = deltaX;
                    thumbTransform.Y = deltaY;
                }

                var shapeTransform = areaShape.RenderTransform as TranslateTransform;
                shapeTransform.X = deltaX;
                shapeTransform.Y = deltaY;
            }
        }

        private void AreaDrawer_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var shape in AreaShapeCollection)
            {
                shape.AddToCanvasChildren(AreasCanvas);
            }
        }

        private void AreaDrawer_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (var shape in AreaShapeCollection)
            {
                SetUnselectedStyle(shape);
                shape.RemoveFromCanvasChildren(AreasCanvas);
            }
        }

        private AreaShape FinishDrawing()
        {
            if (State != CanvasState.Draw || _areaPreview == null)
            {
                return null;
            }

            var areaShape = new AreaShape()
            {
                RenderTransform = new TranslateTransform(),
                Style = AreaUnselectedStyle,
                ThumbStyle = ThumbStyle,
                Points = _areaPreview.Points,
            };

            //transformation/selection eventhandlers
            areaShape.MouseLeftButtonUp += (sender, e) => (sender as Shape).ReleaseMouseCapture();
            areaShape.MouseLeftButtonDown += AreaShape_MouseLeftButtonDown;
            areaShape.MouseRightButtonDown += UnselectAllThenSelectClicked;
            areaShape.MouseMove += AreaShape_MouseMove;

            AreasCanvas.Children.Remove(_areaPreview);
            AreasCanvas.Children.Remove(_nextLinePreview);
            areaShape.AddToCanvasChildren(AreasCanvas);

            AreaShapeCollection.Add(areaShape);

            _nextLinePreview.Points.Clear();
            _areaPreview = null;
            State = CanvasState.Transform;
            AreasCanvas.Cursor = Cursors.Arrow;
            return areaShape;
        }
    }
}
