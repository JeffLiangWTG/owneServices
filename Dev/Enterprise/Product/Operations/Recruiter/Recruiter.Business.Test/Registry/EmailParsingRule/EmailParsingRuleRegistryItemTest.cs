using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(EmailParsingRuleRegistryItem))]
	public class EmailParsingRuleRegistryItemTest : StronglyTypedRegistryItemTestCase<EmailParsingRuleCollection>
	{
		protected override StronglyTypedRegistryItem<EmailParsingRuleCollection, EmailParsingRuleCollection> GetNewRegistryItem()
		{
			return new EmailParsingRuleRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new EmailParsingRuleCollection());
		}
	}

	[TestedType(typeof(EmailParsingRuleRegistryDataType))]
	public class EmailParsingRuleRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EmailParsingRuleRegistryDataType>
	{
		#region Implementation

		protected override EmailParsingRuleRegistryDataType GetNewDataType()
		{
			return new EmailParsingRuleRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "EmailParsingRuleRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new EmailParsingRuleCollection();
			var rule1 = collection.AddNew();
			rule1.ReferringPartyCode = "OH";
			rule1.AllowParseAttachments = true;
			rule1.AllowFallbackToEmailBody = true;

			var collection2 = new EmailParsingRuleCollection();
			var rule2 = collection.AddNew();
			rule2.ReferringPartyCode = "GS";
			rule2.AllowParseAttachments = true;
			rule2.AllowFallbackToEmailBody = false;
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, DataType.Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, DataType.Serialise(collection2))
			};
		}

		#endregion Implementation
	}
}
