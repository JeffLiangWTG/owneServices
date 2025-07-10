using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.NL.Business;

public class NLComparisonEDIMessage : NLEDIMessage
{
	public NLComparisonEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		EM_Status = EDIMessageStatusList.Codes.Acknowledged;
	}

	[BusinessObjectTestExclude]
	public override ZString EM_MessageInterpretation { get => string.Format(CultureInfo.CurrentCulture, messageInterpretation, EM_SystemCreateTimeUtc); set => base.EM_MessageInterpretation = value; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant message string")]
	const string messageInterpretation = "This is a snapshot of the state of your entry at {0} UTC, which was used to create an amendment/CRI request.  The details of what was sent to DMS should be viewed on the AMD/CRI message.";
}
