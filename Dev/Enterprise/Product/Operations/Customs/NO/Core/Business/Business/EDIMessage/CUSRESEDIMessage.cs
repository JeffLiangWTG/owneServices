using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business;

public class CUSRESEDIMessage : NOEDIMessage
{
	public CUSRESEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_MessageType = EDIMessageConstants.MessageTypes.CUSRES;
	}

	public new CUSRESEDIMessageLookups Lookups => (CUSRESEDIMessageLookups)base.Lookups;

	protected override EDIMessageLookups GetNewLookups() => new CUSRESEDIMessageLookups(this);

	protected override IEDIMessagePrettier GetNewPrettier() => new CUSRESEDIMessagePrettier(this);

	public override ZBool IsMessageInterpretationSetterSupported => false;
}
