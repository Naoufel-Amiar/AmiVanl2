using System;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using AmiVanl2.Controller;
using AmiVanl2.Model;

namespace AmiVanl2.View
{
    /// <summary>
    /// Logique d'interaction pour AccImport.xaml
    /// </summary>
    public partial class AccImport : UserControl
    {
        public AccImport()
        {
            InitializeComponent();
        }

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                DropZone.BorderBrush = Brushes.Green;
                TxtDropMessage.Text = "Relâche pour importer";
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            DropZone.BorderBrush = Brushes.Gray;
            TxtDropMessage.Text = "Glisse ton fichier Excel ici";
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            DropZone.BorderBrush = Brushes.Gray;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length == 0)
                return;

            LoadFile(files[0]);
        }

        private void BtnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Sélectionner un fichier Excel";
            dialog.Filter = "Fichiers Excel (*.xlsx;*.xls)|*.xlsx;*.xls";

            if (dialog.ShowDialog() == true)
            {
                LoadFile(dialog.FileName);
            }
        }

        private void LoadFile(string filePath)
        {
            string extension =
                System.IO.Path
                .GetExtension(filePath)
                .ToLower();

            if (extension != ".xlsx"
                && extension != ".xls")
            {
                MessageBox.Show(
                    "Format refusé. Importe un fichier Excel."
                );

                return;
            }

            AppData.Reset();

            AppData.ExcelFilePath = filePath;

            MainWindow fenetre = Application.Current.MainWindow as MainWindow;

            fenetre?.MettreAJourNavigation();

            TxtSelectedFile.Text =
                System.IO.Path.GetFileName(filePath);

            TxtDropMessage.Text =
                "Fichier Excel chargé";

            DropZone.BorderBrush =
                Brushes.Green;
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!AppData.HasExcelFile())
                {
                    MessageBox.Show(
                        "Importe un fichier Excel avant."
                    );

                    return;
                }

                BtnGenerate.IsEnabled = false;
                PanelChargement.Visibility = Visibility.Visible;

                PresseController presseController =
                    new PresseController();

                AssAutoController assAutoController =
                    new AssAutoController();

                JointController jointController =
                    new JointController();

                int nb =
                    await presseController.ChargerPresseAsync();

                int nbAssAuto =
                    await assAutoController.ChargerAssAutoAsync();

                int nbJoints =
                    await jointController.ChargerJointsAsync();

                TriController triController =
                    new TriController();

                AssManuelController assManuelController =
                    new AssManuelController();

                int nbTris =
                    await triController.ChargerTrisAsync();

                int nbAssManu =
                    await assManuelController.ChargerAssManuelsAsync();

                AppData.DonneesGenerees = true;

                MainWindow fenetre =
                    Application.Current.MainWindow as MainWindow;

                fenetre?.MettreAJourNavigation();

                int refPresseAvecProd = AppData.Presses
                    .GroupBy(p => p.Reference)
                    .Count(g => g.Sum(p => p.TotalProduction) > 0);

                int refAssAutoAvecProd = AppData.AssAutos
                    .GroupBy(p => p.Reference)
                    .Count(g => g.Sum(p => p.TotalProduction) > 0);

                int refJointsAvecProd = AppData.Joints
                    .Count(p => p.TotalProduction > 0);

                int refTrisAvecProd = AppData.Tris
                    .Count(p => p.TotalProduction > 0);

                int refAssManuAvecProd = AppData.AssManuels
                    .GroupBy(p => p.Reference)
                    .Count(g => g.Any(p =>
                        p.LundiEqu1 + p.LundiEqu2 + p.LundiEqu3 +
                        p.MardiEqu1 + p.MardiEqu2 + p.MardiEqu3 +
                        p.MercrediEqu1 + p.MercrediEqu2 + p.MercrediEqu3 +
                        p.JeudiEqu1 + p.JeudiEqu2 + p.JeudiEqu3 +
                        p.VendrediEqu1 + p.VendrediEqu2 + p.VendrediEqu3 > 0));

                MessageBox.Show(
                    "✔ Données chargées avec succès !\n\n" +
                    "PRESSE        : " + refPresseAvecProd + " réf. en production  (" + nb + " lignes lues)\n" +
                    "ASS. AUTO  : " + refAssAutoAvecProd + " réf. en production  (" + nbAssAuto + " lignes lues)\n" +
                    "JOINTS         : " + refJointsAvecProd + " réf. en production  (" + nbJoints + " lignes lues)\n" +
                    "TRI                : " + refTrisAvecProd + " réf. en production  (" + nbTris + " lignes lues)\n" +
                    "ASS. MANU : " + refAssManuAvecProd + " réf. en production  (" + nbAssManu + " lignes lues)"
                );
            }

            catch (Exception ex)
            {
                string msg;
                if (ex.Message.Contains("used by another process") || ex.Message.Contains("en cours d'utilisation"))
                    msg = "Impossible de lire le fichier.\nFermez-le dans Excel puis réessayez.";
                else if (ex.Message.Contains("introuvable"))
                    msg = ex.Message + "\n\nVérifiez que le fichier Excel correspond bien au format attendu.";
                else
                    msg = "Une erreur est survenue lors de l'import :\n\n" + ex.Message;

                MessageBox.Show(msg, "Erreur d'import", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnGenerate.IsEnabled = true;
                PanelChargement.Visibility = Visibility.Collapsed;
            }
        }
    }
}
