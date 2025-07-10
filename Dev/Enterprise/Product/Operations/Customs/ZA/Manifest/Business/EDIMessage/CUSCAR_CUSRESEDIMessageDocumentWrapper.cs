using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class CUSCAR_CUSRESEDIMessageDocumentWrapper : DocumentWrapper
	{
		public CUSCAR_CUSRESEDIMessageDocumentWrapper(CUSRESEDIMessage message, BusinessObjectFactory factoryToWrap)
			: base(message, factoryToWrap)
		{
		}

		public static CUSCAR_CUSRESEDIMessageDocumentWrapper New(BusinessObject bizO, BusinessObjectFactory factoryToWrap)
		{
			return bizO is CUSRESEDIMessage cusres ? new CUSCAR_CUSRESEDIMessageDocumentWrapper(cusres, factoryToWrap) : null;
		}

		public CUSRESEDIMessage CUSRESMessage => (CUSRESEDIMessage)WrappedObject;

		public AsycudaManifestHeader Manifest
		{
			get
			{
				var linkedObject = CUSRESMessage.EM_LinkedObject;
				switch (linkedObject)
				{
					case AsycudaManifestHeader manifest:
						return manifest;
					case AsycudaBill bill:
						return bill.Header;
					default:
						return null;
				}
			}
		}

		public CUSCAREDIMessage CUSCARMessage
		{
			get
			{
				var documentNumber = CUSRESMessage.CUSRESHelper?.LRNNumber ?? ZString.Empty;
				return documentNumber.IsEmpty ? null : Manifest?.Messages.OfType<CUSCAREDIMessage>().FirstOrDefault(x => (x.CUSCARD16AHelper?.UniqueReferenceNumber ?? ZString.Empty) == documentNumber);
			}
		}
	}
}
