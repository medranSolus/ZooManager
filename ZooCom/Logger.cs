﻿using System;
using System.IO;
using System.Threading;

namespace ZooCom
{
    public static class Logger
    {
        public static void LogError(string msg, Type type = null, string method = null)
        {
            Log("ERROR", msg, type, method);
        }

        public static void LogWarning(string msg, Type type = null, string method = null)
        {
            Log("WARNING", msg, type, method);
        }

        public static void LogInfo(string msg, Type type = null, string method = null)
        {
            Log("INFO", msg, type, method);
        }

        static void Log(string type, string msg, Type objType, string method)
        {
            new Thread(() =>
            {
                const string path = "log_com.txt";
                if (objType == null)
                    method = $"unknown:{method}";
                else
                    method = $"{objType.ToString()}:{method}";
                bool isFree = false;
                do
                {
                    try
                    {
                        using (Mutex writeLock = new Mutex(true, "ZooCom.Logger", out isFree))
                        {
                            if (isFree)
                            {
                                if (!File.Exists(path))
                                    File.Create(path).Close();
                                using (TextWriter writer = new StreamWriter(path, true))
                                    writer.WriteLine($"<{DateTime.Now.ToString()}> [{type}] @{method}: {msg}");
                                writeLock.ReleaseMutex();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        isFree = false;
                    }
                    if (!isFree)
                        Thread.Sleep(1000);
                } while (!isFree);
            }).Start();
        }
    }
}
