using AmiVanl2.Controller;
using AmiVanl2.Model;
using LiveChartsCore.SkiaSharpView;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AmiVanl2.View
{
    public partial class SuiviAssManuel : UserControl
    {
        private AssManuelController assManuelController;

        public SuiviAssManuel()
        {
            InitializeComponent();

            assManuelController =
                new AssManuelController();

            Loaded += SuiviAssManuel_Loaded;
        }

        private async void SuiviAssManuel_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppData.AssManuels == null || AppData.AssManuels.Count == 0)
            {
                await assManuelController.ChargerAssManuelsAsync();
            }

            ChargerReferences();
        }

        private void ChargerReferences()
        {
            List<string> references =
                AppData.AssManuels
                .Select(x => x.Reference)
                .Distinct()
                .ToList();

            List<AssManuelReferenceViewModel> vues =
                new List<AssManuelReferenceViewModel>();

            foreach (string reference in references)
            {
                AssManuelProduction capuchon =
                    AppData.AssManuels.FirstOrDefault(x =>
                        x.Reference == reference
                        && x.Operation.ToLower().Contains("capuchon"));

                AssManuelProduction insert =
                    AppData.AssManuels.FirstOrDefault(x =>
                        x.Reference == reference
                        && x.Operation.ToLower().Contains("insert"));

                if (capuchon == null && insert == null)
                {
                    continue;
                }

                vues.Add(CreerVueReference(reference, capuchon, insert));
            }

            ListeReferences.ItemsSource = vues;
        }

        private AssManuelReferenceViewModel CreerVueReference(
            string reference,
            AssManuelProduction capuchon,
            AssManuelProduction insert)
        {
            double objectifSemaine =
                capuchon != null
                    ? capuchon.ObjectifSemaine
                    : insert.ObjectifSemaine;

            double objectifJour =
                objectifSemaine / 5.0;

            double[] prodCapuchon =
                ConstruireProductionJournaliere(capuchon);

            double[] prodInsert =
                ConstruireProductionJournaliere(insert);

            double totalCapuchon =
                prodCapuchon.Sum();

            double totalInsert =
                prodInsert.Sum();

            PlotModel graphique =
                CreerGraphiqueReference(
                    reference,
                    prodCapuchon,
                    prodInsert,
                    objectifJour);

            return new AssManuelReferenceViewModel
            {
                Titre =
                    "Référence : " + reference,

                Graphique =
                    graphique,

                InfoCapuchon =
                    "Capuchon : "
                    + totalCapuchon.ToString("0")
                    + " / "
                    + objectifSemaine.ToString("0")
                    + " pièces",

                InfoInsert =
                    "Insert : "
                    + totalInsert.ToString("0")
                    + " / "
                    + objectifSemaine.ToString("0")
                    + " pièces",

                InfoObjectif =
                    "Objectif jour : "
                    + objectifJour.ToString("0")
                    + " | Objectif semaine : "
                    + objectifSemaine.ToString("0")
            };
        }

        private double[] ConstruireProductionJournaliere(
            AssManuelProduction operation)
        {
            if (operation == null)
            {
                return new double[] { 0, 0, 0, 0, 0, 0, 0 };
            }

            return new double[]
            {
                operation.LundiEqu1
                + operation.LundiEqu2
                + operation.LundiEqu3,

                operation.MardiEqu1
                + operation.MardiEqu2
                + operation.MardiEqu3,

                operation.MercrediEqu1
                + operation.MercrediEqu2
                + operation.MercrediEqu3,

                operation.JeudiEqu1
                + operation.JeudiEqu2
                + operation.JeudiEqu3,

                operation.VendrediEqu1
                + operation.VendrediEqu2
                + operation.VendrediEqu3,

                operation.ProdSamedi,

                operation.ProdDimanche
            };
        }

        private PlotModel CreerGraphiqueReference(
    string reference,
    double[] prodCapuchon,
    double[] prodInsert,
    double objectifJour)
        {
            PlotModel model =
                new PlotModel
                {
                    Title = reference
                };

            CategoryAxis axeX =
                new CategoryAxis
                {
                    Position = AxisPosition.Bottom
                };

            axeX.Labels.Add("Lun");
            axeX.Labels.Add("Mar");
            axeX.Labels.Add("Mer");
            axeX.Labels.Add("Jeu");
            axeX.Labels.Add("Ven");
            axeX.Labels.Add("Sam");
            axeX.Labels.Add("Dim");

            LinearAxis axeY =
                new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Minimum = 0,
                    Title = "Production"
                };

            LineSeries serieCapuchon =
                new LineSeries
                {
                    Title = "Capuchon",
                    StrokeThickness = 2,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 4
                };

            LineSeries serieInsert =
                new LineSeries
                {
                    Title = "Insert",
                    StrokeThickness = 2,
                    MarkerType = MarkerType.Square,
                    MarkerSize = 4
                };

            LineSeries serieObjectif =
                new LineSeries
                {
                    Title = "Objectif jour",
                    StrokeThickness = 2,
                    MarkerType = MarkerType.Diamond,
                    MarkerSize = 4
                };

            for (int i = 0; i < 7; i++)
            {
                serieCapuchon.Points.Add(
                    new DataPoint(i, prodCapuchon[i]));

                serieInsert.Points.Add(
                    new DataPoint(i, prodInsert[i]));

                double objectif =
                    i <= 4
                        ? objectifJour
                        : 0;

                serieObjectif.Points.Add(
                    new DataPoint(i, objectif));
            }

            model.Axes.Add(axeX);
            model.Axes.Add(axeY);

            model.Series.Add(serieCapuchon);
            model.Series.Add(serieInsert);
            model.Series.Add(serieObjectif);

            return model;
        }

        public class AssManuelReferenceViewModel
        {
            public string Titre { get; set; } = "";

            public PlotModel Graphique { get; set; }

            public string InfoCapuchon { get; set; } = "";

            public string InfoInsert { get; set; } = "";

            public string InfoObjectif { get; set; } = "";
        }
    }
}