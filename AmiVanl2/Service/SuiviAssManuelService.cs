using AmiVanl2.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace AmiVanl2.Service
{
    public class SuiviAssManuelService
    {
        public async Task<List<AssManuelProduction>> LireAssManuelsAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                List<AssManuelProduction> resultats =
                    new List<AssManuelProduction>();

                ExcelPackage.License
                    .SetNonCommercialPersonal("Naoufel Amiar");

                FileInfo fichier =
                    new FileInfo(filePath);

                using (ExcelPackage package = new ExcelPackage(fichier))
                {
                    ExcelWorksheet feuille =
                        TrouverFeuille(package, "Suivi ASS manuel");

                    if (feuille == null)
                    {
                        throw new Exception(
                            "La feuille 'Suivi ASS manuel' est introuvable.");
                    }

                    if (feuille.Dimension == null)
                    {
                        return resultats;
                    }

                    int ligneDebut = 7;
                    int ligneFin = feuille.Dimension.End.Row;

                    string labelLundi = LireDateExcel(feuille, 4, 6);
                    string labelMardi = LireDateExcel(feuille, 4, 9);
                    string labelMercredi = LireDateExcel(feuille, 4, 12);
                    string labelJeudi = LireDateExcel(feuille, 4, 15);
                    string labelVendredi = LireDateExcel(feuille, 4, 18);
                    string labelSamedi = LireDateExcel(feuille, 4, 21);
                    string labelDimanche = LireDateExcel(feuille, 4, 22);

                    string referenceCourante = "";
                    string equipeCourante = "";

                    int lignesVidesConsecutives = 0;

                    for (int ligne = ligneDebut; ligne <= ligneFin; ligne++)
                    {
                        string referenceTexte =
                            LireReference(feuille, ligne, 1);

                        string equipeTexte =
                            LireTexte(feuille, ligne, 2);

                        string operationTexte =
                            LireTexte(feuille, ligne, 3);

                        if (!string.IsNullOrWhiteSpace(referenceTexte))
                        {
                            referenceCourante = referenceTexte;
                        }

                        if (!string.IsNullOrWhiteSpace(equipeTexte))
                        {
                            equipeCourante = equipeTexte;
                        }

                        if (string.IsNullOrWhiteSpace(operationTexte))
                        {
                            lignesVidesConsecutives++;

                            if (lignesVidesConsecutives >= 6)
                            {
                                break;
                            }

                            continue;
                        }

                        if (!EstReferenceValide(referenceCourante))
                        {
                            continue;
                        }

                        lignesVidesConsecutives = 0;

                        AssManuelProduction assManuel =
                            new AssManuelProduction();

                        assManuel.LabelLundi = labelLundi;
                        assManuel.LabelMardi = labelMardi;
                        assManuel.LabelMercredi = labelMercredi;
                        assManuel.LabelJeudi = labelJeudi;
                        assManuel.LabelVendredi = labelVendredi;
                        assManuel.LabelSamedi = labelSamedi;
                        assManuel.LabelDimanche = labelDimanche;

                        assManuel.Reference = referenceCourante;
                        assManuel.Equipe = equipeCourante;
                        assManuel.Operation = operationTexte;

                        assManuel.ObjectifSemaine = LireDouble(feuille, ligne, 4);
                        assManuel.ObjectifEquipe = LireDouble(feuille, ligne, 5);

                        assManuel.LundiEqu1 = LireDouble(feuille, ligne, 6);
                        assManuel.LundiEqu2 = LireDouble(feuille, ligne, 7);
                        assManuel.LundiEqu3 = LireDouble(feuille, ligne, 8);

                        assManuel.MardiEqu1 = LireDouble(feuille, ligne, 9);
                        assManuel.MardiEqu2 = LireDouble(feuille, ligne, 10);
                        assManuel.MardiEqu3 = LireDouble(feuille, ligne, 11);

                        assManuel.MercrediEqu1 = LireDouble(feuille, ligne, 12);
                        assManuel.MercrediEqu2 = LireDouble(feuille, ligne, 13);
                        assManuel.MercrediEqu3 = LireDouble(feuille, ligne, 14);

                        assManuel.JeudiEqu1 = LireDouble(feuille, ligne, 15);
                        assManuel.JeudiEqu2 = LireDouble(feuille, ligne, 16);
                        assManuel.JeudiEqu3 = LireDouble(feuille, ligne, 17);

                        assManuel.VendrediEqu1 = LireDouble(feuille, ligne, 18);
                        assManuel.VendrediEqu2 = LireDouble(feuille, ligne, 19);
                        assManuel.VendrediEqu3 = LireDouble(feuille, ligne, 20);

                        assManuel.ProdSamedi = LireDouble(feuille, ligne, 21);
                        assManuel.ProdDimanche = LireDouble(feuille, ligne, 22);

                        assManuel.Commentaire = LireTexte(feuille, ligne, 23);

                        resultats.Add(assManuel);
                    }
                }

                return resultats;
            });
        }

        private ExcelWorksheet TrouverFeuille(ExcelPackage package, string nomFeuille)
        {
            foreach (ExcelWorksheet feuille in package.Workbook.Worksheets)
            {
                if (NormaliserNom(feuille.Name) == NormaliserNom(nomFeuille))
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

        private string LireReference(ExcelWorksheet feuille, int ligne, int colonne)
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

            if (int.TryParse(valeur.ToString(), out int referenceNumerique))
            {
                return referenceNumerique.ToString("000000");
            }

            return valeur.ToString().Trim();
        }

        private string LireTexte(ExcelWorksheet feuille, int ligne, int colonne)
        {
            object valeur =
                feuille.Cells[ligne, colonne].Value;

            return valeur == null
                ? ""
                : valeur.ToString().Trim();
        }

        private string LireDateExcel(ExcelWorksheet feuille, int ligne, int colonne)
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

            if (DateTime.TryParse(valeur.ToString(), out DateTime dateTexte))
            {
                return dateTexte.ToString("ddd dd/MM");
            }

            return valeur.ToString();
        }

        private double LireDouble(ExcelWorksheet feuille, int ligne, int colonne)
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

            if (double.TryParse(
                    texte,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out double resultat))
            {
                return resultat;
            }

            return 0;
        }
    }
}