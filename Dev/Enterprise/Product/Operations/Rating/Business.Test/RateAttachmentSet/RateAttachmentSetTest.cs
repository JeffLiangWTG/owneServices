using System.Drawing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateAttachmentSet))]
	sealed class RateAttachmentSetTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var attachmentSet = Factory.New<RateAttachmentSet>();

			AssertEquals(" Quotation Document Attachment", attachmentSet.HumanReadableName);

			attachmentSet.TS_AttachmentName = "Some Pricing Page";

			AssertEquals("Some Pricing Page Quotation Document Attachment", attachmentSet.HumanReadableName);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestDefaultIsSetWhenMandatory()
		{
			var testAttachmentSet = Factory.New<RateAttachmentSet>();
			AssertEquals("Is Mandatory", false, testAttachmentSet.TS_IsMandatory);
			AssertEquals("Is Default", false, testAttachmentSet.TS_IsDefault);

			testAttachmentSet.TS_IsMandatory = true;
			AssertEquals("Is Mandatory", true, testAttachmentSet.TS_IsMandatory);
			AssertEquals("Is Default", true, testAttachmentSet.TS_IsDefault);

			testAttachmentSet.TS_IsMandatory = false;
			AssertEquals("Is Mandatory", false, testAttachmentSet.TS_IsMandatory);
			AssertEquals("Is Default", true, testAttachmentSet.TS_IsDefault);

			testAttachmentSet.TS_IsDefault = false;
			AssertEquals("Is Mandatory", false, testAttachmentSet.TS_IsMandatory);
			AssertEquals("Is Default", false, testAttachmentSet.TS_IsDefault);

			testAttachmentSet.TS_IsMandatory = true;
			AssertEquals("Is Mandatory", true, testAttachmentSet.TS_IsMandatory);
			AssertEquals("Is Default", true, testAttachmentSet.TS_IsDefault);
		}

		public void TestSequenceGetsBumpedUpWhenChanging()
		{
			var set1 = Factory.New<RateAttachmentSet>();
			set1.TS_Sequence = (short)1;
			set1.TS_TemplateType = RatingConstants.DocTemplateTypes.CoverPage;
			var set2 = Factory.New<RateAttachmentSet>();
			set2.TS_Sequence = (short)2;
			set2.TS_TemplateType = RatingConstants.DocTemplateTypes.CoverPage;

			set2.TS_Sequence = (short)1;
			AssertEquals("Sequence number should be bumped up", (ZShort)2, set1.TS_Sequence);

			set1.TS_TemplateType = RatingConstants.DocTemplateTypes.OneOffPricingPage;
			set1.TS_Sequence = (short)1;
			AssertEquals("Sequence number should not be bumped up as the template type hasn't changed", (ZShort)1, set2.TS_Sequence);

			set1.TS_TemplateType = RatingConstants.DocTemplateTypes.CoverPage;
			AssertEquals("Sequence number should be bumped up as the template type has changed back", (ZShort)2, set2.TS_Sequence);
		}

		public void TestCantModifyWhenSystemDefined()
		{
			var set = Factory.New<RateAttachmentSet>();

			set.TS_IsSystemDefined = false;
			set.TS_IsClientSpecific = false;
			AssertEquals(false, set.TS_AttachmentNameInfo.ReadOnly);
			AssertEquals(false, set.TS_SUInfo.ReadOnly);
			AssertEquals(false, set.TS_TemplateTypeInfo.ReadOnly);
			AssertEquals(false, set.TS_GCInfo.ReadOnly);

			set.TS_IsSystemDefined = true;
			AssertEquals(true, set.TS_AttachmentNameInfo.ReadOnly);
			AssertEquals(true, set.TS_SUInfo.ReadOnly);
			AssertEquals(true, set.TS_TemplateTypeInfo.ReadOnly);
			AssertEquals(true, set.TS_GCInfo.ReadOnly);

			set.TS_IsSystemDefined = false;
			set.TS_IsClientSpecific = true;
			AssertEquals(true, set.TS_AttachmentNameInfo.ReadOnly);
			AssertEquals(true, set.TS_SUInfo.ReadOnly);
			AssertEquals(true, set.TS_TemplateTypeInfo.ReadOnly);
			AssertEquals(true, set.TS_GCInfo.ReadOnly);
		}

		public void TestCannotMakePricingPageMandatory()
		{
			var set = Factory.New<RateAttachmentSet>();
			set.TS_TemplateType = RatingConstants.DocTemplateTypes.StandardPricingPage;
			set.TS_IsMandatory = true;
			AssertEquals("A pricing page cannot be mandatory", true, set.TS_IsMandatoryInfo.HasErrors());

			set.TS_IsMandatory = false;
			AssertEquals("A pricing page cannot be mandatory", false, set.TS_IsMandatoryInfo.HasErrors());

			set.TS_IsMandatory = true;
			AssertEquals("A pricing page cannot be mandatory", true, set.TS_IsMandatoryInfo.HasErrors());

			set.TS_TemplateType = RatingConstants.DocTemplateTypes.CoverPage;
			AssertEquals("A pricing page cannot be mandatory", false, set.TS_IsMandatoryInfo.HasErrors());

			set.TS_TemplateType = RatingConstants.DocTemplateTypes.OneOffPricingPage;
			AssertEquals("A pricing page cannot be mandatory", true, set.TS_IsMandatoryInfo.HasErrors());
		}

		public void TestDefaultCanBeOnMoreThanOnePricingTemplateType()
		{
			var pricingSet1 = Factory.NewWithValidTestData<RateAttachmentSet>();
			var pricingSet2 = Factory.NewWithValidTestData<RateAttachmentSet>();
			pricingSet1.TS_IsDefault = true;
			pricingSet2.TS_IsDefault = true;
			Factory.Save();
			AssertEquals("Default sets ok on non-pricing pages", true, pricingSet1.TS_IsDefault);
			AssertEquals("Default sets ok on non-pricing pages", true, pricingSet2.TS_IsDefault);

			pricingSet1.TS_TemplateType = RatingConstants.DocTemplateTypes.StandardPricingPage;
			Factory.Save();
			pricingSet2.TS_TemplateType = RatingConstants.DocTemplateTypes.StandardPricingPage;
			Factory.Save();
			AssertEquals("Should have 2 default pricing sets", true, pricingSet1.TS_IsDefault);
			AssertEquals("Should have 2 default pricing sets", true, pricingSet2.TS_IsDefault);

			pricingSet1.TS_IsDefault = true;
			Factory.Save();
			AssertEquals("Should have 2 default pricing sets", true, pricingSet1.TS_IsDefault);
			AssertEquals("Should have 2 default pricing sets", true, pricingSet2.TS_IsDefault);
		}

		public void TestImageProperties()
		{
			var set = Factory.New<RateAttachmentSet>();
			Assert(!set.IsImage);

			set.TS_TemplateType = RatingConstants.DocTemplateTypes.TrailingPage;
			Assert(!set.IsImage);

			set.TS_AttachmentName = "Trailing Page XX";
			Assert(!set.IsImage);
			AssertEquals(set.NoPreviewImage, set.Image);

			set.TS_AttachmentName = "Trailing Page 3";
			Assert(set.IsImage);
			AssertNull(set.Image);

			Env.Registry.Rating.SetQuoteTermsAndConditionsPages(new Image[] { null, null, new Bitmap(100, 100), null, null, null, null, null, null, new Bitmap(101, 101) });
			set = new BusinessObjectFactory().New<RateAttachmentSet>();
			set.TS_TemplateType = RatingConstants.DocTemplateTypes.TrailingPage;
			set.TS_AttachmentName = "Trailing Page 3";
			Assert(set.IsImage);
			AssertEquals(100, set.Image.Width);

			set.TS_AttachmentName = "Trailing Page 10";
			Assert(set.IsImage);
			AssertEquals(101, set.Image.Width);

			set.TS_AttachmentName = "Trailing Page 11";
			Assert(set.IsImage);
			AssertEquals(set.NoPreviewImage, set.Image);
		}

		public void TestTS_AttachmentNameMultilingual()
		{
			var attachmentSet = Factory.New<RateAttachmentSet>();
			attachmentSet.TS_AttachmentName = "Test Document";
			string resKey = attachmentSet.TS_AttachmentNameInfo.CustomizableDataResourceStrings.GetMultilingualString(attachmentSet, "Test Document").ResourceKey;
			AssertEquals("Test Document", attachmentSet.TS_AttachmentName);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockData = Res.UseMockData())
			{
				mockData.Put(resKey, new ResourceStringData(resKey, "测试文档"));
				AssertEquals("测试文档", attachmentSet.TS_AttachmentNameMultilingual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}
	}
}
