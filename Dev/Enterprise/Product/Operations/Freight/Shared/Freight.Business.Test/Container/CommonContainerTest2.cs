using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using EventConstants = CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business.Testing
{
	public class CommonContainerTest2 : BaseFreightTest
	{
		#region Declaration

		public void TestContainerCouldLoadRelatedDeclaration()
		{
			var container = GetNewContainer();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();

			var cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC] = container.PK;

			string message = string.Format("Container should {0}load related declaration", CouldLoadRelatedDeclaration ? "" : "not ");
			AssertEquals(message, CouldLoadRelatedDeclaration, container.Declaration != null);
		}

		protected virtual bool CouldLoadRelatedDeclaration
		{
			get { return true; }
		}

		#endregion

		public void TestHasHazardous()
		{
			Container.JC_RH_NKContainerCommodityCode = "GEN";
			Assert(!Container.HasHazardous);

			Container.JC_RH_NKContainerCommodityCode = "MTHZ";
			Assert(Container.HasHazardous);
		}

		public void TestClone()
		{
			var container = Factory.New<CommonContainer>();
			container.JC_RC = RC_20GP_PK;
			container.JC_Calc_NetWeight = 1000;
			container.JC_TareWeight = 2500;
			container.JC_JK = ZGuid.NewZGuid();
			container.JC_JS_FCLBookingOnlyLink = ZGuid.NewZGuid();
			container.JC_ContainerJobID = "D10000001";
			container.JC_JSB_SupplierBooking = ZGuid.NewZGuid();
			container.JC_RCA_AllocationLine = ZGuid.NewZGuid();
			container.JC_CLH_LoadListPlan = ZGuid.NewZGuid();

			var clone = (CommonContainer)container.Clone();
			AssertEquals("JC_JK", ZGuid.Empty, clone.JC_JK);
			AssertEquals("JC_JS_FCLBookingOnlyLink", ZGuid.Empty, clone.JC_JS_FCLBookingOnlyLink);
			AssertEquals("JC_GrossWeight", 3500m, clone.JC_GrossWeight);
			AssertEquals("JC_TareWeight", 2500m, clone.JC_TareWeight);
			AssertEquals("JC_Calc_NetWeight", 1000m, clone.JC_Calc_NetWeight);
			AssertEquals("JC_ContainerJobID", ZString.Empty, clone.JC_ContainerJobID);
			AssertEquals("JC_JSB_SupplierBooking", ZGuid.Empty, clone.JC_JSB_SupplierBooking);
			AssertEquals("JC_RCA_AllocationLine", ZGuid.Empty, clone.JC_RCA_AllocationLine);
			AssertEquals("JC_CLH_LoadListPlan", ZGuid.Empty, clone.JC_CLH_LoadListPlan);
		}

		public void TestJC_ContainerMode_List()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var container = consol.Containers.AddNew();

			AssertContainsExactElementsInAnyOrder(new[] { Constants.ContainerModes.ULD }, container.JC_ContainerMode_List.GetAllCodes());

			consol.JK_TransportMode = "ZZZ";
			var expectedContainerModes = new[]
			{
				Constants.ContainerModes.LCL,
				Constants.ContainerModes.FCL,
				Constants.ContainerModes.Groupage,
				Constants.ContainerModes.BuyersConsol,
				Constants.ContainerModes.ShippersConsol,
				Constants.ContainerModes.BreakBulk,
				Constants.ContainerModes.RollOnRollOff
			};

			AssertContainsExactElementsInAnyOrder(expectedContainerModes, container.JC_ContainerMode_List.GetAllCodes());
		}

		public void TestJC_DeliveryMode_List()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Assert(Container.JC_DeliveryMode_List.Count == 0);

			Consol.JK_TransportMode = "ZZZ";
			Assert(Container.JC_DeliveryMode_List.Count == 4);
		}

		public void TestJC_ContainerNum_ContainerEventDataVendorNotified()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Container.JC_ContainerNum = "C1000";
				AssertEquals("ContainerEventDataVendor must be notified of the change", true, MockContainerEventDataVendor.Instance.NotifyContainerNumberChangedCalled);
			}
		}

		public void TestJC_ContainerNum_LowercaseGetsForcedToUppercase()
		{
			CommonContainer testContainer = Factory.New<CommonContainer>();
			const string TestContainerNumber = "crxu1234567";
			testContainer.JC_ContainerNum = TestContainerNumber.ToLower();
			AssertEquals(TestContainerNumber.ToUpper(), testContainer.JC_ContainerNum);
		}

		public void TestJC_RC()
		{
			RefContainer uLD = RefContainer.New(Factory);
			uLD.RC_ShippingMode = "AIR";
			uLD.RC_IATARateClass = "ZZ1";

			RefContainer seaContainer = RefContainer.New(Factory);
			seaContainer.RC_ShippingMode = "SEA";

			CommonContainer testContainer = Factory.New<CommonContainer>();
			testContainer.JC_RC = seaContainer.PK;

			testContainer.JC_ContainerMode = Constants.ContainerModes.AIR;
			Assert("JC_RC should be cleared", testContainer.JC_RC.IsEmpty);
			Assert("JC_RC should be readonly", testContainer.JC_RCInfo.ReadOnly);

			testContainer.JC_ContainerMode = Constants.ContainerModes.ULD;
			testContainer.JC_RC = seaContainer.PK;
			testContainer.JC_ContainerMode = Constants.ContainerModes.ULD;
			Assert("JC_RC should be valid", testContainer.JC_RC.IsValid);
			Assert("JC_RC should not be readonly as Containers can be ULD", !testContainer.JC_RCInfo.ReadOnly);
		}

		public void TestJC_RCVerificationTypeNRQ()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			var consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight = true;

			var container = consol.Containers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ShippingMode = "SEA";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_ActualWeight = 50m;
			packLine.JL_JS = shipment.PK;
			container.PackLines.Add(packLine);

			AssertNotNull("Precondition: Shipment Consignor", shipment.Consignor);
			AssertEquals("Precondition: Shipment is direct shipment", true, shipment.IsDirectShipment);
			AssertEquals("Precondition: VGM set to true", true, shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight);
			AssertNotEquals("Precondition: GrossWeight is not zero", 0m, container.JC_GrossWeight);
			AssertEquals("Precondition: JC_RC should be cleared", true, container.JC_RC.IsEmpty);
			AssertEquals("Precondition: Verification should be Not Verified", Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, container.JC_GrossWeightVerificationType);
			AssertEquals("Precondition: parent shipment count", 1, container.ParentShipmentsCached.Count());

			container.JC_RC = refContainer.PK;

			AssertEquals("JC_RC should be valid", true, container.JC_RC.IsValid);
			AssertEquals("Verification should be Not Required", Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, container.JC_GrossWeightVerificationType);
		}

		public void TestJC_RCVerificationTypeNON()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment = consol.Shipments.AddNew();
			var consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight = true;

			var container = consol.Containers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired;

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ShippingMode = "SEA";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_JS = shipment.PK;
			container.PackLines.Add(packLine);

			AssertNotNull("Precondition: Shipment Consignor", shipment.Consignor);
			AssertEquals("Precondition: Shipment is direct shipment", true, shipment.IsDirectShipment);
			AssertEquals("Precondition: VGM set to true", true, shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight);
			AssertEquals("Precondition: GrossWeight is zero", (ZDecimal)0, container.JC_GrossWeight);
			AssertEquals("Precondition: JC_RC should be cleared", true, container.JC_RC.IsEmpty);
			AssertEquals("Precondition: Verification should be Not Verified", Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, container.JC_GrossWeightVerificationType);
			AssertEquals("Precondition: parent shipment count", 1, container.ParentShipmentsCached.Count());

			container.JC_RC = refContainer.PK;

			AssertEquals("JC_RC should be valid", true, container.JC_RC.IsValid);
			AssertEquals("Verification should be Not Required", Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, container.JC_GrossWeightVerificationType);
		}

		public void TestJC_RCVerificationTypeNotDefaulted()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;

			var shipment = consol.Shipments.AddNew();
			var consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight = true;

			var container = consol.Containers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod;

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ShippingMode = "SEA";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_JS = shipment.PK;
			container.PackLines.Add(packLine);

			AssertNotNull("Precondition: Shipment Consignor", shipment.Consignor);
			AssertEquals("Precondition: Shipment is not direct shipment", false, shipment.IsDirectShipment);
			AssertEquals("Precondition: VGM set to true", true, shipment.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight);
			AssertEquals("Precondition: GrossWeight is zero", (ZDecimal)0, container.JC_GrossWeight);
			AssertEquals("Precondition: JC_RC should be cleared", true, container.JC_RC.IsEmpty);
			AssertEquals("Precondition: parent shipment count", 1, container.ParentShipmentsCached.Count());

			container.JC_RC = refContainer.PK;

			AssertEquals("JC_RC should be valid", true, container.JC_RC.IsValid);
			AssertEquals("Verification should not be changed", Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod, container.JC_GrossWeightVerificationType);
		}

		public void TestJC_GrossWeightVerificationLoadPort()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var container = consol.Containers.AddNew();

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "ONE";
			transport.JW_TransportMode = Constants.TransportModes.Sea;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "TWO";
			transport2.JW_TransportMode = Constants.TransportModes.Sea;

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			container.JC_GrossWeight = 30;

			AssertNotEquals("Precondition: JC_GrossWeightVerificationLoadPort is not empty before save ", container.JC_GrossWeightVerificationLoadPort, ZString.Empty);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedContainer = factory2.Load<CommonContainer>(container.PK);
			AssertNotEquals("Expected JC_GrossWeightVerificationLoadPort in the reloaded container not to be empty", reloadedContainer.JC_GrossWeightVerificationLoadPort, ZString.Empty);
		}

		public void TestJC_GrossWeightVerificationLoadPort_WhenTransportModeIsIWT()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "ONE";
			transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;

			var container1 = consol.Containers.AddNew();
			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			container1.JC_GrossWeight = 30;

			AssertEquals("Precondition: JC_GrossWeightVerificationLoadPort is empty when Transport Mode is IWT", ZString.Empty, container1.JC_GrossWeightVerificationLoadPort);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "TWO";
			transport2.JW_TransportMode = Constants.TransportModes.Sea;

			var container2 = consol.Containers.AddNew();
			container2.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			container2.JC_GrossWeight = 30;

			AssertNotEquals("Precondition: JC_GrossWeightVerificationLoadPort is not empty before save ", container2.JC_GrossWeightVerificationLoadPort, ZString.Empty);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedContainer = factory2.Load<CommonContainer>(container2.PK);
			AssertNotEquals("Expected JC_GrossWeightVerificationLoadPort in the reloaded container not to be empty", reloadedContainer.JC_GrossWeightVerificationLoadPort, ZString.Empty);
		}

		[ExpectNoExceptions]
		public void TestJC_GrossWeightVerificationType_AgentConsol_EmptyShipmentConsignor()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment1 = consol.Shipments.AddNew();
			var consignor1 = Factory.New<OrgHeader>();
			shipment1.ConsignorPK = ZGuid.Empty;

			var container = consol.Containers.AddNew();
			AssertEquals("Precondition", ZGuid.Empty, container.GrossWeightVerifiedByAddress.OrganisationPK);
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			AssertEquals(Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, container.JC_GrossWeightVerificationType);
			AssertEquals(ZGuid.Empty, container.GrossWeightVerifiedByAddress.OrganisationPK);
		}

		public void TestJC_RCWithMultipleShipmentsNRQ()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment1 = consol.Shipments.AddNew();
			var consignor1 = Factory.New<OrgHeader>();
			shipment1.ConsignorPK = consignor1.PK;
			shipment1.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight = true;

			var shipment2 = consol.Shipments.AddNew();
			var consignor2 = Factory.New<OrgHeader>();
			shipment2.ConsignorPK = consignor2.PK;
			shipment2.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight = true;

			var container = consol.Containers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ShippingMode = "SEA";

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_JS = shipment1.PK;
			packLine1.JL_ActualWeight = 50m;
			container.PackLines.Add(packLine1);

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_JS = shipment2.PK;
			packLine2.JL_ActualWeight = 50m;
			container.PackLines.Add(packLine2);

			AssertEquals("Precondition: Shipment is direct shipment", true, shipment1.IsDirectShipment);
			AssertEquals("Precondition: container has multiple parent shipments", 2, container.ParentShipmentsCached.Count());
			AssertEquals("Precondition: Verification should be Not Verified", Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, container.JC_GrossWeightVerificationType);
			AssertNotEquals("Precondition: GrossWeight is not zero", (ZDecimal)0, container.JC_GrossWeight);
			AssertEquals("Precondition: JC_RC should be cleared", true, container.JC_RC.IsEmpty);
			AssertEquals("Precondition: parent shipment count", 2, container.ParentShipmentsCached.Count());

			container.JC_RC = refContainer.PK;

			AssertEquals("JC_RC should be valid", true, container.JC_RC.IsValid);
			AssertEquals("Verification should be changed to lowest rights", Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, container.JC_GrossWeightVerificationType);
		}

		public void TestJC_RCWithMultipleShipmentsNON()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment1 = consol.Shipments.AddNew();
			var consignor1 = Factory.New<OrgHeader>();
			shipment1.ConsignorPK = consignor1.PK;
			shipment1.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight = true;

			var shipment2 = consol.Shipments.AddNew();
			var consignor2 = Factory.New<OrgHeader>();
			shipment2.ConsignorPK = consignor2.PK;
			shipment2.ConsignorPickupAddress.Address.OA_VerifiesContainerGrossWeight = true;

			var container = consol.Containers.AddNew();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired;

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ShippingMode = "SEA";

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_JS = shipment1.PK;
			container.PackLines.Add(packLine1);

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_JS = shipment2.PK;
			container.PackLines.Add(packLine2);

			AssertEquals("Precondition: Shipment is direct shipment", true, shipment1.IsDirectShipment);
			AssertEquals("Precondition: container has multiple parent shipments", 2, container.ParentShipmentsCached.Count());
			AssertEquals("Precondition: Verification should be Not Required", Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired, container.JC_GrossWeightVerificationType);
			AssertEquals("Precondition: GrossWeight is zero", (ZDecimal)0, container.JC_GrossWeight);
			AssertEquals("Precondition: JC_RC should be cleared", true, container.JC_RC.IsEmpty);
			AssertEquals("Precondition: parent shipment count", 2, container.ParentShipmentsCached.Count());

			container.JC_RC = refContainer.PK;

			AssertEquals("JC_RC should be valid", true, container.JC_RC.IsValid);
			AssertEquals("Verification should be changed to lowest rights", Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified, container.JC_GrossWeightVerificationType);
		}

		public void TestJC_DepartureCartageAdvised()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			shipment.Consols.Add(consol);
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			CommonContainer container3 = consol.Containers.AddNew();

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_ActualWeight = 20.2m;
			packLine1.JL_ActualVolume = 2.2m;
			packLine1.JL_JS = shipment.PK;
			container1.PackLines.Add(packLine1);

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLine2.JL_JS = shipment.PK;
			container2.PackLines.Add(packLine2);

			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 2;
			packLine3.JL_ActualWeight = 20.2m;
			packLine3.JL_ActualVolume = 2.2m;
			packLine3.JL_JS = shipment.PK;
			container3.PackLines.Add(packLine3);

			AssertEquals(ZDateTime.Empty, container1.JC_DepartureCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageAdvised);

			ZDateTime currentDateTime = ZDateTime.Now;

			container1.JC_DepartureCartageAdvised = currentDateTime;

			AssertEquals(currentDateTime, container1.JC_DepartureCartageAdvised);
			AssertEquals(ZDateTime.Empty, container2.JC_DepartureCartageAdvised);
			AssertEquals(ZDateTime.Empty, container3.JC_DepartureCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageAdvised);

			container2.JC_DepartureCartageAdvised = currentDateTime.AddDays(1);

			AssertEquals(currentDateTime, container1.JC_DepartureCartageAdvised);
			AssertEquals(currentDateTime.AddDays(1), container2.JC_DepartureCartageAdvised);
			AssertEquals(ZDateTime.Empty, container3.JC_DepartureCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_PickupCartageAdvised);

			container3.JC_DepartureCartageAdvised = currentDateTime.AddDays(-1);

			AssertEquals(currentDateTime, container1.JC_DepartureCartageAdvised);
			AssertEquals(currentDateTime.AddDays(1), container2.JC_DepartureCartageAdvised);
			AssertEquals(currentDateTime.AddDays(-1), container3.JC_DepartureCartageAdvised);
			AssertEquals(currentDateTime.AddDays(1), shipment.DocsAndCartage.JP_PickupCartageAdvised);
		}

		public void TestJC_ArrivalCartageAdvised()
		{
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			Factory.Save();

			shipment.Consols.Add(consol);
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			CommonContainer container3 = consol.Containers.AddNew();

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 2;
			packLine1.JL_ActualWeight = 20.2m;
			packLine1.JL_ActualVolume = 2.2m;
			packLine1.JL_JS = shipment.PK;
			container1.PackLines.Add(packLine1);

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			packLine2.JL_JS = shipment.PK;
			container2.PackLines.Add(packLine2);

			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 2;
			packLine3.JL_ActualWeight = 20.2m;
			packLine3.JL_ActualVolume = 2.2m;
			packLine3.JL_JS = shipment.PK;
			container3.PackLines.Add(packLine3);

			AssertEquals(ZDateTime.Empty, container1.JC_ArrivalCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);

			ZDateTime currentDateTime = ZDateTime.Now;

			container1.JC_ArrivalCartageAdvised = currentDateTime;

			AssertEquals(currentDateTime, container1.JC_ArrivalCartageAdvised);
			AssertEquals(ZDateTime.Empty, container2.JC_ArrivalCartageAdvised);
			AssertEquals(ZDateTime.Empty, container3.JC_ArrivalCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);

			container2.JC_ArrivalCartageAdvised = currentDateTime.AddDays(1);

			AssertEquals(currentDateTime, container1.JC_ArrivalCartageAdvised);
			AssertEquals(currentDateTime.AddDays(1), container2.JC_ArrivalCartageAdvised);
			AssertEquals(ZDateTime.Empty, container3.JC_ArrivalCartageAdvised);
			AssertEquals(ZDateTime.Empty, shipment.DocsAndCartage.JP_DeliveryCartageAdvised);

			container3.JC_ArrivalCartageAdvised = currentDateTime.AddDays(-1);

			AssertEquals(currentDateTime, container1.JC_ArrivalCartageAdvised);
			AssertEquals(currentDateTime.AddDays(1), container2.JC_ArrivalCartageAdvised);
			AssertEquals(currentDateTime.AddDays(-1), container3.JC_ArrivalCartageAdvised);
			AssertEquals(currentDateTime.AddDays(1), shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		public virtual void TestValidateJC_ContainerNum()
		{
			Container.JC_ContainerCount = -2;
			Container.JC_ContainerNum = "";
			AssertEquals("Negative count invalid if blank container num.", true, Container.JC_ContainerCountInfo.HasErrors());

			Container.JC_ContainerCount = 0;
			Assert("Error expected (blank container number and Count = 0)", Container.JC_ContainerCountInfo.HasErrors());

			Consol.Containers.Add(Container);
			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = "XXXX9999999";
			Container.JC_ContainerNum = "XXXX9999999";
			Assert("Error expected (Duplicate)", Container.JC_ContainerNumInfo.HasErrors());

			Container.JC_ContainerMode = Constants.ContainerModes.LCL;
			Container.JC_ContainerNum = "ABCD";
			Container.JC_ContainerCount = 1;
			Assert("Warning expected (ABCD)", Container.JC_ContainerNumInfo.HasWarnings());

			Container.JC_ContainerNum = "ABCD1234567";
			Assert("Warning expected (ABCD1234567)", Container.JC_ContainerNumInfo.HasWarnings());

			Container.JC_ContainerNum = "ABCD1234560";
			Assert("No warnings expected (ABCD1234560)", !Container.JC_ContainerNumInfo.HasWarnings());

			Container.JC_ContainerMode = Constants.ContainerModes.AIR;
			Container.JC_ContainerNum = "ABCD";
			Assert("No warnings expected (AIR Cont. Mode)", !Container.JC_ContainerNumInfo.HasWarnings());

			Container.JC_ContainerMode = Constants.ContainerModes.ULD;
			Container.JC_ContainerNum = "ABCD1234567";
			AssertHasWarning(Container.JC_ContainerNumInfo, "This is not a valid ULD number.");

			Container.JC_ContainerNum = "ABCD1234XX";
			Assert("No warnings expected (UDL Cont. Mode)", !Container.JC_ContainerNumInfo.HasWarnings());

			Container.JC_ContainerCount = 1;
			Container.JC_ContainerNum = "";
			Assert("Error expected (num blank, count = 1)", !Container.JC_ContainerNumInfo.HasErrors());

			Container.JC_ContainerNum = "ABCD1234567";
			Assert("No error expected (ABCD1234567 and Count = 1)", !Container.JC_ContainerNumInfo.HasErrors());
		}

		public void TestValidateJC_ContainerCount()
		{
			Container.JC_ContainerNum = "";
			Container.JC_ContainerCount = 0;
			Assert("Error expected (Count = 0 and blank container number)", Container.JC_ContainerCountInfo.HasErrors());

			Container.JC_ContainerCount = 1;
			Assert("No error expected (Count = 1 and blank container number)", !Container.JC_ContainerCountInfo.HasErrors());

			Container.JC_ContainerNum = "ABCD1234567";
			Container.JC_ContainerCount = 1;
			Assert("No Error expected (Count = 1 and container number is entered)", !Container.JC_ContainerCountInfo.HasErrors());

			Container.JC_ContainerCount = 13;
			Assert("Error expected (container num entered, count != 1)", Container.JC_ContainerCountInfo.HasErrors());
		}

		public void TestValidateJC_ContainerMode()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Container.JC_ContainerMode = "ABC";
			Assert("Error expected", Container.JC_ContainerModeInfo.HasErrors());

			Container.JC_ContainerMode = Constants.ContainerModes.ULD;
			Assert("No error expected", !Container.JC_ContainerModeInfo.HasErrors());

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Container.JC_ContainerMode = Constants.ContainerModes.LCL;
			Assert("No error expected", !Container.JC_ContainerModeInfo.HasErrors());

			Container.JC_ContainerMode = Constants.ContainerModes.ULD;
			Assert("Error expected", Container.JC_ContainerModeInfo.HasErrors());
		}

		public virtual void TestValidateJC_DeliveryMode()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Container.JC_DeliveryMode = "ABC";
			Assert("Error expected", Container.JC_DeliveryModeInfo.HasErrors());

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Container.JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;
			Assert("No error expected", !Container.JC_DeliveryModeInfo.HasErrors());

			Container.JC_DeliveryMode = "ABC";
			Assert("Error expected", Container.JC_DeliveryModeInfo.HasErrors());
		}

		public void TestValidateContainerModeAgainstConsolMode()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Air;
			Consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			Container.JC_ContainerMode = Constants.ContainerModes.AIR;
			Assert("Consol AIR/LSE, Container AIR. No warning", !Container.JC_ContainerModeInfo.HasWarnings());

			Container.JC_ContainerMode = Constants.ContainerModes.ULD;
			Assert("Consol AIR/LSE, Container ULD. No Warning.", !Container.JC_ContainerModeInfo.HasWarnings());

			Consol.JK_ConsolMode = Constants.ContainerModes.ULD;
			Container.Validation.ValidateJC_ContainerMode();
			Assert("Consol ULD, Container ULD. No warning.", !Container.JC_ContainerModeInfo.HasWarnings());

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Constants.ContainerModes.Bulk;
			Container.JC_ContainerMode = Constants.ContainerModes.LCL;
			Assert("Consol SEA/BLK. No warning.", !Container.JC_ContainerModeInfo.HasWarnings());

			Consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			Container.Validation.ValidateJC_ContainerMode();
			Assert("Consol FCL. Container LCL. Warning.", Container.JC_ContainerModeInfo.HasWarnings());

			Container.JC_ContainerMode = Constants.ContainerModes.FCL;
			Assert("Consol FCL. Container LCL. No warning.", !Container.JC_ContainerModeInfo.HasWarnings());
		}

		public void TestPackLineTotals()
		{
			var container = GetNewContainer();
			var shipment1 = CommonShipment.New(Factory);

			PackLine packLineThatDoesNotBelong = shipment1.OuterPackLines.AddNew();
			packLineThatDoesNotBelong.JL_PackageCount = 10;
			packLineThatDoesNotBelong.JL_ActualWeight = 10.5m;
			packLineThatDoesNotBelong.JL_ActualVolume = 10.5m;

			PackLine packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_ActualWeight = 10.1m;
			packLine1.JL_ActualVolume = 1.1m;
			container.PackLines.Add(packLine1);

			PackLine packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 2;
			packLine2.JL_ActualWeight = 20.2m;
			packLine2.JL_ActualVolume = 2.2m;
			container.PackLines.Add(packLine2);

			AssertEquals("Incorrect Total Count.", 3, container.JC_Calc_TotalPackages);
			AssertEquals("Incorrect Total Weight.", 30.3m, container.JC_Calc_TotalWeight);
			AssertEquals("Incorrect Total Weight By Shipment.", 30.3m, container.GetTotalWeightByShipment(shipment1));
			AssertEquals("Incorrect Total Volume.", 3.3m, container.JC_Calc_TotalVolume);
			AssertEquals("Incorrect Total Volume By Shipment.", 3.3m, container.GetTotalVolumeByShipment(shipment1));

			CommonShipment shipment2 = CommonShipment.New(Factory);

			PackLine packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 6;
			packLine3.JL_ActualWeight = 60.6m;
			packLine3.JL_ActualVolume = 6.6m;
			container.PackLines.Add(packLine3);

			Factory.Save();

			AssertEquals("Incorrect Total Count.", 9, container.JC_Calc_TotalPackages);
			AssertEquals("Incorrect Total Packages By Shipment.", 3m, container.GetTotalPackagesByShipment(shipment1));
			AssertEquals("Incorrect Total Weight.", 90.9m, container.JC_Calc_TotalWeight);
			AssertEquals("Incorrect Total Weight By Shipment.", 30.3m, container.GetTotalWeightByShipment(shipment1));
			AssertEquals("Incorrect Total Volume.", 9.9m, container.JC_Calc_TotalVolume);
			AssertEquals("Incorrect Total Volume By Shipment.", 3.3m, container.GetTotalVolumeByShipment(shipment1));
		}

		public void TestMessagesCollection()
		{
			EDIMessage message = Container.Messages.AddNew();
			message.EM_MessageText = "AAA";
			AssertEquals("AAA", message.EM_MessageText);
		}

		public void TestContainerCreated_ContainerEventDataVendorNotified()
		{
			using (MockContainerEventDataVendor.Instance)
			{
				Factory.New<CommonContainer>();
				AssertEquals("ContainerEventDataVendor must be notified of the change", true, MockContainerEventDataVendor.Instance.NotifyContainerCreatedCalled);
			}
		}

		public void TestIsWaitingForPRAResponse()
		{
			ZDateTime now = ZDateTime.Now;

			AssertEquals(false, Container.IsWaitingForPRAResponse);

			EDIMessage unrelatedMessage = Container.Messages.AddNew();
			unrelatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			unrelatedMessage.EM_SystemCreateTimeUtc = now;

			EDIMessage message1 = Container.PRAMessages.AddNew();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message1.EM_SystemCreateTimeUtc = now.AddMinutes(1);
			message1.EM_ReceiveTransmit = "TRX";
			AssertEquals(true, Container.IsWaitingForPRAResponse);

			EDIMessage message2 = Container.PRAMessages.AddNew();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message2.EM_SystemCreateTimeUtc = now.AddMinutes(2);
			message2.EM_ReceiveTransmit = "RCV";
			AssertEquals(false, Container.IsWaitingForPRAResponse);
		}

		public void TestCurrentPRAStatus()
		{
			ZDateTime now = ZDateTime.Now;

			IPRAContainerMessaging containerMessaging = Container;
			AssertEquals("No PRA Messages Have Been Sent.", containerMessaging.CurrentPRAStatus);

			EDIMessage unrelatedMessage = Container.Messages.AddNew();
			unrelatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			unrelatedMessage.EM_SystemCreateTimeUtc = now;

			EDIMessage message1 = Container.PRAMessages.AddNew();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message1.EM_SystemCreateTimeUtc = now.AddMinutes(1);
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SSM"; // Send Message
			AssertEquals("PRA Submit Message Sent but not responded to yet.", containerMessaging.CurrentPRAStatus);

			EDIMessage message2 = Container.PRAMessages.AddNew();
			message2.EM_SystemCreateTimeUtc = now.AddMinutes(2);
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = "ACK"; // Accepted Response
			AssertEquals("PRA Submit Message Sent and was Accepted.", containerMessaging.CurrentPRAStatus);

			EDIMessage message3 = Container.PRAMessages.AddNew();
			message3.EM_SystemCreateTimeUtc = now.AddMinutes(3);
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message3.EM_ReceiveTransmit = "TRX";
			message3.EM_MessageSubType = "SCN"; // Cancel Message
			AssertEquals("PRA Cancellation Message Sent but not responded to yet.", containerMessaging.CurrentPRAStatus);

			EDIMessage message4 = Container.PRAMessages.AddNew();
			message4.EM_SystemCreateTimeUtc = now.AddMinutes(4);
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message4.EM_ReceiveTransmit = "RCV";
			message4.EM_MessageSubType = "REJ"; // Rejected Response
			AssertEquals("PRA Cancellation Message Sent and was Rejected.", containerMessaging.CurrentPRAStatus);
		}

		public void TestLastPRAMessageSentWasCancellation()
		{
			ZDateTime now = ZDateTime.Now;

			AssertEquals(false, Container.LastPRAMessageSentWasCancellation);

			EDIMessage unrelatedMessage = Container.Messages.AddNew();
			unrelatedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.EIDO;
			unrelatedMessage.EM_SystemCreateTimeUtc = now;

			EDIMessage message1 = Container.PRAMessages.AddNew();
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SCN"; // Cancel Message
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message1.EM_SystemCreateTimeUtc = now.AddMinutes(1);
			AssertEquals(true, Container.LastPRAMessageSentWasCancellation);

			EDIMessage message2 = Container.PRAMessages.AddNew();
			message2.EM_ReceiveTransmit = "TRX";
			message2.EM_MessageSubType = "SSM"; // Send Message
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.OneStop;
			message2.EM_SystemCreateTimeUtc = now.AddMinutes(2);
			AssertEquals(false, Container.LastPRAMessageSentWasCancellation);
		}

		public void TestECNorCAN()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonContainer container1 = consol.Containers.AddNew();
			Container.JC_RC = RC_20GP_PK;

			CommonShipment shipment1 = consol.Shipments.AddNew();
			PackLine packLine1 = shipment1.OuterPackLines.AddNew();

			CommonShipment shipment2 = consol.Shipments.AddNew();
			PackLine packLine2 = shipment2.OuterPackLines.AddNew();

			CommonShipment shipment3 = consol.Shipments.AddNew();
			PackLine packLine3 = shipment3.OuterPackLines.AddNew();

			packLine1.SetContainer(container1.PK);
			packLine2.SetContainer(container1.PK);
			packLine3.SetContainer(container1.PK);
			AssertEquals(ZString.Empty, container1.ECNOrCAN);

			shipment1.CustomsEntryNumberType = CANType.CustomsAuthorityNumber.Code;
			shipment1.CustomsEntryNumber = "123456789";
			AssertEquals(ZString.Empty, container1.ECNOrCAN);

			shipment2.CustomsEntryNumberType = CANType.CustomsAuthorityNumber.Code;
			shipment2.CustomsEntryNumber = "123456789";
			shipment3.CustomsEntryNumberType = CANType.CustomsAuthorityNumber.Code;
			shipment3.CustomsEntryNumber = "123456789";
			AssertEquals("123456789", container1.ECNOrCAN);

			shipment2.CustomsEntryNumber = "987654321";
			AssertEquals(ZString.Empty, container1.ECNOrCAN);

			shipment2.JS_IsCancelled = true;
			AssertEquals("123456789", container1.ECNOrCAN);
		}

		public void TestDefaultOutturnFromManifested()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonContainer container1 = consol.Containers.AddNew();
			Container.JC_RC = RC_20GP_PK;

			CommonContainer container2 = consol.Containers.AddNew();
			Container.JC_RC = RC_20RE_PK;

			CommonShipment shipment1 = consol.Shipments.AddNew();

			PackLine packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 5;
			packLine1.JL_Outturn = 4;
			packLine1.JL_Width = 50;
			packLine1.JL_OutturnedWidth = 40;
			packLine1.JL_Height = 51;
			packLine1.JL_OutturnedHeight = 41;
			packLine1.JL_Length = 52;
			packLine1.JL_OutturnedLength = 42;
			packLine1.JL_ActualWeight = 53;
			packLine1.JL_OutturnedWeight = 43;
			packLine1.JL_ActualVolume = 54;
			packLine1.JL_OutturnedVolume = 44;
			packLine1.SetContainer(container1.PK);

			PackLine packLine2 = shipment1.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 1;
			packLine2.JL_Outturn = 0;
			packLine2.JL_Width = 10;
			packLine2.JL_OutturnedWidth = 0;
			packLine2.JL_Height = 11;
			packLine2.JL_OutturnedHeight = 0;
			packLine2.JL_Length = 12;
			packLine2.JL_OutturnedLength = 0;
			packLine2.JL_ActualWeight = 13;
			packLine2.JL_OutturnedWeight = 0;
			packLine2.JL_ActualVolume = 14;
			packLine2.JL_OutturnedVolume = 0;
			packLine2.SetContainer(container1.PK);

			PackLine packLine3 = shipment1.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 7;
			packLine3.JL_Outturn = 0;
			packLine3.JL_Width = 70;
			packLine3.JL_OutturnedWidth = 0;
			packLine3.JL_Height = 71;
			packLine3.JL_OutturnedHeight = 0;
			packLine3.JL_Length = 72;
			packLine3.JL_OutturnedLength = 0;
			packLine3.JL_ActualWeight = 73;
			packLine3.JL_OutturnedWeight = 0;
			packLine3.JL_ActualVolume = 74;
			packLine3.JL_OutturnedVolume = 0;
			packLine3.SetContainer(container2.PK);

			PackLine packLine4 = shipment1.OuterPackLines.AddNew();
			packLine4.JL_PackageCount = 8;
			packLine4.JL_Outturn = 0;
			packLine4.JL_Width = 80;
			packLine4.JL_OutturnedWidth = 0;
			packLine4.JL_Height = 81;
			packLine4.JL_OutturnedHeight = 0;
			packLine4.JL_Length = 82;
			packLine4.JL_OutturnedLength = 0;
			packLine4.JL_ActualWeight = 83;
			packLine4.JL_OutturnedWeight = 0;
			packLine4.JL_ActualVolume = 84;
			packLine4.JL_OutturnedVolume = 0;

			container1.DefaultOutturnFromManifested();
			AssertEquals("PackLine1 should not have defaulted outturn as it is already set", 4, packLine1.JL_Outturn);
			AssertEquals("PackLine1 should not have defaulted outturn width as it is already set", 40m, packLine1.JL_OutturnedWidth);
			AssertEquals("PackLine1 should not have defaulted outturn height as it is already set", 41m, packLine1.JL_OutturnedHeight);
			AssertEquals("PackLine1 should not have defaulted outturn lenght as it is already set", 42m, packLine1.JL_OutturnedLength);
			AssertEquals("PackLine1 should not have defaulted outturn weight as it is already set", 43m, packLine1.JL_OutturnedWeight);
			AssertEquals("PackLine1 should not have defaulted outturn volume as it is already set", 44m, packLine1.JL_OutturnedVolume);

			AssertEquals("PackLine2 should have been defaulted", 1, packLine2.JL_Outturn);
			AssertEquals("PackLine2 outturned width should have been defaulted", 10m, packLine2.JL_OutturnedWidth);
			AssertEquals("PackLine2 outturned height should have been defaulted", 11m, packLine2.JL_OutturnedHeight);
			AssertEquals("PackLine2 outturned length should have been defaulted", 12m, packLine2.JL_OutturnedLength);
			AssertEquals("PackLine2 outturned weight should have been defaulted", 13m, packLine2.JL_OutturnedWeight);
			AssertEquals("PackLine2 outturned volume should have been defaulted", 1320m, packLine2.JL_OutturnedVolume);

			AssertEquals("PackLine3 should not have defaulted as it is on another container", 0, packLine3.JL_Outturn);
			AssertEquals("PackLine3 should not have defaulted outturned width as it is on another container", 0m, packLine3.JL_OutturnedWidth);
			AssertEquals("PackLine3 should not have defaulted outturned height as it is on another container", 0m, packLine3.JL_OutturnedHeight);
			AssertEquals("PackLine3 should not have defaulted outturned lenght as it is on another container", 0m, packLine3.JL_OutturnedLength);
			AssertEquals("PackLine3 should not have defaulted outturned weight as it is on another container", 0m, packLine3.JL_OutturnedWeight);
			AssertEquals("PackLine3 should not have defaulted outturned volume as it is on another container", 0m, packLine3.JL_OutturnedVolume);

			AssertEquals("PackLine4 should not have defaulted as it is not on a container", 0, packLine4.JL_Outturn);
			AssertEquals("PackLine4 should not have defaulted outturned width as it is not on a container", 0m, packLine3.JL_OutturnedWidth);
			AssertEquals("PackLine4 should not have defaulted outturned height as it is not on a container", 0m, packLine3.JL_OutturnedHeight);
			AssertEquals("PackLine4 should not have defaulted outturned lenght as it is not on a container", 0m, packLine3.JL_OutturnedLength);
			AssertEquals("PackLine4 should not have defaulted outturned weight as it is not on a container", 0m, packLine3.JL_OutturnedWeight);
			AssertEquals("PackLine4 should not have defaulted outturned volume as it is not on a container", 0m, packLine3.JL_OutturnedVolume);
		}

		#region CargoAvailable event

		public void TestFactorySave_AvailableDateHasChanges_AddCargoAvailableEvent()
		{
			EnsureCargoAvailableEventCreated(Constants.ContainerModes.FCL, CommonContainer.Schema.JC_FCLAvailable, EventConstants.Facilities.Code.Terminal);
			EnsureCargoAvailableEventCreated(Constants.ContainerModes.LCL, CommonContainer.Schema.JC_LCLAvailable, EventConstants.Facilities.Code.Depot);
		}

		void EnsureCargoAvailableEventCreated(ZString containerMode, string valueExpectedFrom, string expectedFacility)
		{
			var consol = Factory.New<CommonConsol>();
			consol.Transports.AddNew("AUSYD", "USNYC");
			consol.Transports.AddNew("USNYC", "UAIEV");

			var container = consol.Containers.AddNew();

			Factory.Save();

			container.JC_LCLAvailable = 2.DaysAgo();
			container.JC_FCLAvailable = 4.DaysAgo();
			container.JC_ContainerMode = containerMode;

			if (containerMode == Constants.ContainerModes.FCL)
			{
				Assert(container.JC_OverrideFCLAvailableStorage);
				Assert(container.IsArrivalContainerModeFCLorULD);
			}
			else if (containerMode == Constants.ContainerModes.LCL)
			{
				Assert(container.JC_OverrideLCLAvailableStorage);
				Assert(!container.IsArrivalContainerModeFCLorULD);
			}

			Assert(container.JC_FCLAvailableInfo.HasChanges);
			Assert(container.JC_LCLAvailableInfo.HasChanges);

			Factory.Save();

			var log = container.Logs.MostRecentLogByEventTime(Events.CargoAvailable);
			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), log);
			AssertEquals("Event date", container[valueExpectedFrom], log.SL_EventTime);
			AssertEquals("Location", "UAIEV", log.Parameters[Params.Location]);
			AssertEquals("Facility", expectedFacility, log.Parameters[Params.Facility]);
		}

		public void TestFactorySave_AvailableDateHasChanges_AddCargoAvailableEvent_GRPNotInDB()
		{
			var consol = Factory.New<CommonConsol>();
			consol.Transports.AddNew("AUSYD", "USNYC");
			consol.Transports.AddNew("USNYC", "UAIEV");

			var container = consol.Containers.AddNew();

			container.JC_LCLAvailable = 2.DaysAgo();
			container.JC_FCLAvailable = 4.DaysAgo();
			container.JC_ContainerMode = Constants.ContainerModes.Groupage;

			AssertEquals(false, container.JC_FCLAvailableInfo.HasChanges);
			AssertEquals(false, container.JC_LCLAvailableInfo.HasChanges);
			AssertEquals(false, container.IsInDatabase);

			Factory.Save();

			var logs = container.Logs.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == "CAV");

			var ctoLog = logs
				.First(log => StmALog.GetParametersFromReference(log.SL_Reference)
					.TryGetValue(Params.Facility, out var result)
					&& result == EventConstants.Facilities.Code.Terminal);

			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), ctoLog);
			AssertEquals("Event date", container[CommonContainer.Schema.JC_FCLAvailable], ctoLog.SL_EventTime);
			AssertEquals("Location", "UAIEV", ctoLog.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, ctoLog.Parameters[Params.Facility]);

			var cfsLog = logs
				.First(log => StmALog.GetParametersFromReference(log.SL_Reference)
					.TryGetValue(Params.Facility, out var result)
					&& result == EventConstants.Facilities.Code.Depot);

			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), cfsLog);
			AssertEquals("Event date", container[CommonContainer.Schema.JC_LCLAvailable], cfsLog.SL_EventTime);
			AssertEquals("Location", "UAIEV", cfsLog.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Depot, cfsLog.Parameters[Params.Facility]);
		}

		public void TestFactorySave_AvailableDateHasChanges_AddCargoAvailableEvent_GRP()
		{
			var consol = Factory.New<CommonConsol>();
			consol.Transports.AddNew("AUSYD", "USNYC");
			consol.Transports.AddNew("USNYC", "UAIEV");

			var container = consol.Containers.AddNew();

			Factory.Save();

			container.JC_LCLAvailable = 2.DaysAgo();
			container.JC_FCLAvailable = 4.DaysAgo();
			container.JC_ContainerMode = Constants.ContainerModes.Groupage;

			Assert(container.JC_FCLAvailableInfo.HasChanges);
			Assert(container.JC_LCLAvailableInfo.HasChanges);

			Factory.Save();

			var logs = container.Logs.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => log.SL_SE_NKEvent == "CAV");

			var ctoLog = logs
				.First(log => StmALog.GetParametersFromReference(log.SL_Reference)
					.TryGetValue(Params.Facility, out var result)
					&& result == EventConstants.Facilities.Code.Terminal);

			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), ctoLog);
			AssertEquals("Event date", container[CommonContainer.Schema.JC_FCLAvailable], ctoLog.SL_EventTime);
			AssertEquals("Location", "UAIEV", ctoLog.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, ctoLog.Parameters[Params.Facility]);

			var cfsLog = logs
				.First(log => StmALog.GetParametersFromReference(log.SL_Reference)
					.TryGetValue(Params.Facility, out var result)
					&& result == EventConstants.Facilities.Code.Depot);

			AssertNotNull(string.Format("Log with {0} event", Events.CargoAvailable.Code), cfsLog);
			AssertEquals("Event date", container[CommonContainer.Schema.JC_LCLAvailable], cfsLog.SL_EventTime);
			AssertEquals("Location", "UAIEV", cfsLog.Parameters[Params.Location]);
			AssertEquals("Facility", EventConstants.Facilities.Code.Depot, cfsLog.Parameters[Params.Facility]);

			container.JC_LCLAvailable = 3.DaysAgo();
			container.JC_FCLAvailable = 5.DaysAgo();
			Assert(container.JC_FCLAvailableInfo.HasChanges);
			Assert(container.JC_LCLAvailableInfo.HasChanges);

			Factory.Save();
			AssertEquals(true, ctoLog.SL_IsCancelled);
			AssertEquals(true, cfsLog.SL_IsCancelled);
		}

		public void TestFactorySave_DoNotAddCargoAvailableEvent_OverrideAvailableStorageIsNotChecked()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();

			var sailing = Factory.New<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			var transport = consol.Transports.AddNew();
			transport.JW_JX = sailing.PK;

			var today = ZDateTime.Today;
			transport.JW_TerminalAvailabilityDate = today;
			transport.JW_DepotAvailabilityDate = today.AddDays(1);
			transport.JW_TerminalAvailabilityDate = today.AddDays(2);

			Assert("FCL Override is not checked", !container.JC_OverrideFCLAvailableStorage);
			Assert("LCL Override is not checked", !container.JC_OverrideLCLAvailableStorage);
			Assert("Container mode is not FCL", !container.IsArrivalContainerModeFCLorULD);
			Assert("JC_FCLAvailable should have no changes", !container.JC_FCLAvailableInfo.HasChanges);
			Assert("JC_LCLAvailable should have no changes", !container.JC_LCLAvailableInfo.HasChanges);

			Factory.Save();

			var loadedConsol = Factory.Load<CommonConsol>(consol.PK);
			var loadedContainer = (CommonContainer)loadedConsol.Containers.First();

			Assert("JC_FCLAvailable is filled by JW_TerminalAvailabilityDate on loading", loadedContainer.JC_FCLAvailableInfo.HasChanges);

			loadedContainer.SealNumberForBinding = "123";

			Factory.Save();

			var cAVLog = loadedContainer.Logs.Find(l => l.SL_SE_NKEvent == Events.CargoAvailable.Code).FirstOrDefault();
			AssertNull("No event log should have been added", cAVLog);
		}

		public void TestCargoAvailableEventHasBeenAdded_UpdateAvailableDate()
		{
			AssertAvailableDateUpdated(
				inheritedDate: 1.DaysAgo(),
				eventLocation: "",
				eventFacitity: EventConstants.Facilities.Code.Terminal,
				eventDate: 2.DaysAgo().ToOffset(),
				expectFCLOverride: false,
				expectedFCLDate: 1.DaysAgo(),
				expectLCLOverride: false,
				expectedLCLDate: 1.DaysAgo());

			AssertAvailableDateUpdated(
				inheritedDate: 1.DaysAgo(),
				eventLocation: "UAIEV",
				eventFacitity: EventConstants.Facilities.Code.Terminal,
				eventDate: 2.DaysAgo().ToOffset(),
				expectFCLOverride: true,
				expectedFCLDate: 2.DaysAgo(),
				expectLCLOverride: false,
				expectedLCLDate: 1.DaysAgo());

			AssertAvailableDateUpdated(
				inheritedDate: 1.DaysAgo(),
				eventLocation: "UAIEV",
				eventFacitity: EventConstants.Facilities.Code.Terminal,
				eventDate: 1.DaysAgo().ToOffset(),
				expectFCLOverride: false,
				expectedFCLDate: 1.DaysAgo(),
				expectLCLOverride: false,
				expectedLCLDate: 1.DaysAgo());

			AssertAvailableDateUpdated(
				inheritedDate: 1.DaysAgo(),
				eventLocation: "UAIEV",
				eventFacitity: null,
				eventDate: 2.DaysAgo().ToOffset(),
				expectFCLOverride: true,
				expectedFCLDate: 2.DaysAgo(),
				expectLCLOverride: false,
				expectedLCLDate: 1.DaysAgo());

			AssertAvailableDateUpdated(
				inheritedDate: 1.DaysAgo(),
				eventLocation: "UAIEV",
				eventFacitity: EventConstants.Facilities.Code.Depot,
				eventDate: 2.DaysAgo().ToOffset(),
				expectFCLOverride: false,
				expectedFCLDate: 1.DaysAgo(),
				expectLCLOverride: true,
				expectedLCLDate: 2.DaysAgo());
		}

		void AssertAvailableDateUpdated(ZDateTime inheritedDate, string eventLocation, string eventFacitity, ZDateTimeOffset eventDate, bool expectFCLOverride, ZDateTime expectedFCLDate, bool expectLCLOverride, ZDateTime expectedLCLDate)
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			sailing.JX_DepotAvailabilityDate = inheritedDate;
			sailing.Destination.JB_AvailabilityDate = inheritedDate;
			sailing.Destination.JB_RL_NKPortOfDischarge = "UAIEV";

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKDischargePort = "UAIEV";
			consol.Transports[0].JW_IsLinked = true;
			consol.Transports[0].JW_JX = sailing.PK;
			consol.Transports[0].JW_RL_NKDiscPort = "UAIEV";
			consol.Transports[0].JW_TerminalAvailabilityDate = inheritedDate;

			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = ZDateTime.Empty;
			container.JC_OverrideFCLAvailableStorage = false;
			container.JC_LCLAvailable = ZDateTime.Empty;
			container.JC_OverrideLCLAvailableStorage = false;
			container.Logs.RemoveAndDeleteAll();

			container.Logs.CreateOrRecreateEventLog(
				Events.CargoAvailable,
				EstimateActual.Actual,
				eventDate,
				"MCLAREN",
				Params.Location.AsKeyFor(eventLocation),
				Params.Facility.AsKeyFor(eventFacitity));

			AssertEquals("JC_FCLAvailable", expectedFCLDate, container.JC_FCLAvailable);
			AssertEquals("JC_OverrideFCLAvailableStorage", expectFCLOverride, container.JC_OverrideFCLAvailableStorage);
			AssertEquals("JC_LCLAvailable", expectedLCLDate, container.JC_LCLAvailable);
			AssertEquals("JC_OverrideLCLAvailableStorage", expectLCLOverride, container.JC_OverrideLCLAvailableStorage);
		}

		#endregion

		#region Status Updated Event

		public void TestStatusUpdatedEventHasBeenAdded_UpdateStorageDate()
		{
			var cTO = EventConstants.Facilities.Code.Terminal;
			var lastFreeDay = Constants.EventReferenceParameterTypes.LastFreeDay;

			AssertStorageDateUpdated(null, null, 5.DaysAgo().ToOffset(), false, ZDateTime.Empty);
			AssertStorageDateUpdated("", "", 5.DaysAgo().ToOffset(), false, ZDateTime.Empty);
			AssertStorageDateUpdated("McLaren", "McLaren", 5.DaysAgo().ToOffset(), false, ZDateTime.Empty);
			AssertStorageDateUpdated(cTO, "McLaren", 5.DaysAgo().ToOffset(), false, ZDateTime.Empty);
			AssertStorageDateUpdated(cTO, lastFreeDay, 5.DaysAgo().ToOffset(), true, 4.DaysAgo());
		}

		void AssertStorageDateUpdated(string eventFacitity, string eventType, ZDateTimeOffset eventDate, bool expectFCLOverride, ZDateTime expectedFCLStorage)
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();
			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Empty;
			container.JC_OverrideFCLAvailableStorage = false;
			container.Logs.RemoveAndDeleteAll();

			container.Logs.CreateOrRecreateEventLog(
				Events.StatusUpdated,
				EstimateActual.Actual,
				eventDate,
				"MCLAREN",
				Params.Type.AsKeyFor(eventType),
				Params.Facility.AsKeyFor(eventFacitity));

			AssertEquals("JC_ArrivalCTOStorageStartDate", expectedFCLStorage, container.JC_ArrivalCTOStorageStartDate);
			AssertEquals("JC_OverrideFCLAvailableStorage", expectFCLOverride, container.JC_OverrideFCLAvailableStorage);
		}

		#endregion

		#region GetParametersForEvent

		public void TestGetParametersForEvent_EventIsCargoAvailable_ReturnFacilityParam()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();

			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals("Facility", EventConstants.Facilities.Code.Terminal, container.GetParametersForEvent(Events.CargoAvailable)[Params.Facility]);

			container.JC_ContainerMode = Constants.ContainerModes.LCL;
			AssertEquals("Facility", EventConstants.Facilities.Code.Depot, container.GetParametersForEvent(Events.CargoAvailable)[Params.Facility]);
		}

		public void TestGetParametersForEvent_EventIsCargoAvailable_ReturnLocationParam()
		{
			var consol = Factory.New<CommonConsol>();
			consol.Transports.RemoveAll();
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();

			AssertEquals("Location", "AUSYD", container.GetParametersForEvent(Events.CargoAvailable)[Params.Location]);

			consol.Transports.AddNew("AUSYD", "USNYC");
			consol.Transports.AddNew("USNYC", "UAIEV");
			AssertEquals("Location", "UAIEV", container.GetParametersForEvent(Events.CargoAvailable)[Params.Location]);
		}

		public void TestGetParametersForEvent_GetsFLOAndFULFromTransportLegs()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USNYC";
			consol.Transports.AddNew("USNYC", "UAIEV");
			consol.Transports.AddNew("UAIEV", "UAODS");

			AssertEquals("Location", "AUSYD", container.GetParametersForEvent(Events.FreightLoaded)[Params.Location]);
			AssertEquals("Location", "UAODS", container.GetParametersForEvent(Events.FreightUnloaded)[Params.Location]);

			consol.Transports.RemoveAll();
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_RL_NKDischargePort = "UAIEV";

			AssertEquals("Location", "USNYC", container.GetParametersForEvent(Events.FreightLoaded)[Params.Location]);
			AssertEquals("Location", "UAIEV", container.GetParametersForEvent(Events.FreightUnloaded)[Params.Location]);
		}

		public void TestGetParametersForEvent_GetsFLOAndFULFromTransportLegs_Mode()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "USNYC";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Sea;
			consol.Transports.AddNew("USNYC", "UAIEV");
			consol.Transports.AddNew("UAIEV", "UAODS");
			consol.Transports[2].JW_TransportMode = Constants.TransportModes.Air;

			AssertEquals("FLO Mode", Constants.TransportModes.Sea, container.GetParametersForEvent(Events.FreightLoaded)[Params.Mode]);
			AssertEquals("FUL Mode", Constants.TransportModes.Air, container.GetParametersForEvent(Events.FreightUnloaded)[Params.Mode]);

			consol.Transports.RemoveAll();
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_RL_NKDischargePort = "UAIEV";

			AssertNullOrEmpty("FLO Mode", container.GetParametersForEvent(Events.FreightLoaded)[Params.Mode]);
			AssertNullOrEmpty("FUL Mode", container.GetParametersForEvent(Events.FreightUnloaded)[Params.Mode]);

			consol.JK_TransportMode = Constants.TransportModes.Road;

			AssertEquals("FLO Mode", Constants.TransportModes.Road, container.GetParametersForEvent(Events.FreightLoaded)[Params.Mode]);
			AssertEquals("FUL Mode", Constants.TransportModes.Road, container.GetParametersForEvent(Events.FreightUnloaded)[Params.Mode]);
		}

		#endregion

		#region GetCalculatedVolumeWeight

		readonly string defaultLoadedWeightUnit = "KG";
		readonly string defaultLoadedVolumeUnit = "M3";
		readonly decimal defaultLoadedWeight = 5m;
		readonly decimal defaultLoadedVolume = 10m;
		public void TestGetCalculatedVolumeWeight_InValidParam()
		{
			var consol = Factory.New<CommonConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();
			var container4 = consol.Containers.AddNew();
			var calculatedVolumeWeight1 = container1.GetCalculatedVolumeWeight("", defaultLoadedVolumeUnit, defaultLoadedWeight, defaultLoadedVolume);
			var calculatedVolumeWeight2 = container2.GetCalculatedVolumeWeight(defaultLoadedWeightUnit, "", defaultLoadedWeight, defaultLoadedVolume);
			var calculatedVolumeWeight3 = container3.GetCalculatedVolumeWeight(defaultLoadedWeightUnit, defaultLoadedVolumeUnit, 0m, defaultLoadedVolume);
			var calculatedVolumeWeight4 = container4.GetCalculatedVolumeWeight(defaultLoadedWeightUnit, defaultLoadedVolumeUnit, defaultLoadedWeight, 0m);

			AssertEquals(calculatedVolumeWeight1, 0m);
			AssertEquals(calculatedVolumeWeight2, 0m);
			AssertEquals(calculatedVolumeWeight3, 0m);
			AssertEquals(calculatedVolumeWeight4, 0m);
		}

		public void TestGetCalculatedVolumeWeight_ByVolume()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var container = consol.Containers.AddNew();
			container.JC_TotalUnitOfMeasure = "M3";
			var calculatedVolumeWeight = container.GetCalculatedVolumeWeight(defaultLoadedWeightUnit, defaultLoadedVolumeUnit, defaultLoadedWeight, defaultLoadedVolume);

			AssertEquals(container.IsContainerChargeableByWeight(), false);
			AssertEquals(calculatedVolumeWeight, 0.03m);
		}

		public void TestGetCalculatedVolumeWeight_ByWeight()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var container = consol.Containers.AddNew();
			container.JC_TotalUnitOfMeasure = "KG";
			var calculatedVolumeWeight = container.GetCalculatedVolumeWeight(defaultLoadedWeightUnit, defaultLoadedVolumeUnit, defaultLoadedWeight, defaultLoadedVolume);

			AssertEquals(container.IsContainerChargeableByWeight(), true);
			AssertEquals(calculatedVolumeWeight, 10000m);
		}

		#endregion

		#region ArrivalCTOStorageDays Defaulting

		[TestDate(2009, 09, 09)]
		public void TestArrivalCTOStorageDays()
		{
			var today = new ZDateTime(2009, 09, 09);
			var strategy = new Mock<IContainerDefaultingStrategy>();
			var container = Factory.New<IForwardingContainer>() as CommonContainer;
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				Assert("Pre-condition", container.ArrivalCTOStorageDays.IsEmpty);

				container.JC_FCLAvailable = today;
				container.JC_ArrivalCTOStorageStartDate = today;
				container.JC_FCLWharfGateOut = today.AddDays(1);
				strategy.Setup(x => x.CalculateStorageStart(It.IsAny<ZString>())).Returns(() => (container.JC_FCLAvailable, container.JC_FCLAvailable, "CTO Available"));
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => container.JC_FCLAvailable);
				AssertEquals("Not recalculate the storage days as the container has no parent attached", new ZByte(0), container.ArrivalCTOStorageDays);

				var message = "Storage days should be the amount of days from when storage commences and when the container leaves the wharf inclusive of start and end day";

				container.JC_JK = Factory.New<CommonConsol>().PK;
				container.JC_FCLWharfGateOut = today.AddDays(3);
				AssertEquals(message, new ZByte(4), container.ArrivalCTOStorageDays);

				container.JC_FCLWharfGateOut = today.AddDays(-2);
				AssertEquals("If the container leaves the wharf before storage commences, there's no storage", ZByte.Zero, container.ArrivalCTOStorageDays);

				container.JC_FCLWharfGateOut = today.AddDays(3).AddHours(23).AddMinutes(59);
				AssertEquals("No rounding, should still be 4 days", new ZByte(4), container.ArrivalCTOStorageDays);

				container.JC_FCLWharfGateOut = today.AddDays(300);
				message = "Defaulting only up to ZByte.Max as it should be enough for the business requirements";
				AssertEquals(message, new ZByte(255), container.ArrivalCTOStorageDays);

				container.JC_JK = ZGuid.Empty;
				container.JC_FCLAvailable = today.AddDays(291);
				AssertEquals("Not recalculate the storage days as the container has no parent attached", new ZByte(255), container.ArrivalCTOStorageDays);

				container.JC_JK = Factory.New<CommonConsol>().PK;
				container.JC_FCLAvailable = today.AddDays(290);
				AssertEquals("Changing either date field should recalculate the storage days", new ZByte(11), container.ArrivalCTOStorageDays);

				container.JC_FCLAvailable = new ZDateTime(2020, 4, 3, 23, 0, 0);
				container.JC_FCLWharfGateOut = container.JC_FCLAvailable.AddHours(1);
				AssertEquals("Starting storage state date on one day and container leaving on the next should mean 2 days of storage", new ZByte(2), container.ArrivalCTOStorageDays);

				container.ArrivalCTOStorageDays = 6;
				AssertEquals(new ZByte(6), container.ArrivalCTOStorageDays);

				container.ArrivalCTOStorageDays = 0;
				AssertEquals(ZByte.Zero, container.ArrivalCTOStorageDays);
			}
		}

		#endregion

		#region DepartureCTOStorageDays Defaulting

		[TestDate(2009, 09, 09)]
		public void TestDepartureCTOStorageDays()
		{
			var strategy = new Mock<IContainerDefaultingStrategy>();
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var matchResultMock = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock.Setup(x => x.FreeDays).Returns(1);
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CAR", It.IsAny<ZString>(), null)).Returns(matchResultMock.Object);
				var today = ZDateTime.Today;
				var container = Factory.New<IForwardingContainer>() as CommonContainer;
				Assert("Pre-condition", container.DepartureCTOStorageDays.IsEmpty);

				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLOnBoardVessel, "FCL Loaded"));

				container.JC_FCLWharfGateIn = today;
				container.JC_FCLOnBoardVessel = today.AddDays(3);
				AssertEquals("Not recalculate the storage days as the container has no parent attached", new ZByte(0), container.DepartureCTOStorageDays);

				var message = "Storage days should be the amount of days from when pick up and when the container enter the wharf inclusive of start and end day";

				container.JC_JK = Factory.New<CommonConsol>().PK;
				container.JC_FCLOnBoardVessel = today.AddDays(4);
				AssertEquals(message, new ZByte(0), container.DepartureCTOStorageDays);

				container.JC_FCLOnBoardVessel = today.AddDays(-2);
				AssertEquals("If the container enter the wharf before storage commences, there's no storage", ZByte.Zero, container.DepartureCTOStorageDays);

				container.JC_FCLOnBoardVessel = today.AddDays(4).AddHours(23).AddMinutes(59);
				AssertEquals("No rounding, should still be 0 days", new ZByte(0), container.DepartureCTOStorageDays);

				container.JC_FCLOnBoardVessel = today.AddDays(300);
				message = "Defaulting only up to ZByte.Max as it should be enough for the business requirements";
				AssertEquals(message, new ZByte(0), container.DepartureCTOStorageDays);

				container.JC_JK = ZGuid.Empty;
				container.JC_FCLWharfGateIn = today.AddDays(291);
				AssertEquals("Not recalculate the storage days as the container has no parent attached", new ZByte(0), container.DepartureCTOStorageDays);

				container.JC_JK = Factory.New<CommonConsol>().PK;
				container.JC_FCLWharfGateIn = today.AddDays(290);
				AssertEquals("Changing either date field should recalculate the storage days", new ZByte(0), container.DepartureCTOStorageDays);

				container.JC_FCLWharfGateIn = new ZDateTime(2020, 4, 3, 23, 0, 0);
				container.JC_FCLOnBoardVessel = container.JC_FCLWharfGateIn.AddDays(1).AddHours(1);
				AssertEquals("Starting storage state date on one day and container leaving on the next should mean 0 days of storage", new ZByte(0), container.DepartureCTOStorageDays);

				container.JC_FCLWharfGateIn = new ZDateTime(2020, 4, 3, 23, 0, 0);
				container.JC_FCLOnBoardVessel = container.JC_FCLWharfGateIn.AddHours(1).AddDays(2);
				AssertEquals("Starting storage state date on one day and container leaving on the next should mean 0 days of storage", new ZByte(0), container.DepartureCTOStorageDays);

				container.DepartureCTOStorageDays = 6;
				AssertEquals(new ZByte(6), container.DepartureCTOStorageDays);

				container.DepartureCTOStorageDays = 0;
				AssertEquals(ZByte.Zero, container.DepartureCTOStorageDays);
			}
		}

		#endregion

		[TestDate(2009, 09, 09)]
		public void TestCarrierDetentionPenaltyWhileNoValidFreeDays()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 0 }))
			{
				var consol = Factory.New<CommonConsol>();
				var container = consol.Containers.AddNew();
				AssertNull(container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false));
				AssertNull(container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false));
				container.JC_FCLWharfGateIn = ZDateTime.Now;
				container.JC_FCLWharfGateOut = ZDateTime.Now;
				AssertNull(container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false));
				AssertNull(container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false));
			}
		}

		#region TestDepartureDetentionCarrierDays

		[TestDate(2009, 09, 09)]
		public void TestDepartureCarrierDetentionDays()
		{
			var mockDetentionMatch = new Mock<IContainerPenaltyMatchResult>();
			mockDetentionMatch.Setup(x => x.FreeDays).Returns(1);

			var strategy = new Mock<IContainerDefaultingStrategy>();
			strategy.Setup(x => x.GetMatchedDetentionPenalty(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(mockDetentionMatch.Object);
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var today = ZDateTime.Today;
				var container = Factory.New<IForwardingContainer>() as CommonContainer;
				Assert("Pre-condition", container.DepartureCarrierDetentionDays.IsEmpty);

				strategy.Setup(x => x.CalculateAvailableDateForDetention(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLWharfGateIn, "CTO Gate In"));

				container.JC_FCLWharfGateIn = today.AddDays(3);
				AssertEquals("No change as container has no parent attached", new ZByte(0), container.DepartureCarrierDetentionDays);

				container.JC_ContainerYardEmptyPickupGateOut = today;
				AssertEquals("No change as container has no parent attached", new ZByte(0), container.DepartureCarrierDetentionDays);

				container.JC_JK = Factory.New<CommonConsol>().PK;
				container.JC_FCLWharfGateIn = today.AddDays(5);

				AssertEquals($"Detention days is the amount of days from {container.JC_FCLWharfGateInInfo.HumanReadableName} minus free days minus {container.JC_ContainerYardEmptyPickupGateOutInfo.HumanReadableName} plus 1",
					new ZByte(0), container.DepartureCarrierDetentionDays);

				container.JC_FCLWharfGateIn = today.AddDays(-2);
				AssertEquals("If the container is gate in before the due date there's no detention", ZByte.Zero, container.DepartureCarrierDetentionDays);

				container.JC_FCLWharfGateIn = today.AddDays(3).AddHours(23).AddMinutes(59);
				AssertEquals("No rounding, should still be 0 days", new ZByte(0), container.DepartureCarrierDetentionDays);

				container.JC_FCLWharfGateIn = today.AddDays(300);
				AssertEquals("Defaulting only up to ZByte.Max as it should be enough for the business requirements", new ZByte(0), container.DepartureCarrierDetentionDays);

				container.JC_ContainerYardEmptyPickupGateOut = today.AddDays(290);
				AssertEquals("Changing either date field should recalculate the detention days", new ZByte(0), container.DepartureCarrierDetentionDays);

				container.DepartureCarrierDetentionDays = 6;

				AssertEquals(new ZByte(6), container.DepartureCarrierDetentionDays);

				container.DepartureCarrierDetentionDays = 0;

				AssertEquals(ZByte.Zero, container.DepartureCarrierDetentionDays);
			}
		}

		#endregion

		#region ArrivalCarrierDetentionDays Defaulting

		[TestDate(2010, 10, 10)]
		public void TestArrivalCarrierDetentionDays()
		{
			var mockDetentionMatch = new Mock<IContainerPenaltyMatchResult>();
			mockDetentionMatch.Setup(x => x.FreeDays).Returns(1);

			var strategy = new Mock<IContainerDefaultingStrategy>();
			strategy.Setup(x => x.GetMatchedDetentionPenalty(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(mockDetentionMatch.Object);

			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var today = ZDateTime.Today;
				var container = Factory.New<IForwardingContainer>() as CommonContainer;
				Assert("Pre-condition", container.ArrivalCarrierDetentionDays.IsEmpty);

				container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(1);
				AssertEquals("No change as container has no parent attached", new ZByte(0), container.ArrivalCarrierDetentionDays);

				container.JC_EmptyReturnedBy = today;
				AssertEquals("No change as container has no parent attached", new ZByte(0), container.ArrivalCarrierDetentionDays);

				container.JC_JK = Factory.New<CommonConsol>().PK;
				container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(3);

				var message = "Detention days is the amount of days between when a container should be returned by and when it is returned";
				AssertEquals(message, new ZByte(0), container.ArrivalCarrierDetentionDays);

				container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(-2);
				AssertEquals("If the container is returned before the due date there's no detention", ZByte.Zero, container.ArrivalCarrierDetentionDays);

				container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(3).AddHours(23).AddMinutes(59);
				AssertEquals("No rounding, should still be 0 days", new ZByte(0), container.ArrivalCarrierDetentionDays);

				container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(300);
				message = "Defaulting only up to ZByte.Max as it should be enough for the business requirements";
				AssertEquals(message, new ZByte(0), container.ArrivalCarrierDetentionDays);

				container.JC_EmptyReturnedBy = today.AddDays(290);
				AssertEquals("Changing either date field should recalculate the detention days", new ZByte(0), container.ArrivalCarrierDetentionDays);

				container.ArrivalCarrierDetentionDays = 6;

				AssertEquals(new ZByte(6), container.ArrivalCarrierDetentionDays);

				container.ArrivalCarrierDetentionDays = 0;

				AssertEquals(ZByte.Zero, container.ArrivalCarrierDetentionDays);
			}
		}

		#endregion

		#region Universal Copy

		public void TestUniversalCopyAttributes()
		{
			var instruction = Factory.NewWithValidTestData<CommonContainer>();

			var componentType = instruction.GetType();
			Assert("CommonContainer should have UniversalCopyWithExtendedEntitiesAttribute.", componentType.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), true).Length > 0);

			var praMessagesInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "PRAMessages");
			Assert("PRAMessages collection should have UniversalCopyCollectionEntityAttribute.", praMessagesInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), true).First() != null);

			var servicesInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "Services");
			Assert("Services collection should have UniversalCopyCollectionEntityAttribute.", servicesInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), true).First() != null);
		}

		public void TestUniversalCopyIgnoreElement()
		{
			var container = Factory.NewWithValidTestData<CommonContainer>();
			var componentType = container.GetType();
			var ignoreElementAttributes = componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), true);
			var attribute = ignoreElementAttributes[0] as UniversalCopyIgnoreElementAttribute;
			AssertCollectionContains("OriginPickupConfirms", "OriginPickupConfirms", attribute.ElementNames);
			AssertCollectionContains("DestinationDeliveryConfirms", "DestinationDeliveryConfirms", attribute.ElementNames);
			AssertCollectionContains("OriginCFSArrivalConfirms", "OriginCFSArrivalConfirms", attribute.ElementNames);
			AssertCollectionContains("OriginCFSDepartureConfirms", "OriginCFSDepartureConfirms", attribute.ElementNames);
			AssertCollectionContains("DestinationCFSArrivalConfirms", "DestinationCFSArrivalConfirms", attribute.ElementNames);
			AssertCollectionContains("DestinationCFSDepartureConfirms", "DestinationCFSDepartureConfirms", attribute.ElementNames);
		}

		#endregion

		#region JC_GrossWeightVerificationStatus

		public void TestJC_GrossWeightVerificationStatus()
		{
			var commonConsol = Factory.New<CommonConsol>();
			var container = commonConsol.Containers.AddNew();

			AssertNull(container.MostRecentVGMLogByPostedTime(Events.MessageSent));

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotVerified, container.JC_GrossWeightVerificationStatus);
			Assert(container.JC_GrossWeightVerificationStatusInfo.ReadOnly);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired;
			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotRequired, container.JC_GrossWeightVerificationStatus);
			Assert(container.JC_GrossWeightVerificationStatusInfo.ReadOnly);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent, container.JC_GrossWeightVerificationStatus);
			Assert(container.JC_GrossWeightVerificationStatusInfo.ReadOnly);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotRequired, container.JC_GrossWeightVerificationStatus);
			Assert(container.JC_GrossWeightVerificationStatusInfo.ReadOnly);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent, container.JC_GrossWeightVerificationStatus);
			Assert(container.JC_GrossWeightVerificationStatusInfo.ReadOnly);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod;
			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.NotRequired, container.JC_GrossWeightVerificationStatus);
			Assert(container.JC_GrossWeightVerificationStatusInfo.ReadOnly);

			var eventTime = DateTime.Now;
			var newLog1 = container.Logs.AddNew();
			using (newLog1.LockForUpdatingKeyFieldsForTesting())
			{
				newLog1.SL_SE_NKEvent = Events.MessageSentCode;
				newLog1.SL_Reference = "|MST=Verified Gross Container Weight";
				newLog1.SL_EventTime = eventTime;
				newLog1.SL_IsEstimate = true;
			}

			var log = container.MostRecentVGMLogByPostedTime(Events.MessageSent);
			AssertNotNull(log);
			AssertEquals("MSN event", Events.MessageSentCode, log.SL_SE_NKEvent);
			AssertEquals("MST reference", "|MST=Verified Gross Container Weight", log.SL_Reference);
			AssertEquals("IsEstimate", true, log.SL_IsEstimate);
			AssertEquals("SL_EventTime", eventTime, log.SL_EventTime);

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			AssertEquals(Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent, container.JC_GrossWeightVerificationStatus);
			Assert(container.JC_GrossWeightVerificationStatusInfo.ReadOnly);
		}

		public void TestMostRecentVGMLogByPostedTime()
		{
			var container = Factory.New<CommonContainer>();

			var eventTime = ZDateTime.Today;

			var log1 = CreateVGMLog(container, Events.MessageSent, eventTime);
			Factory.Save();

			Thread.Sleep(1);

			var log2 = CreateVGMLog(container, Events.MessageSent, eventTime.AddDays(-1));
			Factory.Save();

			var mostRecentLogByEventTime = container.Logs.MostRecentLogByEventTime(Events.MessageSent);
			AssertEquals("Most recent log by event time is log1", log1.PK, mostRecentLogByEventTime.PK);

			var mostRecentLogByPostedTime = container.MostRecentVGMLogByPostedTime(Events.MessageSent);
			AssertEquals("Most recent log by posted time is log2", log2.PK, mostRecentLogByPostedTime.PK);
		}

		StmALog CreateVGMLog(CommonContainer container, Event @event, ZDateTime eventTime)
		{
			AssertNotNull(container);

			var log = container.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = @event.Code;
				log.SL_Reference = $"|MST=Verified Gross Container Weight";
				log.SL_EventTime = eventTime;
				log.SL_IsEstimate = true;
			}

			return log;
		}

		#endregion

		#region Container Penalty

		#region ArrivalCTOStorageDays

		public void TestContainerPenalty_ArrivalCTOStorageDays_WithoutConsolDischargePort()
		{
			var consol1 = Factory.New<CommonConsol>();
			var container1 = consol1.Containers.AddNew();
			var container2 = consol1.Containers.AddNew();

			AssertArrivalCTOStorageDays_WithoutDischargePort(container1, container2);
		}

		public void TestContainerPenalty_ArrivalCTOStorageDays_WithoutDeclarationDischargePort()
		{
			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration();
			CreateCusContainer(declaration1, container1);

			var container2 = Factory.New<CommonContainer>();
			var declaration2 = CreateDeclaration();
			CreateCusContainer(declaration2, container2);

			AssertArrivalCTOStorageDays_WithoutDischargePort(container1, container2);
		}

		void AssertArrivalCTOStorageDays_WithoutDischargePort(CommonContainer container1, CommonContainer container2)
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertEquals(0, container1.ImportPenalties.Count);

				container1.ArrivalCTOStorageDays = 5;
				AssertEquals(1, container1.ImportPenalties.Count);
				var penalty = container1.ImportPenalties[0];
				AssertContainerPenalty(penalty,
					Constants.ContainerPenaltyPenaltyType.Codes.Storage,
					Constants.ContainerPenaltyCreditorType.Codes.CTO,
					freeTime: ZDateTime.Empty,
					duration: TimeSpan.FromDays(5),
					timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days,
					currency: "USD");

				var days = container1.ArrivalCTOStorageDays;
				AssertEquals(1, container1.ImportPenalties.Count);
				AssertEquals((ZByte)5, days);
			}

			AssertEquals(0, container2.ImportPenalties.Count);

			var days2 = container2.ArrivalCTOStorageDays;
			AssertEquals(0, container2.ImportPenalties.Count);
			AssertEquals((ZByte)0, days2);

			container2.ArrivalCTOStorageDays = 10;
			AssertEquals(1, container2.ImportPenalties.Count);
			var penalty2 = container2.ImportPenalties[0];
			AssertContainerPenalty(penalty2,
				Constants.ContainerPenaltyPenaltyType.Codes.Storage,
				Constants.ContainerPenaltyCreditorType.Codes.CTO,
				freeTime: ZDateTime.Empty,
				duration: TimeSpan.FromDays(10),
				timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days,
				currency: GetLocalCurrency());
		}

		public void TestContainerPenalty_ArrivalCTOStorageDays_WithConsolDischargePort()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";

			var container1 = consol1.Containers.AddNew();
			AssertArrivalCTOStorageDays_WithDischargePort(container1);
		}

		public void TestContainerPenalty_ArrivalCTOStorageDays_WithDeclarationDischargePort()
		{
			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "IMP");
			CreateCusContainer(declaration1, container1);

			AssertArrivalCTOStorageDays_WithDischargePort(container1);
		}

		void AssertArrivalCTOStorageDays_WithDischargePort(CommonContainer container)
		{
			var days = container.ArrivalCTOStorageDays;
			AssertEquals(0, container.ImportPenalties.Count);
			AssertEquals((ZByte)0, days);

			container.ArrivalCTOStorageDays = 10;
			AssertEquals(1, container.ImportPenalties.Count);
			var penalty = container.ImportPenalties[0];
			AssertContainerPenalty(penalty,
				Constants.ContainerPenaltyPenaltyType.Codes.Storage,
				Constants.ContainerPenaltyCreditorType.Codes.CTO,
				location: "HKHKG",
				freeTime: ZDateTime.Empty,
				duration: TimeSpan.FromDays(10),
				timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days,
				currency: "HKD");
		}

		#endregion

		#region ArrivalCarrierDetentionDays

		public void TestContainerPenalty_ArrivalCarrierDetentionDays_WithConsolDischargePort()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";

			var container1 = consol1.Containers.AddNew();
			AssertArrivalCarrierDetentionDays_WithDischargePort(container1);
		}

		public void TestContainerPenalty_ArrivalCarrierDetentionDays_WithDeclarationDischargePort()
		{
			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "IMP");
			CreateCusContainer(declaration1, container1);

			AssertArrivalCarrierDetentionDays_WithDischargePort(container1);
		}

		void AssertArrivalCarrierDetentionDays_WithDischargePort(CommonContainer container)
		{
			AssertEquals(0, container.ImportPenalties.Count);

			var days = container.ArrivalCarrierDetentionDays;
			AssertEquals(0, container.ImportPenalties.Count);
			AssertEquals((ZByte)0, days);

			container.ArrivalCarrierDetentionDays = 10;
			AssertEquals(1, container.ImportPenalties.Count);
			var penalty = container.ImportPenalties[0];
			AssertContainerPenalty(penalty,
				Constants.ContainerPenaltyPenaltyType.Codes.Detention,
				Constants.ContainerPenaltyCreditorType.Codes.Carrier,
				location: "HKHKG",
				duration: TimeSpan.FromDays(10),
				timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days,
				currency: "HKD");
		}

		#endregion

		#region ArrivalTruckWaitTime

		public void TestContainerPenalty_ArrivalTruckWaitTime_WithConsolDischargePort_WithoutPortTransport()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";

			var container1 = consol1.Containers.AddNew();
			AssertArrivalTruckWaitTime_WithDischargePort(container1);
		}

		public void TestContainerPenalty_ArrivalTruckWaitTime_WithConsolDischargePort_WithPortTransport()
		{
			var org = CreateOrgHeader();
			var address = CreateOrgAddress(org);

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";
			consol1.JK_OA_ArrivalUnpackCFSTransportAddress = address.PK;

			var container1 = consol1.Containers.AddNew();
			AssertArrivalTruckWaitTime_WithDischargePort(container1, address);
		}

		public void TestContainerPenalty_ArrivalTruckWaitTime_WithDeclarationDischargePort_WithoutPortTransport()
		{
			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "IMP");
			CreateCusContainer(declaration1, container1);

			AssertArrivalTruckWaitTime_WithDischargePort(container1);
		}

		public void TestContainerPenalty_ArrivalTruckWaitTime_WithDeclarationDischargePort_WithPortTransport()
		{
			var org = CreateOrgHeader();
			var address = CreateOrgAddress(org);

			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "IMP", address);
			CreateCusContainer(declaration1, container1);

			AssertArrivalTruckWaitTime_WithDischargePort(container1, address);
		}

		void AssertArrivalTruckWaitTime_WithDischargePort(CommonContainer container, OrgAddress portTransport = null)
		{
			AssertEquals(0, container.ImportPenalties.Count);

			var time = container.ArrivalTruckWaitTime;
			AssertEquals(0, container.ImportPenalties.Count);
			AssertEquals(ZDateTime.Empty, time);

			container.ArrivalTruckWaitTime = new ZDateTime(ZDateTime.Now.Year, 1, 1, 3, 20, 0);
			AssertEquals(1, container.ImportPenalties.Count);
			var penalty = container.ImportPenalties[0];
			AssertContainerPenalty(penalty,
				Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
				Constants.ContainerPenaltyCreditorType.Codes.Transport,
				creditor: portTransport,
				location: "HKHKG",
				duration: new TimeSpan(3, 20, 0),
				timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Hours,
				currency: "HKD");
		}

		#endregion

		#region DepartureTruckWaitTime

		public void TestContainerPenalty_DepartureTruckWaitTime_WithConsolLoadPort_WithoutPortTransport()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";

			var container1 = consol1.Containers.AddNew();
			AssertDepartureTruckWaitTime_WithLoadPort(container1);
		}

		public void TestContainerPenalty_DepartureTruckWaitTime_WithConsolLoadPort_WithPortTransport()
		{
			var org = CreateOrgHeader();
			var address = CreateOrgAddress(org);

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";
			consol1.JK_OA_DeparturePackCFSTransportAddress = address.PK;

			var container1 = consol1.Containers.AddNew();
			AssertDepartureTruckWaitTime_WithLoadPort(container1, address);
		}

		public void TestContainerPenalty_DepartureTruckWaitTime_WithDeclarationLoadPort_WithoutPortTransport()
		{
			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "EXP");
			CreateCusContainer(declaration1, container1);

			AssertDepartureTruckWaitTime_WithLoadPort(container1);
		}

		public void TestContainerPenalty_DepartureTruckWaitTime_WithDeclarationLoadPort_WithPortTransport()
		{
			var org = CreateOrgHeader();
			var address = CreateOrgAddress(org);

			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "EXP", address);
			CreateCusContainer(declaration1, container1);

			AssertDepartureTruckWaitTime_WithLoadPort(container1, address);
		}

		void AssertDepartureTruckWaitTime_WithLoadPort(CommonContainer container, OrgAddress portTransport = null)
		{
			AssertEquals(0, container.ExportPenalties.Count);

			var time = container.DepartureTruckWaitTime;
			AssertEquals(0, container.ExportPenalties.Count);
			AssertEquals(ZDateTime.Empty, time);

			container.DepartureTruckWaitTime = new ZDateTime(ZDateTime.Now.Year, 1, 1, 3, 20, 0);
			AssertEquals(1, container.ExportPenalties.Count);
			var penalty = container.ExportPenalties[0];
			AssertContainerPenalty(penalty,
				Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
				Constants.ContainerPenaltyCreditorType.Codes.Transport,
				creditor: portTransport,
				location: "NZAKL",
				duration: new TimeSpan(3, 20, 0),
				timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Hours,
				currency: "NZD");
		}

		#endregion

		#region ArrivalCTOStorageCost

		public void TestContainerPenalty_ArrivalCTOStorageCost_WithConsolDischargePort()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";

			var container1 = consol1.Containers.AddNew();
			AssertArrivalCTOStorageCost_WithDischargePort(container1);
		}

		public void TestContainerPenalty_ArrivalCTOStorageCost_WithDeclarationDischargePort()
		{
			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "IMP");
			CreateCusContainer(declaration1, container1);

			AssertArrivalCTOStorageCost_WithDischargePort(container1);
		}

		void AssertArrivalCTOStorageCost_WithDischargePort(CommonContainer container)
		{
			AssertEquals(0, container.ImportPenalties.Count);

			var cost = container.ArrivalCTOStorageCost;
			AssertEquals(0, container.ImportPenalties.Count);
			AssertEquals(0m, cost);

			container.ArrivalCTOStorageCost = 10.92m;
			AssertEquals(1, container.ImportPenalties.Count);
			var penalty = container.ImportPenalties[0];
			AssertContainerPenalty(penalty,
				Constants.ContainerPenaltyPenaltyType.Codes.Storage,
				Constants.ContainerPenaltyCreditorType.Codes.CTO,
				location: "HKHKG",
				freeTime: ZDateTime.Empty,
				timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days,
				perUnitCost: 10.92m,
				currency: "HKD");
		}

		#endregion

		#region ArrivalCarrierDetentionCost

		public void TestContainerPenalty_ArrivalCarrierDetentionCost_WithConsolDischargePort()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";

			var container1 = consol1.Containers.AddNew();
			AssertArrivalCarrierDetentionCost_WithDischargePort(container1);
		}

		public void TestContainerPenalty_ArrivalCarrierDetentionCost_WithDeclarationDischargePort()
		{
			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "IMP");
			CreateCusContainer(declaration1, container1);

			AssertArrivalCarrierDetentionCost_WithDischargePort(container1);
		}

		void AssertArrivalCarrierDetentionCost_WithDischargePort(CommonContainer container)
		{
			AssertEquals(0, container.ImportPenalties.Count);

			var cost = container.ArrivalCarrierDetentionCost;
			AssertEquals(0, container.ImportPenalties.Count);
			AssertEquals(0m, cost);

			container.ArrivalCarrierDetentionCost = 10.92m;
			AssertEquals(1, container.ImportPenalties.Count);
			var penalty = container.ImportPenalties[0];
			AssertContainerPenalty(penalty,
				Constants.ContainerPenaltyPenaltyType.Codes.Detention,
				Constants.ContainerPenaltyCreditorType.Codes.Carrier,
				location: "HKHKG",
				timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Days,
				perUnitCost: 10.92m,
				currency: "HKD");
		}

		#endregion

		#region ArrivalTruckWaitCost

		public void TestContainerPenalty_ArrivalTruckWaitCost_WithConsolDischargePort_WithoutPortTransport()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";

			var container1 = consol1.Containers.AddNew();

			AssertArrivalTruckWaitCost_WithDischargePort(container1);
		}

		public void TestContainerPenalty_ArrivalTruckWaitCost_WithConsolDischargePort_WithPortTransport()
		{
			var org = CreateOrgHeader();
			var address = CreateOrgAddress(org);

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";
			consol1.JK_OA_ArrivalUnpackCFSTransportAddress = address.PK;

			var container1 = consol1.Containers.AddNew();

			AssertArrivalTruckWaitCost_WithDischargePort(container1, address);
		}

		public void TestContainerPenalty_ArrivalTruckWaitCost_WithDeclarationDischargePort_WithoutPortTransport()
		{
			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "IMP");
			CreateCusContainer(declaration1, container1);

			AssertArrivalTruckWaitCost_WithDischargePort(container1);
		}

		public void TestContainerPenalty_ArrivalTruckWaitCost_WithDeclarationDischargePort_WithPortTransport()
		{
			var org = CreateOrgHeader();
			var address = CreateOrgAddress(org);

			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "IMP", address);
			CreateCusContainer(declaration1, container1);

			AssertArrivalTruckWaitCost_WithDischargePort(container1, address);
		}

		void AssertArrivalTruckWaitCost_WithDischargePort(CommonContainer container, OrgAddress portTransport = null)
		{
			AssertEquals(0, container.ImportPenalties.Count);

			var cost = container.ArrivalTruckWaitCost;
			AssertEquals(0, container.ImportPenalties.Count);
			AssertEquals(0m, cost);

			container.ArrivalTruckWaitCost = 10.92m;
			AssertEquals(1, container.ImportPenalties.Count);
			var penalty = container.ImportPenalties[0];
			AssertContainerPenalty(penalty,
				Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
				Constants.ContainerPenaltyCreditorType.Codes.Transport,
				creditor: portTransport,
				location: "HKHKG",
				timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Hours,
				perUnitCost: 10.92m,
				currency: "HKD");
		}

		#endregion

		#region DepartureTruckWaitCost

		public void TestContainerPenalty_DepartureTruckWaitCost_WithConsolLoadPort_WithoutPortTransport()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";

			var container1 = consol1.Containers.AddNew();

			AssertDepartureTruckWaitCost_WithLoadPort(container1);
		}

		public void TestContainerPenalty_DepartureTruckWaitCost_WithConsolLoadPort_WithPortTransport()
		{
			var org = CreateOrgHeader();
			var address = CreateOrgAddress(org);

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "HKHKG";
			consol1.JK_OA_DeparturePackCFSTransportAddress = address.PK;

			var container1 = consol1.Containers.AddNew();

			AssertDepartureTruckWaitCost_WithLoadPort(container1, address);
		}

		public void TestContainerPenalty_DepartureTruckWaitCost_WithDeclarationDischargePort_WithoutPortTransport()
		{
			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "EXP");
			CreateCusContainer(declaration1, container1);

			AssertDepartureTruckWaitCost_WithLoadPort(container1);
		}

		public void TestContainerPenalty_DepartureTruckWaitCost_WithDeclarationLoadPort_WithPortTransport()
		{
			var org = CreateOrgHeader();
			var address = CreateOrgAddress(org);

			var container1 = Factory.New<CommonContainer>();
			var declaration1 = CreateDeclaration("NZAKL", "HKHKG", "EXP", address);
			CreateCusContainer(declaration1, container1);

			AssertDepartureTruckWaitCost_WithLoadPort(container1, address);
		}

		void AssertDepartureTruckWaitCost_WithLoadPort(CommonContainer container, OrgAddress portTransport = null)
		{
			AssertEquals(0, container.ExportPenalties.Count);

			var cost = container.DepartureTruckWaitCost;
			AssertEquals(0, container.ExportPenalties.Count);
			AssertEquals(0m, cost);

			container.DepartureTruckWaitCost = 10.92m;
			AssertEquals(1, container.ExportPenalties.Count);
			var penalty = container.ExportPenalties[0];
			AssertContainerPenalty(penalty,
				Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
				Constants.ContainerPenaltyCreditorType.Codes.Transport,
				creditor: portTransport,
				location: "NZAKL",
				timeUnit: Constants.ContainerPenaltyTimeUnit.Codes.Hours,
				perUnitCost: 10.92m,
				currency: "NZD");
		}

		#endregion

		#region SupportsContainerPenalties

		public void TestGetSupportsContainerPenalties()
		{
			var consol = Factory.New<CommonConsol>();
			var consolContainer = consol.Containers.AddNew();

			var declarationContainer = Factory.New<CommonContainer>();
			var declaration = CreateDeclaration();
			CreateCusContainer(declaration, declarationContainer);

			var noAttachedContainer = Factory.New<CommonContainer>();

			AssertEquals("Consol Container supports container penalties", true, consolContainer.SupportsContainerPenalties);
			AssertEquals("Declaration Container supports container penalties", true, declarationContainer.SupportsContainerPenalties);
			AssertEquals("Container without Consol or Declaration does not support container penalties", false, noAttachedContainer.SupportsContainerPenalties);
		}

		#endregion

		#region Container Penalty Implemenation

		OrgAddress CreateOrgAddress(OrgHeader org)
		{
			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Code = "TESTADDRESS1";
			return address;
		}

		BusinessObject CreateDeclaration(ZString origin = default,
			ZString finalDestination = default,
			ZString messageType = default,
			OrgAddress deliveryAddress = null)
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_TransportMode] = Constants.TransportModes.Sea;
			declaration[JobDeclarationSchema.JE_MessageType] = messageType;
			declaration[JobDeclarationSchema.JE_RL_NKOrigin] = origin;
			declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = finalDestination;
			declaration["JE_OA_DeliveryOrPickupCartageCoAddr"] = deliveryAddress?.PK ?? ZGuid.Empty;

			return declaration;
		}

		void CreateCusContainer(BusinessObject declaration, CommonContainer container)
		{
			var cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC] = container.PK;
		}

		ZString GetLocalCurrency()
		{
			return (GlbCompany.CurrentCompany?.LocalCurrency?.RX_Code).GetValueOrDefault();
		}

		void AssertContainerPenalty(ContainerPenalty penalty,
			ZString penaltyType,
			ZString creditorType,
			OrgAddress creditor = null,
			ZString location = default,
			ZDateTime freeTime = default,
			ZDateTime duration = default,
			ZString timeUnit = default,
			ZDecimal perUnitCost = default,
			ZString currency = default)
		{
			var durationAsDays = ContainerPenalty.ConvertDateTimeToDays(duration);

			AssertEquals(penaltyType, penalty.CPY_PenaltyType);
			AssertEquals(creditorType, penalty.CPY_CreditorType);
			AssertEquals(creditor?.OA_OH ?? ZGuid.Empty, penalty.CPY_OH_Creditor);
			AssertEquals(location, penalty.CPY_RL_NKLocation);
			AssertEquals(freeTime, penalty.CPY_FreeTime);
			AssertEquals(durationAsDays, penalty.DurationAsDays);
			AssertEquals(timeUnit, penalty.CPY_TimeUnit);
			AssertEquals(perUnitCost, penalty.CPY_PerUnitCost);
			AssertEquals(currency, penalty.CPY_RX_NKCurrency);
		}

		#endregion

		#endregion

		public void TestDeletionLoggingDetails_WhenJC_JKChanged()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;

			var consol = Factory.NewWithValidTestData<CommonConsol>();

			var container = consol.Containers.AddNew();
			cusContainer[CusContainerSchema.CO_JC] = container.PK;
			container.JC_JK = ZGuid.BrettsGuid;

			container.Delete();

			AssertContains($"JC_JK changed from '{consol.PK}' to '{ZGuid.BrettsGuid}'", container.DeletionLoggingDetails);
		}

		#region Implementation

		CommonConsol Consol;
		CommonContainer Container;

		protected override void SetUp()
		{
			base.SetUp();

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != HomePort.SubstringSafe(0, 2))
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = HomePort.SubstringSafe(0, 2);
			}

			Consol = GetNewConsol();
			Container = Consol.Containers.AddNew();
		}

		protected virtual CommonContainer GetNewContainer()
		{
			return Factory.New<CommonContainer>();
		}

		protected virtual CommonConsol GetNewConsol()
		{
			return Factory.New<CommonConsol>();
		}

		#endregion
	}
}
