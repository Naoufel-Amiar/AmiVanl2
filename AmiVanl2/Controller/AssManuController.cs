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

        public async Task<int> ChargerAssManuelsAsync()
        {
            if (string.IsNullOrWhiteSpace(AppData.ExcelFilePath))
            {
                throw new Exception("Aucun fichier Excel importé.");
            }

            AppData.AssManuels.Clear();

            AppData.AssManuels =
                await assManuelService
                    .LireAssManuelsAsync(AppData.ExcelFilePath);

            return AppData.AssManuels.Count;
        }
    }
}