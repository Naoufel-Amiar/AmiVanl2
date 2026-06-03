using AmiVanl2.Model;
using AmiVanl2.Service;
using System;
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

            try
            {
                AppData.AssManuels =
                    await assManuelService
                        .LireAssManuelsAsync(AppData.ExcelFilePath, "Suivi ASS EV");
            }
            catch (Exception ex) when (ex.Message.Contains("introuvable"))
            {
                AppData.AssManuels = new System.Collections.Generic.List<AmiVanl2.Model.AssManuelProduction>();
            }

            try
            {
                AppData.TigesPoussee =
                    await assManuelService
                        .LireAssManuelsAsync(AppData.ExcelFilePath, "Suivi ASS Tige de poussée");
            }
            catch (Exception ex) when (ex.Message.Contains("introuvable"))
            {
                AppData.TigesPoussee = new System.Collections.Generic.List<AmiVanl2.Model.AssManuelProduction>();
            }

            return (AppData.AssManuels.Count, AppData.TigesPoussee.Count);
        }
    }
}