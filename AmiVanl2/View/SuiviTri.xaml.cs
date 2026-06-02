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
    public partial class SuiviTri : UserControl
    {
        private TriController triController;
        private Button _boutonSelectionne;

        public SuiviTri()
        {
            InitializeComponent();

            triController = new TriController();

            Loaded += SuiviTri_Loaded;
        }

        private async void SuiviTri_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppData.Tris == null || AppData.Tris.Count == 0)
            {
                await triController.ChargerTrisAsync();
            }

            var refsAvecProd = AppData.Tris
                .Where(t => t.TotalProduction > 0)
                .ToList();

            ListeReferences.ItemsSource = refsAvecProd;

            TriProduction premiereReference =
                refsAvecProd.FirstOrDefault() ?? AppData.Tris.FirstOrDefault();

            if (premiereReference == null)
            {
                TitreReference.Text = "Aucune donnée TRI chargée.";
                return;
            }

            ChargerReference(premiereReference);
        }

        private void BtnReference_Click(object sender, RoutedEventArgs e)
        {
            Button bouton = sender as Button;
            if (bouton == null) return;

            TriProduction tri = bouton.Tag as TriProduction;
            if (tri == null) return;

            if (_boutonSelectionne != null)
                _boutonSelectionne.Tag = _boutonSelectionne.Tag; // force refresh via trigger

            // reset ancien bouton
            if (_boutonSelectionne != null)
                _boutonSelectionne.Background = System.Windows.Media.Brushes.Transparent;

            bouton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(37, 99, 235));
            _boutonSelectionne = bouton;

            ChargerReference(tri);
        }

        private void ChargerReference(TriProduction tri)
        {
            TitreReference.Text =
                "Référence : "
                + tri.Reference
                + " | Ancien code : "
                + tri.AncienCode;

            List<PlotModel> camemberts =
                new List<PlotModel>();

            camemberts.Add(CreerCamembert(
                tri.LabelLundi,
                tri.LundiEqu1,
                tri.LundiEqu2,
                tri.LundiEqu3,
                tri.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                tri.LabelMardi,
                tri.MardiEqu1,
                tri.MardiEqu2,
                tri.MardiEqu3,
                tri.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                tri.LabelMercredi,
                tri.MercrediEqu1,
                tri.MercrediEqu2,
                tri.MercrediEqu3,
                tri.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                tri.LabelJeudi,
                tri.JeudiEqu1,
                tri.JeudiEqu2,
                tri.JeudiEqu3,
                tri.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                tri.LabelVendredi,
                tri.VendrediEqu1,
                tri.VendrediEqu2,
                tri.VendrediEqu3,
                tri.ObjectifEquipe));

            camemberts.Add(CreerCamembertWeekend(
                tri.LabelSamedi,
                tri.ProdSamedi));

            camemberts.Add(CreerCamembertWeekend(
                tri.LabelDimanche,
                tri.ProdDimanche));

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
                    InsideLabelFormat = "{2:0}%",
                    FontSize = 11,
                    Diameter = 0.7
                };

            if (equ1 > 0)
            {
                serie.Slices.Add(
                    new PieSlice("EQU1", equ1)
                    {
                        Fill = OxyColor.FromRgb(0, 90, 0)
                    });
            }

            if (equ2 > 0)
            {
                serie.Slices.Add(
                    new PieSlice("EQU2", equ2)
                    {
                        Fill = OxyColor.FromRgb(0, 150, 0)
                    });
            }

            if (equ3 > 0)
            {
                serie.Slices.Add(
                    new PieSlice("EQU3", equ3)
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
                    InsideLabelFormat = "{2:0}%",
                    FontSize = 11,
                    Diameter = 0.7
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