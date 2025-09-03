using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomClc.Models.Cable;

namespace UncomClc.Service
{
    public static class ExcelReader
    {
        public static List<CableModel> ReadCableDataFromExcel(string sheetName)
        {
            var cableData = new List<CableModel>();
            string filePath = Path.Combine("..", "..", "Catalog.xlsx");
            // Проверка существования файла
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Excel файл не найден: {filePath}");
            }

            // Установка лицензии EPPlus (необходима для некоммерческого использования)
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // Проверка существования листа
                if (package.Workbook.Worksheets[sheetName] == null)
                {
                    throw new ArgumentException($"Лист с именем '{sheetName}' не найден в файле");
                }

                var worksheet = package.Workbook.Worksheets[sheetName];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                // Начинаем со второй строки (пропускаем заголовки)
                for (int row = 3; row <= rowCount; row++)
                {
                    // Проверяем, есть ли данные в первой колонке (чтобы не обрабатывать пустые строки)
                    if (worksheet.Cells[row, 1].Value == null)
                        continue;

                    var cable = new CableModel
                    {
                        RowNumber = int.Parse(worksheet.Cells[row, 1].Value?.ToString()),
                        Mark = worksheet.Cells[row, 2].Value?.ToString(),
                        Cross = worksheet.Cells[row, 3].Value?.ToString(),
                        Resistance = double.Parse(worksheet.Cells[row, 4].Value?.ToString()),
                        Alfa = worksheet.Cells[row, 5].Value?.ToString(),
                        Delta = worksheet.Cells[row, 6].Value?.ToString(),
                        Length = worksheet.Cells[row, 7].Value?.ToString(),
                        Dkab = decimal.Parse(worksheet.Cells[row, 13].Value?.ToString())
                    };

                    cableData.Add(cable);
                }
            }

            return cableData;
        }

    }
}
