using System;

namespace TTG_Tools
{
    // Compression statistics class
    public class CompressionStats
    {
        public int TotalChunks = 0;
        public int OodleChunks = 0;
        public int DeflateChunks = 0;
        public int FailedCompression = 0;
        public long TotalUncompressed = 0;
        public long TotalCompressed = 0;

        public void Report()
        {
            double ratio = TotalUncompressed > 0 ? (TotalCompressed * 100.0 / TotalUncompressed) : 0;
            string report = $"=== COMPRESSION STATS ===\n" +
                $"Total Chunks: {TotalChunks}\n" +
                $"Oodle Chunks: {OodleChunks}\n" +
                $"Deflate Chunks: {DeflateChunks}\n" +
                $"Failed Compression: {FailedCompression}\n" +
                $"Total Uncompressed: {TotalUncompressed:N0} bytes\n" +
                $"Total Compressed: {TotalCompressed:N0} bytes\n" +
                $"Compression Ratio: {ratio:F2}%\n" +
                $"========================";
            System.Diagnostics.Debug.WriteLine(report);
        }
    }
}
