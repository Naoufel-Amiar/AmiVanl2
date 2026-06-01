using AmiVanl2.Model;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AmiVanl2.View
{
    public partial class SuiviPresse : UserControl
    {
        public SuiviPresse()
        {
            InitializeComponent();
            ChargerGraphiquePresse();
        }

        private void ChargerGraphiquePresse()
        {
            if (AppData.Presses == null || AppData.Presses.Count == 0)
                return;

            var refsDistinctes = AppData.Presses
                .GroupBy(p => new { p.Reference, p.Machine })
                .Select(g => g.First())
                .Where(p => p.TotalProduction > 0 || p.ObjectifSemaine > 0)
                .ToList();
            int moitie = (refsDistinctes.Count + 1) / 2;
            List<PresseProduction> groupe1 = refsDistinctes.Take(moitie).ToList();
            List<PresseProduction> groupe2 = refsDistinctes.Skip(moitie).ToList();

            ChargerGraphiqueGroupe(groupe1, PlotPresseGroupe1, LegendPresseGroupe1, "Production presse — Groupe 1");
            ChargerGraphiqueGroupe(groupe2, PlotPresseGroupe2, LegendPresseGroupe2, "Production presse — Groupe 2");
        }

        private void ChargerGraphiqueGroupe(List<PresseProduction> groupe,OxyPlot.Wpf.PlotView plot, Grid legend, string titre)
        {
            if (groupe == null || groupe.Count == 0)
                return;

            PresseProduction premiereLigne = groupe[0];

            string[] labels =
            {
                premiereLigne.LabelLundi,
                premiereLigne.LabelMardi,
                premiereLigne.LabelMercredi,
                premiereLigne.LabelJeudi,
                premiereLigne.LabelVendredi,
                premiereLigne.LabelSamedi,
                premiereLigne.LabelDimanche
            };

            PlotModel model = new PlotModel();
            model.Title = titre;
            CategoryAxis axeX = new CategoryAxis();
            axeX.Position = AxisPosition.Bottom;

            foreach (string label in labels)
                axeX.Labels.Add(label);

            LinearAxis axeY = new LinearAxis();
            axeY.Position = AxisPosition.Left;
            axeY.Title = "Production";
            axeY.Minimum = 0;

            RectangleBarSeries serie = new RectangleBarSeries();
            serie.Title = "Production réelle";

            double largeurBarre = 0.18;
            double espaceGroupe = 0.8;

            for (int jour = 0; jour < 7; jour++)
            {
                for (int refIndex = 0; refIndex < groupe.Count; refIndex++)
                {
                    PresseProduction presse = groupe[refIndex];

                    double production = GetProductionJour(presse, jour);
                    double objectifJour = presse.ObjectifSemaine / 7.0;

                    double xCentre = jour;
                    double depart = xCentre - (espaceGroupe / 2.0);
                    double x0 = depart + refIndex * largeurBarre;
                    double x1 = x0 + largeurBarre;

                    RectangleBarItem item = new RectangleBarItem(x0, 0, x1, production);

                    item.Color = production >= objectifJour
                        ? OxyColors.SeaGreen
                        : OxyColors.IndianRed;

                    serie.Items.Add(item);
                }
            }

            model.Axes.Add(axeX);
            model.Axes.Add(axeY);
            model.Series.Add(serie);

            plot.Model = model;

            ChargerLegendeGroupe(groupe, labels, legend);
        }

        private void ChargerLegendeGroupe(List<PresseProduction> groupe, string[] labels, Grid legend)
        {
            legend.Children.Clear();
            legend.ColumnDefinitions.Clear();

            for (int i = 0; i < 7; i++)
            {
                legend.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int jour = 0; jour < 7; jour++)
            {
                StackPanel panelJour = new StackPanel();
                panelJour.Margin = new System.Windows.Thickness(3);

                TextBlock titreJour = new TextBlock();
                titreJour.Text = labels[jour];
                titreJour.FontFamily = new System.Windows.Media.FontFamily("Bahnschrift");
                titreJour.FontWeight = System.Windows.FontWeights.Bold;
                titreJour.FontSize = 12;
                titreJour.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                titreJour.Margin = new System.Windows.Thickness(0, 0, 0, 1);

                panelJour.Children.Add(titreJour);

                foreach (PresseProduction presse in groupe)
                {
                    double production = GetProductionJour(presse, jour);
                    double objectifJour = presse.ObjectifSemaine / 7.0;

                    TextBlock ligne = new TextBlock();

                    ligne.Text =
                        presse.Reference.ToString("000000") + " - " +
                        presse.Machine.Replace("AUTO ", "A") + " : " +
                        production.ToString("0") + " / " +
                        objectifJour.ToString("0");

                    ligne.FontFamily = new System.Windows.Media.FontFamily("Bahnschrift");
                    ligne.FontSize = 10;
                    ligne.TextWrapping = System.Windows.TextWrapping.NoWrap;
                    ligne.Foreground = production >= objectifJour
                        ? System.Windows.Media.Brushes.SeaGreen
                        : System.Windows.Media.Brushes.IndianRed;

                    ligne.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

                    panelJour.Children.Add(ligne);
                }
                Grid.SetColumn(panelJour, jour);
                legend.Children.Add(panelJour);
            }
        }

        private double GetProductionJour(PresseProduction presse, int jour)
        {
            if (jour == 0) return presse.ProdLundi;
            if (jour == 1) return presse.ProdMardi;
            if (jour == 2) return presse.ProdMercredi;
            if (jour == 3) return presse.ProdJeudi;
            if (jour == 4) return presse.ProdVendredi;
            if (jour == 5) return presse.ProdSamedi;
            if (jour == 6) return presse.ProdDimanche;

            return 0;
        }

        private void BtnGoToPage2Presse_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window fenetre =
      Window.GetWindow(this);

            if (fenetre is MainWindow mainWindow)
            {
                mainWindow.MainContent.Children.Clear();

                mainWindow.MainContent.Children.Add(
                    new SuiviPresse2());
            }
        }
    }
}