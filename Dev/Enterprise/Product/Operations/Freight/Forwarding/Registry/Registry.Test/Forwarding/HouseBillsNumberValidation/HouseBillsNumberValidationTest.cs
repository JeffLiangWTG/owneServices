using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(HouseBillsNumberValidation))]
	internal sealed class HouseBillsNumberValidationTest : RegistryBusinessObjectTemplateTestCase<HouseBillsNumberValidation>
	{
		public void TestTransportMode()
		{
			BizObj.TransportMode = "SEA";
			AssertNoErrors(BizObj.TransportModeInfo);

			BizObj.TransportMode = "AIR";
			AssertNoErrors(BizObj.TransportModeInfo);

			BizObj.TransportMode = "";
			AssertHasErrors(BizObj.TransportModeInfo);

			BizObj.TransportMode = "XYZ";
			AssertHasErrors(BizObj.TransportModeInfo);

			BizObj.TransportMode = "ALL";
			AssertNoErrors(BizObj.TransportModeInfo);
		}

		public void TestOrigin()
		{
			BizObj.Origin = "DE";
			AssertNoErrors(BizObj.OriginInfo);

			BizObj.Origin = "";
			AssertNoErrors(BizObj.OriginInfo);

			BizObj.Origin = "ABCDE";
			AssertHasErrors(BizObj.OriginInfo);

			BizObj.Origin = "DEHAM";
			AssertNoErrors(BizObj.OriginInfo);
		}

		public void TestDestination()
		{
			BizObj.Destination = "US";
			AssertNoErrors(BizObj.DestinationInfo);

			BizObj.Destination = "";
			AssertNoErrors(BizObj.DestinationInfo);

			BizObj.Destination = "ABCDE";
			AssertHasErrors(BizObj.DestinationInfo);

			BizObj.Destination = "USCHI";
			AssertNoErrors(BizObj.DestinationInfo);
		}

		public void TestCheckDigitAlgorithm()
		{
			BizObj.CheckDigitAlgorithm = "NON";
			AssertNoErrors(BizObj.CheckDigitAlgorithmInfo);

			BizObj.CheckDigitAlgorithm = "";
			AssertHasErrors(BizObj.CheckDigitAlgorithmInfo);

			BizObj.CheckDigitAlgorithm = "R07";
			AssertNoErrors(BizObj.CheckDigitAlgorithmInfo);

			BizObj.CheckDigitAlgorithm = "R31";
			AssertNoErrors(BizObj.CheckDigitAlgorithmInfo);

			BizObj.CheckDigitAlgorithm = "XYZ";
			AssertHasErrors(BizObj.CheckDigitAlgorithmInfo);
		}

		public void TestSuffix()
		{
			BizObj.HBLSuffix = "ABC";
			AssertHasError(BizObj.HBLSuffixInfo, "The HBL Suffix must start with a symbol.");

			BizObj.HBLSuffix = "/ABC";
			AssertNoErrors(BizObj.HBLSuffixInfo);
		}

		public void TestLength()
		{
			BizObj.HBLLengthFormatted = 21.ToString();
			AssertHasError(BizObj.HBLLengthFormattedInfo, "The maximum length for a House Bill is 20 characters.");

			BizObj.HBLLengthFormatted = 20.ToString();
			AssertNoErrors(BizObj.HBLLengthFormattedInfo);

			BizObj.HBLLengthFormatted = 0.ToString();
			AssertEquals("0 Length is displayed as blank", ZString.Empty, BizObj.HBLLengthFormatted);
			AssertNoErrors(BizObj.HBLLengthFormattedInfo);
		}

		public void TestDuplicateValidation()
		{
			var collection = new HouseBillsNumberValidationCollection();

			var rule1 = collection.AddNew();
			rule1.TransportMode = "AIR";
			rule1.Origin = "DEHAM";
			rule1.Destination = "FRPAR";
			rule1.HBLPrefix = "ABC";
			rule1.HBLSuffix = "/XYZ";
			rule1.UserDefinedCondition = "\"<JS_ReleaseType>\" == \"Test\"";

			AssertNoRowErrors(rule1);

			var rule2 = collection.AddNew();
			rule2.TransportMode = "AIR";
			rule2.Origin = "DEHAM";
			rule2.Destination = "FRPAR";
			rule2.HBLPrefix = "ABC";
			rule2.HBLSuffix = "/XYZ";
			rule2.UserDefinedCondition = "\"<JS_ReleaseType>\" == \"Test\"";

			AssertHasRowError(rule2, "Duplicate entries are not allowed. Each entry must be unique across Transport Mode, Origin, Destination, HBL Prefix, HBL Suffix and User Defined Condition.");

			rule2.UserDefinedCondition = "";

			AssertNoRowErrors(rule2);
		}

		public void TestTransportModeList()
		{
			Assert(BizObj.TransportModeList.ContainsCode("SEA"));
			Assert(BizObj.TransportModeList.ContainsCode("AIR"));
			Assert(BizObj.TransportModeList.ContainsCode("ROA"));
			Assert(BizObj.TransportModeList.ContainsCode("RAI"));
			Assert(BizObj.TransportModeList.ContainsCode("ALL"));

			AssertEquals(5, BizObj.TransportModeList.Count);
		}

		public void TestCheckDigitAlgorithm_List()
		{
			Assert(BizObj.CheckDigitAlgorithmList.ContainsCode("NON"));
			Assert(BizObj.CheckDigitAlgorithmList.ContainsCode("R07"));
			Assert(BizObj.CheckDigitAlgorithmList.ContainsCode("R31"));
			Assert(BizObj.CheckDigitAlgorithmList.ContainsCode("RCC"));

			AssertEquals(4, BizObj.CheckDigitAlgorithmList.Count);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override HouseBillsNumberValidation GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override HouseBillsNumberValidation GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		HouseBillsNumberValidation NewPopulatedBusinessObject()
		{
			HouseBillsNumberValidation result = new HouseBillsNumberValidation();

			result.TransportMode = "SEA";
			result.Origin = "AUBNE";
			result.Destination = "DEHAM";
			result.HBLPrefix = "ABC";
			result.IncludeHBLPrefix = true;
			result.HBLSuffix = "/XYZ";
			result.HBLLengthFormatted = 12.ToString();
			result.UserDefinedCondition = "12345";
			result.CheckDigitAlgorithm = "R31";

			return result;
		}

		#endregion
	}
}
