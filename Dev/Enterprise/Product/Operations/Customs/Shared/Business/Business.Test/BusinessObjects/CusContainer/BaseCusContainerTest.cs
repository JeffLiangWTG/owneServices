using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseCusContainerTest<TCusContainer, TJobDeclaration> : BaseCusContainerWithCustomLabelsTestCase
		where TCusContainer : BaseCusContainer
		where TJobDeclaration : BaseJobDeclaration
	{
		public void TestCO_DataModelWhenJE_DataModelIsEmpty()
		{
			var declaration = Factory.New<TJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			container.OnSaving();
			AssertEquals("CO_DataModel", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, container.CO_DataModel);
		}

		public void TestCO_DataModel_SetOnSaving()
		{
			var declaration = Factory.New<TJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			AssertEquals("Not set", ZString.Empty, container.CO_DataModel);
			Factory.Save();
			AssertEquals("set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, container.CO_DataModel);
		}

		[ExpectNoExceptions]
		public void TestDeleteJobContainerSafe()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.RemoveAndDeleteAll();
			consol.Containers.RemoveAndDeleteAll();

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();

			var container = consol.Containers.AddNew();
			container.FillWithValidTestData();

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();

			var cusContainer = declaration.CusContainers.AddNew();
			var jobContainer = cusContainer.JobContainer;

			declaration.CusContainers.ListChanged += (s, e) =>
			{
				if (!cusContainer.IsDeleted)
				{
					var value = cusContainer.GrossWeightForBinding;
				}
			};

			cusContainer.Delete();

			Assert("The customs container should be deleted.", cusContainer.IsDeleted);
			Assert("The invalid forwarding container should be deleted.", jobContainer.IsDeleted);

			declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var cusContainerFromConsol = declaration.CusContainers.AddNew();
			cusContainerFromConsol.CO_ContainerNumber = container.JC_ContainerNum;
			cusContainerFromConsol.CO_JC = container.PK;

			var jobContainerFromConsol = cusContainerFromConsol.JobContainer;

			declaration.CusContainers.ListChanged += (s, e) =>
			{
				if (!cusContainer.IsDeleted)
				{
					var value = cusContainer.GrossWeightForBinding;
				}
			};

			cusContainerFromConsol.Delete();

			Assert("The customs container should be deleted.", cusContainerFromConsol.IsDeleted);
			Assert("The valid forwarding container should not be deleted.", !jobContainerFromConsol.IsDeleted);
		}

		public void TestDeliveryModeForBindingList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var bindingList = container.JobContainer.DeliveryMode_ListForBinding;
			Assert("We assume that forwarding container created without Consol is always Sea.", bindingList.ContainsCode(Constants.DeliveryModes.Codes.CFS_CFS));
			Assert("We assume that forwarding container created without Consol is always Sea.", bindingList.ContainsCode(Constants.DeliveryModes.Codes.CFS_CY));
			Assert("We assume that forwarding container created without Consol is always Sea.", bindingList.ContainsCode(Constants.DeliveryModes.Codes.CY_CFS));
			Assert("We assume that forwarding container created without Consol is always Sea.", bindingList.ContainsCode(Constants.DeliveryModes.Codes.CY_CY));
		}

		public void TestDeleteDuplicatePackingGroups()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;

			var bill = declaration.Bills.AddNew();
			var bill2 = declaration.Bills.AddNew();
			var container = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();
			var container3 = declaration.CusContainers.AddNew();

			var packGroupNonContainerised = bill.PackingGroups.AddNew();
			var package = packGroupNonContainerised.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "KG";

			var packGroupContainerised = bill.PackingGroups.AddNew();
			packGroupContainerised.CR_CO_Container = container.PK;
			var package2 = packGroupContainerised.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = "TT";

			var packGroupContainerisedToContainer2 = bill2.PackingGroups.AddNew();
			packGroupContainerisedToContainer2.CR_CO_Container = container2.PK;

			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });

			var packGroupContainerisedToContainer3 = declaration.PackingGroups.AddNew();
			packGroupContainerisedToContainer3.CR_CO_Container = container3.PK;
			AssertEquals("PreCondition:Not linked to bill, therefore will be deleted", ZGuid.Empty, packGroupContainerisedToContainer3.CR_CU_HouseBill);

			container.Delete();
			container2.Delete();
			container3.Delete();

			AssertEquals("packGroupContainerised should be deleted", true, packGroupContainerised.IsDeleted);
			AssertEquals("packGroupContainerisedToContainer2 should not be deleted", false, packGroupContainerisedToContainer2.IsDeleted);
			AssertEquals("packGroupContainerisedToContainer2 should not be deleted", ZGuid.Empty, packGroupContainerisedToContainer2.CR_CO_Container);
			AssertEquals("packGroupContainerisedToContainer3 should be deleted", true, packGroupContainerisedToContainer3.IsDeleted);

			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
		}

		#region TestDeleteRemovesLinkedPackingGroups

		public virtual void TestDeleteRemovesLinkedPackingGroups()
		{
			var testDec = GetJobDeclaration();
			var container1 = testDec.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;
			var container2 = testDec.CusContainers.AddNew();
			var container3 = testDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OCLU7654321";
			var container4 = testDec.CusContainers.AddNew();

			var houseBill1 = testDec.Bills.AddNew();
			var packGroup = houseBill1.PackingGroups.AddNew();
			packGroup.CR_CO_Container = container1.PK;

			var houseBill2 = testDec.Bills.AddNew();
			var packGroup2 = houseBill2.PackingGroups.AddNew();
			packGroup2.CR_CO_Container = container2.PK;

			var houseBill3 = testDec.Bills.AddNew();
			var packGroup3 = houseBill3.PackingGroups.AddNew();
			packGroup3.CR_CO_Container = container1.PK;

			var houseBill4 = testDec.Bills.AddNew();
			var packGroup4 = houseBill4.PackingGroups.AddNew();
			packGroup4.CR_CO_Container = container3.PK;

			var houseBill5 = testDec.Bills.AddNew();
			var packGroup5 = houseBill5.PackingGroups.AddNew();
			packGroup5.CR_CO_Container = container4.PK;

			container1.Delete();
			container3.Delete();
			container4.Delete();
			Assert("PackGroup should have been deleted", packGroup.IsDeleted);
			AssertEquals("PackGroup2 should not be deleted", container2.PK, packGroup2.CR_CO_Container);
			Assert("PackGroup3 should have been deleted", packGroup3.IsDeleted);
			Assert("PackGroup4 should have been deleted", packGroup4.IsDeleted);
			AssertEquals("PackGroup5 should have been detached", ZGuid.Empty, packGroup5.CR_CO_Container);
		}

		public virtual void TestDeleteRemovesLinkedPackingGroupsWithJobContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234567";

			//Set up CusContainer with same data
			var shipment = (CommonShipment)consol.Shipments.AddNew();
			var testDec = GetJobDeclaration();
			testDec.JE_TransportMode = testDec.TransportModeSeaCodeForTesting;
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			testDec.JE_JS = shipment.PK;

			var cusContainer = testDec.CusContainers.AddNew();
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			cusContainer.CO_ContainerNumber = "CONT1234567";
			cusContainer.CO_JC = container.PK;

			var houseBill1 = testDec.Bills.AddNew();
			var packGroup = houseBill1.PackingGroups.AddNew();
			packGroup.CR_CO_Container = cusContainer.PK;

			var houseBill2 = testDec.Bills.AddNew();
			var packGroup2 = houseBill2.PackingGroups.AddNew();
			packGroup2.CR_CO_Container = cusContainer.PK;

			Factory.Save();
			cusContainer.Delete();

			AssertEquals(true, cusContainer.IsDeleted);
			AssertEquals("Deleting CusContainer should *not* delete a JobContainer that has a Consol attached.", false, container.IsDeleted);
			AssertEquals(true, packGroup.IsDeleted);
			AssertEquals(false, packGroup2.IsDeleted);
			AssertEquals(ZGuid.Empty, packGroup2.CR_CO_Container);
		}

		public void TestDeleteDoesNotDeleteBlankJobContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();

			var shipment = (CommonShipment)consol.Shipments.AddNew();
			var testDec = GetJobDeclaration();
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			testDec.JE_JS = shipment.PK;

			var cusContainer = testDec.CusContainers.AddNew();
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			cusContainer.CO_ContainerNumber = "CONT1234567";
			cusContainer.CO_JC = container.PK;

			Factory.Save();
			cusContainer.Delete();

			AssertEquals(true, cusContainer.IsDeleted);
			AssertEquals("Deleting CusContainer should *not* delete a JobContainer that has a Consol attached.", false, container.IsDeleted);
		}

		public void TestLostContainerLinkToJobContainerIsFixed()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "C1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "C2";

			var shipment1 = (CommonShipment)consol.Shipments.AddNew();
			var line1 = shipment1.OuterPackLines.AddNew();
			container1.PackLines.Add(line1);
			line1.JL_ActualWeight = 100m;
			line1.JL_ActualWeightUQ = "LB";

			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_OverrideFreightDefaults = true;
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			cusContainer.CO_ContainerNumber = "C2";
			cusContainer.CO_JC = ZGuid.Empty;
			declaration.JE_JS = shipment1.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);

			AssertEquals("JobContainer link is fixed", container2, cusContainer.JobContainer);
			AssertEquals("GUID", container2.PK, cusContainer.CO_JC);

			var cusContainer2 = declaration.CusContainers.AddNew();
			cusContainer2.CO_ContainerNumber = "C1";
			cusContainer2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("GUID", container1.PK, cusContainer2.CO_JC);
		}

		public void TestDuplicatedJobContainerIsNotCreatedByDataRefresh()
		{
			var consol = Factory.New<ForwardingConsol>();
			var c1 = consol.Containers.AddNew();
			c1.JC_ContainerNum = "C1";

			var shipment1 = (CommonShipment)consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "Shipment1";
			shipment1.JS_ActualWeight = 120m;

			var shipment2 = (CommonShipment)consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "Shipment2";
			shipment2.JS_ActualWeight = 156m;

			var declaration1 = BaseJobDeclaration.New(Factory);
			declaration1.JE_TransportMode = declaration1.TransportModeSeaCodeForTesting;
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration1.JE_JS = shipment1.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration1);

			var dec1Synchroniser = new JobDeclarationSynchroniser(declaration1);
			dec1Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Should have synchronised", 1, declaration1.CusContainers.Count);

			var cusContainer1 = declaration1.CusContainers[0];
			AssertEquals("CusContainer linked to JobContainer", c1.PK, cusContainer1.CO_JC);

			var declaration2 = BaseJobDeclaration.New(Factory);
			declaration2.JE_TransportMode = declaration2.TransportModeSeaCodeForTesting;
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.JE_JS = shipment2.PK;
			var dec2Synchroniser = new JobDeclarationSynchroniser(declaration2);
			dec2Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Should have synchronised", 1, declaration2.CusContainers.Count);

			var cusContainer2 = declaration2.CusContainers[0];
			AssertEquals("CusContainer linked to JobContainer", c1.PK, cusContainer2.CO_JC);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			var cusContainer1InFactory2 = factory2.Load<BaseCusContainer>(cusContainer1.PK);
			var cusContainer2InFactory3 = factory3.Load<BaseCusContainer>(cusContainer2.PK);
			cusContainer1InFactory2.CO_ContainerNumber = "C2";
			cusContainer2InFactory3.CO_ContainerNumber = "C2";
			c1.JC_ContainerNum = "C2";
			factory3.Save();
			AssertEquals("Container number in original factory", "C2", cusContainer2.CO_ContainerNumber);
			factory2.Save();
			AssertEquals("Container number in original factory", "C2", cusContainer1.CO_ContainerNumber);
			Factory.Save();
			AssertEquals("Consol Container number should change", "C2", c1.JC_ContainerNum);

			AssertEquals("CusContainer1 points to correct JobContainer", c1.PK, cusContainer1InFactory2.CO_JC);
			AssertEquals("CusContainer2 points to correct JobContainer", c1.PK, cusContainer2InFactory3.CO_JC);
			consol.Containers.Load();
			AssertEquals("Only one container on consol", 1, consol.Containers.Count);
		}

		public void TestDuplicatedJobContainerIsNotCreatedWithSeparateUsers()
		{
			Factory.RefreshEnabled = false;
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = (CommonShipment)consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "Shipment1";
			shipment1.JS_ActualWeight = 156m;
			shipment1.JS_OuterPacks = 1;

			var c1 = consol.Containers.AddNew();
			c1.JC_ContainerNum = "C1";
			var declaration1 = BaseJobDeclaration.New(Factory);
			declaration1.JE_TransportMode = declaration1.TransportModeSeaCodeForTesting;
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration1.JE_JS = shipment1.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration1);
			var dec1Synchroniser = new JobDeclarationSynchroniser(declaration1);
			dec1Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Should have synchronised", 1, declaration1.CusContainers.Count);

			var cusContainer1 = declaration1.CusContainers[0];
			AssertEquals("CusContainer linked to JobContainer", c1.PK, cusContainer1.CO_JC);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var cusContainer1InFactory2 = factory2.Load<BaseCusContainer>(cusContainer1.PK);
			cusContainer1InFactory2.CO_ContainerNumber = "C2";
			c1.JC_ContainerNum = "C2";
			factory2.Save();

			AssertEquals("CusContainer1 in factory2 points to the same JobContainer", c1.PK, cusContainer1InFactory2.CO_JC);
			consol.Containers.Load();
			AssertEquals("Only one container on consol", 1, consol.Containers.Count);
		}

		public void TestCanMergeIdenticalCusContainers()
		{
			Factory.RefreshEnabled = false;
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = (CommonShipment)consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "Shipment1";
			shipment1.JS_ActualWeight = 156m;
			shipment1.JS_OuterPacks = 1;

			var c1 = consol.Containers.AddNew();
			c1.JC_ContainerNum = "C1";
			var declaration1 = BaseJobDeclaration.New(Factory);
			declaration1.JE_TransportMode = declaration1.TransportModeSeaCodeForTesting;
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration1.JE_JS = shipment1.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration1);
			var dec1Synchroniser = new JobDeclarationSynchroniser(declaration1);
			dec1Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Should have synchronised", 1, declaration1.CusContainers.Count);

			var cusContainer1 = declaration1.CusContainers[0];
			AssertEquals("CusContainer linked to JobContainer", c1.PK, cusContainer1.CO_JC);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var cusContainer1InFactory2 = factory2.Load<BaseCusContainer>(cusContainer1.PK);

			cusContainer1InFactory2.CO_ContainerNumber = "C2";
			factory2.Save();

			cusContainer1.CO_ContainerNumber = "C2";
			AssertNoExceptionThrown("Should save without throwing ZSaveConcurrencyException", Factory.Save);
		}

		public void TestUpdateContainerNumberDoesNotCreateNewContainerOnConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = (CommonShipment)consol.Shipments.AddNew();
			var consolContainer1 = consol.Containers.AddNew();
			consolContainer1.JC_ContainerNum = "TESTCONTAINER1";

			var declaration1 = BaseJobDeclaration.New(Factory);
			declaration1.JE_TransportMode = declaration1.TransportModeSeaCodeForTesting;
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration1.JE_JS = shipment1.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration1);

			var cusContainer1 = declaration1.CusContainers.AddNew();
			cusContainer1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			cusContainer1.CO_ContainerNumber = "TESTCONTAINER1";
			Factory.Save();
			AssertEquals("CusContainer linked to JobContainer", consolContainer1.PK, cusContainer1.CO_JC);
			AssertEquals("consol container number", "TESTCONTAINER1", consol.Containers[0].JC_ContainerNum);
			AssertEquals("declaration container number", "TESTCONTAINER1", declaration1.CusContainers[0].CO_ContainerNumber);
			declaration1.CusContainers[0].CO_ContainerNumber = "CONTAINER2";
			Factory.Save();

			consol.Containers.Load();
			AssertEquals("Only one container on consol", 1, consol.Containers.Count);
			AssertEquals("Should be the same Consol Container Number as Declaration Container Number", "CONTAINER2", consol.Containers[0].JC_ContainerNum);
			AssertEquals("Declaration Container Number", "CONTAINER2", declaration1.CusContainers[0].CO_ContainerNumber);
		}

		public void TestCusContainerPointToCorrectConsolContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolContainer1 = consol.Containers.AddNew();
			consolContainer1.JC_ContainerNum = "C1";

			var consolContainer2 = consol.Containers.AddNew();
			consolContainer2.JC_ContainerNum = "C2";

			var shipment1 = (CommonShipment)consol.Shipments.AddNew();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.Containers.Add(consolContainer1);

			var packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.Containers.Add(consolContainer2);

			var declaration1 = BaseJobDeclaration.New(Factory);
			declaration1.JE_TransportMode = declaration1.TransportModeSeaCodeForTesting;
			declaration1.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration1.JE_JS = shipment1.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration1);
			Factory.Save();

			var dec1Synchroniser = new JobDeclarationSynchroniser(declaration1);
			dec1Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Should have synchronised", 2, declaration1.CusContainers.Count);
			var cusContainer1 = declaration1.CusContainers[0];
			var cusContainer2 = declaration1.CusContainers[1];
			AssertEquals("C1", cusContainer1.CO_ContainerNumber);
			AssertEquals("CusContainer1 linked to JobContainer1", consolContainer1.JC_ContainerNum, cusContainer1.CO_ContainerNumber);

			AssertEquals("C2", cusContainer2.CO_ContainerNumber);
			AssertEquals("CusContainer2 linked to JobContainer2", consolContainer2.JC_ContainerNum, cusContainer2.CO_ContainerNumber);

			cusContainer1.CO_ContainerNumber = "C2";
			AssertEquals("CusContainer1 linked to JobContainer2", consolContainer2.JC_ContainerNum, cusContainer1.CO_ContainerNumber);

			var cusContainer3 = declaration1.CusContainers.AddNew();
			cusContainer3.CO_ContainerNumber = "C3";
			cusContainer3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			AssertNotEquals(consolContainer1.PK, cusContainer3.CO_JC);
			AssertNotEquals(consolContainer2.PK, cusContainer3.CO_JC);

			consol.Containers.Load();
			AssertEquals("should be new container on consol", 3, consol.Containers.Count);
		}

		#endregion

		#region Ref Container Properties

		#region CO_Ref_ContainerCapacity

		public void TestCO_Ref_ContainerCapacity()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_ContainerCapacity", 0m, container.CO_Ref_ContainerCapacity);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			AssertEquals("CO_Ref_ContainerCapacity", refContainer.RC_CubicCapacity, container.CO_Ref_ContainerCapacity);
		}

		public void TestCO_Ref_ContainerCapacityInfo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_ContainerCapacity", container.CO_Ref_ContainerCapacityInfo.Name);
			AssertEquals("Should default to ReadOnly", true, container.CO_Ref_ContainerCapacityInfo.ReadOnly);
		}

		#endregion

		#region CO_Ref_MaxGrossWeight()

		public void TestCO_Ref_MaxGrossWeight()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_MaxGrossWeight", 0m, container.CO_Ref_MaxGrossWeight);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			AssertEquals("CO_Ref_MaxGrossWeight", refContainer.RC_GrossWeight, container.CO_Ref_MaxGrossWeight);
		}

		public void TestCO_Ref_MaxGrossWeightInfo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_MaxGrossWeight", container.CO_Ref_MaxGrossWeightInfo.Name);
			AssertEquals("Should default to ReadOnly", true, container.CO_Ref_MaxGrossWeightInfo.ReadOnly);
		}

		#endregion

		#region CO_Ref_TareWeight

		public void TestCO_Ref_TareWeight()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_TareWeight", 0m, container.CO_Ref_TareWeight);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			AssertEquals("CO_Ref_TareWeight", refContainer.RC_TareWeight, container.CO_Ref_TareWeight);
		}

		public void TestCO_Ref_TareWeightInfo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_TareWeight", container.CO_Ref_TareWeightInfo.Name);
			AssertEquals("Should default to ReadOnly", true, container.CO_Ref_TareWeightInfo.ReadOnly);
		}

		#endregion

		#region CO_Ref_Length

		public void TestCO_Ref_Length()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_Length", 0m, container.CO_Ref_Length);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			AssertEquals("CO_Ref_Length", refContainer.RC_Length, container.CO_Ref_Length);
		}

		public void TestCO_Ref_LengthInfo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_Length", container.CO_Ref_LengthInfo.Name);
			AssertEquals("Should default to ReadOnly", true, container.CO_Ref_LengthInfo.ReadOnly);
		}

		#endregion

		#region CO_Ref_Width

		public void TestCO_Ref_Width()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_Width", 0m, container.CO_Ref_Width);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			AssertEquals("CO_Ref_Width", refContainer.RC_Width, container.CO_Ref_Width);
		}

		public void TestCO_Ref_WidthInfo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_Width", container.CO_Ref_WidthInfo.Name);
			AssertEquals("Should default to ReadOnly", true, container.CO_Ref_WidthInfo.ReadOnly);
		}

		#endregion

		#region CO_Ref_Height

		public void TestCO_Ref_Height()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_Height", 0m, container.CO_Ref_Height);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			AssertEquals("CO_Ref_Height", refContainer.RC_Height, container.CO_Ref_Height);
		}

		public void TestCO_Ref_HeightInfo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Ref_Height", container.CO_Ref_HeightInfo.Name);
			AssertEquals("Should default to ReadOnly", true, container.CO_Ref_HeightInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region Calc Properties

		#region CO_Calc_ActualCapacity

		public void TestCO_Calc_ActualCapacity()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Calc_ActualCapacity", 0m, container.CO_Calc_ActualCapacity);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			AssertEquals("CO_Calc_ActualCapacity", Core.Constants.Volume.Convert(container.TotalHeight * container.TotalWidth * container.TotalLength, Core.Constants.Volume.CubicFeet, Core.Constants.Volume.CubicMetres), container.CO_Calc_ActualCapacity);

			container.TotalHeight = 2m;
			container.TotalWidth = 2m;
			container.TotalLength = 2m;
			AssertEquals("CO_Calc_ActualCapacity", Core.Constants.Volume.Convert(8m, Core.Constants.Volume.CubicFeet, Core.Constants.Volume.CubicMetres), container.CO_Calc_ActualCapacity);
		}

		public void TestCO_Calc_ActualCapacityInfo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Calc_ActualCapacity", container.CO_Calc_ActualCapacityInfo.Name);
			AssertEquals("Should default to ReadOnly", true, container.CO_Calc_ActualCapacityInfo.ReadOnly);
		}

		#endregion

		#region CO_Calc_OverhangLength

		public void TestCO_Calc_OverhangLength()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Calc_OverhangLength", 0m, container.CO_Calc_OverhangLength);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			AssertEquals("CO_Calc_OverhangLength", 0m, container.CO_Calc_OverhangLength);

			container.TotalLength = 50m;
			AssertEquals("CO_Calc_ActualCapacity", 50m - refContainer.RC_Length, container.CO_Calc_OverhangLength);
		}

		public void TestCO_Calc_OverhangLengthInfo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Calc_OverhangLength", container.CO_Calc_OverhangLengthInfo.Name);
			AssertEquals("Should default to ReadOnly", true, container.CO_Calc_OverhangLengthInfo.ReadOnly);
		}

		#endregion

		#region CO_Calc_OverhangWidth

		public void TestCO_Calc_OverhangWidth()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Calc_OverhangWidth", 0m, container.CO_Calc_OverhangWidth);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			AssertEquals("CO_Calc_OverhangWidth", 0m, container.CO_Calc_OverhangWidth);

			container.TotalWidth = 50m;
			AssertEquals("CO_Calc_OverhangWidth", 50m - refContainer.RC_Width, container.CO_Calc_OverhangWidth);
		}

		public void TestCO_Calc_OverhangWidthInfo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Calc_OverhangWidth", container.CO_Calc_OverhangWidthInfo.Name);
			AssertEquals("Should default to ReadOnly", true, container.CO_Calc_OverhangWidthInfo.ReadOnly);
		}

		#endregion

		#region CO_Calc_OverhangHeight

		public void TestCO_Calc_OverhangHeight()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Calc_OverhangHeight", 0m, container.CO_Calc_OverhangHeight);

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.CO_RC = refContainer.PK;
			AssertEquals("CO_Calc_OverhangHeight", 0m, container.CO_Calc_OverhangHeight);

			container.TotalHeight = 50m;
			AssertEquals("CO_Calc_OverhangHeight", 50m - refContainer.RC_Height, container.CO_Calc_OverhangHeight);
		}

		public void TestCO_Calc_OverhangHeightInfo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals("CO_Calc_OverhangHeight", container.CO_Calc_OverhangHeightInfo.Name);
			AssertEquals("Should default to ReadOnly", true, container.CO_Calc_OverhangHeightInfo.ReadOnly);
		}

		#endregion

		#region CO_Calc_TotalPackages

		public void TestCO_Calc_TotalPackages()
		{
			var dec = GetJobDeclaration();
			var container1 = dec.CusContainers.AddNew();
			container1.Packages.RemoveAll(x => true);
			AssertEquals(ZInt.Zero, container1.CO_Calc_TotalPackages);
			var bill1 = dec.Bills.AddNew();
			var packingGroup1 = container1.PackingGroups.AddNew();
			packingGroup1.CR_CU_HouseBill = bill1.PK;
			var bill2 = dec.Bills.AddNew();
			var packingGroup2 = container1.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = bill2.PK;
			var package1 = packingGroup1.Packages.AddNew();
			var package2 = packingGroup2.Packages.AddNew();
			var package3 = packingGroup1.Packages.AddNew();
			var package4 = packingGroup2.Packages.AddNew();
			AssertEquals(ZInt.Zero, container1.CO_Calc_TotalPackages);
			package1.CW_PackQty = 10;
			AssertEquals(10, container1.CO_Calc_TotalPackages);
			package2.CW_PackQty = 21;
			AssertEquals(31, container1.CO_Calc_TotalPackages);
			package3.CW_PackQty = 32;
			AssertEquals(63, container1.CO_Calc_TotalPackages);
			package4.CW_PackQty = 43;
			AssertEquals(106, container1.CO_Calc_TotalPackages);

			var container2 = dec.CusContainers.AddNew();
			var packingGroup3 = container2.PackingGroups.AddNew();
			packingGroup3.CR_CU_HouseBill = bill1.PK;
			var packingGroup4 = container2.PackingGroups.AddNew();
			packingGroup4.CR_CU_HouseBill = bill2.PK;

			var package5 = packingGroup3.Packages.AddNew();
			package5.CW_PackQty = 13;
			AssertEquals(106, container1.CO_Calc_TotalPackages);
			AssertEquals(13, container2.CO_Calc_TotalPackages);
			var package6 = packingGroup4.Packages.AddNew();
			package6.CW_PackQty = 45;
			AssertEquals(106, container1.CO_Calc_TotalPackages);
			AssertEquals(58, container2.CO_Calc_TotalPackages);
		}

		#endregion

		#region CO_Calc_TotalPackagesUnit

		public virtual void TestCO_Calc_TotalPackagesUnit()
		{
			var dec = GetJobDeclaration();
			dec.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Roll;
			var container1 = dec.CusContainers.AddNew();
			dec.Packages.RemoveAndDeleteAll();
			AssertEquals("Use Declaration Type", Core.Constants.PkgUnit.Roll, container1.CO_Calc_TotalPackagesUnit);
			container1.CO_JE = ZGuid.Invalid;
			AssertNull(container1.Declaration);
			AssertEquals("No Declaration", ZString.Empty, container1.CO_Calc_TotalPackagesUnit);
			container1.CO_JE = dec.PK;
			var bill = dec.Bills.AddNew();
			var packingGroup1 = container1.PackingGroups.AddNew();
			var package1 = packingGroup1.Packages.AddNew();
			AssertEquals("Use Declaration Type", Core.Constants.PkgUnit.Roll, container1.CO_Calc_TotalPackagesUnit);
			package1.CW_PackQty = 1;
			AssertEquals("Should use default pack type if packQty is not zero", Core.Constants.PkgUnit.Piece, container1.CO_Calc_TotalPackagesUnit);
			package1.CW_PackType = Core.Constants.PkgUnit.Pallet;
			AssertEquals("Use from package1.CW_PackType", Core.Constants.PkgUnit.Pallet, container1.CO_Calc_TotalPackagesUnit);
			var package2 = packingGroup1.Packages.AddNew();
			AssertEquals("Multiple package types", Core.Constants.PkgUnit.Piece, container1.CO_Calc_TotalPackagesUnit);
			package2.CW_PackType = Core.Constants.PkgUnit.Reel;
			AssertEquals("Multiple package types", Core.Constants.PkgUnit.Piece, container1.CO_Calc_TotalPackagesUnit);
			package2.CW_PackType = Core.Constants.PkgUnit.Pallet;
			AssertEquals("all package type are the same", Core.Constants.PkgUnit.Pallet, container1.CO_Calc_TotalPackagesUnit);

			var container2 = dec.CusContainers.AddNew();
			var packingGroup2 = container2.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = bill.PK;
			var package3 = packingGroup2.Packages.AddNew();
			package3.CW_PackType = Core.Constants.PkgUnit.Keg;
			AssertEquals("all package type for this container are the same", Core.Constants.PkgUnit.Pallet, container1.CO_Calc_TotalPackagesUnit);
			AssertEquals(Core.Constants.PkgUnit.Keg, container2.CO_Calc_TotalPackagesUnit);
		}

		#endregion

		#endregion

		#region IContainerExtraParent

		public void TestGoodsWeight()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			container.CO_Weight = 0m;
			AssertEquals("GoodsWeight should be 0", 0m, container.GoodsWeight);

			container.CO_Weight = 17.7m;
			AssertEquals("GoodsWeight should be 17.7", 17.7m, container.GoodsWeight);
		}

		public void TestCO_WeightUpdatesGrossWeight()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			container.TareWeight = 5m;
			container.DunnageWeight = 6m;
			container.CO_Weight = 0m;
			AssertEquals("GrossWeight should be 11", 11m, container.GrossWeight);

			container.CO_Weight = 2m;
			AssertEquals("GrossWeight should be 13", 13m, container.GrossWeight);
		}

		public virtual void TestGoodsWeightUQIsKG()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			AssertEquals(Core.Constants.Weight.Kilograms, container.GoodsWeightUQ);
		}

		#endregion

		public void TestILandedCostDistributeTo()
		{
			var testDec = GetJobDeclaration();
			var container = testDec.CusContainers.AddNew();
			container.CO_ContainerNumber = "CO110022";
			container.CO_FCL_LCL_AIR = "FCL";

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var pivot1ToInvoiceLine1 = container.InvoiceLinePivotCollection.AddNew();
			pivot1ToInvoiceLine1.C2_JI = invoiceLine1.PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("Unique Code", "Container " + container.CO_ContainerNumber, ((ILandedCostDistributeTo)container).UniqueCode);
			AssertEquals("Description", ((ILandedCostDistributeTo)container).UniqueCode, ((ILandedCostDistributeTo)container).Description);
			AssertEquals("PK", container.PK, ((ILandedCostDistributeTo)container).PK);
			AssertEquals("Table Code", "CO", ((ILandedCostDistributeTo)container).TableCode);
			AssertEquals("UltimateDistributees", invoiceLine1, new List<IUltimateDistributee>(((ILandedCostDistributeTo)container).UltimateDistributees)[0]);
		}

		public void TestDeclarationProperty()
		{
			var declaration = GetJobDeclaration();
			var container = (BaseCusContainer)GetNewBusinessObject();
			container.CO_JE = declaration.PK;
			AssertEquals("There should be a Declaration against the container now", declaration, container.Declaration);
		}

		public void TestContainerCount()
		{
			var dec = GetJobDeclaration();
			AssertEquals("no containers on this dec", ZShort.Zero, dec.JE_ContainerCount);

			var container1 = dec.CusContainers.AddNew();
			AssertEquals("Pre-Condition - pre-save: no containers", 1, dec.CusContainers.Count);

			Factory.Save();
			AssertEquals("Pre-Condition - post-save: 1 container", 1, dec.CusContainers.Count);
			AssertEquals("1 container on this dec", 1, (ZInt)dec.JE_ContainerCount);

			var container2 = dec.CusContainers.AddNew();
			Factory.Save();
			AssertEquals("2 containers now on this dec", 2, (ZInt)dec.JE_ContainerCount);

			container1.Delete();
			Factory.Save();
			AssertEquals("1 container only on this dec now", 1, (ZInt)dec.JE_ContainerCount);

			var container3 = dec.CusContainers.AddNew();
			Factory.Save();
			AssertEquals("2 containers again on this dec, container2 & container3", 2, (ZInt)dec.JE_ContainerCount);
		}

		public virtual void TestUpdateETAAndDelivery()
		{
			var defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = "FCL";
			defaultDelay.G1_RL_NKDischargePort = "USNYC";
			defaultDelay.G1_RL_NKDestinationPort = "USCHI";
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 3;

			var defaultDelay2 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay2.G1_FreightMode = "LCL";
			defaultDelay2.G1_RL_NKDischargePort = "USNYC";
			defaultDelay2.G1_RL_NKDestinationPort = "USCHI";
			defaultDelay2.G1_DaysFromDestinationArrivalToClientDelivery = 2;

			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;//US does not have FCL or LCL in Container Mode
			declaration.JE_RL_NKPortOfArrival = "USNYC";
			declaration.JE_RL_NKFinalDestination = "USCHI";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2010, 1, 1);

			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("Delivery defaulted", new ZDateTime(2010, 1, 4), declaration.JE_EstimatedDeliveryOrPickup);

			declaration.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
			declaration.CusContainers.RemoveAndDeleteAll();
			AssertEquals("Delivery defaulted", ZDateTime.Empty, declaration.JE_EstimatedDeliveryOrPickup);

			declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = "LCL";
			AssertEquals("Delivery defaulted", new ZDateTime(2010, 1, 3), declaration.JE_EstimatedDeliveryOrPickup);
		}

		public virtual void TestUpdateETAAndDeliveryWhenContainerIsDeleted()
		{
			var defaultDelay = Factory.New<GlbPortDeliveryTime>();
			defaultDelay.G1_FreightMode = "FCL";
			defaultDelay.G1_RL_NKDischargePort = "USNYC";
			defaultDelay.G1_RL_NKDestinationPort = "USCHI";
			defaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 3;

			var defaultDelay2 = Factory.New<GlbPortDeliveryTime>();
			defaultDelay2.G1_FreightMode = "LCL";
			defaultDelay2.G1_RL_NKDischargePort = "USNYC";
			defaultDelay2.G1_RL_NKDestinationPort = "USCHI";
			defaultDelay2.G1_DaysFromDestinationArrivalToClientDelivery = 2;

			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;//US does not have FCL or LCL in Container Mode
			declaration.JE_RL_NKPortOfArrival = "USNYC";
			declaration.JE_RL_NKFinalDestination = "USCHI";
			declaration.JE_DateAtFinalDestination = new ZDateTime(2010, 1, 1);

			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = "FCL";
			declaration.CusContainers.AddNew().CO_FCL_LCL_AIR = "LCL";

			declaration.JE_EstimatedDeliveryOrPickup = ZDateTime.Empty;
			declaration.CusContainers.RemoveAndDelete(container);
			AssertEquals("Delivery defaulted", new ZDateTime(2010, 1, 3), declaration.JE_EstimatedDeliveryOrPickup);
		}

		public void TestCusContainerInvoiceLinePivotCollection()
		{
			var container = (BaseCusContainer)GetNewBusinessObject();
			var pivot = Factory.New<CusContainerInvoiceLinePivot>();
			pivot.C2_CO = container.PK;

			CusContainerInvoiceLinePivotCollection invoiceLinePivotCollection = new CusContainerInvoiceLinePivotCollection(container, Factory);
			AssertNotNull("", container.InvoiceLinePivotCollection);
			AssertEquals("Collection Count", 1, container.InvoiceLinePivotCollection.Count);
			var pivotFromCollection = container.InvoiceLinePivotCollection[0];
			AssertEquals("Make sure we have matching objects", pivot.PK, pivotFromCollection.PK);
		}

		public void TestICartageContainer_JobContainerPK()
		{
			var cont = Factory.New<BaseCusContainer>();
			ForwardingContainer fordCon = Factory.New<ForwardingContainer>();

			cont.CO_JC = fordCon.PK;

			AssertEquals(fordCon.PK, ((ICartageContainer)cont).JobContainerPK);
		}

		public virtual void TestContainerValidation()
		{
			var dec = GetJobDeclaration();
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			var container = (BaseCusContainer)GetNewBusinessObject();
			container.CO_JE = dec.PK;
			AssertNoNotifications("Precondion", container.CO_ContainerNumberInfo);
			container.CO_ContainerNumber = "1234";
			Assert("Invalid Container Number", container.CO_ContainerNumberInfo.HasNotifications());
		}

		public virtual void TestContainerNumber_CO_JE_UniqueIndex()
		{
			var dec = GetJobDeclaration();
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			var container1 = (BaseCusContainer)GetNewBusinessObject();
			var container2 = (BaseCusContainer)GetNewBusinessObject();
			container1.CO_ContainerNumber = ContainerNumberForTesting;
			container1.CO_JE = dec.PK;
			container2.CO_JE = dec.PK;
			container2.CO_ContainerNumber = ContainerNumberForTesting;
			AssertEquals("Container 2 should have duplicate container number Error", true, container2.CO_ContainerNumberInfo.HasErrors());
		}

		public void TestDefaultContainerDimensionsFromRefContainer()
		{
			var cusContainer = Factory.New<BaseCusContainer>();
			RefContainer refContainer20RE = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			cusContainer.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			cusContainer.CO_RC = refContainer20RE.PK;
			AssertEquals("Width should use RefContainer20RE value", refContainer20RE.RC_Width, cusContainer.TotalWidth);
			AssertEquals("Height should use RefContainer20RE value", refContainer20RE.RC_Height, cusContainer.TotalHeight);
			AssertEquals("Length should use RefContainer20RE value", refContainer20RE.RC_Length, cusContainer.TotalLength);
			AssertEquals("TareWeight should use RefContainer20RE value", refContainer20RE.RC_TareWeight, cusContainer.TareWeight);

			RefContainer refContainer40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			cusContainer.CO_RC = refContainer40GP.PK;
			AssertEquals("Width should use RefContainer40GP value", refContainer40GP.RC_Width, cusContainer.TotalWidth);
			AssertEquals("Height should use RefContainer40GP value", refContainer40GP.RC_Height, cusContainer.TotalHeight);
			AssertEquals("Length should use RefContainer40GP value", refContainer40GP.RC_Length, cusContainer.TotalLength);
			AssertEquals("TareWeight should use RefContainer40GP value", refContainer40GP.RC_TareWeight, cusContainer.TareWeight);
		}

		public void TestISupportDataImporting()
		{
			BusinessObject testObjectToImport = (BaseCusContainer)GetNewBusinessObject();
			Assert("Woolies importer requires the class to support ISupportDataImporting", testObjectToImport is ISupportDataImporting);
			((ISupportDataImporting)testObjectToImport).IsImportingData = true;
			AssertEquals("Importing should be set to true", true, ((ISupportDataImporting)testObjectToImport).IsImportingData);
			((ISupportDataImporting)testObjectToImport).IsImportingData = false;
			AssertEquals("Importing should be set to false", false, ((ISupportDataImporting)testObjectToImport).IsImportingData);
		}

		public void TestContainerCase()
		{
			var container = (BaseCusContainer)GetNewBusinessObject();
			container.CO_ContainerNumber = "crxu1234567";
			AssertEquals("Lowercase should be forced to uppercase", "CRXU1234567", container.CO_ContainerNumber);
		}

		public void TestSealCase()
		{
			var container = (BaseCusContainer)GetNewBusinessObject();
			container.CO_Seal = "sealnumber";
			AssertEquals("Lowercase should be forced to uppercase", "SEALNUMBER", container.CO_Seal);
		}

		public void TestClone()
		{
			var container = (BaseCusContainer)GetNewBusinessObject();
			container.CO_Seal = "snumber";
			container.CO_ContainerNumber = "cnumber";
			var clonedContainer = (BaseCusContainer)container.Clone();
			AssertEquals("Seal Number", container.CO_Seal, clonedContainer.CO_Seal);
			AssertEquals("CO_ContainerNumber", container.CO_ContainerNumber, clonedContainer.CO_ContainerNumber);
			AssertEquals("Has changes is false", false, clonedContainer.HasChanges);
		}

		public void TestCloneHasChanges()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "cnumber";

			var clonedDeclaration = declaration.GetNewRelatedDeclaration(Factory);
			AssertEquals("CO_ContainerNumber", declaration.CusContainers.Count, clonedDeclaration.CusContainers.Count);
			AssertEquals("Has changes is false", false, clonedDeclaration.CusContainers.HasChanges);
		}

		public virtual void TestDefaultingCO_WeightUQ()
		{
			var container = Factory.New<BaseCusContainer>();
			AssertEquals(Core.Constants.Weight.Kilograms, container.CO_WeightUQ);
		}

		public void TestPackingGroupsAndPackages()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();
			var houseBill = declaration.Bills.AddNew();
			var packingGroup1 = houseBill.PackingGroups.AddNew();
			packingGroup1.CR_CO_Container = container.PK;
			var packingGroup2 = houseBill.PackingGroups.AddNew();
			packingGroup2.CR_CO_Container = container2.PK;

			AssertEquals("PackingGroups for container", true, container.PackingGroups.Contains(packingGroup1));
			AssertEquals("PackingGroups for container", false, container.PackingGroups.Contains(packingGroup2));

			var package1 = packingGroup1.Packages.AddNew();
			var package2 = packingGroup2.Packages.AddNew();

			AssertEquals("Packages for container", true, container.Packages.Contains(package1));
			AssertEquals("Packages for container", false, container.Packages.Contains(package2));
		}

		public void TestModeConverter()
		{
			var container = Factory.New<BaseCusContainer>();
			AssertEquals(Enterprise.Core.Constants.ContainerModes.BuyersConsol, container.ModeConverter.ConvertCustomsToFreight(Enterprise.Core.Constants.ContainerModes.FCLMixedShipper));

			AssertEquals(Enterprise.Core.Constants.ContainerModes.LCL, container.ModeConverter.ConvertFreightToCustoms(Enterprise.Core.Constants.ContainerModes.Groupage));

			AssertEquals(Enterprise.Core.Constants.ContainerModes.LCL, container.ModeConverter.ConvertFreightToCustomsForSCNContainer(true, true, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(Enterprise.Core.Constants.ContainerModes.LCL, container.ModeConverter.ConvertFreightToCustomsForSCNContainer(true, false, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(Enterprise.Core.Constants.ContainerModes.FCL, container.ModeConverter.ConvertFreightToCustomsForSCNContainer(false, true, Core.Constants.CountryCodes.UnitedStates));
			AssertEquals(Enterprise.Core.Constants.ContainerModes.FCL, container.ModeConverter.ConvertFreightToCustomsForSCNContainer(false, false, Core.Constants.CountryCodes.UnitedStates));

			AssertEquals(Enterprise.Core.Constants.ContainerModes.LCL, container.ModeConverter.ConvertFreightToCustomsForSCNContainer(true, true, Core.Constants.CountryCodes.Australia));
			AssertEquals(Enterprise.Core.Constants.ContainerModes.LCL, container.ModeConverter.ConvertFreightToCustomsForSCNContainer(true, false, Core.Constants.CountryCodes.Australia));
			AssertEquals(Enterprise.Core.Constants.ContainerModes.LCL, container.ModeConverter.ConvertFreightToCustomsForSCNContainer(false, true, Core.Constants.CountryCodes.Australia));
			AssertEquals(Enterprise.Core.Constants.ContainerModes.FCL, container.ModeConverter.ConvertFreightToCustomsForSCNContainer(false, false, Core.Constants.CountryCodes.Australia));
		}

		#region IDocumentSupportable
		public void TestDocumentSupporter()
		{
			var container = Factory.New<BaseCusContainer>();
			AssertEquals("DocumentSupporter should be of type CusContainerDocumentSupporter", typeof(CusContainerDocumentSupporter).ToString(), container.DocumentSupporter.GetType().ToString());
		}
		#endregion

		public virtual void TestNewContainerLoadsInOtherFactory()
		{
			var mockDec = Factory.NewMoq<TJobDeclaration>();
			mockDec.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			var declaration = mockDec.Object;
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "NOR";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = "CNT";
			declaration.JE_HouseBill = "HOUSEBILL";
			declaration.JE_TotalNoOfPacks = 123;
			declaration.JE_TotalNoOfPacksPackType = "AE";
			Factory.Save();
			declaration.JE_VoyageFlightNo = "111";
			Factory.Save();
			var containers = new BaseCusContainerCollection<BaseCusContainer>(declaration, Factory);
			var container1 = containers.AddNew();
			container1.CO_ContainerNumber = "OCLU1111110";
			container1.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var packingGroup = declaration.PackingGroups[0];
			Factory.Save();
			packingGroup.Packages[0].CW_PackType = "AE";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var packageGroupLoaded = factory2.Load<BasePackingGroup>(packingGroup.PK);
			AssertNotNull(packageGroupLoaded);
			AssertEquals("Container should be in DB", packingGroup.CR_CO_Container, packageGroupLoaded.CR_CO_Container);
		}

		public virtual void TestDefaultPackingInformationIfNeeded()
		{
			var mockDec = Factory.NewMoq<TJobDeclaration>();
			mockDec.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			var relevantDec = mockDec.Object;
			SetupDeclaration(relevantDec);

			AssertEquals(1, relevantDec.PackingGroups.Count);
			var packGroup = relevantDec.PackingGroups[0];
			AssertNull(packGroup.Container);

			var container1 = relevantDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU1111110";

			AssertSame("Associates new container with existing pack group", container1, packGroup.Container);
			AssertEquals("Associates new container with House Bill", "HOUSEBILL", packGroup.Bill.CU_HouseBill);
			AssertEquals(1, relevantDec.PackingGroups.Count);
			AssertEquals(1, relevantDec.PackingInformationCollection.Count);

			var container2 = relevantDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OCLU2222220";
			AssertEquals(2, relevantDec.PackingGroups.Count);
			AssertEquals(2, relevantDec.PackingInformationCollection.Count);
			var billContainerPivot = relevantDec.PackingInformationCollection.GetElement(1).HouseBillContainer;
			AssertSame("Creates new Pack Group to House Bill for container", container2, billContainerPivot.Container);
			AssertEquals("Associates new container with House Bill", "HOUSEBILL", billContainerPivot.HouseBill.CU_HouseBill);

			packGroup.Packages.RemoveAndDeleteAll();
			packGroup.CR_CO_Container = ZGuid.Empty;
			AssertEquals(2, relevantDec.PackingGroups.Count);

			var container3 = relevantDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OCLU3333330";
			AssertSame("Associates new container with empty pack group.", container3, packGroup.Container);
			AssertEquals("Has a default pack.", 1, packGroup.Packages.Count);
			AssertEquals(2, relevantDec.PackingGroups.Count);
			AssertEquals(2, relevantDec.PackingInformationCollection.Count);

			var mockDec2 = Factory.NewMoq<TJobDeclaration>();
			mockDec2.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(false);
			var irrelevantDec = mockDec2.Object;
			SetupDeclaration(irrelevantDec);
			AssertEquals(0, irrelevantDec.PackingGroups.Count);

			var container = irrelevantDec.CusContainers.AddNew();
			container.CO_ContainerNumber = "OCLU0000000";
			AssertEquals(0, irrelevantDec.PackingGroups.Count);
			AssertEquals(0, irrelevantDec.PackingInformationCollection.Count);
		}

		void SetupDeclaration(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "NOR";
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = "CNT";
			declaration.JE_HouseBill = "HOUSEBILL";
			declaration.JE_TotalNoOfPacks = 123;
			declaration.JE_TotalNoOfPacksPackType = "AE";
			declaration.JE_VoyageFlightNo = "111";
		}

		public void TestIdleWorkerDoesNotCorruptGrossWeight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			container.TareWeight = 5m;
			container.DunnageWeight = 6m;
			container.CO_Weight = 7m;
			AssertEquals("GrossWeight should be 18", 18m, container.GrossWeight);
			PackLineManyToManyCollection freightPackLines = container.JobContainer.PackLines;
			var freightPack = Factory.New<PackLine>();
			freightPack.JL_FreightMode = FreightConstants.DeliveryPackType;
			freightPackLines.Add(freightPack);
			AssertEquals("GrossWeight should still be 18", 18m, container.GrossWeight);
		}

		public void TestSyncroniseNewJobContainerDoesNotCauseEnumerationProblem()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234567";
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			var testDec = GetJobDeclaration();
			testDec.JE_TransportMode = testDec.TransportModeSeaCodeForTesting;
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			testDec.JE_JS = shipment.PK;
			testDec.ShipmentSynchroniser.SetEnabled(true, false);

			testDec.ShipmentSynchroniser.LoadJobContainerForTesting = true;// this is to simulate on list changed refresh of declaration container tab (if exposed)
			testDec.ShipmentSynchroniser.Synchronise();
			shipment.JS_ActualWeight = 1m;
			Assert("No developer error should be reported", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestSettingGoodsWeight()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234567";

			var shipment1 = (CommonShipment)consol.Shipments.AddNew();
			var testDec1 = GetJobDeclaration();
			testDec1.JE_TransportMode = testDec1.TransportModeSeaCodeForTesting;
			testDec1.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			testDec1.JE_JS = shipment1.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, testDec1);

			var shipment2 = (CommonShipment)consol.Shipments.AddNew();
			var testDec2 = GetJobDeclaration();
			testDec2.JE_TransportMode = testDec2.TransportModeSeaCodeForTesting;
			testDec2.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			testDec2.JE_JS = shipment2.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, testDec2);

			var line1 = shipment1.OuterPackLines.AddNew();
			container.PackLines.Add(line1);
			line1.JL_ActualWeight = 100m;
			line1.JL_ActualWeightUQ = "LB";
			var line2 = shipment2.OuterPackLines.AddNew();
			container.PackLines.Add(line2);
			line2.JL_ActualWeight = 200m;
			line2.JL_ActualWeightUQ = "KG";
			var line3 = shipment1.OuterPackLines.AddNew();
			container.PackLines.Add(line3);
			line3.JL_ActualWeight = 300m;
			line3.JL_ActualWeightUQ = "KG";
			var line4 = shipment2.OuterPackLines.AddNew();
			container.PackLines.Add(line2);
			line4.JL_ActualWeight = 400m;
			line4.JL_ActualWeightUQ = "KG";

			var cusContainer1 = testDec1.CusContainers.AddNew();
			cusContainer1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			cusContainer1.CO_JC = container.PK;
			cusContainer1.CO_ContainerNumber = "CONT1234567";

			var cusContainer2 = testDec2.CusContainers.AddNew();
			cusContainer2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			cusContainer2.CO_JC = container.PK;
			cusContainer2.CO_ContainerNumber = "CONT1234567";

			ZDecimal goodsWeightInKGs = Core.Constants.Weight.Convert(cusContainer1.CO_Weight, cusContainer1.CO_WeightUQ, Core.Constants.Weight.Kilograms);
			AssertEquals("CusContainer 1", 345m, goodsWeightInKGs.Round(0));
			goodsWeightInKGs = Core.Constants.Weight.Convert(cusContainer2.CO_Weight, cusContainer2.CO_WeightUQ, Core.Constants.Weight.Kilograms);
			AssertEquals("CusContainer 2", 600m, goodsWeightInKGs.Round(0));
		}

		public virtual void TestFindContainerOnShipmentByContainerNumber()
		{
			var port1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var port2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new ZString[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode }));
			var port3 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new ZString[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode, port2.RL_RN_NKCountryCode }));
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = port1.RL_Code;
			shipment.JS_RL_NKDestination = port3.RL_Code;

			var consol1 = (ForwardingConsol)shipment.Consols.AddNew(typeof(ForwardingConsol));
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = port2.RL_Code;
			consol1.JK_RL_NKDischargePort = localPort.RL_Code;
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1234567";

			var consol2 = (ForwardingConsol)shipment.Consols.AddNew(typeof(ForwardingConsol));
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = localPort.RL_Code;
			consol2.JK_RL_NKDischargePort = port3.RL_Code;
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT8901234";

			var line1 = shipment.OuterPackLines.AddNew();
			container1.PackLines.Add(line1);
			line1.JL_ActualWeight = 100m;
			line1.JL_ActualWeightUQ = "LB";

			var line2 = shipment.OuterPackLines.AddNew();
			container2.PackLines.Add(line2);
			line2.JL_ActualWeight = 150m;
			line2.JL_ActualWeightUQ = "LB";

			Factory.Save(); // Stop JobContainer from being deleted
			var dec = GetJobDeclaration();
			dec.JE_JS = shipment.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(1, dec.CusContainers.Count);
			var cusContainer = dec.CusContainers[0];
			AssertEquals(container1, cusContainer.JobContainer);
			AssertEquals("CONT1234567", cusContainer.CO_ContainerNumber);
			AssertEquals(1, consol1.Containers.Count);
			AssertEquals(1, consol2.Containers.Count);

			var cusContainer2 = dec.CusContainers.AddNew();
			cusContainer2.CO_ContainerNumber = "ABC1";
			var container3 = cusContainer2.JobContainer;
			AssertEquals("ABC1", container3.JC_ContainerNum);
			cusContainer.CO_ContainerNumber = "CONT8901234";
			AssertNotEquals(container2, cusContainer.JobContainer);
			AssertEquals(container1, cusContainer.JobContainer);
			AssertEquals("CONT8901234", container1.JC_ContainerNum);
			AssertEquals(true, cusContainer.JobContainer.IsInDatabase);
			AssertEquals(2, consol1.Containers.Count);
			AssertCollectionContains(container3, consol1.Containers);
			AssertEquals(1, consol2.Containers.Count);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(1, dec.CusContainers.Count);
			cusContainer = dec.CusContainers[0];
			AssertEquals("CONT8901234", cusContainer.CO_ContainerNumber);
			AssertEquals(container2, cusContainer.JobContainer);
			AssertEquals("CONT8901234", container2.JC_ContainerNum);
			AssertNotEquals(container1, cusContainer.JobContainer);
			AssertEquals(1, consol1.Containers.Count);
			AssertEquals(1, consol2.Containers.Count);
			AssertEquals(true, container3.IsDeleted);
		}

		public void TestCalculateGoodsWeightFromShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var containerForConsol = consol.Containers.AddNew();
			containerForConsol.JC_ContainerNum = "CONL0000001";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";

			var packLineForShipment = shipment.OuterPackLines.AddNew();
			packLineForShipment.JL_JC = containerForConsol.PK;
			packLineForShipment.JL_ActualWeight = 10000m;
			packLineForShipment.JL_ActualVolume = 100m;

			Factory.Save();

			packLineForShipment.JL_ActualWeightUQ = "1";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_JC = containerForConsol.PK;
			cusContainer.CO_WeightUQ = Core.Constants.Weight.Tonnes;
			cusContainer.CO_ContainerNumber = "CONT1234567";

			AssertEquals(0m, cusContainer.CO_Weight);

			packLineForShipment.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			cusContainer.CO_ContainerNumber = "CONT1234568";

			AssertEquals(10m, cusContainer.CO_Weight);

			cusContainer.CO_WeightUQ = "1";
			cusContainer.CO_ContainerNumber = "CONT1234569";

			AssertEquals(0m, cusContainer.CO_Weight);
		}

		public void TestGrossWeightForBinding()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.GrossWeightForBinding = 100;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CNSHA";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_JC = container.PK;
			cusContainer.CO_WeightUQ = Core.Constants.Weight.Tonnes;

			AssertEquals("Precondition", 100M, cusContainer.GrossWeightForBinding);

			cusContainer.GrossWeightForBinding = 50;
			AssertEquals("Value is not changed as not a standalone container", 100m, cusContainer.GrossWeightForBinding);
			AssertEquals("Not changed in JC_GrossWeight either", 100m, container.JC_GrossWeight);
		}

		public void TestGrossWeightForBinding_StandaloneContainer()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.JobContainer.GrossWeightForBinding = 55;

			AssertEquals("Precondition", 55M, cusContainer.GrossWeightForBinding);

			cusContainer.GrossWeightForBinding = 100;
			AssertEquals("Can set Gross Weight for Binding for Standalone container", 100m, cusContainer.GrossWeightForBinding);
			AssertEquals("Value is also set in JC_GrossWeight", 100m, cusContainer.JobContainer.JC_GrossWeight);
		}

		public void TestGetContainerModeFromFreight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var cusContainer = declaration.CusContainers.AddNew();
			AssertEquals(Core.Constants.ContainerModes.FCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.FCL));
			AssertEquals(Core.Constants.ContainerModes.LCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.LCL));
			AssertEquals(Core.Constants.ContainerModes.LCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.Groupage));
		}

		public void TestGivenSingleShipmentPackedIntoSCNContainer_WhenConvert_ThenContainerModeShouldBeConvertedByDirectionAndCountry()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234567";

			var shipment = (CommonShipment)consol.Shipments.AddNew();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.ShippersConsol;
			var testDec = GetJobDeclaration();
			testDec.JE_TransportMode = testDec.TransportModeSeaCodeForTesting;
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.ShippersConsol;
			testDec.JE_JS = shipment.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, testDec);

			var line = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(line);
			line.JL_ActualWeight = 100m;
			line.JL_ActualWeightUQ = "LB";

			var cusContainer = testDec.CusContainers.AddNew();
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.ShippersConsol;
			cusContainer.CO_JC = container.PK;
			cusContainer.CO_ContainerNumber = "CONT1234567";

			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("When it is import direction in AU, then container mode should be LCL", Core.Constants.ContainerModes.LCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.ShippersConsol));
			}

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("When it is export direction in AU, then container mode should be FCL", Core.Constants.ContainerModes.FCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.ShippersConsol));
			}

			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				AssertEquals("When it is import direction in non AU, then container mode should be FCL", Core.Constants.ContainerModes.FCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.ShippersConsol));
			}

			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "NZAKL";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				AssertEquals("When it is export direction in non AU, then container mode should be FCL", Core.Constants.ContainerModes.FCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.ShippersConsol));
			}
		}

		public void TestGivenMultiShipmentPackedIntoSCNContainer_WhenConvert_ThenContainerModeShouldBeConvertedByDirectionAndCountry()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234567";

			var shipment1 = (CommonShipment)consol.Shipments.AddNew();
			shipment1.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			var testDec1 = GetJobDeclaration();
			testDec1.JE_TransportMode = testDec1.TransportModeSeaCodeForTesting;
			testDec1.JE_ContainerMode = Core.Constants.ContainerModes.ShippersConsol;
			testDec1.JE_JS = shipment1.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, testDec1);

			var shipment2 = (CommonShipment)consol.Shipments.AddNew();
			shipment2.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			var testDec2 = GetJobDeclaration();
			testDec2.JE_TransportMode = testDec2.TransportModeSeaCodeForTesting;
			testDec2.JE_ContainerMode = Core.Constants.ContainerModes.ShippersConsol;
			testDec2.JE_JS = shipment2.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, testDec2);

			var line1 = shipment1.OuterPackLines.AddNew();
			container.PackLines.Add(line1);
			line1.JL_ActualWeight = 100m;
			line1.JL_ActualWeightUQ = "LB";

			var line2 = shipment2.OuterPackLines.AddNew();
			container.PackLines.Add(line2);
			line2.JL_ActualWeight = 100m;
			line2.JL_ActualWeightUQ = "LB";

			var cusContainer = testDec1.CusContainers.AddNew();
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.ShippersConsol;
			cusContainer.CO_JC = container.PK;
			cusContainer.CO_ContainerNumber = "CONT1234567";

			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("When it is import direction in AU, then container mode should be LCL", Core.Constants.ContainerModes.LCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.ShippersConsol));
			}

			shipment1.JS_PackingMode = Core.Constants.ContainerModes.ShippersConsol;
			shipment2.JS_PackingMode = Core.Constants.ContainerModes.ShippersConsol;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("When it is import direction in AU, then container mode should be LCL", Core.Constants.ContainerModes.LCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.ShippersConsol));
			}

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("When it is export direction in AU, then container mode should be LCL", Core.Constants.ContainerModes.LCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.ShippersConsol));
			}

			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				AssertEquals("When it is import direction in non AU, then container mode should be LCL", Core.Constants.ContainerModes.LCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.ShippersConsol));
			}

			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "NZAKL";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				AssertEquals("When it is export direction in non AU, then container mode should be LCL", Core.Constants.ContainerModes.LCL, cusContainer.GetContainerModeFromFreight(Core.Constants.ContainerModes.ShippersConsol));
			}
		}

		public virtual void TestHumanReadableName()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var cusContainer = declaration.CusContainers.AddNew();

			cusContainer.CO_ContainerNumber = ContainerNumberForTesting;
			AssertEquals($"Container '{cusContainer.CO_ContainerNumber}'", cusContainer.HumanReadableName);
		}

		public virtual void TestCO_ContainerNumber_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<BaseCusContainer>().CO_ContainerNumberInfo);
			AssertEquals("Container Number", resourceStringDataAttribute.Caption);
		}

		public virtual void TestContainerNumberForBinding_Caption()
		{
			CombineAssertions(() =>
			{
				var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<BaseCusContainer>().ContainerNumberForBindingInfo);
				AssertEquals("Container", resourceStringDataAttribute.Caption);
				AssertEquals("Container Number", resourceStringDataAttribute.FullDescription);
			});
		}

		public virtual void TestCO_Seal_Caption()
		{
			var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(Factory.New<BaseCusContainer>().CO_SealInfo);
			AssertEquals("Seal Number", resourceStringDataAttribute.Caption);
		}

		public void TestGlobalSearchBusinessObjectProvider()
		{
			var cusContainer = Factory.New<BaseCusContainer>();
			AssertEquals(true, cusContainer is IGlobalSearchBusinessObjectProvider);
			AssertEquals(cusContainer.Declaration, ((IGlobalSearchBusinessObjectProvider)cusContainer).BusinessObjectForController);
		}

		public void TestSave_DoesNotCreateJobContainer_WhenCusContainerIsDeleted()
		{
			var declaration = GetJobDeclaration();
			declaration.FillWithValidTestData();
			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "CN0001";

			var jobContainer = cusContainer.JobContainer;
			CombineAssertions("[PreCondition]:", () =>
			{
				AssertNotNull("JobContainer is not null", jobContainer);
				Assert("JobContainer has a valid PK", jobContainer.PK.IsValid);
			});

			cusContainer.Delete();

			CombineAssertions("[CusContainer Deleted]:", () =>
			{
				Assert("CusContainer should be deleted", cusContainer.IsDeleted);
				AssertNull("When CusContainer is deleted, JobContainer should be null", cusContainer.JobContainer);
			});

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var jobContainer1InFactory2 = factory2.Load<BaseCusContainer>(jobContainer.PK);

			CombineAssertions(() =>
			{
				Assert("CusContainer should be deleted", cusContainer.IsDeleted);
				AssertNull("JobContainer should be null", cusContainer.JobContainer);
				AssertNull("A JobContainer should NOT be created", jobContainer1InFactory2);
			});
		}

		#region Implementation

		protected const string ContainerNumberForTesting = "GCFU0000019";

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = GetJobDeclaration();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var result = declaration.CusContainers.AddNew();
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = GetJobDeclaration(factory);
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			var result = declaration.CusContainers.AddNew();
			var bill = result.Declaration.Bills.AddNew();
			var packGroup = bill.PackingGroups.AddNew();
			packGroup.CR_CO_Container = result.PK;

			TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(result.Factory);
			return result;
		}

		protected BaseJobDeclaration GetJobDeclaration() => GetJobDeclaration(Factory);

		protected virtual BaseJobDeclaration GetJobDeclaration(BusinessObjectFactory factory)
		{
			var declaration = BaseJobDeclaration.New(factory);
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			return declaration;
		}

		#endregion
	}
}
