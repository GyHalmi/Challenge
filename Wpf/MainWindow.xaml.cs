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
        const int selectorSizeGrower = 4;
        Rectangle roboCleaner = null;

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
                (Canvas.GetLeft(selector) + selectorSizeGrower / 2 != x || Canvas.GetTop(selector) + selectorSizeGrower / 2 != y))
            {
                MapGUI.Children.Remove(selector);
            }

            selector = new Border()
            {
                Width = squareSize + selectorSizeGrower,
                Height = squareSize + selectorSizeGrower,
                BorderBrush = Brushes.DodgerBlue,
                BorderThickness = new Thickness(selectorSizeGrower / 2)
            };


            Canvas.SetLeft(selector, squareSize * x - selectorSizeGrower / 2);
            Canvas.SetTop(selector, squareSize * y - selectorSizeGrower / 2);
            MapGUI.Children.Add(selector);
        }
        private void DrawWall()
        {
            if (IsMouseOverAnyMapGuiShape() == null)
            {
                Rectangle robo = (Rectangle)Application.Current.FindResource("roboCleaner");
                Rectangle newWall = new Rectangle
                {
                    Width = robo.Width,
                    Height = robo.Height,
                    Fill = Brushes.Black
                };

                SetUiElementPosInsideSelector(newWall);
                MapGUI.Children.Add(newWall);
            }

        }
        private void RemoveWall()
        {
            if (IsMouseOverAnyMapGuiShape() is Rectangle rect)
            {
                if (rect == roboCleaner) RoboRemovedFromGUI();
                if (rect != null) MapGUI.Children.Remove(rect);
            }
        }
        private void AddRobo()
        {
            if (roboCleaner is null && IsMouseOverAnyMapGuiShape() == null)
            {
                roboCleaner = roboSource;
                SetUiElementPosInsideSelector(roboCleaner);
                MapGUI.Children.Add(roboCleaner);
                rdbAddRc.IsEnabled = false;
            }
        }
        /// <summary>
        /// returns the shape that has the mouse over it, or returns null
        /// </summary>
        /// <returns></returns>
        private Shape IsMouseOverAnyMapGuiShape()
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
        private void SetUiElementPosInsideSelector(UIElement uiE)
        {
            Canvas.SetTop(uiE, Canvas.GetTop(selector) + selectorSizeGrower / 2);
            Canvas.SetLeft(uiE, Canvas.GetLeft(selector) + selectorSizeGrower / 2);
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
        private void canvPresentation_MouseMove(object sender, MouseEventArgs e)
        {
            DrawSelector(e.MouseDevice.GetPosition(MapGUI));
            EditMap();
        }
        private void canvPresentation_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EditMap();
        }

        private void btnResetCanvas_Click(object sender, RoutedEventArgs e)
        {
            MapGUI.Children.Clear();
            RoboRemovedFromGUI();
        }

        private void RoboRemovedFromGUI()
        {
            roboCleaner = null;
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


