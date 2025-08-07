using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using UncomClc.Models;

namespace UncomClc.Data
{
    public class UploadedData
    {
        private static UploadedData instance;
        private const string DataPath = @"Data\data.json";
        public List<Pipe> Pipes { get; private set; }

        public UploadedData()
        {
            Load();
        }

        public static UploadedData Instance
        {
            get
            {
                if(instance == null) instance = new UploadedData();
                return instance;
            }
        }


        private void Load()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DataPath);
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                var emptyData = new Db { Pipes = new List<Pipe>() };
                string newjson = JsonSerializer.Serialize(emptyData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, newjson);
            }
            var json = File.ReadAllText(path);
            var configData = JsonSerializer.Deserialize<Db>(json);

            string jsonContent = File.ReadAllText(path);
            var loadedData = JsonSerializer.Deserialize<Db>(jsonContent);
            Pipes = loadedData?.Pipes ?? new List<Pipe>();
        }

        public void Save()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DataPath);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var db = new Db { Pipes = this.Pipes };
            string json = JsonSerializer.Serialize(db, options);
            File.WriteAllText(path, json);
        }

        private class Db
        {
            public List<Pipe> Pipes { get; set; }
        }
    }

}
