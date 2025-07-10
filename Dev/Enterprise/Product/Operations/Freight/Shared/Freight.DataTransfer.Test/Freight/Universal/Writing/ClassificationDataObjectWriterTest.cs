using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ClassificationDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateDataObject()
		{
			var harmonisedCodeBO = Factory.New<JobPackLineHarmonisedCode>();
			harmonisedCodeBO.JLH_Code = "HS1";
			harmonisedCodeBO.JLH_RN_NKCountry = "AU";

			var writer = new ClassificationDataObjectWriter<JobPackLineHarmonisedCode>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, harmonisedCodeBO)));
			var dataObject = writer.GetDataObject(harmonisedCodeBO);

			AssertEquals("HS1", dataObject.Code);
			AssertEquals("HSC", dataObject.Type.Code);
			AssertEquals("Harmonized Code", dataObject.Type.Description);
			AssertEquals("AU", dataObject.Country.Code);
			AssertEquals("Australia", dataObject.Country.Name);
		}
	}
}
