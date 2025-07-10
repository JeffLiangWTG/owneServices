using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class IEnumerablePivotsExtensionTest : TestCaseWithFactory
	{
		public void TestGetMatchesIgnoringAttributes()
		{
			var (ownerOrg, supplierOrg, pivots) = CreatePart();

			CombineAssertions(() =>
			{
				var result = pivots.GetMatchesIgnoringAttributes(Core.Constants.CountryCodes.UnitedStates, classificationTypeProvider.HTBCode, ownerOrg.PK, supplierOrg.PK);
				AssertContainsExactElementsInAnyOrder("US|HTB|Owner|Supplier", new[] { pivots[0] }, result);

				result = pivots.GetMatchesIgnoringAttributes(Core.Constants.CountryCodes.UnitedStates, classificationTypeProvider.HTECode, ownerOrg.PK, supplierOrg.PK);
				AssertContainsExactElementsInAnyOrder("US|HTE|Owner|Supplier", new[] { pivots[1] }, result);

				result = pivots.GetMatchesIgnoringAttributes(Core.Constants.CountryCodes.UnitedStates, classificationTypeProvider.HTICode, ownerOrg.PK, supplierOrg.PK);
				AssertContainsExactElementsInAnyOrder("US|HTI|Owner|Supplier", new[] { pivots[2] }, result);

				result = pivots.GetMatchesIgnoringAttributes(Core.Constants.CountryCodes.UnitedStates, classificationTypeProvider.SHBCode, ownerOrg.PK, supplierOrg.PK);
				AssertContainsExactElementsInAnyOrder("US|SHB|Owner|Supplier", new[] { pivots[3] }, result);

				result = pivots.GetMatchesIgnoringAttributes(Core.Constants.CountryCodes.UnitedStates, classificationTypeProvider.SHBCode, ZGuid.Empty, supplierOrg.PK);
				AssertContainsExactElementsInAnyOrder("US|SHB|empty|Supplier", new BaseCusClassPartPivot[] { pivots[3] }, result);

				result = pivots.GetMatchesIgnoringAttributes(Core.Constants.CountryCodes.UnitedStates, classificationTypeProvider.SHBCode, ownerOrg.PK, ZGuid.Empty);
				AssertContainsExactElementsInAnyOrder("US|SHB|Owner|empty", new BaseCusClassPartPivot[] { pivots[3] }, result);

				result = pivots.GetMatchesIgnoringAttributes(Core.Constants.CountryCodes.Afghanistan, classificationTypeProvider.SHBCode, ownerOrg.PK, supplierOrg.PK);
				AssertContainsExactElementsInAnyOrder("AF|SHB|Owner|Supplier", new BaseCusClassPartPivot[] { pivots[3] }, result);
			});
		}

		public void TestGetExportMatch()
		{
			var (ownerOrg, supplierOrg, pivots) = CreatePart();

			CombineAssertions(() =>
			{
				var pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.UnitedStates, false, ownerOrg.PK, supplierOrg.PK);
				AssertEquals("US|false|Owner|Supplier", pivots[1], pivot);

				pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.UnitedStates, true, ownerOrg.PK, supplierOrg.PK);
				AssertEquals("US|true|Owner|Supplier", pivots[3], pivot);

				pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.UnitedStates, false, ZGuid.Empty, supplierOrg.PK);
				AssertEquals("US|false|empty|Supplier", pivots[1], pivot);

				pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.UnitedStates, true, ZGuid.Empty, supplierOrg.PK);
				AssertEquals("US|true|empty|Supplier", pivots[3], pivot);

				pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.UnitedStates, false, ownerOrg.PK, ZGuid.Empty);
				AssertEquals("US|false|Owner|empty", pivots[1], pivot);

				pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.UnitedStates, true, ownerOrg.PK, ZGuid.Empty);
				AssertEquals("US|true|Owner|empty", pivots[3], pivot);

				pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.Afghanistan, false, ownerOrg.PK, supplierOrg.PK);
				AssertEquals("AF|false|Owner|Supplier", pivots[1], pivot);

				AssertExceptionThrown<NotSupportedException>(() => pivots.GetExportMatch(Core.Constants.CountryCodes.Afghanistan, true, ownerOrg.PK, supplierOrg.PK));

				pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.UnitedStates, false, ownerOrg.PK, supplierOrg.PK, ZDate.Today);
				AssertEquals("US|false|Owner|Supplier|Today", pivots[1], pivot);

				pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.UnitedStates, false, ownerOrg.PK, supplierOrg.PK, ZDate.Today.AddDays(-30));
				AssertNull("US|false|Owner|Supplier|Today.AddDays(-30)", pivot);
			});
		}

		public void TestGetImportMatch()
		{
			var (ownerOrg, supplierOrg, pivots) = CreatePart();

			CombineAssertions(() =>
			{
				var pivot = pivots.GetImportMatch(Core.Constants.CountryCodes.UnitedStates, ownerOrg.PK, supplierOrg.PK);
				AssertEquals("US|Owner|Supplier", pivots[2], pivot);

				pivot = pivots.GetImportMatch(Core.Constants.CountryCodes.UnitedStates, ZGuid.Empty, supplierOrg.PK);
				AssertEquals("US|empty|Supplier", pivots[2], pivot);

				pivot = pivots.GetImportMatch(Core.Constants.CountryCodes.UnitedStates, ownerOrg.PK, ZGuid.Empty);
				AssertEquals("US|Owner|empty", pivots[2], pivot);

				pivot = pivots.GetImportMatch(Core.Constants.CountryCodes.Afghanistan, ownerOrg.PK, supplierOrg.PK);
				AssertEquals("AF|Owner|Supplier", pivots[2], pivot);

				pivot = pivots.GetImportMatch(Core.Constants.CountryCodes.Afghanistan, ZGuid.Empty, supplierOrg.PK);
				AssertEquals("AF|empty|Supplier", pivots[2], pivot);

				pivot = pivots.GetImportMatch(Core.Constants.CountryCodes.Afghanistan, ownerOrg.PK, ZGuid.Empty);
				AssertEquals("AF|Owner|empty", pivots[2], pivot);

				pivot = pivots.GetImportMatch(Core.Constants.CountryCodes.UnitedStates, ownerOrg.PK, supplierOrg.PK, ZDate.Today);
				AssertEquals("US|Owner|Supplier|Today", pivots[2], pivot);

				pivot = pivots.GetImportMatch(Core.Constants.CountryCodes.UnitedStates, ownerOrg.PK, supplierOrg.PK, ZDate.Today.AddDays(-30));
				AssertNull("US|Owner|Supplier|Today.Adddays(-30)", pivot);
			});
		}

		public void TestGetMatchesIgnoringMatchOnOrgPK()
		{
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var (ownerOrg, supplierOrg, pivots) = CreatePart();
			pivots.ForEach(p => p.CI_OH = otherOrg.PK);

			CombineAssertions(() =>
			{
				var pivot = pivots.GetImportMatch(Core.Constants.CountryCodes.UnitedStates, ownerOrg.PK, supplierOrg.PK, true);
				AssertEquals("ImportMatch|US|Owner|Supplier|true", pivots[2], pivot);

				pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.UnitedStates, false, ownerOrg.PK, supplierOrg.PK, true);
				AssertEquals("ExportMatch|US|false|Owner|Supplier|true", pivots[1], pivot);

				pivot = pivots.GetExportMatch(Core.Constants.CountryCodes.UnitedStates, true, ownerOrg.PK, supplierOrg.PK, true);
				AssertEquals("ExportMatch|US|true|Owner|Supplier|true", pivots[3], pivot);
			});
		}

		public void TestGetNonDeletedPivots()
		{
			var (_, _, pivots) = CreatePart();
			AssertEquals(4, pivots.Length);

			((IBusinessObjectInternals)pivots[0]).Row.Delete();
			((IBusinessObjectInternals)pivots[1]).Row.Delete();
			AssertContainsExactElementsInAnyOrder(new[] { pivots[2], pivots[3] }, pivots.GetNonDeletedPivots());
		}

		protected override void SetUp()
		{
			base.SetUp();
			classificationTypeProvider = ClassificationTypeProvider.GetProviderFor(Core.Constants.CountryCodes.UnitedStates);
		}

		(OrgHeader, OrgHeader, BaseCusClassPartPivot[]) CreatePart()
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
			pivot1.CI_ChildType = classificationTypeProvider.HTBCode;
			pivot1.CI_DateStart = ZDateTime.Now.AddDays(-21);
			pivot1.CI_TariffNum = "9817009800";

			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = classificationTypeProvider.HTECode;
			pivot2.CI_DateStart = ZDateTime.Now.AddDays(-11);
			pivot2.CI_TariffNum = "98060005";

			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = classificationTypeProvider.HTICode;
			pivot3.CI_DateStart = ZDateTime.Now.AddDays(-8);
			pivot3.CI_TariffNum = "2517100015";

			var pivot4 = part.PivotsForBinding.AddNew();
			pivot4.CI_ChildType = classificationTypeProvider.SHBCode;
			pivot4.CI_DateStart = ZDateTime.Now.AddDays(-2);
			pivot4.CI_TariffNum = "9817009800";

			return (ownerOrg, supplierOrg, new[] { pivot1, pivot2, pivot3, pivot4 });
		}

		IClassificationTypeProvider classificationTypeProvider;
	}
}
