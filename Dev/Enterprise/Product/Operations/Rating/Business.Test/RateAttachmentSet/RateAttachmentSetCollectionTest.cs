using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateAttachmentSetCollection))]
	internal sealed class RateAttachmentSetCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRateAttachmentSetCollection()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "ABC";

			var company2 = Factory.New<GlbCompany>();
			company1.GC_Code = "CBA";

			var rateAttachmentSet1 = CreateRateAttachmentSet(company1.PK, "Cover1", RatingConstants.DocTemplateTypes.CoverPage);
			var rateAttachmentSet2 = CreateRateAttachmentSet(company2.PK, "Cover2", RatingConstants.DocTemplateTypes.CoverPage);
			var rateAttachmentSet3 = CreateRateAttachmentSet(ZGuid.Empty, "Cover3", RatingConstants.DocTemplateTypes.CoverPage);

			testQuote.TH_GC = company1.PK;

			Factory.Save();

			var rateAttachmentSetCollection = new RateAttachmentSetCollection(testQuote, Factory);

			rateAttachmentSetCollection.Load();

			AssertCollectionContains("RateAttachmentSetCollection should contain the RateAttachmentSet1", rateAttachmentSet1, rateAttachmentSetCollection);
			AssertCollectionContains("RateAttachmentSetCollection should contain the RateAttachmentSet3", rateAttachmentSet3, rateAttachmentSetCollection);
			AssertCollectionNotContains("RateAttachmentSetCollection should not contain the RateAttachmentSet2", rateAttachmentSet2, rateAttachmentSetCollection);
		}

		#region Implementation

		Quote testQuote;

		protected override void SetUp()
		{
			base.SetUp();
			testQuote = Factory.New<Quote>();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RateAttachmentSetCollection(Factory);
		}

		RateAttachmentSet CreateRateAttachmentSet(ZGuid companyPK, ZString name, ZString templateType)
		{
			var rateAttachmentSet = testQuote.AvailablePages.AddNew();
			rateAttachmentSet.TS_AttachmentName = name;
			rateAttachmentSet.TS_IsDefault = false;
			rateAttachmentSet.TS_IsMandatory = false;
			rateAttachmentSet.TS_TemplateType = templateType;
			rateAttachmentSet.TS_Sequence = 0;
			if (companyPK != ZGuid.Empty)
			{
				rateAttachmentSet.TS_GC = companyPK;
			}

			rateAttachmentSet.TS_SU = Factory.LoadTop1<DocumentCommand>(new ZQuery(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Quotation)).PK;

			return rateAttachmentSet;
		}

		#endregion
	}
}
