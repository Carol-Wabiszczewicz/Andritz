using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleTimeshift
{
    public class Shifter
    {
        public static async Task Shift(Stream input, Stream output, TimeSpan timeSpan, Encoding encoding, int bufferSize = 1024, bool leaveOpen = false)
        {
            var reader = new StreamReader(input, encoding);
            var writer = new StreamWriter(output, encoding, bufferSize);
            
            var blocks = new List<SubtitleBlock>();
            SubtitleBlock currentBlock = null;
            var lineCount = 0;
            
            try
            {
                string currentLine;
                while ((currentLine = await reader.ReadLineAsync()) != null)
                {
                    lineCount++;

                    if (IsEndOfBlock(currentLine, ref currentBlock))
                    {
                        blocks.Add(currentBlock);
                        currentBlock = null;
                    }

                    ProcessBlock(currentLine, ref currentBlock, lineCount);
                }
                
                if(currentLine == null)
                    blocks.Add(currentBlock);

                foreach (var block in blocks)
                    await WriteBlock(writer, block, timeSpan);

                if (!leaveOpen)
                    writer.Close();
            }
            catch
            {
                reader.Close();
                writer.Close();
                throw;
            }
        }

        private static bool IsEndOfBlock(string line, ref SubtitleBlock currentBlock)
        {
            return int.TryParse(line, out _) && currentBlock != null;
        }

        private static void ProcessBlock(string line, ref SubtitleBlock subtitleBlock, int lineCount)
        {
            if (subtitleBlock == null)
            {
                var index = TryGetIndex(line, lineCount);
                subtitleBlock = new SubtitleBlock{ Index = index };
                return;
            }

            if (subtitleBlock.StartTime == null)
            {
                ProcessBlockTimeSpan(ref subtitleBlock, line, lineCount);
                return;
            }
            
            subtitleBlock.Texts.Add(line);
        }

        private static int TryGetIndex(string line, int lineCount)
        {
            var tryGetIndex = int.TryParse(line, out var index);
            if (!tryGetIndex)
                throw new ArgumentException($"A index was expected but not found on line {lineCount}");

            return index;
        }

        private static void ProcessBlockTimeSpan(ref SubtitleBlock subtitleBlock, string line, int lineCount)
        {
            var entryTimes = line.Split(new[] {" --> "}, StringSplitOptions.None);

            try
            {
                subtitleBlock.StartTime = TimeSpan.Parse(entryTimes[0]);
                subtitleBlock.EndTime = TimeSpan.Parse(entryTimes[1]);
            }
            catch
            {
                throw new ArgumentException($"A valid time interval was expected but not found on line {lineCount}");
            }
        }

        private static async Task WriteBlock(StreamWriter streamWriter, SubtitleBlock subtitleBlock, TimeSpan timeSpanToSync)
        {
            if (subtitleBlock.StartTime == null || subtitleBlock.EndTime == null)
                throw new ArgumentException("Invalid StartTime/EndTime found while writing the new srt");
            
            var (start, end) = (
                (TimeSpan) subtitleBlock.StartTime + timeSpanToSync,
                (TimeSpan) subtitleBlock.EndTime + timeSpanToSync);
            
            
            await streamWriter.WriteLineAsync(subtitleBlock.Index.ToString());
            await streamWriter.WriteLineAsync($"{start:hh\\:mm\\:ss\\.fff} --> {end:hh\\:mm\\:ss\\.fff}");

            foreach (var text in subtitleBlock.Texts)
                await streamWriter.WriteLineAsync(text);
        }
    }
}
