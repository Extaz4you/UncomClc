using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using UncomClc.Models;
using UncomClc.Models.Insulations;

namespace UncomClc.Data
{
    public class UploadedData
    {
        private static UploadedData instance;
        public List<Pipe> Pipes { get; private set; }
        public List<Insulation> Insulations { get; private set; }

        public UploadedData()
        {
            Load();
        }

        public static UploadedData Instance
        {
            get
            {
                if (instance == null) instance = new UploadedData();
                return instance;
            }
        }


        private void Load()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var path = System.IO.Path.Combine(baseDirectory, "Data", "data.json");
            //var path = System.IO.Path.Combine(baseDirectory, "..", "..", "Data", "data.json");
            if (!File.Exists(path))
            {
                var emptyData = new Db { Pipes = new List<Pipe>() };
                string newjson = JsonSerializer.Serialize(emptyData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, newjson);
            }
            var json = File.ReadAllText(path);
            var configData = JsonSerializer.Deserialize<Db>(json);

            string jsonContent = File.ReadAllText(path);
            var loadedData = JsonSerializer.Deserialize<Db>(jsonContent);
            Pipes = loadedData?.Pipes ?? new List<Pipe>();
            Insulations = loadedData?.Insulations ?? new List<Insulation>();
        }

        public void Save()
        {
            var baseDirectory = GetAppDirectory();
            //var path = System.IO.Path.Combine(baseDirectory, "..", "..", "Data", "data.json");
            var path = System.IO.Path.Combine(baseDirectory, "Data", "data.json");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var db = new Db { Pipes = this.Pipes, Insulations = this.Insulations };
            string json = JsonSerializer.Serialize(db, options);
            File.WriteAllText(path, json);
        }

        private class Db
        {
            public List<Pipe> Pipes { get; set; }
            public List<Insulation> Insulations { get; set; }
        }
        public static string GetAppDirectory()
        {
            // Для Single File приложения
            if (AppContext.BaseDirectory.EndsWith(".exe"))
            {
                // Возвращаем директорию где находится exe
                return System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            }

            return AppDomain.CurrentDomain.BaseDirectory;
        }

    }
}

