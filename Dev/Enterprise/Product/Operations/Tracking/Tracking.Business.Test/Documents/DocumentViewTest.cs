using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(DocumentView))]
	sealed class DocumentViewTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDescriptionMaxLength()
		{
			DocumentView testDocView = new DocumentView(Factory);
			ZString original = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			Assert(original.Length > testDocView.DescriptionInfo.MaxLength);
			testDocView.Description = (NoResString)original;
			Assert(testDocView.Description.GetUnresolvedString().Length == testDocView.DescriptionInfo.MaxLength);
		}

		public void TestRT_Desc()
		{
			RefDocType testDocType = Factory.NewWithValidTestData<RefDocType>();
			testDocType.RT_DocType = "TT1";
			testDocType.RT_Desc = "test description1";

			RefDocType testDocType2 = Factory.NewWithValidTestData<RefDocType>();
			testDocType2.RT_DocType = "TT2";
			testDocType2.RT_Desc = "test description2";

			DocumentView testDocView = new DocumentView(Factory);
			Assert("RT_Desc should be empty", testDocView.RT_Desc.IsEmpty);

			testDocView.DocType = "TT1";
			AssertEquals("test description1", testDocView.RT_Desc);

			testDocView.DocType = "TT2";
			AssertEquals("test description2", testDocView.RT_DescInfo.Value);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var testDocView = new DocumentView(Factory);
			testDocView.DocType = "MSC";
			testDocView.Description = (NoResString)"Bla";
			return testDocView;
		}
	}
}
