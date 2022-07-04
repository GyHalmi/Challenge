using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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

        public MainWindow()
        {
            InitializeComponent();
        }


        private void DrawSelector(Point mousePosition)
        {
            //must be an even number
            int squareSize = (int)roboSource.Width;

            int x = Math.Min((int)mousePosition.X / squareSize, (int)MapGUI.ActualWidth / squareSize - 1);
            int y = Math.Min((int)mousePosition.Y / squareSize, (int)MapGUI.ActualHeight / squareSize - 1);

            if (selector != null &&
                (Canvas.GetLeft(selector) + borderThicknessOnOneAxis / 2 != x || Canvas.GetTop(selector) + borderThicknessOnOneAxis / 2 != y))
            {
                MapGUI.Children.Remove(selector);
            }

            selector = new Border()
            {
                //Width = squareSize + borderThicknessOnOneAxis,
                //Height = squareSize + borderThicknessOnOneAxis,
                Width = squareSize + borderThicknessOnOneAxis + borderDistance,
                Height = squareSize + borderThicknessOnOneAxis + borderDistance,
                BorderBrush = Brushes.DodgerBlue,
                BorderThickness = new Thickness(borderThicknessOnOneAxis / 2)
            };


            //Canvas.SetLeft(selector, squareSize * x - borderThicknessOnOneAxis / 2);
            //Canvas.SetTop(selector, squareSize * y - borderThicknessOnOneAxis / 2);

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
                    Width = roboSource.Width,
                    Height = roboSource.Height,
                    Fill = Brushes.Black,
                    Name = Application.Current.FindResource("strWall").ToString()
                };

                SetPosAndUidAccordingToSelector(newWall);
                MapGUI.Children.Add(newWall);
            }

        }
        private void RemoveWall()
        {
            if (ReturnMapGuiShapeWithMouseOver() is Rectangle rect)
            {
                if (rect == roboCleanerOnMapGUI) RoboRemovedFromGUI();
                if (rect != null) MapGUI.Children.Remove(rect);
            }
        }
        private void AddRobo()
        {
            if (roboCleanerOnMapGUI is null && ReturnMapGuiShapeWithMouseOver() == null)
            {
                roboCleanerOnMapGUI = roboSource;
                SetPosAndUidAccordingToSelector(roboCleanerOnMapGUI);
                MapGUI.Children.Add(roboCleanerOnMapGUI);
                rdbAddRc.IsEnabled = false;
            }
        }
        /// <summary>
        /// returns the shape that has the mouse over it, or returns null
        /// </summary>
        /// <returns></returns>
        private Shape ReturnMapGuiShapeWithMouseOver()
        {
            Shape sh = null;
            int i = 0;
            while (i < MapGUI.Children.Count && sh == null)
            {
                if (MapGUI.Children[i] is Shape shape && shape.IsMouseOver) sh = shape;
                i++;
            }
            return sh;
        }
        private void SetPosAndUidAccordingToSelector(UIElement uiE)
        {
            //Canvas.SetTop(uiE, Canvas.GetTop(selector) + borderThicknessOnOneAxis / 2);
            //Canvas.SetLeft(uiE, Canvas.GetLeft(selector) + borderThicknessOnOneAxis / 2);
            int y = (int)Canvas.GetTop(selector);
            int x = (int)Canvas.GetLeft(selector);

            Canvas.SetTop(uiE, y + (borderThicknessOnOneAxis + borderDistance) / 2);
            Canvas.SetLeft(uiE, x + (borderThicknessOnOneAxis + borderDistance) / 2);
            Utilities.SetUID(uiE,y, x);
        }
        private void EditMap()
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (rdbDraw.IsChecked == true) DrawWall();
                if (rdbRemove.IsChecked == true) RemoveWall();
                if (rdbAddRc.IsChecked == true) AddRobo();
            }
           if(Mouse.RightButton == MouseButtonState.Pressed)
            {
                Point pNull = new Point(0, 0);

                Point left = roboCleanerOnMapGUI.TranslatePoint(new Point(0-roboCleanerOnMapGUI.ActualWidth, 0), MapGUI);

                HitTestResult res = VisualTreeHelper.HitTest(MapGUI, left);

                //VisualTreeHelper.GetOffset(roboCleanerOnMapGUI)
                //Canvas  roboCleanerOnMapGUI.Parent();
                //MapGUI.
                //Mouse
                //Mouse.UpdateCursor();
                //if(rec roboCleanerOnMapGUI.TranslatePoint(pNull,MapGUI)
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
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {

                //Rectangle r = new Rectangle { Width = 10, Height = 10, Fill = Brushes.Orange };
                Canvas map = (Canvas)roboCleanerOnMapGUI.Parent;
                //Canvas.SetTop(r, Canvas.GetTop(map));
                //Canvas.SetLeft(r, Canvas.GetLeft(map));
                //map.Children.Add(r);

                foreach (var child in map.Children)
                {
                    if (child is Rectangle rect)
                    {
                        double a = Canvas.GetTop(rect);
                        double b = Canvas.GetTop(roboCleanerOnMapGUI);

                        double x = VisualOffset.X;
                        double y = VisualOffset.Y;

                   if(rect.Uid == "12")
                        {
                            ;
                        }

                        if (a-roboCleanerOnMapGUI.Width == b)
                        {
                            ;
                        }
                    }
                }

            }
        }

        private void btnResetCanvas_Click(object sender, RoutedEventArgs e)
        {
            MapGUI.Children.Clear();
            RoboRemovedFromGUI();
        }

        private void RoboRemovedFromGUI()
        {
            roboCleanerOnMapGUI = null;
            rdbAddRc.IsEnabled = true;
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


