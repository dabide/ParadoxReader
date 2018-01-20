using System;
using System.IO;

namespace ParadoxReader
{
    internal class ParadoxBlobFile : IDisposable
    {
        private readonly BinaryReader _reader;
        private readonly Stream _stream;

        public ParadoxBlobFile(string fileName)
            : this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
        }

        public ParadoxBlobFile(Stream stream)
        {
            _stream = stream;
            _reader = new BinaryReader(stream);
        }

        public virtual void Dispose()
        {
            _stream.Dispose();
        }

        public byte[] ReadBlob(byte[] blobInfo)
        {
            uint offsetAndIndex = BitConverter.ToUInt32(blobInfo, 0);
            uint offset = offsetAndIndex & 0xffffff00;

            int size = BitConverter.ToInt32(blobInfo, 4);
            int hsize = 9;

            if (size > 0)
            {
                //Console.WriteLine("Graphic index={0}; blobsize={1}; mod_nr={2}", index, blobsize, mod_nr);

                _stream.Position = offset;

                byte[] head = new byte[6];
                _reader.Read(head, 0, 3);

                //TODO check for type 2 and index=255

                _reader.Read(head, 0, hsize - 3); //Read remaining 6 bytes of header
                int checkSize = BitConverter.ToInt32(head, 0);
                if (checkSize == size)
                {
                    byte[] buffer = new byte[size];

                    _reader.Read(buffer, 0, size);
                    return buffer;
                }
            }

            return null;
        }
    }
}
