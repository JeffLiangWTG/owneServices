using System;
using System.IO;
using Microsoft.XLANGs.BaseTypes;

// Thanks to http://blogdoc.biztalk247.com/article.aspx?page=0e280374-7a4e-4c86-b1ae-b03fe06d0906

namespace CargoWise.eHub.Products.AsycudaCustoms.Common
{
    public class BinaryStreamFactory : IStreamFactory, IDisposable
    {
        private Stream stream;

        public BinaryStreamFactory(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("Stream passed in is Null.");
            }
            if (stream.Position != 0)
            {
                stream.Position = 0;
            }
            this.stream = stream;
        }

        public Stream CreateStream()
        {
            return stream;
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    } 
}
