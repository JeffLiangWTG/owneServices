using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAllocationOverrideConfigurationCollection))]
	sealed class ComplianceSubTypeAllocationOverrideConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ComplianceSubTypeAllocationOverrideConfigurationCollection>
	{
		public void TestConstructor_SetsCountryCode()
		{
			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection();
			AssertNullOrEmpty(collection.CountryCode);

			var auCollection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, CountryCodes.Australia);
			AssertEquals(CountryCodes.Australia, auCollection.CountryCode);

			var emojiCollection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, "😊");
			AssertEquals("😊", emojiCollection.CountryCode);
		}

		public void TestClone_SetsCountryCode_FromCompanyFallbackLevel()
		{
			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection();
			AssertNullOrEmpty("When deserialising an existing registry, the CountryCode may be empty", collection.CountryCode);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.NewZealand;
			var companyFallback = new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			Factory.Save();

			var clone = (ComplianceSubTypeAllocationOverrideConfigurationCollection)collection.Clone(companyFallback, Factory);
			AssertEquals("Clone() loads country code from fallback", CountryCodes.NewZealand, clone.CountryCode);
			AssertEquals("Clone() sets original collection country code from fallback, so that CountryCode is correctly set when deserialising", CountryCodes.NewZealand, collection.CountryCode);
		}

		public void TestGetAllocationMethod()
		{
			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(NewFallbackLevel(), Factory, CountryCodes.India);
			var branchTXIItem = collection.AddNew();
			branchTXIItem.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			branchTXIItem.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			branchTXIItem.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

			var subTypeTXIItem = collection.AddNew();
			subTypeTXIItem.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			subTypeTXIItem.BranchPK = ZGuid.Empty;
			subTypeTXIItem.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			var subTypeTXCItem = collection.AddNew();
			subTypeTXCItem.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXC;
			subTypeTXCItem.BranchPK = ZGuid.Empty;
			subTypeTXCItem.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print;

			var foundItem = collection.GetMatchingConfiguration(GlbCompany.CurrentCompany.FirstActiveBranch.PK, IndiaComplianceInfo.ComplianceSubTypeCodes.TXI);
			AssertEquals("Should match most specific rule by branch and sub-type", foundItem.PK, branchTXIItem.PK);

			foundItem = collection.GetMatchingConfiguration(ZGuid.NewZGuid(), IndiaComplianceInfo.ComplianceSubTypeCodes.TXI);
			AssertEquals("Should match rule by sub-type and not branch", foundItem.PK, subTypeTXIItem.PK);

			foundItem = collection.GetMatchingConfiguration(ZGuid.NewZGuid(), IndiaComplianceInfo.ComplianceSubTypeCodes.TXC);
			AssertEquals("Should match rule by sub-type alone", foundItem.PK, subTypeTXCItem.PK);

			foundItem = collection.GetMatchingConfiguration(ZGuid.NewZGuid(), IndiaComplianceInfo.ComplianceSubTypeCodes.RVD);
			AssertNull("Should match nothing", foundItem);

			foundItem = collection.GetMatchingConfiguration(GlbCompany.CurrentCompany.FirstActiveBranch.PK, ZString.Empty);
			AssertNull("Should match nothing when no sub-type", foundItem);

			foundItem = collection.GetMatchingConfiguration(ZGuid.Empty, IndiaComplianceInfo.ComplianceSubTypeCodes.TXC);
			AssertNull("Should match nothing when empty branch", foundItem);

			foundItem = collection.GetMatchingConfiguration(ZGuid.Invalid, IndiaComplianceInfo.ComplianceSubTypeCodes.TXC);
			AssertNull("Should match nothing when invalid branch", foundItem);
		}

		#region Implementation

		protected override ComplianceSubTypeAllocationOverrideConfigurationCollection GetCollectionToTest()
		{
			return new ComplianceSubTypeAllocationOverrideConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, "US");
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
