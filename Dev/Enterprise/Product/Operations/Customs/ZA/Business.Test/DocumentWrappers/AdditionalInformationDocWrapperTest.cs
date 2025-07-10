using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(AdditionalInformationDocWrapper))]
	sealed class AdditionalInformationDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstruction()
		{
			var tester = new AdditionalInformationDocWrapper("123123");
			AssertEquals("123", tester.Code);
			AssertEquals("123", tester.Value);

			tester = new AdditionalInformationDocWrapper("12");
			AssertEquals("12", tester.Code);
			AssertEquals("", tester.Value);

			tester = new AdditionalInformationDocWrapper("BND789");
			AssertEquals("BND", tester.Code);
			AssertEquals("789", tester.Value);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AdditionalInformationDocWrapper("");
		}
	}

	[TestedType(typeof(AdditionalInformationDocWrapperWithLineNumber))]
	sealed class AdditionalInformationDocWrapperWithLineNumberTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstruction()
		{
			var tester = new AdditionalInformationDocWrapperWithLineNumber(new AdditionalInformationDocWrapper("123123"), "X", ZDateTime.Today, Factory);
			AssertEquals("123", tester.Code);
			AssertEquals("123", tester.Value);
			AssertEquals("X", tester.LineNumber);

			tester = new AdditionalInformationDocWrapperWithLineNumber(new AdditionalInformationDocWrapper("BND456"), "Y", ZDateTime.Today, Factory);
			AssertEquals("BND", tester.Code);
			AssertEquals("456", tester.Value);
			AssertEquals("Y", tester.LineNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AdditionalInformationDocWrapperWithLineNumber(new AdditionalInformationDocWrapper("123"), "", ZDateTime.Today, Factory);
		}
	}
}
