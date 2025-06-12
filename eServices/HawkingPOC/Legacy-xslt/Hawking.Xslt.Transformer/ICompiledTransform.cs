using System.IO;

namespace Hawking.Xslt.Transformer
{
    public interface ICompiledTransform
    {
        Stream Execute(string xslContent);
    }
}
