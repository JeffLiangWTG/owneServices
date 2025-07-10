using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class PackingSynchroniserTest<T> : TestCaseWithFactory
				where T : BaseJobDeclaration
	{
		public void TestMultiHouseBillsPackLines()
		{
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			var subshipment = shipment.CoLoadShipments.AddNew();
			subshipment.JS_HouseBill = "SH1";
			packLine = subshipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 20;

			var subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.JS_HouseBill = "SH2";
			var packLine2 = subshipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 10;

			declaration.Bills.RemoveAndDeleteAll();
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			shipment.PackLineSynchroniser.MarkSyncDirty();

			AssertEquals("PreCondition:two house bills added", 2, declaration.Bills.Count);
			AssertEquals(2, declaration.PackingGroups.Count);
			AssertEquals(20, declaration.PackingGroups[0].TotalPackageCount());

			AssertEquals(10, declaration.PackingGroups[1].TotalPackageCount());

			var packagesPKs = declaration.Packages.GetPKs();

			packLine.JL_PackageCount = 21;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			foreach (var package in declaration.Packages)
			{
				Assert("When resynchronised, it should recycle", packagesPKs.Contains(package.PK));
			}
		}

		public void TestDoesNotSyncMultiHouseBillsIfPakingInforamtionIsNotRelevant()
		{
			declaration = GetDeclarationPackingNotRelevant();
			if (declaration != null)
			{
				AssertEquals("Pre-Condition: IsPackingInformationRelevant", false, declaration.IsPackingInformationRelevant);

				shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "HBL1";
				var packLine = shipment.OuterPackLines.AddNew();

				var subshipment = shipment.CoLoadShipments.AddNew();
				subshipment.JS_HouseBill = "SH1";
				subshipment.OuterPackLines.AddNew();

				declaration.JE_JS = shipment.PK;
				declaration.ShipmentSynchroniser.Synchronise(true);

				AssertEquals("Only main shipment should be synced", 1, declaration.Bills.Count);
				AssertEquals(shipment.JS_HouseBill, declaration.Bills.PrimaryHouseBill.CU_HouseBill);
			}
			else
			{
				Assert(true);
			}
		}

		protected abstract T GetDeclarationPackingNotRelevant();

		public void TestCS00116517()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "08112345678";
			DecorateConsolToBeRelevant(consol);

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "ULD1";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "ULD2";
			container2.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKOrigin = "KRBUS";
			shipment.Consols.Add(consol);

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine.JL_PackageCount = 1;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container2);
			packLine.JL_PackageCount = 1;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine.JL_PackageCount = 1;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Carton;
			Factory.Save();

			declaration.Bills.RemoveAndDeleteAll();
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
			AssertEquals("For this AIR job, CusContainers are deleted", 0, declaration.CusContainers.Count);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var decLoaded = factory2.Load<T>(declaration.PK);
			decLoaded.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			decLoaded.HasChanges = false;//this is what happens after sync is finished at the plugin level and JobDeclaration.OnSaving does not get called
			AssertNoExceptionThrown(delegate
			{ factory2.Save(); });
			AssertEquals("For this AIR job, CusContainers are deleted", 0, decLoaded.CusContainers.Count);
			AssertEquals(1, decLoaded.PackingGroups.Count);
			AssertEquals(3, decLoaded.Packages.Count);
		}

		public void TestWhenBillIsCreatedLater_CS00135965()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "12345678";//US syncs to MB as 12345678
			DecorateConsolToBeRelevant(consol);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX783247";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKDestination = "KRBUS";
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_HouseBill = ZString.Empty;
			shipment.Consols.Add(consol);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, null);
			packLine.SetContainer(consol, container);
			packLine.JL_PackageCount = 1;

			var declaration = GetDeclarationPackingRelevant();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			TestResultsWhenBillIsCreatedLater(declaration);

			shipment.JS_HouseBill = "1";
			AssertEquals("House bill is added", 2, declaration.Bills.Count);
			AssertEquals("Packages are created", 1, declaration.PackingGroups.Count);
			AssertEquals("Packages are created", 1, declaration.Packages.Count);
		}

		protected virtual void TestResultsWhenBillIsCreatedLater(T declaration)
		{
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(0, declaration.PackingGroups.Count);
		}

		public void TestClearHasChangesShouldNotHappenWhenContainerIsDeleted_CS00135077()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "12345678";//US syncs to MB as 12345678
			DecorateConsolToBeRelevant(consol);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX783247";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKDestination = "KRBUS";
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_HouseBill = ZString.Empty;
			shipment.Consols.Add(consol);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine.JL_PackageCount = 1;

			Factory.Save();

			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("PreCondition", 1, declaration.CusContainers.Count);
			AssertEquals("PreCondition", 1, declaration.PackingGroups.Count);
			AssertEquals("PreCondition", declaration.CusContainers[0].PK, declaration.PackingGroups[0].CR_CO_Container);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declarationLoaded = factory2.Load<T>(declaration.PK);
			var containerLoaded = factory2.Load<ForwardingContainer>(container.PK);
			containerLoaded.Delete(); // Should we block the deletion of JobContainer when there're Custainers linked to it, or fix the issue on saving?

			Assert(declarationLoaded.ShouldSynchroniseWithShipment());
			((Integration.Customs.IJobDeclarationWithShipmentSynchonisation)declarationLoaded).SynchroniseWithShipmentIfNeeded();

			using (TemporarilySetUser(User.InterchangeUserCode))
			{
				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					factory2.Save();
					Fail("factory2 can't save due to FK violation");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);

					var errorMessage = ErrorReporter.LastMessageReported;
					var cusContainer = declarationLoaded.CusContainers[0];
					var newContainer = cusContainer.JobContainer;

					CombineAssertions("Error reported by CommonContainerUniqueContainerNumIndexFailureHandler", () =>
					{
						AssertEquals("expected to report self concurrency", "NR_UX__JC_JK_JC_ContainerNum Self Concurrency: Deleted Container(s) - V3", ErrorReporter.LastKeyReported);
						AssertContains("There's 2 or more containers with the same number and at least one of them was created by Customs, which caused possible self concurrency of NR_UX__JC_JK_JC_ContainerNum", errorMessage);
						AssertContains("The container changed in current Factory: Container 'CRUX783247'|IsExistsInDatabase: True|IsModifiedInDatabase: False|Deleted: True|LastModified", errorMessage);
						AssertContains($"The container in DB is deleting. PK: {container.PK}|JC_ContainerNum: CRUX783247|JC_ContainerJobID: {container.JC_ContainerJobID}", errorMessage);
						AssertContains("Deletion Logging Details:    at Enterprise.Freight.Forwarding.Business.ForwardingContainer.Delete()", errorMessage);
						AssertContains($"The CusContainer changed in current Factory. PK: {cusContainer.PK}|CO_ContainerNumber: CRUX783247|CO_JC: {newContainer.PK}|Original CO_JC: {container.PK}|IsExistsInDatabase: True|IsModifiedInDatabase: False|Deleted: False", errorMessage);
						AssertContains($"The container in DB (loaded by new factory). PK: {container.PK}|JC_ContainerNum: CRUX783247|JC_ContainerJobID: {container.JC_ContainerJobID}", errorMessage);
					});
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}

			IDisposable TemporarilySetUser(string userCode)
			{
				var oldValue = GlbStaff.CurrentUser.GS_Code;
				GlbStaff.CurrentUser.GS_Code = userCode;

				return new DisposableAction(() => { GlbStaff.CurrentUser.GS_Code = oldValue; });
			}
		}

		public void TestDirectShipmentWithNoHouseBillNumber_CS00133722()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "APLU12345678";//US syncs to MB as 12345678
			DecorateConsolToBeRelevant(consol);

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX783247";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKOrigin = "KRBUS";
			shipment.JS_HouseBill = ZString.Empty;
			shipment.Consols.Add(consol);

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.CurrentConsol = consol;
			packLine.JL_Calc_ContainerNumber = "CRUX783247";
			packLine.JL_PackageCount = 1;

			Factory.Save();

			declaration.Bills.RemoveAndDeleteAll();
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Packing should have lines against master bill", 1, declaration.PackingGroups.Count);
			AssertEquals("Packing should have lines against master bill", 1, declaration.Packages.Count);
		}

		public void TestMultiHouseBillsPackLines_MasterShipment()
		{
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			ForwardingShipment subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.JS_HouseBill = "SH1";
			ForwardingPackLine packLine1 = subshipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 20;

			ForwardingShipment subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.JS_HouseBill = "SH2";
			ForwardingPackLine packLine2 = subshipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 30;

			AssertEquals("Master shipment is a Master House", true, shipment.IsMasterShipmentRepresentingAllChildShipments);
			AssertEquals("Master shipment has 2 packlines from sub-shipments", 2, shipment.OuterPackLines.Count);

			declaration.Bills.RemoveAndDeleteAll();
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			shipment.PackLineSynchroniser.MarkSyncDirty();

			AssertEquals("PreCondition:Two house bills are added", 2, declaration.Bills.Count);
			AssertEquals(2, declaration.PackingGroups.Count);
			AssertEquals(20, declaration.PackingGroups[0].TotalPackageCount());
			AssertEquals(30, declaration.PackingGroups[1].TotalPackageCount());
		}

		protected void DoPackSyncronise()
		{
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
		}

		public abstract void TestSychroniseCore();

		public void TestRecycledPackGroupsSyncronising()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DecorateConsolToBeRelevant(consol);

			consol.Shipments.Add(shipment);
			ForwardingContainer forwardingContainer1 = consol.Containers.AddNew();
			forwardingContainer1.JC_ContainerNum = "C1";
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_JC = forwardingContainer1.PK;
			ForwardingContainer forwardingContainer2 = consol.Containers.AddNew();
			forwardingContainer2.JC_ContainerNum = "C2";
			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_JC = forwardingContainer2.PK;
			packLine2.JL_PackageCount = 2;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals(2, declaration.CusContainers.Count);
			BaseCusContainer cusContainer1 = declaration.CusContainers[0];
			BaseCusContainer cusContainer2 = declaration.CusContainers[1];
			if (cusContainer2.CO_ContainerNumber == forwardingContainer1.JC_ContainerNum)
			{
				cusContainer1 = declaration.CusContainers[1];
				cusContainer2 = declaration.CusContainers[0];
			}
			AssertEquals(forwardingContainer1.JC_ContainerNum, cusContainer1.CO_ContainerNumber);
			AssertEquals(forwardingContainer2.JC_ContainerNum, cusContainer2.CO_ContainerNumber);

			shipment.PackLineSynchroniser.MarkSyncDirty();

			AssertEquals("2 packing groups on declaration", 2, declaration.PackingGroups.Count);
			BasePackingGroup declarationPackingGroup1 = declaration.PackingGroups[0];
			AssertEquals("Correct container number", "C1", declarationPackingGroup1.Container.CO_ContainerNumber);
			BasePackingGroup declarationPackingGroup2 = declaration.PackingGroups[1];
			AssertEquals("Correct container number", "C2", declarationPackingGroup2.Container.CO_ContainerNumber);

			forwardingContainer2.JC_ContainerNum = "C3";
			AssertEquals(forwardingContainer2.JC_ContainerNum, cusContainer2.CO_ContainerNumber);

			ForwardingContainer forwardingContainer3 = consol.Containers.AddNew();
			forwardingContainer3.JC_ContainerNum = "C4";
			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_JC = forwardingContainer3.PK;
			packLine3.JL_PackageCount = 3;
			AssertEquals(3, declaration.CusContainers.Count);
			BaseCusContainer cusContainer3 = declaration.CusContainers.Find(forwardingContainer3.JC_ContainerNum);

			shipment.PackLineSynchroniser.MarkSyncDirty();

			AssertEquals("now 3 packing groups on declaration", 3, declaration.PackingGroups.Count);
			BasePackingGroup declarationPackingGroup3 = declaration.PackingGroups[0];
			AssertEquals("Correct container number", "C1", declarationPackingGroup3.Container.CO_ContainerNumber);
			BasePackingGroup declarationPackingGroup4 = declaration.PackingGroups[1];
			AssertEquals("Correct container number", "C3", declarationPackingGroup4.Container.CO_ContainerNumber);
			AssertEquals("First packing group has been recycled", declarationPackingGroup1.PK, declarationPackingGroup3.PK);
			AssertEquals("Second packing group has been recycled", declarationPackingGroup2.PK, declarationPackingGroup4.PK);
			BasePackingGroup declarationPackingGroup5 = declaration.PackingGroups[2];
			AssertEquals("Correct container number", "C4", declarationPackingGroup5.Container.CO_ContainerNumber);

			packLine1.Delete();
			packLine2.Delete();

			shipment.PackLineSynchroniser.MarkSyncDirty();

			AssertEquals("now 1 packing groups on declaration", 1, declaration.PackingGroups.Count);
			BasePackingGroup declarationPackingGroup6 = declaration.PackingGroups[0];
			AssertEquals("Correct container number", "C4", declarationPackingGroup6.Container.CO_ContainerNumber);
			AssertEquals("packing group has been recycled", declarationPackingGroup5.PK, declarationPackingGroup6.PK);
		}

		public void TestDontSynchroniseCoreIfNotDirty()
		{
			var mockDec = Factory.NewMoq<T>();
			var declaration = mockDec.Object;
			var mockPackingSync = SetUpMockPackingSync(mockDec);
			var packSync = mockPackingSync.Object;

			var mockPackingSyncProtected = mockPackingSync.Protected();
			var packLine = shipment.OuterPackLines.AddNew();
			mockPackingSyncProtected.Setup("SynchronisePackLine", packLine, shipment);
			mockDec.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			packSync.Synchronise();
			AssertNoExceptionThrown(() => mockPackingSyncProtected.Verify("SynchronisePackLine", Times.Never(), new object[] { packLine, shipment }));
		}

		public void TestDontSynchroniseCoreIfOverrideFreightDefaultIsTicked()
		{
			var mockDec = Factory.NewMoq<T>();
			var declaration = mockDec.Object;
			var mockPackingSync = SetUpMockPackingSync(mockDec);
			var packSync = mockPackingSync.Object;

			var mockPackingSyncProtected = mockPackingSync.Protected();
			var packLine = shipment.OuterPackLines.AddNew();
			mockPackingSyncProtected.Setup("SynchronisePackLine", packLine, shipment);
			declaration.JE_OverrideFreightDefaults = true;
			mockDec.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			packSync.Synchronise();

			AssertNoExceptionThrown(() => mockPackingSyncProtected.Verify("SynchronisePackLine", Times.Never(), new object[] { packLine, shipment }));
		}

		public void TestDontSynchroniseCoreIfNotRelevant()
		{
			var mockDec = Factory.NewMoq<T>();
			var declaration = mockDec.Object;
			var mockPackingSync = SetUpMockPackingSync(mockDec);
			var packSync = mockPackingSync.Object;

			var mockPackingSyncProtected = mockPackingSync.Protected();
			var packLine = shipment.OuterPackLines.AddNew();
			mockPackingSyncProtected.Setup("SynchronisePackLine", packLine, shipment);
			declaration.JE_OverrideFreightDefaults = false;
			mockDec.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(false);
			packSync.Synchronise();

			AssertNoExceptionThrown(() => mockPackingSyncProtected.Verify("SynchronisePackLine", Times.Never(), new object[] { packLine, shipment }));
		}

		public void TestDontSynchroniseCoreIfNotForcedAndDisabled()
		{
			var mockDec = Factory.NewMoq<T>();
			var declaration = mockDec.Object;

			var mockPackingSync = SetUpMockPackingSync(mockDec);
			var packSync = mockPackingSync.Object;

			var mockPackingSyncProtected = mockPackingSync.Protected();
			var packLine = shipment.OuterPackLines.AddNew();
			mockPackingSyncProtected.Setup("SynchronisePackLine", packLine, shipment);
			mockDec.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			packSync.Synchronise(false);
			mockPackingSyncProtected.Verify("SynchronisePackLine", Times.Never(), new object[] { packLine, shipment });

			packSync.Synchronise(true);
			AssertNoExceptionThrown(() => mockPackingSyncProtected.Verify("SynchronisePackLine", Times.Once(), new object[] { packLine, shipment }));
		}

		public void TestSynchroniseCoreBaseBehaviour()
		{
			var mockDec = Factory.NewMoq<T>();

			var declaration = mockDec.Object;
			declaration.JE_JS = shipment.PK;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_HouseBill = shipment.JS_HouseBill;

			var mockPackingSync = SetUpMockPackingSync(mockDec);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 17;
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undg = packLine.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);

			declaration.PackingGroups.RemoveAndDeleteAll();
			var packSync = mockPackingSync.Object;
			mockDec.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			packSync.Synchronise(true);
			AssertEquals(1, declaration.PackingGroups.Count);
			AssertEquals(1, declaration.Packages.Count);
			AssertEquals(declaration.Bills[0], declaration.PackingGroups[0].Bill);
			AssertEquals("123a", declaration.Packages[0].UNDGs[0].UNDGSubstance.DG_Code);
			AssertEquals(17, declaration.Packages[0].CW_PackQty);
		}

		Mock<PackingSynchroniser> SetUpMockPackingSync(Mock<T> mockDec)
		{
			var declaration = mockDec.Object;
			declaration.JE_JS = shipment.PK;

			var mockShipmentSync = new Mock<JobDeclarationSynchroniser>(declaration) { CallBase = true };
			var shipmentSync = mockShipmentSync.Object;

			var mockPackingSync = new Mock<PackingSynchroniser>(shipmentSync, declaration) { CallBase = true };
			var packSync = mockPackingSync.Object;

			mockShipmentSync.Protected().Setup<PackingSynchroniser>("GetPackingSynchroniser").Returns(packSync);
			mockDec.Protected().Setup<JobDeclarationSynchroniser>("GetNewShipmentSynchroniser").Returns(shipmentSync);
			return mockPackingSync;
		}

		protected abstract T GetDeclarationPackingRelevant();

		protected virtual void DecorateConsolToBeRelevant(ForwardingConsol consol)
		{
			consol.JK_RL_NKLoadPort = "KRBUS";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		}

		protected T declaration;
		protected ForwardingShipment shipment;
		protected PackingSynchroniser synchroniser;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetDeclarationPackingRelevant();
			declaration.JE_GB = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew().PK;
			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			declaration.JE_JS = shipment.PK;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = shipment.JS_HouseBill;

			declaration.ShipmentSynchroniser.Synchronise();//to initialise shipment.PackSynchroniser
		}
	}
}
