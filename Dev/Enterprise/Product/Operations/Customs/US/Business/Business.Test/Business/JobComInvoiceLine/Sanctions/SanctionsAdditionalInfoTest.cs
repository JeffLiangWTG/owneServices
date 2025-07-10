using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(SanctionsAdditionalInfo))]
	public class SanctionsAdditionalInfoTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			ISanctionsAdditionalInfo sanctionsAdditionalInfo = new SanctionsAdditionalInfo("01", "FSHNG INFO", "VESSEL FLAG", "CA");

			AssertEquals("ISanctionsAdditionalInfo.RecordID", "01", sanctionsAdditionalInfo.RecordID);
			AssertEquals("ISanctionsAdditionalInfo.RecordType", "FSHNG INFO", sanctionsAdditionalInfo.RecordType);
			AssertEquals("ISanctionsAdditionalInfo.FieldName", "VESSEL FLAG", sanctionsAdditionalInfo.FieldName);
			AssertEquals("ISanctionsAdditionalInfo.FieldValue", "CA", sanctionsAdditionalInfo.FieldValue);
		}
	}
}
