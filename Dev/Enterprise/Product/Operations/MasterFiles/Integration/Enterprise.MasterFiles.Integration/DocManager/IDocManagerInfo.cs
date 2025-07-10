using CargoWise.IO;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDocManagerInfo
	{
		IeDocBase AddFileOrDocument(SubStreamableStream contents, string filenameOnly, string documentType);
		IeDocBase AddFileOrDocument(byte[] contents, string filenameOnly, string documentType);
	}
}
