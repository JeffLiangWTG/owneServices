using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.DataTransfer;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Test
{
	public class AgencyContainerWorkflowTemplateApplicationExtenderTest : TestCaseWithFactory
	{
		#region OriginCountry / DestinationCountry

		public void TestOriginAndDestinationCountry()
		{
			Shipment.RealContainers.Add(Container);
			Shipment.JS_NKLoadPort = "AUSYD";
			Shipment.JS_NKDischargePort = "NZAKL";
			AssertEquals("Origin for RealContainers", Shipment.CalcLoadPort.RL_RN_NKCountryCode, PortCoundtryApplicationExtender.OriginCountry(Container));
			AssertEquals("Destination for RealContainers", Shipment.CalcDischargePort.RL_RN_NKCountryCode, PortCoundtryApplicationExtender.DestinationCountry(Container));
			Shipment.RealContainers.Remove(Container);
			AssertEquals("No Origin", ZString.Empty, PortCoundtryApplicationExtender.OriginCountry(Container));
			AssertEquals("No Destination", ZString.Empty, PortCoundtryApplicationExtender.DestinationCountry(Container));
			Shipment.BookedContainers.Add(Container);
			AssertEquals("Origin for BookedContainers", Shipment.CalcLoadPort.RL_RN_NKCountryCode, PortCoundtryApplicationExtender.OriginCountry(Container));
			AssertEquals("Destination for BookedContainers", Shipment.CalcDischargePort.RL_RN_NKCountryCode, PortCoundtryApplicationExtender.DestinationCountry(Container));
		}

		#endregion

		#region IsCondition1or2Met - Common Items

		public void TestIsCondition1or2Met_ForImport()
		{
			AssertEquals(false, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.Import));
			AssertEquals(false, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.Import, ""));
			Shipment.JS_RL_NKOrigin = "MYPKG";
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals(true, Shipment.IsImport());
			AssertEquals(true, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.Import));
			AssertEquals(true, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.Import, ""));
		}

		public void TestIsCondition1or2Met_ForExport()
		{
			AssertEquals(false, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.Export));
			AssertEquals(false, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.Export, ""));
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals(true, Shipment.IsExport());
			AssertEquals(true, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.Export));
			AssertEquals(true, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.Export, ""));
		}

		public void TestIsCondition1or2Met_ForDomestic()
		{
			AssertEquals(false, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.Domestic));
			AssertEquals(false, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.Domestic, ""));
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort.Substring(0, 2) + "XXX";
			AssertEquals(true, Shipment.IsDomestic());
			AssertEquals(true, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.Domestic));
			AssertEquals(true, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.Domestic, ""));
		}

		#endregion

		#region IsCondition1Met
		public void TestIsCondition1Met_ForOriginDifferentFromFirstLoad()
		{
			Shipment.JS_NKLoadPort = "AUSYD";
			Shipment.JS_NKDischargePort = "MYPKG";
			Shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals(false, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));
			Shipment.JS_RL_NKOrigin = "AUMEL";
			AssertEquals(true, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));
		}

		public void TestIsCondition1Met_ForDestinationDifferentFromFinalDischarge()
		{
			Shipment.JS_NKLoadPort = "AUSYD";
			Shipment.JS_NKDischargePort = "MYPKG";
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals(false, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));
			Shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals(true, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));
		}

		#endregion

		#region IsCondition2Met
		public void TestIsCondition2Met_ForConfirmed()
		{
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertEquals(true, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.Confirmed));
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.OriginPickup;
			AssertEquals(false, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.Confirmed));
		}

		public void TestIsCondition2Met_ForBooked()
		{
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertEquals(true, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.Booked));
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.OriginPickup;
			AssertEquals(false, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.Booked));
		}

		public void TestIsCondition2Met_ForWaitListed()
		{
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			AssertEquals(true, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.WaitListed));
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.OriginPickup;
			AssertEquals(false, ApplicationExtender.IsCondition1Met(Container, AgencyContainerWorkflowCondition1CodeList.Codes.WaitListed));
		}

		public void TestIsCondition2Met_ForFCL()
		{
			Container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(true, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.FCL, ""));
			Container.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			AssertEquals(false, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.FCL, ""));
		}

		public void TestIsCondition2Met_ForLCL()
		{
			Container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(true, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.LCL, ""));
			Container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(false, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.LCL, ""));
		}

		public void TestIsCondition2Met_ForBCN()
		{
			Container.JC_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals(true, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.BCN, ""));
			Container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(false, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.BCN, ""));
		}

		public void TestIsCondition2Met_ForGRP()
		{
			Container.JC_ContainerMode = Core.Constants.ContainerModes.Groupage;
			AssertEquals(true, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.GRP, ""));
			Container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(false, ApplicationExtender.IsCondition2Met(Container, AgencyContainerWorkflowCondition2CodeList.Codes.GRP, ""));
		}

		#endregion

		#region Implementation

		AgencyShipmentContainer Container
		{
			get
			{
				return container ??= Factory.NewWithValidTestData<AgencyShipmentContainer>();
			}
		}
		AgencyShipmentContainer container;

		AgencyShipment Shipment
		{
			get
			{
				return shipment ??= Factory.NewWithValidTestData<AgencyShipment>();
			}
		}
		AgencyShipment shipment;

		AgencyContainerWorkflowTemplateApplicationExtender applicationExtender;
		IWorkflowTemplateApplicationExtender ApplicationExtender
		{
			get
			{
				return applicationExtender ??= new AgencyContainerWorkflowTemplateApplicationExtender();
			}
		}
		IPortCountryCode PortCoundtryApplicationExtender
		{
			get
			{
				return applicationExtender ??= new AgencyContainerWorkflowTemplateApplicationExtender();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Container.JC_JS_FCLBookingOnlyLink = Shipment.PK;
		}

		#endregion
	}
}
