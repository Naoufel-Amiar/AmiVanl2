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
    public partial class SuiviJoints : UserControl
    {
        private JointController jointController;
        private Button _boutonSelectionne;

        public SuiviJoints()
        {
            InitializeComponent();

            jointController = new JointController();

            Loaded += SuiviJoints_Loaded;
        }

        private async void SuiviJoints_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppData.Joints == null || AppData.Joints.Count == 0)
            {
                await jointController.ChargerJointsAsync();
            }

            var refsAvecProd = AppData.Joints
                .Where(j => j.TotalProduction > 0)
                .ToList();

            ListeReferences.ItemsSource = refsAvecProd;

            JointProduction premiereReference =
                refsAvecProd.FirstOrDefault() ?? AppData.Joints.FirstOrDefault();

            if (premiereReference == null)
            {
                TitreReference.Text = "Aucune donnée joint chargée.";
                return;
            }

            ChargerReference(premiereReference);
        }

        private void BtnReference_Click(object sender, RoutedEventArgs e)
        {
            Button bouton = sender as Button;
            if (bouton == null) return;

            JointProduction joint = bouton.Tag as JointProduction;
            if (joint == null) return;

            if (_boutonSelectionne != null)
                _boutonSelectionne.Background = System.Windows.Media.Brushes.Transparent;

            bouton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(37, 99, 235));
            _boutonSelectionne = bouton;

            ChargerReference(joint);
        }

        private void ChargerReference(JointProduction joint)
        {
            TitreReference.Text =
                "Référence : "
                + joint.Reference
                + " | Ancien code : "
                + joint.AncienCode;

            if (!string.IsNullOrWhiteSpace(joint.Commentaire))
            {
                TxtCommentaire.Text = joint.Commentaire;
                PanelCommentaire.Visibility = Visibility.Visible;
            }
            else
            {
                PanelCommentaire.Visibility = Visibility.Collapsed;
            }

            List<PlotModel> camemberts =
                new List<PlotModel>();

            camemberts.Add(CreerCamembert(
                joint.LabelLundi,
                joint.LundiEqu1,
                joint.LundiEqu2,
                joint.LundiEqu3,
                joint.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                joint.LabelMardi,
                joint.MardiEqu1,
                joint.MardiEqu2,
                joint.MardiEqu3,
                joint.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                joint.LabelMercredi,
                joint.MercrediEqu1,
                joint.MercrediEqu2,
                joint.MercrediEqu3,
                joint.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                joint.LabelJeudi,
                joint.JeudiEqu1,
                joint.JeudiEqu2,
                joint.JeudiEqu3,
                joint.ObjectifEquipe));

            camemberts.Add(CreerCamembert(
                joint.LabelVendredi,
                joint.VendrediEqu1,
                joint.VendrediEqu2,
                joint.VendrediEqu3,
                joint.ObjectifEquipe));

            camemberts.Add(CreerCamembertWeekend(
                joint.LabelSamedi,
                joint.ProdSamedi));

            camemberts.Add(CreerCamembertWeekend(
                joint.LabelDimanche,
                joint.ProdDimanche));

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