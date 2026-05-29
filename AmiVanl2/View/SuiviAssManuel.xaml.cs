using AmiVanl2.Controller;
using AmiVanl2.Model;
using OxyPlot;
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

        private string referenceSelectionnee = "";

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

            List<string> references =
                AppData.AssManuels
                .Select(x => x.Reference)
                .Distinct()
                .ToList();

            ListeReferences.ItemsSource = references;

            string premiereReference =
                references.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(premiereReference))
            {
                TitreReference.Text = "Aucune donnée ASS manuel chargée.";
                return;
            }

            referenceSelectionnee = premiereReference;

            ChargerOperations(referenceSelectionnee);
        }

        private void BtnReference_Click(object sender, RoutedEventArgs e)
        {
            Button bouton =
                sender as Button;

            if (bouton == null)
            {
                return;
            }

            string reference =
                bouton.Tag as string;

            if (string.IsNullOrWhiteSpace(reference))
            {
                return;
            }

            referenceSelectionnee = reference;

            ChargerOperations(referenceSelectionnee);
        }

        private void ChargerOperations(string reference)
        {
            List<string> operations =
                AppData.AssManuels
                .Where(x => x.Reference == reference)
                .Select(x => x.Operation)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            ComboOperations.ItemsSource = operations;

            if (operations.Count > 0)
            {
                ComboOperations.SelectedIndex = 0;
            }
            else
            {
                TitreReference.Text =
                    "Référence : " + reference + " | aucune opération trouvée.";

                ListeCamemberts.ItemsSource = null;
            }
        }

        private void ComboOperations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(referenceSelectionnee))
            {
                return;
            }

            string operation =
                ComboOperations.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(operation))
            {
                return;
            }

            AssManuelProduction assManuel =
                AppData.AssManuels
                .FirstOrDefault(x =>
                    x.Reference == referenceSelectionnee
                    && x.Operation == operation);

            if (assManuel == null)
            {
                return;
            }

            ChargerReferenceOperation(assManuel);
        }

        private void ChargerReferenceOperation(AssManuelProduction assManuel)
        {
            TitreReference.Text =
                "Référence : "
                + assManuel.Reference
                + " | Opération : "
                + assManuel.Operation
                + " | Équipe : "
                + assManuel.Equipe;

            List<PlotModel> camemberts =
                new List<PlotModel>();

            camemberts.Add(CreerCamembert(
                assManuel.LabelLundi,
                assManuel.LundiEqu1,
                assManuel.LundiEqu2,
                assManuel.LundiEqu3,
                assManuel.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                assManuel.LabelMardi,
                assManuel.MardiEqu1,
                assManuel.MardiEqu2,
                assManuel.MardiEqu3,
                assManuel.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                assManuel.LabelMercredi,
                assManuel.MercrediEqu1,
                assManuel.MercrediEqu2,
                assManuel.MercrediEqu3,
                assManuel.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                assManuel.LabelJeudi,
                assManuel.JeudiEqu1,
                assManuel.JeudiEqu2,
                assManuel.JeudiEqu3,
                assManuel.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                assManuel.LabelVendredi,
                assManuel.VendrediEqu1,
                assManuel.VendrediEqu2,
                assManuel.VendrediEqu3,
                assManuel.ObjectifEquipe));

            camemberts.Add(CreerCamembertWeekend(
                assManuel.LabelSamedi,
                assManuel.ProdSamedi));

            camemberts.Add(CreerCamembertWeekend(
                assManuel.LabelDimanche,
                assManuel.ProdDimanche));

            ListeCamemberts.ItemsSource = null;
            ListeCamemberts.ItemsSource = camemberts;
        }

        private PlotModel CreerCamembert(
            string titre,
            double equ1,
            double equ2,
            double equ3,
            double objectifJour)
        {
            double totalProduit =
                equ1 + equ2 + equ3;

            double reste =
                objectifJour - totalProduit;

            if (reste < 0)
            {
                reste = 0;
            }

            PlotModel model =
                new PlotModel
                {
                    Title = titre
                };

            PieSeries serie =
                new PieSeries
                {
                    StrokeThickness = 1,
                    AngleSpan = 360,
                    StartAngle = 0,
                    InsideLabelPosition = 0.65,
                    OutsideLabelFormat = "{1}: {0}",
                    InsideLabelFormat = "{2:0}%"
                };

            if (equ1 > 0)
            {
                serie.Slices.Add(
                    new PieSlice("EQ1", equ1)
                    {
                        Fill = OxyColor.FromRgb(0, 90, 0)
                    });
            }

            if (equ2 > 0)
            {
                serie.Slices.Add(
                    new PieSlice("EQ2", equ2)
                    {
                        Fill = OxyColor.FromRgb(0, 150, 0)
                    });
            }

            if (equ3 > 0)
            {
                serie.Slices.Add(
                    new PieSlice("EQ3", equ3)
                    {
                        Fill = OxyColor.FromRgb(120, 210, 120)
                    });
            }

            if (reste > 0)
            {
                serie.Slices.Add(
                    new PieSlice("Reste", reste)
                    {
                        Fill = OxyColors.Red
                    });
            }

            if (totalProduit == 0 && reste == 0)
            {
                serie.Slices.Add(
                    new PieSlice("Aucune donnée", 1)
                    {
                        Fill = OxyColors.LightGray
                    });
            }

            model.Series.Add(serie);

            return model;
        }

        private PlotModel CreerCamembertWeekend(
            string titre,
            double production)
        {
            PlotModel model =
                new PlotModel
                {
                    Title = titre
                };

            PieSeries serie =
                new PieSeries
                {
                    StrokeThickness = 1,
                    AngleSpan = 360,
                    StartAngle = 0,
                    InsideLabelPosition = 0.7,
                    OutsideLabelFormat = "{1}: {0}",
                    InsideLabelFormat = "{2:0}%"
                };

            if (production > 0)
            {
                serie.Slices.Add(
                    new PieSlice("PROD", production)
                    {
                        Fill = OxyColor.FromRgb(70, 160, 0)
                    });
            }
            else
            {
                serie.Slices.Add(
                    new PieSlice("Aucune prod", 1)
                    {
                        Fill = OxyColors.LightGray
                    });
            }

            model.Series.Add(serie);

            return model;
        }
    }
}