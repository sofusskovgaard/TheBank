using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace TheBank.Core.Utilities
{
    public static class LoggerService
    {
        static LoggerService()
        {
            if (!File.Exists(FullFilePath))
            {
                File.CreateText(FullFilePath);
            }
        }

        private static string FullFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs.txt");

        public static void Write(string message)
        {
            using (StreamWriter writer = File.AppendText(FullFilePath))
            {
                writer.WriteLine($"[{DateTime.Now.ToString()}]\t{message}");
            }
        }

        public static List<string> Read()
        {
            List<string> _logs = new List<string>();
            
            using (StreamReader reader = File.OpenText(FullFilePath))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    _logs.Add(str);
                }
            }

            return _logs;
        }

        public static void Clear()
        {
            File.Delete(FullFilePath);
        }
    }
}