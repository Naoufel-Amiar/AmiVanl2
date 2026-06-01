using AmiVanl2.Model;
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
            MettreAJourNavigation();
        }

        public void MettreAJourNavigation()
        {
            bool navigationActive =
                AppData.HasExcelFile()
                && AppData.DonneesGenerees;

            BtnSuiviPresse.IsEnabled = navigationActive;
            BtnSuiviAssAuto.IsEnabled = navigationActive;
            BtnSuiviJoints.IsEnabled = navigationActive;
            BtnSuiviTri.IsEnabled = navigationActive;
            BtnSuiviAssManuel.IsEnabled = navigationActive;
            BtnExportPDF.IsEnabled = navigationActive;

            BtnAccueilImport.IsEnabled = true;
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