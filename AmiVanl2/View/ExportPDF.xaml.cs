using AmiVanl2.Model;
using AmiVanl2.Service;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AmiVanl2.View
{
    public partial class ExportPDF : UserControl
    {
        public ExportPDF()
        {
            InitializeComponent();
            Loaded += ExportPDF_Loaded;
        }

        private void ExportPDF_Loaded(object sender, RoutedEventArgs e)
        {
            ChargerListeSections();
        }

        private void ChargerListeSections()
        {
            var sections = new List<SectionPdfItem>
            {
                new SectionPdfItem
                {
                    Titre         = "Suivi Presse",
                    Fond          = new SolidColorBrush(Color.FromArgb(80, 206, 195, 193)),
                    NbElements    = AppData.Presses?.Count > 0
                        ? AppData.Presses.Count + " ligne(s) — 2 pages (barres + camemberts)"
                        : "",
                    Statut        = AppData.Presses?.Count > 0 ? "✓ Inclus" : "⚠ Aucune donnée",
                    CouleurStatut = AppData.Presses?.Count > 0
                        ? new SolidColorBrush(Color.FromRgb(30, 120, 30))
                        : new SolidColorBrush(Color.FromRgb(180, 60, 0))
                },
                new SectionPdfItem
                {
                    Titre         = "Assemblage Automatique",
                    Fond          = new SolidColorBrush(Color.FromArgb(80, 246, 225, 207)),
                    NbElements    = AppData.AssAutos?.Count > 0
                        ? AppData.AssAutos.Count + " ligne(s) — 2 pages (barres + camemberts)"
                        : "",
                    Statut        = AppData.AssAutos?.Count > 0 ? "✓ Inclus" : "⚠ Aucune donnée",
                    CouleurStatut = AppData.AssAutos?.Count > 0
                        ? new SolidColorBrush(Color.FromRgb(30, 120, 30))
                        : new SolidColorBrush(Color.FromRgb(180, 60, 0))
                },
                new SectionPdfItem
                {
                    Titre         = "Joints",
                    Fond          = new SolidColorBrush(Color.FromArgb(80, 195, 228, 195)),
                    NbElements    = AppData.Joints?.Count > 0
                        ? AppData.Joints.Count + " référence(s) — tableau compact coloré"
                        : "",
                    Statut        = AppData.Joints?.Count > 0 ? "✓ Inclus" : "⚠ Aucune donnée",
                    CouleurStatut = AppData.Joints?.Count > 0
                        ? new SolidColorBrush(Color.FromRgb(30, 120, 30))
                        : new SolidColorBrush(Color.FromRgb(180, 60, 0))
                },
                new SectionPdfItem
                {
                    Titre         = "Tri",
                    Fond          = new SolidColorBrush(Color.FromArgb(80, 195, 215, 235)),
                    NbElements    = AppData.Tris?.Count > 0
                        ? AppData.Tris.Count + " référence(s) — tableau compact coloré"
                        : "",
                    Statut        = AppData.Tris?.Count > 0 ? "✓ Inclus" : "⚠ Aucune donnée",
                    CouleurStatut = AppData.Tris?.Count > 0
                        ? new SolidColorBrush(Color.FromRgb(30, 120, 30))
                        : new SolidColorBrush(Color.FromRgb(180, 60, 0))
                },
                new SectionPdfItem
                {
                    Titre         = "Assemblage Manuel",
                    Fond          = new SolidColorBrush(Color.FromArgb(80, 230, 210, 235)),
                    NbElements    = AppData.AssManuels?.Count > 0
                        ? AppData.AssManuels.Count + " ligne(s) — courbes Capuchon / Insert / Objectif"
                        : "",
                    Statut        = AppData.AssManuels?.Count > 0 ? "✓ Inclus" : "⚠ Aucune donnée",
                    CouleurStatut = AppData.AssManuels?.Count > 0
                        ? new SolidColorBrush(Color.FromRgb(30, 120, 30))
                        : new SolidColorBrush(Color.FromRgb(180, 60, 0))
                }
            };

            ListeSections.ItemsSource = sections;
        }

        private void BtnPdfGenerate_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter   = "Fichier PDF (*.pdf)|*.pdf",
                FileName = "Rapport_Suivi_Production_" + DateTime.Now.ToString("yyyy-MM-dd") + ".pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            string cheminPdf = dialog.FileName;

            BtnPdfGenerate.IsEnabled = false;
            TxtStatut.Text = "Génération en cours… Veuillez patienter.";

            // Force l'affichage du message avant de bloquer le thread UI
            Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

            // PngExporter d'OxyPlot.Wpf requiert le thread STA (thread UI).
            // On lance un thread STA dédié et on dispatche le résultat sur l'UI.
            var dispatcher = Dispatcher;
            var thread = new Thread(() =>
            {
                Exception erreur = null;
                try
                {
                    var service = new PdfLayoutService();
                    service.GenererRapport(cheminPdf);
                }
                catch (Exception ex)
                {
                    erreur = ex;
                }

                dispatcher.Invoke(() =>
                {
                    BtnPdfGenerate.IsEnabled = true;
                    TxtStatut.Text = "";

                    if (erreur == null)
                    {
                        MessageBox.Show(
                            "Le rapport PDF a été généré avec succès.\n\n" + cheminPdf,
                            "Export PDF — Succès",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Erreur lors de la génération du PDF :\n\n" + erreur.Message,
                            "Erreur PDF",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                });
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }
    }

    public class SectionPdfItem
    {
        public string Titre         { get; set; }
        public Brush  Fond          { get; set; }
        public string NbElements    { get; set; }
        public string Statut        { get; set; }
        public Brush  CouleurStatut { get; set; }
    }
}
