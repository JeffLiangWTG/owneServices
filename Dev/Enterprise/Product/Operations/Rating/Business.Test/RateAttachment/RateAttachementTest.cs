using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateAttachment))]
	public class RateAttachementTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<Quote>();

			var set = header.AvailablePages.AddNew();
			set.TS_AttachmentName = "STDAttachment";
			set.TS_IsDefault = true;
			set.TS_IsMandatory = false;
			set.TS_TemplateType = RatingConstants.DocTemplateTypes.StandardPricingPage;
			set.TS_Sequence = 0;
			set.TS_SU = Factory.LoadTop1<DocumentCommand>(new ZQuery(StmMenuItemSchema.SU_BusinessContext, Constants.DataContext.Quotation)).PK;

			var attachment = factory.New<RateAttachment>();
			attachment.TA_TH = header.PK;
			attachment.TA_TS = set.PK;

			return attachment;
		}

		public void TestTA_RateAttachmentName()
		{
			var attachmentSet = Factory.New<RateAttachmentSet>();
			attachmentSet.TS_AttachmentName = "Test Document";
			string resKey = attachmentSet.TS_AttachmentNameInfo.CustomizableDataResourceStrings.GetMultilingualString(attachmentSet, "Test Document").ResourceKey;
			AssertEquals("Test Document", attachmentSet.TS_AttachmentName);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockData = Res.UseMockData())
			{
				mockData.Put(resKey, new ResourceStringData(resKey, "测试文档"));

				var attachment = Factory.New<RateAttachment>();
				attachment.TA_TS = attachmentSet.PK;
				AssertEquals("测试文档", attachment.TA_RateAttachmentName);
			}
		}
	}
}
