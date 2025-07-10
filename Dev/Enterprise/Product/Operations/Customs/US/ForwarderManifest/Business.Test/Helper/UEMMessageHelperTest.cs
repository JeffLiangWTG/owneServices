using CargoWise.Customs.US.MessageDefinitions.ExportManifest;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class UEMMessageHelperTest : TestCaseWithFactory
	{
		public void TestDeSerializeManifestFiling()
		{
			string xml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<CBPManifestMessage
	xmlns=""http://manifest.cbp.dhs.gov/shared/model"">
	<Version></Version>
	<Filing>
		<SenderId>
			<Value>CUSTOMS</Value>
		</SenderId>
		<ReceiverId>
			<Value>OTT1</Value>
		</ReceiverId>
		<MessageDateTime>
			<Value>20240424 025042</Value>
		</MessageDateTime>
		<MessageControlNumber>
			<Value>MANPHL4X0000686</Value>
		</MessageControlNumber>
		<MessageReferenceNumber>
			<Value>WTLDUSBAK_1</Value>
		</MessageReferenceNumber>
	</Filing>
	<Conveyance>
		<CarrierCode>
			<Value>OTT1</Value>
		</CarrierCode>
		<ConveyanceName>
			<Value>TITANIC</Value>
		</ConveyanceName>
		<BOLInfoList>
			<BOLIssuerCode>
				<Value>APLU</Value>
			</BOLIssuerCode>
			<BOLNumber>
				<Value>369852147812</Value>
			</BOLNumber>
		</BOLInfoList>
	</Conveyance>
	<ResponseMessage>
		<ResponseCode>000</ResponseCode>
		<SeverityIndicator>I</SeverityIndicator>
		<NarrativeText>MESSAGE PROCESSED</NarrativeText>
	</ResponseMessage>
</CBPManifestMessage>";

			var result = UEMMessageHelper.DeSerializeManifestFiling(xml);

			AssertEquals(typeof(ManifestFiling), result.GetType());

			AssertEquals("CUSTOMS", result.Filing.SenderId.Value);
			AssertEquals("OTT1", result.Filing.ReceiverId.Value);
			AssertEquals("MANPHL4X0000686", result.Filing.MessageControlNumber.Value);
			AssertEquals("WTLDUSBAK_1", result.Filing.MessageReferenceNumber.Value);
			AssertEquals("OTT1", result.Conveyance[0].CarrierCode.Value);
			AssertEquals("TITANIC", result.Conveyance[0].ConveyanceName.Value);
			AssertEquals("APLU", result.Conveyance[0].BolInfoList[0].BolIssuerCode.Value);
			AssertEquals("369852147812", result.Conveyance[0].BolInfoList[0].BolNumber.Value);
			AssertEquals("000", result.ResponseMessage[0].ResponseCode);
			AssertEquals("I", result.ResponseMessage[0].SeverityIndicator);
			AssertEquals("MESSAGE PROCESSED", result.ResponseMessage[0].NarrativeText);
		}

		public void TestSeperateElementName()
		{
			AssertEquals("abc", UEMMessageHelper.SeparateElementName("abc"));
			AssertEquals("Abc", UEMMessageHelper.SeparateElementName("Abc"));
			AssertEquals("abc D", UEMMessageHelper.SeparateElementName("abcD"));
			AssertEquals("ABC", UEMMessageHelper.SeparateElementName("ABC"));
			AssertEquals("AB Cd", UEMMessageHelper.SeparateElementName("ABCd"));
			AssertEquals("Flight Trip Voyage Number", UEMMessageHelper.SeparateElementName("FlightTripVoyageNumber"));
			AssertEquals("BOL Info List", UEMMessageHelper.SeparateElementName("BOLInfoList"));
		}
	}
}
