using AmiVanl2.Model;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AmiVanl2.View
{
    public partial class SuiviPresse2 : UserControl
    {
        public ObservableCollection<PresseCamembertItem> Camemberts { get; set; }

        public SuiviPresse2()
        {
            InitializeComponent();

            Camemberts = new ObservableCollection<PresseCamembertItem>();
            DataContext = this;

            Loaded += SuiviPresse2_Loaded;
        }

        private void SuiviPresse2_Loaded(object sender, RoutedEventArgs e)
        {
            ChargerCamemberts();
        }

        private void ChargerCamemberts()
        {
            Camemberts.Clear();

            List<PresseProduction> presses =
            AppData.Presses
            .GroupBy(p => p.Reference)
            .Select(g => g.First())
            .Where(p => p.TotalProduction > 0 || p.ObjectifSemaine > 0)
            .ToList();

            foreach (PresseProduction presse in presses)
            {
                double productionTotale =
                    presse.ProdLundi +
                    presse.ProdMardi +
                    presse.ProdMercredi +
                    presse.ProdJeudi +
                    presse.ProdVendredi +
                    presse.ProdSamedi +
                    presse.ProdDimanche;

                double objectif =
                    presse.ObjectifSemaine;

                Camemberts.Add(
                    CreerCamembert(
                        presse,
                        productionTotale,
                        objectif));
            }
        }

        private PresseCamembertItem CreerCamembert(
            PresseProduction presse,
            double productionTotale,
            double objectif)
        {
            double reste =
                objectif - productionTotale;

            if (reste < 0)
                reste = 0;

            bool objectifAtteint =
                productionTotale >= objectif;

            OxyColor couleurProduction =
                objectifAtteint
                ? OxyColor.FromRgb(40, 160, 80)
                : OxyColor.FromRgb(210, 60, 60);

            PlotModel model =
                new PlotModel();

            PieSeries serie =
                new PieSeries
                {
                StrokeThickness = 0,
                AngleSpan = 360,
                StartAngle = 0,

                InsideLabelPosition = 0.72,
                InsideLabelFormat = "{1}",

                OutsideLabelFormat = "",

                TickDistance = 0,
                TickRadialLength = 0,
                TickHorizontalLength = 0,

                FontSize = 12,
                FontWeight = OxyPlot.FontWeights.Bold
                };

            serie.Slices.Add(
                new PieSlice("Production", productionTotale)
                {
                    Fill = couleurProduction
                });

            if (reste > 0)
            {
                serie.Slices.Add(
                    new PieSlice("Reste", reste)
                    {
                        Fill = OxyColor.FromRgb(220, 220, 220)
                    });
            }

            model.Series.Add(serie);

            double pourcentage =
                objectif == 0
                ? 0
                : productionTotale / objectif * 100;

            return new PresseCamembertItem
            {
                Titre = presse.Reference + " - " + presse.Machine,
                Resume =
                    productionTotale.ToString("0") +
                    " / " +
                    objectif.ToString("0") +
                    " pièces (" +
                    pourcentage.ToString("0") +
                    " %)",
                PlotModel = model
            };
        }

        private void BtnRetourPage1_Click(object sender, RoutedEventArgs e)
        {
            Window fenetre = Window.GetWindow(this);

            if (fenetre is MainWindow mainWindow)
            {
                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(new SuiviPresse());
            }
        }
    }

    public class PresseCamembertItem
    {
        public string Titre { get; set; }
        public string Resume { get; set; }
        public PlotModel PlotModel { get; set; }
    }
}