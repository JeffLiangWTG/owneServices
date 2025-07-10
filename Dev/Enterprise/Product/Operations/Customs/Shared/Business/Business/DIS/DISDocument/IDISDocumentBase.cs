using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface IDISDocumentBase
	{
		ZString DocumentDescription { get; set; }
		ZString Status { get; set; }
		ZGuid RequiredDocumentPK { get; set; }
		JobRequiredDocument RequiredDocument { get; }
		JobRequiredDocumentAddInfo RequiredDocumentAddInfo { get; set; }
		IDisposable SuspendSettingHasChanges();
		IDisposable GetValidationSuspender();
		ZString Serialize(string xmlNamespace);
		void Deserialize(string xml, string xmlNamespace = null);
		ZGuid EDocsDocumentPK { get; set; }
	}
}
