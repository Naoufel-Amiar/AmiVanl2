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
        private string selectedFilePath = "";
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

            selectedFilePath = filePath;

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

        private void TxtObjectifProduction_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                TxtObjectifProduction.Text))
            {
                PlaceholderObjectif.Visibility =
                    Visibility.Visible;
            }
            else
            {
                PlaceholderObjectif.Visibility =
                    Visibility.Collapsed;
            }
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

                //TEST DE VERIFICATION DES DONNEES RECUPEREES////////////////////////////////////////////////
                if (AppData.Presses.Count > 0)
                {
                    PresseProduction premiereLigne = AppData.Presses[0];

                    MessageBox.Show(
                        "Première ligne récupérée :\n\n" +
                        "Référence : " + premiereLigne.Reference + "\n" +
                        "Ancien code : " + premiereLigne.AncienCode + "\n" +
                        "Equipe : " + premiereLigne.Equipe + "\n" +
                        "Machine : " + premiereLigne.Machine + "\n" +
                        "Obj semaine : " + premiereLigne.ObjectifSemaine + "\n" +
                        "Lundi : " + premiereLigne.ProdLundi + "\n" +
                        "Mardi : " + premiereLigne.ProdMardi + "\n" +
                        "Mercredi : " + premiereLigne.ProdMercredi + "\n" +
                        "Jeudi : " + premiereLigne.ProdJeudi + "\n" +
                        "Vendredi : " + premiereLigne.ProdVendredi + "\n" +
                        "Samedi : " + premiereLigne.ProdSamedi + "\n" +
                        "Dimanche : " + premiereLigne.ProdDimanche + "\n" +
                        "Total : " + premiereLigne.TotalProduction
                    );
                    
                }

                AppData.DonneesGenerees = true;

                MainWindow fenetre =
                    Application.Current.MainWindow as MainWindow;

                fenetre?.MettreAJourNavigation();

                MessageBox.Show(
                    nb +
                    " lignes presse analysées.\n" +

                    nbAssAuto +
                    " lignes assemblage automatique analysées.\n" +

                    nbJoints +
                    " lignes joints analysées.\n" +

                    nbTris +
                    " lignes tri analysées.\n" +

                    nbAssManu +
                    " lignes assemblage manuel analysées."
                );
                //////////////////////////////////////////////////////////////////////////////////////////
            }

            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message
                );
            }
        }
    }
}
