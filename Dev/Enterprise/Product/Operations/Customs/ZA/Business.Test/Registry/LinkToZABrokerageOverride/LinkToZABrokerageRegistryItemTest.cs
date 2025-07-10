using System;
using System.Text;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.Business.Testing
{
	[TestedType(typeof(LinkToZABrokerageRegistryItem))]
	sealed class LinkToZABrokerageRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		public void TestNewStringRegistryItem()
		{
			LinkToZABrokerageRegistryItem linkRegistryItem = new LinkToZABrokerageRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System);
			AssertEquals("Name", linkRegistryItem.Name);
			AssertEquals("Category", linkRegistryItem.Category);
			AssertEquals("Caption", linkRegistryItem.Caption);
			AssertEquals("Hint", linkRegistryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, linkRegistryItem.Storage);
		}

		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new LinkToZABrokerageRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(LinkToZABrokerageOverrideRegistryDataType))]
	sealed class LinkToZABrokerageOverrideRegistryDataTypeTest : RegistryDataTypeTestCase<LinkToZABrokerageOverrideRegistryDataType>
	{
		public void TestLinkToZABrokerageValdation()
		{
			var linkRegistryItem = new LinkToZABrokerageRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System);

			var dataType = (LinkToZABrokerageOverrideRegistryDataType)linkRegistryItem.DataType;
			AssertExceptionThrown(typeof(RegistryValidationException), "It should be entered in the following format: http://{SERVER_NAME}/cClearing/CWLink", delegate
				{
					dataType.Validate(linkRegistryItem, "abc", Guid.Empty, Guid.Empty, Guid.Empty);
				});

			AssertNoExceptionThrown(
				delegate
				{
					dataType.Validate(linkRegistryItem, "http://t.msn.com/", Guid.Empty, Guid.Empty, Guid.Empty);
				});
		}

		protected override LinkToZABrokerageOverrideRegistryDataType GetNewDataType()
		{
			return new LinkToZABrokerageOverrideRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB("http://t.msn.com/", Encoding.Unicode.GetBytes("http://t.msn.com/")),
				new ValidSampleAndBinaryValueInDB("https://www.google.com/", Encoding.Unicode.GetBytes("https://www.google.com/")),
				new ValidSampleAndBinaryValueInDB("ftp://some.more.stuff.andNumbers80808181", Encoding.Unicode.GetBytes("ftp://some.more.stuff.andNumbers80808181")) //Numbers & Caps
			};
		}

		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}
	}
}
