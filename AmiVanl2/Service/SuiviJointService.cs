using AmiVanl2.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace AmiVanl2.Service
{
    public class SuiviJointService
    {
        public async Task<List<JointProduction>> LireJointsAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                List<JointProduction> resultats =
                    new List<JointProduction>();

                ExcelPackage.License
                    .SetNonCommercialPersonal("Naoufel Amiar");

                FileInfo fichier =
                    new FileInfo(filePath);

                using (ExcelPackage package = new ExcelPackage(fichier))
                {
                    ExcelWorksheet feuille =
                        TrouverFeuille(package, "Suivi Joint");

                    if (feuille == null)
                    {
                        throw new Exception(
                            "La feuille 'Suivi JOINT' est introuvable.");
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

                        JointProduction joint =
                            new JointProduction();

                        joint.LabelLundi = labelLundi;
                        joint.LabelMardi = labelMardi;
                        joint.LabelMercredi = labelMercredi;
                        joint.LabelJeudi = labelJeudi;
                        joint.LabelVendredi = labelVendredi;
                        joint.LabelSamedi = labelSamedi;
                        joint.LabelDimanche = labelDimanche;

                        joint.Reference = referenceTexte;
                        joint.AncienCode = LireTexte(feuille, ligne, 2);
                        joint.Equipe = LireTexte(feuille, ligne, 3);

                        joint.ObjectifSemaine = LireDouble(feuille, ligne, 4);
                        joint.ObjectifEquipe = LireDouble(feuille, ligne, 5);

                        joint.LundiEqu1 = LireDouble(feuille, ligne, 6);
                        joint.LundiEqu2 = LireDouble(feuille, ligne, 7);
                        joint.LundiEqu3 = LireDouble(feuille, ligne, 8);

                        joint.MardiEqu1 = LireDouble(feuille, ligne, 9);
                        joint.MardiEqu2 = LireDouble(feuille, ligne, 10);
                        joint.MardiEqu3 = LireDouble(feuille, ligne, 11);

                        joint.MercrediEqu1 = LireDouble(feuille, ligne, 12);
                        joint.MercrediEqu2 = LireDouble(feuille, ligne, 13);
                        joint.MercrediEqu3 = LireDouble(feuille, ligne, 14);

                        joint.JeudiEqu1 = LireDouble(feuille, ligne, 15);
                        joint.JeudiEqu2 = LireDouble(feuille, ligne, 16);
                        joint.JeudiEqu3 = LireDouble(feuille, ligne, 17);

                        joint.VendrediEqu1 = LireDouble(feuille, ligne, 18);
                        joint.VendrediEqu2 = LireDouble(feuille, ligne, 19);
                        joint.VendrediEqu3 = LireDouble(feuille, ligne, 20);

                        joint.ProdSamedi = LireDouble(feuille, ligne, 21);
                        joint.ProdDimanche = LireDouble(feuille, ligne, 22);

                        joint.Commentaire = LireTexte(feuille, ligne, 23);

                        resultats.Add(joint);
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