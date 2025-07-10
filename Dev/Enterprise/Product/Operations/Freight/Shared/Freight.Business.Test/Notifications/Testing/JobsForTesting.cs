using System.ComponentModel;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobsForTesting : NUnit.Framework.Assertion
	{
		public JobsForTesting(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		#region Consol

		public CommonConsol DomesticConsol
		{
			get
			{
				if (domesticConsol == null)
				{
					domesticConsol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
					domesticConsol.JK_RL_NKLoadPort = "AUPER";
					domesticConsol.JK_RL_NKDischargePort = "AUSYD";

					Assert(((IImportExport)domesticConsol).JobDirection == Directions.Domestic);
				}
				return domesticConsol;
			}
		}
		CommonConsol domesticConsol;

		public CommonConsol CrossTradeConsol
		{
			get
			{
				if (crossTradeConsol == null)
				{
					crossTradeConsol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
					crossTradeConsol.JK_RL_NKLoadPort = "USLAX";
					crossTradeConsol.JK_RL_NKDischargePort = "MYPKG";

					Assert(((IImportExport)crossTradeConsol).JobDirection == Directions.CrossTrade);
				}
				return crossTradeConsol;
			}
		}
		CommonConsol crossTradeConsol;

		public CommonConsol ImportConsol
		{
			get
			{
				if (fImportConsol == null)
				{
					fImportConsol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
					fImportConsol.JK_RL_NKLoadPort = "MYPKG";
					fImportConsol.JK_RL_NKDischargePort = "AUSYD";

					Assert(((IImportExport)fImportConsol).JobDirection == Directions.Import);
				}
				return fImportConsol;
			}
		}
		CommonConsol fImportConsol;

		public CommonConsol ImportCommonConsol
		{
			get
			{
				if (fImportCommonConsol == null)
				{
					fImportCommonConsol = Factory.New<CommonConsol>();
					fImportCommonConsol.JK_RL_NKLoadPort = "MYPKG";
					fImportCommonConsol.JK_RL_NKDischargePort = "AUSYD";

					Assert(((IImportExport)fImportCommonConsol).JobDirection == Directions.Import);
				}
				return fImportCommonConsol;
			}
		}
		CommonConsol fImportCommonConsol;

		public CommonConsol ExportConsol
		{
			get
			{
				if (fExportConsol == null)
				{
					fExportConsol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
					fExportConsol.JK_RL_NKLoadPort = "AUSYD";
					fExportConsol.JK_RL_NKDischargePort = "USLAX";

					Assert(((IImportExport)fExportConsol).JobDirection == Directions.Export);
				}
				return fExportConsol;
			}
		}
		CommonConsol fExportConsol;

		#endregion

		#region CFS Load List

		public CommonConsol ImportLoadList
		{
			get
			{
				if (fImportLoadList == null)
				{
					fImportLoadList = (CommonConsol)Factory.New<CFS.ICFSLoadListConsol>();
					fImportLoadList.Transports.AddNew();
					fImportLoadList.JK_RL_NKLoadPort = "MYPKG";
					fImportLoadList.JK_RL_NKDischargePort = "AUSYD";

					Assert(((IImportExport)fImportLoadList).JobDirection == Directions.Import);
				}
				return fImportLoadList;
			}
		}
		CommonConsol fImportLoadList;

		public CommonConsol ExportLoadList
		{
			get
			{
				if (fExportLoadList == null)
				{
					fExportLoadList = (CommonConsol)Factory.New<CFS.ICFSLoadListConsol>();
					fImportLoadList.Transports.AddNew();
					fExportLoadList.JK_RL_NKLoadPort = "AUSYD";
					fExportLoadList.JK_RL_NKDischargePort = "USLAX";

					Assert(((IImportExport)fExportLoadList).JobDirection == Directions.Export);
				}
				return fExportLoadList;
			}
		}
		CommonConsol fExportLoadList;

		#endregion

		#region Shipment

		public IQuotedBooking BookingShipment
		{
			get { return bookingShipment ?? (bookingShipment = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory)); }
		}
		IQuotedBooking bookingShipment;

		#endregion

		#region AgencyBooking

		public CommonShipment ExportAgencyBooking
		{
			get
			{
				if (exportAgencyBooking == null)
				{
					exportAgencyBooking = (CommonShipment)Factory.New<Integration.Agency.IAgencyBooking>();
					exportAgencyBooking.JS_RL_NKOrigin = "AUSYD";
					exportAgencyBooking.JS_RL_NKDestination = "USLAX";
					exportAgencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;

					Assert(((IImportExport)exportAgencyBooking).JobDirection == Directions.Export);
				}
				return exportAgencyBooking;
			}
		}
		CommonShipment exportAgencyBooking;

		public CommonShipment ImportAgencyBooking
		{
			get
			{
				if (importAgencyBooking == null)
				{
					importAgencyBooking = (CommonShipment)Factory.New<Integration.Agency.IAgencyBooking>();
					importAgencyBooking.JS_RL_NKOrigin = "MYPKG";
					importAgencyBooking.JS_RL_NKDestination = "AUSYD";
					importAgencyBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;

					Assert(((IImportExport)importAgencyBooking).JobDirection == Directions.Import);
				}
				return importAgencyBooking;
			}
		}
		CommonShipment importAgencyBooking;

		#endregion

		#region AgencyDocumentation

		public CommonShipment DomesticAgencyDocumentation
		{
			get
			{
				if (domesticAgencyDocumentation == null)
				{
					domesticAgencyDocumentation = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
					domesticAgencyDocumentation.JS_RL_NKOrigin = "AUPER";
					domesticAgencyDocumentation.JS_RL_NKDestination = "AUSYD";
					domesticAgencyDocumentation.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

					Assert(((IImportExport)domesticAgencyDocumentation).JobDirection == Directions.Domestic);
				}
				return domesticAgencyDocumentation;
			}
		}
		CommonShipment domesticAgencyDocumentation;

		public CommonShipment CrossTradeAgencyDocumentation
		{
			get
			{
				if (crossTradeDomesticAgencyDocumentation == null)
				{
					crossTradeDomesticAgencyDocumentation = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
					crossTradeDomesticAgencyDocumentation.JS_RL_NKOrigin = "USLAX";
					crossTradeDomesticAgencyDocumentation.JS_RL_NKDestination = "MYPKG";
					crossTradeDomesticAgencyDocumentation.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

					Assert(((IImportExport)crossTradeDomesticAgencyDocumentation).JobDirection == Directions.CrossTrade);
				}
				return crossTradeDomesticAgencyDocumentation;
			}
		}
		CommonShipment crossTradeDomesticAgencyDocumentation;

		public CommonShipment ImportAgencyDocumentation
		{
			get
			{
				if (importAgencyDocumentation == null)
				{
					importAgencyDocumentation = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
					importAgencyDocumentation.JS_RL_NKOrigin = "MYPKG";
					importAgencyDocumentation.JS_RL_NKDestination = "AUSYD";
					importAgencyDocumentation.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

					Assert(((IImportExport)importAgencyDocumentation).JobDirection == Directions.Import);
				}
				return importAgencyDocumentation;
			}
		}
		CommonShipment importAgencyDocumentation;

		public CommonShipment ImportAgencyDocumentation2
		{
			get
			{
				if (importAgencyDocumentation2 == null)
				{
					importAgencyDocumentation2 = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
					importAgencyDocumentation2.JS_RL_NKOrigin = "MYPKG";
					importAgencyDocumentation2.JS_RL_NKDestination = "AUSYD";
					importAgencyDocumentation2.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

					Assert(((IImportExport)importAgencyDocumentation2).JobDirection == Directions.Import);
				}
				return importAgencyDocumentation2;
			}
		}
		CommonShipment importAgencyDocumentation2;

		public CommonShipment ExportAgencyDocumentation
		{
			get
			{
				if (exportAgencyDocumentation == null)
				{
					exportAgencyDocumentation = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
					exportAgencyDocumentation.JS_RL_NKOrigin = "AUSYD";
					exportAgencyDocumentation.JS_RL_NKDestination = "USLAX";
					exportAgencyDocumentation.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

					Assert(((IImportExport)exportAgencyDocumentation).JobDirection == Directions.Export);
				}
				return exportAgencyDocumentation;
			}
		}
		CommonShipment exportAgencyDocumentation;

		public CommonShipment ExportAgencyDocumentation2
		{
			get
			{
				if (exportAgencyDocumentation2 == null)
				{
					exportAgencyDocumentation2 = (CommonShipment)Factory.New<Integration.Agency.IBillOfLading>();
					exportAgencyDocumentation2.JS_RL_NKOrigin = "AUSYD";
					exportAgencyDocumentation2.JS_RL_NKDestination = "USLAX";
					exportAgencyDocumentation2.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;

					Assert(((IImportExport)exportAgencyDocumentation2).JobDirection == Directions.Export);
				}
				return exportAgencyDocumentation2;
			}
		}
		CommonShipment exportAgencyDocumentation2;

		#endregion

		#region Declaration

		public BusinessObject ImportDeclaration
		{
			get
			{
				if (fImportDeclaration == null)
				{
					fImportDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					fImportDeclaration[JobDeclarationSchema.JE_MessageType] = "IMP";
					fImportDeclaration[JobDeclarationSchema.Constants.JE_TransportMode] = Core.Constants.TransportModes.Sea;
					fImportDeclaration[JobDeclarationSchema.Constants.JE_ContainerMode] = Core.Constants.ContainerModes.Containerised;
					fImportDeclaration[JobDeclarationSchema.JE_VesselName] = "DecVessel";
					fImportDeclaration[JobDeclarationSchema.JE_VoyageFlightNo] = "DevVoyage";
					fImportDeclaration[JobDeclarationSchema.JE_HouseBill] = "HouseBill";
					AssertEquals(true, ((IImportExport)fImportDeclaration).IsImport());
				}
				return fImportDeclaration;
			}
		}
		BusinessObject fImportDeclaration;

		public BusinessObject ImportDeclaration2
		{
			get
			{
				if (fImportDeclaration2 == null)
				{
					fImportDeclaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					fImportDeclaration2[JobDeclarationSchema.JE_MessageType] = "IMP";
					fImportDeclaration2[JobDeclarationSchema.Constants.JE_TransportMode] = Core.Constants.TransportModes.Sea;
					fImportDeclaration2[JobDeclarationSchema.Constants.JE_ContainerMode] = Core.Constants.ContainerModes.Containerised;
					fImportDeclaration2[JobDeclarationSchema.JE_VesselName] = "DecVessel";
					fImportDeclaration2[JobDeclarationSchema.JE_VoyageFlightNo] = "DevVoyage";
					fImportDeclaration2[JobDeclarationSchema.JE_HouseBill] = "HouseBill";
					AssertEquals(true, ((IImportExport)fImportDeclaration2).IsImport());
				}
				return fImportDeclaration2;
			}
		}
		BusinessObject fImportDeclaration2;

		public BusinessObject ExportDeclaration
		{
			get
			{
				if (fExportDeclaration == null)
				{
					fExportDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					fExportDeclaration[JobDeclarationSchema.JE_MessageType] = "EXP";
					fExportDeclaration[JobDeclarationSchema.Constants.JE_TransportMode] = Core.Constants.TransportModes.Sea;
					fExportDeclaration[JobDeclarationSchema.Constants.JE_ContainerMode] = Core.Constants.ContainerModes.Containerised;
					fExportDeclaration[JobDeclarationSchema.JE_VesselName] = "DecVessel";
					fExportDeclaration[JobDeclarationSchema.JE_VoyageFlightNo] = "DevVoyage";
					fExportDeclaration[JobDeclarationSchema.JE_HouseBill] = "HouseBill";
					AssertEquals(true, ((IImportExport)fExportDeclaration).IsExport());
				}
				return fExportDeclaration;
			}
		}
		BusinessObject fExportDeclaration;

		public BusinessObject ExportDeclaration2
		{
			get
			{
				if (fExportDeclaration2 == null)
				{
					fExportDeclaration2 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					fExportDeclaration2[JobDeclarationSchema.JE_MessageType] = "EXP";
					fExportDeclaration2[JobDeclarationSchema.Constants.JE_TransportMode] = Core.Constants.TransportModes.Sea;
					fExportDeclaration2[JobDeclarationSchema.Constants.JE_ContainerMode] = Core.Constants.ContainerModes.Containerised;
					fExportDeclaration2[JobDeclarationSchema.JE_VesselName] = "DecVessel";
					fExportDeclaration2[JobDeclarationSchema.JE_VoyageFlightNo] = "DevVoyage";
					fExportDeclaration2[JobDeclarationSchema.JE_HouseBill] = "HouseBill";
					AssertEquals(true, ((IImportExport)fExportDeclaration2).IsExport());
				}
				return fExportDeclaration2;
			}
		}
		BusinessObject fExportDeclaration2;

		public BusinessObjectCollection ImportDeclarationCusContainers
		{
			get
			{
				PropertyDescriptor property = TypeDescriptor.GetProperties(ImportDeclaration)["CusContainers"];
				return (BusinessObjectCollection)property.GetValue(ImportDeclaration);
			}
		}

		public BusinessObjectCollection ExportDeclarationCusContainers
		{
			get
			{
				PropertyDescriptor property = TypeDescriptor.GetProperties(ExportDeclaration)["CusContainers"];
				return (BusinessObjectCollection)property.GetValue(ExportDeclaration);
			}
		}

		#endregion

		#region Orders

		public BusinessObject Order
		{
			get
			{
				PropertyDescriptor orderProperty = TypeDescriptor.GetProperties(OrderLineDeliverContainer)["Order"];
				return (BusinessObject)orderProperty.GetValue(OrderLineDeliverContainer);
			}
		}

		public BusinessObject OrderLineDelivery
		{
			get
			{
				PropertyDescriptor orderLineDeliveryProperty = TypeDescriptor.GetProperties(OrderLineDeliverContainer)["OrderLineDelivery"];
				return (BusinessObject)orderLineDeliveryProperty.GetValue(OrderLineDeliverContainer);
			}
		}

		public BusinessObject OrderLineDeliverContainer
		{
			get
			{
				if (fOrderLineDeliverContainer == null)
				{
					fOrderLineDeliverContainer = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Integration.Forwarding.IOrderLineDeliverContainer)));
				}
				return fOrderLineDeliverContainer;
			}
		}
		BusinessObject fOrderLineDeliverContainer;

		#endregion

		readonly BusinessObjectFactory Factory;
	}
}
