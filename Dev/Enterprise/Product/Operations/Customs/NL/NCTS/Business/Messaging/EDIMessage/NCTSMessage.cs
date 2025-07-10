using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NCTSMessage : NLEDIMessage
{
	public NCTSMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_MessageType = NLNctsConstants.Messaging.NCT;
	}
}
