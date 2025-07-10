using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PutawayLocationFactLoaderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PutawayLocationFactLoader(null));
		}

		public void TestIOCRegistration()
		{
			var loader = ObjectFactory.Get<IPutawayLocationFactLoader>();
			AssertType<PutawayLocationFactLoader>(loader);
			AssertEquals("Should be of the same instance.", true,
				ReferenceEquals(loader, ObjectFactory.Get<IPutawayLocationFactLoader>()));
		}

		#region TestGetPutawayLocationFacts

		public void TestGetPutawayLocationFacts_IsDynamicPickFace() => TestGetPutawayLocationFacts("DYN", false, false);
		public void TestGetPutawayLocationFacts_IsFixedPickFace() => TestGetPutawayLocationFacts("FIX", false, false);

		public void TestGetPutawayLocationFacts_IsFixedPickFaceFull() =>
			TestGetPutawayLocationFacts("PFC", true, false);

		public void TestGetPutawayLocationFacts_IsTsaKnownLocation() => TestGetPutawayLocationFacts("LOC", false, true);
		public void TestGetPutawayLocationFacts_NoBooleansTrue() => TestGetPutawayLocationFacts("LOC", false, false);
		public void TestGetPutawayLocationFacts_AllBooleansTrue() => TestGetPutawayLocationFacts("LOC", true, true);

		void TestGetPutawayLocationFacts(string locationCacheTypeCode, bool isFixedPickFaceFull,
			bool isTsaKnownLocation)
		{
			// Put unique values for all values that support enough values, test combos for booleans which can only have two values
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			var locationPk1 = Guid.NewGuid();
			var locationPk2 = Guid.NewGuid();
			var warehousePk = Guid.NewGuid();

			var area1Pk = Guid.NewGuid();
			var area1Name = "PUT";
			var area1TypeCode = "HEL";

			var area2Pk = Guid.NewGuid();
			var area2Name = "PT2";
			var area2TypeCode = "AVL";

			var locationTypeCode = "TYP";
			var locationClass = "NOR";
			var rowName = "A";

			var column = (short)1;
			var level = (short)2;
			var tray = (short)3;
			var putawaySequence = 4;

			var locationStatus = "AVL";

			var availableWeight = 42.42m;
			var maxWeightUnit = "KG";
			var maxWeight = 123.456m;

			var availableVolume = 3.14m;
			var maxVolumeUnit = "M3";
			var maxVolume = 456.78m;

			var availableUnits = 1.23m;
			var currentAndPendingStock = 9.81m;
			var maxQuantity = 12.52m;

			var partialPalletID = "PLT-123";

			var productPk = Guid.NewGuid();
			var clientPk = ZGuid.NewZGuid();
			var lastAllocatedOrChangedID1 = Guid.NewGuid();
			var lastAllocatedOrChangedID2 = Guid.NewGuid();

			var productData = JsonConvert.SerializeObject(new HashSet<Guid>(new[] { productPk }));
			var pa1Data = CreateAttributeData("PA1", 1, productPk);
			var pa2Data = CreateAttributeData("PA2", 1, productPk);
			var pa3Data = CreateAttributeData("PA3", 1, productPk);

			var date1 = ZDate.BrettsBirthday.ToDateTime();
			var date2 = ZDate.BrettsBirthday.AddYears(10).ToDateTime();
			var expiryDate = CreateAttributeData(date1, 1, productPk);
			var packingDate = CreateAttributeData(date2, 1, productPk);

			var palletSpaces = (short)20;
			var palletQuantity = (short)18;

			var sql = @"
SELECT
	WPC_PK = @PK1,
	WPC_LocationTypeCode = @LocationTypeCode,
	WPC_LocationClass = @LocationClass,
	WPC_WW_Warehouse = @WarehousePK,
	WPC_WA_Area = @Area1PK,
	WPC_AreaName = @Area1Name,
	WPC_AreaType = @Area1Type,
	WPC_RowName = @RowName,
	WPC_Column = @Column,
	WPC_Level = @Level,
	WPC_Tray = @Tray,
	WPC_PutawaySequence = @PutawaySequence,
	WPC_LocationStatus = @LocationStatus,
	WPC_IsTSAApprovedKnown = @IsTSAApprovedKnown,
	WPC_IsFixedPickFaceFull = @IsFixedPickFaceFull,
	WPC_OP_Product = @ProductPK,
	WPC_OH_Client = @ClientPK,
	WPC_PartialPalletID = @PartialPalletID,
	WPC_MaxQuantity = @MaxQuantity,
	WPC_AvailableQuantity = @AvailableQuantity,
	WPC_StockOnHand = @Quantity,
	WPC_StockOnHandData = @QuantityData,
	WPC_MaxWeight = @MaxWeight,
	WPC_AvailableWeight = @AvailableWeight,
	WPC_WeightUnit = @WeightUnit,
	WPC_MaxVolume = @MaxVolume,
	WPC_AvailableVolume = @AvailableVolume,
	WPC_VolumeUnit = @VolumeUnit,
	WPC_ProductData = @ProductData,
	WPC_PartAttribute1Data = @PartAttribute1Data,
	WPC_PartAttribute2Data = @PartAttribute2Data,
	WPC_ExpiryDateData = @ExpiryDateData,
	WPC_PackingDateData = @PackingDateData,
	WPC_PartAttribute3Data = @PartAttribute3Data,
	WPC_LocationCacheType = @LocationCacheType,
	WPC_WL_Location = @LocationPK1,
	WPC_WF_PickFace = @PK1,
	WPC_PalletSpaces = @PalletSpaces,
	WPC_PalletQuantity = @PalletQuantity,
	WPC_LastAllocatedOrChangedID = @LastAllocatedOrChangedID1

UNION ALL

SELECT
	WPC_PK = @PK2,
	WPC_LocationTypeCode = @LocationTypeCode,
	WPC_LocationClass = @LocationClass,
	WPC_WW_Warehouse = @WarehousePK,
	WPC_WA_Area = @Area2PK,
	WPC_AreaName = @Area2Name,
	WPC_AreaType = @Area2Type,
	WPC_RowName = @RowName,
	WPC_Column = @Column,
	WPC_Level = @Level,
	WPC_Tray = @Tray,
	WPC_PutawaySequence = @PutawaySequence,
	WPC_LocationStatus = @LocationStatus,
	WPC_IsTSAApprovedKnown = @IsTSAApprovedKnown,
	WPC_IsFixedPickFaceFull = @IsFixedPickFaceFull,
	WPC_OP_Product = @ProductPK,
	WPC_OH_Client = @ClientPK,
	WPC_PartialPalletID = @PartialPalletID,
	WPC_MaxQuantity = 0,
	WPC_AvailableQuantity = 0,
	WPC_StockOnHand = @Quantity,
	WPC_StockOnHandData = @QuantityData,
	WPC_MaxWeight = @MaxWeight,
	WPC_AvailableWeight = @AvailableWeight,
	WPC_WeightUnit = @WeightUnit,
	WPC_MaxVolume = @MaxVolume,
	WPC_AvailableVolume = @AvailableVolume,
	WPC_VolumeUnit = @VolumeUnit,
	WPC_ProductData = @ProductData,
	WPC_PartAttribute1Data = @PartAttribute1Data,
	WPC_PartAttribute2Data = @PartAttribute2Data,
	WPC_ExpiryDateData = @ExpiryDateData,
	WPC_PackingDateData = @PackingDateData,
	WPC_PartAttribute3Data = @PartAttribute3Data,
	WPC_LocationCacheType = @LocationCacheType,
	WPC_WL_Location = @LocationPK2,
	WPC_WF_PickFace = @PK2,
	WPC_PalletSpaces = @PalletSpaces,
	WPC_PalletQuantity = @PalletQuantity,
	WPC_LastAllocatedOrChangedID = @LastAllocatedOrChangedID2";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@PK1", pk1, WhsDocketSchema.PK);
			parameters.Add("@PK2", pk2, WhsDocketSchema.PK);
			parameters.Add("@LocationPK1", locationPk1, WhsDocketSchema.PK);
			parameters.Add("@LocationPK2", locationPk2, WhsDocketSchema.PK);
			parameters.Add("@WarehousePK", warehousePk, WhsDocketSchema.PK);
			parameters.Add("@ProductPK", productPk, WhsDocketSchema.PK);
			parameters.Add("@ClientPK", clientPk, WhsDocketSchema.PK);

			parameters.Add("@Area1PK", area1Pk, WhsDocketSchema.PK);
			parameters.Add("@Area1Name", area1Name, WhsDocketSchema.WD_DocketType);
			parameters.Add("@Area1Type", area1TypeCode, WhsDocketSchema.WD_DocketType);

			parameters.Add("@Area2PK", area2Pk, WhsDocketSchema.PK);
			parameters.Add("@Area2Name", area2Name, WhsDocketSchema.WD_DocketType);
			parameters.Add("@Area2Type", area2TypeCode, WhsDocketSchema.WD_DocketType);

			parameters.Add("@LocationTypeCode", locationTypeCode, WhsDocketSchema.WD_DocketType);
			parameters.Add("@LocationClass", locationClass, WhsDocketSchema.WD_DocketType);
			parameters.Add("@RowName", rowName, WhsDocketSchema.WD_DocketType);
			parameters.Add("@LocationStatus", locationStatus, WhsDocketSchema.WD_DocketType);
			parameters.Add("@PartialPalletID", partialPalletID, WhsDocketSchema.WD_DocketType);
			parameters.Add("@WeightUnit", maxWeightUnit, WhsDocketSchema.WD_DocketType);
			parameters.Add("@VolumeUnit", maxVolumeUnit, WhsDocketSchema.WD_DocketType);
			parameters.Add("@LocationCacheType", locationCacheTypeCode, WhsDocketSchema.WD_DocketType);

			parameters.Add("@QuantityData", "", WhsDocketSchema.WD_DocketType);
			parameters.Add("@ProductData", productData, WhsDocketSchema.WD_DocketType);
			parameters.Add("@PartAttribute1Data", pa1Data, WhsDocketSchema.WD_DocketType);
			parameters.Add("@PartAttribute2Data", pa2Data, WhsDocketSchema.WD_DocketType);
			parameters.Add("@PartAttribute3Data", pa3Data, WhsDocketSchema.WD_DocketType);
			parameters.Add("@ExpiryDateData", expiryDate, WhsDocketSchema.WD_DocketType);
			parameters.Add("@PackingDateData", packingDate, WhsDocketSchema.WD_DocketType);

			parameters.Add("@Column", column, WhsLocationSchema.WL_Column);
			parameters.Add("@Level", level, WhsLocationSchema.WL_Column);
			parameters.Add("@Tray", tray, WhsLocationSchema.WL_Column);
			parameters.Add("@PalletSpaces", palletSpaces, WhsLocationSchema.WL_Column);
			parameters.Add("@PalletQuantity", palletQuantity, WhsLocationSchema.WL_Column);
			parameters.Add("@PutawaySequence", putawaySequence, WhsLocationSchema.WL_PutawayPathSequence);

			parameters.Add("@IsTSAApprovedKnown", isTsaKnownLocation, WhsDocketSchema.WD_IsPutawayTransfer);
			parameters.Add("@IsFixedPickFaceFull", isFixedPickFaceFull, WhsDocketSchema.WD_IsPutawayTransfer);

			parameters.Add("@MaxQuantity", maxQuantity, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@AvailableQuantity", availableUnits, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@Quantity", currentAndPendingStock, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@MaxWeight", maxWeight, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@AvailableWeight", availableWeight, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@MaxVolume", maxVolume, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@AvailableVolume", availableVolume, WhsDocketLineSchema.WE_StockOnHand);

			parameters.Add("@LastAllocatedOrChangedID1", lastAllocatedOrChangedID1, WhsLocationSchema.WL_LastAllocatedOrChangedID);
			parameters.Add("@LastAllocatedOrChangedID2", lastAllocatedOrChangedID2, WhsLocationSchema.WL_LastAllocatedOrChangedID);

			IEnumerable<DataRow> rows = null;
			using (var command =
				   new ZSqlConnectionInfo(Db.Connection, Db.DatabaseName).GetNewDbCommandForSelect(sql,
					   parameters.OfType<ZSqlParameter>().ToArray()))
			using (var adapter = command.NewDataAdapter())
			{
				var table = new DataTable();
				adapter.Fill(table);
				rows = table.Rows.Cast<DataRow>();
			}

			var cacheManagerMock = new Mock<IWhsPutawayLocationCacheManager>();
			cacheManagerMock.Setup(cm => cm.GetCache(Factory, warehousePk, new[] { clientPk }, new[] { (ZGuid)productPk }, It.IsAny<IEnumerable<ZGuid>>()))
				.Returns(rows);

			var putawayLocationFactLoader = new PutawayLocationFactLoader(cacheManagerMock.Object);
			var putawayFacts =
				putawayLocationFactLoader.GetPutawayLocationFacts(Factory, warehousePk, new[] { clientPk },
					new[] { (ZGuid)productPk });
			AssertEquals(2, putawayFacts.Count());

			var putawayLocationFacts = putawayFacts.OfType<IPutawayLocationFact>().ToArray();
			AssertEquals(2, putawayLocationFacts.Length);
			AssertEquals(nameof(PutawayLocationFact.PK), pk1, putawayLocationFacts[0].PK);
			AssertEquals(nameof(PutawayLocationFact.PK), pk2, putawayLocationFacts[1].PK);

			AssertEquals(nameof(PutawayLocationFact.LocationPK), locationPk1, putawayLocationFacts[0].LocationPK);
			AssertEquals(nameof(PutawayLocationFact.LocationPK), locationPk2, putawayLocationFacts[1].LocationPK);

			AssertEquals(nameof(PutawayLocationFact.AvailableUnits), availableUnits,
				putawayLocationFacts[0].AvailableUnits);
			AssertEquals(nameof(PutawayLocationFact.AvailableUnits), 0m, putawayLocationFacts[1].AvailableUnits);

			AssertEquals(nameof(PutawayLocationFact.AreaName), "PUT", putawayLocationFacts[0].AreaName);
			AssertEquals(nameof(PutawayLocationFact.AreaName), "PT2", putawayLocationFacts[1].AreaName);

			AssertEquals(nameof(PutawayLocationFact.AreaTypeCode), "HEL", putawayLocationFacts[0].AreaTypeCode);
			AssertEquals(nameof(PutawayLocationFact.AreaTypeCode), "AVL", putawayLocationFacts[1].AreaTypeCode);

			AssertEquals(nameof(PutawayLocationFact.LastAllocatedOrChangedID), lastAllocatedOrChangedID1, putawayLocationFacts[0].LastAllocatedOrChangedID);
			AssertEquals(nameof(PutawayLocationFact.LastAllocatedOrChangedID), lastAllocatedOrChangedID2, putawayLocationFacts[1].LastAllocatedOrChangedID);

			foreach (var putawayLocationFact in putawayLocationFacts)
			{
				AssertEquals(nameof(PutawayLocationFact.WarehousePK), warehousePk, putawayLocationFact.WarehousePK);
				AssertEquals(nameof(PutawayLocationFact.LocationTypeCode), locationTypeCode,
					putawayLocationFact.LocationTypeCode);
				AssertEquals(nameof(PutawayLocationFact.LocationClass), locationClass,
					putawayLocationFact.LocationClass);
				AssertEquals(nameof(PutawayLocationFact.RowName), rowName, putawayLocationFact.RowName);
				AssertEquals(nameof(PutawayLocationFact.Column), column, putawayLocationFact.Column);
				AssertEquals(nameof(PutawayLocationFact.Level), level, putawayLocationFact.Level);
				AssertEquals(nameof(PutawayLocationFact.Tray), tray, putawayLocationFact.Tray);
				AssertEquals(nameof(PutawayLocationFact.PutawaySequence), putawaySequence,
					putawayLocationFact.PutawaySequence);
				AssertEquals(nameof(PutawayLocationFact.LocationStatus), locationStatus,
					putawayLocationFact.LocationStatus);
				AssertEquals(nameof(PutawayLocationFact.AvailableWeight), availableWeight,
					putawayLocationFact.AvailableWeight);
				AssertEquals(nameof(PutawayLocationFact.MaxWeightUnit), maxWeightUnit,
					putawayLocationFact.MaxWeightUnit);
				AssertEquals(nameof(PutawayLocationFact.AvailableVolume), availableVolume,
					putawayLocationFact.AvailableVolume);
				AssertEquals(nameof(PutawayLocationFact.MaxVolumeUnit), maxVolumeUnit,
					putawayLocationFact.MaxVolumeUnit);
				AssertEquals(nameof(PutawayLocationFact.CurrentAndIncomingStock), currentAndPendingStock,
					putawayLocationFact.CurrentAndIncomingStock);
				AssertEquals(nameof(PutawayLocationFact.IsPartialPallet), locationCacheTypeCode == "PLT",
					putawayLocationFact.IsPartialPallet);
				AssertEquals(nameof(PutawayLocationFact.PartialPalletID), partialPalletID,
					putawayLocationFact.PartialPalletID);
				AssertEquals(nameof(PutawayLocationFact.IsDynamicPickFace), locationCacheTypeCode == "DYN",
					putawayLocationFact.IsDynamicPickFace);
				AssertEquals(nameof(PutawayLocationFact.IsFixedPickFace), locationCacheTypeCode == "FIX",
					putawayLocationFact.IsFixedPickFace);
				AssertEquals(nameof(PutawayLocationFact.ProductPK), productPk, putawayLocationFact.ProductPK);
				AssertEquals(nameof(PutawayLocationFact.ClientPK), clientPk, putawayLocationFact.ClientPK);
				AssertEquals(nameof(PutawayLocationFact.IsTSAKnownLocation), isTsaKnownLocation,
					putawayLocationFact.IsTSAKnownLocation);

				var product1Mock = new Mock<IPutawayProductFact>();
				product1Mock.Setup(p => p.PK).Returns(productPk);

				var invMock1 = new Mock<IInventoryFact>();
				invMock1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(product1Mock.Object));
				invMock1.Setup(inv => inv.Quantity).Returns(3m);
				invMock1.Setup(inv => inv.PartAttribute1).Returns("PA1");
				invMock1.Setup(inv => inv.PartAttribute2).Returns("PA2");
				invMock1.Setup(inv => inv.PartAttribute3).Returns("PA3");
				invMock1.Setup(inv => inv.ExpiryDate).Returns(date1);
				invMock1.Setup(inv => inv.PackingDate).Returns(date2);

				var invMock2 = new Mock<IInventoryFact>();
				invMock2.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(product1Mock.Object));
				invMock2.Setup(inv => inv.Quantity).Returns(2m);
				invMock2.Setup(inv => inv.ProductWeight).Returns(30m);
				invMock2.Setup(inv => inv.ProductWeightUQ).Returns("KG");
				invMock2.Setup(inv => inv.PartAttribute1).Returns("PB1");
				invMock2.Setup(inv => inv.PartAttribute2).Returns("PB2");
				invMock2.Setup(inv => inv.PartAttribute3).Returns("PB3");
				invMock2.Setup(inv => inv.ExpiryDate).Returns(date2);
				invMock2.Setup(inv => inv.PackingDate).Returns(date1);

				var product2Mock = new Mock<IPutawayProductFact>();
				product2Mock.Setup(p => p.PK).Returns(Guid.NewGuid());

				var invMockOtherProduct = new Mock<IInventoryFact>();
				invMockOtherProduct.Setup(inv => inv.Product)
					.Returns(new FactJoin<IPutawayProductFact>(product2Mock.Object));
				invMockOtherProduct.Setup(inv => inv.Quantity).Returns(10m);
				invMockOtherProduct.Setup(inv => inv.ProductVolume).Returns(3m);
				invMockOtherProduct.Setup(inv => inv.ProductVolumeUQ).Returns("M3");

				AssertEquals(nameof(PutawayLocationFact.ContainsThisProduct), false,
					putawayLocationFact.ContainsThisProduct(invMockOtherProduct.Object));
				AssertEquals(nameof(PutawayLocationFact.ContainsThisProduct), true,
					putawayLocationFact.ContainsThisProduct(invMock1.Object));

				AssertEquals(nameof(PutawayLocationFact.ContainsOtherPartAttribute1s), true,
					putawayLocationFact.ContainsOtherPartAttribute1s(invMock2.Object));
				AssertEquals(nameof(PutawayLocationFact.ContainsOtherPartAttribute1s), false,
					putawayLocationFact.ContainsOtherPartAttribute1s(invMock1.Object));

				AssertEquals(nameof(PutawayLocationFact.ContainsOtherPartAttribute2s), true,
					putawayLocationFact.ContainsOtherPartAttribute2s(invMock2.Object));
				AssertEquals(nameof(PutawayLocationFact.ContainsOtherPartAttribute2s), false,
					putawayLocationFact.ContainsOtherPartAttribute2s(invMock1.Object));

				AssertEquals(nameof(PutawayLocationFact.ContainsOtherPartAttribute3s), true,
					putawayLocationFact.ContainsOtherPartAttribute3s(invMock2.Object));
				AssertEquals(nameof(PutawayLocationFact.ContainsOtherPartAttribute3s), false,
					putawayLocationFact.ContainsOtherPartAttribute3s(invMock1.Object));

				AssertEquals(nameof(PutawayLocationFact.ContainsOtherExpiryDates), true,
					putawayLocationFact.ContainsOtherExpiryDates(invMock2.Object));
				AssertEquals(nameof(PutawayLocationFact.ContainsOtherExpiryDates), false,
					putawayLocationFact.ContainsOtherExpiryDates(invMock1.Object));

				AssertEquals(nameof(PutawayLocationFact.ContainsOtherPackingDates), true,
					putawayLocationFact.ContainsOtherPackingDates(invMock2.Object));
				AssertEquals(nameof(PutawayLocationFact.ContainsOtherPackingDates), false,
					putawayLocationFact.ContainsOtherPackingDates(invMock1.Object));

				AssertEquals(nameof(PutawayLocationFact.StockQuantityThatCanBePutaway),
					putawayLocationFact.PK == pk1 ? 1m : 3m,
					putawayLocationFact.StockQuantityThatCanBePutaway(invMock1.Object)); // Quantity limited
				AssertEquals(nameof(PutawayLocationFact.StockQuantityThatCanBePutaway), 1m,
					putawayLocationFact.StockQuantityThatCanBePutaway(invMock2.Object)); // Weight limited
				AssertEquals(nameof(PutawayLocationFact.StockQuantityThatCanBePutaway), 1m,
					putawayLocationFact.StockQuantityThatCanBePutaway(invMockOtherProduct.Object)); // Volume limited
			}
		}

		string CreateAttributeData<T>(T attribute, int numberOfAttributes, Guid productPK)
		{
			var productAttributeData =
				new WhsPutawayLocationCacheAttributeData
				{
					Attribute = attribute,
					NumberOfAttributes = numberOfAttributes
				};
			var attributeData = new Dictionary<Guid, WhsPutawayLocationCacheAttributeData>();
			attributeData.Add(productPK, productAttributeData);
			return JsonConvert.SerializeObject(attributeData);
		}

		public void TestGetPutawayLocationFacts_PassInSkipLocationPKs()
		{
			IEnumerable<ZGuid> skipLocationPKsPassedIn = null;
			var cacheManagerMock = new Mock<IWhsPutawayLocationCacheManager>();
			cacheManagerMock.Setup(cm => cm.GetCache(Factory, It.IsAny<ZGuid>(), It.IsAny<IEnumerable<ZGuid>>(), It.IsAny<IEnumerable<ZGuid>>(), It.IsAny<IEnumerable<ZGuid>>()))
				.Callback((Action<BusinessObjectFactory, ZGuid, IEnumerable<ZGuid>, IEnumerable<ZGuid>, IEnumerable<ZGuid>>)((factory, whsPK, clientPKs, productPKs, locationPKs) => skipLocationPKsPassedIn = locationPKs));

			var skipLocationPKs = new List<ZGuid>() { ZGuid.NewZGuid(), ZGuid.NewZGuid() };
			var putawayLocationFactLoader = new PutawayLocationFactLoader(cacheManagerMock.Object);
			var putawayFacts =
				putawayLocationFactLoader.GetPutawayLocationFacts(Factory, ZGuid.Empty, new[] { ZGuid.Empty },
					new[] { ZGuid.Empty }, skipLocationPKs);

			AssertContainsExactElementsInAnyOrder(skipLocationPKs, skipLocationPKsPassedIn);
		}

		#endregion

		#region TestGetPutawayLocationFacts_PartialPallet

		public void TestGetPutawayLocationFacts_PartialPallet() =>
			TestGetPutawayLocationFacts_PartialPallet(hasLocation: true);

		public void TestGetPutawayLocationFacts_PartialPallet_WithoutLocation() =>
			TestGetPutawayLocationFacts_PartialPallet(hasLocation: false);

		void TestGetPutawayLocationFacts_PartialPallet(bool hasLocation)
		{
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();

			var warehousePk = Guid.NewGuid();

			var area1Pk = Guid.NewGuid();
			var area1Name = "PUT";
			var area1TypeCode = "HEL";

			var locationTypeCode = "TYP";
			var locationClass = "NOR";
			var rowName = "A";

			var column = (short)1;
			var level = (short)2;
			var tray = (short)3;
			var putawaySequence = 4;

			var locationStatus = "AVL";

			var availableWeight = 42.42m;
			var maxWeightUnit = "KG";
			var maxWeight = 123.456m;

			var availableVolume = 3.14m;
			var maxVolumeUnit = "M3";
			var maxVolume = 456.78m;

			var availableUnits1 = 10m;
			var availableUnits2 = 20m;
			var currentAndPendingStock = 0m;

			var partialPalletID = "PLT-123";

			var productPk = Guid.NewGuid();
			var clientPk = ZGuid.NewZGuid();

			var lastAllocatedOrChangedID1 = Guid.NewGuid();
			var lastAllocatedOrChangedID2 = Guid.NewGuid();

			var palletSpaces = (short)0;
			var palletQuantity = (short)0;

			var sql = @"
SELECT
	WPC_PK = @PK1,
	WPC_LocationTypeCode = @LocationTypeCode,
	WPC_LocationClass = @LocationClass,
	WPC_WW_Warehouse = @WarehousePK,
	WPC_WA_Area = @Area1PK,
	WPC_AreaName = @Area1Name,
	WPC_AreaType = @Area1Type,
	WPC_RowName = @RowName,
	WPC_Column = @Column,
	WPC_Level = @Level,
	WPC_Tray = @Tray,
	WPC_PutawaySequence = @PutawaySequence,
	WPC_LocationStatus = @LocationStatus,
	WPC_IsTSAApprovedKnown = @IsTSAApprovedKnown,
	WPC_IsFixedPickFaceFull = @IsFixedPickFaceFull,
	WPC_OP_Product = @ProductPK,
	WPC_OH_Client = @ClientPK,
	WPC_PartialPalletID = @PartialPalletID,
	WPC_MaxQuantity = @AvailableQuantity1 + 10,
	WPC_AvailableQuantity = @AvailableQuantity1,
	WPC_StockOnHand = @Quantity,
	WPC_StockOnHandData = @QuantityData,
	WPC_MaxWeight = 0,
	WPC_AvailableWeight = 0,
	WPC_WeightUnit = '',
	WPC_MaxVolume = 0,
	WPC_AvailableVolume = 0,
	WPC_VolumeUnit = '',
	WPC_ProductData = @ProductData,
	WPC_PartAttribute1Data = @PartAttribute1Data,
	WPC_PartAttribute2Data = @PartAttribute2Data,
	WPC_ExpiryDateData = @ExpiryDateData,
	WPC_PackingDateData = @PackingDateData,
	WPC_PartAttribute3Data = @PartAttribute3Data,
	WPC_LocationCacheType = @LocationCacheTypePLT,
	WPC_WL_Location = @PK2,
	WPC_WF_PickFace = null,
	WPC_PalletSpaces = @PalletSpaces,
	WPC_PalletQuantity = @PalletQuantity,
	WPC_LastAllocatedOrChangedID = @LastAllocatedOrChangedID1";

			if (hasLocation)
			{
				sql +=
					@"

UNION ALL

SELECT
	WPC_PK = @PK2,
	WPC_LocationTypeCode = @LocationTypeCode,
	WPC_LocationClass = @LocationClass,
	WPC_WW_Warehouse = @WarehousePK,
	WPC_WA_Area = @Area1PK,
	WPC_AreaName = @Area1Name,
	WPC_AreaType = @Area1Type,
	WPC_RowName = @RowName,
	WPC_Column = @Column,
	WPC_Level = @Level,
	WPC_Tray = @Tray,
	WPC_PutawaySequence = @PutawaySequence,
	WPC_LocationStatus = @LocationStatus,
	WPC_IsTSAApprovedKnown = @IsTSAApprovedKnown,
	WPC_IsFixedPickFaceFull = @IsFixedPickFaceFull,
	WPC_OP_Product = @ProductPK,
	WPC_OH_Client = @ClientPK,
	WPC_PartialPalletID = '',
	WPC_MaxQuantity = @AvailableQuantity2 + 10,
	WPC_AvailableQuantity = @AvailableQuantity2,
	WPC_StockOnHand = @Quantity,
	WPC_StockOnHandData = @QuantityData,
	WPC_MaxWeight = @MaxWeight,
	WPC_AvailableWeight = @AvailableWeight,
	WPC_WeightUnit = @WeightUnit,
	WPC_MaxVolume = @MaxVolume,
	WPC_AvailableVolume = @AvailableVolume,
	WPC_VolumeUnit = @VolumeUnit,
	WPC_ProductData = @ProductData,
	WPC_PartAttribute1Data = @PartAttribute1Data,
	WPC_PartAttribute2Data = @PartAttribute2Data,
	WPC_ExpiryDateData = @ExpiryDateData,
	WPC_PackingDateData = @PackingDateData,
	WPC_PartAttribute3Data = @PartAttribute3Data,
	WPC_LocationCacheType = @LocationCacheTypeLOC,
	WPC_WL_Location = @PK2,
	WPC_WF_PickFace = null,
	WPC_PalletSpaces = @PalletSpaces,
	WPC_PalletQuantity = @PalletQuantity,
	WPC_LastAllocatedOrChangedID = @LastAllocatedOrChangedID2";
			}

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@PK1", pk1, WhsDocketSchema.PK);
			parameters.Add("@PK2", pk2, WhsDocketSchema.PK);
			parameters.Add("@WarehousePK", warehousePk, WhsDocketSchema.PK);
			parameters.Add("@ProductPK", productPk, WhsDocketSchema.PK);
			parameters.Add("@ClientPK", clientPk, WhsDocketSchema.PK);

			parameters.Add("@Area1PK", area1Pk, WhsDocketSchema.PK);
			parameters.Add("@Area1Name", area1Name, WhsDocketSchema.WD_DocketType);
			parameters.Add("@Area1Type", area1TypeCode, WhsDocketSchema.WD_DocketType);

			parameters.Add("@LocationTypeCode", locationTypeCode, WhsDocketSchema.WD_DocketType);
			parameters.Add("@LocationClass", locationClass, WhsDocketSchema.WD_DocketType);
			parameters.Add("@RowName", rowName, WhsDocketSchema.WD_DocketType);
			parameters.Add("@LocationStatus", locationStatus, WhsDocketSchema.WD_DocketType);
			parameters.Add("@PartialPalletID", partialPalletID, WhsDocketSchema.WD_DocketType);
			parameters.Add("@WeightUnit", maxWeightUnit, WhsDocketSchema.WD_DocketType);
			parameters.Add("@VolumeUnit", maxVolumeUnit, WhsDocketSchema.WD_DocketType);
			parameters.Add("@LocationCacheTypePLT", "PLT", WhsDocketSchema.WD_DocketType);
			parameters.Add("@LocationCacheTypeLOC", "LOC", WhsDocketSchema.WD_DocketType);

			parameters.Add("@QuantityData", "", WhsDocketSchema.WD_DocketType);
			parameters.Add("@ProductData", "[]", WhsDocketSchema.WD_DocketType);
			parameters.Add("@PartAttribute1Data", "{}", WhsDocketSchema.WD_DocketType);
			parameters.Add("@PartAttribute2Data", "{}", WhsDocketSchema.WD_DocketType);
			parameters.Add("@PartAttribute3Data", "{}", WhsDocketSchema.WD_DocketType);
			parameters.Add("@ExpiryDateData", "{}", WhsDocketSchema.WD_DocketType);
			parameters.Add("@PackingDateData", "{}", WhsDocketSchema.WD_DocketType);

			parameters.Add("@Column", column, WhsLocationSchema.WL_Column);
			parameters.Add("@Level", level, WhsLocationSchema.WL_Column);
			parameters.Add("@Tray", tray, WhsLocationSchema.WL_Column);
			parameters.Add("@PalletSpaces", palletSpaces, WhsLocationSchema.WL_Column);
			parameters.Add("@PalletQuantity", palletQuantity, WhsLocationSchema.WL_Column);
			parameters.Add("@PutawaySequence", putawaySequence, WhsLocationSchema.WL_PutawayPathSequence);

			parameters.Add("@IsTSAApprovedKnown", false, WhsDocketSchema.WD_IsPutawayTransfer);
			parameters.Add("@IsFixedPickFaceFull", false, WhsDocketSchema.WD_IsPutawayTransfer);

			parameters.Add("@AvailableQuantity1", availableUnits1, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@AvailableQuantity2", availableUnits2, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@Quantity", currentAndPendingStock, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@MaxWeight", maxWeight, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@AvailableWeight", availableWeight, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@MaxVolume", maxVolume, WhsDocketLineSchema.WE_StockOnHand);
			parameters.Add("@AvailableVolume", availableVolume, WhsDocketLineSchema.WE_StockOnHand);

			parameters.Add("@LastAllocatedOrChangedID1", lastAllocatedOrChangedID1, WhsLocationSchema.WL_LastAllocatedOrChangedID);
			parameters.Add("@LastAllocatedOrChangedID2", lastAllocatedOrChangedID2, WhsLocationSchema.WL_LastAllocatedOrChangedID);

			IEnumerable<DataRow> rows = null;
			using (var command =
				   new ZSqlConnectionInfo(Db.Connection, Db.DatabaseName).GetNewDbCommandForSelect(sql,
					   parameters.OfType<ZSqlParameter>().ToArray()))
			using (var adapter = command.NewDataAdapter())
			{
				var table = new DataTable();
				adapter.Fill(table);
				rows = table.Rows.Cast<DataRow>();
			}

			var cacheManagerMock = new Mock<IWhsPutawayLocationCacheManager>();
			cacheManagerMock.Setup(cm => cm.GetCache(Factory, warehousePk, new[] { clientPk }, new[] { (ZGuid)productPk }, It.IsAny<IEnumerable<ZGuid>>()))
				.Returns(rows);

			var putawayLocationFactLoader = new PutawayLocationFactLoader(cacheManagerMock.Object);
			var putawayFacts =
				putawayLocationFactLoader.GetPutawayLocationFacts(Factory, warehousePk, new[] { clientPk },
					new[] { (ZGuid)productPk });
			if (!hasLocation)
			{
				AssertEquals("Should not have returned pallet if it had no linked location.", 0, putawayFacts.Count());
			}
			else
			{
				AssertEquals(2, putawayFacts.Count());

				var putawayLocationFacts = putawayFacts.OfType<IPutawayLocationFact>().ToArray();
				AssertEquals(2, putawayLocationFacts.Length);
				var locationFact = putawayLocationFacts[0];
				var partialPalletFact = putawayLocationFacts[1];

				AssertEquals(nameof(PutawayLocationFact.PK), pk2, locationFact.PK);
				AssertEquals(nameof(PutawayLocationFact.PK), pk1, partialPalletFact.PK);

				AssertEquals(nameof(PutawayLocationFact.LocationPK), pk2, locationFact.LocationPK);
				AssertEquals(nameof(PutawayLocationFact.LocationPK), pk2, partialPalletFact.LocationPK);

				AssertEquals(nameof(PutawayLocationFact.IsPartialPallet), false, locationFact.IsPartialPallet);
				AssertEquals(nameof(PutawayLocationFact.IsPartialPallet), true, partialPalletFact.IsPartialPallet);

				AssertEquals(nameof(PutawayLocationFact.PartialPalletID), string.Empty, locationFact.PartialPalletID);
				AssertEquals(nameof(PutawayLocationFact.PartialPalletID), partialPalletID,
					partialPalletFact.PartialPalletID);

				AssertEquals(nameof(PutawayLocationFact.AvailableUnits), availableUnits2, locationFact.AvailableUnits);
				AssertEquals(nameof(PutawayLocationFact.AvailableUnits), availableUnits1,
					partialPalletFact.AvailableUnits);

				AssertEquals(nameof(PutawayLocationFact.LastAllocatedOrChangedID), lastAllocatedOrChangedID2, locationFact.LastAllocatedOrChangedID);
				AssertEquals(nameof(PutawayLocationFact.LastAllocatedOrChangedID), lastAllocatedOrChangedID1, partialPalletFact.LastAllocatedOrChangedID);

				foreach (var putawayLocationFact in putawayLocationFacts)
				{
					AssertEquals(nameof(PutawayLocationFact.WarehousePK), warehousePk, putawayLocationFact.WarehousePK);
					AssertEquals(nameof(PutawayLocationFact.LocationTypeCode), locationTypeCode,
						putawayLocationFact.LocationTypeCode);
					AssertEquals(nameof(PutawayLocationFact.LocationClass), locationClass,
						putawayLocationFact.LocationClass);
					AssertEquals(nameof(PutawayLocationFact.RowName), rowName, putawayLocationFact.RowName);
					AssertEquals(nameof(PutawayLocationFact.Column), column, putawayLocationFact.Column);
					AssertEquals(nameof(PutawayLocationFact.Level), level, putawayLocationFact.Level);
					AssertEquals(nameof(PutawayLocationFact.Tray), tray, putawayLocationFact.Tray);
					AssertEquals(nameof(PutawayLocationFact.AreaName), "PUT", putawayLocationFact.AreaName);
					AssertEquals(nameof(PutawayLocationFact.AreaTypeCode), "HEL", putawayLocationFact.AreaTypeCode);

					AssertEquals(nameof(PutawayLocationFact.PutawaySequence), putawaySequence,
						putawayLocationFact.PutawaySequence);
					AssertEquals(nameof(PutawayLocationFact.LocationStatus), locationStatus,
						putawayLocationFact.LocationStatus);
					AssertEquals(nameof(PutawayLocationFact.AvailableWeight), availableWeight,
						putawayLocationFact.AvailableWeight);
					AssertEquals(nameof(PutawayLocationFact.MaxWeightUnit), maxWeightUnit,
						putawayLocationFact.MaxWeightUnit);
					AssertEquals(nameof(PutawayLocationFact.AvailableVolume), availableVolume,
						putawayLocationFact.AvailableVolume);
					AssertEquals(nameof(PutawayLocationFact.MaxVolumeUnit), maxVolumeUnit,
						putawayLocationFact.MaxVolumeUnit);
					AssertEquals(nameof(PutawayLocationFact.CurrentAndIncomingStock), currentAndPendingStock,
						putawayLocationFact.CurrentAndIncomingStock);
					AssertEquals(nameof(PutawayLocationFact.IsDynamicPickFace), false,
						putawayLocationFact.IsDynamicPickFace);
					AssertEquals(nameof(PutawayLocationFact.IsFixedPickFace), false,
						putawayLocationFact.IsFixedPickFace);
					AssertEquals(nameof(PutawayLocationFact.ProductPK), productPk, putawayLocationFact.ProductPK);
					AssertEquals(nameof(PutawayLocationFact.ClientPK), clientPk, putawayLocationFact.ClientPK);
					AssertEquals(nameof(PutawayLocationFact.IsTSAKnownLocation), false,
						putawayLocationFact.IsTSAKnownLocation);
				}

				var productMock = new Mock<IPutawayProductFact>();
				productMock.Setup(p => p.PK).Returns(productPk);

				var invMock1 = new Mock<IInventoryFact>();
				invMock1.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(productMock.Object));
				invMock1.Setup(inv => inv.Quantity).Returns(1m);

				var invMock2 = new Mock<IInventoryFact>();
				invMock2.Setup(inv => inv.Product).Returns(new FactJoin<IPutawayProductFact>(productMock.Object));
				invMock2.Setup(inv => inv.Quantity).Returns(15m);

				AssertEquals(nameof(PutawayLocationFact.ContainsThisProduct), false,
					locationFact.ContainsThisProduct(invMock1.Object));
				AssertEquals(nameof(PutawayLocationFact.ContainsThisProduct), false,
					partialPalletFact.ContainsThisProduct(invMock1.Object));

				partialPalletFact.UpdateDataAfterPutawayAllocation(invMock1.Object, 1m);
				AssertEquals(nameof(PutawayLocationFact.ContainsThisProduct), true,
					locationFact.ContainsThisProduct(invMock1.Object));
				AssertEquals(nameof(PutawayLocationFact.ContainsThisProduct), true,
					partialPalletFact.ContainsThisProduct(invMock1.Object));

				AssertEquals(nameof(PutawayLocationFact.ContainsThisProduct), 19m, locationFact.AvailableUnits);
				AssertEquals(nameof(PutawayLocationFact.ContainsThisProduct), 9m, partialPalletFact.AvailableUnits);

				locationFact.UpdateDataAfterPutawayAllocation(invMock2.Object, 12m);
				AssertEquals(nameof(PutawayLocationFact.ContainsThisProduct), 7m, locationFact.AvailableUnits);
				AssertEquals(nameof(PutawayLocationFact.ContainsThisProduct), 7m, partialPalletFact.AvailableUnits);
			}
		}

		#endregion
	}
}
