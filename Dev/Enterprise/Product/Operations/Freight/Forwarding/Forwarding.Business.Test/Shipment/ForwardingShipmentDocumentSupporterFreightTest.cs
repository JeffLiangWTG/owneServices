using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Business.CommonShipmentDocumentSupporter;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentDocumentSupporterFreightTest : BaseFreightTest
	{
		public void TestQueryProvider()
		{
			ForwardingShipmentDocumentSupporterForTest documentSupporter = new ForwardingShipmentDocumentSupporterForTest(Factory.New<ForwardingShipment>());
			IForwardingShipmentDocumentSupporterQueryProvider queryProvider = documentSupporter.QueryProvider;
			AssertNotNull(queryProvider);
			Assert(queryProvider is ForwardingShipmentDocumentSupporterQueryProvider);

			Factory.SetValue<IForwardingShipmentDocumentSupporterQueryProvider, ForwardingShipmentDocumentSupporterQueryProviderForTest>();

			documentSupporter = new ForwardingShipmentDocumentSupporterForTest(Factory.New<ForwardingShipment>());
			queryProvider = documentSupporter.QueryProvider;
			AssertNotNull(queryProvider);
			Assert(queryProvider is ForwardingShipmentDocumentSupporterQueryProviderForTest);

			IForwardingShipmentDocumentSupporterQueryProvider queryProviderInSaveTransaction = null;
			Factory.Saving += _ => queryProviderInSaveTransaction = documentSupporter.QueryProvider;
			Factory.Save();
			AssertNotNull(queryProviderInSaveTransaction);
			Assert(queryProviderInSaveTransaction is ForwardingShipmentDocumentSupporterQueryProvider);
		}

		public void TestGenericFreightJobFromShipmentIgnoresTheDeclaration()
		{
			AssertDataContextIgnoresTheDeclaration(Core.Constants.DataContext.GenericFreightJobFromShipment, "FreightWrapperFromShipment");
			AssertDataContextIgnoresTheDeclaration(Core.Constants.DataContext.GenericFreightJobFrmShipByContIfFCL, "FreightWrapperFromShipment");
		}

		void AssertDataContextIgnoresTheDeclaration(DataContext dataContext, string wrapperName)
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var documentSupporter = shipment.DocumentSupporter;
			var documentWrappers = documentSupporter.GetDocumentWrappers(dataContext, null);
			AssertEquals("documentWrappers.Length", 1, documentWrappers.Length);
			var documentWrapper = documentWrappers[0];
			AssertNotNull("documentWrappers[0]", documentWrapper);
			AssertEquals("documentWrapper.GetType().Name", wrapperName, documentWrapper.GetType().Name);

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			documentWrappers = documentSupporter.GetDocumentWrappers(dataContext, null);
			AssertEquals("documentWrappers.Length", 1, documentWrappers.Length);
			documentWrapper = documentWrappers[0];
			AssertNotNull("documentWrappers[0]", documentWrapper);
			AssertEquals("documentWrapper.GetType().Name", wrapperName, documentWrapper.GetType().Name);
		}

		public void TestGenericPickupDeliveryConfirmIgnoresTheDeclaration()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.JS_OuterPacks = 10;
			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Now;
			shipment.DocsAndCartage.JP_PickupRequiredBy = ZDateTime.Now.AddHours(2);
			shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Now.AddHours(3);

			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = "Pickup Delivery Confirm doc";
			menuItem.SU_DocumentDirection = "DEP";

			var documentSupporter = shipment.DocumentSupporter;
			var documentWrappers = documentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericPickupDeliveryConfirm, menuItem);
			AssertEquals("documentWrappers.Length", 1, documentWrappers.Length);
			var documentWrapper = documentWrappers[0];
			AssertNotNull("documentWrappers[0]", documentWrapper);
			AssertEquals("documentWrapper.GetType().Name", "FreightWrapperFromShipment", documentWrapper.GetType().Name);

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			documentWrappers = documentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericPickupDeliveryConfirm, menuItem);
			AssertEquals("documentWrappers.Length", 1, documentWrappers.Length);
			documentWrapper = documentWrappers[0];
			AssertNotNull("documentWrappers[0]", documentWrapper);
			AssertEquals("documentWrapper.GetType().Name", "FreightWrapperFromShipment", documentWrapper.GetType().Name);
		}

		public void TestDocumentEventsHandlers()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var documentSupporter = new ForwardingShipmentDocumentSupporter(shipment);

			var awbBarcodeLabelMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			awbBarcodeLabelMenuItem.SU_MenuName = "AWB Barcode Label";
			AssertDocumentEventHandlersCanHandleMenuItem(documentSupporter, awbBarcodeLabelMenuItem);

			var otherAWBMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			otherAWBMenuItem.SU_MenuName = "Other AWB Document";
			AssertDocumentEventHandlersCanHandleMenuItem(documentSupporter, otherAWBMenuItem);
		}

		void AssertDocumentEventHandlersCanHandleMenuItem(DocumentSupporter documentSupporter, IStmMenuItem menuItem)
		{
			var result = false;

			var documentEventsHandlers = documentSupporter.DocumentEventsHandlers;
			foreach (var handler in documentEventsHandlers)
			{
				result = handler.CanHandleMenuItem(menuItem);
				if (result)
				{
					break;
				}
			}

			Assert(string.Format("[{0}] can not handle MenuItem [{1}].", documentSupporter.GetType().ToString(), menuItem.SU_MenuName), result);
		}

		public void TestGenericFreightJobByContainerIfFCL()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			ForwardingContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "OOCL0000006";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			ForwardingContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "OOCL0000013";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			ForwardingContainer container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "OOCL0000027";
			container3.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "NZAKL";

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "PLT";
			packLine1.JL_JC = container1.PK;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 1;
			packLine2.JL_F3_NKPackType = "PLT";
			packLine2.JL_JC = container2.PK;

			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			ForwardingShipmentDocumentSupporter docSupporter = (ForwardingShipmentDocumentSupporter)shipment.DocumentSupporter;

			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			DocumentWrapper[] wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobByContainerIfFCL, menuItem);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			DocumentWrapper freightWrapper = wrappers[0];
			PropertyInfo propertyInfo = freightWrapper.GetType().GetProperty("Containers");
			DocumentWrapperCollection containers = (DocumentWrapperCollection)propertyInfo.GetValue(freightWrapper, null);
			AssertEquals("freightWrapper.Containers.Count", 2, containers.Count);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobByContainerIfFCL, menuItem);
			AssertEquals("wrappers.Length", 2, wrappers.Length);
			freightWrapper = wrappers[0];
			propertyInfo = freightWrapper.GetType().GetProperty("Containers");
			containers = (DocumentWrapperCollection)propertyInfo.GetValue(freightWrapper, null);
			AssertEquals("freightWrapper.Containers.Count", 1, containers.Count);

			menuItem.SU_MenuName = "IMO Dangerous Goods Declaration";
			wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobByContainerIfFCL, menuItem);
			AssertEquals("wrappers.Length", 0, wrappers.Length);

			packLine2.UNDGs.AddNew();
			wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobByContainerIfFCL, menuItem);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			freightWrapper = wrappers[0];
			propertyInfo = freightWrapper.GetType().GetProperty("Containers");
			containers = (DocumentWrapperCollection)propertyInfo.GetValue(freightWrapper, null);
			AssertEquals("freightWrapper.Containers.Count", 1, containers.Count);
		}

		public void TestServicerWrappersForDocBuilder()
		{
			DataContext dataContext = Core.Constants.DataContext.GenericFreightJobServices;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.Services.AddNew();

			ForwardingShipmentDocumentSupporter documentSupporter = (ForwardingShipmentDocumentSupporter)shipment.DocumentSupporter;
			DocumentWrapper[] documentWrappersToTest = documentSupporter.GetDocumentWrappers(dataContext, null);

			DocumentWrapper freightWrapper = documentWrappersToTest[0];
			PropertyInfo propertyInfo = freightWrapper.GetType().GetProperty("Services");
			DocumentWrapperCollection services = (DocumentWrapperCollection)propertyInfo.GetValue(freightWrapper, null);

			AssertEquals("1 Service expected.", 1, services.Count);
		}

		public void TestPopulateAWBWhenHAWBPrinted()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "BLAH";

			ForwardingShipment shipment = GetShipment();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(0m, shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight);

			shipment.JS_ActualChargeable = 100m;
			AssertEquals(0m, shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight);

			DocumentCancelEventArgs args = new DocumentCancelEventArgs(menuItem);
			var documentSupporter = shipment.DocumentSupporter;
			var documentEvents = new IDocumentEventsMock();
			documentSupporter.Initialise(documentEvents);
			documentEvents.NotifyDocumentPrintRequested(args);
			AssertEquals(0m, shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight);

			menuItem.SU_MenuName = "AWB Barcode Label";
			Factory.Save();
			documentEvents.NotifyDocumentPrintRequested(args);
			AssertEquals(100m, shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight);

			shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight = 10m;
			Factory.Save();
			menuItem.SU_MenuName = "Laser HAWB";
			documentEvents.NotifyDocumentPrintRequested(args);
			AssertEquals(100m, shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight);

			shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight = 10m;
			Factory.Save();
			menuItem.SU_MenuName = "Laser HAWB with Folder on Page";
			documentEvents.NotifyDocumentPrintRequested(args);
			AssertEquals(100m, shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight);

			shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight = 10m;
			Factory.Save();
			menuItem.SU_MenuName = "Neutral HAWB";
			documentEvents.NotifyDocumentPrintRequested(args);
			AssertEquals(100m, shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight);

			shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight = 10m;
			Factory.Save();
			menuItem.SU_MenuName = "HAWB Barcode Label";
			documentEvents.NotifyDocumentPrintRequested(args);
			AssertEquals(100m, shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight);

			shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight = 10m;
			Factory.Save();
			menuItem.SU_MenuName = "AWB Security Declaration";
			documentEvents.NotifyDocumentPrintRequested(args);
			AssertEquals(100m, shipment.AWBHeader.AWBRateLines[0].ER_ChargeableWeight);
		}

		public void TestSADHIsSupportedAndWorking()
		{
			ForwardingShipment shipment = GetShipment();
			AssertEquals("SADH is supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.SADH)));
			DocumentWrapper[] result = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.SADH, null);
			AssertEquals("GetDocumentWrappers with no Declaration", null, result);

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.EU.IJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			result = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.SADH, null);
			AssertEquals("GetDocumentWrappers with a Declaration but no Entry Headers", 0, result.Length);
		}

		public void TestESSADHIsSupportedAndWorking()
		{
			ForwardingShipment shipment = GetShipment();
			AssertEquals("ESSADH is supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ESSADH)));
			DocumentWrapper[] result = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ESSADH, null);
			AssertEquals("GetDocumentWrappers with no Declaration", null, result);

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.ES.IJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			result = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ESSADH, null);
			AssertEquals("GetDocumentWrappers with a Declaration but no Entry Headers", 0, result.Length);
		}

		public void TestFRSADHIsSupportedAndWorking()
		{
			var shipment = GetShipment();
			AssertEquals("FRSADH is supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.FRSADH)));
			var result = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.FRSADH, null);
			AssertEquals("GetDocumentWrappers with no Declaration", null, result);

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.FR.IJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			result = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.FRSADH, null);
			AssertEquals("GetDocumentWrappers with a Declaration but no Entry Headers", 0, result.Length);
		}

		public void TestLiquidationDetailsIsSupportedAndWorking()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				ForwardingShipment shipment = GetShipment();
				AssertEquals("LiquidationDetails is supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.LiquidationDetails)));
				DocumentWrapper[] result = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LiquidationDetails, null);
				AssertEquals("GetDocumentWrappers with no Declaration", null, result);

				BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.FR.IJobDeclaration>();
				declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
				result = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LiquidationDetails, null);
				AssertEquals("GetDocumentWrappers with a Declaration but no Entry Headers", 0, result.Length);
			}
		}

		public void TestEuNctsIsSupportedAndWorking()
		{
			var shipment = GetShipment();
			AssertEquals("Is EuNcts DataContext supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.EuNcts)));
			DocumentWrapper[] result = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.EuNcts, commandBeingRun: null);
			AssertEquals("GetDocumentWrappers without NCTS header", null, result);

			var nctsHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();
			nctsHeader[CusInBondHeaderSchema.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader[CusInBondHeaderSchema.BH_ParentID] = shipment.PK;
			var nctsDepartureMovement = (BusinessObject)Factory.New<Enterprise.Integration.Customs.EU.NCTS.IDepartureMovementHeader>();
			nctsDepartureMovement[CusInBondMoveHeaderSchema.BM_BH] = nctsHeader.PK;
			result = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.EuNcts, commandBeingRun: null);
			AssertEquals("GetDocumentWrappers with a departure NCTS header", 1, result.Length);
		}

		public void TestInBond7512DepartureIsSupported()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var dataSourceType = new DataContextValue(ForwardingShipmentDocumentSupporter.InBond7512Departure);
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var docSupporter = shipment.DocumentSupporter;
			AssertEquals("InBond7512Departure is supported", true, docSupporter.IsDataContextSupported(dataSourceType));
			var boDocDataProviders = docSupporter.GetBODocDataProviders(dataSourceType, null);
			AssertEquals("no inbond", 0, boDocDataProviders.Length);

			var ams = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondHeader>();
			ams.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
			ams.BH_ParentID = shipment.PK;
			ams.BH_ParentTableCode = shipment.TablePrefix;
			var amsMoveHeader = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			amsMoveHeader.BM_BH = ams.PK;

			var inbond = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond.BH_ParentID = shipment.PK;
			inbond.BH_ParentTableCode = shipment.TablePrefix;
			var inbondMoveHeader = Factory.New<Enterprise.Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			inbondMoveHeader.BM_BH = inbond.PK;
			Factory.Save();
			AssertEquals("InBond7512Departure is supported", true, docSupporter.IsDataContextSupported(dataSourceType));
			boDocDataProviders = docSupporter.GetBODocDataProviders(dataSourceType, null);
			AssertEquals("boDocDataProviders.Length with a InBond", 1, boDocDataProviders.Length);
		}

		public void TestGetSupportedBOBusinessSource()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			AssertEquals("InBond7512Departure is supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.InBond7512Departure)));
			AssertEquals("ForwardingShipment is supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ForwardingShipment)));
		}

		public void TestSupportedChildBusinessContexts()
		{
			ForwardingShipment shipment = GetShipment();
			AssertEquals(5, shipment.DocumentSupporter.SupportedChildBusinessContexts.Length);
			AssertEquals(BusinessContext.Shipment, shipment.DocumentSupporter.SupportedChildBusinessContexts[0]);
			AssertEquals(BusinessContext.SubShipment, shipment.DocumentSupporter.SupportedChildBusinessContexts[1]);
			AssertEquals(BusinessContext.Customs, shipment.DocumentSupporter.SupportedChildBusinessContexts[2]);
			AssertEquals(BusinessContext.DtbBooking, shipment.DocumentSupporter.SupportedChildBusinessContexts[3]);
			AssertEquals(BusinessContext.CusHAWB, shipment.DocumentSupporter.SupportedChildBusinessContexts[4]);
		}

		public void TestGetChildCollection()
		{
			ForwardingShipment shipment = GetShipment();
			AssertEquals(1, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.Shipment, null).Length);
			AssertEquals(shipment, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.Shipment, null)[0]);

			AssertEquals(0, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.Customs, null).Length);

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			AssertEquals(1, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.Customs, null).Length);
			AssertEquals(declaration, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.Customs, null)[0]);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

			var subShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment1.JS_JS_ColoadMasterShipment = shipment.PK;

			var subShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment2.JS_JS_ColoadMasterShipment = shipment.PK;

			AssertEquals(2, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.SubShipment, null).Length);
			AssertEquals(subShipment1, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.SubShipment, null)[0]);
			AssertEquals(subShipment2, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.SubShipment, null)[1]);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertEquals(0, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.SubShipment, null).Length);

			// Test Document Packs in BCN shipments
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			var subShipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipment3.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment3.JS_JS_ColoadMasterShipment = shipment.PK;

			var subShipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipment4.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment4.JS_JS_ColoadMasterShipment = shipment.PK;

			AssertEquals(2, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.SubShipment, null).Length);
			AssertEquals(subShipment3, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.SubShipment, null)[0]);
			AssertEquals(subShipment4, shipment.DocumentSupporter.GetChildCollection(null, BusinessContext.SubShipment, null)[1]);
		}

		public void TestGetChildCollection_DtbBooking()
		{
			var shipment = GetShipment();
			var docSupporter = shipment.DocumentSupporter;

			AssertEquals("normally should show", true, docSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, null));

			var menu = Factory.New<StmMenuItem>();
			menu.SU_DocumentDirection = "ARV";
			AssertEquals(0, docSupporter.GetChildCollection(menu, BusinessContext.DtbBooking, null).Length);

			AssertEquals("Issues are handled in TB, so should not show reason for not printing", false, docSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, menu));
		}

		public void TestGetChildCollection_DtbBooking_WithTransportBookings_ShowsReasonForNotPrinting()
		{
			var originalIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;

				var shipment = GetShipmentWithOneBooking();
				var docSupporter = shipment.DocumentSupporter;

				Factory.Save();

				var menu = Factory.New<StmMenuItem>();
				menu.SU_DocumentDirection = "ARV";
				AssertEquals("Precondition: need to have a transport booking", 1, docSupporter.GetChildCollection(menu, BusinessContext.DtbBooking, null).Length);

				AssertEquals("Has transport bookings, so should show reason for not printing", true, docSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, menu));
			}
			finally
			{
				Globals.IsUserInteractive = originalIsUserInteractive;
			}
		}

		public ForwardingShipment GetShipmentWithOneBooking()
		{
			var shipment = GetShipment();

			var consolidation = Factory.New<IDtbBookingConsolidation>();
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = shipment.TablePrefix;
			consolidation.KB_JobDirection = ((IDtbBookingParent)shipment).GetSupportedDirections()[0].ToString();

			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;

			return shipment;
		}

		[GuiTest]
		public void TestGetChildCollection_DtbBooking_WithContainers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container1 = CreateContainer(consol, "20GP", "CNT-1");
			var container2 = CreateContainer(consol, "20GP", "CNT-2");
			var container3 = CreateContainer(consol, "20GP", "CNT-3");

			var shipment = consol.Shipments.AddNew();
			var packLine1 = CreateOuterPackages(shipment, container1, 5, Constants.PkgUnit.Bag);
			var packLine2 = CreateOuterPackages(shipment, container1, 4, Constants.PkgUnit.Box);
			var packLine3 = CreateOuterPackages(shipment, container2, 3, Constants.PkgUnit.Pallet);

			Factory.Save();

			var documentSupporter = shipment.DocumentSupporter;
			var menu = Factory.New<StmMenuItem>();
			menu.SU_DocumentDirection = "ARV";
			var tbHelper = new TransportBookingSharedTestHelper(Factory);
			using (tbHelper.LastDocumentOptionsForTestStartRecording())
			{
				documentSupporter.GetChildCollection(menu, BusinessContext.DtbBooking, null);
				ITransportBookingDocumentOptions documentOptions = tbHelper.LastDocumentOptionsForTest();
				AssertEquals(2, documentOptions.Containers.Count);
				documentOptions.Containers.Cast<IDtbDocumentContainerOption>().Single(c => c.ContainerNumber == "CNT-1");
				documentOptions.Containers.Cast<IDtbDocumentContainerOption>().Single(c => c.ContainerNumber == "CNT-2");
			}
		}

		CommonContainer CreateContainer(ForwardingConsol consol, string containerType, string containerNum)
		{
			var container = consol.Containers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, containerType)).PK;
			container.JC_ContainerNum = containerNum;

			return container;
		}

		ForwardingPackLine CreateOuterPackages(ForwardingShipment shipment, CommonContainer container, ZInt packs, ZString packsUQ)
		{
			var pack = shipment.OuterPackLines.AddNew();
			pack.JL_JC = container.PK;
			pack.JL_PackageCount = packs;
			pack.JL_F3_NKPackType = packsUQ;

			return pack;
		}

		public void TestGetDocBusinessObjectForImportCargoLabel()
		{
			var shipment = GetShipment();
			shipment.JS_OuterPacks = 5;

			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);

			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.SetupSequence(m => m.GetImportCargoLabelToPrint(It.IsAny<DocumentImportCargoLabel>()))
				.Returns((DocumentImportCargoLabel)null)
				.CallBase();
			var wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericImportCargoLabel, null);
			AssertEquals("Import Cargo Label wrapper should be null", null, wrapper);
			queryProvider
				.SetupSequence(m => m.GetImportCargoLabelToPrint(It.IsAny<DocumentImportCargoLabel>()))
				.Returns(new DocumentImportCargoLabel(shipment))
				.CallBase();
			wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericImportCargoLabel, null);
			AssertEquals("Import Cargo Label wrapper should be created", 5, wrapper.Length);
			AssertEquals("Wrapper type should be FreightWrapperFromShipment", "FreightWrapperFromShipment", wrapper[0].GetType().Name);
		}

		public void TestAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms()
		{
			AssertAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms(Core.Constants.CountryCodes.Australia);
			AssertAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms(Core.Constants.CountryCodes.NewZealand);
			AssertAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms(Core.Constants.CountryCodes.UnitedStates);
			AssertAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms(Core.Constants.CountryCodes.SouthAfrica);
			AssertAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms(Core.Constants.CountryCodes._TemplateCountryName_);
		}

		void AssertAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms(string countryCode)
		{
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			DataContextValue dataSourceType = new DataContextValue(".JobDeclaration");
			ForwardingShipment shipment = GetShipment();
			DocumentSupporter docSupporter = shipment.DocumentSupporter;
			AssertEquals("docSupporter.IsDataContextSupported(dataSourceType) with no Dec", true, docSupporter.IsDataContextSupported(dataSourceType));
			IBODocDataProvider[] boDocDataProviders = docSupporter.GetBODocDataProviders(dataSourceType, null);
			AssertEquals("boDocDataProviders with no Dec", null, boDocDataProviders);

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;

			AssertEquals("docSupporter.IsDataContextSupported(dataSourceType) with a Dec", true, docSupporter.IsDataContextSupported(dataSourceType));
			boDocDataProviders = docSupporter.GetBODocDataProviders(dataSourceType, null);
			AssertEquals("boDocDataProviders.Length with a Dec", 1, boDocDataProviders.Length);
			AssertEquals("boDocDataProviders[0] with a Dec", declaration, BODocDataProvider.GetBusinessObject(boDocDataProviders[0]));

			BusinessObject nctsHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();
			nctsHeader[CusInBondHeaderSchema.Constants.BH_ParentID] = shipment.PK;
			nctsHeader[CusInBondHeaderSchema.Constants.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS4;
			boDocDataProviders = docSupporter.GetBODocDataProviders(new DataContextValue("EuNcts"), null);
			AssertEquals("boDocDataProviders.Length with a nctsHeader", 1, boDocDataProviders.Length);
			AssertEquals("boDocDataProviders[0] with a nctsHeader", nctsHeader, boDocDataProviders[0].ParentBusinessObject);
		}

		public void TestSecurityMenuTemplateFilterValueForEuNcts()
		{
			var shipment = GetShipment();
			var documentSupporter = shipment.DocumentSupporter;
			var menuTemplateFilterValue = documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.Security, dataProvider: null);
			AssertNull("When no linked NctsHeader, GetMenuTemplateFilterValue", menuTemplateFilterValue);

			BusinessObject nctsHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();
			nctsHeader[CusInBondHeaderSchema.Constants.BH_ParentID] = shipment.PK;
			nctsHeader[CusInBondHeaderSchema.Constants.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS4;
			var dataProvider = documentSupporter.GetBODocDataProviders(new DataContextValue("EuNcts"), null)[0];
			menuTemplateFilterValue = documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.Security, dataProvider);
			AssertEquals("When linked NctsHeader without SafetyAndSecurityFlag, GetMenuTemplateFilterValue", "N", menuTemplateFilterValue);
		}

		public void TestGetDocBusinessObjectForGenericFreightJob()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			ForwardingShipment shipment = GetShipment();
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJob, null);
			AssertNotEquals("Wrappers with Shipment", null, wrappers);
			AssertEquals("Wrappers.Length with Shipment", 1, wrappers.Length);
			AssertNotEquals("Wrappers[0] with Shipment", null, wrappers[0]);
			AssertEquals("((BusinessObject)wrappers[0].WrappedObject).PK with Shipment", shipment.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		public void TestGetDocBusinessObjectForGenericCommercialInvoice()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			ForwardingShipment shipment = GetShipment();
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericCommercialInvoice, null);
			AssertEquals("Wrappers when No Declaration Present", null, wrappers);

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericCommercialInvoice, null);
			AssertNotEquals("Wrappers with Declaration with no Invoices", null, wrappers);
			AssertEquals("Wrappers.Length with Declaration with no Invoices", 0, wrappers.Length);

			BusinessObject invoiceGroupHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceGroupHeader>();
			invoiceGroupHeader[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration.PK;
			IBusinessObjectCollection invoices = (IBusinessObjectCollection)ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>().GetProperty("Invoices").GetGetMethod().Invoke(declaration, null);
			BusinessObject invoiceHeader = invoices.AddNew();
			invoiceHeader[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration.PK;
			invoiceHeader[JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK] = invoiceGroupHeader.PK;

			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericCommercialInvoice, null);
			AssertNotEquals("Wrappers with Declaration with a Commercial Invoice", null, wrappers);
			AssertEquals("Wrappers.Length with Declaration with a Commercial Invoice", 1, wrappers.Length);
			AssertNotEquals("Wrappers[0] with Declaration with a Commercial Invoice", null, wrappers[0]);
			AssertEquals("((BusinessObject)wrappers[0].WrappedObject).PK with Declaration with a Commercial Invoice", invoiceHeader.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		public void TestGetDocBusinessObjectForGenericFreightJobByComInv()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			ForwardingShipment shipment = GetShipment();
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobByComInv, null);
			AssertEquals("Wrappers when No Declaration Present", null, wrappers);

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJobByComInv, null);
			AssertNotEquals("Wrappers with Declaration with no Invoices", null, wrappers);
			AssertEquals("Wrappers.Length with Declaration with no Invoices", 0, wrappers.Length);

			BusinessObject invoiceGroupHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceGroupHeader>();
			invoiceGroupHeader[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration.PK;
			IBusinessObjectCollection invoices = (IBusinessObjectCollection)ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>().GetProperty("Invoices").GetGetMethod().Invoke(declaration, null);
			BusinessObject invoiceHeader1 = invoices.AddNew();
			invoiceHeader1[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration.PK;
			invoiceHeader1[JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK] = invoiceGroupHeader.PK;
			BusinessObject invoiceHeader2 = invoices.AddNew();
			invoiceHeader2[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration.PK;
			invoiceHeader2[JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK] = invoiceGroupHeader.PK;

			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJobByComInv, null);
			AssertNotEquals("Wrappers with Declaration with a Commercial Invoice", null, wrappers);
			AssertEquals("Wrappers.Length with Declaration with a Commercial Invoice", 2, wrappers.Length);
			AssertNotEquals("Wrappers[0] with Declaration with a Commercial Invoice", null, wrappers[0]);
			AssertEquals("((BusinessObject)wrappers[0].WrappedObject).PK with Declaration with a Commercial Invoice", declaration.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		public void TestGetDocBusinessObjectForGenericFreightJobInvoice()
		{
			AccTransactionHeader invoiceConsignee = GetInvoice("00001001");
			AccTransactionHeader invoiceConsignor = GetInvoice("00001002");
			AccTransactionHeader invoiceLocalClient = GetInvoice("00001003");
			AccTransactionHeader invoiceAgent = GetInvoice("00001004");
			Factory.Save();

			ForwardingShipment shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "S00001111";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = invoiceConsignee.Header.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = invoiceConsignor.Header.MainAddress.PK;
			JobHeader job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = invoiceLocalClient.Header.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = invoiceAgent.Header.MainAddress.PK;

			invoiceConsignee.AH_JH = job.PK;
			invoiceConsignor.AH_JH = job.PK;
			invoiceLocalClient.AH_JH = job.PK;
			invoiceAgent.AH_JH = job.PK;

			AssertCorrectJobInvoiceObjects(shipment, ContactType.NoContactType, invoiceLocalClient);
			AssertCorrectJobInvoiceObjects(shipment, ContactType.Consignee, invoiceConsignee);
			AssertCorrectJobInvoiceObjects(shipment, ContactType.Consignor, invoiceConsignor);
			AssertCorrectJobInvoiceObjects(shipment, ContactType.ImportAirFreightAgent, invoiceAgent);
			AssertCorrectJobInvoiceObjects(shipment, ContactType.ImportSeaFreightAgent, invoiceAgent);
			AssertCorrectJobInvoiceObjects(shipment, ContactType.ExportAirFreightAgent, invoiceAgent);
			AssertCorrectJobInvoiceObjects(shipment, ContactType.ExportSeaFreightAgent, invoiceAgent);
			AssertCorrectJobInvoiceObjects(shipment, ContactType.All, invoiceLocalClient);
			AssertCorrectJobInvoiceObjects(shipment, ContactType.Receivables, invoiceLocalClient);
			AssertCorrectJobInvoiceObjects(shipment, null, invoiceLocalClient);

			job.JH_OA_LocalChargesAddr = ZGuid.Empty;
			AssertCorrectJobInvoiceObjects(shipment, ContactType.Receivables, null);
		}

		void AssertCorrectJobInvoiceObjects(ForwardingShipment shipment, ContactType contactType, AccTransactionHeader expectedInvoice)
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = contactType != null ? contactType.Code : "";
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobInvoice, menuItem);

			if (expectedInvoice == null)
			{
				AssertNull(wrappers);
			}
			else
			{
				AssertEquals(1, wrappers.Length);
				AssertEquals(expectedInvoice.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
			}
		}

		AccTransactionHeader GetInvoice(ZString invoiceNumber)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = invoiceNumber;

			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = invoiceNumber;
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00001111";
			newInvoice.AH_TransactionReference = invoiceNumber;

			return newInvoice;
		}

		public void TestGetDocBusinessObjectForGenericFreightJobInvoice_UnknownMenuItem()
		{
			ForwardingShipment shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "S00001111";

			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobInvoice, null);
			AssertNull("Can't create the correct wrapper if we don't know the contact type", wrappers);
		}

		public void TestGetDocBusinessObjectForIMOUnContainerisedPackLine()
		{
			StmMenuItem menuItem = new BusinessObjectFactory().New<StmMenuItem>();
			menuItem.SU_DocumentDirection = "DEP";
			ForwardingShipment shipment = GetShipment();
			CommonConsol consol = shipment.Consols.AddNew();
			PackLine line = shipment.OuterPackLines.AddNew();
			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals("Count", 1, shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.IMO, menuItem).Length);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			AssertNull("Should be null", shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.IMO, menuItem));
		}

		public void TestGetDocBusinessObjects()
		{
			ForwardingShipment shipment = GetShipment();
			GlbCompany.CurrentCompany.SetCountry("AU");
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null);
			AssertEquals("Wrappers", null, wrappers);

			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ShipmentDeclaration, null);
			AssertEquals("ShipmentDeclaration", 1, wrappers.Length);
			AssertNotNull("Contents of array is not null", wrappers[0]);

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;

			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.Declaration, null);
			AssertEquals("Array size with a declaration", 1, wrappers.Length);
			AssertNotNull("Contents of array is not null", wrappers[0]);

			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Notes, null);
			AssertEquals("Wrapper created for Notes data context", 1, wrappers.Length);
			AssertNotNull("Shipment wrapper for Notes shouldn't be null", wrappers[0]);
		}

		public void TestGetDocBusinessObjectForWorksheet()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			DocumentWrapper[] wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Worksheet, null);
			AssertEquals("Shipment wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocForwardingShipment", "DocForwardingShipment", wrapper[0].GetType().Name);

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Worksheet, null);
			AssertEquals("Shipment wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocForwardingShipment", "DocForwardingShipment", wrapper[0].GetType().Name);
		}

		public void TestGetDocBusinessObjectsForCFSAndForwardingShipment()
		{
			ForwardingShipment shipment = GetShipment();
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CFSAndForwardingShipment, null);
			AssertEquals("Wrapper created for CFSAndForwardingShipment data context", 1, wrappers.Length);
			AssertNotNull("Shipment wrapper for CFSAndForwardingShipment shouldn't be null", wrappers[0]);
		}

		public void TestGetDocBusinessObjectsForCombinedCartageAdvice()
		{
			ForwardingShipment shipment = GetShipment();
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CombinedCartageAdvice, null);
			AssertEquals("Should have returned 1 DocShipment wrapper.", 1, wrappers.Length);
			AssertNotNull("DocForwardingShipment wrapper should not be null.", wrappers[0]);

			DocumentWrapper shipmentWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingShipment, Factory.New<ForwardingShipment>());
			AssertEquals("Wrapper is of type DocForwardingShipment", shipmentWrapper.GetType(), wrappers[0].GetType());
		}

		public void TestGetDocBusinessObjectsForERA()
		{
			ForwardingShipment shipment = GetShipment();
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ERA, null);
			AssertEquals("Should have returned 1 DocShipment wrapper.", 1, wrappers.Length);
			AssertNotNull("DocForwardingShipment wrapper should not be null.", wrappers[0]);

			DocumentWrapper shipmentWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.ForwardingShipment, Factory.New<ForwardingShipment>());
			AssertEquals("Wrapper is of type DocForwardingShipment", shipmentWrapper.GetType(), wrappers[0].GetType());
		}

		public void TestGetDocBusinessObjectsForImportCartageAdvice()
		{
			StmMenuItem menuItem = new BusinessObjectFactory().New<StmMenuItem>();
			menuItem.SU_DocumentDirection = "ARV";

			ForwardingShipment shipment = GetShipment();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper", 0, wrappers.Length);

			CommonConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKDischargePort = OverseasPort;
			consol1.JK_RL_NKLoadPort = HomePort;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper", 0, wrappers.Length);

			PackLine pack1 = shipment.OuterPackLines.AddNew();
			CommonContainer cont1 = consol1.Containers.AddNew();
			cont1.PackLines.Add(pack1);
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper is created", 1, wrappers.Length);

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKDischargePort = OverseasPort2;
			consol2.JK_RL_NKLoadPort = OverseasPort;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper", 0, wrappers.Length);

			PackLine pack2 = shipment.OuterPackLines.AddNew();
			CommonContainer cont2 = consol2.Containers.AddNew();
			cont2.PackLines.Add(pack2);
			PackLine pack3 = shipment.OuterPackLines.AddNew();
			CommonContainer cont3 = consol2.Containers.AddNew();
			cont3.PackLines.Add(pack3);
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Container Wrapper is created", 2, wrappers.Length);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper is created", 1, wrappers.Length);

			consol1.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper is created", 1, wrappers.Length);

			consol2.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Container Wrapper is created", 2, wrappers.Length);
		}

		public void TestGetDocBusinessObjectsForExportCartageAdvice()
		{
			StmMenuItem menuItem = new BusinessObjectFactory().New<StmMenuItem>();
			menuItem.SU_DocumentDirection = "DEP";

			ForwardingShipment shipment = GetShipment();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper", 0, wrappers.Length);

			CommonConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKDischargePort = HomePort;
			consol1.JK_RL_NKLoadPort = OverseasPort2;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper", 0, wrappers.Length);

			PackLine pack1 = shipment.OuterPackLines.AddNew();
			CommonContainer cont1 = consol1.Containers.AddNew();

			cont1.PackLines.Add(pack1);
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper is created", 1, wrappers.Length);

			CommonConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKDischargePort = OverseasPort2;
			consol2.JK_RL_NKLoadPort = OverseasPort;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper", 0, wrappers.Length);

			PackLine pack2 = shipment.OuterPackLines.AddNew();
			CommonContainer cont2 = consol2.Containers.AddNew();
			cont2.PackLines.Add(pack2);
			PackLine pack3 = shipment.OuterPackLines.AddNew();
			CommonContainer cont3 = consol2.Containers.AddNew();
			cont3.PackLines.Add(pack3);
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.CartageAdvice, menuItem);
			AssertEquals("Container Wrapper is created", 2, wrappers.Length);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper is created", 1, wrappers.Length);

			consol1.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper is created", 1, wrappers.Length);

			consol2.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, menuItem);
			AssertEquals("Shipment Wrapper is created", 1, wrappers.Length);
		}

		public void TestGetDocBusinessObjectForFreightLabels()
		{
			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider.Object);

			var shipment = Factory.New<ForwardingShipment>();

			queryProvider
				.SetupSequence(m =>
					m.GetDocumentOptions(It.Is<DocumentShipment>(docShipment => docShipment.Shipment == shipment)))
				.Returns((DocumentShipment)null)
				.CallBase();
			var wrapper = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.FreightLabels, null);
			AssertEquals("Shipment wrapper should be null", null, wrapper);
			queryProvider.SetupSequence(m =>
					m.GetDocumentOptions(It.Is<DocumentShipment>(docShipment => docShipment.Shipment == shipment)))
				.Returns(new DocumentShipment(shipment, DataContext.FreightLabels))
				.CallBase();
			wrapper = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.FreightLabels, null);
			AssertEquals("Shipment wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocForwardingShipment", "DocForwardingShipment", wrapper[0].GetType().Name);
		}

		#region GetGenericWrapperForOuterPacks

		public void TestGetGenericWrapperForOuterPacks()
		{
			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider.Object);

			var shipment = Factory.New<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = "PLT";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 12;
			packLine2.JL_F3_NKPackType = "PLT";

			var documentShipment = new DocumentShipment(shipment, DataContext.GenericFreightJobByPackages);

			documentShipment.IncludeConsignee = true;
			documentShipment.IncludeConsignor = true;
			documentShipment.IncludeNone = true;
			documentShipment.NumberOfLabelsToPrint = 0;

			queryProvider.Setup(m => m.GetDocumentOptions(It.IsAny<DocumentShipment>())).Returns(documentShipment);
			var wrappers = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobByPackages, null);
			foreach (var documentWrapper in wrappers)
			{
				AssertEquals("Include Consignee", true, documentWrapper.DocIncludeConsignee);
				AssertEquals("Include Consignor", true, documentWrapper.DocIncludeConsignor);
				AssertEquals("Include None", true, documentWrapper.DocIncludeNone);
				AssertEquals("NumberOfLabelsToPrint", 0, documentWrapper.DocNumberOfLabelsToPrint);
			}

			AssertEquals("Shipment wrappers should be created", 22, wrappers.Length);
			documentShipment = new DocumentShipment(shipment, DataContext.GenericFreightJobByPackages);
			documentShipment.IncludeConsignor = false;
			documentShipment.IncludeConsignee = false;
			documentShipment.IncludeNone = false;
			documentShipment.NumberOfLabelsToPrint = 17;

			queryProvider.Setup(m => m.GetDocumentOptions(It.IsAny<DocumentShipment>())).Returns(documentShipment);

			var wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobByPackages, null);
			foreach (var documentWrapper in wrapper)
			{
				AssertEquals("Include Consignee", false, documentWrapper.DocIncludeConsignee);
				AssertEquals("Include Consignor", false, documentWrapper.DocIncludeConsignor);
				AssertEquals("Include None", false, documentWrapper.DocIncludeNone);
				AssertEquals("NumberOfLabelsToPrint", 17, documentWrapper.DocNumberOfLabelsToPrint);
			}

			AssertEquals("Shipment wrappers should be created", 17, wrapper.Length);
		}

		public void TestGetGenericWrapperWithGenericFreightJobBySelectedPackages()
		{
			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider.Object);

			var shipment = Factory.New<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = "PLT";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 12;
			packLine2.JL_F3_NKPackType = "PLT";

			var documentShipment = new DocumentShipment(shipment, DataContext.GenericFreightJobBySelectedPackages);

			documentShipment.IncludeConsignee = true;
			documentShipment.IncludeConsignor = true;
			documentShipment.IncludeNone = true;
			documentShipment.NumberOfLabelsToPrint = 0;

			queryProvider.Setup(m => m.GetDocumentOptions(It.IsAny<DocumentShipment>())).Returns(documentShipment);
			var wrappers = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobBySelectedPackages, null);
			foreach (var documentWrapper in wrappers)
			{
				AssertEquals("Include Consignee", true, documentWrapper.DocIncludeConsignee);
				AssertEquals("Include Consignor", true, documentWrapper.DocIncludeConsignor);
				AssertEquals("Include None", true, documentWrapper.DocIncludeNone);
				AssertEquals("NumberOfLabelsToPrint", 0, documentWrapper.DocNumberOfLabelsToPrint);
			}

			AssertEquals("Shipment wrappers should be created", 22, wrappers.Length);

			documentShipment = new DocumentShipment(shipment, DataContext.GenericFreightJobBySelectedPackages);

			documentShipment.IncludeConsignee = true;
			documentShipment.IncludeConsignor = true;
			documentShipment.IncludeNone = true;
			documentShipment.LabelRangeFrom = 4;
			documentShipment.NumberOfLabelsToPrint = 5;

			queryProvider.Setup(m => m.GetDocumentOptions(It.IsAny<DocumentShipment>())).Returns(documentShipment);
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobBySelectedPackages, null);
			foreach (var documentWrapper in wrappers)
			{
				AssertEquals("Include Consignee", true, documentWrapper.DocIncludeConsignee);
				AssertEquals("Include Consignor", true, documentWrapper.DocIncludeConsignor);
				AssertEquals("Include None", true, documentWrapper.DocIncludeNone);
				AssertEquals("NumberOfLabelsToPrint", 5, documentWrapper.DocNumberOfLabelsToPrint);
			}

			AssertEquals("Shipment wrappers should be created", 5, wrappers.Length);
		}

		public void TestGetDocumentWrappersWithGenericFreightJobByPackages1Doc()
		{
			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider.Object);

			var shipment = Factory.New<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = "PLT";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 12;
			packLine2.JL_F3_NKPackType = "PLT";

			var documentShipment = new DocumentShipment(shipment, DataContext.GenericFreightJobByPackages1Doc);

			documentShipment.IncludeConsignee = true;
			documentShipment.IncludeConsignor = true;
			documentShipment.IncludeNone = true;
			documentShipment.LabelRangeFrom = 1;
			documentShipment.NumberOfLabelsToPrint = 0;

			queryProvider.Setup(m => m.GetDocumentOptions(It.IsAny<DocumentShipment>())).Returns(documentShipment);

			var wrappers = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobByPackages1Doc, null);

			AssertEquals("A single document should be created", 1, wrappers.Length);

			var documentWrapper = wrappers.Single();
			AssertEquals("Include Consignee", true, documentWrapper.DocIncludeConsignee);
			AssertEquals("Include Consignor", true, documentWrapper.DocIncludeConsignor);
			AssertEquals("Include None", true, documentWrapper.DocIncludeNone);
			AssertEquals("NumberOfLabelsToPrint", 0, documentWrapper.DocNumberOfLabelsToPrint);

			var packageCollection = (BusinessObjectCollection)documentWrapper["Packages"];
			AssertEquals("Packages", 22, packageCollection.Count);
			AssertPackageWrapperNumberedCorrectly(1, packageCollection);

			documentShipment = new DocumentShipment(shipment, DataContext.GenericFreightJobByPackages1Doc);

			documentShipment.IncludeConsignee = true;
			documentShipment.IncludeConsignor = true;
			documentShipment.IncludeNone = true;
			documentShipment.LabelRangeFrom = 1;
			documentShipment.NumberOfLabelsToPrint = 5;

			queryProvider.Setup(m => m.GetDocumentOptions(It.IsAny<DocumentShipment>())).Returns(documentShipment);

			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobByPackages1Doc, null);

			AssertEquals("A single document should be created", 1, wrappers.Length);

			documentWrapper = wrappers.Single();
			AssertEquals("Include Consignee", true, documentWrapper.DocIncludeConsignee);
			AssertEquals("Include Consignor", true, documentWrapper.DocIncludeConsignor);
			AssertEquals("Include None", true, documentWrapper.DocIncludeNone);
			AssertEquals("NumberOfLabelsToPrint", 5, documentWrapper.DocNumberOfLabelsToPrint);

			packageCollection = (BusinessObjectCollection)documentWrapper["Packages"];
			AssertEquals("Packages", 5, packageCollection.Count);
			AssertPackageWrapperNumberedCorrectly(1, packageCollection);
		}

		public void TestGetDocumentWrappersWithGenericFreightJobBySelectedPackages1Doc()
		{
			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider.Object);

			var shipment = Factory.New<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = "PLT";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 12;
			packLine2.JL_F3_NKPackType = "PLT";

			var documentShipment = new DocumentShipment(shipment, DataContext.GenericFreightJobBySelectedPkgs1Doc);

			documentShipment.IncludeConsignee = true;
			documentShipment.IncludeConsignor = true;
			documentShipment.IncludeNone = true;
			documentShipment.LabelRangeFrom = 1;
			documentShipment.NumberOfLabelsToPrint = 0;

			queryProvider.Setup(m => m.GetDocumentOptions(It.IsAny<DocumentShipment>())).Returns(documentShipment);
			var wrappers = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobBySelectedPkgs1Doc, null);
			AssertEquals("A single document should be created", 1, wrappers.Length);

			var documentWrapper = wrappers.Single();
			AssertEquals("Include Consignee", true, documentWrapper.DocIncludeConsignee);
			AssertEquals("Include Consignor", true, documentWrapper.DocIncludeConsignor);
			AssertEquals("Include None", true, documentWrapper.DocIncludeNone);
			AssertEquals("NumberOfLabelsToPrint", 0, documentWrapper.DocNumberOfLabelsToPrint);

			var packageCollection = (BusinessObjectCollection)documentWrapper["Packages"];
			AssertEquals("Packages", 22, packageCollection.Count);
			AssertPackageWrapperNumberedCorrectly(1, packageCollection);

			documentShipment = new DocumentShipment(shipment, DataContext.GenericFreightJobBySelectedPkgs1Doc);

			documentShipment.IncludeConsignee = true;
			documentShipment.IncludeConsignor = true;
			documentShipment.IncludeNone = true;
			documentShipment.LabelRangeFrom = 4;
			documentShipment.NumberOfLabelsToPrint = 5;

			queryProvider.Setup(m => m.GetDocumentOptions(It.IsAny<DocumentShipment>())).Returns(documentShipment);
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobBySelectedPkgs1Doc, null);
			AssertEquals("A single document should be created", 1, wrappers.Length);

			documentWrapper = wrappers.Single();
			AssertEquals("Include Consignee", true, documentWrapper.DocIncludeConsignee);
			AssertEquals("Include Consignor", true, documentWrapper.DocIncludeConsignor);
			AssertEquals("Include None", true, documentWrapper.DocIncludeNone);
			AssertEquals("NumberOfLabelsToPrint", 5, documentWrapper.DocNumberOfLabelsToPrint);

			packageCollection = (BusinessObjectCollection)documentWrapper["Packages"];
			AssertEquals("Packages", 5, packageCollection.Count);
			AssertPackageWrapperNumberedCorrectly(4, packageCollection);
		}

		void AssertPackageWrapperNumberedCorrectly(int start, BusinessObjectCollection packages)
		{
			foreach (var package in packages)
			{
				AssertEquals(start++, package["PackageNumber"]);
			}
		}

		#endregion

		public void TestGetDocumentEventsHandlers()
		{
			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider);

			var shipment = Factory.New<ForwardingShipment>();
			var awbBarcodeLabelMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			var documentSupporter = new ForwardingShipmentDocumentSupporter(shipment);

			queryProvider
				.SetupSequence(m => m.PrintAWBBarcodeLabel())
				.CallBase()
				.Returns(false);
			awbBarcodeLabelMenuItem.SU_MenuName = "AWB Barcode Label";
			AssertDocumentEventHandlersCanHandleMenuItem(documentSupporter, awbBarcodeLabelMenuItem);
			queryProvider
				.SetupSequence(m => m.PrintAWBBarcodeLabel())
				.CallBase()
				.Returns(true);
			documentSupporter = new ForwardingShipmentDocumentSupporter(shipment);
			awbBarcodeLabelMenuItem.SU_MenuName = "AWB Barcode Label";
			AssertDocumentEventHandlersCanHandleMenuItem(documentSupporter, awbBarcodeLabelMenuItem);
		}

		public void TestDocumentWrapperGeneratedForAWBDataContext()
		{
			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>();
			Factory.SetValue(() => queryProvider.Object);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			var documentSupporter = shipment.DocumentSupporter;

			queryProvider
				.SetupSequence(m => m.PrintAWBBarcodeLabel())
				.Returns(false)
				.CallBase();
			menuItem.SU_MenuName = "AWB Barcode Label";
			var documentWrappers = documentSupporter.GetDocumentWrappers(DataContext.AWB, menuItem);
			AssertNull(documentWrappers);
			queryProvider
				.SetupSequence(m => m.PrintAWBBarcodeLabel())
				.Returns(true)
				.CallBase();
			menuItem.SU_MenuName = "AWB Barcode Label";
			documentWrappers = documentSupporter.GetDocumentWrappers(Core.Constants.DataContext.AWB, menuItem);
			AssertNotNull(documentWrappers);

			menuItem.SU_MenuName = "Laser HAWB";
			documentWrappers = documentSupporter.GetDocumentWrappers(Core.Constants.DataContext.AWB, menuItem);
			AssertNotNull(documentWrappers);
		}

		#region AR Invoice

		public void TestGetDocBusinessObjectsForARInvoice()
		{
			AccTransactionHeader invoiceConsignee = GetInvoiceForTest("00001001");
			AccTransactionHeader invoiceConsignor = GetInvoiceForTest("00001002");
			AccTransactionHeader invoiceLocalClient = GetInvoiceForTest("00001003");
			AccTransactionHeader invoiceAgent = GetInvoiceForTest("00001004");
			Factory.Save();

			ForwardingShipment shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "S00009999";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = invoiceConsignee.Header.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = invoiceConsignor.Header.MainAddress.PK;
			JobHeader job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_OA_LocalChargesAddr = invoiceLocalClient.Header.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = invoiceAgent.Header.MainAddress.PK;

			invoiceConsignee.AH_JH = job.PK;
			invoiceConsignor.AH_JH = job.PK;
			invoiceLocalClient.AH_JH = job.PK;
			invoiceAgent.AH_JH = job.PK;

			AssertCorrectInvoiceObjects(shipment, ContactType.NoContactType, invoiceLocalClient);
			AssertCorrectInvoiceObjects(shipment, ContactType.Consignee, invoiceConsignee);
			AssertCorrectInvoiceObjects(shipment, ContactType.Consignor, invoiceConsignor);
			AssertCorrectInvoiceObjects(shipment, ContactType.ImportAirFreightAgent, invoiceAgent);
			AssertCorrectInvoiceObjects(shipment, ContactType.ImportSeaFreightAgent, invoiceAgent);
			AssertCorrectInvoiceObjects(shipment, ContactType.ExportAirFreightAgent, invoiceAgent);
			AssertCorrectInvoiceObjects(shipment, ContactType.ExportSeaFreightAgent, invoiceAgent);
			AssertCorrectInvoiceObjects(shipment, ContactType.All, invoiceLocalClient);
			AssertCorrectInvoiceObjects(shipment, ContactType.Receivables, invoiceLocalClient);
			AssertCorrectInvoiceObjects(shipment, null, invoiceLocalClient);

			job.JH_OA_LocalChargesAddr = ZGuid.Empty;
			AssertCorrectInvoiceObjects(shipment, ContactType.Receivables, null);
		}

		void AssertCorrectInvoiceObjects(ForwardingShipment shipment, ContactType contactType, AccTransactionHeader expectedInvoice)
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = contactType != null ? contactType.Code : "";
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, menuItem);

			if (expectedInvoice == null)
			{
				AssertNull(wrappers);
			}
			else
			{
				AssertEquals(1, wrappers.Length);
				AssertEquals(expectedInvoice.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
			}
		}

		AccTransactionHeader GetInvoiceForTest(ZString invoiceNumber)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = invoiceNumber;

			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = invoiceNumber;
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";
			newInvoice.AH_TransactionReference = invoiceNumber;

			return newInvoice;
		}

		#endregion

		public void TestGetDocBusinessObjectForLetterOfIndemnity()
		{
			ForwardingShipment shipment = (ForwardingShipment)GetImportShipment(typeof(ForwardingShipment), Factory);
			shipment.JS_MarksAndNumbers = "OldMarksAndNumbers";
			shipment.DetailedGoodsDescriptionNoteText = "OldGoodsDescription";
			shipment.JS_ActualWeight = ZDecimal.ParseSafe("1.111", 0);
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_ActualVolume = ZDecimal.ParseSafe("2.222", 0);
			shipment.JS_UnitOfVolume = "M3";

			Factory.Save();

			Assert("Saved, shouldn't have changes", !shipment.HasChanges);

			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.SetupSequence(m =>
					m.GetLetterOfIndemnityOptions(It.Is<DocumentShipment>(docShipment =>
						docShipment.Shipment == shipment)))
				.Returns((LetterOfIndemnityOptions)null)
				.CallBase();
			DocumentWrapper[] wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LetterOfIndemnity, null);
			AssertEquals("Shipment wrapper should be null", null, wrapper);
			Assert("Details NOT updated, should NOT have changes", !shipment.HasChanges);
			queryProvider
				.SetupSequence(m =>
					m.GetLetterOfIndemnityOptions(It.Is<DocumentShipment>(docShipment =>
						docShipment.Shipment == shipment)))
				.Returns(new LetterOfIndemnityOptions())
				.CallBase();
			wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LetterOfIndemnity, null);
			AssertEquals("Shipment wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocForwardingShipment", "DocForwardingShipment", wrapper[0].GetType().Name);
			Assert("No new details, should NOT have changes", !shipment.HasChanges);

			var optionsToReturn = new LetterOfIndemnityOptions()
			{
				ChangeGoodsDescription = true,
				NewGoodsDescription = "NewGoodsDescription",
				ChangeMarksAndNumbers = true,
				NewMarksAndNumbers = "NewMarksAndNumbers",
				ChangeWeight = true,
				NewWeight = ZDecimal.ParseSafe("4.444", 0),
				NewWeightUnit = Core.Constants.Weight.Kilograms,
				ChangeVolume = true,
				NewVolume = ZDecimal.ParseSafe("3.333", 0),
				NewVolumeUnit = Core.Constants.Volume.CubicInches
			};

			queryProvider
				.SetupSequence(m =>
					m.GetLetterOfIndemnityOptions(It.Is<DocumentShipment>(docShipment =>
						docShipment.Shipment == shipment)))
				.Returns(optionsToReturn)
				.CallBase();

			wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LetterOfIndemnity, null);
			AssertEquals("Shipment wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocForwardingShipment", "DocForwardingShipment", wrapper[0].GetType().Name);
			Assert("Details updated, should have changes", shipment.HasChanges);
			AssertEquals("GoodsDescription should be", "NewGoodsDescription", shipment.DetailedGoodsDescriptionNoteText);
			AssertEquals("MarkAndNumbers should be", "NewMarksAndNumbers", shipment.JS_MarksAndNumbers);
			AssertEquals("Volume should be", ZDecimal.ParseSafe("3.333", 0), shipment.JS_ActualVolume);
			AssertEquals("VolumeUnit should be", "CI", shipment.JS_UnitOfVolume);
			AssertEquals("Weight should be", ZDecimal.ParseSafe("4.444", 0), shipment.JS_ActualWeight);
			AssertEquals("WeightUnit should be", "KG", shipment.JS_UnitOfWeight);
		}

		public void TestGetDocBusinessObjectsForRequestForService()
		{
			var shipment = GetShipment();

			var servicesSelectionProvider = new Mock<IServicesSelectionProvider>(MockBehavior.Strict);
			Factory.SetValue(() => servicesSelectionProvider.Object);

			servicesSelectionProvider
				.SetupSequence(m =>
					m.GetServicesToPrint(It.Is<IHaveServices>(parent => parent.Equals(shipment.DocsAndCartage))))
				.Returns((JobService[])null)
				.CallBase();
			var wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForService, null);
			AssertEquals("Wrapper should be null", null, wrapper);

			JobService service1 = shipment.DocsAndCartage.Services.AddNew();
			JobService service2 = shipment.DocsAndCartage.Services.AddNew();
			AssertEquals(false, service1.ReadOnly);

			servicesSelectionProvider
				.SetupSequence(m =>
					m.GetServicesToPrint(It.Is<IHaveServices>(parent => parent.Equals(shipment.DocsAndCartage))))
				.Returns(new[] { service1, service2 })
				.CallBase();
			wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForService, null);
			AssertEquals("Service Wrappers should be created", 2, wrapper.Length);
			AssertEquals("Wrapper type should be DocService", "DocService", wrapper[0].GetType().Name);
			AssertEquals("Wrapper type should be DocService", "DocService", wrapper[1].GetType().Name);
			AssertEquals(false, service1.ReadOnly);
		}

		public void TestGetDocBusinessObjectForChargeSheet()
		{
			AssertChargeSheet(Core.Constants.DataContext.ChargeSheet);
		}

		public void TestGenericChargeSheet()
		{
			AssertChargeSheet(Core.Constants.DataContext.GenericChargeSheet);
		}

		void AssertChargeSheet(DataContext dataContext)
		{
			StmMenuItem menuItem = new BusinessObjectFactory().New<StmMenuItem>();
			menuItem.SU_DocumentDirection = "ARV";

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			GlbCompany.CurrentCompany.SetCountry("AU");
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(dataContext, menuItem);
			AssertEquals("Wrapper created for ChargeSheet data context", 1, wrappers.Length);
			AssertNotNull("Shipment wrapper for ChargeSheet shouldn't be null", wrappers[0]);

			menuItem.SU_DocumentDirection = "DEP";
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(dataContext, menuItem);
			AssertEquals("Wrapper should be null", null, wrappers);

			JobHeader header = Factory.NewJobForTesting<JobHeader>();
			header.JH_ParentID = shipment.PK;
			header.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header.JH_GC = GlbCompany.CurrentCompany.PK;
			header.JH_OA_LocalChargesAddr = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress.PK;

			AccChargeCode testCode1 = CreateChargeCode("TESTCH1");
			AccChargeCode testCode2 = CreateChargeCode("TESTCH2");
			AccChargeCode testCode3 = CreateChargeCode("TESTCH3");

			JobCharge lineCharge1 = CreateLineCharge(header, header.LocalChargesPK, 10.000M, testCode1.PK);
			JobCharge lineCharge2 = CreateLineCharge(header, header.LocalChargesPK, 30.000M, testCode2.PK);

			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, header.LocalChargesPK));
			JobCharge lineCharge3 = CreateLineCharge(header, orgHeader.PK, 20.000M, testCode3.PK);
			lineCharge3.JR_OH_SellAccount = orgHeader.PK;

			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			JobCharge lineCharge4 = CreateLineCharge(header, orgHeader2.PK, 20.000M, testCode3.PK);
			lineCharge4.JR_OH_SellAccount = orgHeader2.PK;

			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(dataContext, menuItem);
			AssertEquals("Wrappers created for ChargeSheet data context", 3, wrappers.Length);
		}

		public void TestGetDocBusinessObjectForTimeSlotRequest()
		{
			ForwardingShipment shipment = GetShipment();
			DocumentWrapper[] wrapper = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.TimeSlotRequest, null);
			AssertEquals("Document wrapper for data context of declaration is of type DocShipment", "DocForwardingShipment", wrapper[0].GetType().Name);
		}

		public void TestContainerSelectionForMultiModelDangerousGoodsDeclarationDocument()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USORD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "OOCL0000006";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "OOCL0000013";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "OOCL0000027";
			container3.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "NZAKL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_F3_NKPackType = "PLT";
			packLine1.JL_JC = container1.PK;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 1;
			packLine2.JL_F3_NKPackType = "PLT";
			packLine2.JL_JC = container2.PK;

			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			var docSupporter = (ForwardingShipmentDocumentSupporter)shipment.DocumentSupporter;

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_DocumentDirection = nameof(DocumentDirection.ARV);
			menuItem.SU_MenuName = "Multimodal Dangerous Goods Form";

			var wrappers = docSupporter.GetDocumentWrappers(DataContext.GenericFreightJobByContainerIfFCL, menuItem);
			AssertEquals("wrappers.Length", 0, wrappers.Length);

			packLine1.UNDGs.AddNew();
			wrappers = docSupporter.GetDocumentWrappers(DataContext.GenericFreightJobByContainerIfFCL, menuItem);
			AssertEquals("wrappers.Length", 1, wrappers.Length);

			packLine2.JL_RH_NKCommodityCode = CargoTypes.Hazardous;

			wrappers = docSupporter.GetDocumentWrappers(DataContext.GenericFreightJobByContainerIfFCL, menuItem);
			AssertEquals("wrappers.Length", 2, wrappers.Length);
		}

		public void TestGetDocBusinessObjectForGenericFreightJobRouting()
		{
			ForwardingShipment shipment = (ForwardingShipment)GetImportShipment(typeof(ForwardingShipment), Factory);
			Transport transport1 = shipment.TransportsIncludingRelated.AddNew();
			Transport transport2 = shipment.TransportsIncludingRelated.AddNew();
			Factory.Save();

			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "In Bond (Routing)";
			DocumentCancelEventArgs eventArgs = new DocumentCancelEventArgs(menuItem);

			var queryProvider = new Mock<IForwardingShipmentDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.SetupSequence(m =>
					m.GetTransportToPrint(It.Is<DocumentShipment>(docShipment => docShipment.Shipment == shipment)))
				.Returns((Transport)null)
				.CallBase();
			DocumentWrapper[] wrapper = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobRouting, menuItem);
			AssertEquals("Shipment wrapper should be null", null, wrapper);
			queryProvider
				.SetupSequence(m =>
					m.GetTransportToPrint(It.Is<DocumentShipment>(docShipment => docShipment.Shipment == shipment)))
				.Returns(transport1)
				.CallBase();
			wrapper = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobRouting, menuItem);
			AssertEquals("Shipment wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be FreightWrapperFromShipment", "FreightWrapperFromShipment", wrapper[0].GetType().Name);
			queryProvider
				.SetupSequence(m =>
					m.GetTransportToPrint(It.Is<DocumentShipment>(docShipment => docShipment.Shipment == shipment)))
				.Returns(transport2)
				.CallBase();
			wrapper = shipment.DocumentSupporter.GetDocumentWrappers(DataContext.GenericFreightJobRouting, menuItem);
			AssertEquals("Shipment wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be FreightWrapperFromShipment", "FreightWrapperFromShipment", wrapper[0].GetType().Name);
		}

		public void TestGetDocBusinessObjectForStandardShippingNote()
		{
			ForwardingShipment shipment = GetShipment();
			DocumentWrapper[] wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Shipment, null);
			AssertEquals("Single wrapper created", 1, wrappers.Length);
			AssertEquals("Wrapper is of type DocForwardingShipment", "DocForwardingShipment", wrappers[0].GetType().Name);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container1 = consol.Containers.AddNew();
			ForwardingContainer container2 = consol.Containers.AddNew();
			ForwardingContainer container3 = consol.Containers.AddNew();
			consol.Shipments.Add(shipment);

			shipment.OuterPackLines.AddNew().JL_JC = container1.PK;
			shipment.OuterPackLines.AddNew().JL_JC = container2.PK;
			shipment.OuterPackLines.AddNew().JL_JC = container3.PK;

			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Standard Shipping Note";    // This is a document menu name
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Shipment, menuItem);

			AssertEquals("One wrapper created - Shipment SSN no longer used for containerised shipments", 1, wrappers.Length);
			AssertEquals("Wrapper is of type DocForwardingShipment", "DocForwardingShipment", wrappers[0].GetType().Name);

			menuItem.SU_MenuName = "Some menu item";    // This is a document menu name
			wrappers = shipment.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Shipment, menuItem);

			AssertEquals("Single wrapper created", 1, wrappers.Length);
			AssertEquals("Wrapper is of type DocForwardingShipment", "DocForwardingShipment", wrappers[0].GetType().Name);
		}

		class IDocumentEventsMock : IDocumentEvents
		{
			#region IDocumentEvents Members

			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed?.Invoke(this, e);
			}

			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrePrinted != null)
				{
					DocumentPrePrinted(this, e);
				}
			}

			public event DocumentCancelEventHandler DocumentPrintRequested;
			public void NotifyDocumentPrintRequested(DocumentCancelEventArgs e)
			{
				if (DocumentPrintRequested != null)
				{
					DocumentPrintRequested(this, e);
				}
			}

			public event DocumentPrintedEventHandler DocumentPrinted;
			public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrinted(this, e);
				}
			}

			#endregion
		}

		public void TestSupportedDataContext()
		{
			ForwardingShipment shipment = GetShipment();
			AssertEquals("DataContext.Shipment is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Shipment)));
			AssertEquals("DataContext.AWB is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.AWB)));
			AssertEquals("DataContext.Declaration is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Declaration)));
			AssertEquals("DataContext.DeclarationWithCusEntryHeaders is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.DeclarationWithCusEntryHeaders)));
			AssertEquals("DataContext.Container is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Container)));
			AssertEquals("DataContext.Notes is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Notes)));
			AssertEquals("DataContext.CartageAdvice is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CartageAdvice)));
			AssertEquals("DataContext.CommercialInvoice is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CommercialInvoice)));
			AssertEquals("DataContext.ChargeSheet is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ChargeSheet)));
			AssertEquals("DataContext.CusEntryHeader is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CusEntryHeader)));
			AssertEquals("DataContext.ComInvoiceHeader is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ComInvoiceHeader)));
			AssertEquals("DataContext.PreAlert is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.PreAlert)));
			AssertEquals("DataContext.ForwardingPreAdvice is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ForwardingPreAdvice)));
			AssertEquals("DataContext.ARInvoice is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ARInvoice)));
			AssertEquals("DataContext.ShipperDepartureNotice is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ShipperDepartureNotice)));
			AssertEquals("DataContext.LandedCostEntryHeaders is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.LandedCostEntryHeaders)));
			AssertEquals("DataContext.LandedCostHeader is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.LandedCostHeader)));
			AssertEquals("DataContext.ShipmentDeclaration is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ShipmentDeclaration)));
			AssertEquals("DataContext.FreightLabels is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.FreightLabels)));
			AssertEquals("DataContext.ShippingOrder is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ShippingOrder)));
			AssertEquals("DataContext.ATD is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ATD)));
			AssertEquals("DataContext.EFTPaymentAdvice is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.EFTPaymentAdvice)));
			AssertEquals("DataContext.LetterOfIndemnity is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.LetterOfIndemnity)));
			AssertEquals("DataContext.CombinedCartageAdvice is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CombinedCartageAdvice)));
			AssertEquals("DataContext.ERA is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ERA)));
			AssertEquals("DataContext.CFSAndForwardingShipment is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CFSAndForwardingShipment)));
			AssertEquals("DataContext.RequestForMissingDocuments is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.RequestForMissingDocuments)));
			AssertEquals("DataContext.Worksheet is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Worksheet)));
			AssertEquals("DataContext.ForwardingShipment is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ForwardingShipment)));
			AssertEquals("DataContext.IMO is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.IMO)));
			AssertEquals("DataContext.RequestForService is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.RequestForService)));
			AssertEquals("DataContext.ForwardingShipmentAndConsol is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ForwardingShipmentAndConsol)));
			AssertEquals("DataContext.ForwardingShipment is supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ForwardingShipment)));
			AssertEquals("DataContext.Service is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Service)));
			AssertEquals("DataContext.TimeSlotRequest is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.TimeSlotRequest)));
			AssertEquals("DataContext.GenericFreightJobRouting is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJobRouting)));
			AssertEquals("DataContext.GenericFreightJobByComInv is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJobByComInv)));
			AssertEquals("DataContext.GenericFreightJobInvoice is Supported", true, shipment.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJobInvoice)));
		}

		public void TestGetDocumentTitlesForPivotHBLWhenRunForShipperDocumentPack()
		{
			ForwardingShipment shipment = GetShipment();
			shipment.JS_UniqueConsignRef = "S1";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_NoOriginalBills = 5;
			shipment.JS_NoCopyBills = 3;
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.Parent = shipment;

			StmMenuItem shipperDocPackMenu = Factory.New<StmMenuItem>();
			shipperDocPackMenu.SU_MenuName = "Shipper Document Pack (Sea)";

			StmMenuItem hBLMenu = Factory.New<StmMenuItem>();
			hBLMenu.SU_MenuName = "Bill Of Lading";

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			pivot.SI_DocumentTitle = "COPY";
			pivot.SI_SU = hBLMenu.PK;

			TitleCopyCountPair titles = shipment.DocumentSupporter.GetDocumentTitlesForPivot(shipperDocPackMenu.SU_MenuName, shipment, pivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);
			string expected = "A Shipper Document Pack cannot be created.\r\nEither no HBL Type has been specified or no Invoice(s) against the Consignor or the Consignor's Bill-To-Party exists.";
			AssertEquals("Error Message", expected, ((NothingToPrint)titles).Reason);

			shipment.JS_HouseBillOfLadingType = "FIA";
			titles = shipment.DocumentSupporter.GetDocumentTitlesForPivot(shipperDocPackMenu.SU_MenuName, shipment, pivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);
			AssertEquals("Error Message", expected, ((NothingToPrint)titles).Reason);

			OrgHeader consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			titles = shipment.DocumentSupporter.GetDocumentTitlesForPivot(shipperDocPackMenu.SU_MenuName, shipment, pivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);
			AssertEquals("Error Message", expected, ((NothingToPrint)titles).Reason);

			AccTransactionHeader accHeader = Factory.New<AccTransactionHeader>();
			accHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			accHeader.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			accHeader.AH_OH = consignor.PK;
			accHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader.AH_JH = job.PK;
			accHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			titles = shipment.DocumentSupporter.GetDocumentTitlesForPivot(shipperDocPackMenu.SU_MenuName, shipment, pivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);

			accHeader.AH_OH = consignor.PK;
			titles = shipment.DocumentSupporter.GetDocumentTitlesForPivot(shipperDocPackMenu.SU_MenuName, shipment, pivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			shipperDocPackMenu.SU_MenuName = "Shipper Document Pack (Rail)";
			titles = shipment.DocumentSupporter.GetDocumentTitlesForPivot(shipperDocPackMenu.SU_MenuName, shipment, pivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);

			shipment.JS_HouseBillOfLadingType = "";
			titles = shipment.DocumentSupporter.GetDocumentTitlesForPivot(shipperDocPackMenu.SU_MenuName, shipment, pivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);
			AssertEquals("Error Message", expected, ((NothingToPrint)titles).Reason);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipperDocPackMenu.SU_MenuName = "Shipper Document Pack (Air)";
			titles = shipment.DocumentSupporter.GetDocumentTitlesForPivot(shipperDocPackMenu.SU_MenuName, shipment, pivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);

			accHeader.AH_OH = ZGuid.Empty;
			titles = shipment.DocumentSupporter.GetDocumentTitlesForPivot(shipperDocPackMenu.SU_MenuName, shipment, pivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);
			expected = "A Shipper Document Pack cannot be created.\r\nNo Invoice(s) against the Consignor or the Consignor's Bill-To-Party exists.";
			AssertEquals("Error Message", expected, ((NothingToPrint)titles).Reason);
		}

		public void TestGetDocumentTitlesForPivotHBLWhenRunFromShipment()
		{
			ForwardingShipment testShipment = GetShipment();
			testShipment.JS_NoOriginalBills = 5;
			testShipment.JS_NoCopyBills = 3;

			StmMenuItem menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Bill Of Lading";

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			pivot.SI_DocumentTitle = "ORIGINAL";
			pivot.SI_SU = menu.PK;

			TitleCopyCountPair titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals("Copy count", (short)5, titles.CopyCount);
			AssertEquals("Title", "ORIGINAL", titles.Title);

			pivot.SI_DocumentTitle = "COPY";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals("Copy count", (short)3, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			menu.SU_MenuName = "Bill Of Lading Sea";
			AssertEquals("Copy count", (short)3, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			pivot.SI_PrintCopyType = nameof(PrintCopyType.FAX);
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals("Copy count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			pivot.SI_PrintCopyType = nameof(PrintCopyType.EML);
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals("Copy count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			menu.SU_MenuName = "Something else";
			pivot.SI_DocumentTitle = "Test Doc";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertNull("Titles", titles);
		}

		public void TestGetDocumentTitlesForManufacturerBillOfLadingWhenRunFromShipment()
		{
			var testShipment = GetShipment();
			testShipment.JS_NoOriginalBills = 5;
			testShipment.JS_NoCopyBills = 3;

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = DocumentNames.ManufacturerBillOfLading;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			pivot.SI_DocumentTitle = "ORIGINAL";
			pivot.SI_SU = menu.PK;

			var titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals("Copy count", (short)5, titles.CopyCount);
			AssertEquals("Title", "ORIGINAL", titles.Title);

			pivot.SI_DocumentTitle = "COPY";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals("Copy count", (short)3, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);
		}

		public void TestHaveSamePivotsBetweenBillOfLadingAndManufacturerBillOfLading()
		{
			var normalBOLPivots = GetPivots("Bill Of Lading", "Departure/Bill Of Lading", "");
			var manufacturerBOLPivots = GetPivots("Legacy Manufacturer Bill of Lading", "Departure/Bill Of Lading", "");

			AssertContainsExactElementsInAnyOrder(new BillOfLadingPivotComparer(), normalBOLPivots, manufacturerBOLPivots);
		}

		public void TestHaveSamePivotsBetweenBillOfLadingToPreprintedAndManufacturerBillOfLadingToPreprinted()
		{
			var normalBOLPivots = GetPivots("Bill Of Lading To Preprinted", "Departure/Bill Of Lading", "");
			var manufacturerBOLPivots = GetPivots("Legacy Manufacturer BoL To Preprinted", "Departure/Bill Of Lading", "");

			AssertContainsExactElementsInAnyOrder(new BillOfLadingPivotComparer(), normalBOLPivots, manufacturerBOLPivots);
		}

		class BillOfLadingPivotComparer : IEqualityComparer<StmMenuTemplatePivotBase>
		{
			public bool Equals(StmMenuTemplatePivotBase pivot1, StmMenuTemplatePivotBase pivot2)
			{
				var collection1 = GetPropertyInfosForComparison(pivot1).OrderBy(x => x.Name);
				var collection2 = GetPropertyInfosForComparison(pivot2).OrderBy(x => x.Name);
				return collection1.Zip(collection2, (x, y) => x.Value.Equals(y.Value)).All(x => x);
			}

			public IEnumerable<ZPropertyInfo> GetPropertyInfosForComparison(StmMenuTemplatePivotBase pivot)
			{
				return pivot.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => x.IsPersistent && x != pivot.SI_SUInfo);
			}

			public int GetHashCode(StmMenuTemplatePivotBase pivot)
			{
				return GetPropertyInfosForComparison(pivot).Aggregate(0, (hashCode, propertyInfo) => hashCode ^ propertyInfo.Value.GetHashCode());
			}
		}

		public void TestElectronicBillOfLading()
		{
			StmMenuTemplatePivotBase[] bolPivots = GetPivots("Bill Of Lading", "Departure/Bill Of Lading", "ORIGINAL");
			StmMenuTemplatePivotBase[] electronicBolPivots = GetPivots("Send Electronic Original Bill of Lading", "Departure/Bill Of Lading", "ORIGINAL");

			Converter<StmMenuTemplatePivotBase, ZString> pivotFormatterTemplateNamesAndFilters = (pivot) => ZString.Format("'{0}' '{1}'", pivot.SI_MenuTemplateFilter, pivot.SO_Name);

			Converter<StmMenuTemplatePivotBase, ZString> pivotFormatterDocTypes = (p) =>
			{
				RefDocType type = Factory.Load<RefDocType>(p.SI_RT_DocType);
				return type == null ? (ZString)"<NULL>" : type.RT_DocType;
			};

			AssertContainsExactElementsInAnyOrder("'[filter]' '[name]'",
				Array.ConvertAll(bolPivots, pivotFormatterTemplateNamesAndFilters),
				Array.ConvertAll(electronicBolPivots, pivotFormatterTemplateNamesAndFilters));

			AssertContainsExactElementsInAnyOrder(
				Array.ConvertAll(bolPivots, pivotFormatterDocTypes),
				Array.ConvertAll(electronicBolPivots, pivotFormatterDocTypes));

			AssertEquals(true, Array.TrueForAll(electronicBolPivots, (p) => p.SI_PrintCopyType == nameof(PrintCopyType.ALL)));
		}

		StmMenuTemplatePivotBase[] GetPivots(string menuName, string menuPath, string documentTitle)
		{
			var pivotsQuery = new ZDBOnlyQuery(typeof(StmMenuTemplatePivotBase));

			if (!string.IsNullOrEmpty(documentTitle))
			{
				pivotsQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_DocumentTitle, documentTitle);
			}

			var menuItemSubQuery = new ZDBOnlySubQuery(typeof(StmMenuItem), StmMenuItemSchema.PK);
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, menuName);
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "Shipment");
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_MenuPath, menuPath);
			menuItemSubQuery.AddToFilter(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Documents);

			pivotsQuery.AddSubQuery(StmMenuTemplatePivotSchema.SI_SU, menuItemSubQuery, JoinCondition.And);

			return Factory.Load<StmMenuTemplatePivotBase>(pivotsQuery);
		}

		#region Get Document Titles For Pivot

		public void TestGetDocumentTitlesForPivotHBLForExpressBOL()
		{
			ForwardingShipment testShipment = GetShipment();
			testShipment.JS_NoOriginalBills = 5;
			testShipment.JS_NoCopyBills = 3;

			StmMenuItem menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Bill Of Lading";

			StmMenuTemplatePivotBase pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			pivot.SI_DocumentTitle = "ORIGINAL";
			pivot.SI_SU = menu.PK;

			TitleCopyCountPair titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals("Copy count", (short)5, titles.CopyCount);
			AssertEquals("Title", "ORIGINAL", titles.Title);

			pivot.SI_DocumentTitle = "COPY";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals("Copy count", (short)3, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			testShipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.ExpressBofL;
			testShipment.JS_NoCopyBills = 3;
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals("Copy count", (short)3, titles.CopyCount);
			AssertEquals("Title", "EXPRESS", titles.Title);

			testShipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.OriginalReqSurrender;
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals("Titles count", (short)3, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);
		}

		public void TestGetDocumentTitlesForPivotHBLWhenRunFromConsolForAllShipments()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment testShipment = consol.Shipments.AddNew();
			testShipment.JS_NoOriginalBills = 5;
			testShipment.JS_NoCopyBills = 3;

			StmMenuItem hBLMenu = Factory.New<StmMenuItem>();
			hBLMenu.SU_MenuName = "Bill Of Lading";

			StmMenuTemplatePivotBase originalTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			originalTemplatePivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			originalTemplatePivot.SI_DocumentTitle = "ORIGINAL";
			originalTemplatePivot.SI_SU = hBLMenu.PK;

			StmMenuTemplatePivotBase copyTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			copyTemplatePivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			copyTemplatePivot.SI_DocumentTitle = "COPY";
			copyTemplatePivot.SI_SU = hBLMenu.PK;

			StmMenuItem packMenu = Factory.New<StmMenuItem>();
			packMenu.SU_MenuName = "Doc Pack";

			StmMenuMenuPivot packPivot = Factory.New<StmMenuMenuPivot>();
			packPivot.SF_SU_Inward = packMenu.PK;
			packPivot.SF_SU_Outward = hBLMenu.PK;

			TitleCopyCountPair titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, originalTemplatePivot);
			AssertEquals("Copy count", (short)0, titles.CopyCount);

			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, copyTemplatePivot);
			AssertEquals("Copy count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			testShipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.ExpressBofL;
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, copyTemplatePivot);
			AssertEquals("Title", "EXPRESS", titles.Title);
		}

		public void TestGetDocumentTitlesForPivotHBLWhenRunFromConsolForMasterShipmentsWithFlagTrue()
		{
			ForwardingConsol testConsol = Factory.New<ForwardingConsol>();
			ForwardingShipment testShipment = testConsol.Shipments.AddNew();
			testShipment.JS_NoOriginalBills = 5;
			testShipment.JS_NoCopyBills = 3;

			ForwardingShipment masterShipment = testConsol.Shipments.AddNew();
			masterShipment.JS_NoOriginalBills = 3;
			masterShipment.JS_NoCopyBills = 9;

			testShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			StmMenuItem hBLMenu = Factory.New<StmMenuItem>();
			hBLMenu.SU_MenuName = "Bill Of Lading";

			StmMenuTemplatePivotBase templatePivot = Factory.New<StmMenuTemplatePivotBase>();
			templatePivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			templatePivot.SI_DocumentTitle = "ORIGINAL";
			templatePivot.SI_SU = hBLMenu.PK;

			StmMenuItem packMenu = Factory.New<StmMenuItem>();
			packMenu.SU_MenuName = "Doc Pack";

			StmMenuMenuPivot packPivot = Factory.New<StmMenuMenuPivot>();
			packPivot.SF_SU_Inward = packMenu.PK;
			packPivot.SF_SU_Outward = hBLMenu.PK;

			//ORIGINAL
			TitleCopyCountPair titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);

			//COPY
			templatePivot.SI_DocumentTitle = "COPY";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			//EMAIL
			templatePivot.SI_DocumentTitle = "EMAIL COPY";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			//FAX
			templatePivot.SI_DocumentTitle = "FAX COPY";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);
		}

		public void TestGetDocumentTitlesForPivotHBLWhenRunFromConsolForMasterShipmentsWithFlagFalse()
		{
			ForwardingConsol testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;

			ForwardingShipment testShipment = testConsol.Shipments.AddNew();
			testShipment.JS_NoOriginalBills = 5;
			testShipment.JS_NoCopyBills = 3;

			ForwardingShipment masterShipment = testConsol.Shipments.AddNew();
			masterShipment.JS_NoOriginalBills = 3;
			masterShipment.JS_NoCopyBills = 9;

			testShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			StmMenuItem hBLMenu = Factory.New<StmMenuItem>();
			hBLMenu.SU_MenuName = "Bill Of Lading";

			StmMenuTemplatePivotBase templatePivot = Factory.New<StmMenuTemplatePivotBase>();
			templatePivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			templatePivot.SI_DocumentTitle = "ORIGINAL";
			templatePivot.SI_SU = hBLMenu.PK;

			StmMenuItem packMenu = Factory.New<StmMenuItem>();
			packMenu.SU_MenuName = "Doc Pack";

			StmMenuMenuPivot packPivot = Factory.New<StmMenuMenuPivot>();
			packPivot.SF_SU_Inward = packMenu.PK;
			packPivot.SF_SU_Outward = hBLMenu.PK;

			//ORIGINAL
			TitleCopyCountPair titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);

			//COPY
			templatePivot.SI_DocumentTitle = "COPY";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			//EMAIL
			templatePivot.SI_DocumentTitle = "EMAIL COPY";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			//FAX
			templatePivot.SI_DocumentTitle = "FAX COPY";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)0, titles.CopyCount);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, testConsol, templatePivot);
			AssertEquals("Titles count", (short)1, titles.CopyCount);
			AssertEquals("Title", "COPY", titles.Title);

			testConsol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;
		}

		public void TestGetDocumentTitlesForPivotHAWBWhenRunFromConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment testShipment = consol.Shipments.AddNew();

			StmMenuItem hAWBMenu = Factory.New<StmMenuItem>();
			hAWBMenu.SU_MenuName = "Laser HAWB";

			StmMenuTemplatePivotBase templatePivot = Factory.New<StmMenuTemplatePivotBase>();
			templatePivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			templatePivot.SI_DocumentTitle = "Copy 8 - (for Agent)";
			templatePivot.SI_SU = hAWBMenu.PK;

			StmMenuItem packMenu = Factory.New<StmMenuItem>();
			packMenu.SU_MenuName = "Doc Pack";

			StmMenuMenuPivot packPivot = Factory.New<StmMenuMenuPivot>();
			packPivot.SF_SU_Inward = packMenu.PK;
			packPivot.SF_SU_Outward = hAWBMenu.PK;

			TitleCopyCountPair titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Copy 8 - (for Agent)", titles.Title);

			templatePivot.SI_DocumentTitle = "Email Copy";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "EMAIL COPY", titles.Title);

			templatePivot.SI_DocumentTitle = "Fax Copy";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "FAX COPY", titles.Title);
		}

		public void TestGetDocumentTitlesForPivotHAWBWhenRunFromConsolForMasterShipmentWithFlagTrue()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment testShipment = consol.Shipments.AddNew();

			ForwardingShipment masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_NoOriginalBills = 3;
			masterShipment.JS_NoCopyBills = 9;

			testShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			StmMenuItem hAWBMenu = Factory.New<StmMenuItem>();
			hAWBMenu.SU_MenuName = "Laser HAWB";

			StmMenuTemplatePivotBase templatePivot = Factory.New<StmMenuTemplatePivotBase>();
			templatePivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			templatePivot.SI_DocumentTitle = "Copy 8 - (for Agent)";
			templatePivot.SI_SU = hAWBMenu.PK;

			StmMenuItem packMenu = Factory.New<StmMenuItem>();
			packMenu.SU_MenuName = "Doc Pack";

			StmMenuMenuPivot packPivot = Factory.New<StmMenuMenuPivot>();
			packPivot.SF_SU_Inward = packMenu.PK;
			packPivot.SF_SU_Outward = hAWBMenu.PK;

			TitleCopyCountPair titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Copy 8 - (for Agent)", titles.Title);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Copy 8 - (for Agent)", titles.Title);

			templatePivot.SI_DocumentTitle = "Email Copy";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "EMAIL COPY", titles.Title);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "EMAIL COPY", titles.Title);

			templatePivot.SI_DocumentTitle = "Fax Copy";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "FAX COPY", titles.Title);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "FAX COPY", titles.Title);
		}

		public void TestGetDocumentTitlesForPivotHAWBWhenRunFromConsolForMasterShipmentWithFlagFalse()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;

			ForwardingShipment testShipment = consol.Shipments.AddNew();

			ForwardingShipment masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_NoOriginalBills = 3;
			masterShipment.JS_NoCopyBills = 9;

			testShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			StmMenuItem hAWBMenu = Factory.New<StmMenuItem>();
			hAWBMenu.SU_MenuName = "Laser HAWB";

			StmMenuTemplatePivotBase templatePivot = Factory.New<StmMenuTemplatePivotBase>();
			templatePivot.SI_PrintCopyType = nameof(PrintCopyType.ALL);
			templatePivot.SI_DocumentTitle = "Copy 8 - (for Agent)";
			templatePivot.SI_SU = hAWBMenu.PK;

			StmMenuItem packMenu = Factory.New<StmMenuItem>();
			packMenu.SU_MenuName = "Doc Pack";

			StmMenuMenuPivot packPivot = Factory.New<StmMenuMenuPivot>();
			packPivot.SF_SU_Inward = packMenu.PK;
			packPivot.SF_SU_Outward = hAWBMenu.PK;

			TitleCopyCountPair titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)0, titles.CopyCount);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Copy 8 - (for Agent)", titles.Title);

			templatePivot.SI_DocumentTitle = "Email Copy";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)0, titles.CopyCount);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "EMAIL COPY", titles.Title);

			templatePivot.SI_DocumentTitle = "Fax Copy";
			titles = testShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)0, titles.CopyCount);

			titles = masterShipment.DocumentSupporter.GetDocumentTitlesForPivot(packMenu.SU_MenuName, consol, templatePivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "FAX COPY", titles.Title);
		}

		public void TestGetDocumentTitlesForPivotHouseAirWaybill()
		{
			var testShipment = Factory.New<ShipmentForHAWBTest>();

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Laser HAWB";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_PrintCopyType = nameof(PrintCopyType.PRN);
			pivot.SI_DocumentTitle = "Name";
			pivot.SI_SU = menu.PK;

			var docTitles = new AWBDocumentTitle("", "", false, "", "", false, "", "", false, "Copy 4 - (Delivery Receipt)", "Copy4 Changed", false, "", "", false, "", "", false, "", "", false, "", "", false);
			testShipment.DocumentSupporterforHAWBTest.SetHAWBTitle(docTitles);
			var titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)0, titles.CopyCount);

			docTitles = new AWBDocumentTitle("Name", "Replacement", true, "", "", false, "", "", false, "", "", false, "", "", false, "", "", false, "", "", false, "", "", false);
			testShipment.DocumentSupporterforHAWBTest.SetHAWBTitle(docTitles);
			titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Replacement", titles.Title);

			docTitles = new AWBDocumentTitle("", "", false, "Name", "Replacement", true, "", "", false, "", "", false, "", "", false, "", "", false, "", "", false, "", "", false);
			testShipment.DocumentSupporterforHAWBTest.SetHAWBTitle(docTitles);
			titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Replacement", titles.Title);

			docTitles = new AWBDocumentTitle("", "", false, "", "", false, "Name", "Replacement", true, "", "", false, "", "", false, "", "", false, "", "", false, "", "", false);
			testShipment.DocumentSupporterforHAWBTest.SetHAWBTitle(docTitles);
			titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Replacement", titles.Title);

			docTitles = new AWBDocumentTitle("", "", false, "", "", false, "", "", false, "Name", "Replacement", true, "", "", false, "", "", false, "", "", false, "", "", false);
			testShipment.DocumentSupporterforHAWBTest.SetHAWBTitle(docTitles);
			titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Replacement", titles.Title);

			docTitles = new AWBDocumentTitle("", "", false, "", "", false, "", "", false, "", "", false, "Name", "Replacement", true, "", "", false, "", "", false, "", "", false);
			testShipment.DocumentSupporterforHAWBTest.SetHAWBTitle(docTitles);
			titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Replacement", titles.Title);

			docTitles = new AWBDocumentTitle("", "", false, "", "", false, "", "", false, "", "", false, "", "", false, "Name", "Replacement", true, "", "", false, "", "", false);
			testShipment.DocumentSupporterforHAWBTest.SetHAWBTitle(docTitles);
			titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Replacement", titles.Title);

			docTitles = new AWBDocumentTitle("", "", false, "", "", false, "", "", false, "", "", false, "", "", false, "", "", false, "Name", "Replacement", true, "", "", false);
			testShipment.DocumentSupporterforHAWBTest.SetHAWBTitle(docTitles);
			titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Replacement", titles.Title);

			docTitles = new AWBDocumentTitle("", "", false, "", "", false, "", "", false, "", "", false, "", "", false, "", "", false, "", "", false, "Name", "Replacement", true);
			testShipment.DocumentSupporterforHAWBTest.SetHAWBTitle(docTitles);
			titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "Replacement", titles.Title);

			pivot.SI_DocumentTitle = "Email Copy";
			titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "EMAIL COPY", titles.Title);

			pivot.SI_DocumentTitle = "Fax Copy";
			titles = testShipment.DocumentSupporterforHAWBTest.GetDocumentTitlesForPivot(menu.SU_MenuName, testShipment, pivot);
			AssertEquals((short)1, titles.CopyCount);
			AssertEquals("Title", "FAX COPY", titles.Title);
		}

		#endregion

		#region Printing AWB Populates Shipment Issue Date

		[TestDate(2013, 11, 11)]
		public void TestPopulatingIssueDateDefaultsShipmentIssueDate()
		{
			FreightConfigurationRegistry.Instance.AWBIssueDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate);
			var today = ZDateTime.Today;
			var consolCutOffDate = new ZDateTime(2013, 10, 10);
			var overridenDate = new ZDateTime(2013, 12, 12);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillIssueDate = consolCutOffDate;
			var shipment = consol.Shipments.AddNew();
			var hawb = shipment.AWBHeader;
			hawb.Populate();

			var filter = new DocumentZQuery(BusinessContext.Shipment, CommonShipmentDocumentSupporter.DocumentNames.LaserHAWB);
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			Factory.Save();

			AssertEquals("Pre-condition: DefaultShipmentIssueDateFromAWB registry setting is false by default", false, FreightConfigurationRegistry.Instance.DefaultShipmentIssueDateFromHAWB.Value);
			AssertEquals("Pre-condition: shipment issue date should default as empty", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			//Set up document printing events
			var args = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false);
			var events = new IDocumentEventsMock();
			var supporter = shipment.DocumentSupporter;
			supporter.Initialise(events);

			events.NotifyDocumentPrePrinted(args);
			AssertEquals("Previewing the HAWB should not set the shipment issue date", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			events.NotifyDocumentPrinted(args);
			AssertEquals("The HAWB's issue date should default to the consol cut off date", consolCutOffDate, hawb.EH_AWBIssueDate);
			AssertEquals("Printing the HAWB should default to today rather than the awb issue date as the registry has not been set to true", today, shipment.JS_HouseBillIssueDate);

			FreightConfigurationRegistry.Instance.DefaultShipmentIssueDateFromHAWB.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			shipment.JS_HouseBillIssueDate = ZDateTime.Empty;

			events.NotifyDocumentPrePrinted(args);
			AssertEquals("Previewing the HAWB should never set the shipment issue date", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			events.NotifyDocumentPrinted(args);
			AssertEquals("The HAWB should still be using the consol cut off date for the issue date", consolCutOffDate, hawb.EH_AWBIssueDate);
			AssertEquals("Printing the HAWB should now set it to the consol cut off date", consolCutOffDate, shipment.JS_HouseBillIssueDate);

			shipment.JS_HouseBillIssueDate = ZDateTime.Empty;
			shipment.IsAWBValuesOverriddenProperty = true;
			hawb.EH_AWBIssueDate = overridenDate;

			events.NotifyDocumentPrinted(args);
			AssertEquals("Expected the HAWB issue date to remain overriden", overridenDate, hawb.EH_AWBIssueDate);
			AssertEquals("Shipment issue date should default from the overriden HAWB issue date", overridenDate, shipment.JS_HouseBillIssueDate);

			hawb.EH_AWBIssueDate = today;
			events.NotifyDocumentPrinted(args);
			AssertEquals("If the Shipment issue date is not empty when printing the HAWB, it will not be changed", overridenDate, shipment.JS_HouseBillIssueDate);

			hawb.EH_AWBIssueDate = ZDateTime.Empty;
			shipment.JS_HouseBillIssueDate = ZDateTime.Empty;

			events.NotifyDocumentPrinted(args);
			AssertEquals("If the Shipment issue date is empty, and the HAWB issue date is empty, issue date will not default to today, it should remain empty", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);
		}

		[TestDate(2006, 6, 6)]
		public void TestHAWBPrinted()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			AssertEquals("Pre-condition: JS_HouseBillIssueDate is not set", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			var filter = new DocumentZQuery(BusinessContext.Shipment, CommonShipmentDocumentSupporter.DocumentNames.LaserHAWB);
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false);
			var events = new IDocumentEventsMock();
			var supporter = shipment.DocumentSupporter;
			supporter.Initialise(events);

			events.NotifyDocumentPrinted(eventArgs);
			AssertEquals("JS_HouseBillIssueDate is set", ZDateTime.Today, shipment.JS_HouseBillIssueDate);
			FreightConfigurationRegistry.Instance.DefaultShipmentIssueDateFromHAWB.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, shipment.IsAWBLoaded);
		}

		public void TestSaveOnPrintingCartageAdvice()
		{
			var previousIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = true;

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				Factory.Save();

				var savedCount = 0;
				shipment.OnShipmentSaved += (sender, e) => savedCount++;

				AssertEquals("Pre-condition", ZDate.Empty, shipment.JS_HouseBillIssueDate);

				var filter = new DocumentZQuery(BusinessContext.Shipment, DocumentNames.LaserHAWB);
				var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

				var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false);
				var events = new IDocumentEventsMock();
				var supporter = shipment.DocumentSupporter;
				supporter.Initialise(events);

				events.NotifyDocumentPrinted(eventArgs);
				AssertEquals("JS_HouseBillIssueDate is set", ZDateTime.Today, shipment.JS_HouseBillIssueDate);
				AssertEquals("Shipment has been saved", 1, savedCount);

				Globals.IsUserInteractive = false;

				shipment.JS_HouseBillIssueDate = ZDate.Empty;
				shipment.JS_GoodsDescription = "Has changes";

				events.NotifyDocumentPrinted(eventArgs);
				AssertEquals("JS_HouseBillIssueDate is set", ZDateTime.Today, shipment.JS_HouseBillIssueDate);
				AssertEquals("Shipment has not been saved again", 1, savedCount);
			}
			finally
			{
				Globals.IsUserInteractive = previousIsUserInteractive;
			}
		}

		[TestDate(2014, 1, 1)]
		public void TestFutureMilestoneDoesNotPreventPrintingCartageAdvice()
		{
			var previousIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportCodes.Air;
				shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
				shipment.DocsAndCartage.JP_LCLDatesOverrideConsol = ZBool.True;
				shipment.JS_IsForwardRegistered = true;
				WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Globals.IsUserInteractive = true;

				var cargoMilestone = shipment.WorkflowItems.Milestones.AddNew();
				cargoMilestone.P9_Description = "CAV";
				cargoMilestone.TriggerConditions.TriggerEventCode = Events.CargoAvailableCode;

				Factory.Save();

				var savedCount = 0;
				shipment.OnShipmentSaved += (sender, e) => savedCount++;

				//Create a milestone event with future date
				shipment.DocsAndCartage.JP_LCLAvailable = ZDateTime.Now.AddDays(10);

				var filter = new DocumentZQuery(BusinessContext.Shipment, DocumentNames.CartageAdvice);
				var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
				var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false);
				var events = new IDocumentEventsMock();
				var supporter = shipment.DocumentSupporter;
				supporter.Initialise(events);
				shipment.JS_GoodsDescription = "Has changes";

				events.NotifyDocumentPrinted(eventArgs);
				AssertHasError("Validation error message", cargoMilestone.P9_ActualDateInfo, "Milestones cannot have an Actual Start time that is in the future.");
				AssertEquals("Shipment should be saved when printing document", 1, savedCount);
			}
			finally
			{
				Globals.IsUserInteractive = previousIsUserInteractive;
			}
		}

		[TestDate(2014, 1, 1)]
		public void TestHAWBPrinted_DoNotLoadAWBIfNotNeeded()
		{
			try
			{
				FreightConfigurationRegistry.Instance.DefaultShipmentIssueDateFromHAWB.SetValue(
					Env.CurrentCompany.PK,
					Guid.Empty,
					Guid.Empty,
					false);

				var shipment = Factory.New<ForwardingShipment>();
				Factory.Save();

				AssertEquals("Pre-condition: JS_HouseBillIssueDate is not set", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

				var filter = new DocumentZQuery(BusinessContext.Shipment,
					CommonShipmentDocumentSupporter.DocumentNames.LaserHAWB);
				var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

				var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false);
				var events = new IDocumentEventsMock();
				var supporter = shipment.DocumentSupporter;
				supporter.Initialise(events);

				events.NotifyDocumentPrinted(eventArgs);
				AssertEquals("JS_HouseBillIssueDate is set", ZDateTime.Today, shipment.JS_HouseBillIssueDate);

				AssertEquals(false, shipment.IsAWBLoaded);
			}
			finally
			{
				FreightConfigurationRegistry.Instance.DefaultShipmentIssueDateFromHAWB.SetValue(
					Env.CurrentCompany.PK,
					Guid.Empty,
					Guid.Empty,
					FreightConfigurationRegistry.Instance.DefaultShipmentIssueDateFromHAWB.DefaultValue);
			}
		}

		[TestDate(2006, 6, 6)]
		public void TestHAWBEmailed_Faxed()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			AssertEquals("Pre-condition: JS_HouseBillIssueDate is not set", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);

			var filter = new DocumentZQuery(BusinessContext.Shipment, CommonShipmentDocumentSupporter.DocumentNames.LaserHAWB);
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);

			var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menuItem, false);
			var events = new IDocumentEventsMock();
			var supporter = shipment.DocumentSupporter;
			supporter.Initialise(events);

			events.NotifyDocumentPrinted(eventArgs);
			AssertEquals("JS_HouseBillIssueDate is set", ZDateTime.Today, shipment.JS_HouseBillIssueDate);
		}

		#endregion

		public void TestPrintColoadsOnManifest()
		{
			var documentSupporter = new ForwardingShipmentDocumentSupporterForTest(Factory.New<ForwardingShipment>());

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			AssertEquals(false, documentSupporter.PrintColoadsOnManifest(consol));

			consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals(true, documentSupporter.PrintColoadsOnManifest(consol));

			consol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			AssertEquals(true, documentSupporter.PrintColoadsOnManifest(consol));
		}

		[ExpectNoExceptions]
		public void TestGetDocumentWrappersInternalNullRef()
		{
			var ds = new ForwardingShipmentDocumentSupporterForTest(Factory.New<ForwardingShipment>());

			ds.GetDocumentWrappersInternal_Exposed(Core.Constants.DataContext.GenericFreightJobServices, null);

			ds.GetDocumentWrappersInternal_Exposed(Core.Constants.DataContext.AWB, null);

			ds.GetDocumentWrappersInternal_Exposed(Core.Constants.DataContext.GenericFreightJobByComInv, null);
			ds.GetDocumentWrappersInternal_Exposed(Core.Constants.DataContext.IMO, null);
		}

		public void TestSuspendDeclarationInGenericFreightJobByPackages()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = "PLT";

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var dataSource = new DocBuilderDataSource { Brokerage = true };
			DocumentsDataRegistry.Instance.DocBuilderDataSource.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, dataSource);

			var ds = new ForwardingShipmentDocumentSupporterForTest(shipment);
			var wappers = ds.GetDocumentWrappersInternal_Exposed(Core.Constants.DataContext.GenericFreightJobByPackages, null);

			AssertEquals(10, wappers.Length);
			foreach (var wrapper in wappers)
			{
				AssertEquals("wrapper.GetType().Name", "FreightWrapperFromShipment", wrapper.GetType().Name);
			}
		}

		public void TestGetDocumentWrappersInternalHandlesNullCommandBeingRunForAWB()
		{
			ForwardingShipmentDocumentSupporterForTest ds = new ForwardingShipmentDocumentSupporterForTest(Factory.New<ForwardingShipment>());

			DocumentWrapper[] dw = ds.GetDocumentWrappersInternal_Exposed(Core.Constants.DataContext.AWB, null);
			AssertNull("DocumentWrapper[] should be null when Core.Constants.DataContext.AWB commandBeingRun is passed as null", dw);
		}

		public void TestNeutralHAWB_SecurityRights()
		{
			RunPrintFinalAWBMaster_SecurityRightsTestOn("Staff", "Neutral HAWB", runTestOnGroupPermissions: false);
			RunPrintFinalAWBMaster_SecurityRightsTestOn("Group", "Neutral HAWB", runTestOnGroupPermissions: true);
		}

		public void TestAWBBarcodeLabel_SecurityRights()
		{
			RunPrintFinalAWBMaster_SecurityRightsTestOn("Staff", "AWB Barcode Label", runTestOnGroupPermissions: false);
			RunPrintFinalAWBMaster_SecurityRightsTestOn("Group", "AWB Barcode Label", runTestOnGroupPermissions: true);
		}

		public void TestLaserHAWBwithFollowonPage_SecurityRights()
		{
			RunPrintFinalAWBMaster_SecurityRightsTestOn("Staff", "Laser HAWB with Follow on Page", runTestOnGroupPermissions: false);
			RunPrintFinalAWBMaster_SecurityRightsTestOn("Group", "Laser HAWB with Follow on Page", runTestOnGroupPermissions: true);
		}

		public void TestLaserHAWBExportRateLinesDataWithOverrideRateLines()
		{
			var (shipment, menuItem) = SetUpShipmentForLaserHAWBPrinting();

			shipment.AWBHeader.EH_AreRateLinesOverridden = true;

			var documentSupporter = new ForwardingShipmentDocumentSupporterForTest(shipment);
			var result = documentSupporter.GetDocumentWrappersInternal_Exposed(Core.Constants.DataContext.AWB, menuItem)[0];
			var resultType = result.GetType();
			var noPieces1 = (ZString)resultType.GetProperty("NoPieces1").GetValue(result);
			var grossWeight1 = (ZString)resultType.GetProperty("GrossWeight1").GetValue(result);
			var chargeableWeight1 = (ZString)resultType.GetProperty("ChargeableWeight1").GetValue(result);

			AssertEquals("1", noPieces1);
			AssertEquals("25.0", grossWeight1);
			AssertEquals("35.0", chargeableWeight1);
		}

		public void TestLaserHAWBExportRateLinesDataWithoutOverrideRateLines()
		{
			var (shipment, menuItem) = SetUpShipmentForLaserHAWBPrinting();

			shipment.AWBHeader.EH_AreRateLinesOverridden = false;

			var documentSupporter = new ForwardingShipmentDocumentSupporterForTest(shipment);
			var result = documentSupporter.GetDocumentWrappersInternal_Exposed(Core.Constants.DataContext.AWB, menuItem)[0];

			var resultType = result.GetType();
			var noPieces1 = (ZString)resultType.GetProperty("NoPieces1").GetValue(result);
			var grossWeight1 = (ZString)resultType.GetProperty("GrossWeight1").GetValue(result);
			var chargeableWeight1 = (ZString)resultType.GetProperty("ChargeableWeight1").GetValue(result);

			AssertEquals("5", noPieces1);
			AssertEquals("125.0", grossWeight1);
			AssertEquals("175.0", chargeableWeight1);
		}

		(ForwardingShipment, StmMenuItem) SetUpShipmentForLaserHAWBPrinting()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var mockRateLine = shipment.AWBHeader.AWBRateLines[0];
			mockRateLine.ER_NoOfPiecesOrRCP = "1";
			mockRateLine.ER_GrossWeight = 25;
			mockRateLine.ER_ChargeableWeight = 35;

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Laser HAWB";

			shipment.JS_OuterPacks = 5;
			shipment.JS_ActualWeight = 125;
			shipment.JS_ActualChargeable = 175;
			shipment.JS_UnitOfWeight = "KG";

			return (shipment, menuItem);
		}

		public void RunPrintFinalAWBMaster_SecurityRightsTestOn(string securityTargetName, string documentMenu, bool runTestOnGroupPermissions)
		{
			#region Setup

			var currentUser = Factory.NewWithValidTestData<GlbStaff>();
			currentUser.GS_IsOperational = true;

			BusinessObject securityTarget = currentUser;
			if (runTestOnGroupPermissions)
			{
				var group = currentUser.Groups.AddNew();
				group.GG_Code = "AAA";

				securityTarget = group;
			}

			var menuQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, documentMenu);
			menuQuery.AddToFilter(new ZQuery(StmMenuItemSchema.SU_MenuPath, "AWB"));

			var menuItem = Factory.LoadTop1<StmMenuItem>(menuQuery);
			var jobShipmentModuleId = ModuleIDs.JobShipment;

			Factory.Save();

			#endregion

			using (var module = ObjectFactory.Get<IModuleFactory>().Create(jobShipmentModuleId))
			{
				var supporter = new ForwardingShipmentDocumentSupporter(Factory.New<ForwardingShipment>());
				var parentCheckpoint = Env.Security.FindOrCreateDocumentsCheckpoint(jobShipmentModuleId, module.SecurityCheckpoint);
				var explicitCheckpoint = Env.Security.FindOrCreateDocumentCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.JobShipment, parentCheckpoint);

				void GetDataStateAndAssertError(string message, bool shouldHaveError)
				{
					using (Env.Instance.SetTemporaryUserContext(currentUser.PK.ToGuid(), Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
					{
						var dataState = supporter.GetDataStateBeforeRun(menuItem);
						var errorMessage = shouldHaveError ? explicitCheckpoint.ErrorMessageForNotAllowed : string.Empty;

						CombineAssertions(message, delegate
						{
							AssertEquals(!shouldHaveError, dataState.IsValid);
							AssertEquals(errorMessage, dataState.ErrorMessage);
						});
					}
				}

				GetDataStateAndAssertError("Pre-Condition - Default has no permissions", shouldHaveError: true);

				AddSecurityPermission(securityTarget, parentCheckpoint, true);
				Factory.Save();

				GetDataStateAndAssertError($"{securityTargetName} now has implicit permission so should be able to use it (Implicit Yes)", shouldHaveError: false);

				UpdateSecurityPermission(securityTarget, parentCheckpoint, false);
				Factory.Save();

				GetDataStateAndAssertError($"{securityTargetName} now has implicit permission so should be able to use it (Implicit No)", shouldHaveError: true);

				UpdateSecurityPermission(securityTarget, parentCheckpoint, true);
				AddSecurityPermission(securityTarget, explicitCheckpoint, false);
				Factory.Save();

				GetDataStateAndAssertError($"{securityTargetName} has implicit permission but explicit takes priority so access is denied (Explicit No)", shouldHaveError: true);

				UpdateSecurityPermission(securityTarget, explicitCheckpoint, true);
				Factory.Save();

				GetDataStateAndAssertError($"{securityTargetName} now has explicit permission so should be able to use it (Explicit Yes)", shouldHaveError: false);
			}
		}

		void AddSecurityPermission(BusinessObject target, SecurityCheckpoint checkpoint, bool allowed)
		{
			var permission = Factory.New<GlbSecurity>();
			permission.GU_SecurityItemIsAllowed = allowed;
			permission.GU_SecurityRight = checkpoint.Code;
			permission.GU_ItemGUID = checkpoint.ItemGuid;

			if (target is GlbStaff)
			{
				permission.GU_GS = target.PK;
			}
			else if (target is GlbGroup)
			{
				permission.GU_GG = target.PK;
			}
		}

		void UpdateSecurityPermission(BusinessObject target, SecurityCheckpoint checkpoint, bool value)
		{
			var permissionQuery = new ZQuery(GlbSecuritySchema.GU_SecurityRight, checkpoint.Code);

			if (target is GlbStaff)
			{
				permissionQuery.AddToFilter(GlbSecuritySchema.GU_GS, target.PK);
			}
			else if (target is GlbGroup)
			{
				permissionQuery.AddToFilter(GlbSecuritySchema.GU_GG, target.PK);
			}

			var permission = Factory.LoadTop1<GlbSecurity>(permissionQuery);
			permission.GU_SecurityItemIsAllowed = value;
		}

		#region Implementation

		class ShipmentForHAWBTest : ForwardingShipment
		{
			public ShipmentForHAWBTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ShipmentForHAWBTestDocumentSupporter DocumentSupporterforHAWBTest
			{
				get
				{
					if (fDocumentSupporterforHAWBTest == null)
					{
						fDocumentSupporterforHAWBTest = new ShipmentForHAWBTestDocumentSupporter(this);
					}
					return fDocumentSupporterforHAWBTest;
				}
			}

			protected ShipmentForHAWBTestDocumentSupporter fDocumentSupporterforHAWBTest;

			public class ShipmentForHAWBTestDocumentSupporter : CommonShipmentDocumentSupporter
			{
				public ShipmentForHAWBTestDocumentSupporter(ShipmentForHAWBTest shipment)
					: base(shipment)
				{
				}

				public void SetHAWBTitle(AWBDocumentTitle hAWBTitle)
				{
					this.fHAWBTitle = hAWBTitle;
				}

				protected override AWBDocumentTitle HAWBTitle
				{
					get { return fHAWBTitle; }
				}

				protected AWBDocumentTitle fHAWBTitle;
			}
		}

		AccChargeCode CreateChargeCode(string chargeCode)
		{
			AccChargeCode code = Factory.New<AccChargeCode>();
			code.AC_Code = chargeCode;
			code.AC_Desc = "Test Charge Code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;
			code.AC_IsActive = ZBool.True;
			return code;
		}

		JobCharge CreateLineCharge(JobHeader jobHeaderBizObj, ZGuid localChargesPK, ZDecimal amount, ZGuid chargeCodePK)
		{
			JobCharge lineCharge = Factory.New<JobCharge>();
			lineCharge.JR_JH = jobHeaderBizObj.PK;
			lineCharge.JR_GE = jobHeaderBizObj.JH_GE;
			lineCharge.JR_GB = jobHeaderBizObj.JH_GB;
			lineCharge.JR_AC = chargeCodePK;
			lineCharge.JR_LocalSellAmt = amount;
			lineCharge.JR_OH_SellAccount = localChargesPK;
			return lineCharge;
		}

		ForwardingShipment GetShipment()
		{
			return Factory.New<ForwardingShipment>();
		}

		#endregion
	}
}
