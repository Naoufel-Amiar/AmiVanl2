using AmiVanl2.View;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AmiVanl2
{
    public partial class MainWindow : Window
    {
        private string selectedFilePath = "";

        public MainWindow()
        {
            InitializeComponent();
        }



        //ACCUEIL / IMPORT

        private void BtnAccueilImport_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(new AccImport());
        }

        private void BtnSuiviPresse_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviPresse());
        }

        private void BtnSuiviTri_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviTri());
        }

        private void BtnSuiviAssAuto_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviAssAuto());
        }

        private void BtnSuiviAssManuel_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviAssManuel());
        }

        private void BtnSuiviJoints_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviJoints());
        }

        private void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Children.Clear();
            MainContent.Children.Add(new ExportPDF());
        }
    }
}