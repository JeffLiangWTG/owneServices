using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.ProductionRules.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.US.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Environment.CodeLists.US;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PutawayEngineManagerForReceiveTest : PutawayEngineManagerTest<WhsReceive, WhsReceiveLine>
	{
		#region TestIOCConfiguration

		public void TestIOCConfiguration()
		{
			var putawayEngineManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
			AssertType<PutawayEngineManagerForReceive>(putawayEngineManager);
			AssertEquals("Should be a singleton.", true,
				ReferenceEquals(putawayEngineManager, ObjectFactory.Get<IPutawayEngineManagerForReceive>()));
		}

		#endregion

		#region TestConstructor

		public void TestConstructor_Engine_Null_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PutawayEngineManagerForReceive(null,
				Mock.Of<ICrossDockManager>(), Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(), Mock.Of<IPutawayLocationCacheUpdater>()));
		}

		public void TestConstructor_CrossDockManager_Null_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayEngineManagerForReceive(Mock.Of<IUserHaltableProductionRulesEngineService>(), null,
					Mock.Of<IPutawayExistingPalletManager>(), Mock.Of<IPutawayLocationFactLoader>(),
					Mock.Of<IPutawayLocationCacheUpdater>()));
		}

		public void TestConstructor_PutawayExistingPalletManager_Null_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayEngineManagerForReceive(Mock.Of<IUserHaltableProductionRulesEngineService>(),
					Mock.Of<ICrossDockManager>(), null, Mock.Of<IPutawayLocationFactLoader>(),
					Mock.Of<IPutawayLocationCacheUpdater>()));
		}

		public void TestConstructor_PutawayLocationFactLoader_Null_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayEngineManagerForReceive(Mock.Of<IUserHaltableProductionRulesEngineService>(),
					Mock.Of<ICrossDockManager>(), Mock.Of<IPutawayExistingPalletManager>(), null,
					Mock.Of<IPutawayLocationCacheUpdater>()));
		}

		public void TestConstructor_PutawayLocationCacheUpdater_Null_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayEngineManagerForReceive(Mock.Of<IUserHaltableProductionRulesEngineService>(),
					Mock.Of<ICrossDockManager>(), Mock.Of<IPutawayExistingPalletManager>(),
					Mock.Of<IPutawayLocationFactLoader>(), null));
		}

		#endregion

		#region TestPutaway_GetFacts

		public void TestPutaway_GetFacts_Sequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m,
				data.Whs1.DefaultLocation); // Already putaway
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m); // "Crossdocked"
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m); // "Pallet"
			var line4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 4m); // Will putaway
			var emptyLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m); // No quantity to putaway
			var emptyPalletLine =
				Helper.CreateWhsReceiveLine(receive, data.Part1, 0m, null, "PLT-123"); // No quantity to putaway
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m); // Not passed in

			var mockSequence = new MockSequence();

			var crossDockManagerMock = new Mock<ICrossDockManager>(MockBehavior.Strict);
			crossDockManagerMock.InSequence(mockSequence)
				.Setup(cdm => cdm.AllocateCrossDockedLines(It.IsAny<BusinessObjectFactory>(),
					It.Is(ValidateInventory(line2, line3, line4))))
				.Callback(() => line2.WE_WL = data.Whs1.DefaultLocation.PK);

			var palletManagerMock = new Mock<IPutawayExistingPalletManager>(MockBehavior.Strict);
			palletManagerMock.InSequence(mockSequence)
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive.PK)),
					It.Is(ValidateInventory(line3, line4)),
					data.Whs1)).Returns(Enumerable.Empty<WhsInventoryView>())
				.Callback(() => line3.WE_WL = data.Whs1.DefaultLocation.PK);

			IEnumerable<ZGuid> skipLocationPKsPassedIn = null;
			var locationFact = Mock.Of<IPutawayLocationFact>();
			var locationFactLoader = new Mock<IPutawayLocationFactLoader>(MockBehavior.Strict);
			locationFactLoader
				.InSequence(mockSequence).Setup(lfl => lfl.GetPutawayLocationFacts(Factory, data.Whs1.PK, new[] { data.Org1.PK },
					It.Is<IEnumerable<ZGuid>>(invs => invs.Single() == data.Part1.PK), It.IsAny<IEnumerable<ZGuid>>()))
				.Callback((Action<BusinessObjectFactory, ZGuid, IEnumerable<ZGuid>, IEnumerable<ZGuid>, IEnumerable<ZGuid>>)((factory, whsPK, clientPKs, partPKs, locationPKs) => skipLocationPKsPassedIn = locationPKs))
				.Returns(new[] { locationFact });

			var skipLocationPKs = new List<ZGuid> { ZGuid.NewZGuid(), ZGuid.NewZGuid() };
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock.Object,
				palletManagerMock.Object,
				locationFactLoader.Object,
				receive,
				new[] { line1, line2, line3, line4, emptyLine, emptyPalletLine },
				null,
				skipLocationPKs);

			var resultingFacts = getFacts();
			AssertEquals("Should have only returned one location.", locationFact,
				resultingFacts.OfType<IPutawayLocationFact>().Single());
			AssertEquals("Should have only returned one inventory for running putaway rules on.", line4.PK,
				resultingFacts.OfType<IInventoryFact>().Single().PK);
			AssertContainsExactElementsInExactOrder(
				"Should pass skipLocationPKs into the call of GetPutawayLocationFacts.",
				skipLocationPKs, skipLocationPKsPassedIn);
		}

		public void TestPutaway_GetFacts_Sequence_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = (WhsReceiveLine)Helper
				.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 1m)
				.InDocketLine; // Already putaway on DDL transfer
			var line2 = (WhsReceiveLine)Helper
				.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 1m)
				.InDocketLine; // "Crossdocked"
			var line3 = (WhsReceiveLine)Helper
				.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 1m)
				.InDocketLine; // "Pallet"
			var line4 = (WhsReceiveLine)Helper
				.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 1m)
				.InDocketLine; // Will putaway
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1,
				nonDockDoorLocation, "PLT1", 1m);
			var transferLine2 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, null, "PLT1", 1m);
			var transferLine3 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, null, "PLT1", 1m);
			var transferLine4 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, null, "PLT1", 1m);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transferLine3.PickedTime = ZDateTimeOffset.Now;
			transferLine4.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			var mockSequence = new MockSequence();

			var crossDockManagerMock = new Mock<ICrossDockManager>(MockBehavior.Strict);
			crossDockManagerMock.InSequence(mockSequence)
				.Setup(cdm => cdm.AllocateCrossDockedLines(It.IsAny<BusinessObjectFactory>(),
					It.Is(ValidateInventory(line2, line3, line4)))).Callback(() =>
					((ILineToPutaway)line2).LocationPK = nonDockDoorLocation.PK);

			var palletManagerMock = new Mock<IPutawayExistingPalletManager>(MockBehavior.Strict);
			palletManagerMock.InSequence(mockSequence)
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive.PK)),
					It.Is(ValidateInventory(line3, line4)),
					data.Whs1))
				.Returns(Enumerable.Empty<WhsInventoryView>())
				.Callback(() => ((ILineToPutaway)line3).LocationPK = nonDockDoorLocation.PK);

			var locationFact = Mock.Of<IPutawayLocationFact>();
			var locationFactLoader = new Mock<IPutawayLocationFactLoader>(MockBehavior.Strict);
			locationFactLoader
				.InSequence(mockSequence).Setup(lfl => lfl.GetPutawayLocationFacts(Factory, data.Whs1.PK, new[] { data.Org1.PK },
					It.Is<IEnumerable<ZGuid>>(invs => invs.Single() == data.Part1.PK), It.IsAny<IEnumerable<ZGuid>>()))
				.Returns(new[] { locationFact });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock.Object,
				palletManagerMock.Object,
				locationFactLoader.Object,
				receive,
				new[] { line2, line3, line4 });

			var resultingFacts = getFacts();
			AssertEquals("Should have only returned one location.", locationFact,
				resultingFacts.OfType<IPutawayLocationFact>().Single());
			AssertEquals("Should have only returned one inventory for running putaway rules on.", line4.PK,
				resultingFacts.OfType<IInventoryFact>().Single().PK);
		}

		public void TestPutaway_GetFacts_DistinctsProductPKs_AnotherReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var otherClient = Helper.CreateClient("OTH");
			var part3 = Helper.CreateProduct("P3", otherClient);

			var otherReceive = Helper.CreateWhsReceive(otherClient, data.Whs1, "OR");
			var otherLine = Helper.CreateWhsReceiveLine(otherReceive, part3, 10m);
			otherLine.WE_PalletID = "PLT-123";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m);
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 3m);
			var line4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 4m);
			line4.WE_PalletID = "PLT-123";
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);

			var mockSequence = new MockSequence();

			var crossDockManagerMock = new Mock<ICrossDockManager>(MockBehavior.Strict);
			crossDockManagerMock.InSequence(mockSequence).Setup(cdm =>
				cdm.AllocateCrossDockedLines(It.IsAny<BusinessObjectFactory>(),
					It.Is(ValidateInventory(line1, line2, line3, line4))));

			var palletManagerMock = new Mock<IPutawayExistingPalletManager>(MockBehavior.Strict);
			palletManagerMock.InSequence(mockSequence)
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive.PK)),
					It.Is(ValidateInventory(line1, line2, line3, line4)),
					data.Whs1))
				.Returns(new[]
				{
					otherLine.Inventory[0]
				}); // It doesn't matter if the other line is on another client/has another product - a mixed client/product pallet will never match a location for a specific client/product

			var locationFact = Mock.Of<IPutawayLocationFact>();
			var locationFactLoader = new Mock<IPutawayLocationFactLoader>(MockBehavior.Strict);
			locationFactLoader
				.InSequence(mockSequence)
				.Setup(lfl => lfl.GetPutawayLocationFacts(Factory, data.Whs1.PK, new[] { data.Org1.PK },
					It.Is<IEnumerable<ZGuid>>(prods =>
						prods.Count() == 2 && prods.Contains(data.Part1.PK) && prods.Contains(data.Part2.PK)), It.IsAny<IEnumerable<ZGuid>>()))
				.Returns(new[] { locationFact });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock.Object,
				palletManagerMock.Object,
				locationFactLoader.Object,
				receive,
				new[] { line1, line2, line3, line4 });

			var resultingFacts = getFacts();
			AssertEquals("Should have only returned one location.", locationFact,
				resultingFacts.OfType<IPutawayLocationFact>().Single());
		}

		public void TestPutaway_GetFacts_Sequence_NoInventoryRemaining()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m,
				data.Whs1.DefaultLocation); // Already putaway
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m); // "Crossdocked"
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m); // "Pallet"
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m); // Not passed in

			var mockSequence = new MockSequence();

			var crossDockManagerMock = new Mock<ICrossDockManager>(MockBehavior.Strict);
			crossDockManagerMock.InSequence(mockSequence)
				.Setup(cdm => cdm.AllocateCrossDockedLines(It.IsAny<BusinessObjectFactory>(),
					It.Is(ValidateInventory(line2, line3)))).Callback(() => line2.WE_WL = data.Whs1.DefaultLocation.PK);

			var palletManagerMock = new Mock<IPutawayExistingPalletManager>(MockBehavior.Strict);
			palletManagerMock.InSequence(mockSequence)
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive.PK)),
					It.Is(ValidateInventory(line3)),
					data.Whs1))
				.Returns(Enumerable.Empty<WhsInventoryView>())
				.Callback(() => line3.WE_WL = data.Whs1.DefaultLocation.PK);

			var locationFactLoader = new Mock<IPutawayLocationFactLoader>(MockBehavior.Strict); // Should not get called

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock.Object,
				palletManagerMock.Object,
				locationFactLoader.Object,
				receive,
				new[] { line2, line3 });

			var resultingFacts = getFacts();
			AssertEquals("Should have returned no facts, not even locations.", false, resultingFacts.Any());
		}

		public void TestPutaway_GetFacts_InventoryData()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Core.Constants.PkgUnit.Pallet, 5m);
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, (ZString)Core.Constants.PkgUnit.Pallet)
				.F3_UOMType = UOMPackTypesList.Codes.Pallet;

			data.Part1.OP_Weight = 2m;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Pounds;
			data.Part1.OP_Cubic = 4m;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicDecimetres;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			receive.WD_ExternalReference = "RR";
			receive.WD_CustomerReference = "CR";
			receive.WD_RS_NKServiceLevel = "TST";
			receive.WD_ArrivalDate = ZDateTime.BrettsBirthday.ToOffset();

			var inventoryLine = receive.Lines[0];
			inventoryLine.WE_WHC_NKOriginalInventoryHeldCode = "123";
			inventoryLine.WE_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			inventoryLine.WE_ClientOrderedUnits = 3m;
			inventoryLine.WE_TransactionQuantity = 5m;

			inventoryLine.WE_PartAttrib1 = "1";
			inventoryLine.WE_PartAttrib2 = "2";
			inventoryLine.WE_PartAttrib3 = "3";
			inventoryLine.WE_ExpiryDate = ZDate.BrettsBirthday.AddDays(20);
			inventoryLine.WE_PackingDate = ZDate.BrettsBirthday.AddDays(-10);
			inventoryLine.WE_RequiredByDate = ZDateTime.BrettsBirthday.AddDays(10).ToOffset();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Quantity), 5m, inventoryFact.Quantity);
			AssertEquals(nameof(IInventoryFact.PackUnits), 1m, inventoryFact.PackUnits);
			AssertEquals(nameof(IInventoryFact.PackUQ), Core.Constants.PkgUnit.Pallet, inventoryFact.PackUQ);
			AssertEquals(nameof(IInventoryFact.UOMType), UOMPackTypesList.Codes.Pallet, inventoryFact.UOMType);
			AssertEquals(nameof(IInventoryFact.ServiceLevel), "TST", inventoryFact.ServiceLevel);
			AssertEquals(nameof(IInventoryFact.ReceiveReference), "RR", inventoryFact.ReceiveReference);
			AssertEquals(nameof(IInventoryFact.CustomerReference), "CR", inventoryFact.CustomerReference);
			AssertEquals(nameof(IInventoryFact.HoldCode), "123", inventoryFact.HoldCode);
			AssertEquals(nameof(IInventoryFact.ArrivalDate), ZDateTime.BrettsBirthday, inventoryFact.ArrivalDate);
			AssertEquals(nameof(IInventoryFact.RequiredDate), ZDateTime.BrettsBirthday.AddDays(10),
				inventoryFact.RequiredDate);
			AssertEquals(nameof(IInventoryFact.PartAttribute1), "1", inventoryFact.PartAttribute1);
			AssertEquals(nameof(IInventoryFact.PartAttribute2), "2", inventoryFact.PartAttribute2);
			AssertEquals(nameof(IInventoryFact.PartAttribute3), "3", inventoryFact.PartAttribute3);
			AssertEquals(nameof(IInventoryFact.ExpiryDate), ZDate.BrettsBirthday.AddDays(20), inventoryFact.ExpiryDate);
			AssertEquals(nameof(IInventoryFact.PackingDate), ZDateTime.BrettsBirthday.AddDays(-10),
				inventoryFact.PackingDate);
			AssertEquals(nameof(IInventoryFact.ProductWeight), 2m, inventoryFact.ProductWeight);
			AssertEquals(nameof(IInventoryFact.ProductWeightUQ), Core.Constants.Weight.Pounds,
				inventoryFact.ProductWeightUQ);
			AssertEquals(nameof(IInventoryFact.ProductVolume), 4m, inventoryFact.ProductVolume);
			AssertEquals(nameof(IInventoryFact.ProductVolumeUQ), Core.Constants.Volume.CubicDecimetres,
				inventoryFact.ProductVolumeUQ);
			AssertNull(nameof(IInventoryFact.Equipment), inventoryFact.Equipment.Fact);
		}

		public void TestPutaway_GetFacts_IsTSAKnown()
		{
			var helper = new WhsTestHelperFunctionsEnvUS(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.MainAddress;
			address.OA_RL_NKRelatedPortCode = helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			helper.SetOrgAddressTSAStatus(address, Environment.CodeLists.US.TSAStatus.Codes.Known);
			helper.CreateProductClientRelationShip(organisation, data.Part1);

			var receive = Helper.CreateWhsReceiveWithInventory(organisation, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.IsTSAKnownClient), true, inventoryFact.IsTSAKnownClient);
		}

		public void TestPutaway_GetFacts_IsTSAKnown_Unknown()
		{
			var helper = new WhsTestHelperFunctionsEnvUS(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.MainAddress;
			address.OA_RL_NKRelatedPortCode = helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			helper.SetOrgAddressTSAStatus(address, Environment.CodeLists.US.TSAStatus.Codes.Unknown);
			helper.CreateProductClientRelationShip(organisation, data.Part1);

			var receive = Helper.CreateWhsReceiveWithInventory(organisation, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.IsTSAKnownClient), false, inventoryFact.IsTSAKnownClient);
		}

		public void TestPutaway_GetFacts_IsTSAKnown_NoStatusSet()
		{
			var helper = new WhsTestHelperFunctionsEnvUS(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.MainAddress;
			address.OA_RL_NKRelatedPortCode = helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			helper.CreateProductClientRelationShip(organisation, data.Part1);

			var receive = Helper.CreateWhsReceiveWithInventory(organisation, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.IsTSAKnownClient), false, inventoryFact.IsTSAKnownClient);
		}

		public void TestPutaway_GetFacts_IsTSAKnown_NotUSOrganisation()
		{
			var helper = new WhsTestHelperFunctionsEnvUS(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.MainAddress;
			address.OA_RL_NKRelatedPortCode = helper.GetCountryUNLOCO(Core.Constants.CountryCodes.Australia).RL_Code;
			helper.CreateProductClientRelationShip(organisation, data.Part1);

			var receive = Helper.CreateWhsReceiveWithInventory(organisation, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.IsTSAKnownClient), false, inventoryFact.IsTSAKnownClient);
		}

		public void TestPutaway_GetFacts_TSAPolicyRequired()
		{
			var helper = new WhsTestHelperFunctionsEnvUS(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var usWhs = helper.CreateWarehouse("TST");
			helper.SetWarehouseTSAStatus(usWhs, TSAStatus.Codes.Known);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, usWhs, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.TSAPolicyRequired), true, inventoryFact.TSAPolicyRequired);
		}

		public void TestPutaway_GetFacts_TSAPolicyRequired_NotUSWarehouse()
		{
			var helper = new WhsTestHelperFunctionsEnvUS(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			helper.SetWarehouseTSAStatus(data.Whs1, TSAStatus.Codes.Known);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.TSAPolicyRequired), false, inventoryFact.TSAPolicyRequired);
		}

		public void TestPutaway_GetFacts_TSAPolicyRequired_Unknown()
		{
			var helper = new WhsTestHelperFunctionsEnvUS(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var usWhs = helper.CreateWarehouse("TST");
			helper.SetWarehouseTSAStatus(usWhs, TSAStatus.Codes.Unknown);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, usWhs, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.TSAPolicyRequired), false, inventoryFact.TSAPolicyRequired);
		}

		public void TestPutaway_GetFacts_Supplier()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var supplier = Helper.CreateClient("SUP");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var supplierFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == supplier.PK);
			AssertEquals(nameof(IOrganisationFact.Code), supplier.OH_Code, supplierFact.Code);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Supplier), supplier.PK, inventoryFact.Supplier.Fact.PK);
		}

		public void TestPutaway_GetFacts_Supplier_IsOrgProxyOfCurrentCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var supplier = Helper.CreateClient("SUP");
			var company = GlbCompany.GetCurrentCompany(Factory);
			company.GC_OH_OrgProxy = supplier.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var supplierFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == supplier.PK);
			AssertEquals(nameof(IOrganisationFact.Code), supplier.OH_Code, supplierFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, supplierFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, supplierFact.IsProxyOrgOfCurrentCompany);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Supplier), supplier.PK, inventoryFact.Supplier.Fact.PK);
		}

		public void TestPutaway_GetFacts_Supplier_IsOrgProxyOfCurrentCompany_BranchOrgProxy()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var supplier = Helper.CreateClient("SUP");
			var company = GlbCompany.GetCurrentCompany(Factory);
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = supplier.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var supplierFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == supplier.PK);
			AssertEquals(nameof(IOrganisationFact.Code), supplier.OH_Code, supplierFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, supplierFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, supplierFact.IsProxyOrgOfCurrentCompany);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Supplier), supplier.PK, inventoryFact.Supplier.Fact.PK);
		}

		public void TestPutaway_GetFacts_Supplier_IsOrgProxyOfAnyCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var supplier = Helper.CreateClient("SUP");
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = supplier.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var supplierFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == supplier.PK);
			AssertEquals(nameof(IOrganisationFact.Code), supplier.OH_Code, supplierFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, supplierFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, supplierFact.IsProxyOrgOfCurrentCompany);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Supplier), supplier.PK, inventoryFact.Supplier.Fact.PK);
		}

		public void TestPutaway_GetFacts_Supplier_IsOrgProxyOfAnyCompany_BranchOrgProxy()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var supplier = Helper.CreateClient("SUP");
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = supplier.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var supplierFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == supplier.PK);
			AssertEquals(nameof(IOrganisationFact.Code), supplier.OH_Code, supplierFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, supplierFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, supplierFact.IsProxyOrgOfCurrentCompany);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Supplier), supplier.PK, inventoryFact.Supplier.Fact.PK);
		}

		public void TestPutaway_GetFacts_Supplier_IsTheClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			receive.SupplierDocAddress.OrganisationPK = data.Org1.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var supplierFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == data.Org1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, supplierFact.Code);

			var inventoryFact = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.Client), data.Org1.PK, inventoryFact.Client.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Supplier), data.Org1.PK, inventoryFact.Supplier.Fact.PK);
		}

		public void TestPutaway_GetFacts_Consignees()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var consignee1 = Helper.CreateClient("CN1");
			var consignee2 = Helper.CreateClient("CN2");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			inventory1.ConsigneeDocAddress.OrganisationPK = consignee1.PK;
			inventory2.ConsigneeDocAddress.OrganisationPK = consignee2.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { inventory1, inventory2 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var consigneeFact1 = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == consignee1.PK);
			var consigneeFact2 = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == consignee2.PK);
			AssertEquals(nameof(IOrganisationFact.Code), consignee1.OH_Code, consigneeFact1.Code);
			AssertEquals(nameof(IOrganisationFact.Code), consignee2.OH_Code, consigneeFact2.Code);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == inventory1.PK);
			AssertEquals(nameof(IInventoryFact.Supplier), consignee1.PK, inventoryFact1.Consignee.Fact.PK);

			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == inventory2.PK);
			AssertEquals(nameof(IInventoryFact.Supplier), consignee2.PK, inventoryFact2.Consignee.Fact.PK);
		}

		public void TestPutaway_GetFacts_Consignee_IsTheClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			inventory1.ConsigneeDocAddress.OrganisationPK = data.Org1.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { inventory1, inventory2 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			AssertEquals("Should be a single organisation fact.", 1,
				nestedFacts.OfType<IOrganisationFact>().Count(o => o.PK == data.Org1.PK));
		}

		public void TestPutaway_GetFacts_Consignee_IsTheSupplier()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var supplier = Helper.CreateClient("SUP");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;

			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			inventory1.ConsigneeDocAddress.OrganisationPK = supplier.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { inventory1, inventory2 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			AssertEquals("Should be a single organisation fact for the supplier organisation.", 1,
				nestedFacts.OfType<IOrganisationFact>().Count(o => o.PK == supplier.PK));
		}

		public void TestPutaway_GetFacts_Consignee_IsOrgProxyOfCurrentCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var consignee = Helper.CreateClient("CN1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			inventory1.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			var company = GlbCompany.GetCurrentCompany(Factory);
			company.GC_OH_OrgProxy = consignee.PK;
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { inventory1, inventory2 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var consigneeFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == consignee.PK);
			AssertEquals(nameof(IOrganisationFact.Code), consignee.OH_Code, consigneeFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, consigneeFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, consigneeFact.IsProxyOrgOfCurrentCompany);
		}

		public void TestPutaway_GetFacts_Consignee_IsOrgProxyOfCurrentCompany_BranchOrgProxy()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var consignee = Helper.CreateClient("CN1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			inventory1.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			var company = GlbCompany.GetCurrentCompany(Factory);
			var branch = company.FirstActiveBranch;
			branch.GB_OH_OrgProxy = consignee.PK;
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { inventory1, inventory2 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var consigneeFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == consignee.PK);
			AssertEquals(nameof(IOrganisationFact.Code), consignee.OH_Code, consigneeFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, consigneeFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, consigneeFact.IsProxyOrgOfCurrentCompany);
		}

		public void TestPutaway_GetFacts_Consignee_IsOrgProxyOfAnyCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var consignee = Helper.CreateClient("CN1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			inventory1.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = consignee.PK;
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { inventory1, inventory2 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var consigneeFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == consignee.PK);
			AssertEquals(nameof(IOrganisationFact.Code), consignee.OH_Code, consigneeFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, consigneeFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, consigneeFact.IsProxyOrgOfCurrentCompany);
		}

		public void TestPutaway_GetFacts_Consignee_IsOrgProxyOfAnyCompany_BranchOrgProxy()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var consignee = Helper.CreateClient("CN1");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			inventory1.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = consignee.PK;
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { inventory1, inventory2 });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var consigneeFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == consignee.PK);
			AssertEquals(nameof(IOrganisationFact.Code), consignee.OH_Code, consigneeFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, consigneeFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, consigneeFact.IsProxyOrgOfCurrentCompany);
		}

		public void TestPutaway_GetFacts_Pallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, null, "P1",
				allocateLocations: false, finalise: false);
			var line1 = receive.Lines[0];
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "P1");
			var line3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, null, "P2");
			var line4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, null, "P2");
			var line5 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { line1, line2, line3, line4, line5 });

			var resultingFacts = getFacts();
			var palletFacts = resultingFacts.OfType<IPalletFact>().ToArray();
			AssertEquals(2, palletFacts.Length);
			var pallet1Fact = palletFacts.Single(p => p.PalletID == "P1");
			var pallet2Fact = palletFacts.Single(p => p.PalletID == "P2");

			var part1OwnerRelation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var part2OwnerRelation =
				data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line1.PK);
			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line2.PK);
			AssertEquals(nameof(IInventoryFact.Product), part1OwnerRelation.PK, inventoryFact1.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), part1OwnerRelation.PK, inventoryFact2.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.PalletID), "P1", inventoryFact1.PalletID);
			AssertEquals(nameof(IInventoryFact.PalletID), "P1", inventoryFact2.PalletID);

			var inventoryFact3 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line3.PK);
			var inventoryFact4 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line4.PK);
			AssertEquals(nameof(IInventoryFact.Product), part2OwnerRelation.PK, inventoryFact3.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), part2OwnerRelation.PK, inventoryFact4.Product.Fact.PK);

			AssertEquals(nameof(IInventoryFact.PalletID), "P2", inventoryFact3.PalletID);
			AssertEquals(nameof(IInventoryFact.PalletID), "P2", inventoryFact4.PalletID);

			var inventoryFact5 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line5.PK);
			AssertEquals(nameof(IInventoryFact.PalletID), string.Empty, inventoryFact5.PalletID);
		}

		public void TestPutaway_GetFacts_Pallets_CaseInsensitive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, null, "P1",
				allocateLocations: false, finalise: false);
			var line1 = receive.Lines[0];
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "p1");

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader,
				receive,
				new[] { line1, line2 });

			var resultingFacts = getFacts();
			var palletFacts = resultingFacts.OfType<IPalletFact>().ToArray();
			AssertEquals(1, palletFacts.Length);

			var pallet1Fact = palletFacts.Single();
			AssertEquals(nameof(IPalletFact.PalletID), "P1", pallet1Fact.PalletID);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line1.PK);
			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Single(i => i.PK == line2.PK);
			AssertEquals(nameof(IInventoryFact.PalletID), "P1", inventoryFact1.PalletID);
			AssertEquals(nameof(IInventoryFact.PalletID), "p1", inventoryFact2.PalletID);
		}

		static Expression<Func<IEnumerable<WhsReceiveLine>, bool>> ValidateInventory(params WhsReceiveLine[] lines) =>
			invLines => invLines.All(i => lines.Select(i2 => i2.PK).Contains(i.PK));

		#endregion

		#region TestPutaway_GetFacts_DifferentReceive

		public void TestPutaway_GetFacts_DifferentReceive_InventoryData()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Core.Constants.PkgUnit.Pallet, 5m);
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, (ZString)Core.Constants.PkgUnit.Pallet)
				.F3_UOMType = UOMPackTypesList.Codes.Pallet;

			data.Part1.OP_Weight = 2m;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Pounds;
			data.Part1.OP_Cubic = 4m;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicDecimetres;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			receive1.WD_ExternalReference = "RR1";
			receive1.WD_CustomerReference = "CR1";
			receive1.WD_RS_NKServiceLevel = "TST";
			receive1.WD_ArrivalDate = ZDateTime.BrettsBirthday.ToOffset();
			receive1.WD_RequiredDate = ZDateTime.BrettsBirthday.AddDays(10).ToOffset();

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m,
				allocateLocations: false, finalise: false);
			receive2.WD_ExternalReference = "RR2";
			receive2.WD_CustomerReference = "CR2";
			receive2.WD_RS_NKServiceLevel = "TS2";
			receive2.WD_ArrivalDate = ZDateTime.BrettsBirthday.ToOffset().AddDays(20);

			var inventoryLine1 = receive1.Lines[0];
			inventoryLine1.WE_PalletID = "PLT-123";
			inventoryLine1.WE_WHC_NKOriginalInventoryHeldCode = "123";
			inventoryLine1.WE_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			inventoryLine1.WE_ClientOrderedUnits = 3m;
			inventoryLine1.WE_TransactionQuantity = 5m;
			inventoryLine1.WE_PartAttrib1 = "1";
			inventoryLine1.WE_PartAttrib2 = "2";
			inventoryLine1.WE_PartAttrib3 = "3";
			inventoryLine1.WE_ExpiryDate = ZDate.BrettsBirthday.AddDays(20);
			inventoryLine1.WE_PackingDate = ZDate.BrettsBirthday.AddDays(-10);
			inventoryLine1.WE_RequiredByDate = ZDateTime.BrettsBirthday.AddDays(10).ToOffset();

			var inventoryLine2 = receive2.Lines[0];
			inventoryLine2.WE_PalletID = "PLT-123";
			inventoryLine2.WE_WHC_NKOriginalInventoryHeldCode = "ABC";
			inventoryLine2.WE_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			inventoryLine2.WE_ClientOrderedUnits = 4m;
			inventoryLine2.WE_TransactionQuantity = 6m;
			inventoryLine2.WE_PartAttrib1 = "4";
			inventoryLine2.WE_PartAttrib2 = "5";
			inventoryLine2.WE_PartAttrib3 = "6";
			inventoryLine2.WE_ExpiryDate = ZDate.BrettsBirthday.AddDays(50);
			inventoryLine2.WE_PackingDate = ZDate.BrettsBirthday.AddDays(-20);
			inventoryLine2.WE_RequiredByDate = ZDateTime.BrettsBirthday.AddDays(30).ToOffset();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();
			palletManagerMock
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive1.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					data.Whs1))
				.Returns(new[] { inventoryLine2.Inventory[0] });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive1,
				new[] { receive1.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			AssertEquals("Should have returned two inventories.", 2, resultingFacts.OfType<IInventoryFact>().Count());
			AssertEquals("Should have returned a single copy of the client.", 1,
				nestedFacts.OfType<IOrganisationFact>().Count(c => c.PK == data.Org1.PK));

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().First();
			AssertEquals(nameof(IInventoryFact.Quantity), 5m, inventoryFact1.Quantity);
			AssertEquals(nameof(IInventoryFact.Equipment), "PLT-123", inventoryFact1.PalletID);
			AssertEquals(nameof(IInventoryFact.PackUnits), 1m, inventoryFact1.PackUnits);
			AssertEquals(nameof(IInventoryFact.PackUQ), Core.Constants.PkgUnit.Pallet, inventoryFact1.PackUQ);
			AssertEquals(nameof(IInventoryFact.ServiceLevel), "TST", inventoryFact1.ServiceLevel);
			AssertEquals(nameof(IInventoryFact.ReceiveReference), "RR1", inventoryFact1.ReceiveReference);
			AssertEquals(nameof(IInventoryFact.CustomerReference), "CR1", inventoryFact1.CustomerReference);
			AssertEquals(nameof(IInventoryFact.HoldCode), "123", inventoryFact1.HoldCode);
			AssertEquals(nameof(IInventoryFact.ArrivalDate), ZDateTime.BrettsBirthday, inventoryFact1.ArrivalDate);
			AssertEquals(nameof(IInventoryFact.RequiredDate), ZDateTime.BrettsBirthday.AddDays(10),
				inventoryFact1.RequiredDate);
			AssertEquals(nameof(IInventoryFact.PartAttribute1), "1", inventoryFact1.PartAttribute1);
			AssertEquals(nameof(IInventoryFact.PartAttribute2), "2", inventoryFact1.PartAttribute2);
			AssertEquals(nameof(IInventoryFact.PartAttribute3), "3", inventoryFact1.PartAttribute3);
			AssertEquals(nameof(IInventoryFact.ExpiryDate), ZDate.BrettsBirthday.AddDays(20),
				inventoryFact1.ExpiryDate);
			AssertEquals(nameof(IInventoryFact.PackingDate), ZDateTime.BrettsBirthday.AddDays(-10),
				inventoryFact1.PackingDate);
			AssertEquals(nameof(IInventoryFact.ProductWeight), 2m, inventoryFact1.ProductWeight);
			AssertEquals(nameof(IInventoryFact.ProductWeightUQ), Core.Constants.Weight.Pounds,
				inventoryFact1.ProductWeightUQ);
			AssertEquals(nameof(IInventoryFact.ProductVolume), 4m, inventoryFact1.ProductVolume);
			AssertEquals(nameof(IInventoryFact.ProductVolumeUQ), Core.Constants.Volume.CubicDecimetres,
				inventoryFact1.ProductVolumeUQ);
			AssertNull(nameof(IInventoryFact.Equipment), inventoryFact1.Equipment.Fact);

			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Last();
			AssertEquals(nameof(IInventoryFact.Quantity), 6m, inventoryFact2.Quantity);
			AssertEquals(nameof(IInventoryFact.Equipment), "PLT-123", inventoryFact2.PalletID);
			AssertEquals(nameof(IInventoryFact.PackUnits), 6m, inventoryFact2.PackUnits);
			AssertEquals(nameof(IInventoryFact.PackUQ), Core.Constants.PkgUnit.Pallet, inventoryFact2.PackUQ);
			AssertEquals(nameof(IInventoryFact.ServiceLevel), "TS2", inventoryFact2.ServiceLevel);
			AssertEquals(nameof(IInventoryFact.ReceiveReference), "RR2", inventoryFact2.ReceiveReference);
			AssertEquals(nameof(IInventoryFact.CustomerReference), "CR2", inventoryFact2.CustomerReference);
			AssertEquals(nameof(IInventoryFact.HoldCode), "ABC", inventoryFact2.HoldCode);
			AssertEquals(nameof(IInventoryFact.ArrivalDate), ZDateTime.BrettsBirthday.AddDays(20),
				inventoryFact2.ArrivalDate);
			AssertEquals(nameof(IInventoryFact.RequiredDate), ZDateTime.BrettsBirthday.AddDays(30),
				inventoryFact2.RequiredDate);
			AssertEquals(nameof(IInventoryFact.PartAttribute1), "4", inventoryFact2.PartAttribute1);
			AssertEquals(nameof(IInventoryFact.PartAttribute2), "5", inventoryFact2.PartAttribute2);
			AssertEquals(nameof(IInventoryFact.PartAttribute3), "6", inventoryFact2.PartAttribute3);
			AssertEquals(nameof(IInventoryFact.ExpiryDate), ZDate.BrettsBirthday.AddDays(50),
				inventoryFact2.ExpiryDate);
			AssertEquals(nameof(IInventoryFact.PackingDate), ZDateTime.BrettsBirthday.AddDays(-20),
				inventoryFact2.PackingDate);
			AssertEquals(nameof(IInventoryFact.ProductWeight), 2.0m, inventoryFact2.ProductWeight);
			AssertEquals(nameof(IInventoryFact.ProductWeightUQ), Core.Constants.Weight.Kilograms,
				inventoryFact2.ProductWeightUQ);
			AssertEquals(nameof(IInventoryFact.ProductVolume), 0.02m, inventoryFact2.ProductVolume);
			AssertEquals(nameof(IInventoryFact.ProductVolumeUQ), Core.Constants.Volume.CubicMetres,
				inventoryFact2.ProductVolumeUQ);
			AssertNull(nameof(IInventoryFact.Equipment), inventoryFact2.Equipment.Fact);
		}

		public void TestPutaway_GetFacts_DifferentReceive_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var org2 = Helper.CreateClient("Or2");
			var org3 = Helper.CreateClient("Or3");

			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Helper.CreateProductClientRelationShip(org3, data.Part1);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			var receive3 = Helper.CreateWhsReceiveWithInventory(org3, data.Whs1, "R3", data.Part1, 4m,
				allocateLocations: false, finalise: false);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();
			palletManagerMock
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive1.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					data.Whs1))
				.Returns(new[] { receive2.Inventory[0], receive3.Inventory[0] });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive1,
				new[] { receive1.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			AssertEquals("Should have returned three inventories.", 3, resultingFacts.OfType<IInventoryFact>().Count());
			AssertEquals("Should have returned three organisations.", 3,
				nestedFacts.OfType<IOrganisationFact>().Count());
			AssertNotNull(nestedFacts.OfType<IOrganisationFact>().SingleOrDefault(o => o.PK == data.Org1.PK));
			AssertNotNull(nestedFacts.OfType<IOrganisationFact>().SingleOrDefault(o => o.PK == org2.PK));
			AssertNotNull(nestedFacts.OfType<IOrganisationFact>().SingleOrDefault(o => o.PK == org3.PK));

			AssertNotNull(
				resultingFacts.OfType<IInventoryFact>().SingleOrDefault(i => i.Client.Fact.PK == data.Org1.PK));
			AssertNotNull(resultingFacts.OfType<IInventoryFact>().SingleOrDefault(i => i.Client.Fact.PK == org2.PK));
			AssertNotNull(resultingFacts.OfType<IInventoryFact>().SingleOrDefault(i => i.Client.Fact.PK == org3.PK));
		}

		public void TestPutaway_GetFacts_DifferentReceive_DifferentClients_IsTSAKnown()
		{
			var helper = new WhsTestHelperFunctionsEnvUS(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = organisation1.MainAddress;
			address1.OA_RL_NKRelatedPortCode =
				helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			helper.SetOrgAddressTSAStatus(address1, TSAStatus.Codes.Known);
			helper.CreateProductClientRelationShip(organisation1, data.Part1);

			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = organisation2.MainAddress;
			address2.OA_RL_NKRelatedPortCode =
				helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;
			helper.SetOrgAddressTSAStatus(address2, TSAStatus.Codes.Unknown);
			helper.CreateProductClientRelationShip(organisation2, data.Part1);

			var receive1 = Helper.CreateWhsReceiveWithInventory(organisation1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(organisation2, data.Whs1, "R2", data.Part1, 4m,
				allocateLocations: false, finalise: false);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>(MockBehavior.Strict);
			palletManagerMock
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.IsAny<IEnumerable<WhsReceive>>(),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					data.Whs1))
				.Returns(new[] { receive2.Inventory[0] });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive1,
				new[] { receive1.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			AssertEquals("Should have returned two inventories.", 2, resultingFacts.OfType<IInventoryFact>().Count());
			AssertEquals("Should have returned two organisations.", 2, nestedFacts.OfType<IOrganisationFact>().Count());

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().First();
			AssertEquals(nameof(IInventoryFact.IsTSAKnownClient), true, inventoryFact1.IsTSAKnownClient);

			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Last();
			AssertEquals(nameof(IInventoryFact.IsTSAKnownClient), false, inventoryFact2.IsTSAKnownClient);
		}

		public void TestPutaway_GetFacts_DifferentReceive_SameOrgSupplierPart()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var org2 = Helper.CreateClient("Or2");

			var relation1 =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var relation2 = Helper.CreateProductClientRelationShip(org2, data.Part1);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 4m,
				allocateLocations: false, finalise: false);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();
			palletManagerMock
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive1.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					data.Whs1)).Returns(new[] { receive2.Inventory[0] });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive1,
				new[] { receive1.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			AssertEquals("Should have returned two inventories.", 2, resultingFacts.OfType<IInventoryFact>().Count());
			AssertEquals("Should have returned two organisations.", 2, nestedFacts.OfType<IOrganisationFact>().Count());
			AssertEquals("Should have returned two products.", 2, nestedFacts.OfType<IProductFact>().Count());
			AssertNotNull(nestedFacts.OfType<IProductFact>().SingleOrDefault(p => p.PK == relation1.PK));
			AssertNotNull(nestedFacts.OfType<IProductFact>().SingleOrDefault(p => p.PK == relation2.PK));

			var inventory1 = resultingFacts.OfType<IInventoryFact>().First();
			AssertEquals(nameof(IInventoryFact.Client), data.Org1.PK, inventory1.Client.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), relation1.PK, inventory1.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part1.PK, inventory1.PartPK);

			var inventory2 = resultingFacts.OfType<IInventoryFact>().Last();
			AssertEquals(nameof(IInventoryFact.Client), org2.PK, inventory2.Client.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Product), relation2.PK, inventory2.Product.Fact.PK);
			AssertEquals(nameof(IInventoryFact.PartPK), data.Part1.PK, inventory2.PartPK);
		}

		public void TestPutaway_GetFacts_ClientIsOrgProxy_CurrentComany()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var relation1 = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			var company = GlbCompany.GetCurrentCompany(Factory);
			company.GC_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == data.Org1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, clientFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, clientFact.IsProxyOrgOfCurrentCompany);
		}

		public void TestPutaway_GetFacts_ClientIsOrgProxy_CurrentComanyBranch()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var relation1 = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			var company = GlbCompany.GetCurrentCompany(Factory);
			var branch = company.FirstActiveBranch;
			company.GC_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == data.Org1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, clientFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), true, clientFact.IsProxyOrgOfCurrentCompany);
		}

		public void TestPutaway_GetFacts_ClientIsOrgProxy_IsOrgProxyOfAnyCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var relation1 = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			var company = Factory.New<GlbCompany>();
			company.GC_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == data.Org1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, clientFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, clientFact.IsProxyOrgOfCurrentCompany);
		}

		public void TestPutaway_GetFacts_ClientIsOrgProxy_IsOrgProxyOfAnyCompany_Branch()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var relation1 = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m, allocateLocations: false, finalise: false);
			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = data.Org1.PK;
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive,
				new[] { receive.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == data.Org1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfAnyCompany), true, clientFact.IsProxyOrgOfAnyCompany);
			AssertEquals(nameof(IOrganisationFact.IsProxyOrgOfCurrentCompany), false, clientFact.IsProxyOrgOfCurrentCompany);
		}

		public void TestPutaway_GetFacts_DifferentReceive_MultipleSuppliers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var supplier1 = Helper.CreateClient("SP1");
			var supplier2 = Helper.CreateClient("SP2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			receive1.SupplierDocAddress.OrganisationPK = supplier1.PK;
			receive2.SupplierDocAddress.OrganisationPK = supplier2.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();
			palletManagerMock
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive1.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					data.Whs1))
				.Returns(new[] { receive2.Inventory[0] });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive1,
				new[] { receive1.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			AssertEquals("Should have returned two inventories.", 2, resultingFacts.OfType<IInventoryFact>().Count());
			AssertEquals("Should have returned three organisations (1x client + 2x suppliers).", 3,
				nestedFacts.OfType<IOrganisationFact>().Count());

			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == data.Org1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);

			var supplierFact1 = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == supplier1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), supplier1.OH_Code, supplierFact1.Code);

			var supplierFact2 = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == supplier2.PK);
			AssertEquals(nameof(IOrganisationFact.Code), supplier2.OH_Code, supplierFact2.Code);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().First();
			AssertEquals(nameof(IInventoryFact.Supplier), supplier1.PK, inventoryFact1.Supplier.Fact.PK);

			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Last();
			AssertEquals(nameof(IInventoryFact.Supplier), supplier2.PK, inventoryFact2.Supplier.Fact.PK);
		}

		public void TestPutaway_GetFacts_DifferentReceive_SameSupplier()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var supplier = Helper.CreateClient("SP1");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			receive1.SupplierDocAddress.OrganisationPK = supplier.PK;
			receive2.SupplierDocAddress.OrganisationPK = supplier.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();
			palletManagerMock
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive1.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					data.Whs1))
				.Returns(new[] { receive2.Inventory[0] });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive1,
				new[] { receive1.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			AssertEquals("Should have returned two inventories.", 2, resultingFacts.OfType<IInventoryFact>().Count());
			AssertEquals("Should have returned two organisations (1x client + 1x suppliers).", 2,
				nestedFacts.OfType<IOrganisationFact>().Count());

			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == data.Org1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);

			var supplierFact1 = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == supplier.PK);
			AssertEquals(nameof(IOrganisationFact.Code), supplier.OH_Code, supplierFact1.Code);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().First();
			AssertEquals(nameof(IInventoryFact.Supplier), supplier.PK, inventoryFact1.Supplier.Fact.PK);

			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Last();
			AssertEquals(nameof(IInventoryFact.Supplier), supplier.PK, inventoryFact2.Supplier.Fact.PK);
		}

		public void TestPutaway_GetFacts_DifferentReceive_MultipleConsignees()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var consignee1 = Helper.CreateClient("CN1");
			var consignee2 = Helper.CreateClient("CN2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			receive1.Lines[0].ConsigneeDocAddress.OrganisationPK = consignee1.PK;
			receive2.Lines[0].ConsigneeDocAddress.OrganisationPK = consignee2.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();
			palletManagerMock
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive1.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					data.Whs1))
				.Returns(new[] { receive2.Inventory[0] });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive1,
				new[] { receive1.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			AssertEquals("Should have returned two inventories.", 2, resultingFacts.OfType<IInventoryFact>().Count());
			AssertEquals("Should have returned three organisations (1x client + 2x consignees).", 3,
				nestedFacts.OfType<IOrganisationFact>().Count());

			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == data.Org1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);

			var consigneeFact1 = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == consignee1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), consignee1.OH_Code, consigneeFact1.Code);

			var consigneeFact2 = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == consignee2.PK);
			AssertEquals(nameof(IOrganisationFact.Code), consignee2.OH_Code, consigneeFact2.Code);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().First();
			AssertEquals(nameof(IInventoryFact.Consignee), consignee1.PK, inventoryFact1.Consignee.Fact.PK);

			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Last();
			AssertEquals(nameof(IInventoryFact.Consignee), consignee2.PK, inventoryFact2.Consignee.Fact.PK);
		}

		public void TestPutaway_GetFacts_DifferentReceive_SameConsignee()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var consignee = Helper.CreateClient("CN1");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m,
				allocateLocations: false, finalise: false);
			receive1.Lines[0].ConsigneeDocAddress.OrganisationPK = consignee.PK;
			receive2.Lines[0].ConsigneeDocAddress.OrganisationPK = consignee.PK;

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();
			palletManagerMock
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive1.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					data.Whs1)).Returns(new[] { receive2.Inventory[0] });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive1,
				new[] { receive1.Lines[0] });

			var resultingFacts = getFacts();
			var nestedFacts = resultingFacts.GetNestedFacts();
			AssertEquals("Should have returned two inventories.", 2, resultingFacts.OfType<IInventoryFact>().Count());
			AssertEquals("Should have returned three organisations (1x client + 1x consignee).", 2,
				nestedFacts.OfType<IOrganisationFact>().Count());

			var clientFact = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == data.Org1.PK);
			AssertEquals(nameof(IOrganisationFact.Code), data.Org1.OH_Code, clientFact.Code);

			var consigneeFact1 = nestedFacts.OfType<IOrganisationFact>().Single(o => o.PK == consignee.PK);
			AssertEquals(nameof(IOrganisationFact.Code), consignee.OH_Code, consigneeFact1.Code);

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().First();
			AssertEquals(nameof(IInventoryFact.Consignee), consignee.PK, inventoryFact1.Consignee.Fact.PK);

			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Last();
			AssertEquals(nameof(IInventoryFact.Consignee), consignee.PK, inventoryFact2.Consignee.Fact.PK);
		}

		public void TestPutaway_GetFacts_DifferentReceive_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Core.Constants.PkgUnit.Pallet, 5m);
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, (ZString)Core.Constants.PkgUnit.Pallet)
				.F3_UOMType = UOMPackTypesList.Codes.Pallet;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			data.Part1.OP_Weight = 2m;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Pounds;
			data.Part1.OP_Cubic = 4m;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicDecimetres;

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			receive1.WD_ExternalReference = "RR1";
			receive1.WD_CustomerReference = "CR1";
			receive1.WD_RS_NKServiceLevel = "TST";
			receive1.WD_ArrivalDate = ZDateTime.BrettsBirthday.ToOffset();

			var inventoryLine1 = receive1.Lines[0];
			inventoryLine1.WE_PalletID = "PLT-123";
			inventoryLine1.WE_RequiredByDate = ZDateTime.BrettsBirthday.AddDays(10).ToOffset();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_ExternalReference = "RR2";
			receive2.WD_CustomerReference = "CR2";
			receive2.WD_RS_NKServiceLevel = "TS2";
			receive2.WD_ArrivalDate = ZDateTime.BrettsBirthday.ToOffset().AddDays(20);

			var consignee = Helper.CreateClient("CNE");
			var inventory2 =
				Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, dockDoorLocation1, "PLT-123", 5m);
			var inventory2DocketLine = (WhsReceiveLine)inventory2.InDocketLine;
			inventory2DocketLine.WE_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			inventory2DocketLine.WE_ClientOrderedUnits = 3m;
			inventory2DocketLine.WE_TransactionQuantity = 5m;
			inventory2DocketLine.WE_PartAttrib1 = "1";
			inventory2DocketLine.WE_PartAttrib2 = "2";
			inventory2DocketLine.WE_PartAttrib3 = "3";
			inventory2DocketLine.WE_ExpiryDate = ZDate.BrettsBirthday.AddDays(20);
			inventory2DocketLine.WE_PackingDate = ZDate.BrettsBirthday.AddDays(-10);
			inventory2DocketLine.WE_RequiredByDate = ZDateTime.BrettsBirthday.AddDays(30).ToOffset();
			inventory2DocketLine.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, null, "PLT-123",
					5m);
			transferLine1.WE_PartAttrib1 = "1";
			transferLine1.WE_PartAttrib2 = "2";
			transferLine1.WE_PartAttrib3 = "3";
			transferLine1.WE_ExpiryDate = ZDate.BrettsBirthday.AddDays(20);
			transferLine1.WE_PackingDate = ZDate.BrettsBirthday.AddDays(-10);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();
			palletManagerMock
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive1.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					data.Whs1))
				.Returns(new[] { transferLine1.Inventory[0] });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive1,
				new[] { receive1.Lines[0] });

			var resultingFacts = getFacts();
			AssertEquals("Should have returned two inventories.", 2, resultingFacts.OfType<IInventoryFact>().Count());

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().First();
			AssertEquals(nameof(IInventoryFact.ServiceLevel), "TST", inventoryFact1.ServiceLevel);
			AssertEquals(nameof(IInventoryFact.ReceiveReference), "RR1", inventoryFact1.ReceiveReference);
			AssertEquals(nameof(IInventoryFact.CustomerReference), "CR1", inventoryFact1.CustomerReference);
			AssertEquals(nameof(IInventoryFact.ArrivalDate), ZDateTime.BrettsBirthday, inventoryFact1.ArrivalDate);
			AssertEquals(nameof(IInventoryFact.RequiredDate), ZDateTime.BrettsBirthday.AddDays(10),
				inventoryFact1.RequiredDate);

			var inventoryFact2 = resultingFacts.OfType<IInventoryFact>().Last();
			AssertEquals(nameof(IInventoryFact.ServiceLevel), "TS2", inventoryFact2.ServiceLevel);
			AssertEquals(nameof(IInventoryFact.ReceiveReference), "RR2", inventoryFact2.ReceiveReference);
			AssertEquals(nameof(IInventoryFact.CustomerReference), "CR2", inventoryFact2.CustomerReference);
			AssertEquals(nameof(IInventoryFact.ArrivalDate), ZDateTime.BrettsBirthday.AddDays(20),
				inventoryFact2.ArrivalDate);
			AssertEquals(nameof(IInventoryFact.RequiredDate), ZDateTime.BrettsBirthday.AddDays(30),
				inventoryFact2.RequiredDate);

			AssertEquals(nameof(IInventoryFact.Consignee), consignee.PK, inventoryFact2.Consignee.Fact.PK);
			AssertEquals(nameof(IInventoryFact.Quantity), 5m, inventoryFact2.Quantity);
			AssertEquals(nameof(IInventoryFact.PackUnits), 1m, inventoryFact2.PackUnits);
			AssertEquals(nameof(IInventoryFact.PackUQ), Core.Constants.PkgUnit.Pallet, inventoryFact2.PackUQ);
			AssertEquals(nameof(IInventoryFact.PartAttribute1), "1", inventoryFact2.PartAttribute1);
			AssertEquals(nameof(IInventoryFact.PartAttribute2), "2", inventoryFact2.PartAttribute2);
			AssertEquals(nameof(IInventoryFact.PartAttribute3), "3", inventoryFact2.PartAttribute3);
			AssertEquals(nameof(IInventoryFact.ExpiryDate), ZDate.BrettsBirthday.AddDays(20),
				inventoryFact2.ExpiryDate);
			AssertEquals(nameof(IInventoryFact.PackingDate), ZDateTime.BrettsBirthday.AddDays(-10),
				inventoryFact2.PackingDate);
			AssertEquals(nameof(IInventoryFact.ProductWeight), 2m, inventoryFact2.ProductWeight);
			AssertEquals(nameof(IInventoryFact.ProductWeightUQ), Core.Constants.Weight.Pounds,
				inventoryFact2.ProductWeightUQ);
			AssertEquals(nameof(IInventoryFact.ProductVolume), 4m, inventoryFact2.ProductVolume);
			AssertEquals(nameof(IInventoryFact.ProductVolumeUQ), Core.Constants.Volume.CubicDecimetres,
				inventoryFact2.ProductVolumeUQ);
		}

		public void TestPutaway_GetFacts_DifferentReceive_RegularTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, null,
				"PLT-123", allocateLocations: false, finalise: false);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m,
				receive2.Lines[0].Location.WLV_LocationString, "", "", "PLT-123");
			transfer.RunPreSaveValidation();
			Factory.Save();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: Has stock on hand.", 5m, transferLine.WE_StockOnHand);

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var locationFactLoader = Mock.Of<IPutawayLocationFactLoader>();
			var palletManagerMock = new Mock<IPutawayExistingPalletManager>();
			palletManagerMock
				.Setup(pm => pm.AllocateExistingPalletLocations(
					Factory,
					It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive1.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					data.Whs1))
				.Returns(new[] { transferLine.Inventory[0] });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock.Object,
				locationFactLoader,
				receive1,
				new[] { receive1.Lines[0] });

			var resultingFacts = getFacts();
			AssertEquals("Should have returned a single inventory.", 1,
				resultingFacts.OfType<IInventoryFact>().Count());

			var inventoryFact1 = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals(nameof(IInventoryFact.PK), receive1.Lines[0].PK, inventoryFact1.PK);
		}

		public void TestPutaway_GetFacts_HasPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, "PLT1");
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFact = Mock.Of<IPutawayLocationFact>();
			var locationFactLoader = new Mock<IPutawayLocationFactLoader>(MockBehavior.Strict);
			locationFactLoader
				.Setup(lfl => lfl.GetPutawayLocationFacts(Factory, data.Whs1.PK, new[] { data.Org1.PK },
					It.Is<IEnumerable<ZGuid>>(invs => invs.Single() == data.Part1.PK), It.IsAny<IEnumerable<ZGuid>>()))
				.Returns(new[] { locationFact });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader.Object,
				receive,
				new[] { line1 });

			var resultingFacts = getFacts();
			AssertEquals("Should have only returned one location.", locationFact,
				resultingFacts.OfType<IPutawayLocationFact>().Single());

			var inventoryMock = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals("HasPutawayTransfer should be false.", false, inventoryMock.HasPutawayTransfer);
		}

		public void TestPutaway_GetFacts_HasPutawayTransfer_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 1m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, null, "PLT1", 1m);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			var crossDockManagerMock = Mock.Of<ICrossDockManager>();
			var palletManagerMock = Mock.Of<IPutawayExistingPalletManager>();
			var locationFact = Mock.Of<IPutawayLocationFact>();
			var locationFactLoader = new Mock<IPutawayLocationFactLoader>(MockBehavior.Strict);
			locationFactLoader
				.Setup(lfl => lfl.GetPutawayLocationFacts(Factory, data.Whs1.PK, new[] { data.Org1.PK },
					It.Is<IEnumerable<ZGuid>>(invs => invs.Single() == data.Part1.PK), It.IsAny<IEnumerable<ZGuid>>()))
				.Returns(new[] { locationFact });

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (getFacts, _) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				crossDockManagerMock,
				palletManagerMock,
				locationFactLoader.Object,
				receive,
				new[] { (WhsReceiveLine)line1.InDocketLine });

			var resultingFacts = getFacts();
			AssertEquals("Should have only returned one location.", locationFact,
				resultingFacts.OfType<IPutawayLocationFact>().Single());

			var inventoryMock = resultingFacts.OfType<IInventoryFact>().Single();
			AssertEquals("HasPutawayTransfer should be true.", true, inventoryMock.HasPutawayTransfer);
		}

		#endregion

		#region TestPutaway_ProcessResults

		public void TestPutaway_ProcessResults_NotAllInventoryPassedIn_NoWarning()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var inventory = new[] { inventory1 };

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				receive,
				inventory);

			var putawayResult = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(inventory1.PK.ToGuid(), location.PK.ToGuid(), 10m, "")
			});
			AssertNull(processResults(putawayResult));
			AssertEquals("Should have putaway inventory.", location.PK, inventory1.WE_WL);
			AssertEquals("Should *not* have putaway inventory.", ZGuid.Empty, inventory2.WE_WL);
		}

		public void TestPutaway_ProcessResults_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 =
				Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT1", 15m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, null, "PLT1", 15m);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				receive,
				new[] { (WhsReceiveLine)inventory1.InDocketLine });

			var putawayResult = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(inventory1.PK.ToGuid(), nonDockDoorLocation.PK.ToGuid(), 15m, "")
			});
			AssertNull(processResults(putawayResult));
			AssertEquals("Should *not* have split inventory.", 15m, transferLine1.WE_StockOnHand);
			AssertEquals("Should *not* have changed location on inventory.", dockDoorLocation1.PK, inventory1.WI_WL);
			AssertEquals("Should have putaway inventory.", nonDockDoorLocation.PK, transferLine1.WE_WL);
		}

		public void TestPutaway_ProcessResults_PutsOntoPallet_NoPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory = receive.Lines[0];

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				receive,
				new[] { inventory });

			var putawayResult = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(inventory.PK.ToGuid(), location.PK.ToGuid(), 5m, "PLT-123")
			});
			AssertNull(processResults(putawayResult));
			AssertEquals("Should *not* have split inventory.", 1, receive.Inventory.Count);
			AssertEquals("Should *not* have split inventory.", 5m, inventory.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location.PK, inventory.WE_WL);
			AssertEquals("Should have set pallet.", "PLT-123", inventory.WE_PalletID);
		}

		public void TestPutaway_ProcessResults_PutsOntoPallet_WithPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = (WhsReceiveLine)Helper
				.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT1", 15m).InDocketLine;
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, null, "PLT1", 15m);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				receive,
				new[] { inventory });

			var putawayResult = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(inventory.PK.ToGuid(), nonDockDoorLocation.PK.ToGuid(), 15m, "PLT-123")
			});
			AssertNull(processResults(putawayResult));
			AssertEquals("Should *not* have split inventory.", 15m, transferLine.WE_StockOnHand);
			AssertEquals("Should *not* have changed location on inventory.", dockDoorLocation.PK, inventory.WE_WL);
			AssertEquals("Should have putaway inventory.", nonDockDoorLocation.PK, transferLine.WE_WL);
			AssertEquals("Should not have updated pallet id on inventory.", "PLT1", inventory.WE_PalletID);
			AssertEquals("Should have set pallet on putaway transfer line.", "PLT-123", transferLine.WE_PalletID);
		}

		public void TestPutaway_ProcessResults_DifferentReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive1.Lines[0];

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory2 = receive2.Lines[0];

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				receive1,
				new[] { inventory1 });

			var resultFact1 = new PutawayResultFact(inventory1.PK.ToGuid(), location.PK.ToGuid(), 5m, "");
			var resultFact2 = new PutawayResultFact(inventory2.PK.ToGuid(), location.PK.ToGuid(), 5m, "");

			var putawayResult = new ProductionRulesEngineResult(new[] { resultFact1, resultFact2 });
			AssertNull(processResults(putawayResult));
			AssertEquals("Should *not* have split inventory.", 1, receive1.Inventory.Count);
			AssertEquals("Should *not* have split inventory.", 5m, inventory1.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location.PK, inventory1.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory1.WE_PalletID);

			AssertEquals("Should *not* have putaway the inventory on the other receive.", ZGuid.Empty,
				inventory2.WE_WL);
		}

		public void TestPutaway_ProcessResults_DifferentReceive_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive1.Lines[0];

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory2 =
				Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, dockDoorLocation1, "PLT1", 15m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, null, "PLT1", 15m);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				receive1,
				new[] { inventory1 });

			var resultFact1 = new PutawayResultFact(inventory1.PK.ToGuid(), nonDockDoorLocation.PK.ToGuid(), 5m, "");
			var resultFact2 = new PutawayResultFact(inventory2.PK.ToGuid(), nonDockDoorLocation.PK.ToGuid(), 5m, "");

			var putawayResult = new ProductionRulesEngineResult(new[] { resultFact1, resultFact2 });
			AssertNull(processResults(putawayResult));
			AssertEquals("Should *not* have split inventory.", 1, receive1.Inventory.Count);
			AssertEquals("Should *not* have split inventory.", 5m, inventory1.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", nonDockDoorLocation.PK, inventory1.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory1.WE_PalletID);

			AssertEquals("Should *not* have split inventory on the other receive.", 1, receive2.Inventory.Count);
			AssertEquals("Should *not* have split inventory on the other receive.", 15m, transferLine1.WE_StockOnHand);
			AssertEquals("Should *not* have putaway the inventory on the other receive.", ZGuid.Empty,
				transferLine1.WE_WL);
		}

		public void TestPutaway_ProcessResults_DefersUpdateTotalPalletsReceived()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var inventory = new[] { inventory1, inventory2 };

			var refreshBindingHitCount = 0;
			receive.TotalPalletsReceivedInfo.ValueChanged += (s, e) => refreshBindingHitCount++;
			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				receive,
				inventory);

			var putawayResult = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(inventory1.PK.ToGuid(), location.PK.ToGuid(), 10m, "P1"),
				new PutawayResultFact(inventory2.PK.ToGuid(), location.PK.ToGuid(), 10m, "P2")
			});
			AssertNull(processResults(putawayResult));
			AssertEquals("Should have putaway inventory.", location.PK, inventory1.WE_WL);
			AssertEquals("Should have putaway inventory.", location.PK, inventory2.WE_WL);
			AssertEquals("Should have putaway inventory.", "P1", inventory1.WE_PalletID);
			AssertEquals("Should have putaway inventory.", "P2", inventory2.WE_PalletID);
			AssertEquals("UpdateTotalPalletsReceived should only have been called once at the end.", 1,
				refreshBindingHitCount);
		}

		public void TestPutaway_ProcessResults_UseLocationConcurrencyHandling()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location = data.Whs1.FindLocation("A-2");
			var locationChangeID = ZGuid.NewZGuid();
			location.WLV_LastAllocatedOrChangedID = locationChangeID;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory = new[] { inventory1 };

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				receive,
				inventory,
				useLocationConcurrencyHandling: true);

			var putawayResult = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(inventory1.PK.ToGuid(), location.PK.ToGuid(), 10m, "P1", locationChangeID.ToGuid())
			});
			AssertNoExceptionThrown(() => processResults(putawayResult));

			var putwayResultWithDifferentChangeID = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(inventory1.PK.ToGuid(), location.PK.ToGuid(), 10m, "P1", Guid.NewGuid())
			});
			AssertExceptionThrown<PutawayAllocateLocationConcurrencyException>("Exception thrown when location change ID changed.", () => processResults(putwayResultWithDifferentChangeID));

			AssertEquals("Concurrency policy should be strict after process result.", ConcurrencyPolicy.Strict, location.WLV_LastAllocatedOrChangedIDInfo.ConcurrencyPolicy);
			AssertEquals("Concurrency policy should not change without process result", ConcurrencyPolicy.Ignore, location1.WLV_LastAllocatedOrChangedIDInfo.ConcurrencyPolicy);
		}

		public void TestPutaway_ProcessResults_DoNotUseLocationConcurrencyHandling()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");
			var locationChangeID = ZGuid.NewZGuid();
			location.WLV_LastAllocatedOrChangedID = locationChangeID;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory = new[] { inventory1 };

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var (_, processResults) = CallRulesEngineMockAndReturnInputOutputFunctions(
				engineMock,
				Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(),
				Mock.Of<IPutawayLocationFactLoader>(),
				receive,
				inventory,
				useLocationConcurrencyHandling: false);

			var putawayResult = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(inventory1.PK.ToGuid(), location.PK.ToGuid(), 10m, "P1", locationChangeID.ToGuid())
			});
			AssertNoExceptionThrown(() => processResults(putawayResult));

			var putwayResultWithDifferentChangeID = new ProductionRulesEngineResult(new[]
			{
				new PutawayResultFact(inventory1.PK.ToGuid(), location.PK.ToGuid(), 10m, "P1", Guid.NewGuid())
			});
			AssertNoExceptionThrown(() => processResults(putwayResultWithDifferentChangeID));
		}

		#endregion

		#region TestPutaway_EndToEnd

		[GuiTest]
		public void TestPutaway_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule1 = GetRule(ruleSet, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			var rule2 = GetRule(ruleSet, 2,
				data.Part2.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 2);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			Factory.Save();

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
			putawayManager.Putaway(new[] { receive }, new[] { inventory1, inventory2 }, receive.NotificationSubscriber);
			AssertEquals("Should have shown no errors.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals("Should *not* have split inventory.", 2, receive.Inventory.Count);
			AssertEquals("Should *not* have split inventory.", 5m, inventory1.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location1.PK, inventory1.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory1.WE_PalletID);

			AssertEquals("Should *not* have split inventory.", 5m, inventory2.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location2.PK, inventory2.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory2.WE_PalletID);
		}

		[GuiTest]
		public void TestPutaway_EndToEnd_PalletSpaces()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PalletFloorSpaces = 1;
			location1.WLV_PalletStackHeight = 1;
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_PalletFloorSpaces = 2;
			location2.WLV_PalletStackHeight = 1;
			var location3 = data.Whs1.FindLocation("A-3");
			location3.WLV_PalletFloorSpaces = 3;
			location3.WLV_PalletStackHeight = 1;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 5m, location1, "PLT001");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 5m, location2, "PLT002");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 5m, location3, "PLT003");
			receive1.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK, location3.PK });

			var cache1 = WhsPutawayLocationCacheTestHelper.GetAllCacheRecords(Factory, data.Whs1.PK).Where(c =>
				location1.PK.Equals(((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]))).ToArray();
			var cache2 = WhsPutawayLocationCacheTestHelper.GetAllCacheRecords(Factory, data.Whs1.PK).Where(c =>
				location2.PK.Equals(((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]))).ToArray();
			var cache3 = WhsPutawayLocationCacheTestHelper.GetAllCacheRecords(Factory, data.Whs1.PK).Where(c =>
				location3.PK.Equals(((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]))).ToArray();

			AssertEquals((short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_PalletSpaces.Name]);
			AssertEquals((short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_PalletQuantity.Name]);
			AssertEquals((short)2, cache2[0][WhsPutawayLocationCacheSchema.WPC_PalletSpaces.Name]);
			AssertEquals((short)1, cache2[0][WhsPutawayLocationCacheSchema.WPC_PalletQuantity.Name]);
			AssertEquals((short)3, cache3[0][WhsPutawayLocationCacheSchema.WPC_PalletSpaces.Name]);
			AssertEquals((short)1, cache3[0][WhsPutawayLocationCacheSchema.WPC_PalletQuantity.Name]);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, allocateLocations: false, finalise: false);
			var receiveLine1 = receive2.Lines[0];
			receiveLine1.WE_PalletID = "PLT004";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 5m);
			receiveLine2.WE_PalletID = "PLT004";
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 5m);
			receiveLine3.WE_PalletID = "PLT005";
			Factory.Save();

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
			putawayManager.Putaway(new[] { receive2 }, new[] { receiveLine1, receiveLine2, receiveLine3 }, receive2.NotificationSubscriber);

			AssertEquals("Should have shown no errors.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals("Should *not* have split inventory.", 3, receive2.Inventory.Count);
			AssertEquals("Should have putaway inventory.", location2.PK, receiveLine1.WE_WL);
			AssertEquals("Should have set pallet.", "PLT004", receiveLine1.WE_PalletID);
			AssertEquals("Should have putaway inventory.", location2.PK, receiveLine2.WE_WL);
			AssertEquals("Should have set pallet.", "PLT004", receiveLine2.WE_PalletID);
			AssertEquals("Should have putaway inventory.", location3.PK, receiveLine3.WE_WL);
			AssertEquals("Should have set pallet.", "PLT005", receiveLine3.WE_PalletID);
		}

		[GuiTest]
		public void TestPutaway_EndToEnd_MultipleReceives()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule1 = GetRule(ruleSet, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			var rule2 = GetRule(ruleSet, 2,
				data.Part2.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 2);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive1.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive1, data.Part2, 5m);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory3 = receive2.Lines[0];
			var inventory4 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 5m);

			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory5 = receive3.Lines[0];
			var inventory6 = Helper.CreateWhsReceiveLine(receive3, data.Part2, 5m);

			Factory.Save();

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
			putawayManager.Putaway(new[] { receive1, receive2, receive3 }, new[] { inventory1, inventory2, inventory3, inventory4, inventory5, inventory6 }, receive1.NotificationSubscriber);
			AssertEquals("Should have shown no errors.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals("Should have putaway inventory.", location1.PK, inventory1.WE_WL);
			AssertEquals("Should have putaway inventory.", location2.PK, inventory2.WE_WL);
			AssertEquals("Should have putaway inventory.", location1.PK, inventory3.WE_WL);
			AssertEquals("Should have putaway inventory.", location2.PK, inventory4.WE_WL);
			AssertEquals("Should have putaway inventory.", location1.PK, inventory5.WE_WL);
			AssertEquals("Should have putaway inventory.", location2.PK, inventory6.WE_WL);
		}

		[GuiTest]
		public void TestPutaway_EndToEnd_MultipleReceivesWithDifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("Wh2");

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule1 = GetRule(ruleSet, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			var rule2 = GetRule(ruleSet, 2,
				data.Part2.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 2);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive1.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive1, data.Part2, 5m);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory3 = receive2.Lines[0];
			var inventory4 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 5m);

			Factory.Save();

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
			AssertExceptionThrown(
				typeof(InvalidOperationException),
				() => putawayManager.Putaway(new[] { receive1, receive2 }, new[] { inventory1, inventory2, inventory3, inventory4 }, receive1.NotificationSubscriber));
		}

		[GuiTest]
		public void TestPutaway_EndToEnd_MultipleReceivesWithDifferentFactories()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule1 = GetRule(ruleSet, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			var rule2 = GetRule(ruleSet, 2,
				data.Part2.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 2);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive1.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive1, data.Part2, 5m);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory3 = receive2.Lines[0];
			var inventory4 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 5m);

			Factory.Save();

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();

			var otherFactory = new BusinessObjectFactory();
			var receive2InOtherFactory = otherFactory.Load<WhsReceive>(receive2.PK);
			AssertExceptionThrown(
				typeof(InvalidOperationException),
				() => putawayManager.Putaway(new[] { receive1, receive2InOtherFactory }, new[] { inventory1, inventory2, inventory3, inventory4 }, receive1.NotificationSubscriber));
		}

		[GuiTest]
		public void TestPutaway_Warehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet1 = Factory.New<ProductionRuleSet>();
			ruleSet1.PRS_Name = "Rules 1";
			ruleSet1.PRS_Description = "Rules 1";
			ruleSet1.PRS_Context = "PWP";
			ruleSet1.PRS_IsLive = true;
			ruleSet1.PRS_IsSystem = false;

			var ruleSet2 = Factory.New<ProductionRuleSet>();
			ruleSet2.PRS_Name = "Rules 2";
			ruleSet2.PRS_Description = "Rules 2";
			ruleSet2.PRS_Context = "PWP";
			ruleSet2.PRS_WW_Warehouse = data.Whs1.PK;
			ruleSet2.PRS_IsLive = true;
			ruleSet2.PRS_IsSystem = false;

			var rule1_1 = GetRule(ruleSet1, 1,
				data.Part2.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			var rule1_2 = GetRule(ruleSet1, 2,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 2);

			var rule2_1 = GetRule(ruleSet2, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			var rule2_2 = GetRule(ruleSet2, 2,
				data.Part2.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 2);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			Factory.Save();

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
			putawayManager.Putaway(new[] { receive }, new[] { inventory1, inventory2 }, receive.NotificationSubscriber);
			AssertEquals("Should have shown no errors.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals("Should *not* have split inventory.", 2, receive.Inventory.Count);
			AssertEquals("Should *not* have split inventory.", 5m, inventory1.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location1.PK, inventory1.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory1.WE_PalletID);

			AssertEquals("Should *not* have split inventory.", 5m, inventory2.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location2.PK, inventory2.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory2.WE_PalletID);
		}

		[GuiTest]
		public void TestPutaway_EndToEnd_Date()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var relation1PK = data.Part1.RelatedOrganisations
				.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK;
			var relation2PK = data.Part2.RelatedOrganisations
				.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK;
			var rule1 = GetRule(ruleSet, 1, relation1PK, 1);
			var rule2 = GetRule(ruleSet, 2, relation2PK, 2);

			rule1.PRL_RuleDefinition = $@"{{
    ""conditions"": [
        {{
            ""fieldPath"": ""ArrivalDate.Day"",
            ""operation"": ""notEquals"",
            ""value"": ""32""
        }},
        {{
            ""fieldPath"": ""Product"",
            ""operation"": ""equals"",
            ""value"": ""{relation1PK}""
        }}
    ],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": 1
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			rule2.PRL_RuleDefinition = $@"{{
    ""conditions"": [
        {{
            ""fieldPath"": ""ArrivalDate.Day"",
            ""operation"": ""notEquals"",
            ""value"": ""32""
        }},
        {{
            ""fieldPath"": ""Product"",
            ""operation"": ""equals"",
            ""value"": ""{relation2PK}""
        }}
    ],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": 2
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			Factory.Save();

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
			putawayManager.Putaway(new[] { receive }, new[] { inventory1, inventory2 }, receive.NotificationSubscriber);
			AssertEquals("Should have shown no errors.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals("Should *not* have split inventory.", 2, receive.Inventory.Count);
			AssertEquals("Should *not* have split inventory.", 5m, inventory1.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location1.PK, inventory1.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory1.WE_PalletID);

			AssertEquals("Should *not* have split inventory.", 5m, inventory2.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location2.PK, inventory2.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory2.WE_PalletID);
		}

		[GuiTest]
		public void TestPutaway_EndToEnd_Priority()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var owner = data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
				OrgPartRelation.RelationshipTypes.Owner);
			var rule1 = GetRule(ruleSet, 1, owner.PK, column: 1); // Set location to A-1
			var rule2 = GetRule(ruleSet, 2, owner.PK, column: 2); // Set location to A-2

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			Factory.Save();

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
			putawayManager.Putaway(new[] { receive }, new[] { inventory1 }, receive.NotificationSubscriber);
			AssertEquals("Should have shown no errors.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals("Should *not* have split inventory.", 1, receive.Inventory.Count);
			AssertEquals("Should *not* have split inventory.", 5m, inventory1.WE_StockOnHand);
			AssertEquals("Should have putaway inventory using priority 1 rule.", location1.PK, inventory1.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory1.WE_PalletID);
		}

		[GuiTest]
		public void TestPutaway_EndToEnd_WithDangerousGoods()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateUNDGDataItem(data.Part1, "0073a", "1.1D");

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule1 = GetRule(ruleSet, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			rule1.PRL_RuleDefinition = $@"{{
    ""conditions"": [
        {{
            ""fieldPath"": ""Product.FirstDG.ClassCode"",
            ""operation"": ""equals"",
            ""value"": ""1.1D""
        }}
    ],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": 1
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			var rule2 = GetRule(ruleSet, 2,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			rule2.PRL_RuleDefinition = $@"{{
		""conditions"":[],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": 2
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			Factory.Save();

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
			putawayManager.Putaway(new[] { receive }, new[] { inventory1, inventory2 }, receive.NotificationSubscriber);
			AssertEquals("Should have shown no errors.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals("Should *not* have split inventory.", 2, receive.Inventory.Count);
			AssertEquals("Should *not* have split inventory.", 5m, inventory1.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location1.PK, inventory1.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory1.WE_PalletID);

			AssertEquals("Should *not* have split inventory.", 5m, inventory2.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location2.PK, inventory2.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory2.WE_PalletID);
		}

		[GuiTest]
		public void TestPutaway_EndToEnd_WithMultipleDangerousGoods()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateUNDGDataItem(data.Part1, "0073a", "1.1D");
			Helper.CreateUNDGDataItem(data.Part1, "0074a", "1.2D");
			Helper.CreateUNDGDataItem(data.Part2, "0073a", "1.1D");

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule1 = GetRule(ruleSet, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			rule1.PRL_RuleDefinition = $@"{{
    ""conditions"": [
        {{
            ""fieldPath"": ""Product.HasMultipleDangerousGoods"",
            ""operation"": ""equals"",
            ""value"": true
        }}
    ],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": 1
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			var rule2 = GetRule(ruleSet, 2,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			rule2.PRL_RuleDefinition = $@"{{
		""conditions"":[],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
            {{
                ""fieldPath"": ""Column"",
                ""operation"": ""equals"",
                ""value"": 2
            }}
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m);
			Factory.Save();

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
			putawayManager.Putaway(new[] { receive }, new[] { inventory1, inventory2 }, receive.NotificationSubscriber);
			AssertEquals("Should have shown no errors.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals("Should *not* have split inventory.", 2, receive.Inventory.Count);
			AssertEquals("Should *not* have split inventory.", 5m, inventory1.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location1.PK, inventory1.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory1.WE_PalletID);

			AssertEquals("Should *not* have split inventory.", 5m, inventory2.WE_StockOnHand);
			AssertEquals("Should have putaway inventory.", location2.PK, inventory2.WE_WL);
			AssertEquals("Should *not* have set pallet.", string.Empty, inventory2.WE_PalletID);
		}

		#endregion

		#region TestPutaway_DbHits

		#region TestPutaway_DbHits

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits()
		{
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct("P" + i, client);
				products.Add(product);

				if (i % 2 == 0)
				{
					var param = Helper.CreateProductParamsByWhsAndClient(product, client, whs);
					param.W3_WPG_PutawayGroup = Helper.CreatePutawayGroup("P" + i, "P" + i).PK;
				}
			}

			Factory.Save();

			var locations = row.Locations.Select(l => l.PK).ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations);

			var putawayInstructions = new List<PutawayResultFact>();
			var receive = Helper.CreateWhsReceive(client, whs);
			for (var i = 0; i < 1000; i++)
			{
				var line = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), locations[i].ToGuid(), 1m, ""));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 16 }, // 1000/62 = 16.1, max query size results in split query
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsPickLineSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 },
				{ WhsPutawayGroupSchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					receiveInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#region TestPutaway_DbHits_PutawayTransfer

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var transfers = new List<WhsTransfer>(20);
			for (var i = 0; i < 20; i++)
			{
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, $"TR{i}", Notify);
				transfer.WD_IsPutawayTransfer = true;

				transfers.Add(transfer);
			}

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (var i = 0; i < 100; i++)
			{
				var line = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 1m, nonDockDoorLocation, $"PLT{i % 10}");
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			for (var i = 0; i < 100; i++)
			{
				var line = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1,
					$"PLT{i % 10}", 1m);
			}

			var putawayInstructions = new List<PutawayResultFact>();
			for (var i = 100; i < 200; i++)
			{
				var line = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1,
					$"PLT{10 + (i % 10)}", 1m);
				putawayInstructions.Add(
					new PutawayResultFact(line.PK.ToGuid(), nonDockDoorLocation.PK.ToGuid(), 1m, ""));
			}

			Factory.Save();

			for (var i = 0; i < 200; i++)
			{
				var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(transfers[i % 10], data.Part1,
					dockDoorLocation1, null, $"PLT{i % 20}", 1m);
				transferLine1.PickedTime = ZDateTimeOffset.Now;
			}

			transfers.ForEach(t => t.RunPreSaveValidation());
			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 15 }, // max query size results in split query
				{ WhsInventoryViewSchema.Constants.TableName, 8 }, // max query size results in split query
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 8 }, // max query size results in split query
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					receiveInOtherFactory.Lines.Cast<ILineToPutaway>().All(i => i.LocationPK.IsValid));
			}
		}

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_PutawayTransfer_Pallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var transfers = new List<WhsTransfer>(20);
			for (var i = 0; i < 20; i++)
			{
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, $"TR{i}", Notify);
				transfer.WD_IsPutawayTransfer = true;

				transfers.Add(transfer);
			}

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (var i = 0; i < 100; i++)
			{
				var line = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 1m, nonDockDoorLocation, $"PLT{i % 10}");
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			for (var i = 0; i < 100; i++)
			{
				var line = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1,
					$"PLT{i % 10}", 1m);
			}

			var putawayInstructions = new List<PutawayResultFact>();
			for (var i = 100; i < 200; i++)
			{
				var line = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1,
					$"PLT{10 + (i % 10)}", 1m);
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), nonDockDoorLocation.PK.ToGuid(), 1m,
					"PLTABC"));
			}

			Factory.Save();

			for (var i = 0; i < 200; i++)
			{
				var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(transfers[i % 10], data.Part1,
					dockDoorLocation1, null, $"PLT{i % 20}", 1m);
				transferLine1.PickedTime = ZDateTimeOffset.Now;
			}

			transfers.ForEach(t => t.RunPreSaveValidation());
			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 15 }, // max query size results in split query
				{ WhsInventoryViewSchema.Constants.TableName, 8 }, // max query size results in split query
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 8 }, // max query size results in split query
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					receiveInOtherFactory.Lines.Cast<ILineToPutaway>().All(i => i.LocationPK.IsValid));
			}
		}

		#endregion

		#region TestPutaway_DbHits_Split

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_Split()
		{
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);
			Factory.Save();

			var locations = row.Locations.Select(l => l.PK).ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations);

			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", Helper.CreateProduct("P1", client),
				1000m, allocateLocations: false, finalise: false);
			var line = receive.Lines[0];

			var putawayInstructions = new List<PutawayResultFact>();
			for (var i = 0; i < 1000; i++)
			{
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), locations[i].ToGuid(), 1m, ""));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", 1000, receiveInOtherFactory.Inventory.Count);
				AssertEquals("Should have putaway all inventory.", true,
					receiveInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#region TestPutaway_DbHits_Pallets

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_Pallets()
		{
			// 1k inventory to putaway across 100 pallets, 25 on another receive, 25 already putaway on this receive, 50 to go to engine
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				products.Add(Helper.CreateProduct("P" + i, client));
			}

			Factory.Save();

			var locations = row.Locations.ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations.Select(l => l.PK));

			var putawayInstructions = new List<PutawayResultFact>();
			var receive = Helper.CreateWhsReceive(client, whs, "R2");

			// Some pallets on this receive
			for (var i = 0; i < 250; i++)
			{
				var line1 = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				var line2 = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m, locations[i]);
				line1.WE_PalletID = $"PLT-{i % 10}";
				line2.WE_PalletID = $"PLT-{i % 10}";
			}

			// Some pallets on another receive
			var oldReceive = Helper.CreateWhsReceive(client, whs, "R1");
			for (var i = 250; i < 500; i++)
			{
				var line1 = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				var line2 = Helper.CreateWhsReceiveLine(oldReceive, products[i % products.Count], 1m, locations[i]);
				line1.WE_PalletID = $"PLT-{25 + (i % 10)}";
				line2.WE_PalletID = $"PLT-{25 + (i % 10)}";
			}

			oldReceive.FinaliseDocket();

			// Rest of the pallets go to the engine
			for (var i = 500; i < 1000; i++)
			{
				var line = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				line.WE_PalletID = $"PLT-{50 + (i % 10)}";
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), locations[i].PK.ToGuid(), 1m, ""));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 8 }, // 500/62 = 8.06, max query size results in split query
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 5 },
				{ WhsInventoryViewSchema.Constants.TableName, 20 }, // max query size results in split query
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsPickLineSchema.Constants.TableName, 20 }, // max query size results in split query
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					receiveInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#region TestPutaway_DbHits_PartialPallets

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_PartialPallets()
		{
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10);

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				products.Add(Helper.CreateProduct("P" + i, client));
			}

			Factory.Save();

			var locations = row.Locations.Select(l => l.PK).ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations);

			var putawayInstructions = new List<PutawayResultFact>();
			var receive = Helper.CreateWhsReceive(client, whs);
			for (var i = 0; i < 1000; i++)
			{
				var line = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), locations[i % 100].ToGuid(), 1m,
					$"PLT-{i % 100}"));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 16 }, // 1000/62 = 16.1, max query size results in split query
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 6 }, // max query size results in split query
				{ WhsInventoryViewSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					receiveInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
				AssertEquals("Should have putaway all inventory.", 100,
					receiveInOtherFactory.Lines.Select(i => i.WE_PalletID).Distinct().Count());
			}
		}

		#endregion

		#region TestPutaway_Pallets_MemoryLoad

		[GuiTest]
		[StressTest]
		public void TestPutaway_Pallets_MemoryLoad()
		{
			// 1k inventory to putaway across 100 pallets, 25 on another receive, 25 already putaway on this receive, 50 to go to engine
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				products.Add(Helper.CreateProduct("P" + i, client));
			}

			Factory.Save();

			var locations = row.Locations.ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations.Select(l => l.PK));

			var putawayInstructions = new List<PutawayResultFact>();
			var receive = Helper.CreateWhsReceive(client, whs, "R2");

			// Some pallets on this receive
			for (var i = 0; i < 100; i++)
			{
				var line1 = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				var line2 = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m, locations[i]);
				line1.WE_PalletID = $"PLT-{i % 10}";
				line2.WE_PalletID = $"PLT-{i % 10}";
			}

			// Some pallets on another receive
			var oldReceive = Helper.CreateWhsReceive(client, whs, "R1");
			for (var i = 100; i < 200; i++)
			{
				var line1 = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				var line2 = Helper.CreateWhsReceiveLine(oldReceive, products[i % products.Count], 1m, locations[i]);
				line1.WE_PalletID = $"PLT-{25 + (i % 10)}";
				line2.WE_PalletID = $"PLT-{25 + (i % 10)}";
			}

			oldReceive.FinaliseDocket();

			// Rest of the pallets go to the engine
			for (var i = 200; i < 300; i++)
			{
				var number = 50 + (i % 10);
				var line = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				line.WE_PalletID = $"PLT-{number}";
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), locations[number].PK.ToGuid(), 1m, ""));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (ObjectFactory.Substitute(engineMock.Object))
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				BusinessObjectFactory.StartLogging();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				var loadLog = BusinessObjectFactory.DebugLog;
				BusinessObjectFactory.StopLogging();

				var logCount = Regex.Matches(loadLog,
					Regex.Escape("WE_StockOnHand > 0 and WE_PalletID <> '' and WE_PalletID")).Count;
				CombineAssertions(() =>
				{
					AssertEquals("Should load minimum Pallets.", 30, logCount);
					AssertEquals("Should have shown no errors.", true,
						UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals("Should have putaway all inventory.", true,
						receiveInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
				});
			}
		}

		#endregion

		#region TestPutaway_DbHits_CrossDocked

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_CrossDocked()
		{
			const int NumberOfInventoriesToCreate = 25; // Inventory Lines to create
			var data = new TestDataSimpleEnvironment(Factory, 5, 5, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			var inventories = new List<WhsReceiveLine>(NumberOfInventoriesToCreate);
			for (int i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				inventories.Add(Helper.CreateWhsReceiveLine(receive, data.Part1, 1m));
			}

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			for (int i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, data.Part1, 1m);
				order.WD_WL_CrossDock = dockLocationA.PK;

				var orderLine = order.Lines[0];
				AssertEquals("Precondition: Order is reserved.", 1m,
					orderLine.ReserveStockIfAbleTo(inventories[i].Inventory[0]).ReservedQuantity);
			}

			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertEquals("Ensure operation worked.", true, receiveInOtherFactory.Lines.All(l => !l.WE_WL.IsEmpty));
		}

		#endregion

		#region TestPutaway_DbHits_DifferentReceive

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_DifferentReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location = data.Whs1.FindLocation("A-1");

			var otherReceives = new List<WhsReceive>(10);
			for (int i = 0; i < 10; i++)
			{
				otherReceives.Add(Helper.CreateWhsReceive(data.Org1, data.Whs1, $"OR{i}", Notify));
			}

			for (var i = 0; i < 100; i++)
			{
				var line = Helper.CreateWhsReceiveLine(otherReceives[i % 10], data.Part1, 1m, null, "PLT");
			}

			var putawayInstructions = new List<PutawayResultFact>();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			for (var i = 0; i < 100; i++)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT");
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), location.PK.ToGuid(), 1m, ""));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 4 }, // max query size results in split query
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 3 }, // max query size results in split query
				{ WhsInventoryViewSchema.Constants.TableName, 4 }, // max query size results in split query
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 2 }, // max query size results in split query
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					receiveInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#region TestPutaway_DbHits_DifferentReceive_PutawayTransfers

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_DifferentReceive_PutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var transfers = new List<WhsTransfer>(10);
			for (var i = 0; i < 20; i++)
			{
				var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, $"TR{i}", Notify);
				transfer.WD_IsPutawayTransfer = true;

				transfers.Add(transfer);
			}

			var otherReceives = new List<WhsReceive>(10);
			for (int i = 0; i < 10; i++)
			{
				otherReceives.Add(Helper.CreateWhsReceive(data.Org1, data.Whs1, $"OR{i}", Notify));
			}

			Factory.Save();

			for (var i = 0; i < 100; i++)
			{
				Helper.CreateInventoryForDockDoorLocation(otherReceives[i % 10], data.Part1, dockDoorLocation1, "PLT",
					1m);
			}

			Factory.Save();

			for (var i = 0; i < 100; i++)
			{
				var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(transfers[i % 10], data.Part1,
					dockDoorLocation1, null, "PLT", 1m);
				transferLine1.PickedTime = ZDateTimeOffset.Now;
			}

			transfers.ForEach(t => t.RunPreSaveValidation());
			Factory.Save();

			var putawayInstructions = new List<PutawayResultFact>();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R", Notify);
			for (var i = 0; i < 100; i++)
			{
				var line = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT");
				putawayInstructions.Add(
					new PutawayResultFact(line.PK.ToGuid(), nonDockDoorLocation.PK.ToGuid(), 1m, ""));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 4 }, // max query size results in split query
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 5 }, // max query size results in split query
				{ WhsInventoryViewSchema.Constants.TableName, 6 }, // max query size results in split query
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 6 }, // max query size results in split query
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					receiveInOtherFactory.Lines.Cast<ILineToPutaway>().All(i => i.LocationPK.IsValid));
			}
		}

		#endregion

		#region TestPerformance_AllocateLocations_MultipleInventoriesPerLocation_DBHits

		[GuiTest]
		[StressTest]
		public void TestPerformance_AllocateLocations_MultipleInventoriesPerLocation_DBHits()
		{
			const int numberOfInventoriesToCreate = 10;
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("WH1", "A");

			var receive1 = Helper.CreateWhsReceive(client, whs, "R1", Notify);
			var order = Helper.CreateWhsOrder(client, whs, "O1");
			for (int i = 0; i < numberOfInventoriesToCreate; i++)
			{
				var part = Helper.CreateProduct("P" + i, client);
				var receiveLine = Helper.CreateWhsReceiveLine(receive1, part, 1m);
				receiveLine.WE_WL = whs.DefaultLocation.PK;
				Helper.CreateWhsOrderLine(order, part, 1m);
			}

			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", numberOfInventoriesToCreate, pick.GetAllPickLines().Count());

			var product = Helper.CreateProduct("Part", client);
			var receive2 = Helper.CreateWhsReceiveWithInventory(client, whs, "Receive", product, 10m,
				allocateLocations: false, finalise: false);

			new WhsPutawayLocationCacheManager().CreateCache(Factory, whs.DefaultLocation.PK);

			var putawayInstructions = new List<PutawayResultFact>();
			putawayInstructions.Add(new PutawayResultFact(receive2.Lines.Single().PK.ToGuid(),
				whs.DefaultLocation.PK.ToGuid(), 10m, ""));

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == whs.PK),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive2.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				ReceiveAllocationHelper.AllocateLocations(receiveInOtherFactory, receiveInOtherFactory.Warehouse);
				Assert("Ensure operation worked.", !receiveInOtherFactory.Lines[0].WE_WL.IsEmpty);
			}
		}

		#endregion

		#region TestPerformance_AllocateLocationsAndRunPreSaveValidation_DBHits

		[GuiTest]
		[StressTest]
		public void TestPerformance_AllocateLocationsAndRunPreSaveValidation_DBHits()
		{
			const int NumberOfInventoriesToCreate = 100; // Inventory Lines to create
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);
			var product = Helper.CreateProduct("P1", client);

			Factory.Save();

			var locations = row.Locations.Select(l => l.PK).ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations);

			var putawayInstructions = new List<PutawayResultFact>();
			var receive = Helper.CreateWhsReceive(client, whs);
			for (int i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				var line = Helper.CreateWhsReceiveLine(receive, product, 1m);
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), locations[i].ToGuid(), 1m, ""));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 2 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				ReceiveAllocationHelper.AllocateLocations(receiveInOtherFactory, receiveInOtherFactory.Warehouse);
				receiveInOtherFactory.RunPreSaveValidation();
			}

			Assert("Ensure operation worked.", receiveInOtherFactory.Lines.All(line => !line.WE_WL.IsEmpty));
		}

		#endregion

		#region TestPutaway_DbHits_EndToEnd

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Factory.New<ProductionRuleSet>();
			ruleSet.PRS_Name = "Rules";
			ruleSet.PRS_Description = "Rules";
			ruleSet.PRS_Context = "PWP";
			ruleSet.PRS_IsLive = true;
			ruleSet.PRS_IsSystem = false;

			var rule1 = GetRule(ruleSet, 1,
				data.Part1.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 1);
			var rule2 = GetRule(ruleSet, 2,
				data.Part2.RelatedOrganisations
					.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner).PK, 2);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			new WhsPutawayLocationCacheManager().CreateCache(Factory, new[] { location1.PK, location2.PK });

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 1000; i++)
			{
				Helper.CreateWhsReceiveLine(receive, i % 2 != 0 ? data.Part1 : data.Part2, 5m);
			}

			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 16 }, // 1000/62 = 16.1, max query size results in split query
				{ ProductionRuleSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (RowFactory.SetCachedTables())
			{
				ReceiveAllocationHelper.AllocateLocations(receiveInOtherFactory, receiveInOtherFactory.Warehouse);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertEquals("Ensure operation worked.", true, receiveInOtherFactory.Lines.All(l => !l.WE_WL.IsEmpty));
		}

		#endregion

		#endregion

		#region TestPutaway_LocationCacheUpdateFailed

		public void TestPutaway_LocationCacheUpdateFailed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m,
				allocateLocations: false, finalise: false);
			var inventory = receive.Lines[0];
			var inventories = new[] { inventory };

			var engineMock = new Mock<IUserHaltableProductionRulesEngineService>(MockBehavior.Strict);
			var mockedLocationCacheUpdater = new Mock<IPutawayLocationCacheUpdater>();
			mockedLocationCacheUpdater.Setup(mcu =>
					mcu.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, It.IsAny<ZGuid>(),
						It.IsAny<INotifications>(), 30))
				.Returns(false);

			var putawayManager = new PutawayEngineManagerForReceive(engineMock.Object, Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(), Mock.Of<IPutawayLocationFactLoader>(),
				mockedLocationCacheUpdater.Object);
			putawayManager.Putaway(new[] { receive }, inventories, new NotificationBuffer(), null);

			mockedLocationCacheUpdater.Verify(mcu =>
				mcu.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, data.Whs1.PK,
					It.IsAny<INotifications>(), 30));
			engineMock.VerifyNoOtherCalls();
			Assert(inventory.WE_WL.IsEmpty);
		}

		#endregion

		#region TestPutaway_DbHits_ProductWithUNDG

		[GuiTest]
		[StressTest]
		public void TestPutaway_DbHits_ProductWithUNDG()
		{
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct("P" + i, client);
				products.Add(product);

				if (i % 2 == 0)
				{
					var param = Helper.CreateProductParamsByWhsAndClient(product, client, whs);
					param.W3_WPG_PutawayGroup = Helper.CreatePutawayGroup("P" + i, "P" + i).PK;
				}

				Helper.CreateUNDGDataItem(product, $"007{i}a", "1.1D");
			}

			Factory.Save();

			var locations = row.Locations.Select(l => l.PK).ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations);

			var putawayInstructions = new List<PutawayResultFact>();
			var receive = Helper.CreateWhsReceive(client, whs);
			for (var i = 0; i < 1000; i++)
			{
				var line = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), locations[i].ToGuid(), 1m, ""));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 16 }, // 1000/62 = 16.1, max query size results in split query
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ UNDGSubstanceSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsPickLineSchema.Constants.TableName, 16 }, // max query size results in split query
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsABCCategorySchema.Constants.TableName, 1 },
				{ WhsPutawayGroupSchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
				putawayManager.Putaway(new[] { receiveInOtherFactory }, receiveInOtherFactory.Lines.Cast<WhsReceiveLine>(),
					receiveInOtherFactory.NotificationSubscriber);
				AssertEquals("Should have shown no errors.", true,
					UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Should have putaway all inventory.", true,
					receiveInOtherFactory.Lines.All(i => i.WE_WL.IsValid));
			}
		}

		#endregion

		#region TestPerformance_AllocateLocationsAndRunPreSaveValidation_DBHits_ProductWithUNDG

		[GuiTest]
		[StressTest]
		public void TestPerformance_AllocateLocationsAndRunPreSaveValidation_DBHits_ProductWithUNDG()
		{
			const int NumberOfInventoriesToCreate = 100; // Inventory Lines to create
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);
			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				var product = Helper.CreateProduct("P" + i, client);
				products.Add(product);

				if (i % 2 == 0)
				{
					var param = Helper.CreateProductParamsByWhsAndClient(product, client, whs);
					param.W3_WPG_PutawayGroup = Helper.CreatePutawayGroup("P" + i, "P" + i).PK;
				}

				Helper.CreateUNDGDataItem(product, $"0{i}a", "1.1D");
			}

			Factory.Save();

			var locations = row.Locations.Select(l => l.PK).ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations);

			var putawayInstructions = new List<PutawayResultFact>();
			var receive = Helper.CreateWhsReceive(client, whs);
			for (var i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				var line = Helper.CreateWhsReceiveLine(receive, products[i], 1m);
				putawayInstructions.Add(new PutawayResultFact(line.PK.ToGuid(), locations[i].ToGuid(), 1m, ""));
			}

			Factory.Save();

			var engineMock = new Mock<IProductionRulesEnginePushService>();
			engineMock.Setup(
					e => e.RunRulesEngine(
						RulesContextType.InventoryPutaway,
						RulesContextSubType.None,
						It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
						It.IsAny<IEnumerable<IEnumerable<IInputFact>>>(),
						null,
						It.IsAny<CancellationToken>()))
				.Returns(new ProductionRulesEngineResult(putawayInstructions));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 2 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 2 },
				{ OrgPartUnitSchema.Constants.TableName, 2 },
				{ OrgSupplierPartSchema.Constants.TableName, 2 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ UNDGDataItemSchema.Constants.TableName, 2 },
				{ UNDGSubstanceSchema.Constants.TableName, 2 },
				{ WhsABCCategorySchema.Constants.TableName, 5 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPutawayGroupSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			using (ObjectFactory.Substitute(engineMock.Object))
			using (RowFactory.SetCachedTables())
			{
				ReceiveAllocationHelper.AllocateLocations(receiveInOtherFactory, receiveInOtherFactory.Warehouse);
				receiveInOtherFactory.RunPreSaveValidation();
			}

			Assert("Ensure operation worked.", receiveInOtherFactory.Lines.All(line => !line.WE_WL.IsEmpty));
		}

		#endregion

		#region Implementation

		(Func<IEnumerable<IInputFact>> GetFacts, Func<ProductionRulesEngineResult, INotification> ProcessResults)
			CallRulesEngineMockAndReturnInputOutputFunctions(
				Mock<IUserHaltableProductionRulesEngineService> engineMock,
				ICrossDockManager crossDockManager,
				IPutawayExistingPalletManager putawayExistingPalletManager,
				IPutawayLocationFactLoader locationFactLoader,
				WhsReceive receive,
				IEnumerable<WhsReceiveLine> lines,
				RefEquipment equipment = null,
				IEnumerable<ZGuid> skipLocationPKs = null,
				bool useLocationConcurrencyHandling = false)
		{
			Func<IEnumerable<IInputFact>> getFacts = null;
			Func<ProductionRulesEngineResult, INotification> processResults = null;

			var notifications = new NotificationBuffer();

			engineMock.Setup(em => em.RunRulesEngine(Factory, notifications, RulesContextType.InventoryPutaway,
					It.Is<ProductionRuleSetFilter>(f => f.WarehousePK == receive.WD_WW_Whs),
					It.IsAny<Func<IEnumerable<IInputFact>>>(),
					It.IsAny<Func<ProductionRulesEngineResult, INotification>>()))
				.Callback<BusinessObjectFactory, INotifications, RulesContextType, ProductionRuleSetFilter,
					Func<IEnumerable<IInputFact>>, Func<ProductionRulesEngineResult, INotification>>(
					(f, n, rct, filters, f1, f2) =>
					{
						getFacts = f1;
						processResults = f2;
					});

			var mockedLocationCacheUpdater = new Mock<IPutawayLocationCacheUpdater>();
			mockedLocationCacheUpdater.Setup(mcu =>
					mcu.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, It.IsAny<ZGuid>(),
						It.IsAny<INotifications>(), 30))
				.Returns(true);

			var putawayManager = new PutawayEngineManagerForReceive(engineMock.Object, crossDockManager,
				putawayExistingPalletManager, locationFactLoader, mockedLocationCacheUpdater.Object);
			putawayManager.Putaway(new[] { receive }, lines, notifications, equipment, skipLocationPKs, useLocationConcurrencyHandling);

			AssertNotNull("Should have run RunRulesEngine.", getFacts);
			AssertNotNull("Should have run RunRulesEngine.", processResults);
			mockedLocationCacheUpdater.Verify(locationCacheUpdater =>
				locationCacheUpdater.UpdateWarehousePutawayLocationCacheWithProgressForm(Factory, receive.WD_WW_Whs,
					It.IsAny<INotifications>(), 30));

			return (getFacts, processResults);
		}

		protected override (Func<IEnumerable<IInputFact>> GetFacts, Func<ProductionRulesEngineResult, INotification>
			ProcessResults) CallRulesEngineMockAndReturnInputOutputFunctions(
				Mock<IUserHaltableProductionRulesEngineService> engineMock,
				IPutawayLocationFactLoader locationFactLoader, WhsReceive receive, IEnumerable<WhsReceiveLine> lines,
				RefEquipment equipment = null)
		{
			return CallRulesEngineMockAndReturnInputOutputFunctions(engineMock, Mock.Of<ICrossDockManager>(),
				Mock.Of<IPutawayExistingPalletManager>(), locationFactLoader, receive, lines, equipment);
		}

		protected override WhsReceive CreateJob(OrgHeader client, WhsWarehouse warehouse, string reference)
		{
			return Helper.CreateWhsReceive(client, warehouse, reference);
		}

		protected override WhsReceiveLine CreateLine(WhsReceive job, OrgSupplierPart part, decimal units,
			string palletID = "")
		{
			return Helper.CreateWhsReceiveLine(job, part, units, null, palletID);
		}

		protected override IEnumerable<WhsReceiveLine> GetLinesOnJob(WhsReceive job) =>
			job.Lines.Cast<WhsReceiveLine>();

		#endregion
	}
}
