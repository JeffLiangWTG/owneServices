using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSupplierPartFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestConstructorCallingNewGetSupplierOwnerFilterDontResolveDuplicates()
		{
			OrgHeader importerMatch = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader importerNoMatch = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader importerCanResell = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplierMatch = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplierNoMatch = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierPart part1 = GetNewPart(commonPartCode, importerMatch, supplierMatch);
			OrgSupplierPart part2 = GetNewPart(commonPartCode, importerNoMatch, supplierMatch);
			OrgSupplierPart part3 = GetNewPart(commonPartCode, importerCanResell, supplierMatch);
			part3.OP_CanResell = false;
			Factory.Save();

			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var collection = new OrgSupplierPartCollection(Factory, supplierMatch, importerMatch, true, ZString.Empty, ZString.Empty, false);
				var additionalFilter = ((IBusinessObjectCollectionTestingMembers)collection).GetAdditionalFilter();
				collection.Load(additionalFilter);
				AssertCollectionContains(part1, collection);
				AssertCollectionNotContains(part2, collection);
				AssertEquals(true, collection.ForceSearchOnEnteringModule);
			}

			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var collection = new OrgSupplierPartCollection(Factory, supplierMatch, null, false, ZString.Empty, ZString.Empty, false);
				var additionalFilter = ((IBusinessObjectCollectionTestingMembers)collection).GetAdditionalFilter();
				collection.Load(additionalFilter);
				AssertCollectionContains(part1, collection);
				AssertCollectionContains(part2, collection);
				AssertEquals(false, collection.ForceSearchOnEnteringModule);

				collection = new OrgSupplierPartCollection(Factory, supplierMatch, null, false, PartFilterOptions.ExcludeNotForResale);
				additionalFilter = ((IBusinessObjectCollectionTestingMembers)collection).GetAdditionalFilter();
				collection.Load(additionalFilter);
				AssertCollectionContains(part1, collection);
				AssertCollectionContains(part2, collection);
				AssertCollectionNotContains(part3, collection);
			}
		}

		public void TestFilterDoesNotIncludeInactiveParts()
		{
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierPart part1 = GetNewPart(commonPartCode + "A", importer, supplier);
			part1.OP_IsActive = false;
			OrgSupplierPart part2 = GetNewPart(commonPartCode + "Z", importer, supplier);
			Factory.Save();

			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory, supplier, importer, true, ZString.Empty, ZString.Empty, false);
				ZQuery additionalFilter = ((IBusinessObjectCollectionTestingMembers)collection).GetAdditionalFilter();
				collection.Load(additionalFilter);
				AssertEquals("collection.Contains(Part1)", false, collection.Contains(part1));
				AssertEquals("collection.Contains(Part2)", true, collection.Contains(part2));

				OrgSupplierPartCollection collection1 = new OrgSupplierPartCollection(Factory, supplier, importer, false);
				ZQuery additionalFilter1 = ((IBusinessObjectCollectionTestingMembers)collection1).GetAdditionalFilter();
				collection1.Load(additionalFilter1);

				AssertEquals("collection.Contains(Part1)", false, collection1.Contains(part1));
				AssertEquals("collection.Contains(Part2)", true, collection1.Contains(part2));
			}
		}

		public void TestFilterIncludesInActivePartsWhenSpecified()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = GetNewPart(commonPartCode + "A", importer, supplier);
			var part2 = GetNewPart(commonPartCode + "Z", importer, supplier);
			part1.OP_IsActive = false;
			Factory.Save();

			var collection = new OrgSupplierPartCollection(Factory, supplier, importer, false, PartFilterOptions.IncludeInActive);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { part1, part2 }, collection);
		}

		public void TestGetSupplierOwnerFilterDontResolveDuplicates()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var noMatch = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = GetNewPart("P1", owner, null);
			var part2 = GetNewPart("P2", noMatch, null);
			var part3 = GetNewPart("P3", null, supplier, isActive: false);
			var part4 = GetNewPart("P4", null, noMatch);
			var part5 = GetNewPart("P5", owner, supplier);
			var part6 = GetNewPart("P6", owner, noMatch);
			var part7 = GetNewPart("P7", noMatch, supplier);
			var part8 = GetNewPart("P8", noMatch, noMatch);
			Factory.Save();

			AssertExceptionThrown(typeof(ArgumentNullException), () => OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, null, PartFilterOptions.OwnerMandatory));

			var filter1 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(null, null, PartFilterOptions.None);
			AssertEquals("Passing in null for Supplier & Owner should return no products.", 0, Factory.Load<OrgSupplierPart>(filter1).Length);

			var filter2 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(null, owner, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part5, part6 }, Factory.Load<OrgSupplierPart>(filter2));

			var filter3 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(null, owner, PartFilterOptions.OwnerMandatory);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part5, part6 }, Factory.Load<OrgSupplierPart>(filter3));

			var filter4 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, null, PartFilterOptions.IncludeInActive);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part3, part5, part7 }, Factory.Load<OrgSupplierPart>(filter4));

			var filter5 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, null, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part5, part7 }, Factory.Load<OrgSupplierPart>(filter5));

			var filter6 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, owner, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part5, part6, part7 }, Factory.Load<OrgSupplierPart>(filter6));

			var filter7 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, owner, PartFilterOptions.OwnerMandatory);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part5 }, Factory.Load<OrgSupplierPart>(filter7));
		}

		public void TestGetSupplierOwnerFilterDontResolveDuplicates_Both()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var noMatch = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = GetNewPart("P1", null, null, both: owner);
			var part2 = GetNewPart("P2", null, null, both: supplier);
			var part3 = GetNewPart("P3", null, null, both: noMatch);
			var part4 = GetNewPart("P4", null, supplier, both: owner);
			var part5 = GetNewPart("P5", owner, null, both: supplier);
			var part6 = GetNewPart("P6", owner, supplier, both: noMatch);
			var part7 = GetNewPart("P7", null, supplier, both: noMatch);
			var part8 = GetNewPart("P8", owner, null, both: noMatch);
			var part9 = GetNewPart("P9", noMatch, null, both: supplier);
			var part10 = GetNewPart("P10", null, noMatch, both: owner);
			var part11 = GetNewPart("P11", owner, noMatch, both: supplier);
			var part12 = GetNewPart("P12", noMatch, supplier, both: owner);
			var part13 = GetNewPart("P13", noMatch, noMatch, both: owner);
			var part14 = GetNewPart("P14", noMatch, noMatch, both: supplier);
			Factory.Save();

			var filter1 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(null, owner, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part4, part5, part6, part8, part10, part11, part12, part13 }, Factory.Load<OrgSupplierPart>(filter1));

			var filter2 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, null, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part2, part4, part5, part6, part7, part9, part11, part12, part14 }, Factory.Load<OrgSupplierPart>(filter2));

			var filter3 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, owner, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part2, part4, part5, part6, part7, part8, part9, part10, part11, part12, part13, part14 }, Factory.Load<OrgSupplierPart>(filter3));

			var filter4 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, owner, PartFilterOptions.OwnerMandatory);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part4, part5, part6, part11, part12 }, Factory.Load<OrgSupplierPart>(filter4));
		}

		public void TestGetSupplierOwnerFilterDontResolveDuplicates_ClassificationOrganisation()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var noMatch = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = GetNewPart("P1", null, null, cls: owner);
			var part2 = GetNewPart("P2", null, null, cls: supplier);
			var part3 = GetNewPart("P3", null, null, cls: noMatch);
			var part4 = GetNewPart("P4", null, supplier, cls: owner);
			var part5 = GetNewPart("P5", owner, null, cls: supplier);
			var part6 = GetNewPart("P6", owner, supplier, cls: noMatch);
			var part7 = GetNewPart("P7", null, supplier, cls: noMatch);
			var part8 = GetNewPart("P8", owner, null, cls: noMatch);
			var part9 = GetNewPart("P9", noMatch, null, cls: supplier);
			var part10 = GetNewPart("P10", null, noMatch, cls: owner);
			var part11 = GetNewPart("P11", owner, noMatch, cls: supplier);
			var part12 = GetNewPart("P12", noMatch, supplier, cls: owner);
			Factory.Save();

			var filter1 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(null, owner, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part4, part5, part6, part8, part10, part11, part12 }, Factory.Load<OrgSupplierPart>(filter1));

			var filter2 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, null, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part2, part4, part5, part6, part7, part9, part11, part12 }, Factory.Load<OrgSupplierPart>(filter2));

			var filter3 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, owner, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part2, part4, part5, part6, part7, part8, part9, part10, part11, part12 }, Factory.Load<OrgSupplierPart>(filter3));

			var filter4 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, owner, PartFilterOptions.OwnerMandatory);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part4, part5, part6, part8, part11, part12 }, Factory.Load<OrgSupplierPart>(filter4));
		}

		public void TestGetSupplierOwnerFilterDontResolveDuplicates_WithParent()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var ownerParent = GetNewOrganisationParent(owner);
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierParent = GetNewOrganisationParent(supplier);
			var noMatch = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = GetNewPart("P1", ownerParent, null);
			var part2 = GetNewPart("P2", null, supplierParent);
			var part3 = GetNewPart("P3", ownerParent, supplierParent);
			var part4 = GetNewPart("P4", null, null, both: ownerParent);
			var part5 = GetNewPart("P5", null, null, both: supplierParent);
			var part6 = GetNewPart("P6", null, supplier, both: ownerParent);
			var part7 = GetNewPart("P7", ownerParent, null, both: supplierParent);
			var part8 = GetNewPart("P8", owner, null, both: ownerParent);
			var part9 = GetNewPart("P9", null, supplier, both: supplierParent);
			var part10 = GetNewPart("P10", null, supplierParent, cls: ownerParent);
			var part11 = GetNewPart("P11", ownerParent, noMatch, both: supplierParent);
			var part12 = GetNewPart("P12", noMatch, supplierParent, both: ownerParent);
			var part13 = GetNewPart("P13", owner, ownerParent, null);
			var part14 = GetNewPart("P14", supplierParent, supplier, null);
			Factory.Save();

			var filter1 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(null, owner, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part3, part4, part6, part7, part8, part10, part11, part12, part13 }, Factory.Load<OrgSupplierPart>(filter1));

			var filter2 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, null, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part2, part3, part5, part6, part7, part9, part10, part11, part12, part14 }, Factory.Load<OrgSupplierPart>(filter2));

			var filter3 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, owner, PartFilterOptions.None);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part2, part3, part4, part5, part6, part7, part8, part9, part10, part11, part12, part13, part14 }, Factory.Load<OrgSupplierPart>(filter3));

			var filter4 = OrgSupplierPartCollection.GetSupplierOwnerFilterDontResolveDuplicates(supplier, owner, PartFilterOptions.OwnerMandatory);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part3, part4, part6, part7, part8, part10, part11, part12 }, Factory.Load<OrgSupplierPart>(filter4));
		}

		public void TestGetSupplierOwnerMatchesQuery()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var noMatch = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = GetNewPart("P1", owner, null);
			var part2 = GetNewPart("P2", noMatch, null);
			var part3 = GetNewPart("P3", null, supplier, isActive: false);
			var part4 = GetNewPart("P4", null, noMatch);
			var part5 = GetNewPart("P5", owner, supplier);
			var part6 = GetNewPart("P6", owner, noMatch);
			var part7 = GetNewPart("P7", noMatch, supplier);
			var part8 = GetNewPart("P8", noMatch, noMatch);
			Factory.Save();

			var filter1 = OrgSupplierPartCollection.GetSupplierOwnerMatchesQuery(supplier.PK, owner.PK, loadOnlyActiveProducts: false);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part3, part5 }, Factory.Load<OrgSupplierPart>(filter1));
			var filter2 = OrgSupplierPartCollection.GetSupplierOwnerMatchesQuery(supplier.PK, owner.PK, loadOnlyActiveProducts: true);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part5 }, Factory.Load<OrgSupplierPart>(filter2));
		}

		public void TestGetSupplierOwnerMatchesQuery_Both()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var noMatch = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = GetNewPart("P1", null, null, both: owner);
			var part2 = GetNewPart("P2", null, null, both: supplier);
			var part3 = GetNewPart("P3", null, null, both: noMatch);
			var part4 = GetNewPart("P4", null, supplier, both: owner);
			var part5 = GetNewPart("P5", owner, null, both: supplier);
			var part6 = GetNewPart("P6", owner, supplier, both: noMatch);
			var part7 = GetNewPart("P7", null, supplier, both: noMatch);
			var part8 = GetNewPart("P8", owner, null, both: noMatch);
			var part9 = GetNewPart("P9", noMatch, null, both: supplier);
			var part10 = GetNewPart("P10", null, noMatch, both: owner);
			var part11 = GetNewPart("P11", owner, noMatch, both: supplier);
			var part12 = GetNewPart("P12", noMatch, supplier, both: owner);
			var part13 = GetNewPart("P13", noMatch, noMatch, both: owner);
			Factory.Save();

			var filter = OrgSupplierPartCollection.GetSupplierOwnerMatchesQuery(supplier.PK, owner.PK, loadOnlyActiveProducts: false);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part2, part4, part5, part6, part11, part12 }, Factory.Load<OrgSupplierPart>(filter));
		}

		public void TestGetSupplierOwnerMatchesQuery_ClassificationOrganisation()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var noMatch = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = GetNewPart("P1", null, null, cls: owner);
			var part2 = GetNewPart("P2", null, null, cls: supplier);
			var part3 = GetNewPart("P3", null, null, cls: noMatch);
			var part4 = GetNewPart("P4", null, supplier, cls: owner);
			var part5 = GetNewPart("P5", owner, null, cls: supplier);
			var part6 = GetNewPart("P6", owner, supplier, cls: noMatch);
			var part7 = GetNewPart("P7", null, supplier, cls: noMatch);
			var part8 = GetNewPart("P8", owner, null, cls: noMatch);
			var part9 = GetNewPart("P9", noMatch, null, cls: supplier);
			var part10 = GetNewPart("P10", null, noMatch, cls: owner);
			var part11 = GetNewPart("P11", owner, noMatch, cls: supplier);
			var part12 = GetNewPart("P12", noMatch, supplier, cls: owner);
			var part13 = GetNewPart("P13", owner, noMatch, cls: owner);
			Factory.Save();

			var filter = OrgSupplierPartCollection.GetSupplierOwnerMatchesQuery(supplier.PK, owner.PK, loadOnlyActiveProducts: false);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part2, part4, part5, part6, part7, part8, part11, part12 }, Factory.Load<OrgSupplierPart>(filter));
		}

		public void TestGetSupplierOwnerMatchesQuery_WithParent()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var ownerParent = GetNewOrganisationParent(owner);
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierParent = GetNewOrganisationParent(supplier);
			var noMatch = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = GetNewPart("P1", ownerParent, null);
			var part2 = GetNewPart("P2", null, supplierParent);
			var part3 = GetNewPart("P3", ownerParent, supplierParent);
			var part4 = GetNewPart("P4", null, null, both: ownerParent);
			var part5 = GetNewPart("P5", null, null, both: supplierParent);
			var part6 = GetNewPart("P6", null, supplier, both: ownerParent);
			var part7 = GetNewPart("P7", ownerParent, null, both: supplierParent);
			var part8 = GetNewPart("P8", owner, null, both: ownerParent);
			var part9 = GetNewPart("P9", null, supplier, both: supplierParent);
			var part10 = GetNewPart("P10", null, supplierParent, cls: ownerParent);
			var part11 = GetNewPart("P11", ownerParent, noMatch, both: supplierParent);
			var part12 = GetNewPart("P12", noMatch, supplierParent, both: ownerParent);
			var part13 = GetNewPart("P13", owner, ownerParent, null);
			var part14 = GetNewPart("P14", supplierParent, supplier, null);
			Factory.Save();

			var filter = OrgSupplierPartCollection.GetSupplierOwnerMatchesQuery(supplier.PK, owner.PK, loadOnlyActiveProducts: false);
			AssertContainsExactElementsInAnyOrder(p => p.OP_PartNum, new[] { part1, part2, part3, part4, part5, part6, part7, part8, part9, part10, part11, part12 }, Factory.Load<OrgSupplierPart>(filter));
		}

		public void TestDescriptionFromCode()
		{
			AssertEquals("DescriptionFromCode", "part1", list.DescriptionFromCode("PTCODE"));

			partData.Part1.OP_PartNum = "PTCODE1";
			Factory.Save();

			AssertEquals("DescriptionFromCode", "part3", list.DescriptionFromCode("PTCODE"));

			partData.Part3.OP_PartNum = "PTCODE3";
			Factory.Save();

			AssertEquals("DescriptionFromCode", "part2", list.DescriptionFromCode("PTCODE"));
		}

		public void TestCodeFromPrimaryKey()
		{
			AssertEquals("CodeFromPrimaryKey", "PTCODE", list.CodeFromPrimaryKey(partData.Part1.PK));
			AssertEquals("CodeFromPrimaryKey", "PTCODE", list.CodeFromPrimaryKey(partData.Part2.PK));
			AssertEquals("CodeFromPrimaryKey", "PTCODE", list.CodeFromPrimaryKey(partData.Part3.PK));
		}

		public void TestNearestMatch()
		{
			AssertEquals("NearestMatch", "PTCODE", list.NearestMatch("pt", true, -1).Item1);
		}

		public void TestPrimaryKeyFromCode()
		{
			AssertEquals("PrimaryKeyFromCode", partData.Part1.PK, list.PrimaryKeyFromCode("PTCODE"));

			partData.Part1.OP_PartNum = "PTCODE1";
			Factory.Save();
			AssertEquals("PrimaryKeyFromCode", partData.Part3.PK, list.PrimaryKeyFromCode("PTCODE"));

			partData.Part3.OP_PartNum = "PTCODE3";
			Factory.Save();
			AssertEquals("PrimaryKeyFromCode", partData.Part2.PK, list.PrimaryKeyFromCode("PTCODE"));

			partData.Part2.OP_PartNum = "PTCODE2";
			Factory.Save();

			AssertEquals("PrimaryKeyFromCode", ZGuid.Invalid, list.PrimaryKeyFromCode("PTCODE"));
			AssertEquals("PrimaryKeyFromCode", ZGuid.Invalid, list.PrimaryKeyFromCode("123456"));
			AssertEquals("PrimaryKeyFromCode", ZGuid.Empty, list.PrimaryKeyFromCode(""));
		}

		public void TestLargeProductDescriptionIsTruncated()
		{
			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory, null, null, false, new ZString('x', 500), ZString.Empty, false);
			OrgSupplierPart part = collection.AddNew();
			AssertEquals(new ZString('x', part.OP_DescInfo.MaxLength), part.OP_Desc);
		}

		public void TestInactiveProductsHaveErrorMessage()
		{
			OrgHeader owner = Factory.NewWithValidTestData<OrgHeader>();
			OrgSupplierPart part = GetNewPart(commonPartCode, owner, null);
			part.OP_IsActive = false;

			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory, null, owner, false);
			AssertContains(OrgSupplierPartCollection.ProductIsNotActiveErrorMessage, collection.GetAllNotificationsWhenAdditionalFilterNotMet(part));
		}

		public void TestNonResellProductsHaveErrorMessage()
		{
			OrgHeader owner = Factory.NewWithValidTestData<OrgHeader>();
			OrgSupplierPart part = GetNewPart(commonPartCode, owner, null);
			part.OP_CanResell = false;

			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory, null, owner, false, PartFilterOptions.ExcludeNotForResale);
			AssertContains(OrgSupplierPartCollection.ProductIsNotForResaleErrorMessage, collection.GetAllNotificationsWhenAdditionalFilterNotMet(part));

			collection = new OrgSupplierPartCollection(Factory, null, owner, false);
			AssertNotContains("Shows error message but resellable is not in filter",
				OrgSupplierPartCollection.ProductIsNotForResaleErrorMessage, collection.GetAllNotificationsWhenAdditionalFilterNotMet(part));
		}

		public void TestErrorMessagesForAllFilterRestrictions()
		{
			OrgHeader owner = Factory.NewWithValidTestData<OrgHeader>();

			OrgSupplierPart validPart = GetNewPart("Valid", owner, null);
			OrgSupplierPart invalidPart = GetNewPart("Not Valid", owner, null);
			invalidPart.OP_IsActive = false;
			invalidPart.OP_CanResell = false;

			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory, null, owner, false, PartFilterOptions.ExcludeNotForResale);

			AssertNotContains("Shows error message but product is active"
				, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage, collection.GetAllNotificationsWhenAdditionalFilterNotMet(validPart));
			AssertNotContains("Shows error message but product is resellable"
				, OrgSupplierPartCollection.ProductIsNotForResaleErrorMessage, collection.GetAllNotificationsWhenAdditionalFilterNotMet(validPart));
			AssertContains(OrgSupplierPartCollection.ProductIsNotActiveErrorMessage, collection.GetAllNotificationsWhenAdditionalFilterNotMet(invalidPart));
			AssertContains(OrgSupplierPartCollection.ProductIsNotForResaleErrorMessage, collection.GetAllNotificationsWhenAdditionalFilterNotMet(invalidPart));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			partData = new PartDataSetupHelper();
			partData.Setup(Factory);

			OrgSupplierPartCollection collection = new OrgSupplierPartCollection(Factory, partData.Supplier, partData.Buyer, false);
			list = collection;
		}

		PartDataSetupHelper partData;
		IFindBoxListProvider list;

		OrgHeader GetNewOrganisationParent(OrgHeader child)
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = parent.AllRelatedParties.AddNew();
			orgRelatedParty.PR_OH_RelatedParty = child.PK;
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			return parent;
		}

		OrgSupplierPart GetNewPart(ZString partNo, OrgHeader importer, OrgHeader supplier, OrgHeader both = null, OrgHeader cls = null, bool isActive = true)
		{
			OrgSupplierPart result = Factory.New<OrgSupplierPart>();
			result.OP_PartNum = partNo;
			if (supplier != null)
			{
				result.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
			}
			if (importer != null)
			{
				result.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			}
			if (both != null)
			{
				result.RelatedOrganisations.AddOrganisationIfNotExist(both.PK, OrgPartRelation.RelationshipTypes.Both);
			}
			if (cls != null)
			{
				result.RelatedOrganisations.AddOrganisationIfNotExist(cls.PK, OrgPartRelation.RelationshipTypes.ClassificationOrganization);
			}
			result.OP_IsActive = isActive;
			return result;
		}

		const string commonPartCode = "NEVERUSETHISCODE";

		#endregion
	}

	[TestedType(typeof(OrgSupplierPartCollection))]
	public class OrgSupplierPartCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollection()
		{
			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "JohnDrop1";
			buyer.MainAddress.OA_Address1 = "Add 1";
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "JohnDrop2";
			supplier.MainAddress.OA_Address1 = "Add 1";

			OrgSupplierPart part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "1";
			OrgPartRelation relationPart1 = part1.RelatedOrganisations.AddNew();
			relationPart1.OU_OH = supplier.PK;
			relationPart1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			OrgSupplierPart part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "2";
			OrgPartRelation relationPart2 = part2.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = buyer.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			OrgSupplierPart partThatIsAlreadyOnOwner = OrgSupplierPart.New(Factory);
			partThatIsAlreadyOnOwner.OP_PartNum = "2";
			OrgPartRelation relationBogusPart = partThatIsAlreadyOnOwner.RelatedOrganisations.AddNew();
			relationBogusPart.OU_OH = supplier.PK;
			relationBogusPart.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			OrgSupplierPartCollection parts = new OrgSupplierPartCollection(Factory, supplier, buyer, false);
			parts.Load();
			AssertEquals("Part list contains part 1", true, parts.Contains(part1.PK));
			AssertEquals("Part list contains part 2", true, parts.Contains(part2.PK));
			AssertEquals("Parts.Contains(PartThatIsAlreadyOnOwner.PK)", false, parts.Contains(partThatIsAlreadyOnOwner.PK));
		}

		public void TestConstructor()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			AssertExceptionThrown(typeof(ArgumentNullException), () => new OrgSupplierPartCollection(Factory, supplier, null, false, PartFilterOptions.OwnerMandatory));
		}

		public void TestFindBoxListProviderReturned()
		{
			OrgSupplierPartCollection parts = new OrgSupplierPartCollection(Factory);
			Object obj = typeof(OrgSupplierPartCollection).GetProperty("FindBoxListProvider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parts, null);
			AssertEquals("CargoWise.EntityFramework.FindBoxListProvider", obj.ToString());
			OrgHeader buyer = Factory.New<OrgHeader>();
			OrgHeader supplier = Factory.New<OrgHeader>();
			parts = new OrgSupplierPartCollection(Factory, supplier, buyer, false);
			obj = typeof(OrgSupplierPartCollection).GetProperty("FindBoxListProvider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parts, null);
			AssertEquals("Enterprise.MasterFiles.Business.OrgSupplierPartCollection+OrgSupplierPartFindBoxListProvider", obj.ToString());
		}

		public void TestOrgSupplierPartCollectionUtilisesInactiveFilters()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "P1";
			part.RelatedOrganisations.AddOwner(owner);
			part.RelatedOrganisations.AddSupplier(supplier);
			part.OP_IsActive = false;
			Factory.Save();

			var collection = new OrgSupplierPartCollection(Factory, supplier, owner, false, PartFilterOptions.IncludeInActive);
			collection.Load();
			AssertContainsExactElementsInAnyOrder("Precondition: Inactive parts should be found in the collection.", new[] { part }, collection);

			IFindBoxListProvider findBoxListProvider = collection;
			AssertEquals("ZGuid should match part", part.PK, findBoxListProvider.PrimaryKeyFromCode(part.OP_PartNum));

			var collectionWithOnlyActiveParts = new OrgSupplierPartCollection(Factory, supplier, owner, false);
			collectionWithOnlyActiveParts.Load();
			AssertEquals("Precondition: There should not be inactive parts in the collection.", 0, collectionWithOnlyActiveParts.Count);

			IFindBoxListProvider findBoxListProviderFromActivePartCollection = collectionWithOnlyActiveParts;
			AssertEquals("ZGuid should be invalid", ZGuid.Invalid, findBoxListProviderFromActivePartCollection.PrimaryKeyFromCode(part.OP_PartNum));
		}

		public void TestLoad_RemovesDuplicatesOnlyIfTheyAreDifferentProducts()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "P1";
			part.RelatedOrganisations.AddOwner(owner);
			part.RelatedOrganisations.AddSupplier(supplier);
			Factory.Save();

			var collection = new OrgSupplierPartCollection(Factory, supplier, owner, false);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { part }, collection);
		}

		public void TestOwnerOverridesSupplierPart()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "sup";
			supplier.MainAddress.OA_Address1 = "supaddr";
			OrgSupplierPart part1 = OrgSupplierPart.New(Factory);
			part1.OP_PartNum = "splat";
			OrgPartRelation relationPart1 = part1.RelatedOrganisations.AddNew();
			relationPart1.OU_OH = supplier.PK;
			relationPart1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "buy";
			buyer.MainAddress.OA_Address1 = "buyaddr";
			OrgSupplierPart part2 = OrgSupplierPart.New(Factory);
			part2.OP_PartNum = "splat";
			OrgPartRelation relationPart2 = part2.RelatedOrganisations.AddNew();
			relationPart2.OU_OH = buyer.PK;
			relationPart2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Factory.Save();

			OrgSupplierPartCollection parts = new OrgSupplierPartCollection(Factory, supplier, buyer, false);
			parts.Load();
			AssertEquals("Contains buyer part", true, parts.Contains(part2.PK));
			AssertEquals("Doesn't contain supplier part", false, parts.Contains(part1.PK));
		}

		#region TestDefaultsForNewChild

		public void TestDefaultsForNewChild()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "sup";
			supplier.MainAddress.OA_Address1 = "supaddr";

			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "buy";
			buyer.MainAddress.OA_Address1 = "buyaddr";
			buyer.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.AccrualBasis.Code;
			buyer.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			buyer.OH_RL_NKClosestPort = "AUSYD";

			OrgSupplierPartCollection parts = new OrgSupplierPartCollection(Factory, supplier, buyer, false);
			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Supplier = supplier.PK;
			link.OL_OH_Buyer = buyer.PK;
			link.OL_RN_NKImporterCountry = "AU";

			link.OL_ProductRelation = OrgRelationTypeList.Codes.Supplier;
			Factory.Save();
			OrgSupplierPart part1 = parts.AddNew();
			part1.OP_PartNum = "001";
			AssertNotNull("Should be supplier", part1.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier));

			link.OL_ProductRelation = OrgRelationTypeList.Codes.Importer;
			Factory.Save();
			OrgSupplierPart part2 = parts.AddNew();
			part2.OP_PartNum = "002";
			AssertNotNull("Should be owner", part2.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer.PK, OrgPartRelation.RelationshipTypes.Owner));

			link.OL_ProductRelation = ZString.Empty;
			Factory.Save();
			buyer.CountryData.OV_MakePartsBothImportAndExport = true;
			OrgSupplierPart part3 = parts.AddNew();
			part3.OP_PartNum = "003";
			AssertNotNull("Should be both", part3.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer.PK, OrgPartRelation.RelationshipTypes.Both));

			buyer.CountryData.OV_MakePartsBothImportAndExport = false;
			OrgSupplierPart part4 = parts.AddNew();
			part4.OP_PartNum = "004";
			AssertNull("Should not be both", part4.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(buyer.PK, OrgPartRelation.RelationshipTypes.Both));
		}

		public void TestDefaultsForNewChild_SetProperties()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "sup";
			supplier.MainAddress.OA_Address1 = "supaddr";

			OrgSupplierPartCollection partsCollection1 = new OrgSupplierPartCollection(Factory, supplier, null, false);
			OrgSupplierPart part1 = partsCollection1.AddNew();
			AssertEquals("", part1.OP_Desc);
			AssertEquals("UNT", part1.OP_StockKeepingUnit);

			OrgSupplierPartCollection partsCollection2 = new OrgSupplierPartCollection(Factory, supplier, null, "ProductDescription", "BAG", false);
			OrgSupplierPart part2 = partsCollection2.AddNew();
			AssertEquals("ProductDescription", part2.OP_Desc);
			AssertEquals("BAG", part2.OP_StockKeepingUnit);
		}

		public void TestSetDefaultsForNewChildPerRelationShipType()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "imp";
			importer.MainAddress.OA_Address1 = "impaddr";
			importer.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.AccrualBasis.Code;
			importer.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			importer.OH_RL_NKClosestPort = "AUSYD";

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "sup";
			supplier.MainAddress.OA_Address1 = "supaddr";

			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Supplier = supplier.PK;
			link.OL_OH_Buyer = importer.PK;
			link.OL_ProductRelation = OrgRelationTypeList.Codes.Supplier;
			Factory.Save();

			var partsCollection = new OrgSupplierPartCollection(Factory, supplier, importer, false);
			var part1 = partsCollection.AddNew();
			AssertEquals("Relationship for Supplier", OrgPartRelation.RelationshipTypes.Supplier, part1.RelatedOrganisations.FindFirstByOrganisationPK(supplier.PK).OU_Relationship);

			link.OL_ProductRelation = OrgRelationTypeList.Codes.Importer;
			var part2 = partsCollection.AddNew();
			AssertEquals("Relationship for Importer", OrgPartRelation.RelationshipTypes.Owner, part2.RelatedOrganisations.FindFirstByOrganisationPK(importer.PK).OU_Relationship);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgSupplierPartCollection(Factory);
		}

		#endregion
	}
}
