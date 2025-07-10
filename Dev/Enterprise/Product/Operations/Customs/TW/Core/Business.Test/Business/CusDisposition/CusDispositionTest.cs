using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusDisposition))]
	sealed class CusDispositionTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestCDI_StatusKey()
		{
			var disposition = Factory.New<CusDisposition>();
			BusinessObjectCaptionTestHelper.AssertCaptions(disposition.CDI_StatusKeyInfo, "Type");
		}

		[ExpectNoExceptions]
		public void TestCDI_Status()
		{
			var disposition = Factory.New<CusDisposition>();
			BusinessObjectCaptionTestHelper.AssertCaptions(disposition.CDI_StatusInfo, "Status");
		}

		[ExpectNoExceptions]
		public void TestCDI_StatusDate()
		{
			var disposition = Factory.New<CusDisposition>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(disposition.CDI_StatusDateInfo, "Status Date", "Date");
		}

		[ExpectNoExceptions]
		public void TestStatusDescription()
		{
			var disposition = Factory.New<CusDisposition>();
			BusinessObjectCaptionTestHelper.CombineAssertCaptionsWithFullDescription(disposition.StatusDescriptionInfo, "Status Description", "Status Desc.", "Desc.", "");

			disposition.CDI_StatusKey = "RFM";
			disposition.CDI_Status = "A01";
			NUnit.Framework.Assert.That(disposition.StatusDescription, NUnit.Framework.Is.EqualTo("請補送報單及必備之有關文件C2、C3").Using(CustomComparers.TypeComparison));

			disposition.CDI_StatusKey = "ARM";
			NUnit.Framework.Assert.That(disposition.StatusDescription, NUnit.Framework.Is.EqualTo("未能核銷艙單").Using(CustomComparers.TypeComparison));

			disposition.CDI_StatusKey = "CLR";
			disposition.CDI_Status = "1";
			NUnit.Framework.Assert.That(disposition.StatusDescription, NUnit.Framework.Is.EqualTo("押運").Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var disposition = new CusDispositionCollection(Factory.NewWithValidTestData<CusEntryHeader>()).AddNew();
			disposition.CDI_Type = "CUS";
			disposition.CDI_StatusKey = "RFM";
			disposition.CDI_Status = "A01";
			disposition.CDI_StatusDate = ZDateTime.Now;
			return disposition;
		}
	}
}
