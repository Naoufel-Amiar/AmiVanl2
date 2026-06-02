using AmiVanl2.Controller;
using AmiVanl2.Model;
using OxyPlot;
using OxyPlot.Annotations;
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
        private Button _boutonSelectionne;

        private static readonly string[] LabelsJours =
            { "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim" };

        public SuiviAssManuel()
        {
            InitializeComponent();
            assManuelController = new AssManuelController();
            Loaded += SuiviAssManuel_Loaded;
        }

        private async void SuiviAssManuel_Loaded(object sender, RoutedEventArgs e)
        {
            if (AppData.AssManuels == null || AppData.AssManuels.Count == 0)
                await assManuelController.ChargerAssManuelsAsync();

            ChargerBoutons();
        }

        private void ChargerBoutons()
        {
            var refsAvecProd = AppData.AssManuels
                .Select(x => x.Reference)
                .Distinct()
                .Where(ref_ =>
                {
                    var ops = AppData.AssManuels.Where(x => x.Reference == ref_).ToList();
                    return ops.Any(p =>
                        p.LundiEqu1 + p.LundiEqu2 + p.LundiEqu3 +
                        p.MardiEqu1 + p.MardiEqu2 + p.MardiEqu3 +
                        p.MercrediEqu1 + p.MercrediEqu2 + p.MercrediEqu3 +
                        p.JeudiEqu1 + p.JeudiEqu2 + p.JeudiEqu3 +
                        p.VendrediEqu1 + p.VendrediEqu2 + p.VendrediEqu3 > 0);
                })
                .Select(ref_ => new RefViewModel { Reference = ref_ })
                .ToList();

            ListeBoutons.ItemsSource = refsAvecProd;

            if (refsAvecProd.Count > 0)
                ChargerDetailReference(refsAvecProd[0].Reference);
        }

        private void BtnReference_Click(object sender, RoutedEventArgs e)
        {
            Button bouton = sender as Button;
            if (bouton == null) return;

            RefViewModel vm = bouton.Tag as RefViewModel;
            if (vm == null) return;

            if (_boutonSelectionne != null)
                _boutonSelectionne.Background = System.Windows.Media.Brushes.Transparent;

            bouton.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(37, 99, 235));
            _boutonSelectionne = bouton;

            ChargerDetailReference(vm.Reference);
        }

        private void ChargerDetailReference(string reference)
        {
            AssManuelProduction capuchon = AppData.AssManuels.FirstOrDefault(x =>
                x.Reference == reference && x.Operation.ToLower().Contains("capuchon"));

            AssManuelProduction insert = AppData.AssManuels.FirstOrDefault(x =>
                x.Reference == reference && x.Operation.ToLower().Contains("insert"));

            double objectifSemaine = capuchon != null
                ? (capuchon.ObjectifSemaine > 0 ? capuchon.ObjectifSemaine : (insert != null ? insert.ObjectifSemaine : 0))
                : (insert != null ? insert.ObjectifSemaine : 0);

            double objectifJour = objectifSemaine / 5.0;

            double[] prodCapuchon = ConstruireProduction(capuchon);
            double[] prodInsert   = ConstruireProduction(insert);

            double totalCapuchon = prodCapuchon.Take(5).Sum();
            double totalInsert   = prodInsert.Take(5).Sum();

            TitreReference.Text = "Référence : " + reference;

            AssManuelProduction anyWithComment = AppData.AssManuels
                .FirstOrDefault(x => x.Reference == reference
                    && !string.IsNullOrWhiteSpace(x.Commentaire));

            if (anyWithComment != null)
            {
                TxtCommentaire.Text = anyWithComment.Commentaire;
                PanelCommentaire.Visibility = Visibility.Visible;
            }
            else
            {
                PanelCommentaire.Visibility = Visibility.Collapsed;
            }

            TxtInfoCapuchon.Text = "Capuchon : " + totalCapuchon.ToString("0")
                + " / " + objectifSemaine.ToString("0") + " pcs";

            TxtInfoInsert.Text = "Insert : " + totalInsert.ToString("0")
                + " / " + objectifSemaine.ToString("0") + " pcs";

            TxtInfoObjectif.Text = "Obj/jour : " + objectifJour.ToString("0")
                + "  |  Obj semaine : " + objectifSemaine.ToString("0");

            PlotCapuchon.Model = BuildBarChart(prodCapuchon, objectifJour);
            PlotInsert.Model   = BuildBarChart(prodInsert,   objectifJour);
        }

        private PlotModel BuildBarChart(double[] prods, double objectifJour)
        {
            double maxProd = prods.Length > 0 ? prods.Max() : 0;
            double yMax = objectifJour > 0
                ? System.Math.Max(maxProd, objectifJour) * 1.15
                : (maxProd > 0 ? maxProd * 1.15 : 10);

            var model = new PlotModel { Background = OxyColors.White };

            var axeX = new CategoryAxis { Position = AxisPosition.Bottom };
            foreach (string label in LabelsJours)
                axeX.Labels.Add(label);

            var axeY = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = yMax,
                Title = "Production"
            };

            var serie = new RectangleBarSeries();

            for (int j = 0; j < 7; j++)
            {
                double prod = prods[j];
                bool weekend = j >= 5;

                OxyColor couleur;
                if (weekend)
                    couleur = OxyColor.FromRgb(100, 150, 220);
                else if (objectifJour > 0)
                    couleur = prod >= objectifJour
                        ? OxyColor.FromRgb(40, 160, 80)
                        : OxyColor.FromRgb(210, 60, 60);
                else
                    couleur = OxyColor.FromRgb(100, 150, 220);

                serie.Items.Add(new RectangleBarItem(j - 0.35, 0, j + 0.35, prod)
                {
                    Color = couleur
                });
            }

            if (objectifJour > 0)
            {
                model.Annotations.Add(new LineAnnotation
                {
                    Type = LineAnnotationType.Horizontal,
                    Y = objectifJour,
                    Color = OxyColor.FromRgb(220, 80, 0),
                    StrokeThickness = 2.5,
                    LineStyle = LineStyle.Solid,
                    Text = "Obj: " + objectifJour.ToString("0"),
                    FontSize = 11,
                    FontWeight = OxyPlot.FontWeights.Bold
                });
            }

            model.Axes.Add(axeX);
            model.Axes.Add(axeY);
            model.Series.Add(serie);

            return model;
        }

        private double[] ConstruireProduction(AssManuelProduction op)
        {
            if (op == null)
                return new double[] { 0, 0, 0, 0, 0, 0, 0 };

            return new double[]
            {
                op.LundiEqu1    + op.LundiEqu2    + op.LundiEqu3,
                op.MardiEqu1    + op.MardiEqu2    + op.MardiEqu3,
                op.MercrediEqu1 + op.MercrediEqu2 + op.MercrediEqu3,
                op.JeudiEqu1    + op.JeudiEqu2    + op.JeudiEqu3,
                op.VendrediEqu1 + op.VendrediEqu2 + op.VendrediEqu3,
                op.ProdSamedi,
                op.ProdDimanche
            };
        }

        public class RefViewModel
        {
            public string Reference { get; set; } = "";
        }
    }
}
