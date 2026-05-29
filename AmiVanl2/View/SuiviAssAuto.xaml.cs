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
    public partial class SuiviAssAuto : UserControl
    {
        public SuiviAssAuto()
        {
            InitializeComponent();
            ChargerGraphiqueAssAuto();
        }

        private void ChargerGraphiqueAssAuto()
        {
            if (AppData.AssAutos == null || AppData.AssAutos.Count == 0)
                return;

            List<AssAutoProduction> groupe1 = AppData.AssAutos.Take(4).ToList();
            List<AssAutoProduction> groupe2 = AppData.AssAutos.Skip(4).Take(4).ToList();

            ChargerGraphiqueGroupe(groupe1, PlotAssAutoGroupe1, LegendAssAutoGroupe1, "Assemblage automatique — Groupe 1");
            ChargerGraphiqueGroupe(groupe2, PlotAssAutoGroupe2, LegendAssAutoGroupe2, "Assemblage automatique — Groupe 2");
        }

        private void ChargerGraphiqueGroupe(List<AssAutoProduction> groupe, OxyPlot.Wpf.PlotView plot, Grid legend, string titre)
        {
            if (groupe == null || groupe.Count == 0)
                return;

            AssAutoProduction premiereLigne = groupe[0];

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
                    AssAutoProduction assAuto = groupe[refIndex];

                    double production = GetProductionJour(assAuto, jour);
                    double objectifJour = assAuto.ObjectifJournalierCalcule;

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

        private void ChargerLegendeGroupe(List<AssAutoProduction> groupe, string[] labels, Grid legend)
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

                foreach (AssAutoProduction assAuto in groupe)
                {
                    double production = GetProductionJour(assAuto, jour);
                    double objectifJour = assAuto.ObjectifJournalierCalcule;

                    TextBlock ligne = new TextBlock();

                    ligne.Text =
                        assAuto.Reference + " - " +
                        assAuto.Machine + " : " +
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

        private double GetProductionJour(AssAutoProduction assAuto, int jour)
        {
            if (jour == 0) return assAuto.ProdLundi;
            if (jour == 1) return assAuto.ProdMardi;
            if (jour == 2) return assAuto.ProdMercredi;
            if (jour == 3) return assAuto.ProdJeudi;
            if (jour == 4) return assAuto.ProdVendredi;
            if (jour == 5) return assAuto.ProdSamedi;
            if (jour == 6) return assAuto.ProdDimanche;

            return 0;
        }

        private void BtnGoToPage2AssAuto_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window fenetre = Window.GetWindow(this);

            if (fenetre is MainWindow mainWindow)
            {
                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(new SuiviAssAuto2());
            }
        }
    }
}
