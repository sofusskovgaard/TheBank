using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace TheBank.Services
{
    public static class LoggerService
    {
        private static string FullFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs.txt");
        
        private static readonly object _loggingLock = new object();
        
        static LoggerService()
        {
            if (!File.Exists(FullFilePath))
            {
                File.CreateText(FullFilePath).Dispose();
            }
        }

        /// <summary>
        /// Make an entry in the log
        /// </summary>
        /// <param name="message">Log message</param>
        public static void Write(string message)
        {
            lock (_loggingLock)
            {
                using (StreamWriter writer = File.AppendText(FullFilePath))
                {
                    writer.WriteLine($"[{Thread.CurrentThread.ManagedThreadId:00}][{DateTime.Now.ToString()}]\t{message}");
                }
            }
        }

        /// <summary>
        /// Read the log
        /// </summary>
        /// <returns>List&lt;string&gt;</returns>
        public static List<string> Read()
        {
            lock (_loggingLock)
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
        }

        /// <summary>
        /// Clear the log
        /// </summary>
        public static void Clear()
        {
            File.Delete(FullFilePath);
        }
    }
}