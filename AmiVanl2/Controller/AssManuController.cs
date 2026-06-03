using AmiVanl2.Model;
using AmiVanl2.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmiVanl2.Controller
{
    public class AssManuelController
    {
        private SuiviAssManuelService assManuelService;

        public AssManuelController()
        {
            assManuelService =
                new SuiviAssManuelService();
        }

        public async Task<(int nbEV, int nbTige)> ChargerAssManuelsAsync()
        {
            if (string.IsNullOrWhiteSpace(AppData.ExcelFilePath))
            {
                throw new Exception("Aucun fichier Excel importé.");
            }

            AppData.AssManuels.Clear();
            AppData.TigesPoussee.Clear();

            var toutes = await assManuelService
                .LireAssManuelsAsync(AppData.ExcelFilePath, "Suivi ASS manuel");

            // Refs alphanumériques (contiennent une lettre) = EV
            // Refs numériques pures = Tige de poussée
            foreach (var ligne in toutes)
            {
                if (EstRefAlphanum(ligne.Reference))
                    AppData.AssManuels.Add(ligne);
                else
                    AppData.TigesPoussee.Add(ligne);
            }

            return (AppData.AssManuels.Count, AppData.TigesPoussee.Count);
        }

        private bool EstRefAlphanum(string reference)
        {
            foreach (char c in reference)
                if (char.IsLetter(c)) return true;
            return false;
        }
    }
}