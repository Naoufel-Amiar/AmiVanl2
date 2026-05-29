using AmiVanl2.Service;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AmiVanl2.View
{
    public partial class ExportPDF : UserControl
    {
        public ExportPDF()
        {
            InitializeComponent();
        }

        private void BtnPdfGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Fichier PDF (*.pdf)|*.pdf";
                dialog.FileName = "Rapport_EV_Tracking.pdf";

                if (dialog.ShowDialog() != true)
                    return;

                List<FrameworkElement> pages = new List<FrameworkElement>();

                pages.Add(new SuiviPresse());

                PdfExportService pdfService = new PdfExportService();
                pdfService.ExporterPages(pages, dialog.FileName);

                MessageBox.Show(
                    "PDF généré avec succès.",
                    "Export PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erreur pendant la génération du PDF :\n" + ex.Message,
                    "Erreur PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}