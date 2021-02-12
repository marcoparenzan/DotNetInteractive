using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoWebAPI
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

        public void Write(IBufferWriter<byte> writer)
        {
            lock (entries)
            {
                while (entries.Count > 0)
                {
                    var entry = entries.Dequeue();
                    entry.WriteLine(writer);
                }
            }
        }
    }

    record LogEntry(string Message, LogEntryType Type)
    {
        public void WriteLine(IBufferWriter<byte> writer)
        {
            switch (Type)
            {
                case LogEntryType.Code:
                    WriteLineImpl(writer, ConsoleColor.Blue);
                    break;
                case LogEntryType.Value:
                    WriteLineImpl(writer, ConsoleColor.Green);
                    break;
                case LogEntryType.Error:
                    WriteLineImpl(writer, ConsoleColor.Red);
                    break;
                default:
                    WriteLineImpl(writer);
                    break;
            }
        }

        void WriteLineImpl(IBufferWriter<byte> writer, ConsoleColor? color = null)
        {
            writer.Write(Encoding.UTF8.GetBytes(Message));
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
