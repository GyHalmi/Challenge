using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double roboWidth = (double)Application.Current.FindResource("roboWidth");
        double roboHeight = (double)Application.Current.FindResource("roboHeight");
        Brush roboColour = (Brush)Application.Current.FindResource("roboColour");
        Rectangle roboCleanerOnMapGUI;

        Border selector = null;
        const int borderThicknessOnOneAxis = 2;
        int borderDistance = 4;

        public MainWindow()
        {
            InitializeComponent();
            InitializeRobocleanerOnMapGUI();


            Binding roboEnabled = new Binding("IsEnabled");
            roboEnabled.Source = roboCleanerOnMapGUI;
            btnStart.SetBinding(Button.IsEnabledProperty, roboEnabled);

            tbTest.SetBinding(TextBlock.TextProperty, roboEnabled);

            Binding roboEnabledReversed = new Binding("IsEnabled");
            roboEnabledReversed.Source = roboCleanerOnMapGUI;
            roboEnabledReversed.Converter = new Utilities.ReverseBool();
            rdbAddRc.SetBinding(Button.IsEnabledProperty, roboEnabledReversed);

        }

        private void InitializeRobocleanerOnMapGUI()
        {
            string roboTag = (string)Application.Current.FindResource("roboTag");
            roboCleanerOnMapGUI = new Rectangle() { Width = roboWidth, Tag = roboTag, Height = roboHeight, Fill = roboColour };
            DeactivateRC();
            MapGUI.Children.Add(roboCleanerOnMapGUI);
        }

        private void ActivateRC()
        {
            roboCleanerOnMapGUI.IsEnabled = true;
            roboCleanerOnMapGUI.Fill = roboColour;
        }
        private void DeactivateRC()
        {
            roboCleanerOnMapGUI.IsEnabled = false;
            roboCleanerOnMapGUI.Fill = Brushes.Transparent;
        }
        private void DrawSelector(Point mousePosition)
        {
            //must be an even number
            int squareSize = (int)roboWidth;

            int x = Math.Min((int)mousePosition.X / squareSize, (int)MapGUI.ActualWidth / squareSize - 1);
            int y = Math.Min((int)mousePosition.Y / squareSize, (int)MapGUI.ActualHeight / squareSize - 1);

            if (selector != null &&
                (Canvas.GetLeft(selector) + borderThicknessOnOneAxis / 2 != x || Canvas.GetTop(selector) + borderThicknessOnOneAxis / 2 != y))
            {
                MapGUI.Children.Remove(selector);
            }

            selector = new Border()
            {
                Width = squareSize + borderThicknessOnOneAxis + borderDistance,
                Height = squareSize + borderThicknessOnOneAxis + borderDistance,
                BorderBrush = Brushes.DodgerBlue,
                BorderThickness = new Thickness(borderThicknessOnOneAxis / 2)
            };

            Canvas.SetLeft(selector, squareSize * x - (borderThicknessOnOneAxis + borderDistance) / 2);
            Canvas.SetTop(selector, squareSize * y - (borderThicknessOnOneAxis + borderDistance) / 2);
            MapGUI.Children.Add(selector);
        }
        private void DrawWall()
        {
            if (ReturnMapGuiShapeWithMouseOver() == null)
            {
                Rectangle newWall = new Rectangle
                {
                    Width = roboWidth,
                    Height = roboHeight,
                    Fill = Brushes.Black,
                    Tag = Application.Current.FindResource("wallTag").ToString()
                };

                SetPosAccordingToSelector(newWall);
                MapGUI.Children.Add(newWall);
            }

        }
        private void RemoveWall()
        {
            if (ReturnMapGuiShapeWithMouseOver() is Rectangle rect)
            {
                if (rect == roboCleanerOnMapGUI) DeactivateRC();
                if (rect != null) MapGUI.Children.Remove(rect);
            }
        }
        private void AddRobo()
        {
            if (!roboCleanerOnMapGUI.IsEnabled && ReturnMapGuiShapeWithMouseOver() == null)
            {
                SetPosAccordingToSelector(roboCleanerOnMapGUI);
                if (MapGUI.Children.IndexOf(roboCleanerOnMapGUI) == -1) MapGUI.Children.Add(roboCleanerOnMapGUI);
                ActivateRC();
            }
        }
        /// <summary>
        /// returns the shape that has the mouse over it, or returns null
        /// </summary>
        /// <returns></returns>
        private Shape ReturnMapGuiShapeWithMouseOver()
        {
            Shape sh = null;
            DependencyObject visualHit = null;

            try
            {
                visualHit = VisualTreeHelper.HitTest(MapGUI, Mouse.GetPosition(MapGUI)).VisualHit;
            }
            //catch if mouse point elsewhere than MapGUI
            catch (NullReferenceException)
            {
                ;
            }

            if (visualHit != MapGUI && visualHit != selector) sh = (Shape)visualHit;

            return sh;
        }
        private void SetPosAccordingToSelector(UIElement uiE)
        {
            int y = (int)Canvas.GetTop(selector);
            int x = (int)Canvas.GetLeft(selector);

            Canvas.SetTop(uiE, y + (borderThicknessOnOneAxis + borderDistance) / 2);
            Canvas.SetLeft(uiE, x + (borderThicknessOnOneAxis + borderDistance) / 2);
        }
        private void EditMap()
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (rdbDraw.IsChecked == true) DrawWall();
                if (rdbRemove.IsChecked == true) RemoveWall();
                if (rdbAddRc.IsChecked == true) AddRobo();
            }
        }
        private void MapGUI_MouseMove(object sender, MouseEventArgs e)
        {
            DrawSelector(e.MouseDevice.GetPosition(MapGUI));
            EditMap();
        }
        private void MapGUI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EditMap();
        }

        private void btnResetCanvas_Click(object sender, RoutedEventArgs e)
        {
            resetMapGUI();
        }

        private void resetMapGUI()
        {
            UIElementCollection mapGuiShapes = MapGUI.Children;
            int roboIndex = mapGuiShapes.IndexOf(roboCleanerOnMapGUI);

            if (roboIndex == -1)
            {
                mapGuiShapes.Clear();
            }
            else
            {
                mapGuiShapes.RemoveRange(0, roboIndex);
                mapGuiShapes.RemoveRange(1, mapGuiShapes.Count - 1);
                DeactivateRC();
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //Task cleanTheHouse = new Task(() => {
            //    Canvas map = MapGUI;
            //    new RoboCleaner(roboCleanerOnMapGUI).CleanTheHouse();
            //});
            //cleanTheHouse.Start();

            //MapGUI.DataContextChanged += MapGUI_DataContextChanged;
            SaveMapGUI();
            new RoboCleaner(roboCleanerOnMapGUI).CleanTheHouse();
        }

        private void SaveMapGUI()
        {
            MapGUI.Children.Remove(selector);
            using (FileStream fs = File.Open("map.xaml", FileMode.Create))
            {
                XamlWriter.Save(MapGUI, fs);
            }
            //Debugger.Break();
        }
        private void LoadMapGUI()
        {
            try
            {
                Canvas savedCanvas;
                using (FileStream fs = File.Open("map.xaml", FileMode.Open, FileAccess.Read))
                {
                    savedCanvas = XamlReader.Load(fs) as Canvas;
                }

                resetMapGUI();
                foreach (Rectangle rect in savedCanvas.Children)
                {
                    if (rect.Tag != null && rect.Tag.ToString() == roboCleanerOnMapGUI.Tag.ToString())
                    {
                        Canvas.SetTop(roboCleanerOnMapGUI, Canvas.GetTop(rect));
                        Canvas.SetLeft(roboCleanerOnMapGUI, Canvas.GetLeft(rect));
                        ActivateRC();
                    }
                    else if(rect.Tag != null && rect.Tag.ToString() == "Wall")
                    {
                        Rectangle r = new Rectangle();
                        r.Width = rect.Width;
                        r.Height = rect.Height;
                        r.Fill = rect.Fill;
                        r.Tag = rect.Tag;
                        Canvas.SetTop(r, Canvas.GetTop(rect));
                        Canvas.SetLeft(r, Canvas.GetLeft(rect));

                        MapGUI.Children.Add(r);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("There is no stored map available");
            }
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadMapGUI();
        }
        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (roboCleanerOnMapGUI.IsEnabled)
            {
                DeactivateRC();
            }
            else
            {
                ActivateRC();
            }
        }
    }


    //private void CommDrawWall_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    //{

    //}

    //private void CommDrawWall_Executed(object sender, ExecutedRoutedEventArgs e)
    //{

    //}

    //public static class CustomCommands
    //{
    //    public static readonly RoutedUICommand DrawWall = new RoutedUICommand
    //    (
    //        "DrawWall","DrawWall", typeof(CustomCommands)
    //    );
    //}

}


