using AmiVanl2.Model;
using AmiVanl2.Service;
using System;
using System.Threading.Tasks;

namespace AmiVanl2.Controller
{
    public class PresseController
    {
        private PresseService presseService;

        public PresseController()
        {
            presseService = new PresseService();
        }

        public async Task<int> ChargerPresseAsync()
        {
            if (string.IsNullOrWhiteSpace(AppData.ExcelFilePath))
            {
                throw new Exception("Aucun fichier Excel importé.");
            }

            AppData.Presses = await presseService.LirePresseAsync(AppData.ExcelFilePath);

            return AppData.Presses.Count;
        }
    }
}