using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OrgImpAddInfo))]
	public class OrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeserialiseOnConstruction()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)organisation.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_ImporterType = "T";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgCountryData countryDataLoaded = factory2.Load<OrgCountryData>(organisation.CountryData.PK);
			AssertNotNull(countryDataLoaded);
			OrgImpAddInfo addInfoLoaded = new OrgImpAddInfo((ZPropertyInfoString)countryDataLoaded.OV_ImportCustomsDefaultAddInfoInfo);

			AssertEquals("T", addInfoLoaded.ZO_ImporterType);
			AssertEquals(false, addInfoLoaded.HasChanges);
		}

		public void TestIsPaidByImporter()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			var addInfo = new OrgImpAddInfo((ZPropertyInfoString)organisation.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
			addInfo.ZO_BrokerToPay = YesNoDefaultList.Codes.No;
			Assert(addInfo.IsPaidByImporter);

			addInfo.ZO_BrokerToPay = ZString.Empty;
			Assert(!addInfo.IsPaidByImporter);

			addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			Assert(!addInfo.IsPaidByImporter);

			addInfo.ZO_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			Assert(addInfo.IsPaidByImporter);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			return new OrgImpAddInfo((ZPropertyInfoString)organisation.CountryData.OV_ImportCustomsDefaultAddInfoInfo);
		}

		#region ReadOnly Security

		public void TestReadOnlySecurity()
		{
			bool oldOrgConsigneeValue = Env.Security.OrgConsigneeModifyCountrySpecificDetails.IsAllowed;
			bool oldOrgConfigValue = Env.Security.OrgConfigModifyCountryDefaults.IsAllowed;

			try
			{
				OrgHeader organisation = Factory.New<OrgHeader>();
				organisation.FillWithValidTestData();
				OrgImpAddInfo addInfo = new OrgImpAddInfo((ZPropertyInfoString)organisation.CountryData.OV_ImportCustomsDefaultAddInfoInfo);

				Env.Security.OrgConsigneeModifyCountrySpecificDetails.IsAllowed = true;
				AssertEquals("Access should be Allowed - Not ReadOnly", false, addInfo.ZO_PurchasedInfo.ReadOnly);

				Env.Security.OrgConsigneeModifyCountrySpecificDetails.IsAllowed = false;
				AssertEquals("Access should NOT Allowed - ReadOnly", true, addInfo.ZO_PurchasedInfo.ReadOnly);

				Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = true;
				AssertEquals("Access should be Allowed - Not ReadOnly", false, addInfo.ZO_SPDNumberOfDaysInfo.ReadOnly);
				AssertEquals("Access should be Allowed - Not ReadOnly", false, addInfo.ZO_PaymentTypeInfo.ReadOnly);
				AssertEquals("Access should be Allowed - Not ReadOnly", false, addInfo.ZO_DoNotAutoGenerateSDCRInfo.ReadOnly);
				AssertEquals("Access should be Allowed - Not ReadOnly", false, addInfo.ZO_IsEINNumberVerifiedIndicatorInfo.ReadOnly);

				Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = false;
				AssertEquals("Access NOT Allowed - ReadOnly", true, addInfo.ZO_SPDNumberOfDaysInfo.ReadOnly);
				AssertEquals("Access NOT Allowed - ReadOnly", true, addInfo.ZO_PaymentTypeInfo.ReadOnly);
				AssertEquals("Access NOT Allowed - ReadOnly", true, addInfo.ZO_DoNotAutoGenerateSDCRInfo.ReadOnly);
				AssertEquals("Access NOT Allowed - ReadOnly", true, addInfo.ZO_IsEINNumberVerifiedIndicatorInfo.ReadOnly);

				Env.Security.OrgConsigneeModifyCountrySpecificDetails.IsAllowed = false;
				Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = true;
				AssertEquals("Access should NOT Allowed - ReadOnly", true, addInfo.ZO_PurchasedInfo.ReadOnly);
				AssertEquals("Access should be Allowed - Not ReadOnly", false, addInfo.ZO_SPDNumberOfDaysInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgConsigneeModifyCountrySpecificDetails.IsAllowed = oldOrgConsigneeValue;
				Env.Security.OrgConfigModifyCountryDefaults.IsAllowed = oldOrgConfigValue;
			}
		}

		#endregion
	}
}
