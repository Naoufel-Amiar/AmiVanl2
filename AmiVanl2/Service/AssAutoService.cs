using AmiVanl2.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace AmiVanl2.Service
{
    public class AssAutoService
    {
        public async Task<List<AssAutoProduction>> LireAssAutoAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                List<AssAutoProduction> resultats =
                    new List<AssAutoProduction>();

                ExcelPackage.License
                    .SetNonCommercialPersonal("Naoufel Amiar");

                FileInfo fichier =
                    new FileInfo(filePath);

                using (ExcelPackage package = new ExcelPackage(fichier))
                {
                    ExcelWorksheet feuille =
                        TrouverFeuille(package, "Suivi ASS AUTO");

                    if (feuille == null)
                    {
                        throw new Exception(
                            "La feuille 'Suivi ASS AUTO' est introuvable.");
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

                    int lignesVidesConsecutives = 0;

                    for (int ligne = ligneDebut; ligne <= ligneFin; ligne++)
                    {
                        string referenceTexte =
                            LireReference(feuille, ligne, 1);

                        if (!EstReferenceValide(referenceTexte))
                        {
                            lignesVidesConsecutives++;

                            if (lignesVidesConsecutives >= 5)
                            {
                                break;
                            }

                            continue;
                        }

                        lignesVidesConsecutives = 0;

                        AssAutoProduction assAuto =
                            new AssAutoProduction();

                        assAuto.LabelLundi = labelLundi;
                        assAuto.LabelMardi = labelMardi;
                        assAuto.LabelMercredi = labelMercredi;
                        assAuto.LabelJeudi = labelJeudi;
                        assAuto.LabelVendredi = labelVendredi;
                        assAuto.LabelSamedi = labelSamedi;
                        assAuto.LabelDimanche = labelDimanche;

                        assAuto.Reference =
                            referenceTexte;

                        assAuto.AncienCode =
                            LireTexte(feuille, ligne, 2);

                        assAuto.Equipe =
                            LireTexte(feuille, ligne, 3);

                        assAuto.Machine =
                            LireTexte(feuille, ligne, 4);

                        assAuto.ObjectifSemaine =
                            LireDouble(feuille, ligne, 5);

                        assAuto.ObjectifJour =
                            LireDouble(feuille, ligne, 6);

                        assAuto.ProdLundi =
                            LireDouble(feuille, ligne, 7);

                        assAuto.ProdMardi =
                            LireDouble(feuille, ligne, 8);

                        assAuto.ProdMercredi =
                            LireDouble(feuille, ligne, 9);

                        assAuto.ProdJeudi =
                            LireDouble(feuille, ligne, 10);

                        assAuto.ProdVendredi =
                            LireDouble(feuille, ligne, 11);

                        assAuto.ProdSamedi =
                            LireDouble(feuille, ligne, 12);

                        assAuto.ProdDimanche =
                            LireDouble(feuille, ligne, 13);

                        assAuto.Commentaire =
                            LireTexte(feuille, ligne, 14);

                        resultats.Add(assAuto);
                    }
                }

                return resultats;
            });
        }

        private ExcelWorksheet TrouverFeuille(
            ExcelPackage package,
            string nomFeuille)
        {
            foreach (
                ExcelWorksheet feuille
                in package.Workbook.Worksheets)
            {
                if (
                    NormaliserNom(feuille.Name)
                    ==
                    NormaliserNom(nomFeuille)
                )
                {
                    return feuille;
                }
            }

            return null;
        }

        private string NormaliserNom(string texte)
        {
            return texte
                .Replace(" ", "")
                .Replace("_", "")
                .ToUpperInvariant();
        }

        private bool EstReferenceValide(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return false;
            }

            reference = reference.Trim();

            if (reference.Length != 6)
            {
                return false;
            }

            return int.TryParse(reference, out _);
        }

        private string LireReference(
            ExcelWorksheet feuille,
            int ligne,
            int colonne)
        {
            string texte =
                feuille.Cells[ligne, colonne].Text;

            if (!string.IsNullOrWhiteSpace(texte))
            {
                return texte.Trim();
            }

            object valeur =
                feuille.Cells[ligne, colonne].Value;

            if (valeur == null)
            {
                return "";
            }

            if (
                int.TryParse(
                    valeur.ToString(),
                    out int referenceNumerique)
            )
            {
                return
                    referenceNumerique.ToString("000000");
            }

            return valeur.ToString().Trim();
        }

        private string LireTexte(
            ExcelWorksheet feuille,
            int ligne,
            int colonne)
        {
            object valeur =
                feuille.Cells[ligne, colonne].Value;

            return valeur == null
                ? ""
                : valeur.ToString().Trim();
        }

        private string LireDateExcel(
            ExcelWorksheet feuille,
            int ligne,
            int colonne)
        {
            object valeur =
                feuille.Cells[ligne, colonne].Value;

            if (valeur == null)
            {
                return "";
            }

            if (valeur is double nombre)
            {
                DateTime date =
                    DateTime.FromOADate(nombre);

                return date.ToString("ddd dd/MM");
            }

            if (
                DateTime.TryParse(
                    valeur.ToString(),
                    out DateTime dateTexte)
            )
            {
                return
                    dateTexte.ToString("ddd dd/MM");
            }

            return valeur.ToString();
        }

        private double LireDouble(
            ExcelWorksheet feuille,
            int ligne,
            int colonne)
        {
            string texte =
                feuille.Cells[ligne, colonne].Text;

            if (string.IsNullOrWhiteSpace(texte))
            {
                object valeur =
                    feuille.Cells[ligne, colonne].Value;

                if (valeur == null)
                {
                    return 0;
                }

                texte = valeur.ToString();
            }

            texte = texte
                .Replace(" ", "")
                .Replace("\u00A0", "")
                .Replace(",", ".") 
                .Trim();

            if (
                double.TryParse(
                    texte,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double resultat)
            )
            {
                return resultat;
            }

            return 0;
        }
    }
}