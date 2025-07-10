using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Manifest.Business;

sealed class DMOManifestHeaderMessageSendingObject : DMOMessageSendingObject
{
	public DMOManifestHeaderMessageSendingObject(AsycudaManifestHeader manifestHeader) : base(manifestHeader?.Factory)
	{
		Argument.NotNull(manifestHeader, nameof(manifestHeader));
	}

	public override ZString CustomsLevel => NODMOEDIMessageTypeList.Descriptions.TRA;

	public override ZString BillNumber => ZString.Empty;

	public override ZString Representative => ZString.Empty;

	public override ZString Consignee => ZString.Empty;
}
