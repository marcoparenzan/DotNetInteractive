using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoConsoleNotebook
{
    public class Log
    {
        Queue<LogEntry> entries = new Queue<LogEntry>();

        public Log Info(string message)
        {
            entries.Enqueue(new LogEntry(message, LogEntryType.Info));
            return this;
        }

        public Log Code(string message)
        {
            entries.Enqueue(new LogEntry(message, LogEntryType.Code));
            return this;
        }

        public Log Value(string message)
        {
            entries.Enqueue(new LogEntry(message, LogEntryType.Value));
            return this;
        }

        public Log Error(string message)
        {
            entries.Enqueue(new LogEntry(message, LogEntryType.Error));
            return this;
        }

        public void Write()
        {
            lock (entries)
            {
                while (entries.Count > 0)
                {
                    var entry = entries.Dequeue();
                    entry.WriteLine();
                }
            }
        }
    }

    record LogEntry(string Message, LogEntryType Type)
    {
        public void WriteLine()
        {
            switch (Type)
            {
                case LogEntryType.Code:
                    WriteLineImpl(ConsoleColor.Blue);
                    break;
                case LogEntryType.Value:
                    WriteLineImpl(ConsoleColor.Green);
                    break;
                case LogEntryType.Error:
                    WriteLineImpl(ConsoleColor.Red);
                    break;
                default:
                    WriteLineImpl();
                    break;
            }
        }

        void WriteLineImpl(ConsoleColor? color = null)
        {
            if (color == null)
            {
                Console.WriteLine(Message);
            }
            else
            {
                var saveColor = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
                Console.WriteLine(Message);
                Console.ForegroundColor = saveColor;
            }
        }
    }

    enum LogEntryType
    {
        Info,
        Code,
        Value,
        Error
    }
}
