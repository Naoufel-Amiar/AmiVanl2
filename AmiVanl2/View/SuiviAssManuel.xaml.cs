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

        private List<AssManuelProduction> _source;
        private string _titrePage;

        private static readonly string[] LabelsJours =
            { "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim" };

        // Ass Manuel EV (capuchon / insert)
        public SuiviAssManuel() : this(null, "SUIVI ASS MANUEL — EV") { }

        // Constructeur générique
        public SuiviAssManuel(List<AssManuelProduction> source, string titrePage)
        {
            InitializeComponent();
            assManuelController = new AssManuelController();
            _source = source;
            _titrePage = titrePage;
            Loaded += SuiviAssManuel_Loaded;
        }

        private async void SuiviAssManuel_Loaded(object sender, RoutedEventArgs e)
        {
            TitrePage.Text = _titrePage;

            if (AppData.AssManuels == null || AppData.AssManuels.Count == 0)
                await assManuelController.ChargerAssManuelsAsync();

            // Si source non passée → défaut = EV
            if (_source == null)
                _source = AppData.AssManuels;

            ChargerBoutons();
        }

        private void ChargerBoutons()
        {
            var refs = _source
                .Select(x => x.Reference)
                .Distinct()
                .Select(r => new RefViewModel { Reference = r })
                .ToList();

            ListeBoutons.ItemsSource = refs;

            if (refs.Count > 0)
                ChargerDetailReference(refs[0].Reference);
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
            var operations = _source
                .Where(x => x.Reference == reference)
                .OrderBy(x => x.Operation)
                .ToList();

            if (operations.Count == 0) return;

            double objectifSemaine = operations
                .Select(x => x.ObjectifSemaine)
                .FirstOrDefault(v => v > 0);

            double objectifJour = objectifSemaine / 5.0;

            TitreReference.Text = "Référence : " + reference;

            // Commentaire
            var avecCommentaire = operations.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Commentaire));
            if (avecCommentaire != null)
            {
                TxtCommentaire.Text = avecCommentaire.Commentaire;
                PanelCommentaire.Visibility = Visibility.Visible;
            }
            else
            {
                PanelCommentaire.Visibility = Visibility.Collapsed;
            }

            // Infos résumé
            PanelInfos.Children.Clear();
            foreach (var op in operations)
            {
                double[] prods = ConstruireProduction(op);
                double total = prods.Take(5).Sum();
                var tb = new TextBlock
                {
                    Text = op.Operation + " : " + total.ToString("0") + " / " + objectifSemaine.ToString("0") + " pcs",
                    FontFamily = new System.Windows.Media.FontFamily("Bahnschrift"),
                    FontSize = 13,
                    Margin = new Thickness(0, 0, 25, 0)
                };
                PanelInfos.Children.Add(tb);
            }
            var tbObj = new TextBlock
            {
                Text = "Obj/jour : " + objectifJour.ToString("0") + "  |  Obj semaine : " + objectifSemaine.ToString("0"),
                FontFamily = new System.Windows.Media.FontFamily("Bahnschrift"),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold
            };
            PanelInfos.Children.Add(tbObj);

            // Graphiques dynamiques
            var graphItems = operations.Select(op => new GraphItem
            {
                Titre = op.Operation,
                Modele = BuildBarChart(ConstruireProduction(op), objectifJour)
            }).ToList();

            // UniformGrid ajuste automatiquement le nombre de colonnes
            var panel = PanelGraphiques.ItemsPanel.LoadContent() as UniformGrid;
            PanelGraphiques.ItemsSource = graphItems;
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

        public class GraphItem
        {
            public string Titre { get; set; } = "";
            public PlotModel Modele { get; set; }
        }
    }
}
