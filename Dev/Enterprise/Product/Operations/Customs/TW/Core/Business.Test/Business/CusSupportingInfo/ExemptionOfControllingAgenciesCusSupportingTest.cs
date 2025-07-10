using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExemptionOfControllingAgenciesCusSupporting))]
	sealed class ExemptionOfControllingAgenciesCusSupportingTest : Customs.Business.Testing.CusSupportingInfoTest<ExemptionOfControllingAgenciesCusSupporting>
	{
		protected override IEnumerable<ExemptionOfControllingAgenciesCusSupporting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var invoiceLine = factory.NewWithValidTestData<JobComInvoiceLine>();
			var exemptionOfControllingAgenciesCusSupporting = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgenciesCusSupporting.CSI_ReferenceNumber = "X1";
			yield return exemptionOfControllingAgenciesCusSupporting;
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var supporting = GetBizObjsForCorrectlyTypeDecideTest(Factory).FirstOrDefault();
			NUnit.Framework.Assert.That(supporting.CSI_Type, NUnit.Framework.Is.EqualTo(CusSupportingInfoTypeList.Codes.PermitExemptionCodes).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(supporting.CSI_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCSI_ReferenceNumber()
		{
			var supporting = GetBizObjsForCorrectlyTypeDecideTest(Factory).FirstOrDefault();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(supporting.CSI_ReferenceNumberInfo, "Code", "The special code for exemption provided by the controlling agency. Entered in the Permit Number field.");
		}

		[ExpectNoExceptions]
		public void TestRefreshBindingCalledWhenSetCSI_ReferenceNumber()
		{
			var line = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var permitExemptionCode1InfoRefreshed = false;
			line.PermitExemptionCode1Info.ValueChanged += (e, s) => { permitExemptionCode1InfoRefreshed = true; };
			var permitExemptionCode2InfoRefreshed = false;
			line.PermitExemptionCode2Info.ValueChanged += (e, s) => { permitExemptionCode2InfoRefreshed = true; };
			var permitExemptionCode3InfoRefreshed = false;
			line.PermitExemptionCode3Info.ValueChanged += (e, s) => { permitExemptionCode3InfoRefreshed = true; };
			var permitExemptionCode4InfoRefreshed = false;
			line.PermitExemptionCode4Info.ValueChanged += (e, s) => { permitExemptionCode4InfoRefreshed = true; };
			var permitExemptionCode5InfoRefreshed = false;
			line.PermitExemptionCode5Info.ValueChanged += (e, s) => { permitExemptionCode5InfoRefreshed = true; };

			var exemptionOfControllingAgency1 = line.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgency1.CSI_ReferenceNumber = "1";
			var exemptionOfControllingAgency2 = line.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgency2.CSI_ReferenceNumber = "2";
			var exemptionOfControllingAgency3 = line.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgency3.CSI_ReferenceNumber = "3";
			var exemptionOfControllingAgency4 = line.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgency4.CSI_ReferenceNumber = "4";
			var exemptionOfControllingAgency5 = line.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			exemptionOfControllingAgency5.CSI_ReferenceNumber = "5";

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(permitExemptionCode1InfoRefreshed, NUnit.Framework.Is.True, "PermitExemptionCode1Info Refreshed");
				NUnit.Framework.Assert.That(permitExemptionCode2InfoRefreshed, NUnit.Framework.Is.True, "PermitExemptionCode2Info Refreshed");
				NUnit.Framework.Assert.That(permitExemptionCode3InfoRefreshed, NUnit.Framework.Is.True, "PermitExemptionCode3Info Refreshed");
				NUnit.Framework.Assert.That(permitExemptionCode4InfoRefreshed, NUnit.Framework.Is.True, "PermitExemptionCode4Info Refreshed");
				NUnit.Framework.Assert.That(permitExemptionCode5InfoRefreshed, NUnit.Framework.Is.True, "PermitExemptionCode5Info Refreshed");
			});
		}

		[ExpectNoExceptions]
		public void TestSpecialCodeDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Codes.SpecialCodesForExemptionOfControllingAgencies, "Permit Exemption Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.SpecialCodesForExemptionOfControllingAgencies, "SP999999999999", "TestSpecialCodeDescription", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var supporting = GetBizObjsForCorrectlyTypeDecideTest(Factory).FirstOrDefault();
			var resourceStringDataAttribute = supporting.SpecialCodeDescriptionInfo.GetAttribute<ResourceStringDataAttribute>();
			NUnit.Framework.Assert.That(resourceStringDataAttribute.Caption, NUnit.Framework.Is.EqualTo("Description"));
			var listAttribute = supporting.SpecialCodeDescriptionInfo.GetAttribute<ListAttribute>();
			supporting.CSI_ReferenceNumber = "SP999999999999";
			NUnit.Framework.Assert.That(supporting.SpecialCodeDescription, NUnit.Framework.Is.EqualTo("TestSpecialCodeDescription").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetSpecialCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Codes.SpecialCodesForExemptionOfControllingAgencies, "Permit Exemption Codes");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.SpecialCodesForExemptionOfControllingAgencies, "SP1234", "SP1234Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Codes.SpecialCodesForExemptionOfControllingAgencies, "XD5678", "XD5678Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Codes.SpecialCodesForExemptionOfControllingAgencies, "FR1234", "FR1234Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Codes.SpecialCodesForExemptionOfControllingAgencies, "US1234", "US1234Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var list = (TWSpecialCodeCollection)ExemptionOfControllingAgenciesCusSupporting.GetSpecialCodeList(Factory);
			list.Load();
			CombineAssertions(() =>
			{
				var specialCodes = list.Cast<TWSpecialCode>();
				NUnit.Framework.Assert.That(specialCodes.Any(c => c.Code == "SP1234" && c.Description == "SP1234Description"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(specialCodes.Any(c => c.Code == "XD5678" && c.Description == "XD5678Description"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!specialCodes.Any(c => c.Code == "FR1234" && c.Description == "FR1234Description"), NUnit.Framework.Is.True);
				NUnit.Framework.Assert.That(!specialCodes.Any(c => c.Code == "US1234" && c.Description == "US1234Description"), NUnit.Framework.Is.True);
			});
		}
	}
}
