using AmiVanl2.Model;
using OxyPlot;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AmiVanl2.View
{
    public partial class SuiviAssAuto2 : UserControl
    {
        public ObservableCollection<AssAutoCamembertItem> Camemberts { get; set; }

        public SuiviAssAuto2()
        {
            InitializeComponent();

            Camemberts = new ObservableCollection<AssAutoCamembertItem>();
            DataContext = this;

            Loaded += SuiviAssAuto2_Loaded;
        }

        private void SuiviAssAuto2_Loaded(object sender, RoutedEventArgs e)
        {
            ChargerCamemberts();
        }

        private void ChargerCamemberts()
        {
            Camemberts.Clear();

            List<AssAutoProduction> assAutos =
                AppData.AssAutos
                .Where(a => a.ObjectifSemaine > 0)
                .GroupBy(a => a.Reference)
                .Select(g => new AssAutoProduction
                {
                    Reference = g.Key,
                    Machine = string.Join(" / ", g.Select(x => x.Machine).Distinct()),
                    ObjectifSemaine = g.Sum(x => x.ObjectifSemaine),
                    ProdLundi = g.Sum(x => x.ProdLundi),
                    ProdMardi = g.Sum(x => x.ProdMardi),
                    ProdMercredi = g.Sum(x => x.ProdMercredi),
                    ProdJeudi = g.Sum(x => x.ProdJeudi),
                    ProdVendredi = g.Sum(x => x.ProdVendredi),
                    ProdSamedi = g.Sum(x => x.ProdSamedi),
                    ProdDimanche = g.Sum(x => x.ProdDimanche)
                })
                .Take(8)
                .ToList();

            foreach (AssAutoProduction assAuto in assAutos)
            {
                double productionTotale =
                    assAuto.ProdLundi +
                    assAuto.ProdMardi +
                    assAuto.ProdMercredi +
                    assAuto.ProdJeudi +
                    assAuto.ProdVendredi +
                    assAuto.ProdSamedi +
                    assAuto.ProdDimanche;

                Camemberts.Add(
                    CreerCamembert(
                        assAuto,
                        productionTotale,
                        assAuto.ObjectifSemaine));
            }
        }

        private AssAutoCamembertItem CreerCamembert(
            AssAutoProduction assAuto,
            double productionTotale,
            double objectif)
        {
            double reste = objectif - productionTotale;

            if (reste < 0)
                reste = 0;

            bool objectifAtteint = productionTotale >= objectif;

            OxyColor couleurProduction =
                objectifAtteint
                ? OxyColor.FromRgb(40, 160, 80)
                : OxyColor.FromRgb(210, 60, 60);

            PlotModel model = new PlotModel();

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

            return new AssAutoCamembertItem
            {
                Titre = assAuto.Reference + " - " + assAuto.Machine,
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
                mainWindow.MainContent.Children.Add(new SuiviAssAuto());
            }
        }
    }

    public class AssAutoCamembertItem
    {
        public string Titre { get; set; }
        public string Resume { get; set; }
        public PlotModel PlotModel { get; set; }
    }
}