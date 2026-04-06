using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTG_Tools.ClassesStructs
{
    public class Ttarch2Class
    {
        public struct Ttarch2files
        {
            public ulong fileOffset;
            public int fileSize;
            public ulong fileNameCRC64;
            public string fileName;
        }

        public bool isCompressed;
        public bool isEncrypted;
        public bool isEncryptedLua;
        public uint chunkSize;
        public int compressAlgorithm; //-1 - unknown, 1 - deflate, 2 - oodle LZHLW, 3 - oodle Kraken
        public int version;
        public ulong filesOffset;
        public ulong cFilesOffset;
        public string fileName;
        public ulong[] compressedBlocks;
        public List<string> fileFormats;
        public Ttarch2files[] files;
        public int lastChunkSize; // actual decompressed size of the last chunk (may be < chunkSize)

        public Ttarch2Class()
        {
            isCompressed = false;
            isEncrypted = false;
            isEncryptedLua = false;
            version = 1;
            compressAlgorithm = -1; //-1 - unknown, 1 - deflate, 2 - oodle lz
            chunkSize = 0x10000;
        }
    }
}
