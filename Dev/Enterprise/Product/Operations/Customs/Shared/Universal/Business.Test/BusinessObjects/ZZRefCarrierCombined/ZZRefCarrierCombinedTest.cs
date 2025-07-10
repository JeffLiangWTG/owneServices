using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCarrierCombined))]
	class ZZRefCarrierCombinedTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDelete()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "B0B";
			carrier.ZZ4_Description = "BOB THE BUILDER";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Cambodia;
			var attribute1 = carrier.Attributes.AddNew("BOBAttribute", "SHORT");
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var carrierInDiffFactory = newFactory.Load<ZZRefCarrierCombined>(carrier.PK);
			carrierInDiffFactory.Delete();
			newFactory.Save();
			AssertEquals("carrier.IsDeleted", true, carrier.IsDeleted);
			AssertEquals("attribute1.IsDeleted", true, attribute1.IsDeleted);
			AssertEquals("attribute2.IsDeleted", true, attribute1.IsDeleted);
		}

		public void TestAutoLoggingIsEnabled()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "B0B";
			carrier.ZZ4_Description = "BOB THE BUILDER";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Cambodia;
			AssertNull(carrier.Logs.MostRecentLog);
			Factory.Save();
			AssertNotNull(carrier.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem));
		}

		public void TestCanDelete()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_IsSystem = false;
			AssertEquals("CanDelete", true, carrier.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "", carrier.ReasonForNotAbleToDelete);
			carrier.ZZ4_IsSystem = true;
			AssertEquals("CanDelete", false, carrier.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", ZZRefCarrierCombined.CannotDeleteSystemGenerated, carrier.ReasonForNotAbleToDelete);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var carrier = factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "B0B";
			carrier.ZZ4_Description = "BOB THE BUILDER";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Cambodia;
			return carrier;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		public void TestGetTransportModePropertyName()
		{
			ISet<string> transportList = new HashSet<string>();
			transportList.Add(ZZRefCarrierCombinedSchema.Constants.ZZ4_IsAir);
			transportList.Add(ZZRefCarrierCombinedSchema.Constants.ZZ4_IsRail);
			transportList.Add(ZZRefCarrierCombinedSchema.Constants.ZZ4_IsRoad);
			transportList.Add(ZZRefCarrierCombinedSchema.Constants.ZZ4_IsSea);
			foreach (CodeDescriptionPair transport in RefCarrierHelper.GetTransportModesList(Factory))
			{
				AssertEquals("TransportMode not found or deleted", true, transportList.Remove(ZZRefCarrierCombined.GetTransportModePropertyName(transport.Code)));
			}

			AssertEquals("Transport list is not empty", 0, transportList.Count);
		}

		public void TestITranslatableZZBusinessObjectIsImplemented()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			AssertEquals(1, carrier.GetType().FindInterfaces(new System.Reflection.TypeFilter((type, criteria) => type.ToString().Equals(criteria.ToString())), typeof(ITranslatableZZBusinessObject).ToString()).Length);
		}

		public void TestTranslatableValue()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			var carrierCodeLanguage1 = Factory.New<RefCarrierCodeLanguage>();
			carrierCodeLanguage1.ZCL_ZX6_NKLanguage = Core.Constants.CountryCodes.UnitedStates;
			carrierCodeLanguage1.ZCL_ZZ4_CarrierCode = carrier.PK;
			carrierCodeLanguage1.ZCL_Description = "Translate Test - US";
			var carrierCodeLanguage2 = Factory.New<RefCarrierCodeLanguage>();
			carrierCodeLanguage2.ZCL_ZX6_NKLanguage = Core.Constants.CountryCodes.KoreaSouth;
			carrierCodeLanguage2.ZCL_ZZ4_CarrierCode = carrier.PK;
			carrierCodeLanguage2.ZCL_Description = "번역 테스트 - 한국";

			carrier.ZZ4_Description = "Translate Test";
			var user = (GlbStaff)Env.CurrentUser;
			user.GS_WorkingLanguage = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("Translate Test - US", carrier.ZZ4_Description);
			AssertEquals("Translate Test", carrier.UnTranslatedZZ4_Description);

			user.GS_WorkingLanguage = Core.Constants.CountryCodes.KoreaSouth;
			AssertEquals("번역 테스트 - 한국", carrier.ZZ4_Description);
			AssertEquals("Translate Test", carrier.UnTranslatedZZ4_Description);
		}
	}
}
