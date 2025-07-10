using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityProfileDataType))]
	sealed class OrgSecurityProfileDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OrgSecurityProfileDataType>
	{
		#region Implementation

		protected override OrgSecurityProfileDataType GetNewDataType()
		{
			return new OrgSecurityProfileDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "OrgSecurityProfileRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new OrgSecurityProfileCollection();
			var profile1 = collection.AddNew();
			profile1.Name = "Default";
			profile1.Default = true;
			profile1.OrgSecuritySettings.PopulateDefaultSettings();

			var collection2 = new OrgSecurityProfileCollection();
			var profile2 = collection.AddNew();
			profile2.Name = "Default2";
			profile2.Default = true;
			profile2.OrgSecuritySettings.PopulateDefaultSettings();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}

		#endregion Implementation
	}
}
