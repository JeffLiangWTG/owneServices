using CargoWise.Types;
using Enterprise.Customs.ZA.Business;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public interface IManifestSupportingDocSendingObject : IZASupportingDocSendingObject
	{
		ZString Reference { get; }
	}
}
