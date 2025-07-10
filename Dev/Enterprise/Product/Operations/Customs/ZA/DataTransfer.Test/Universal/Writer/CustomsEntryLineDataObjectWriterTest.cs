using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class CustomsEntryLineDataObjectWriterTest : TestCaseWithUniversalObjectFactory
	{
		public void TestPopulateAdditionalInformation()
		{
			var universalHelper = new ZAUniversalReferenceTestDataHelper(Factory.BOFactory, setupBasicTariffData: false);
			universalHelper.CreateAdditionalInformationCusCodeEntry("NUI");
			universalHelper.CreateAdditionalInformationCusCodeEntry("PPR");
			Factory.SaveForTesting();
			var source = Factory.New<CusEntryLine>();
			source.AdditionalInformationCodes.AddNew("NUI", "Y");
			source.AdditionalInformationCodes.AddNew("PPR", "PPRVal");
			source.AdditionalInformationCodes.AddNew("XXX", "FFD");
			var writer = new CustomsEntryLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.SouthAfrica));
			var result = writer.GetDataObject(source);
			CombineAssertions(() =>
			{
				AssertEquals(3, result.CustomsReferenceCollection.Count);
				AssertEquals("Y", result.CustomsReferenceCollection.FirstOrDefault(x => x.SubType.Code.Value == "NUI").Reference.Value);
				AssertEquals("ADI", result.CustomsReferenceCollection.FirstOrDefault(x => x.SubType.Code.Value == "NUI").Type.Code.Value);
				AssertEquals("New Used Indicator", result.CustomsReferenceCollection.FirstOrDefault(x => x.SubType.Code.Value == "NUI").SubType.Description.Value);
				AssertEquals("PPRVal", result.CustomsReferenceCollection.FirstOrDefault(x => x.SubType.Code.Value == "PPR").Reference.Value);
				AssertEquals("ADI", result.CustomsReferenceCollection.FirstOrDefault(x => x.SubType.Code.Value == "PPR").Type.Code.Value);
				AssertEquals("Provisional Payment Surety Reference", result.CustomsReferenceCollection.FirstOrDefault(x => x.SubType.Code.Value == "PPR").SubType.Description.Value);
				AssertEquals("FFD", result.CustomsReferenceCollection.FirstOrDefault(x => x.SubType.Code.Value == "XXX").Reference.Value);
				AssertEquals("ADI", result.CustomsReferenceCollection.FirstOrDefault(x => x.SubType.Code.Value == "XXX").Type.Code.Value);
				AssertEquals(null, result.CustomsReferenceCollection.FirstOrDefault(x => x.SubType.Code.Value == "XXX").SubType.Description);
			});
		}

		public void TestPopulateProvisionalPayments()
		{
			var source = Factory.New<CusEntryLine>();
			source.Fees.AddOrUpdate("1P1", 50m);
			var pp1 = source.ProvisionalPayments.AddNew();
			pp1.CY_Code = "PPA";
			pp1.CY_Value = 70m;
			var pp2 = source.ProvisionalPayments.AddNew();
			pp2.CY_Code = "PPX";
			pp2.CY_Value = 80m;
			var writer = new CustomsEntryLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.SouthAfrica));
			var result = writer.GetDataObject(source);
			CombineAssertions(() =>
			{
				AssertEquals(3, result.EntryLineChargeCollection.Count);
				AssertEquals(50m, result.EntryLineChargeCollection.FirstOrDefault(x => x.Type.Code.Value == "1P1").Amount);
				AssertEquals(70m, result.EntryLineChargeCollection.FirstOrDefault(x => x.Type.Code.Value == "PPA").Amount);
				AssertEquals(80m, result.EntryLineChargeCollection.FirstOrDefault(x => x.Type.Code.Value == "PPX").Amount);
			});
		}
	}
}
