using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

public class CUSDECEDIMessage : NOEDIMessage
{
	public CUSDECEDIMessage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_MessageType = EDIMessageConstants.MessageTypes.CUSDEC;
		EM_ReceiveTransmit = Direction.Transmit;
	}

	protected override string GetMessageReferenceNumber()
	{
		return ZDateTime.Now.Ticks.ToString();
	}
}
