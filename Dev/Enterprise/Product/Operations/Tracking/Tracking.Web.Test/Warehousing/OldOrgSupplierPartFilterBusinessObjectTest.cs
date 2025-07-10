using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(OldOrgSupplierPartFilterBusinessObject))]
	class OldOrgSupplierPartFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		public void TestBuyerList()
		{
			AssertEquals("BuyerList: ", typeof(OrgHeaderCollection), OrgSupplierPartFilter.BuyerList.GetType());
		}

		public void TestSupplierList()
		{
			AssertEquals("SupplierList: ", typeof(OrgHeaderCollection), OrgSupplierPartFilter.SupplierList.GetType());
		}

		public void TestOrganisationFilter()
		{
			TestOrganisationFilterCore();
		}

		protected virtual void TestOrganisationFilterCore()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";
			OrgHeader buyer = OrgHeader.New(Factory);
			buyer.OH_Code = "BUYER";
			OrgHeader buyer2 = OrgHeader.New(Factory);
			buyer2.OH_Code = "BUYER2";
			OrgHeader otherOrg1 = OrgHeader.New(Factory);
			otherOrg1.OH_Code = "OTHERORG1";
			OrgHeader otherOrg2 = OrgHeader.New(Factory);
			otherOrg2.OH_Code = "OTHERORG2";

			OrgSupplierPart buyerPart = GetNewOrgSupplierPart();
			buyerPart.OP_PartNum = "BuyerPart";
			buyerPart.OP_Desc = "ASL2";
			OrgPartRelation relation1 = buyerPart.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation1.OU_OH = buyer.PK;

			OrgSupplierPart buyerPart2 = GetNewOrgSupplierPart();
			buyerPart2.OP_PartNum = "BuyerPart";
			buyerPart2.OP_Desc = "ASL2";
			OrgPartRelation relation7 = buyerPart2.RelatedOrganisations.AddNew();
			relation7.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation7.OU_OH = buyer2.PK;

			OrgSupplierPart supplierPart = GetNewOrgSupplierPart();
			supplierPart.OP_Desc = "ASL1";
			supplierPart.OP_PartNum = "SupplierPart";
			OrgPartRelation relation2 = supplierPart.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation2.OU_OH = supplier.PK;

			OrgSupplierPart bothPart = GetNewOrgSupplierPart();
			bothPart.OP_Desc = "ASL3";
			bothPart.OP_PartNum = "BothPart";
			OrgPartRelation relation3 = bothPart.RelatedOrganisations.AddNew();
			relation3.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation3.OU_OH = supplier.PK;
			OrgPartRelation relation4 = bothPart.RelatedOrganisations.AddNew();
			relation4.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation4.OU_OH = buyer.PK;

			Factory.Save();

			OrgSupplierPartCollection parts = new OrgSupplierPartCollection(Factory);
			OrgSupplierPartFilter.Both = true;
			OrgSupplierPartFilter.OrgAnd = true;
			OrgSupplierPartFilter.OP_PartNum = "BuyerPart";
			AssertEquals("Only BuyerParts should be selected", 2, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));
			OrgSupplierPartFilter.OP_PartNum = "SupplierPart";
			AssertEquals("Only SupplierPart should be selected", 1, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));
			OrgSupplierPartFilter.OP_PartNum = "BothPart";
			AssertEquals("Only BothPart should be selected", 1, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));

			OrgSupplierPartFilter.OP_PartNum = ZString.Empty;
			OrgSupplierPartFilter.Buyer = buyer.PK;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Buyer and Both should be selected", 2, parts.Count);
			AssertEquals("Buyer part should be in list", true, parts.Contains(buyerPart));
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));

			OrgSupplierPartFilter.Buyer = ZGuid.Empty;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Supplier and Both should be selected", 2, parts.Count);
			AssertEquals("Supplier part should be in list", true, parts.Contains(supplierPart));
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));

			OrgSupplierPartFilter.Buyer = buyer.PK;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			AssertEquals("All Parts should be selected", 3, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));

			OrgSupplierPartFilter.OrgAnd = false;
			OrgSupplierPartFilter.OrgOr = true;
			AssertEquals("All Parts should be selected", 3, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));

			OrgSupplierPartFilter.Buyer = otherOrg1.PK;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			OrgSupplierPartFilter.OrgAnd = false;
			OrgSupplierPartFilter.OrgOr = true;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Supplier and Both should be selected", 2, parts.Count);
			AssertEquals("Supplier part should be in list", true, parts.Contains(supplierPart));
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));

			OrgSupplierPartFilter.Buyer = otherOrg1.PK;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			OrgSupplierPartFilter.OrgAnd = true;
			OrgSupplierPartFilter.OrgOr = false;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Only Supplier Part should be selected", 1, parts.Count);
			AssertEquals("Supplier part should be in list", true, parts.Contains(supplierPart));

			OrgSupplierPartFilter.Buyer = buyer.PK;
			OrgSupplierPartFilter.Supplier = otherOrg1.PK;
			OrgSupplierPartFilter.OrgAnd = false;
			OrgSupplierPartFilter.OrgOr = true;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Buyer and Both should be selected", 2, parts.Count);
			AssertEquals("Buyer part should be in list", true, parts.Contains(buyerPart));
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));

			OrgSupplierPartFilter.Buyer = buyer.PK;
			OrgSupplierPartFilter.Supplier = otherOrg1.PK;
			OrgSupplierPartFilter.OrgAnd = true;
			OrgSupplierPartFilter.OrgOr = false;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Only Buyer Part should be selected", 1, parts.Count);
			AssertEquals("Buyer part should be in list", true, parts.Contains(buyerPart));

			OrgPartRelation relation5 = buyerPart.RelatedOrganisations.AddNew();
			relation5.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation5.OU_OH = otherOrg1.PK;

			OrgPartRelation relation6 = supplierPart.RelatedOrganisations.AddNew();
			relation6.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation6.OU_OH = otherOrg2.PK;

			Factory.Save();

			OrgSupplierPartFilter.Buyer = buyer.PK;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Only Both should be selected", 1, parts.Count);
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));

			OrgSupplierPartFilter.OrgAnd = false;
			OrgSupplierPartFilter.OrgOr = true;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("All Parts should be selected", 3, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));

			OrgSupplierPartFilter.Buyer = otherOrg2.PK;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			OrgSupplierPartFilter.OrgAnd = false;
			OrgSupplierPartFilter.OrgOr = true;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Supplier and Both should be selected", 2, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));
			AssertEquals("Supplier part should be in list", true, parts.Contains(supplierPart));

			OrgSupplierPartFilter.OrgAnd = true;
			OrgSupplierPartFilter.OrgOr = false;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Only Supplier should be selected", 1, parts.Count);
			AssertEquals("Supplier part should be in list", true, parts.Contains(supplierPart));
		}

		public void TestOrganisationFilterWithBoth()
		{
			TestOrganisationFilterWithBothCore();
		}

		protected virtual void TestOrganisationFilterWithBothCore()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";
			OrgHeader buyer = OrgHeader.New(Factory);
			buyer.OH_Code = "BUYER";
			OrgHeader buyer2 = OrgHeader.New(Factory);
			buyer2.OH_Code = "BUYER2";
			OrgHeader otherOrg1 = OrgHeader.New(Factory);
			otherOrg1.OH_Code = "OTHERORG1";
			OrgHeader otherOrg2 = OrgHeader.New(Factory);
			otherOrg2.OH_Code = "OTHERORG2";

			OrgSupplierPart buyerPart = GetNewOrgSupplierPart();
			buyerPart.OP_PartNum = "BuyerPart";
			buyerPart.OP_Desc = "ASL2";
			OrgPartRelation relation1 = buyerPart.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation1.OU_OH = buyer.PK;

			OrgSupplierPart buyerPart2 = GetNewOrgSupplierPart();
			buyerPart2.OP_PartNum = "BuyerPart";
			buyerPart2.OP_Desc = "ASL2";
			OrgPartRelation relation7 = buyerPart2.RelatedOrganisations.AddNew();
			relation7.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation7.OU_OH = buyer2.PK;

			OrgSupplierPart supplierPart = GetNewOrgSupplierPart();
			supplierPart.OP_Desc = "ASL1";
			supplierPart.OP_PartNum = "SupplierPart";
			OrgPartRelation relation2 = supplierPart.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relation2.OU_OH = supplier.PK;

			OrgSupplierPart bothPart = GetNewOrgSupplierPart();
			bothPart.OP_Desc = "ASL3";
			bothPart.OP_PartNum = "BothPart";
			OrgPartRelation relation3 = bothPart.RelatedOrganisations.AddNew();
			relation3.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation3.OU_OH = supplier.PK;
			OrgPartRelation relation4 = bothPart.RelatedOrganisations.AddNew();
			relation4.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation4.OU_OH = buyer.PK;

			Factory.Save();

			OrgSupplierPartCollection parts = new OrgSupplierPartCollection(Factory);
			OrgSupplierPartFilter.Both = true;
			OrgSupplierPartFilter.OrgAnd = true;
			OrgSupplierPartFilter.OP_PartNum = "BuyerPart";
			AssertEquals("Only BuyerParts should be selected", 2, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));
			OrgSupplierPartFilter.OP_PartNum = "SupplierPart";
			AssertEquals("Only SupplierPart should be selected", 1, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));
			OrgSupplierPartFilter.OP_PartNum = "BothPart";
			AssertEquals("Only BothPart should be selected", 1, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));

			OrgSupplierPartFilter.OP_PartNum = ZString.Empty;
			OrgSupplierPartFilter.Buyer = buyer.PK;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Buyer and Both should be selected", 2, parts.Count);
			AssertEquals("Buyer part should be in list", true, parts.Contains(buyerPart));
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));

			OrgSupplierPartFilter.Buyer = ZGuid.Empty;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Supplier and Both should be selected", 2, parts.Count);
			AssertEquals("Supplier part should be in list", true, parts.Contains(supplierPart));
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));

			OrgSupplierPartFilter.Buyer = buyer.PK;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			AssertEquals("All Parts should be selected", 3, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));

			OrgSupplierPartFilter.OrgAnd = false;
			OrgSupplierPartFilter.OrgOr = true;
			AssertEquals("All Parts should be selected", 3, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));

			OrgSupplierPartFilter.Buyer = otherOrg1.PK;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			OrgSupplierPartFilter.OrgAnd = false;
			OrgSupplierPartFilter.OrgOr = true;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Supplier and Both should be selected", 2, parts.Count);
			AssertEquals("Supplier part should be in list", true, parts.Contains(supplierPart));
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));

			OrgSupplierPartFilter.Buyer = otherOrg1.PK;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			OrgSupplierPartFilter.OrgAnd = true;
			OrgSupplierPartFilter.OrgOr = false;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Only Supplier Part should be selected", 1, parts.Count);
			AssertEquals("Supplier part should be in list", true, parts.Contains(supplierPart));

			OrgSupplierPartFilter.Buyer = buyer.PK;
			OrgSupplierPartFilter.Supplier = otherOrg1.PK;
			OrgSupplierPartFilter.OrgAnd = false;
			OrgSupplierPartFilter.OrgOr = true;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Buyer and Both should be selected", 2, parts.Count);
			AssertEquals("Buyer part should be in list", true, parts.Contains(buyerPart));
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));

			OrgSupplierPartFilter.Buyer = buyer.PK;
			OrgSupplierPartFilter.Supplier = otherOrg1.PK;
			OrgSupplierPartFilter.OrgAnd = true;
			OrgSupplierPartFilter.OrgOr = false;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Only Buyer Part should be selected", 1, parts.Count);
			AssertEquals("Buyer part should be in list", true, parts.Contains(buyerPart));

			OrgPartRelation relation5 = buyerPart.RelatedOrganisations.AddNew();
			relation5.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation5.OU_OH = otherOrg1.PK;

			OrgPartRelation relation6 = supplierPart.RelatedOrganisations.AddNew();
			relation6.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation6.OU_OH = otherOrg2.PK;

			Factory.Save();

			OrgSupplierPartFilter.Buyer = buyer.PK;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Only Both should be selected", 1, parts.Count);
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));

			OrgSupplierPartFilter.OrgAnd = false;
			OrgSupplierPartFilter.OrgOr = true;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("All Parts should be selected", 3, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));

			OrgSupplierPartFilter.Buyer = otherOrg2.PK;
			OrgSupplierPartFilter.Supplier = supplier.PK;
			OrgSupplierPartFilter.OrgAnd = false;
			OrgSupplierPartFilter.OrgOr = true;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Supplier and Both should be selected", 2, Factory.GetDatabaseCount(typeof(OrgSupplierPart), OrgSupplierPartFilter.Filter));
			AssertEquals("Both part should be in list", true, parts.Contains(bothPart));
			AssertEquals("Supplier part should be in list", true, parts.Contains(supplierPart));

			OrgSupplierPartFilter.OrgAnd = true;
			OrgSupplierPartFilter.OrgOr = false;
			parts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Only Supplier should be selected", 1, parts.Count);
			AssertEquals("Supplier part should be in list", true, parts.Contains(supplierPart));
		}

		public void TestStatusFilter()
		{
			OrgSupplierPart part1 = GetNewOrgSupplierPart();
			part1.OP_PartNum = "PNUM1";
			part1.OP_IsActive = true;

			OrgSupplierPart part2 = GetNewOrgSupplierPart();
			part2.OP_PartNum = "PNUM2";
			part2.OP_IsActive = false;

			OrgSupplierPart part3 = GetNewOrgSupplierPart();
			part3.OP_PartNum = "PNUM3";
			part3.OP_IsActive = false;

			Factory.Save();

			OrgSupplierPartCollection supplierParts = new OrgSupplierPartCollection(Factory);

			OrgSupplierPartFilter.Both = true;
			supplierParts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Both", 3, supplierParts.Count);

			OrgSupplierPartFilter.Both = false;
			OrgSupplierPartFilter.Inactive = true;
			supplierParts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Inactive", 2, supplierParts.Count);

			OrgSupplierPartFilter.Both = false;
			OrgSupplierPartFilter.Inactive = false;
			OrgSupplierPartFilter.Active = true;
			supplierParts.Load(OrgSupplierPartFilter.Filter);
			AssertEquals("Active", 1, supplierParts.Count);
		}

		void SetupSomeParts()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";
			OrgHeader buyer = OrgHeader.New(Factory);
			buyer.OH_Code = "BUYER";

			OrgSupplierPart buyerPart = GetNewOrgSupplierPart();
			buyerPart.OP_PartNum = "TestOrganisationFilter";
			buyerPart.OP_Desc = "ASL2";
			OrgPartRelation relation1 = buyerPart.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation1.OU_OH = buyer.PK;

			OrgSupplierPart supplierPart = GetNewOrgSupplierPart();
			supplierPart.OP_Desc = "SL1";
			supplierPart.OP_PartNum = "TestOrganisationFilter";
			OrgPartRelation relation2 = supplierPart.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation2.OU_OH = supplier.PK;

			OrgSupplierPart supplierPart2 = GetNewOrgSupplierPart();
			supplierPart2.OP_Desc = "ASL1";
			supplierPart2.OP_PartNum = "123";
			OrgPartRelation supplierPart2relation = supplierPart2.RelatedOrganisations.AddNew();
			supplierPart2relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			supplierPart2relation.OU_OH = supplier.PK;

			OrgSupplierPart freePart = GetNewOrgSupplierPart();
			freePart.OP_PartNum = "TestOrganisationFilter";

			//par1
			OrgSupplierPart part1 = GetNewOrgSupplierPart();
			part1.OP_Desc = "AK";
			part1.OP_PartNum = "HelloTest";

			OrgPartRelation part1relation = part1.RelatedOrganisations.AddNew();
			part1relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part1relation.OU_OH = supplier.PK;

			//part2
			OrgSupplierPart part2 = GetNewOrgSupplierPart();
			part2.OP_Desc = "AK2";
			part2.OP_PartNum = "123TestHello";

			OrgPartRelation part2relation = part2.RelatedOrganisations.AddNew();
			part2relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part2relation.OU_OH = supplier.PK;

			//part3
			OrgSupplierPart part3 = GetNewOrgSupplierPart();
			part3.OP_Desc = "KA2";
			part3.OP_PartNum = "TestHello";

			OrgPartRelation part3relation = part3.RelatedOrganisations.AddNew();
			part3relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			part3relation.OU_OH = supplier.PK;

			Factory.Save();
		}

		public void TestProductCodePanelFilter()
		{
			SetupSomeParts();

			Clear();

			OrgSupplierPartFilter.CodeContains = true;
			OrgSupplierPartFilter.OP_PartNum = "Hello";

			OrgSupplierPartCollection result = new OrgSupplierPartCollection(Factory, OrgSupplierPartFilter.Filter);
			result.Load();
			AssertEquals(3, result.Count);

			Clear();

			OrgSupplierPartFilter.CodeContains = false;
			OrgSupplierPartFilter.CodeExact = true;
			OrgSupplierPartFilter.OP_PartNum = "123TestHello";

			result = new OrgSupplierPartCollection(Factory, OrgSupplierPartFilter.Filter);
			result.Load();
			AssertEquals(1, result.Count);
			AssertEquals("AK2", result[0].OP_Desc);

			Clear();

			OrgSupplierPartFilter.CodeContains = false;
			OrgSupplierPartFilter.CodeStartWith = true;
			OrgSupplierPartFilter.OP_PartNum = "Hello";

			result = new OrgSupplierPartCollection(Factory, OrgSupplierPartFilter.Filter);
			result.Load();
			AssertEquals(1, result.Count);

			Clear();
		}

		public void TestProductDescriptionPanelFilter()
		{
			SetupSomeParts();

			OrgSupplierPartFilter.DescContains = true;
			OrgSupplierPartFilter.OP_Desc = "ASL";

			OrgSupplierPartCollection result = new OrgSupplierPartCollection(Factory, OrgSupplierPartFilter.Filter);
			result.Load();
			AssertEquals(2, result.Count);

			Clear();

			OrgSupplierPartFilter.DescContains = false;
			OrgSupplierPartFilter.DescExact = true;
			OrgSupplierPartFilter.OP_Desc = "ASL1";

			result = new OrgSupplierPartCollection(Factory, OrgSupplierPartFilter.Filter);
			result.Load();
			AssertEquals(1, result.Count);
			AssertEquals("ASL1", result[0].OP_Desc);

			Clear();

			OrgSupplierPartFilter.DescContains = false;
			OrgSupplierPartFilter.DescStartsWith = true;
			OrgSupplierPartFilter.OP_Desc = "AS";

			result = new OrgSupplierPartCollection(Factory, OrgSupplierPartFilter.Filter);
			result.Load();
			AssertEquals(2, result.Count);
		}

		void Clear()
		{
			OrgSupplierPartFilter.ResetToDefaultValues();
		}

		public void TestIsExpensiveQuery()
		{
			AssertEquals(false, OrgSupplierPartFilter.IsExpensiveQuery);
			OrgSupplierPartFilter.CodeContains = true;
			AssertEquals(false, OrgSupplierPartFilter.IsExpensiveQuery);
			OrgSupplierPartFilter.OP_PartNum = "12";
			AssertEquals(true, OrgSupplierPartFilter.IsExpensiveQuery);
			OrgSupplierPartFilter.CodeContains = false;
			AssertEquals(false, OrgSupplierPartFilter.IsExpensiveQuery);
		}

		public void TestExpensiveQueryString()
		{
			OrgSupplierPartFilter.CodeContains = true;
			OrgSupplierPartFilter.OP_PartNum = "12";
			AssertEquals(true, OrgSupplierPartFilter.IsExpensiveQuery);
			AssertEquals(OldOrgSupplierPartFilterBusinessObject.ExpensiveQueryWarningText, OrgSupplierPartFilter.ExpensiveQueryWarning);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			OrgSupplierPartFilter = GetNewSupplierPartFilterBusinessObject();
		}

		protected virtual OldOrgSupplierPartFilterBusinessObject GetNewSupplierPartFilterBusinessObject()
		{
			return (OldOrgSupplierPartFilterBusinessObject)FilterBusinessObjectFactory.New(typeof(OldOrgSupplierPartFilterBusinessObject));
		}

		OldOrgSupplierPartFilterBusinessObject OrgSupplierPartFilter;

		protected virtual OrgSupplierPart GetNewOrgSupplierPart()
		{
			return Factory.New<OrgSupplierPart>();
		}

		#endregion
	}
}
