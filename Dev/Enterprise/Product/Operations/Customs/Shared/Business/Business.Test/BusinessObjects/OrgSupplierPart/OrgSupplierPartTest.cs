using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(OrgSupplierPart))]
	public class OrgSupplierPartTest : MasterFiles.Business.Testing.OrgSupplierPartTest
	{
		#region TestTemplateCopy

		protected override void SubclassSetupForTestITemplateCopyable(MasterFiles.Business.OrgSupplierPart part)
		{
			AssertEquals("PreCondition", 2, part.RelatedOrganisations.Count);
			BaseCusClassification class0 = Factory.New<BaseCusClassification>();
			class0.FillWithValidTestData();

			BaseCusClassification class1 = Factory.New<BaseCusClassification>();
			class1.FillWithValidTestData();

			BaseCusClassPartPivot partPivot0 = Factory.New<BaseCusClassPartPivot>();
			partPivot0.FillWithValidTestData();
			partPivot0.CI_OP = part.PK;
			partPivot0.CI_OH = part.RelatedOrganisations[0].OU_OH;
			partPivot0.CI_UsageComment = "1";
			partRelOrgPK1 = part.RelatedOrganisations[0].OU_OH;

			BaseCusClassPartPivot partPivot1 = Factory.New<BaseCusClassPartPivot>();
			partPivot1.FillWithValidTestData();
			partPivot1.CI_OP = part.PK;
			partPivot1.CI_OH = part.RelatedOrganisations[1].OU_OH;
			partPivot1.CI_UsageComment = "2";
			partRelOrgPK2 = part.RelatedOrganisations[1].OU_OH;
		}
		ZGuid partRelOrgPK1;
		ZGuid partRelOrgPK2;

		protected override void SubclassAssertForTestITemplateCopyable(MasterFiles.Business.OrgSupplierPart clonedPart)
		{
			AssertNotEquals("PreCondition", ZGuid.Empty, partRelOrgPK1);
			AssertNotEquals("PreCondition", ZGuid.Empty, partRelOrgPK2);
			ZQuery partPivotsfilter = new ZQuery(CusClassPartPivotSchema.CI_OP, clonedPart.PK);
			BaseCusClassPartPivot[] partPivots = Factory.Load<BaseCusClassPartPivot>(partPivotsfilter);
			AssertEquals(2, partPivots.Length);
			BaseCusClassPartPivot pivot1 = partPivots[0];
			BaseCusClassPartPivot pivot2 = partPivots[1];
			if (pivot1.CI_UsageComment == "2")
			{
				pivot1 = partPivots[1];
				pivot2 = partPivots[0];
			}
			AssertEquals("1", pivot1.CI_UsageComment);
			AssertEquals(partRelOrgPK1, pivot1.CI_OH);
			AssertEquals("2", pivot2.CI_UsageComment);
			AssertEquals(partRelOrgPK2, pivot2.CI_OH);
		}

		public void TestTemplateCopyPivots()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "AU";
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.PivotsForBinding.AddNew();
			Factory.Save();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			var usFactory = new BusinessObjectFactory();
			var usPart = usFactory.Load<OrgSupplierPart>(part.PK) as ITemplateCopyable;
			var copy = usPart.TemplateCopy() as OrgSupplierPart;
			AssertEquals("AU pivot was not copied", 0, copy.PivotsForBinding.Count);
		}

		#endregion

		#region TestTypeDecider
		public void TestTypeDecider()
		{
			var part = Factory.NewWithValidTestData<MasterFiles.Business.OrgSupplierPart>();
			Assert("Update MasterFiles.Business.OrgSupplierPart to include a decider for this class", GetExpectedBusinessObjectType().IsAssignableFrom(part.GetType()));
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var anotherCountry = GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Australia ? Core.Constants.CountryCodes.UnitedStates : Core.Constants.CountryCodes.Australia;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(anotherCountry))
			{
				AssertNoExceptionThrown("Should load as expected type", () => anotherFactory.Load(GetExpectedBusinessObjectType(), new ZQuery(OrgSupplierPartSchema.OP_PartNum, part.OP_PartNum)));
			}
		}
		#endregion

		#region TestDeletedByDataRefresh
		public void TestDeletedByDataRefresh()
		{
			OrgSupplierPart part = (OrgSupplierPart)GetNewBusinessObject();
			part.OP_PartNum = "PartNumDeletedByData";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgSupplierPart partReloaded = factory2.Load<OrgSupplierPart>(part.PK);

			partReloaded.DeletedByDataRefresh += new OrgSupplierPart.DeletedHandler(PartReloaded_DeletedByDataRefresh);

			part.Delete();
			AssertEquals("DeletedCallCount", 0, deletedCallCount);
			AssertEquals("Part should be deleted.", true, part.IsDeleted);
			AssertEquals("Part should have changes.", true, part.HasChanges);
			AssertEquals("PartReloaded should not be deleted yet.", false, partReloaded.IsDeleted);
			AssertEquals("Datarefresh should be enabled", true, Factory.RefreshEnabled);
			AssertEquals("Datarefresh should be enabled", true, factory2.RefreshEnabled);

			Factory.Save();
			AssertEquals("PartReloaded should have been deleted by the data refresh bus.", true, partReloaded.IsDeleted);
			AssertEquals("DeletedCallCount", 1, deletedCallCount);
		}

		int deletedCallCount;
		void PartReloaded_DeletedByDataRefresh(OrgSupplierPart part)
		{
			deletedCallCount++;
		}
		#endregion

		#region TestClassifications

		public void TestClassifications()
		{
			OrgSupplierPart part = (OrgSupplierPart)GetNewBusinessObject();
			Assert("Part.Classifications.GetType()", ExpectedClassificationCollectionType.IsAssignableFrom(part.ClassificationsForBinding.GetType()));
		}

		protected virtual Type ExpectedClassificationCollectionType
		{
			get { return typeof(ClassificationCollection<BaseCusClassification>); }
		}
		#endregion

		public void TestPivots()
		{
			OrgSupplierPart part = (OrgSupplierPart)GetNewBusinessObject();
			BaseCusClassPartPivot pivotCurrentCountry = Factory.New<BaseCusClassPartPivot>();
			pivotCurrentCountry.CI_RN_NKCountry = GlbCompany.CurrentCompany.Country.RN_Code;
			pivotCurrentCountry.CI_OP = part.PK;

			BaseCusClassPartPivot pivotAnotherCountry = Factory.New<BaseCusClassPartPivot>();
			pivotAnotherCountry.CI_RN_NKCountry = Core.Constants.CountryCodes.Afghanistan;
			pivotAnotherCountry.CI_OP = part.PK;

			AssertNotNull(part.PivotsForBinding);
			AssertEquals(1, part.PivotsForBinding.Count);
			AssertEquals(pivotCurrentCountry.PK, part.PivotsForBinding[0].PK);
		}

		public void TestCanDelete()
		{
			ZGuid pk;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "ORG";

				var product = Factory.New<OrgSupplierPart>();
				product.OP_PartNum = "PART";
				product.OP_Desc = "Desc.";
				pk = product.PK;

				var caPivot = product.PivotsForBinding.AddNew();
				caPivot.CI_TariffNum = "1";
				caPivot.CI_RN_NKCountry = Core.Constants.CountryCodes.Canada;
				Factory.Save();

				AssertEquals("Should not be able to delete the product as a classification for another country exists...", false, product.CanDelete);
				AssertEquals("Unable to delete a product that has classifications related to other countries.", product.ReasonForNotAbleToDelete.GetUnresolvedString());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var otherFactory = new BusinessObjectFactory();
				var product = otherFactory.Load<OrgSupplierPart>(pk);
				AssertEquals("Should be able to delete the product as no classification for other countries exists...", true, product.CanDelete);
				product.Delete();
				AssertEquals(true, product.IsDeleted);

				otherFactory.Save();
			}
		}

		public void TestExportAuditAndImportAudit()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == "AU" || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == "NZ")
			{
				var product = Factory.New<OrgSupplierPart>();
				var pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_LastAuditedUser = "IU";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 9);
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				pivot.CI_LastAuditedUser = "EU";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 8);
				AssertEquals("ImportLastAuditUser - single value", "IU", product.ImportLastAuditUser);
				AssertEquals("ImportLastAuditDate - single value", "09-Apr-18", product.ImportLastAuditDate);
				AssertEquals("ExportLastAuditUser - single value", "EU", product.ExportLastAuditUser);
				AssertEquals("ExportLastAuditDate - single value", "08-Apr-18", product.ExportLastAuditDate);

				product = Factory.New<OrgSupplierPart>();
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_LastAuditedUser = "IU";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 9);
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				pivot.CI_LastAuditedUser = "EU1";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 8);
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				pivot.CI_LastAuditedUser = "EU2";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 7);
				AssertEquals("ImportLastAuditUser - single value", "IU", product.ImportLastAuditUser);
				AssertEquals("ImportLastAuditDate - single value", "09-Apr-18", product.ImportLastAuditDate);
				AssertEquals("ExportLastAuditUser - multiple value", "MULTI", product.ExportLastAuditUser);
				AssertEquals("ExportLastAuditDate - multiple value", "MULTI", product.ExportLastAuditDate);

				product = Factory.New<OrgSupplierPart>();
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_LastAuditedUser = "IU1";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 9);
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_LastAuditedUser = "IU2";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 8);
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				pivot.CI_LastAuditedUser = "EU";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 7);
				AssertEquals("ImportLastAuditUser - multiple value", "MULTI", product.ImportLastAuditUser);
				AssertEquals("ImportLastAuditDate - multiple value", "MULTI", product.ImportLastAuditDate);
				AssertEquals("ExportLastAuditUser - single value", "EU", product.ExportLastAuditUser);
				AssertEquals("ExportLastAuditDate - single value", "07-Apr-18", product.ExportLastAuditDate);

				product = Factory.New<OrgSupplierPart>();
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_LastAuditedUser = "IU1";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 9);
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_LastAuditedUser = "IU2";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 8);
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				pivot.CI_LastAuditedUser = "EU1";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 7);
				pivot = product.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				pivot.CI_LastAuditedUser = "EU2";
				pivot.CI_LastAuditedDate = new ZDateTime(2018, 4, 6);
				AssertEquals("ImportLastAuditUser - multiple value", "MULTI", product.ImportLastAuditUser);
				AssertEquals("ImportLastAuditDate - multiple value", "MULTI", product.ImportLastAuditDate);
				AssertEquals("ExportLastAuditUser - multiple value", "MULTI", product.ExportLastAuditUser);
				AssertEquals("ExportLastAuditDate - multiple value", "MULTI", product.ExportLastAuditDate);
			}
			else
			{
				Assert(true);
			}
		}

		public override void TestBusinessObjectsWithRelatedEvents()
		{
			base.TestBusinessObjectsWithRelatedEvents();
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			AssertCollectionContains("BusinessObjectsWithRelatedEvents should contain pivot.", pivot, product.BusinessObjectsWithRelatedEvents);
		}

		#region Implementation
		protected virtual string ValidTariffCode
		{
			get { return "10.10.10"; }
		}

		#region GetNewBusinessObject
		protected override BusinessObject GetNewBusinessObject()
		{
			return OrgSupplierPart.New(Factory);
		}
		#endregion
		#endregion
	}
}
