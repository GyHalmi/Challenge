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
        int squareSize = 20;
        Border selector = null;
        int selectorSizeGrower = 4;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DrawSelector(Point mousePosition)
        {
            //must be an even number
            selectorSizeGrower = 8;

            int x = Math.Min((int)mousePosition.X / squareSize, (int)canvPresentation.ActualWidth / squareSize - 1);
            int y = Math.Min((int)mousePosition.Y / squareSize, (int)canvPresentation.ActualHeight / squareSize - 1);

            if (selector != null &&
                (Canvas.GetLeft(selector) + selectorSizeGrower / 2 != x || Canvas.GetTop(selector) + selectorSizeGrower / 2 != y))
            {
                canvPresentation.Children.Remove(selector);
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

            canvPresentation.Children.Add(selector);
        }
        private void DrawWall()
        {
            Rectangle wallWithMouseOver = null;

            foreach (UIElement item in canvPresentation.Children)
            {
                if (item is Rectangle rect)
                {
                    if (item.IsMouseOver) wallWithMouseOver = rect;
                }
            }

            if (wallWithMouseOver == null)
            {
                Rectangle newWall = new Rectangle
                {
                    Width = squareSize,
                    Height = squareSize,
                    Fill = Brushes.Black
                };

                Canvas.SetTop(newWall, Canvas.GetTop(selector) + selectorSizeGrower / 2);
                Canvas.SetLeft(newWall, Canvas.GetLeft(selector) + selectorSizeGrower / 2);

                canvPresentation.Children.Add(newWall);
            }

        }
        private void RemoveWall()
        {
            Rectangle toDel = null;

            foreach (UIElement item in canvPresentation.Children)
            {
                if (item is Rectangle rect && rect.IsMouseOver)
                {
                    toDel = rect;
                }
            }

            canvPresentation.Children.Remove(toDel);
        }
        private void DrawOrRemoveWall()
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (rdbDraw.IsChecked == true) DrawWall();
                if (rdbRemove.IsChecked == true) RemoveWall();
            }
        }
        private void canvPresentation_MouseMove(object sender, MouseEventArgs e)
        {
            DrawSelector(e.MouseDevice.GetPosition(canvPresentation));
            DrawOrRemoveWall();
        }
        private void canvPresentation_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DrawOrRemoveWall();
        }

        private void btnResetCanvas_Click(object sender, RoutedEventArgs e)
        {
            canvPresentation.Children.Clear();
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


