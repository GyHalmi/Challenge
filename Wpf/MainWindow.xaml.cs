﻿using Microsoft.Win32;
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
        Point roboStartPosition;

        Border selector;
        const int borderThicknessOnOneAxis = 2;
        int borderDistance = 4;

        string savedMapsPath = AppDomain.CurrentDomain.BaseDirectory + Application.Current.FindResource("savedMapsPath").ToString();
        ObservableCollection<ListBoxItem> savedMaps;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSavedMaps();
            InitializeRobocleanerOnMapGUI();
            InitializeSelector();




            //robo bindings
            Binding roboEnabled = new Binding("IsEnabled");
            roboEnabled.Source = roboCleanerOnMapGUI;
            btnStart.SetBinding(Button.IsEnabledProperty, roboEnabled);

            tbTest.SetBinding(TextBlock.TextProperty, roboEnabled);

            Binding roboEnabledReversed = new Binding("IsEnabled");
            roboEnabledReversed.Source = roboCleanerOnMapGUI;
            roboEnabledReversed.Converter = new Utilities.ConverterReverseBool();
            rdbAddRc.SetBinding(Button.IsEnabledProperty, roboEnabledReversed);

            //store maps bindings
            Binding savedMapSelected = new Binding("SelectedItem");
            savedMapSelected.Source = lbSavedMaps;
            savedMapSelected.Converter = new Utilities.ConverterObjectIsNullToBool();
            btnLoad.SetBinding(Button.IsEnabledProperty, savedMapSelected);
            btnDelete.SetBinding(Button.IsEnabledProperty, savedMapSelected);

            Binding selectedContent = new Binding("SelectedItem");
            selectedContent.Source = lbSavedMaps;
            selectedContent.Converter = new Utilities.ConverterSelectedItemToItsContent();
            txtSave.SetBinding(TextBox.TextProperty, selectedContent);

        }

        private void InitializeSavedMaps()
        {
            savedMaps = new ObservableCollection<ListBoxItem>();
            //savedMaps.Any()
            //Binding binding 
            lbSavedMaps.ItemsSource = savedMaps;


            try // try to access savedMapsPath
            {
                foreach (string item in Directory.GetFiles(savedMapsPath))
                {
                    savedMaps.Add(FileNameWithPathToListBoxItem(item));
                }
            }
            catch (DirectoryNotFoundException)
            {
                ;
            }
        }

        private void InitializeSelector()
        {
            selector = new Border()
            {
                Width = roboWidth + borderThicknessOnOneAxis + borderDistance,
                Height = roboHeight + borderThicknessOnOneAxis + borderDistance,
                BorderBrush = Brushes.DodgerBlue,
                BorderThickness = new Thickness(borderThicknessOnOneAxis / 2)
            };
            Canvas.SetZIndex(selector, 2);
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

            int x = Math.Min((int)mousePosition.X / squareSize, (int)MapGUI.Width / squareSize - 1);
            int y = Math.Min((int)mousePosition.Y / squareSize, (int)MapGUI.Height / squareSize - 1);


            if (MapGUI.Children.IndexOf(selector) == -1)
            {
                MapGUI.Children.Add(selector);
            }
            if (Canvas.GetLeft(selector) + borderThicknessOnOneAxis / 2 != x ||
                Canvas.GetTop(selector) + borderThicknessOnOneAxis / 2 != y)
            {
                Canvas.SetTop(selector, squareSize * y - (borderThicknessOnOneAxis + borderDistance) / 2);
                Canvas.SetLeft(selector, squareSize * x - (borderThicknessOnOneAxis + borderDistance) / 2);
            }

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
                roboStartPosition = new Point(Canvas.GetLeft(roboCleanerOnMapGUI), Canvas.GetTop(roboCleanerOnMapGUI));
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
            //SaveMapGUI();
            new RoboCleaner(roboCleanerOnMapGUI).CleanTheHouse();
        }
        private ListBoxItem FileNameWithPathToListBoxItem(string fileNameWithPath)
        {
            return new ListBoxItem()
            {
                Content = System.IO.Path.GetFileNameWithoutExtension(fileNameWithPath),
                DataContext = fileNameWithPath
            };
        }
        private void SaveMapGUI()
        {
            MapGUI.Children.Remove(selector);
            Canvas.SetLeft(roboCleanerOnMapGUI, roboStartPosition.X);
            Canvas.SetTop(roboCleanerOnMapGUI, roboStartPosition.Y);

            Directory.CreateDirectory(savedMapsPath);
            string fileNameWithPath = savedMapsPath + txtSave.Text + ".xaml";
            using (FileStream fs = File.Open(fileNameWithPath, FileMode.Create))
            {
                XamlWriter.Save(MapGUI, fs);
            }

            savedMaps.Add(FileNameWithPathToListBoxItem(fileNameWithPath));

        }
        private void LoadMapGUI()
        {
            try
            {
                Canvas savedCanvas;
                string fileNameWithPath = ((ListBoxItem)lbSavedMaps.SelectedItem).DataContext.ToString();

                using (FileStream fs = File.Open(fileNameWithPath, FileMode.Open, FileAccess.Read))
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
                    else if (rect.Tag != null && rect.Tag.ToString() == "Wall")
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveMapGUI();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedItem = lbSavedMaps.SelectedItem as ListBoxItem;
            savedMaps.Remove(selectedItem);
            File.Delete(selectedItem.DataContext.ToString());
        }
    }

}


