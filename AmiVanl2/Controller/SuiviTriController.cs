using AmiVanl2.Model;
using AmiVanl2.Service;
using System;
using System.Threading.Tasks;

namespace AmiVanl2.Controller
{
    public class TriController
    {
        private SuiviTriService triService;

        public TriController()
        {
            triService = new SuiviTriService();
        }

        public async Task<int> ChargerTrisAsync()
        {
            if (string.IsNullOrWhiteSpace(AppData.ExcelFilePath))
            {
                throw new Exception("Aucun fichier Excel importé.");
            }

            AppData.Tris.Clear();

            AppData.Tris =
                await triService.LireTrisAsync(AppData.ExcelFilePath);

            return AppData.Tris.Count;
        }
    }
}