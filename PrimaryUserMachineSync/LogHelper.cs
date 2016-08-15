using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace PrimaryUserMachineSync
{
    class LogHelper
    {
        private static string path = string.Empty;
        private static string errorLogPath = string.Empty;

        private static object _lockMessage = new object();

        public static void SetNewLogPath(string appFolder, string fileName)
        {
            string dir = Path.Combine(appFolder, "Logs");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            path = Path.Combine(dir, string.Format("{0}_{1}.log", fileName, DateTime.Now.ToString("yyyyMMdd_HHmmss")));
        }

        public static void SetNewErrorLogPath(string appFolder, string fileName)
        {
            string dir = Path.Combine(appFolder, "Logs");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            errorLogPath = Path.Combine(dir, string.Format("{0}_ErrorLog_{1}.log", fileName, DateTime.Now.ToString("yyyyMMdd_HHmmss")));
        }

        public static void LogMessageAsync(object param)
        {
            lock (_lockMessage)
            {
                string logMessage = (string)param;
                StreamWriter log;
                if (!File.Exists(path))
                {
                    log = new StreamWriter(path);
                }
                else
                {
                    log = File.AppendText(path);
                }

                // Write to the file:
                log.WriteLine(DateTime.Now);
                log.WriteLine(logMessage);
                log.WriteLine();

                // Close the stream:
                log.Close();
            }
        }

        public static void LogErrorMessageAsync(object param)
        {
            lock (_lockMessage)
            {
                string logMessage = (string)param;
                StreamWriter log;
                if (!File.Exists(errorLogPath))
                {
                    log = new StreamWriter(errorLogPath);
                }
                else
                {
                    log = File.AppendText(errorLogPath);
                }

                // Write to the file:
                log.WriteLine(DateTime.Now);
                log.WriteLine(logMessage);
                log.WriteLine();

                // Close the stream:
                log.Close();
            }
        }

        public static void LogMessageWithoutTimeAsync(object param)
        {
            lock (_lockMessage)
            {
                string filePath = path;
                string logMessage = (string)param;
                StreamWriter log;
                if (!File.Exists(filePath))
                {
                    log = new StreamWriter(filePath);
                }
                else
                {
                    log = File.AppendText(filePath);
                }

                log.WriteLine(logMessage);

                // Close the stream:
                log.Close();
            }
        }

        public static void LogMessage(string message)
        {
            Thread logThread = new Thread(LogMessageAsync);
            logThread.Start(message);
        }

        public static void LogErrorMessage(string message)
        {
            Thread logThread = new Thread(LogErrorMessageAsync);
            logThread.Start(message);
        }

        public static void LogMessageWithoutTime(string message)
        {
            Thread logThread = new Thread(LogMessageWithoutTimeAsync);
            logThread.Start(message);
        }
    }
}
