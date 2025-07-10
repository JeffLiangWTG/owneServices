using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using IContainer = Enterprise.Customs.US.AMS.Messaging.Interface.IContainer;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondContainer))]
	sealed class CusInBondContainerTest : Customs.Business.Testing.CusInBondContainerTest<CusInBondContainer>
	{
		public void TestForeignPortKCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "12345", "port1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var billContainer = bill.MovementDetail.Containers.AddNew();

			billContainer.BC_ForeignPortKCode = "12345";

			var foreignPortKCode = billContainer.ForeignPortKCode;
			CombineAssertions(() =>
			{
				AssertNotNull(foreignPortKCode);
				AssertEquals("port1", foreignPortKCode.ZZD_Description);
				AssertSame("Cached", Factory.GetCachedValue<ZZRefCusCodeListCombined>("CusInBondContainer|12345", () => null), foreignPortKCode);
			});
		}

		public void TestUpdatingContainerDetailSuspended()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var anotherContainer = header.MovementHeader.MovementDetails.AddNew().Containers.AddNew();
			var billContainer = bill.MovementDetail.Containers.AddNew();

			anotherContainer.BC_ContainerNum = "ABCD123456";
			anotherContainer.BC_TypeOfService = ServiceTypeList.Codes.HouseToHouse;
			billContainer.BC_TypeOfService = ServiceTypeList.Codes.BreakBulk;

			billContainer.BC_ContainerNum = "ABCD123456";

			AssertEquals("Pre-condition: Service type was updated", ServiceTypeList.Codes.HouseToHouse, billContainer.BC_TypeOfService);

			billContainer.BC_TypeOfService = ServiceTypeList.Codes.BreakBulk;
			billContainer.BC_ContainerNum = "EFGH654321";

			using (header.MovementHeader.SuspendUpdatingContainerDetail())
			{
				billContainer.BC_ContainerNum = "ABCD123456";
				AssertEquals("Service type was not updated because UpdateContainerDetails() is suspended", ServiceTypeList.Codes.BreakBulk, billContainer.BC_TypeOfService);
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteContainerAlsoDeleteVehicles()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var container = bill.MovementDetail.Containers.AddNew();
			var vehicle = container.Vehicles.AddNew();
			vehicle.FillWithValidTestData();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			container = anotherFactory.Load<CusInBondContainer>(container.PK);
			container.Delete();
			anotherFactory.Save();
		}

		public void TestIContainerHazardousMaterials()
		{
			var (_, container) = GetHeaderAndContainer(Factory);
			IContainer icontainer = container;

			var hazardousMaterials = new List<IHazardousMaterial>(icontainer.HazardousMaterials);
			AssertEquals(0, hazardousMaterials.Count);

			var undg1 = container.UNDGs.AddNew();
			undg1.DI_DGFlashPoint = 23m;
			hazardousMaterials = new List<IHazardousMaterial>(icontainer.HazardousMaterials);
			AssertEquals(1, hazardousMaterials.Count);
			AssertEquals(23m, hazardousMaterials[0].FlashPointTemp);
		}

		public void TestICanDeleteMembers()
		{
			var (header, container) = GetHeaderAndContainer(Factory);
			AssertEquals(true, ((ICanDelete)container).CanDelete);

			var consol = Factory.New<ForwardingConsol>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			AssertEquals(false, ((ICanDelete)container).CanDelete);
			AssertEquals(CusInBondContainer.ReasonForCannotDelete, ((ICanDelete)container).ReasonForNotAbleToDelete);

			header.BH_OverrideFreightDefaults = true;
			AssertEquals(true, ((ICanDelete)container).CanDelete);
		}

		public void TestLookups()
		{
			var (_, container) = GetHeaderAndContainer(Factory);
			AssertEquals(typeof(CusInBondContainerLookups), container.Lookups.GetType());
		}

		public void TestValidation()
		{
			var (_, container) = GetHeaderAndContainer(Factory);
			AssertEquals(typeof(CusInBondContainerValidation), container.Validation.GetType());
		}

		public void TestIVehicle_IVINOrEmptyContainerMembers()
		{
			var (_, container) = GetHeaderAndContainer(Factory);
			var helper = new MasterFilesTestHelper(Factory);
			container.BC_RL_NKForeignPort = helper.AUSYD.RL_Code;
			IVINOrEmptyContainer data = container;
			AssertEquals("", data.VIN);
			AssertEquals(MasterFilesTestHelper.AUSYDScheduleDOrK, data.ForeignPort);
			AssertEquals("", data.FactoryCarOrderNumber);

			var vehicle = container.Vehicles.AddNew();
			vehicle.BV_VIN = "GT001";

			IACEContainer iACEContainer = container;
			AssertEquals(1, iACEContainer.VehicleDetails.ToList().Count);
		}

		public void TestContainerDetailsDefaulting()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeader;
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			var containerType = Factory.New<RefContainer>();
			containerType.SetCountrySpecificContainerCode("40", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container.BC_ContainerNum = "TURE2122123";
			container.BC_RC = containerType.PK;
			container.BC_Seal1 = "SL123";
			container.BC_Seal2 = "SL456";
			container.BC_TypeOfService = ServiceTypeList.Codes.PierToPier;
			container.BC_IsEmpty = ZBool.True;
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			var container2 = moveDetail2.Containers.AddNew();
			container2.BC_ContainerNum = "TURE2122123";
			AssertEquals(containerType.PK, container2.BC_RC);
			AssertEquals("SL123", container2.BC_Seal1);
			AssertEquals("SL456", container2.BC_Seal2);
			AssertEquals(ServiceTypeList.Codes.PierToPier, container2.BC_TypeOfService);
			AssertEquals(ZBool.True, container2.BC_IsEmpty);

			container2.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			AssertEquals(ZGuid.Empty, container2.BC_RC);
			AssertEquals(ZString.Empty, container2.BC_Seal1);
			AssertEquals(ZString.Empty, container2.BC_Seal2);
			AssertEquals(ZString.Empty, container2.BC_TypeOfService);
			AssertEquals(ZBool.False, container2.BC_IsEmpty);
		}

		public void TestClearWhenIsEmptyIsFalse()
		{
			var (_, container) = GetHeaderAndContainer(Factory);
			container.BC_IsEmpty = ZBool.True;
			container.BC_RL_NKForeignPort = "AUSYD";
			container.BC_ForeignPortKCode = "66666";

			container.BC_IsEmpty = false;
			AssertEquals("It becomes readonly and should clear the value", ZString.Empty, container.BC_RL_NKForeignPort);
			AssertEquals("It becomes readonly and should clear the value", ZString.Empty, container.BC_ForeignPortKCode);
		}

		public void TestReadOnlyFields()
		{
			var (_, container) = GetHeaderAndContainer(Factory);
			var containerType = Factory.New<RefContainer>();
			containerType.SetCountrySpecificContainerCode("40", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container.BC_ContainerNum = ZString.Empty;
			container.BC_RC = containerType.PK;
			container.BC_Seal1 = "SL123";
			container.BC_Seal2 = "SL456";
			container.BC_IsEmpty = ZBool.True;
			container.BC_RL_NKForeignPort = "AUSYD";
			container.BC_TypeOfService = ServiceTypeList.Codes.HeadloadOrDevanning;
			AssertEquals(false, container.BC_RCInfo.ReadOnly);
			AssertEquals(false, container.BC_Seal1Info.ReadOnly);
			AssertEquals(false, container.BC_Seal2Info.ReadOnly);
			AssertEquals(false, container.BC_IsEmptyInfo.ReadOnly);
			AssertEquals(false, container.BC_RL_NKForeignPortInfo.ReadOnly);
			AssertEquals(false, container.BC_TypeOfServiceInfo.ReadOnly);

			container.BC_ContainerNum = "TURE2342322";
			AssertEquals(false, container.BC_RCInfo.ReadOnly);
			AssertEquals(false, container.BC_Seal1Info.ReadOnly);
			AssertEquals(false, container.BC_Seal2Info.ReadOnly);
			AssertEquals(containerType.PK, container.BC_RC);
			AssertEquals("SL123", container.BC_Seal1);
			AssertEquals("SL456", container.BC_Seal2);
			AssertEquals(false, container.BC_IsEmptyInfo.ReadOnly);
			AssertEquals(false, container.BC_RL_NKForeignPortInfo.ReadOnly);
			AssertEquals(false, container.BC_TypeOfServiceInfo.ReadOnly);

			container.BC_IsEmpty = ZBool.False;
			AssertEquals(true, container.BC_RL_NKForeignPortInfo.ReadOnly);
			AssertEquals("", container.BC_RL_NKForeignPort);

			container.BC_IsEmpty = ZBool.True;
			AssertEquals(false, container.BC_RL_NKForeignPortInfo.ReadOnly);

			container.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			//Users might enter NC1 as NonContainerised like NVOCC has declared. So all readonly features are gone to make things consistent between NC & NC1
			AssertEquals(false, container.BC_RCInfo.ReadOnly);
			AssertEquals(false, container.BC_Seal1Info.ReadOnly);
			AssertEquals(false, container.BC_Seal2Info.ReadOnly);
			AssertEquals(false, container.BC_IsEmptyInfo.ReadOnly);
			AssertEquals(true, container.BC_RL_NKForeignPortInfo.ReadOnly);
			AssertEquals(false, container.BC_TypeOfServiceInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, container.BC_RC);
			AssertEquals("", container.BC_Seal1);
			AssertEquals("", container.BC_Seal2);
			AssertEquals(ZBool.False, container.BC_IsEmpty);
			AssertEquals("", container.BC_RL_NKForeignPort);
			AssertEquals("", container.BC_TypeOfService);
		}

		public void TestContainerEquipmentType()
		{
			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "20KK";
			containerType.RC_ISOType = "20KK";
			containerType.RC_ContainerType = "TNK";
			containerType.SetCountrySpecificContainerCode("20TK", Enterprise.Core.Constants.CountryCodes.UnitedStates, USContainerUsageList.Codes.AMS);

			var containerType1 = Factory.New<RefContainer>();
			containerType1.RC_Code = "2000";
			containerType1.RC_ISOType = "2000";
			containerType1.RC_ContainerType = "TNK";

			var containerType2 = Factory.New<RefContainer>();
			containerType2.RC_Code = "1000";
			containerType2.RC_ISOType = "1000";
			containerType2.RC_ContainerType = "TNK";
			containerType2.SetCountrySpecificContainerCode("CX", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			var (_, container) = GetHeaderAndContainer(Factory);
			container.BC_RC = containerType.PK;
			AssertEquals("20TK", ((ICommonContainer)container).ContainerEquipmentType);

			container.BC_RC = containerType1.PK;
			AssertEquals("2000", ((ICommonContainer)container).ContainerEquipmentType);

			container.BC_RC = containerType2.PK;
			AssertEquals("1000", ((ICommonContainer)container).ContainerEquipmentType);
		}

		protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory) => GetHeaderAndContainer(factory).container;

		(CusInBondHeader header, CusInBondContainer container) GetHeaderAndContainer(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeader;
			var moveDetail = bill.MovementDetail;
			var container = moveDetail.Containers.AddNew();
			return (header, container);
		}
	}
}
