using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ForwardingPackingLineCollectionReader))]
	public class ForwardingPackingLineCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Complete;

			var packingLineDO1 = CreatePackingLineDO(1, "BOX", "REF0001", "E0001", "description1", "Marks1");
			var packingLineDO2 = CreatePackingLineDO(2, "PKG", "REF0002", "E0002", "description2", "Marks2");
			var packingLineDO3 = CreatePackingLineDO(3, "PKG", "REF0003", "E0003", "description3", "Marks3");
			var packingLineDO4 = CreatePackingLineDO(4, "PKG", "REF0004", "E0004", "description4", "Marks4");
			var packingLineDO5 = CreatePackingLineDO(5, "PKG", "REF0005", "E0005", "description5", "Marks5");
			var packingLineDO6 = CreatePackingLineDO(6, "PKG", "REF0006", "E0006", "description6", "Marks6");

			packingLineDOs.AddRange(new[] { packingLineDO1, packingLineDO2, packingLineDO3, packingLineDO4, packingLineDO5, packingLineDO6 });

			var shipment = Factory.New<ForwardingShipment>();

			CreateForwardingPackingLine(shipment, 1, "BOX", "REF0001", "E0001", "description1", "Marks1");
			CreateForwardingPackingLine(shipment, 2, "PKG", "REF0002", "E0002", "", "");
			CreateForwardingPackingLine(shipment, 3, "PKG", "REF0003", "E0003", "", "");
			CreateForwardingPackingLine(shipment, 4, "PKG", "REF0004", "E0004", "description4", "");
			CreateForwardingPackingLine(shipment, 5, "PKG", "REF0005", "E0005", "", "Marks5");
			CreateForwardingPackingLine(shipment, 6, "PKG", "REF0006", "E0006", "", "");
			CreateForwardingPackingLine(shipment, 6, "PKG", "REF0006", "E0006", "", "");
			CreateForwardingPackingLine(shipment, 10, "PKG", "REF00010", "E00010", "description10", "Marks10");

			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(6, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "REF0001", "E0001", "description1", "Marks1");
			AssertPackingLineBO(shipment.OuterPackLines[1], true, 2, "PKG", "REF0002", "E0002", "description2", "Marks2");
			AssertPackingLineBO(shipment.OuterPackLines[2], true, 3, "PKG", "REF0003", "E0003", "description3", "Marks3");
			AssertPackingLineBO(shipment.OuterPackLines[3], true, 4, "PKG", "REF0004", "E0004", "description4", "Marks4");
			AssertPackingLineBO(shipment.OuterPackLines[4], true, 5, "PKG", "REF0005", "E0005", "description5", "Marks5");
			AssertPackingLineBO(shipment.OuterPackLines[5], false, 6, "PKG", "REF0006", "E0006", "description6", "Marks6");
		}

		public void TestReadIntoCollection_SingleToSingle_CollectionContent_Complete()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLineBO = shipment.OuterPackLines.AddNew();
			packLineBO.JL_PackageCount = 1;
			packLineBO.JL_F3_NKPackType = "BOX";
			packLineBO.JL_RefNumber = "HELLO";

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType { Code = "BOX" },
				ReferenceNumber = "HELLO"
			};

			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Complete;

			packingLineDOs.Add(packingLine);

			// Matched
			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertEquals("MATCHED: existing packline was updated, no deleting/adding new", packLineBO.PK, shipment.OuterPackLines[0].PK);

			// Not matched
			packingLine.ReferenceNumber = "HEY HEY";

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertEquals("NOT MATCHED: Existing packline was updated, no deleting/adding new", packLineBO.PK, shipment.OuterPackLines[0].PK);
			AssertEquals("HEY HEY", packLineBO.JL_RefNumber);
		}

		public void TestReadIntoCollection_SingleToSingle_CollectionContent_Partial()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packLineBO = shipment.OuterPackLines.AddNew();
			packLineBO.JL_PackageCount = 1;
			packLineBO.JL_F3_NKPackType = "BOX";
			packLineBO.JL_RefNumber = "HELLO";

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PackQty = 1,
				PackType = new PackageType { Code = "BOX" },
				ReferenceNumber = "HELLO"
			};

			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Partial;

			packingLineDOs.Add(packingLine);

			// Matched
			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertEquals("MATCHED: existing packline was updated, no deleting/adding new", packLineBO.PK, shipment.OuterPackLines[0].PK);

			// Not matched
			packingLine.ReferenceNumber = "HEY HEY";

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			CombineAssertions("NOT MATCHED: Existing packline left unchanged, new packline imported", () =>
			{
				AssertEquals(2, shipment.OuterPackLines.Count);
				AssertEquals(packLineBO.PK, shipment.OuterPackLines[0].PK);
				AssertEquals("HELLO", shipment.OuterPackLines[0].JL_RefNumber);
				AssertEquals("HEY HEY", shipment.OuterPackLines[1].JL_RefNumber);
			});
		}

		public void TestReadIntoCollection_WithUXMLContainsUNDGAndProductUNDG()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "ABC";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "PRODUCT";
			var orgPartRelationOwner = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var undg = orgSupplierPart.UNDGs.AddNew();
			undg.LinkDefault(subs);

			var packedItem1 = new PackedItem();
			packedItem1.Product = new Product { Code = "PRODUCT" };
			var undgDO = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);
			undgDO.UNDGCode = "ABC";
			undgDO.IMOClass = "2.2";

			var packingLineDO1 = CreatePackingLineDO(1, "BOX", "REF0001", "E0001", "description1", "Marks1");
			packingLineDO1.SetUNDGCollection(() => new List<UNDG> { undgDO });
			packingLineDO1.SetPackedItemCollection(() => new List<PackedItem> { packedItem1 });

			var packingLineDOs = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };
			packingLineDOs.Add(packingLineDO1);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = orgPartRelationOwner.OU_OH;

			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();
			AssertEquals("Should contains 1 Outer PackLine", 1, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines[0];
			AssertNotNull(outerPackLine);
			AssertEquals("Should contains 2 UNDGs", 2, outerPackLine.UNDGs.Count);
		}

		public void TestReadIntoCollection_WithUXMLOnlyHaveProduct()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "AB";
			subs.DG_Variant = "C";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "PRODUCT";
			var orgPartRelationOwner = orgSupplierPart.RelatedOrganisations.Cast<OrgPartRelation>().FirstOrDefault();
			orgPartRelationOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var undg = orgSupplierPart.UNDGs.AddNew();
			undg.LinkDefault(subs);

			var packedItem1 = new PackedItem();
			packedItem1.Product = new Product { Code = "PRODUCT" };

			var packingLineDO1 = CreatePackingLineDO(1, "BOX", "REF0001", "E0001", "description1", "Marks1");
			packingLineDO1.SetPackedItemCollection(() => new List<PackedItem> { packedItem1 });

			var packingLineDOs = new DataObjectList<PackingLine>() { Content = CollectionContent.Complete };
			packingLineDOs.Add(packingLineDO1);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = orgPartRelationOwner.OU_OH;

			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();
			AssertEquals(1, shipment.OuterPackLines.Count);

			var outerPackLine = shipment.OuterPackLines[0];
			AssertNotNull(outerPackLine);
			AssertEquals(1, outerPackLine.UNDGs.Count);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			var newShipment = newFactory.Load<ForwardingShipment>(shipment.PK);

			reader = GetReader(packingLineDOs, newShipment, newFactory);
			reader.ReadIntoCollection();
			AssertEquals(1, shipment.OuterPackLines.Count);

			outerPackLine = shipment.OuterPackLines[0];
			AssertNotNull(outerPackLine);
			AssertEquals(1, outerPackLine.UNDGs.Count);
		}

		public void TestReadIntoCollection_DuplicatedPackingLineDataObject()
		{
			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Complete;

			var packingLineDO1 = CreatePackingLineDO(1, "BOX", "REF0001", "E0001", "description1", "Marks1");
			var packingLineDO2 = CreatePackingLineDO(1, "BOX", "REF0001", "E0001", "description1", "Marks1");

			packingLineDOs.AddRange(new[] { packingLineDO1, packingLineDO2 });

			var shipment = Factory.New<ForwardingShipment>();

			CreateForwardingPackingLine(shipment, 1, "BOX", "REF0001", "E0001", "description1", "Marks1");

			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(2, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "REF0001", "E0001", "description1", "Marks1");
			AssertPackingLineBO(shipment.OuterPackLines[1], false, 1, "BOX", "REF0001", "E0001", "description1", "Marks1");
		}

		public void TestReadIntoCollection_MatchPackagesNumberAndType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Complete;

			//case1: both Packages number and type matched: MATCH

			var packingLineDO1 = CreatePackingLineDO(1, "BOX", "A1", "", "", "Marks1");
			var packingLineBO1 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "", "", "");
			var packingLineBO2 = CreateForwardingPackingLine(shipment, 2, "PKG", "A1", "", "", "");

			packingLineDOs.AddRange(new[] { packingLineDO1 });

			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "A1", "", "", "Marks1");

			//case2: multil-lines matched: NOT A MATCH

			shipment.OuterPackLines.RemoveAndDeleteAll();
			packingLineBO1 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "", "", "");
			packingLineBO2 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "", "", "");

			packingLineDOs.Clear();
			packingLineDOs.Add(packingLineDO1);

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "A1", "", "", "Marks1");

			// case3: Packages Number is null: NOT A MATCH

			shipment.OuterPackLines.RemoveAndDeleteAll();
			packingLineBO1 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "", "", "");
			packingLineBO2 = CreateForwardingPackingLine(shipment, 2, "PKG", "A1", "", "", "");
			packingLineDO1 = CreatePackingLineDO(null, "BOX", "A1", "", "", "Marks1");

			packingLineDOs.Clear();
			packingLineDOs.Add(packingLineDO1);
			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 0, "BOX", "A1", "", "", "Marks1");

			// case4: Packages Type is null: NOT A MATCH

			shipment.OuterPackLines.RemoveAndDeleteAll();
			packingLineBO1 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "", "", "");
			packingLineBO2 = CreateForwardingPackingLine(shipment, 2, "PKG", "A1", "", "", "");
			packingLineDO1 = CreatePackingLineDO(1, null, "A1", "", "", "Marks1");

			packingLineDOs.Clear();
			packingLineDOs.Add(packingLineDO1);
			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "", "A1", "", "", "Marks1");
		}

		public void TestReadIntoCollection_MatchRefNumberAndExportRefNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Complete;

			Action initializePackingLineBOs = () =>
			{
				shipment.OuterPackLines.RemoveAndDeleteAll();
				var packingBO1 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "B1", "", "");
				var packingBO2 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "", "", "");
				var packingBO3 = CreateForwardingPackingLine(shipment, 1, "BOX", "", "B1", "", "");
				var packingBO4 = CreateForwardingPackingLine(shipment, 1, "BOX", "", "", "", "");
			};

			Action<ZString?, ZString?> initializePackingLineDOs = (ZString? refNumber, ZString? exportRefNumber) =>
			{
				packingLineDOs = new DataObjectList<PackingLine>();
				var packingDO1 = CreatePackingLineDO(1, "BOX", refNumber, exportRefNumber, "", "Marks1");
				packingLineDOs.AddRange(new[] { packingDO1 });
			};

			//case1: (A1, B1): match packingBO1

			initializePackingLineBOs();
			initializePackingLineDOs("A1", "B1");

			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "A1", "B1", "", "Marks1");

			//case2: (A1, ""): match packingBO2

			initializePackingLineBOs();
			initializePackingLineDOs("A1", "");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "A1", "", "", "Marks1");

			//case3: (A1, null): match packingBO1 and packingBO2

			initializePackingLineBOs();
			initializePackingLineDOs("A1", null);

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "A1", "", "", "Marks1");

			//case4: ("", B1): match packingBO3

			initializePackingLineBOs();
			initializePackingLineDOs("", "B1");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "", "B1", "", "Marks1");

			//case5: ("", ""): no match

			initializePackingLineBOs();
			initializePackingLineDOs("", "");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "", "Marks1");

			//case6: ("", null): no match

			initializePackingLineBOs();
			initializePackingLineDOs("", null);

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "", "Marks1");

			//case7: (null, "B1"): match packingBO1 and packingBO3

			initializePackingLineBOs();
			initializePackingLineDOs(null, "B1");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "B1", "", "Marks1");

			//case8: (null, ""): no match

			initializePackingLineBOs();
			initializePackingLineDOs(null, "");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "", "Marks1");

			//case9: (null, null): no match

			initializePackingLineBOs();
			initializePackingLineDOs(null, null);

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "", "Marks1");
		}

		public void TestReadIntoCollection_MatchGoodsDescriptionAndMarksNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Complete;

			Action initializePackingLineBOs = () =>
			{
				shipment.OuterPackLines.RemoveAndDeleteAll();
				var packingBO1 = CreateForwardingPackingLine(shipment, 1, "BOX", "", "", "A1", "B1");
				var packingBO2 = CreateForwardingPackingLine(shipment, 1, "BOX", "", "", "A1", "");
				var packingBO3 = CreateForwardingPackingLine(shipment, 1, "BOX", "", "", "", "B1");
				var packingBO4 = CreateForwardingPackingLine(shipment, 1, "BOX", "", "", "", "");
			};

			Action<ZString?, ZString?> initializePackingLineDOs = (ZString? descrition, ZString? marksAndNos) =>
			{
				packingLineDOs = new DataObjectList<PackingLine>();
				var packingDO1 = CreatePackingLineDO(1, "BOX", "", "", descrition, marksAndNos);
				packingLineDOs.AddRange(new[] { packingDO1 });
			};

			//case1: (A1, B1): match packingBO1

			initializePackingLineBOs();
			initializePackingLineDOs("A1", "B1");

			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "", "", "A1", "B1");

			//case2: (A1, ""): match packingBO2

			initializePackingLineBOs();
			initializePackingLineDOs("A1", "");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "", "", "A1", "");

			//case3: (A1, null): match packingBO1 and packingBO2

			initializePackingLineBOs();
			initializePackingLineDOs("A1", null);

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "A1", "");

			//case4: ("", B1): match packingBO3

			initializePackingLineBOs();
			initializePackingLineDOs("", "B1");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "", "", "", "B1");

			//case5: ("", ""): no match

			initializePackingLineBOs();
			initializePackingLineDOs("", "");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "", "");

			//case6: ("", null): no match

			initializePackingLineBOs();
			initializePackingLineDOs("", null);

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "", "");

			//case7: (null, "B1"): match packingBO1 and packingBO3

			initializePackingLineBOs();
			initializePackingLineDOs(null, "B1");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "", "B1");

			//case8: (null, ""): no match

			initializePackingLineBOs();
			initializePackingLineDOs(null, "");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "", "");

			//case9: (null, null): no match

			initializePackingLineBOs();
			initializePackingLineDOs(null, null);

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "", "");
		}

		public void TestReadIntoCollection_MatchGoodsDescriptionAndMarksNumber_FallbackForEmptyReferences()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Complete;

			Action initializePackingLineBOs = () =>
			{
				shipment.OuterPackLines.RemoveAndDeleteAll();
				var packingBO1 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "B1", "C1", "D1");
				var packingBO2 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "", "C2", "D2");
				var packingBO3 = CreateForwardingPackingLine(shipment, 1, "BOX", "", "B1", "C3", "D3");
				var packingBO4 = CreateForwardingPackingLine(shipment, 1, "BOX", "", "", "C4", "D4");
				var packingBO5 = CreateForwardingPackingLine(shipment, 1, "PLT", "", "B3", "C3", "D3");
			};

			Action<ZString?, ZString?, ZString?, ZString?> initializePackingLineDOs = (ZString? refNumber, ZString? exportRefNumber, ZString? descrition, ZString? marksAndNos) =>
			{
				packingLineDOs = new DataObjectList<PackingLine>();
				var packingDO1 = CreatePackingLineDO(1, "BOX", refNumber, exportRefNumber, descrition, marksAndNos);
				packingLineDOs.AddRange(new[] { packingDO1 });
			};

			//case1: ("", null, "C3", "D3"): match packingBO3

			initializePackingLineBOs();
			initializePackingLineDOs("", null, "C3", "D3");

			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "", "B1", "C3", "D3");

			//case2: ("", null, "C4", "D4"): match packingBO4

			initializePackingLineBOs();
			initializePackingLineDOs("", null, "C4", "D4");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "", "", "C4", "D4");

			//case3: (null, "", "C2", "D2"): match packingBO2

			initializePackingLineBOs();
			initializePackingLineDOs(null, "", "C2", "D2");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "A1", "", "C2", "D2");

			//case4: (null, "", "C4", "D4"): match packingBO4

			initializePackingLineBOs();
			initializePackingLineDOs(null, "", "C4", "D4");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "", "", "C4", "D4");

			//case5: (null, null, "C2", "D2"): match packingBO2

			initializePackingLineBOs();
			initializePackingLineDOs(null, null, "C2", "D2");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "A1", "", "C2", "D2");

			//case6: (null, "", "C2", "D2"), there are multi fallbacks

			initializePackingLineBOs();
			CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "", "C5", "D5");

			initializePackingLineDOs(null, "", "C2", "D2");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], false, 1, "BOX", "", "", "C2", "D2");
		}

		public void TestReadIntoCollection_Partial_WithMutilDangerousGoods()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "123", "a", "IMO").FirstOrDefault();
			subs.DG_Class = "1.1A";

			var shipment = Factory.New<ForwardingShipment>();
			var packingLineBO = CreateForwardingPackingLine(shipment, 1, "BOX", "REF0001", "E0001", "OldDes", "Marks");

			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Partial;

			var packingLineDO = CreatePackingLineDO(1, "BOX", "REF0001", "E0001", "NewDes", "NewMarks");

			packingLineDOs.AddRange(new[] { packingLineDO });

			//case1: there is 1 DG record for db PackingLine, but no DG record in xml: MATCH

			packingLineBO.UNDGs.Add(Factory.New<UNDGDataItem>());

			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);

			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "REF0001", "E0001", "NewDes", "NewMarks");

			//case2: there are multi DG records for db PackingLine,but no DG record in xml: MATCH

			shipment.OuterPackLines.RemoveAndDeleteAll();
			packingLineBO = CreateForwardingPackingLine(shipment, 1, "BOX", "REF0001", "E0001", "OldDes", "Marks");
			packingLineBO.UNDGs.Add(Factory.New<UNDGDataItem>());

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "REF0001", "E0001", "NewDes", "NewMarks");

			//case3: there is 1 DG record for db PackingLine, and multi DG records in xml: NOT A MATCH

			shipment.OuterPackLines.RemoveAndDeleteAll();
			packingLineBO = CreateForwardingPackingLine(shipment, 1, "BOX", "REF0001", "E0001", "OldDes", "Marks");

			var undgBO1 = Factory.New<UNDGDataItem>();

			undgBO1.DI_DG = subs.PK;
			undgBO1.DI_IMOClass = "";

			packingLineDO.SetUNDGCollection(() => new List<UNDG>());
			var undgDO1 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				UNDGCode = subs.DG_Code,
				IMOClass = ""
			};
			var undgDO2 = new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				UNDGCode = subs.DG_Code,
				IMOClass = ""
			};

			packingLineDO.UNDGCollection.AddRange(new[] { undgDO1, undgDO2 });

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(2, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "REF0001", "E0001", "OldDes", "Marks");
			AssertPackingLineBO(shipment.OuterPackLines[1], false, 1, "BOX", "REF0001", "E0001", "NewDes", "NewMarks");

			//case4: there are multi DG records for db PackingLine, and multi DG records in xml: NOT A MATCH

			shipment.OuterPackLines.RemoveAndDeleteAll();
			packingLineBO = CreateForwardingPackingLine(shipment, 1, "BOX", "REF0001", "E0001", "OldDes", "Marks");

			undgBO1 = Factory.New<UNDGDataItem>();

			undgBO1.DI_DG = subs.PK;
			undgBO1.DI_IMOClass = "";
			var undgBO2 = Factory.New<UNDGDataItem>();

			undgBO2.DI_DG = subs.PK;
			undgBO2.DI_IMOClass = "";

			packingLineBO.UNDGs.AddRange(new[] { undgBO1, undgBO2 });

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(2, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "REF0001", "E0001", "OldDes", "Marks");
			AssertPackingLineBO(shipment.OuterPackLines[1], false, 1, "BOX", "REF0001", "E0001", "NewDes", "NewMarks");
		}

		public void TestReadIntoCollection_Partial_WithOneDangerousGoods()
		{
			var subs1 = Factory.New<UNDGSubstance>();
			subs1.DG_UNNO = "1233";
			subs1.DG_Variant = "a";
			subs1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs1.DG_Class = "1.1A";

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "1233";
			subs2.DG_Variant = "b";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs2.DG_Class = "2.1";

			var shipment = Factory.New<ForwardingShipment>();

			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Partial;

			var packingLineDO = CreatePackingLineDO(1, "BOX", "REF0001", "E0001", "NewDes", "NewMarks");
			packingLineDOs.AddRange(new[] { packingLineDO });

			Action<ZString, ZString> addUNDGDO = (substance, dgClass) =>
			{
				var undg = new UNDG(DefaultDataObjectWriterStrategy.TestInstance);

				undg.UNDGCode = substance;
				undg.IMOClass = dgClass;

				packingLineDO.SetUNDGCollection(() => new List<UNDG>());
				packingLineDO.UNDGCollection.Add(undg);
			};

			Action<ZString, ZString> addUNDGBO = (substance, dgClass) =>
			{
				var undg = Factory.New<UNDGDataItem>();

				var subs = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, substance.SubstringSafe(0, 4), substance.SubstringSafe(4, 2), "IMO").FirstOrDefault();

				if (subs != null)
				{
					undg.DI_DG = subs.PK;
					undg.LinkDefault(subs);
				}
				undg.DI_IMOClass = dgClass;

				shipment.OuterPackLines.RemoveAndDeleteAll();
				var packingLine = CreateForwardingPackingLine(shipment, 1, "BOX", "REF0001", "E0001", "OldDes", "Marks");

				packingLine.UNDGs.Add(undg);
			};

			Action<ZBool> assertMatch = (isMatched) =>
			{
				var reader = GetReader(packingLineDOs, shipment);
				reader.ReadIntoCollection();

				if (isMatched)
				{
					AssertEquals(1, shipment.OuterPackLines.Count);
					AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "REF0001", "E0001", "NewDes", "NewMarks");
				}
				else
				{
					AssertEquals(2, shipment.OuterPackLines.Count);
					AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "REF0001", "E0001", "OldDes", "Marks");
					AssertPackingLineBO(shipment.OuterPackLines[1], false, 1, "BOX", "REF0001", "E0001", "NewDes", "NewMarks");
				}
			};

			//case1: same DG Substance, DG Class are both empty: MATCH
			addUNDGDO(subs1.DG_Code, "");
			addUNDGBO(subs1.DG_Code, "");

			assertMatch(true);

			//case2: different DG Substance, DG Class are both empty: NOT A MATCH
			addUNDGDO(subs1.DG_Code, "");
			addUNDGBO(subs2.DG_Code, "");

			assertMatch(false);

			//case3: same DG Substance, DG Class from DO is not empty: MATCH
			addUNDGDO(subs1.DG_Code, "1.1D");
			addUNDGBO(subs1.DG_Code, "");

			assertMatch(true);

			//case4: same DG Substance, DG Class from BO is not empty: MATCH
			addUNDGDO(subs1.DG_Code, "");
			addUNDGBO(subs1.DG_Code, "1.1D");

			assertMatch(true);

			//case5: same DG Class, DG Substance are both empty: MATCH
			addUNDGDO("", "1.1D");
			addUNDGBO("", "1.1D");

			assertMatch(true);

			//case6: different DG Class, DG Substance are both empty: NOT A MATCH

			addUNDGDO("", "1.1A");
			addUNDGBO("", "1.1D");

			assertMatch(false);

			//case7: same DG Class, DG Substance from DO is empty: NOT A MATCH
			addUNDGDO(subs1.DG_Code, "1.1D");
			addUNDGBO("", "1.1D");

			assertMatch(false);

			//case8: same DG Class, DG Substance from BO is empty: NOT A MATCH
			addUNDGDO("", "1.1D");
			addUNDGBO(subs1.DG_Code, "1.1D");

			assertMatch(false);

			//case9: same DG Class, same DG Substance: MATCH
			addUNDGDO(subs1.DG_Code, "1.1D");
			addUNDGBO(subs1.DG_Code, "1.1D");

			assertMatch(true);
		}

		public void TestReadIntoCollection_MatchPackingLineID()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packingLineDOs = new DataObjectList<PackingLine>();
			packingLineDOs.Content = CollectionContent.Complete;

			Action initializePackingLineBOs = () =>
			{
				shipment.OuterPackLines.RemoveAndDeleteAll();
				var packingBO1 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "B1", "C1", "D1");
				packingBO1.JL_PackLineId = "1234";
				var packingBO2 = CreateForwardingPackingLine(shipment, 1, "BOX", "A1", "", "C2", "D2");
				packingBO2.JL_PackLineId = "2345";
				var packingBO3 = CreateForwardingPackingLine(shipment, 1, "BOX", "", "B1", "C3", "D3");
				packingBO3.JL_PackLineId = "3456";
				var packingBO4 = CreateForwardingPackingLine(shipment, 1, "BOX", "", "", "C4", "D4");
				packingBO4.JL_PackLineId = "4567";
				var packingBO5 = CreateForwardingPackingLine(shipment, 1, "PLT", "", "B3", "C3", "D3");
				packingBO5.JL_PackLineId = "0000";
			};

			Action<ZString?, ZString?, ZString?, ZString?, ZString?> initializePackingLineDOs = (ZString? refNumber, ZString? exportRefNumber, ZString? descrition, ZString? marksAndNos, ZString? packingLineID) =>
			{
				packingLineDOs = new DataObjectList<PackingLine>();
				var packingDO1 = CreatePackingLineDO(1, "BOX", refNumber, exportRefNumber, descrition, marksAndNos);
				packingDO1.PackingLineID = packingLineID;
				packingLineDOs.AddRange(new[] { packingDO1 });
			};

			//case1: matches correct packline id
			initializePackingLineBOs();
			initializePackingLineDOs("", null, "C3", "D3", "2345");

			var reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "", "", "C3", "D3");
			AssertEquals(shipment.OuterPackLines[0].JL_PackLineId, "2345");

			//case2: no matching packline ids
			initializePackingLineBOs();
			initializePackingLineDOs("", null, "C3", "D3", "5678");

			reader = GetReader(packingLineDOs, shipment);
			reader.ReadIntoCollection();

			AssertEquals(1, shipment.OuterPackLines.Count);
			AssertPackingLineBO(shipment.OuterPackLines[0], true, 1, "BOX", "", "B1", "C3", "D3");
			AssertNotEquals(shipment.OuterPackLines[0].JL_PackLineId, "5678");
		}

		PackingLine CreatePackingLineDO(ZLong? packingQuantity, ZString? packingType, ZString? refNumber, ZString? exportRefNumber, ZString? descrition, ZString? marksAndNos)
		{
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.PackQty = packingQuantity;
			packingLine.PackType = new PackageType
			{
				Code = packingType
			};

			packingLine.ExportReferenceNumber = exportRefNumber;
			packingLine.ReferenceNumber = refNumber;
			packingLine.GoodsDescription = descrition;
			packingLine.MarksAndNos = marksAndNos;

			return packingLine;
		}

		ForwardingPackLine CreateForwardingPackingLine(ForwardingShipment shipment, ZInt packingQuantity, ZString packingType, ZString refNumber, ZString exportRefNumber, ZString descrition, ZString marksAndNos)
		{
			var packingLineBO = shipment.OuterPackLines.AddNew();

			packingLineBO.JL_PackageCount = packingQuantity;
			packingLineBO.JL_F3_NKPackType = packingType;
			packingLineBO.JL_RefNumber = refNumber;
			packingLineBO.JL_ExportRefNumber = exportRefNumber;
			packingLineBO.JL_Description = descrition;
			packingLineBO.JL_MarksAndNumbers = marksAndNos;
			packingLineBO.JL_Height = 110m;

			return packingLineBO;
		}

		void AssertPackingLineBO(ForwardingPackLine packingLine, ZBool isOriginallyFromBO, ZInt packingQuantity, ZString packingType, ZString refNumber, ZString exportRefNumber, ZString descrition, ZString marksAndNos)
		{
			AssertEquals(isOriginallyFromBO, packingLine.JL_Height == 110m);
			AssertEquals(packingQuantity, packingLine.JL_PackageCount);
			AssertEquals(packingType, packingLine.JL_F3_NKPackType);
			AssertEquals(refNumber, packingLine.JL_RefNumber);
			AssertEquals(exportRefNumber, packingLine.JL_ExportRefNumber);
			AssertEquals(descrition, packingLine.JL_Description);
			AssertEquals(marksAndNos, packingLine.JL_MarksAndNumbers);
		}

		ForwardingPackingLineCollectionReader GetReader(DataObjectList<PackingLine> packingLines, ForwardingShipment parentShipment, UniversalObjectFactory factory = null)
		{
			if (factory == null)
			{
				factory = Factory;
			}

			var readingContext = new ForwardingPackingLineCollectionReadingContext
			{
				PackingLineDataObjectCollection = packingLines,
				Logger = logger,
				Factory = factory,
				ShipmentBO = parentShipment,
				ContainerLinkManager = linkManager.Object,
				OrderLineLinkManager = orderLinkManager.Object
			};

			return new ForwardingPackingLineCollectionReader(readingContext);
		}

		TestErrorLogger logger;
		Mock<IContainerLinkManager<ForwardingConsol>> linkManager;
		Mock<IOrderLineLinkManager> orderLinkManager;

		protected override void SetUp()
		{
			logger = new TestErrorLogger();
			linkManager = new Mock<IContainerLinkManager<ForwardingConsol>>();
			orderLinkManager = new Mock<IOrderLineLinkManager>();

			base.SetUp();
		}
	}
}
