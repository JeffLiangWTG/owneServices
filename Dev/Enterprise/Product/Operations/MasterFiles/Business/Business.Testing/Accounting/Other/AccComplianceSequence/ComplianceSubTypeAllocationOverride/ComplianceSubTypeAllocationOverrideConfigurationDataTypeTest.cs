using System;
using System.Linq;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAllocationOverrideConfigurationRegistryDataType))]
	sealed class ComplianceSubTypeAllocationOverrideConfigurationDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ComplianceSubTypeAllocationOverrideConfigurationRegistryDataType>
	{
		#region Implementation

		protected override ComplianceSubTypeAllocationOverrideConfigurationRegistryDataType GetNewDataType()
		{
			return new ComplianceSubTypeAllocationOverrideConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ComplianceSubTypeAllocationOverrideConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var fallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var brCollection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(fallbackLevel, null, CountryCodes.Brazil);
			var item1 = brCollection.AddNew();
			item1.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
			item1.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
			var item2 = brCollection.AddNew();
			item2.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			item2.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
			item2.BranchPK = GlbCompany.CurrentCompany.Branches.First(b => b.GB_Code == "SYD").PK;

			var brXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfComplianceSubTypeAllocationOverrideConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<ComplianceSubTypeAllocationOverrideConfiguration>
		<Country>BR</Country>
		<SubType>CNS</SubType>
		<AllocationMethod>PST</AllocationMethod>
		<BranchPK>00000000-0000-0000-0000-000000000000</BranchPK>
	</ComplianceSubTypeAllocationOverrideConfiguration>
	<ComplianceSubTypeAllocationOverrideConfiguration>
		<Country>BR</Country>
		<SubType>XNC</SubType>
		<AllocationMethod>MAN</AllocationMethod>
		<BranchPK>FDD429D2-648C-4895-8F9F-06E90DED2BE5</BranchPK>
	</ComplianceSubTypeAllocationOverrideConfiguration>
</ArrayOfComplianceSubTypeAllocationOverrideConfiguration>";

			var inCollection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(fallbackLevel, null, CountryCodes.India);
			var item = inCollection.AddNew();
			item.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.XCD;
			item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print;

			var inXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfComplianceSubTypeAllocationOverrideConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<ComplianceSubTypeAllocationOverrideConfiguration>
		<Country>IN</Country>
		<SubType>XCD</SubType>
		<AllocationMethod>PRN</AllocationMethod>
		<BranchPK>00000000-0000-0000-0000-000000000000</BranchPK>
	</ComplianceSubTypeAllocationOverrideConfiguration>
</ArrayOfComplianceSubTypeAllocationOverrideConfiguration>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(brCollection, brXml),
				new ValidSampleAndBinaryValueInDB(inCollection, inXml),
			};
		}

		#endregion
	}
}
