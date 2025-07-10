using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class will be used in a future Work Item")]
	public class T15MessageGenerator : Phase5NcstMessageGenerator
	{
		public T15MessageGenerator(IMessageSender sender) : base(sender)
		{
		}

		public override ZString MessageType => TRMessageTypes.Codes.T15;

		protected override ZString GetXMLMessage()
		{
			var sample = (NoResString)@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<ns2:CC170C xmlns:ns2=""http://ncts.dgtaxud.ec"">
	<messageSender>TPA.TR</messageSender>
	<messageRecipient>NTA.TR</messageRecipient>
	<preparationDateAndTime>2025-01-13T11:11:54</preparationDateAndTime>
	<messageIdentification>Y6WcQyL2w6qGiS2eoE4mXNvDy3D2lHjWh6K</messageIdentification>
	<messageType>CC170C</messageType>
	<TransitOperation>
		<LRN>25LR210400000002</LRN>
	</TransitOperation>
	<CustomsOfficeOfDeparture>
		<referenceNumber>TR210400</referenceNumber>
	</CustomsOfficeOfDeparture>
	<HolderOfTheTransitProcedure>
		<identificationNumber>4650271675</identificationNumber>
	</HolderOfTheTransitProcedure>
	<Representative>
		<identificationNumber>30433448583</identificationNumber>
		<status>2</status>
	</Representative>
</ns2:CC170C>";
			return sample;
		}
	}
}
