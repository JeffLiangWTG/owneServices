using System.Text.RegularExpressions;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ClientContractNumberRegistryDataType))]
	sealed class ClientContractNumberRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ClientContractNumberRegistryDataType>
	{
		public void TestDefaultValues()
		{
			var dataType = GetNewDataType();

			CombineAssertions(() =>
			{
				AssertEquals("CLA", dataType.FountainPrefix);
				AssertEquals("Client Contract Number Format", dataType.GeneratedNumberName);
				AssertEquals(RatingContractSchema.RCT_ContractNumber.MaxLength, dataType.MaxLength);
				AssertEquals(dataType.DefaultValue.Categories, NumberCustomisationElementCategories.ClientContract);
				AssertEquals("NON", dataType.DefaultValue.CheckDigitAlgorithm);
			});
		}

		protected override string ExpectedEditorName => "BillOfLadingNumberCustomisationRegistryItemEditor";

		protected override ClientContractNumberRegistryDataType GetNewDataType() => new ClientContractNumberRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result1 = new ClientContractNumberCustomisation();
			result1.CheckDigitAlgorithm = "NON";

			var result2 = new ClientContractNumberCustomisation();
			result2.CheckDigitAlgorithm = "R31";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(result1, GetExpectedXML("NON")),
				new ValidSampleAndBinaryValueInDB(result2, GetExpectedXML("R31"))
			};
		}

		string GetExpectedXML(string exepctedCheckDigit)
		{
			var xml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<ClientContractNumberCustomisation>
	<ServiceLevel />
	<RemoveFountainPrefix>N</RemoveFountainPrefix>
	<CheckDigitAlgorithm>{exepctedCheckDigit}</CheckDigitAlgorithm>
	<UseShipmentSequenceNumber>N</UseShipmentSequenceNumber>
	<Elements>
		<Element key=""SequenceNumber"">
			<Order>50</Order>
			<CheckDigit>Y</CheckDigit>
			<Detail>8</Detail>
		</Element>
		<Element key=""GlobalOrLocal"">
			<Order>2</Order>
			<CheckDigit>Y</CheckDigit>
		</Element>
	</Elements>
</ClientContractNumberCustomisation>
";

			return Regex.Replace(xml, @"\t|\n|\r", "");
		}
	}
}
