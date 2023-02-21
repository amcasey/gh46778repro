using System.Diagnostics;

namespace ConsoleApp1
{
    enum TraceLevel
    {
        Verbose = 0,
        Info,
        Warning,
        Error
    }

    public class Tracer
    {
        private static TraceLevel _masterTraceLevel = TraceLevel.Info;

        public static void Verbose(string message) => Trace(TraceLevel.Verbose, message);
        public static void Info(string message) => Trace(TraceLevel.Info, message);
        public static void Warning(string message) => Trace(TraceLevel.Warning, message);
        public static void Error(string message) => Trace(TraceLevel.Error, message);

        private static void Trace(TraceLevel tl, string msg)
        {
            if ((int)tl >= (int)_masterTraceLevel)
            {
                Console.WriteLine($"{msg}");
            }
        }
    }
}
