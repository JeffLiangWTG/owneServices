using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ReferringPartyConfigurationRegistryItem))]
	public class ReferringPartyConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<ReferringPartyConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<ReferringPartyConfigurationCollection, ReferringPartyConfigurationCollection> GetNewRegistryItem()
		{
			return new ReferringPartyConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(ReferringPartyConfigurationRegistryDataType))]
	class OrgSecurityProfileDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ReferringPartyConfigurationRegistryDataType>
	{
		#region Implementation

		protected override ReferringPartyConfigurationRegistryDataType GetNewDataType()
		{
			return new ReferringPartyConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ReferringPartyConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new ReferringPartyConfigurationCollection();
			var cfg1 = collection.AddNew();
			cfg1.Domain = "@cw1.com";
			cfg1.ReferringParty = "OH";
			cfg1.OrganizationPK = Org1.PK;
			cfg1.DefaultReferringSource = "WEB";

			var collection2 = new ReferringPartyConfigurationCollection();
			var cfg2 = collection2.AddNew();
			cfg2.Domain = "@cw2.com";
			cfg2.ReferringParty = "GS";
			cfg2.OrganizationPK = ZGuid.Empty;
			cfg2.DefaultReferringSource = "STF";
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}

		OrgHeader Org1;

		protected override void SetUp()
		{
			base.SetUp();
			Org1 = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			Org1.OH_Code = "TEST_ORG1";
			Org1.Factory.Save();
		}

		#endregion Implementation
	}
}
