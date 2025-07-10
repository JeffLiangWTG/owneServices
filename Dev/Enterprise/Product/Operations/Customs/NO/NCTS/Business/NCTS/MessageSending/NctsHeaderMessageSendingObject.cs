namespace Enterprise.Customs.NO.NCTS.Business;

public sealed class NctsHeaderMessageSendingObject : EU.NCTS.Business.NctsHeaderMessageSendingObject
{
	public NctsHeaderMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;
}
