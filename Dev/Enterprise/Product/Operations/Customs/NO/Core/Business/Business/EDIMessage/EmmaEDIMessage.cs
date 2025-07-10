using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business;

public class EmmaEDIMessage(BusinessObjectFactory factory, DataRow row) : NOEDIMessage(factory, row)
{
	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_MessageType = EDIMessageConstants.MessageTypes.EMMA;
		EM_ReceiveTransmit = Direction.Transmit;
	}
}
