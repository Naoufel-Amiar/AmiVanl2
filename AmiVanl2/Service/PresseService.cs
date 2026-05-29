using AmiVanl2.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace AmiVanl2.Service
{
    public class PresseService
    {
        public async Task<List<PresseProduction>> LirePresseAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                List<PresseProduction> resultats =
                    new List<PresseProduction>();

                ExcelPackage.License
                    .SetNonCommercialPersonal(
                        "Naoufel Amiar");

                FileInfo fichier =
                    new FileInfo(filePath);

                using (ExcelPackage package =
                    new ExcelPackage(fichier))
                {
                    ExcelWorksheet feuille =
                        package.Workbook.Worksheets["Suivi PRESSES"];

                    if (feuille == null)
                    {
                        throw new Exception(
                            "La feuille 'Suivi PRESSES' est introuvable.");
                    }

                    if (feuille.Dimension == null)
                    {
                        return resultats;
                    }

                    int ligneDebut = 7;

                    int ligneFin = feuille.Dimension.End.Row;

                    string labelLundi = LireDateExcel(feuille, 4, 7);
                    string labelMardi = LireDateExcel(feuille, 4, 8);
                    string labelMercredi = LireDateExcel(feuille, 4, 9);
                    string labelJeudi = LireDateExcel(feuille, 4, 10);
                    string labelVendredi = LireDateExcel(feuille, 4, 11);
                    string labelSamedi = LireDateExcel(feuille, 4, 12);
                    string labelDimanche = LireDateExcel(feuille, 4, 13);

                    for (int ligne = ligneDebut; ligne <= ligneFin; ligne++)
                    {
                        string referenceTexte =
                            LireTexte(feuille, ligne, 1);
                        if (string.IsNullOrWhiteSpace(referenceTexte))
                        {
                            continue;
                        }

                        PresseProduction presse = new PresseProduction();

                        presse.LabelLundi = labelLundi;
                        presse.LabelMardi = labelMardi;
                        presse.LabelMercredi = labelMercredi;
                        presse.LabelJeudi = labelJeudi;
                        presse.LabelVendredi = labelVendredi;
                        presse.LabelSamedi = labelSamedi;
                        presse.LabelDimanche = labelDimanche;
                        presse.Reference = LireInt(feuille, ligne, 1);
                        presse.AncienCode = LireTexte(feuille, ligne, 2);
                        presse.Equipe = LireTexte(feuille, ligne, 3);
                        presse.Machine = LireTexte(feuille, ligne, 4);
                        presse.ObjectifSemaine = LireDouble(feuille, ligne, 5);
                        presse.ObjectifEquipe = LireDouble(feuille, ligne, 6);

                        presse.ProdLundi = LireDouble(feuille, ligne, 7);
                        presse.ProdMardi = LireDouble(feuille, ligne, 8);
                        presse.ProdMercredi = LireDouble(feuille, ligne, 9);
                        presse.ProdJeudi = LireDouble(feuille, ligne, 10);
                        presse.ProdVendredi = LireDouble(feuille, ligne, 11);
                        presse.ProdSamedi = LireDouble(feuille, ligne, 12);
                        presse.ProdDimanche = LireDouble(feuille, ligne, 13);

                        presse.Commentaire = LireTexte(feuille, ligne, 14);


                        resultats.Add(presse);
                    }
                }

                return resultats;
            });
        }
        private string LireTexte(ExcelWorksheet feuille, int ligne, int colonne)
        {
            object valeur = feuille.Cells[ligne, colonne].Value;

            return valeur == null ? "" : valeur.ToString();
        }

        private string LireDateExcel(ExcelWorksheet feuille, int ligne, int colonne)
        {
            object valeur = feuille.Cells[ligne, colonne].Value;

            if (valeur == null)
                return "";

            if (valeur is double nombre)
            {
                DateTime date = DateTime.FromOADate(nombre);
                return date.ToString("ddd dd/MM");
            }

            if (DateTime.TryParse(valeur.ToString(), out DateTime dateTexte))
            {
                return dateTexte.ToString("ddd dd/MM");
            }

            return valeur.ToString();
        }

        private int LireInt(ExcelWorksheet feuille, int ligne, int colonne)
        {
            object valeur = feuille.Cells[ligne, colonne].Value;

            if (valeur == null)
                return 0;

            int resultat;

            if (int.TryParse(valeur.ToString(), out resultat))
            {
                return resultat;
            }

            return 0;
        }

        private double LireDouble(ExcelWorksheet feuille, int ligne, int colonne)
        {
            object valeur = feuille.Cells[ligne, colonne].Value;

            if (valeur == null)
                return 0;

            string texte = valeur.ToString().Replace(",", ".");

            double resultat;

            if (double.TryParse(texte, NumberStyles.Any, CultureInfo.InvariantCulture, out resultat))
            {
                return resultat;
            }

            return 0;
        }
    }
}