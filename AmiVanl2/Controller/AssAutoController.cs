using AmiVanl2.Model;
using AmiVanl2.Service;
using System;
using System.Threading.Tasks;

namespace AmiVanl2.Controller
{
    public class AssAutoController
    {
        private AssAutoService assAutoService;

        public AssAutoController()
        {
            assAutoService = new AssAutoService();
        }

        public async Task<int> ChargerAssAutoAsync()
        {
            if (string.IsNullOrWhiteSpace(AppData.ExcelFilePath))
            {
                throw new Exception("Aucun fichier Excel importé.");
            }

            AppData.AssAutos = await assAutoService.LireAssAutoAsync(AppData.ExcelFilePath);

            return AppData.AssAutos.Count;
        }
    }
}
