using System.Collections.Generic;
using System.IO;

namespace Hawking.Xslt.Transformer
{
    public interface IXsltTransformer
    {
        Stream Execute(Stream xslStream, Stream inputStream, IDictionary<string, object> extensionObjects, bool executeCompiled = true);
    }
}
