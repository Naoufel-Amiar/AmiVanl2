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
        private static readonly SolidColorBrush CouleurActive  = new SolidColorBrush(Color.FromRgb(47, 93, 140));
        private static readonly SolidColorBrush CouleurNormale = new SolidColorBrush(Color.FromRgb(58, 58, 61));

        private Button _boutonActif;

        public MainWindow()
        {
            InitializeComponent();
            MettreAJourNavigation();
            MainContent.Children.Add(new WelcomePage());
            MarquerBoutonActif(BtnAccueilImport);
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
            BtnSuiviAssTige.IsEnabled = navigationActive;
            BtnExportPDF.IsEnabled = navigationActive;

            BtnAccueilImport.IsEnabled = true;
        }

        public void MarquerBoutonActif(Button bouton)
        {
            if (_boutonActif != null)
                _boutonActif.Background = CouleurNormale;

            _boutonActif = bouton;
            _boutonActif.Background = CouleurActive;
        }

        private void BtnAccueilImport_Click(object sender, RoutedEventArgs e)
        {
            MarquerBoutonActif(BtnAccueilImport);
            MainContent.Children.Clear();
            MainContent.Children.Add(new AccImport());
        }

        private void BtnSuiviPresse_Click(object sender, RoutedEventArgs e)
        {
            MarquerBoutonActif(BtnSuiviPresse);
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviPresse());
        }

        private void BtnSuiviTri_Click(object sender, RoutedEventArgs e)
        {
            MarquerBoutonActif(BtnSuiviTri);
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviTri());
        }

        private void BtnSuiviAssAuto_Click(object sender, RoutedEventArgs e)
        {
            MarquerBoutonActif(BtnSuiviAssAuto);
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviAssAuto());
        }

        private void BtnSuiviAssManuel_Click(object sender, RoutedEventArgs e)
        {
            MarquerBoutonActif(BtnSuiviAssManuel);
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviAssManuel(AppData.AssManuels, "SUIVI ASS MANUEL — EV"));
        }

        private void BtnSuiviAssTige_Click(object sender, RoutedEventArgs e)
        {
            MarquerBoutonActif(BtnSuiviAssTige);
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviAssManuel(AppData.TigesPoussee, "SUIVI ASS MANUEL — TIGE DE POUSSÉE"));
        }

        private void BtnSuiviJoints_Click(object sender, RoutedEventArgs e)
        {
            MarquerBoutonActif(BtnSuiviJoints);
            MainContent.Children.Clear();
            MainContent.Children.Add(new SuiviJoints());
        }

        private void BtnExportPDF_Click(object sender, RoutedEventArgs e)
        {
            MarquerBoutonActif(BtnExportPDF);
            MainContent.Children.Clear();
            MainContent.Children.Add(new ExportPDF());
        }
    }
}