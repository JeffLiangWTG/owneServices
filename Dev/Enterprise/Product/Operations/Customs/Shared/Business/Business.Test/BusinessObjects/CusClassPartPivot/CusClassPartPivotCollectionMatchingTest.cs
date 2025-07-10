using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CusClassPartPivotCollectionMatchingTest : TestCaseWithFactory
	{
		public void TestCusClassPartPivotCollectionWithCountryCode()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART";
			part.OP_Desc = "Desc.";

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;

			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var reloadedPart = otherFactory.Load<OrgSupplierPart>(part.PK);
			var collection = new CusClassPartPivotCollection<BaseCusClassPartPivot>(reloadedPart, Core.Constants.CountryCodes.UnitedStates);
			collection.Load();

			AssertEquals(2, collection.Count);
		}

		public void TestCusClassPartPivotCollectionWithNullCountryCode()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART";
			part.OP_Desc = "Desc.";

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;

			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_RN_NKCountry = Core.Constants.CountryCodes.Canada;

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			var reloadedPart = otherFactory.Load<OrgSupplierPart>(part.PK);
			var collection = new CusClassPartPivotCollection<BaseCusClassPartPivot>(reloadedPart, null);
			collection.Load();

			AssertEquals(3, collection.Count);
			AssertEquals("AU, US, CA", string.Join(", ", collection.OfType<BaseCusClassPartPivot>().Select(x => x.CI_RN_NKCountry)));
		}

		public void TestGetPivotChildFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "123";

				var pivot = part.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

				var childPivot = pivot.Children.AddNew();
				childPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

				Factory.Save();

				//Occurs when The Pivot is RemoveAndDelete()'d, and the parent pivot is left without a CI_OP, when the parent pivot tries to delete the child pivot, it previously tried to match on the CI_OP matching, which was never the case.
				pivot.CI_OP = ZGuid.Empty;

				var collection = new CusClassPartPivotCollection<BaseCusClassPartPivot>(pivot, Core.Constants.CountryCodes.UnitedStates);
				collection.Load();
				AssertCollectionContains(childPivot, collection);
			}
		}

		public void TestGetNonDeletedPivots()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();
			var ownerOrg = Factory.New<OrgHeader>();
			var supplierOrg = Factory.New<OrgHeader>();

			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = "OWN";
			ownerRelation.OU_OH = ownerOrg.PK;

			var ownerRelation1 = part.RelatedOrganisations.AddNew();
			ownerRelation1.OU_Relationship = "SUP";
			ownerRelation1.OU_OH = ownerOrg.PK;

			var ownerRelation2 = part.RelatedOrganisations.AddNew();
			ownerRelation2.OU_Relationship = "BTH";
			ownerRelation2.OU_OH = ownerOrg.PK;

			var supplierRelation1 = part.RelatedOrganisations.AddNew();
			supplierRelation1.OU_Relationship = "OWN";
			supplierRelation1.OU_OH = supplierOrg.PK;

			var supplierRelation2 = part.RelatedOrganisations.AddNew();
			supplierRelation2.OU_Relationship = "SUP";
			supplierRelation2.OU_OH = supplierOrg.PK;

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = classificationTypeProvider.HTICode;
			pivot1.CI_DateStart = ZDateTime.Now.AddDays(-21);
			pivot1.CI_TariffNum = "9817009800";

			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = classificationTypeProvider.HTBCode;
			pivot2.CI_DateStart = ZDateTime.Now.AddDays(-11);
			pivot2.CI_TariffNum = "98060005";

			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = classificationTypeProvider.HTICode;
			pivot3.CI_DateStart = ZDateTime.Now.AddDays(-31);
			pivot3.CI_TariffNum = "2517100015";

			var pivot4 = part.PivotsForBinding.AddNew();
			pivot4.CI_ChildType = classificationTypeProvider.HTICode;
			pivot4.CI_DateStart = ZDateTime.Now.AddDays(-20);
			pivot4.CI_TariffNum = "9817009800";

			var pivot5 = part.PivotsForBinding.AddNew();
			pivot5.CI_ChildType = classificationTypeProvider.HTICode;
			pivot5.CI_DateStart = ZDateTime.Now.AddDays(-10);
			pivot5.CI_TariffNum = "98060005";

			var pivot6 = part.PivotsForBinding.AddNew();
			pivot6.CI_ChildType = classificationTypeProvider.HTICode;
			pivot6.CI_DateStart = ZDateTime.Now.AddDays(-30);
			pivot6.CI_TariffNum = "2517100015";

			((IBusinessObjectInternals)pivot1).Row.Delete();
			((IBusinessObjectInternals)pivot2).Row.Delete();
			((IBusinessObjectInternals)pivot3).Row.Delete();
			AssertEquals(6, part.PivotsForBinding.Count);
			AssertContainsExactElementsInAnyOrder(new[] { pivot4, pivot5, pivot6 }, part.PivotsForBinding.GetNonDeletedPivots());
			var pivot = part.PivotsForBinding.GetImportMatch(ownerOrg.PK, ZGuid.Empty, ZDate.Empty);
			AssertEquals(pivot.CI_TariffNum, pivot6, pivot);
			pivot = part.PivotsForBinding.GetMatch(classificationTypeProvider.HTICode, ownerOrg.PK, ZGuid.Empty, true);
			AssertEquals(pivot.CI_TariffNum, pivot6, pivot);
			pivot = part.PivotsForBinding.GetMatch(classificationTypeProvider.HTICode, ownerOrg.PK, ZGuid.Empty, ZDate.Empty, true);
			AssertEquals(pivot.CI_TariffNum, pivot6, pivot);
			AssertContainsExactElementsInAnyOrder(new[] { pivot4, pivot5, pivot6 }, part.PivotsForBinding.GetMatchesIgnoringAttributes(classificationTypeProvider.HTICode, ownerOrg.PK, ZGuid.Empty));
		}

		#region matching tests

		public void TestGetBestImporterMatchByExpiringDate()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();
			OrgHeader ownerOrg = Factory.New<OrgHeader>();
			OrgHeader supplierOrg = Factory.New<OrgHeader>();

			OrgPartRelation ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = "OWN";
			ownerRelation.OU_OH = ownerOrg.PK;

			OrgPartRelation ownerRelation1 = part.RelatedOrganisations.AddNew();
			ownerRelation1.OU_Relationship = "SUP";
			ownerRelation1.OU_OH = ownerOrg.PK;

			OrgPartRelation ownerRelation2 = part.RelatedOrganisations.AddNew();
			ownerRelation2.OU_Relationship = "BTH";
			ownerRelation2.OU_OH = ownerOrg.PK;

			OrgPartRelation supplierRelation1 = part.RelatedOrganisations.AddNew();
			supplierRelation1.OU_Relationship = "OWN";
			supplierRelation1.OU_OH = supplierOrg.PK;

			OrgPartRelation supplierRelation2 = part.RelatedOrganisations.AddNew();
			supplierRelation2.OU_Relationship = "SUP";
			supplierRelation2.OU_OH = supplierOrg.PK;

			var htiPivot1 = part.PivotsForBinding.AddNew();
			htiPivot1.CI_ChildType = classificationTypeProvider.HTICode;
			htiPivot1.CI_DateStart = ZDateTime.Now.AddDays(-20);
			htiPivot1.CI_TariffNum = "9817009800";

			var htiPivot2 = part.PivotsForBinding.AddNew();
			htiPivot2.CI_ChildType = classificationTypeProvider.HTICode;
			htiPivot2.CI_DateStart = ZDateTime.Now.AddDays(-10);
			htiPivot2.CI_TariffNum = "98060005";

			var htiPivot3 = part.PivotsForBinding.AddNew();
			htiPivot3.CI_ChildType = classificationTypeProvider.HTICode;
			htiPivot3.CI_DateStart = ZDateTime.Now.AddDays(-30);
			htiPivot3.CI_TariffNum = "2517100015";

			var pivot = part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty, ZDate.Empty);
			AssertEquals(pivot.CI_TariffNum, htiPivot3, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ownerOrg.PK, ZGuid.Empty, ZDate.Empty);
			AssertEquals(pivot.CI_TariffNum, htiPivot3, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ZGuid.Empty, supplierOrg.PK, ZDate.Empty);
			AssertEquals(pivot.CI_TariffNum, htiPivot3, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ownerOrg.PK, supplierOrg.PK, ZDate.Empty);
			AssertEquals(pivot.CI_TariffNum, htiPivot3, pivot);

			pivot = part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty, ZDate.Today);
			AssertEquals(pivot.CI_TariffNum, htiPivot2, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ownerOrg.PK, ZGuid.Empty, ZDate.Today);
			AssertEquals(pivot.CI_TariffNum, htiPivot2, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ZGuid.Empty, supplierOrg.PK, ZDate.Today);
			AssertEquals(pivot.CI_TariffNum, htiPivot2, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ownerOrg.PK, supplierOrg.PK, ZDate.Today);
			AssertEquals(pivot.CI_TariffNum, htiPivot2, pivot);

			var htiPivot4 = part.PivotsForBinding.AddNew();
			htiPivot4.CI_ChildType = classificationTypeProvider.HTICode;
			htiPivot4.CI_DateStart = ZDateTime.Now.AddDays(-5);
			htiPivot4.CI_TariffNum = "98060006";
			htiPivot4.CI_OH = ownerRelation.OU_OH;

			htiPivot3.CI_OH = ownerRelation2.OU_OH;

			var htiPivot5 = part.PivotsForBinding.AddNew();
			htiPivot5.CI_ChildType = classificationTypeProvider.HTICode;
			htiPivot5.CI_DateStart = ZDateTime.Now.AddDays(-4);
			htiPivot5.CI_TariffNum = "98060007";
			htiPivot5.CI_OH = supplierRelation1.OU_OH;

			var htiPivot6 = part.PivotsForBinding.AddNew();
			htiPivot6.CI_ChildType = classificationTypeProvider.HTICode;
			htiPivot6.CI_DateStart = ZDateTime.Now.AddDays(-3);
			htiPivot6.CI_TariffNum = "98060008";
			htiPivot6.CI_OH = supplierRelation2.OU_OH;

			pivot = part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty, ZDate.Empty);
			AssertEquals(pivot.CI_TariffNum, htiPivot1, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ownerOrg.PK, ZGuid.Empty, ZDate.Empty);
			AssertEquals(pivot.CI_TariffNum, htiPivot3, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ZGuid.Empty, supplierOrg.PK, ZDate.Empty);
			AssertEquals(pivot.CI_TariffNum, htiPivot5, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ownerOrg.PK, supplierOrg.PK, ZDate.Empty);
			AssertEquals(pivot.CI_TariffNum, htiPivot3, pivot);

			pivot = part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty, ZDate.Today);
			AssertEquals(pivot.CI_TariffNum, htiPivot2, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ownerOrg.PK, ZGuid.Empty, ZDate.Today);
			AssertEquals(pivot.CI_TariffNum, htiPivot4, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ZGuid.Empty, supplierOrg.PK, ZDate.Today);
			AssertEquals(pivot.CI_TariffNum, htiPivot6, pivot);
			pivot = part.PivotsForBinding.GetImportMatch(ownerOrg.PK, supplierOrg.PK, ZDate.Today);
			AssertEquals(pivot.CI_TariffNum, htiPivot4, pivot);
		}

		public void TestChildPivotsDoesNotIncludeParentPivot()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var childPivots = new CusClassPartPivotCollection<BaseCusClassPartPivot>(pivot, MasterFiles.Business.GlbCompany.CurrentCompany.Country.RN_Code);
			pivot.CI_CI_Parent = pivot.PK;
			AssertEquals("CusClassPartPivot.CI_CI_Parent is pointing to itself.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			AssertEquals("Should not contain itself", 0, childPivots.Count);
		}

		public void TestGetBestImporterMatch()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();

			OrgHeader ownerOrg = Factory.New<OrgHeader>();
			OrgHeader supplierOrg = Factory.New<OrgHeader>();

			OrgPartRelation ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = "OWN";
			ownerRelation.OU_OH = ownerOrg.PK;

			OrgPartRelation supplierRelation = part.RelatedOrganisations.AddNew();
			supplierRelation.OU_Relationship = "SUP";
			supplierRelation.OU_OH = supplierOrg.PK;

			AssertNull(part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty));
			AssertNull(part.PivotsForBinding.GetImportMatch(ownerOrg.PK, ZGuid.Empty));
			AssertNull(part.PivotsForBinding.GetImportMatch(ZGuid.Empty, supplierOrg.PK));
			AssertNull(part.PivotsForBinding.GetImportMatch(ownerOrg.PK, supplierOrg.PK));

			var htiPivot = part.PivotsForBinding.AddNew();
			htiPivot.CI_ChildType = classificationTypeProvider.HTICode;

			var htePivot = part.PivotsForBinding.AddNew();
			htePivot.CI_ChildType = classificationTypeProvider.HTECode;

			var schBPivot = part.PivotsForBinding.AddNew();
			schBPivot.CI_ChildType = classificationTypeProvider.SHBCode;

			AssertEquals(htiPivot, part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty));
			AssertEquals(htiPivot, part.PivotsForBinding.GetImportMatch(ownerOrg.PK, ZGuid.Empty));
			AssertEquals(htiPivot, part.PivotsForBinding.GetImportMatch(ZGuid.Empty, supplierOrg.PK));
			AssertEquals(htiPivot, part.PivotsForBinding.GetImportMatch(ownerOrg.PK, supplierOrg.PK));

			var htiOwnerPivot = part.PivotsForBinding.AddNew();
			htiOwnerPivot.CI_ChildType = classificationTypeProvider.HTICode;
			htiOwnerPivot.CI_OH = ownerRelation.OU_OH;

			var htiSuppliertPivot = part.PivotsForBinding.AddNew();
			htiSuppliertPivot.CI_ChildType = classificationTypeProvider.HTICode;
			htiSuppliertPivot.CI_OH = supplierRelation.OU_OH;

			AssertEquals(htiPivot, part.PivotsForBinding.GetImportMatch(ZGuid.Empty, ZGuid.Empty));
			AssertEquals(htiOwnerPivot, part.PivotsForBinding.GetImportMatch(ownerOrg.PK, ZGuid.Empty));
			AssertEquals(htiSuppliertPivot, part.PivotsForBinding.GetImportMatch(ZGuid.Empty, supplierOrg.PK));
			AssertEquals(htiOwnerPivot, part.PivotsForBinding.GetImportMatch(ownerOrg.PK, supplierOrg.PK));
		}

		public void TestGetBestExportMatchByExpiringDate()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();
			OrgHeader ownerOrg = Factory.New<OrgHeader>();
			OrgHeader supplierOrg = Factory.New<OrgHeader>();

			OrgPartRelation ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = "OWN";
			ownerRelation.OU_OH = ownerOrg.PK;

			OrgPartRelation ownerRelation1 = part.RelatedOrganisations.AddNew();
			ownerRelation1.OU_Relationship = "SUP";
			ownerRelation1.OU_OH = ownerOrg.PK;

			OrgPartRelation supplierRelation1 = part.RelatedOrganisations.AddNew();
			supplierRelation1.OU_Relationship = "OWN";
			supplierRelation1.OU_OH = supplierOrg.PK;

			OrgPartRelation supplierRelation2 = part.RelatedOrganisations.AddNew();
			supplierRelation2.OU_Relationship = "SUP";
			supplierRelation2.OU_OH = supplierOrg.PK;

			var htiPivot = part.PivotsForBinding.AddNew();
			htiPivot.CI_ChildType = classificationTypeProvider.HTICode;

			var htePivot1 = part.PivotsForBinding.AddNew();
			htePivot1.CI_ChildType = classificationTypeProvider.HTECode;
			htePivot1.CI_DateStart = ZDateTime.Now.AddDays(-20);
			htePivot1.CI_TariffNum = "000014";

			var htePivot2 = part.PivotsForBinding.AddNew();
			htePivot2.CI_ChildType = classificationTypeProvider.HTECode;
			htePivot2.CI_DateStart = ZDateTime.Now.AddDays(-10);
			htePivot2.CI_TariffNum = "000015";

			var schBPivot1 = part.PivotsForBinding.AddNew();
			schBPivot1.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot1.CI_DateStart = ZDateTime.Now.AddDays(-20);
			schBPivot1.CI_TariffNum = "000024";

			var schBPivot2 = part.PivotsForBinding.AddNew();
			schBPivot2.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot2.CI_DateStart = ZDateTime.Now.AddDays(-10);
			schBPivot2.CI_TariffNum = "000025";

			AssertEquals(schBPivot1, part.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty, ZDate.Empty));
			AssertEquals(schBPivot1, part.PivotsForBinding.GetExportMatch(true, ownerOrg.PK, ZGuid.Empty, ZDate.Empty));
			AssertEquals(schBPivot1, part.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, supplierOrg.PK, ZDate.Empty));
			AssertEquals(schBPivot1, part.PivotsForBinding.GetExportMatch(true, ownerOrg.PK, supplierOrg.PK, ZDate.Empty));
			AssertEquals(htePivot1, part.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty, ZDate.Empty));
			AssertEquals(htePivot1, part.PivotsForBinding.GetExportMatch(false, ownerOrg.PK, ZGuid.Empty, ZDate.Empty));
			AssertEquals(htePivot1, part.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, supplierOrg.PK, ZDate.Empty));
			AssertEquals(htePivot1, part.PivotsForBinding.GetExportMatch(false, ownerOrg.PK, supplierOrg.PK, ZDate.Empty));

			var schBPivot3 = part.PivotsForBinding.AddNew();
			schBPivot3.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot3.CI_DateStart = ZDateTime.Now.AddDays(-30);
			schBPivot3.CI_TariffNum = "000026";
			schBPivot3.CI_OH = supplierRelation1.OU_OH;

			var schBPivot4 = part.PivotsForBinding.AddNew();
			schBPivot4.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot4.CI_DateStart = ZDateTime.Now.AddDays(-20);
			schBPivot4.CI_TariffNum = "000027";
			schBPivot4.CI_OH = supplierRelation2.OU_OH;

			var schBPivot5 = part.PivotsForBinding.AddNew();
			schBPivot5.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot5.CI_DateStart = ZDateTime.Now.AddDays(-5);
			schBPivot5.CI_TariffNum = "000028";
			schBPivot5.CI_OH = ownerRelation.OU_OH;
			schBPivot2.CI_OH = ownerRelation1.OU_OH;

			AssertEquals(schBPivot1, part.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty, ZDate.Today));
			AssertEquals(schBPivot5, part.PivotsForBinding.GetExportMatch(true, ownerOrg.PK, ZGuid.Empty, ZDate.Today));
			AssertEquals(schBPivot4, part.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, supplierOrg.PK, ZDate.Today));
			AssertEquals(schBPivot5, part.PivotsForBinding.GetExportMatch(true, ownerOrg.PK, supplierOrg.PK, ZDate.Today));

			AssertEquals(htePivot2, part.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty, ZDate.Today));
			AssertEquals(htePivot2, part.PivotsForBinding.GetExportMatch(false, ownerOrg.PK, ZGuid.Empty, ZDate.Today));
			AssertEquals(htePivot2, part.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, supplierOrg.PK, ZDate.Today));
			AssertEquals(htePivot2, part.PivotsForBinding.GetExportMatch(false, ownerOrg.PK, supplierOrg.PK, ZDate.Today));
		}

		public void TestGetBestExportMatch()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();

			var ownerOrg = Factory.New<OrgHeader>();
			var supplierOrg = Factory.New<OrgHeader>();

			var ownerRelation = part.RelatedOrganisations.AddOwner(ownerOrg);
			var supplierRelation = part.RelatedOrganisations.AddSupplier(supplierOrg);

			AssertNull(part.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty));
			AssertNull(part.PivotsForBinding.GetExportMatch(true, ownerOrg.PK, ZGuid.Empty));
			AssertNull(part.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, supplierOrg.PK));
			AssertNull(part.PivotsForBinding.GetExportMatch(true, ownerOrg.PK, supplierOrg.PK));

			var htiPivot = part.PivotsForBinding.AddNew();
			htiPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			var htePivot = part.PivotsForBinding.AddNew();
			htePivot.CI_ChildType = ClassificationTypeList.Codes.HTE;

			var schBPivot = part.PivotsForBinding.AddNew();
			schBPivot.CI_ChildType = classificationTypeProvider.SHBCode;

			AssertEquals(schBPivot, part.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty));
			AssertEquals(schBPivot, part.PivotsForBinding.GetExportMatch(true, ownerOrg.PK, ZGuid.Empty));
			AssertEquals(schBPivot, part.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, supplierOrg.PK));
			AssertEquals(schBPivot, part.PivotsForBinding.GetExportMatch(true, ownerOrg.PK, supplierOrg.PK));
			AssertEquals(htePivot, part.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty));
			AssertEquals(htePivot, part.PivotsForBinding.GetExportMatch(false, ownerOrg.PK, ZGuid.Empty));
			AssertEquals(htePivot, part.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, supplierOrg.PK));
			AssertEquals(htePivot, part.PivotsForBinding.GetExportMatch(false, ownerOrg.PK, supplierOrg.PK));

			var schbOwnerPivot = part.PivotsForBinding.AddNew();
			schbOwnerPivot.CI_ChildType = classificationTypeProvider.SHBCode;
			schbOwnerPivot.CI_OH = ownerRelation.OU_OH;

			var schbSuppliertPivot = part.PivotsForBinding.AddNew();
			schbSuppliertPivot.CI_ChildType = classificationTypeProvider.SHBCode;
			schbSuppliertPivot.CI_OH = supplierRelation.OU_OH;

			AssertEquals(schBPivot, part.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, ZGuid.Empty));
			AssertEquals(schbOwnerPivot, part.PivotsForBinding.GetExportMatch(true, ownerOrg.PK, ZGuid.Empty));
			AssertEquals(schbSuppliertPivot, part.PivotsForBinding.GetExportMatch(true, ZGuid.Empty, supplierOrg.PK));
			AssertEquals(schbOwnerPivot, part.PivotsForBinding.GetExportMatch(true, ownerOrg.PK, supplierOrg.PK));
			AssertEquals(htePivot, part.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty));
			AssertEquals(htePivot, part.PivotsForBinding.GetExportMatch(false, ownerOrg.PK, ZGuid.Empty));
			AssertEquals(htePivot, part.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, supplierOrg.PK));
			AssertEquals(htePivot, part.PivotsForBinding.GetExportMatch(false, ownerOrg.PK, supplierOrg.PK));

			var hteOwnerPivot = part.PivotsForBinding.AddNew();
			hteOwnerPivot.CI_ChildType = classificationTypeProvider.HTECode;
			hteOwnerPivot.CI_OH = ownerRelation.OU_OH;

			var hteSuppliertPivot = part.PivotsForBinding.AddNew();
			hteSuppliertPivot.CI_ChildType = classificationTypeProvider.HTECode;
			hteSuppliertPivot.CI_OH = supplierRelation.OU_OH;

			AssertEquals(htePivot, part.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, ZGuid.Empty));
			AssertEquals(hteOwnerPivot, part.PivotsForBinding.GetExportMatch(false, ownerOrg.PK, ZGuid.Empty));
			AssertEquals(hteSuppliertPivot, part.PivotsForBinding.GetExportMatch(false, ZGuid.Empty, supplierOrg.PK));
			AssertEquals(hteOwnerPivot, part.PivotsForBinding.GetExportMatch(false, ownerOrg.PK, supplierOrg.PK));
		}

		public void TestGetMatch()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();
			OrgHeader ownerOrg = Factory.New<OrgHeader>();
			OrgHeader supplierOrg = Factory.New<OrgHeader>();

			OrgPartRelation ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = "OWN";
			ownerRelation.OU_OH = ownerOrg.PK;

			OrgPartRelation ownerRelation1 = part.RelatedOrganisations.AddNew();
			ownerRelation1.OU_Relationship = "SUP";
			ownerRelation1.OU_OH = ownerOrg.PK;

			OrgPartRelation supplierRelation1 = part.RelatedOrganisations.AddNew();
			supplierRelation1.OU_Relationship = "OWN";
			supplierRelation1.OU_OH = supplierOrg.PK;

			OrgPartRelation supplierRelation2 = part.RelatedOrganisations.AddNew();
			supplierRelation2.OU_Relationship = "SUP";
			supplierRelation2.OU_OH = supplierOrg.PK;

			var htiPivot = part.PivotsForBinding.AddNew();
			htiPivot.CI_ChildType = classificationTypeProvider.HTICode;

			var htePivot1 = part.PivotsForBinding.AddNew();
			htePivot1.CI_ChildType = classificationTypeProvider.HTECode;
			htePivot1.CI_DateStart = ZDateTime.Now.AddDays(-20);
			htePivot1.CI_TariffNum = "000014";

			var htePivot2 = part.PivotsForBinding.AddNew();
			htePivot2.CI_ChildType = classificationTypeProvider.HTECode;
			htePivot2.CI_DateStart = ZDateTime.Now.AddDays(-10);
			htePivot2.CI_TariffNum = "000015";

			var schBPivot1 = part.PivotsForBinding.AddNew();
			schBPivot1.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot1.CI_DateStart = ZDateTime.Now.AddDays(-20);
			schBPivot1.CI_TariffNum = "000024";

			var schBPivot2 = part.PivotsForBinding.AddNew();
			schBPivot2.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot2.CI_DateStart = ZDateTime.Now.AddDays(-10);
			schBPivot2.CI_TariffNum = "000025";

			AssertEquals(schBPivot1, part.PivotsForBinding.GetMatch(classificationTypeProvider.SHBCode, ZGuid.Empty, ZGuid.Empty, ZDate.Empty, false));
			AssertEquals(schBPivot1, part.PivotsForBinding.GetMatch(classificationTypeProvider.SHBCode, ownerOrg.PK, ZGuid.Empty, ZDate.Empty, false));
			AssertEquals(schBPivot1, part.PivotsForBinding.GetMatch(classificationTypeProvider.SHBCode, ZGuid.Empty, supplierOrg.PK, ZDate.Empty, false));
			AssertEquals(schBPivot1, part.PivotsForBinding.GetMatch(classificationTypeProvider.SHBCode, ownerOrg.PK, supplierOrg.PK, ZDate.Empty, false));

			AssertEquals(htePivot1, part.PivotsForBinding.GetMatch(classificationTypeProvider.HTECode, ZGuid.Empty, ZGuid.Empty, ZDate.Empty, false));
			AssertEquals(htePivot1, part.PivotsForBinding.GetMatch(classificationTypeProvider.HTECode, ownerOrg.PK, ZGuid.Empty, ZDate.Empty, false));
			AssertEquals(htePivot1, part.PivotsForBinding.GetMatch(classificationTypeProvider.HTECode, ZGuid.Empty, supplierOrg.PK, ZDate.Empty, false));
			AssertEquals(htePivot1, part.PivotsForBinding.GetMatch(classificationTypeProvider.HTECode, ownerOrg.PK, supplierOrg.PK, ZDate.Empty, false));

			var schBPivot3 = part.PivotsForBinding.AddNew();
			schBPivot3.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot3.CI_DateStart = ZDateTime.Now.AddDays(-30);
			schBPivot3.CI_TariffNum = "000026";
			schBPivot3.CI_OH = supplierRelation1.OU_OH;

			var schBPivot4 = part.PivotsForBinding.AddNew();
			schBPivot4.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot4.CI_DateStart = ZDateTime.Now.AddDays(-20);
			schBPivot4.CI_TariffNum = "000027";
			schBPivot4.CI_OH = supplierRelation2.OU_OH;

			var schBPivot5 = part.PivotsForBinding.AddNew();
			schBPivot5.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot5.CI_DateStart = ZDateTime.Now.AddDays(-5);
			schBPivot5.CI_TariffNum = "000028";
			schBPivot5.CI_OH = ownerRelation.OU_OH;
			schBPivot2.CI_OH = ownerRelation1.OU_OH;

			AssertEquals(schBPivot1, part.PivotsForBinding.GetMatch(classificationTypeProvider.SHBCode, ZGuid.Empty, ZGuid.Empty, ZDate.Today, true));
			AssertEquals(schBPivot5, part.PivotsForBinding.GetMatch(classificationTypeProvider.SHBCode, ownerOrg.PK, ZGuid.Empty, ZDate.Today, true));
			AssertEquals(schBPivot4, part.PivotsForBinding.GetMatch(classificationTypeProvider.SHBCode, ZGuid.Empty, supplierOrg.PK, ZDate.Today, true));
			AssertEquals(schBPivot5, part.PivotsForBinding.GetMatch(classificationTypeProvider.SHBCode, ownerOrg.PK, supplierOrg.PK, ZDate.Today, true));

			AssertEquals(htePivot2, part.PivotsForBinding.GetMatch(classificationTypeProvider.HTECode, ZGuid.Empty, ZGuid.Empty, ZDate.Today, true));
			AssertEquals(htePivot2, part.PivotsForBinding.GetMatch(classificationTypeProvider.HTECode, ownerOrg.PK, ZGuid.Empty, ZDate.Today, true));
			AssertEquals(htePivot2, part.PivotsForBinding.GetMatch(classificationTypeProvider.HTECode, ZGuid.Empty, supplierOrg.PK, ZDate.Today, true));
			AssertEquals(htePivot2, part.PivotsForBinding.GetMatch(classificationTypeProvider.HTECode, ownerOrg.PK, supplierOrg.PK, ZDate.Today, true));
		}

		protected override void SetUp()
		{
			base.SetUp();
			countrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			classificationTypeProvider = ClassificationTypeProvider.GetProviderFor(Core.Constants.CountryCodes.UnitedStates);
		}
		IClassificationTypeProvider classificationTypeProvider;

		protected override void TearDown()
		{
			if (countrySetter != null)
			{
				countrySetter.Dispose();
				countrySetter = null;
			}
			base.TearDown();
		}
		IDisposable countrySetter;
		#endregion
	}
}
