using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
namespace LibraryWPF
{
    
    public class ConfigReader
    {
        private readonly Dictionary<string, string> _configValues;

        public ConfigReader(string filePath)
        {
            _configValues = new Dictionary<string, string>();
            LoadConfig(filePath);
        }

        private void LoadConfig(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Configuration file not found: " + filePath);
            }

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (!string.IsNullOrWhiteSpace(line) && line.Contains("="))
                {
                    var parts = line.Split('=', 2);
                    _configValues[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }

        public string GetValue(string key)
        {
            return _configValues.ContainsKey(key) ? _configValues[key] : null;
        }
    }

}
