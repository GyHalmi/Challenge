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
        Rectangle roboSource = (Rectangle)Application.Current.FindResource("roboCleaner");
        Border selector = null;
        const int borderThicknessOnOneAxis = 2;
        Rectangle roboCleanerOnMapGUI = null;
        int borderDistance = 4;

        Canvas actualMapGUI() { return borderMap.Child as Canvas; }

        public MainWindow()
        {
            InitializeComponent();
            btnStart.IsEnabled = false;
            addEventsToMap(MapGUI);
        }

        private void DrawSelector(Point mousePosition)
        {
            //must be an even number
            int squareSize = (int)roboSource.Width;

            int x = Math.Min((int)mousePosition.X / squareSize, (int)actualMapGUI().ActualWidth / squareSize - 1);
            int y = Math.Min((int)mousePosition.Y / squareSize, (int)actualMapGUI().ActualHeight / squareSize - 1);

            if (selector != null &&
                (Canvas.GetLeft(selector) + borderThicknessOnOneAxis / 2 != x || Canvas.GetTop(selector) + borderThicknessOnOneAxis / 2 != y))
            {
                actualMapGUI().Children.Remove(selector);
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
            actualMapGUI().Children.Add(selector);
        }
        private void DrawWall()
        {
            if (ReturnMapGuiShapeWithMouseOver() == null)
            {
                Rectangle newWall = new Rectangle
                {
                    Width = roboSource.Width,
                    Height = roboSource.Height,
                    Fill = Brushes.Black,
                    Tag = Application.Current.FindResource("strWall").ToString()
                };

                SetPosAccordingToSelector(newWall);
                actualMapGUI().Children.Add(newWall);
            }

        }


        private void RemoveWall()
        {
            if (ReturnMapGuiShapeWithMouseOver() is Rectangle rect)
            {
                if (rect == roboCleanerOnMapGUI) RoboRemovedFromGUI();
                if (rect != null) actualMapGUI().Children.Remove(rect);
            }
        }
        private void AddRobo()
        {
            if (roboCleanerOnMapGUI is null && ReturnMapGuiShapeWithMouseOver() == null)
            {
                roboCleanerOnMapGUI = roboSource;
                SetPosAccordingToSelector(roboCleanerOnMapGUI);
                MapGUI.Children.Add(roboCleanerOnMapGUI);
                rdbAddRc.IsEnabled = false;
                btnStart.IsEnabled = true;
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
                visualHit = VisualTreeHelper.HitTest(actualMapGUI(), Mouse.GetPosition(actualMapGUI())).VisualHit;
            }
            //catch if mouse point elsewhere than MapGUI
            catch (NullReferenceException)
            {
                ;
            }

            if (visualHit != actualMapGUI() && visualHit != selector) sh = (Shape)visualHit;

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
            DrawSelector(e.MouseDevice.GetPosition(actualMapGUI()));
            EditMap();
        }
        private void MapGUI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EditMap();
        }

        private void btnResetCanvas_Click(object sender, RoutedEventArgs e)
        {
            actualMapGUI().Children.Clear();
            RoboRemovedFromGUI();
        }

        private void RoboRemovedFromGUI()
        {
            roboCleanerOnMapGUI = null;
            rdbAddRc.IsEnabled = true;
            btnStart.IsEnabled = false;
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
            actualMapGUI().Children.Remove(selector);
            using (FileStream fs = File.Open("map.xaml", FileMode.Create))
            {
                XamlWriter.Save(actualMapGUI(), fs);
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

                addEventsToMap(savedCanvas);
                bool isRoboSaved = false;
                foreach (Rectangle rect in savedCanvas.Children)
                {
                    if (rect.Tag != null && rect.Tag.ToString() == roboSource.Tag.ToString())
                    {
                        isRoboSaved = true;
                        roboCleanerOnMapGUI = rect;
                        rdbAddRc.IsEnabled = false;
                        btnStart.IsEnabled = true;
                    }
                }
                if (!isRoboSaved)
                {
                    rdbAddRc.IsEnabled = true;
                    btnStart.IsEnabled = false;
                }
                borderMap.Child = savedCanvas;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("There is no stored map available");
            }
        }

        private void addEventsToMap(Canvas map)
        {
            map.MouseMove += MapGUI_MouseMove;
            map.MouseDown += MapGUI_MouseDown;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadMapGUI();
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


