using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotCollection))]
	sealed class CusClassPartPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestExpensiveCusClassPartPivotQueryDetection()
		{
			ZQuery query = new ZQuery(CusClassPartPivotSchema.CI_RN_NKCountry, "AU");
			Factory.Load<CusClassPartPivot>(query);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("Query loads excessive number of records from dbo.CusClassPartPivot table.", ErrorReporter.LastMessageReported);
			NewFactory().Load<CusClassPartPivot>(query);
			AssertEquals("error reported once", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestAddNewPartClassificationsWhenPivotIsDeleted()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART";
			part.OP_Desc = "Desc.";
			var ownerOrg = Factory.New<MasterFiles.Business.OrgHeader>();
			ownerOrg.FillWithValidTestData();
			ownerOrg.OH_Code = "O1231";
			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = "OWN";
			ownerRelation.OU_OH = ownerOrg.PK;
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_FormattedTariffNum = "10203040";
			Factory.Save();
			var factory = new BusinessObjectFactory()
			{ RefreshEnabled = false };
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = ownerOrg.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			var reloadedPivot = invoiceLine.Pivot;
			AssertEquals(pivot.PK, reloadedPivot.PK);
			factory.Save();
			pivot.Delete();
			Factory.Save();
			((IBusinessObjectInternals)reloadedPivot).MarkAsDeleted();
			((IBusinessObjectInternals)reloadedPivot).Row.Delete();
			declaration.SaveNewProductsorActivateInactiveOnes(Customs.Business.ProductRelationDefaultOption.OptionForImporter);
			declaration.InvoiceLines.AddNewPartClassifications();
			AssertNotEquals(reloadedPivot.PK, invoiceLine.Pivot.PK);
		}

		public void TestSetDefaultData()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals(ClassificationTypeList.Codes.HTI, pivot.CI_ChildType);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var pivot2 = part.PivotsForBinding.AddNew();
			AssertEquals(ClassificationTypeList.Codes.HTE, pivot2.CI_ChildType);
			var pivotChild = pivot.Children.AddNew();
			AssertEquals(ClassificationChildTypeList.Codes.COMPONENT, pivotChild.CI_ChildType);
			pivotChild.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			var pivotChild2 = pivot.Children.AddNew();
			AssertEquals(ClassificationChildTypeList.Codes.Related, pivotChild2.CI_ChildType);
		}

		public void TestDeleteByDataRefresh()
		{
			var importer = Factory.New<MasterFiles.Business.OrgHeader>();
			importer.OH_Code = "IMP";
			var supplier = Factory.New<MasterFiles.Business.OrgHeader>();
			supplier.OH_Code = "SUP";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "TestProduct";
			part.OP_Desc = "Description";
			var impRelOrg = part.RelatedOrganisations.AddNew();
			impRelOrg.OU_OH = importer.PK;
			impRelOrg.OU_Relationship = "OWN";
			var supRelOrg = part.RelatedOrganisations.AddNew();
			supRelOrg.OU_OH = supplier.PK;
			supRelOrg.OU_Relationship = MasterFiles.Business.OrgRelationTypeList.Codes.Supplier;
			var parentPivot = part.PivotsForBinding.AddNew();
			parentPivot.CI_ChildType = "HTI";
			parentPivot.CI_FormattedTariffNum = "000";
			parentPivot.CI_FormattedSupplementalTariff = "111";
			var childPivot = parentPivot.Children.AddNew();
			childPivot.CI_CI_Parent = parentPivot.PK;
			childPivot.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			childPivot.CI_TariffNum = "222";
			Factory.Save();
			var pivotsCache = part.PivotsForBinding;
			var anotherFactory = new BusinessObjectFactory();
			var partInAnotherFactory = anotherFactory.Load<OrgSupplierPart>(part.PK);
			partInAnotherFactory.PivotsForBinding[0].Delete();
			anotherFactory.Save();
			Assert("Parent Pivot should have been deleted", parentPivot.IsDeleted);
			Assert("Child Pivot should have been deleted", childPivot.IsDeleted);
			AssertEquals("Part Pivots have been reloaded", pivotsCache, part.PivotsForBinding);
			Assert("Parent Pivot should have been removed from PivotCollection of Part", !(((IBusinessObjectCollection<CusClassPartPivot>)part.PivotsForBinding).Count > 0));
			var pivotCollectionAsChildren = ((part as IBusiness).Children).Where(x => x is CusClassPartPivotCollection).Cast<CusClassPartPivotCollection>();
			AssertEquals("Part should only have one Pivot Children", 1, pivotCollectionAsChildren.Count());
			Assert("Deleted child should be removed", !pivotCollectionAsChildren.SelectMany(x => x.Cast<BusinessObject>()).Any(x => x.IsDeleted));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusClassPartPivot>();

		protected override BusinessObjectCollection GetCollectionToTest() => new CusClassPartPivotCollection(Factory.New<OrgSupplierPart>());
	}
}
