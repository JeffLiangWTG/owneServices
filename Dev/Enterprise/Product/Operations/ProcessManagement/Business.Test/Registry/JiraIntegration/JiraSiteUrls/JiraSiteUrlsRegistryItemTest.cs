using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test.Registry.JiraIntegration.JiraSiteUrls
{
	class JiraSiteUrlsRegistryItemTest : TestCaseWithFactory
	{
		public void TestValidation_SystemCodesMustBeUnique()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("SYS", "https://carrie.com");
			list.AddPair("SYS", "https://saul.com");

			AssertExceptionThrown<RegistryValidationException>("Systems must be unique. SAD!", "The code 'SYS' has been duplicated. Please enter a unique code.", () => ValidateRegistryItem(list));

			list.RemoveAt(1);
			list.AddPair("ABC", "https://saul.com");

			AssertNoExceptionThrown(() => ValidateRegistryItem(list));
		}

		public void TestValidation_UrlsMustBeUnique()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("SYS", "https://carrie.com");
			list.AddPair("ABC", "https://carrie.com");

			AssertExceptionThrown<RegistryValidationException>("Urls must be unique. SAD!", "The description 'https://carrie.com' has been duplicated. Please enter a unique Description.", () => ValidateRegistryItem(list));

			list.RemoveAt(1);
			list.AddPair("ABC", "https://saul.com");

			AssertNoExceptionThrown(() => ValidateRegistryItem(list));
		}

		public void TestValidation_UrlsMustBeValid()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("SYS", "http://carrie.com");

			AssertExceptionThrown<RegistryValidationException>("https:// required", "Only HTTPS protocol is supported.", () => ValidateRegistryItem(list));

			list.RemoveAt(0);
			list.AddPair("SYS", "https://www.creedthoughts .gov.www/creedthoughts");

			AssertExceptionThrown<RegistryValidationException>("Need a well-formed uri.", "HTTPS Address is not well formed.", () => ValidateRegistryItem(list));

			list.RemoveAt(0);
			list.AddPair("SYS", "https://www.creedthoughts.com");

			AssertNoExceptionThrown(() => ValidateRegistryItem(list));
		}

		public void TestValidation_WhenLinksExistForSystemInDatabase_MustNotAllowChanges()
		{
			var registryItem = ProcessManagementRegistry.Instance.JiraSiteUrls;
			var list = new CodeDescriptionPairList();
			list.AddPair("SYS", "https://mainsystem.com");
			SetRegistryItemAndSimulateDeserialisation(registryItem, list);

			AssertNoExceptionThrown("Adding the first system when there are no links should be allowed. SAD!", () => ValidateRegistryItem(list, registryItem));

			var linkable = new DummyExternalEntityLinkable("12345", "WKI");
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			ExternalEntityLinkHelper.CreateLink(linkable, workItem, "SYS");
			Factory.Save();

			AssertNoExceptionThrown("Adding a link for an existing system should be allowed. SAD!", () => ValidateRegistryItem(list, registryItem));

			list.AddPair("OTH", "https://othersystem.com");
			AssertNoExceptionThrown("Adding an additional system when there are links for an other system in the database already should be allowed. SAD!", () => ValidateRegistryItem(list, registryItem));

			SetRegistryItemAndSimulateDeserialisation(registryItem, list);

			var listWithMissingSystem = new CodeDescriptionPairList();
			list.AddPair("OTH", "https://othersystem.com");
			AssertExceptionThrown<RegistryValidationException>("Systems that already have links in the database can't be removed. SAD!", "There are imported items linked to the [SYS] system. Changes cannot be made to the Code or URL for systems that have imported links already.", () => ValidateRegistryItem(listWithMissingSystem, registryItem));

			list.RemoveAt(1);
			AssertNoExceptionThrown("Removing a system that doesn't already have links in the database should be allowed. SAD!", () => ValidateRegistryItem(list, registryItem));

			SetRegistryItemAndSimulateDeserialisation(registryItem, list);

			var listWithModifiedUrl = new CodeDescriptionPairList();
			listWithModifiedUrl.AddPair("SYS", "https://thisisamodifiedurl.com");
			AssertExceptionThrown<RegistryValidationException>("Systems that already have links in the database can't have their urls changed. SAD!", "There are imported items linked to the [SYS] system. Changes cannot be made to the Code or URL for systems that have imported links already.", () => ValidateRegistryItem(listWithModifiedUrl, registryItem));
		}

		#region Implementation

		static void SetRegistryItemAndSimulateDeserialisation(CodeDescriptionPairListRegistryItem registryItem, CodeDescriptionPairList list)
		{
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			((JiraSiteUrlsRegistryDataType)registryItem.DataType).SetDeserializedList_ForTest(list);
		}

		static void ValidateRegistryItem(CodeDescriptionPairList list)
		{
			var registryItem = ProcessManagementRegistry.Instance.JiraSiteUrls;
			ValidateRegistryItem(list, registryItem);
		}

		static void ValidateRegistryItem(CodeDescriptionPairList list, CodeDescriptionPairListRegistryItem registryItem)
		{
			registryItem.DataType.Validate(registryItem, list, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		#endregion
	}

	[TestedType(typeof(JiraSiteUrlsRegistryDataType))]
	class JiraSiteUrlsRegistryItemMandatoryTest : RegistryDataTypeTestCase<JiraSiteUrlsRegistryDataType>
	{
		protected override JiraSiteUrlsRegistryDataType GetNewDataType()
		{
			return new JiraSiteUrlsRegistryDataType(3);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var list1 = new CodeDescriptionPairList();
			var list2 = new CodeDescriptionPairList();

			list1.AddPair("ABC", "https://wisetech.com");
			list2.AddPair("123", "https://cargowise.com");

			var list1ReadOnly = new ReadOnlyCodeDescriptionPairList(list1);
			var list2ReadOnly = new ReadOnlyCodeDescriptionPairList(list2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(list1ReadOnly, new byte[] { 60,78,101,119,68,97,116,97,83,101,116,62,13,10,32,32,60,84,97,98,108,101,49,62,13,10,32,32,32,32,60,67,111,100,101,62,65,66,67,60,47,67,111,100,101,62,13,10,32,32,32,32,60,68,101,115,99,114,105,112,116,105,111,110,62,104,116,116,112,115,58,47,47,119,105,115,101,116,101,99,104,46,99,111,109,60,47,68,101,115,99,114,105,112,116,105,111,110,62,13,10,32,32,60,47,84,97,98,108,101,49,62,13,10,60,47,78,101,119,68,97,116,97,83,101,116,62 }),
				new ValidSampleAndBinaryValueInDB(list2ReadOnly, new byte[] { 60,78,101,119,68,97,116,97,83,101,116,62,13,10,32,32,60,84,97,98,108,101,49,62,13,10,32,32,32,32,60,67,111,100,101,62,49,50,51,60,47,67,111,100,101,62,13,10,32,32,32,32,60,68,101,115,99,114,105,112,116,105,111,110,62,104,116,116,112,115,58,47,47,99,97,114,103,111,119,105,115,101,46,99,111,109,60,47,68,101,115,99,114,105,112,116,105,111,110,62,13,10,32,32,60,47,84,97,98,108,101,49,62,13,10,60,47,78,101,119,68,97,116,97,83,101,116,62 }),
			};
		}
	}
}
