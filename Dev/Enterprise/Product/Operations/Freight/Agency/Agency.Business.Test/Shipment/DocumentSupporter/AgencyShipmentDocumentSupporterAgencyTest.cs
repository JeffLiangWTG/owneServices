using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using BitMap = System.Drawing.Bitmap;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentDocumentSupporterAgencyTest : BaseAgencyTest
	{
		public const string DocAgencyShipment = "DocAgencyShipment";
		public const string DocAgencyContainer = "DocAgencyContainer";
		public void TestLocalPort()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUCNS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = voyage.Sailings[0].PK;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "HKHKG";
			ContactType[] contacts = { ContactType.ShippingLine, ContactType.FreightAgent, ContactType.Consignor, ContactType.ExportFreightAgent, ContactType.ExportSeaFreightAgent, ContactType.Consignee, ContactType.ImportFreightAgent, ContactType.ImportSeaFreightAgent, };
			DocumentDirection[] directions = { DocumentDirection.ANY, DocumentDirection.DEP, DocumentDirection.ARV };
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendLine("     ANY   DEP   ARV  ");
			foreach (ContactType contact in contacts)
			{
				string code = contact.Code;
				string anyPort = shipment.DocumentSupporter.LocalPort(contact, DocumentDirection.ANY);
				string depPort = shipment.DocumentSupporter.LocalPort(contact, DocumentDirection.DEP);
				string arvPort = shipment.DocumentSupporter.LocalPort(contact, DocumentDirection.ARV);
				builder.Append(code);
				builder.Append(' ', 4 - code.Length);
				builder.Append(anyPort);
				builder.Append(' ', 6 - anyPort.Length);
				builder.Append(depPort);
				builder.Append(' ', 6 - depPort.Length);
				builder.Append(arvPort);
				builder.Append(' ', 6 - arvPort.Length);
				builder.AppendLine();
			}

			const string expected = @"
     ANY   DEP   ARV  
SHP                   
FWD       AUBNE HKHKG 
CNR AUBNE AUBNE AUBNE 
FWE AUBNE AUBNE AUBNE 
FES AUBNE AUBNE AUBNE 
CNE HKHKG HKHKG HKHKG 
FWI HKHKG HKHKG HKHKG 
FIS HKHKG HKHKG HKHKG 
";
			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestForeignPort()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUCNS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = voyage.Sailings[0].PK;
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "HKHKG";
			ContactType[] contacts = { ContactType.ShippingLine, ContactType.FreightAgent, ContactType.Consignor, ContactType.ExportAirFreightAgent, ContactType.ExportFreightAgent, ContactType.ExportSeaFreightAgent, ContactType.Consignee, ContactType.ImportAirFreightAgent, ContactType.ImportFreightAgent, ContactType.ImportSeaFreightAgent, };
			DocumentDirection[] directions = { DocumentDirection.ANY, DocumentDirection.DEP, DocumentDirection.ARV };
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendLine("     ANY   DEP   ARV  ");
			foreach (ContactType contact in contacts)
			{
				string code = contact.Code;
				string anyPort = shipment.DocumentSupporter.ForeignPort(contact, DocumentDirection.ANY);
				string depPort = shipment.DocumentSupporter.ForeignPort(contact, DocumentDirection.DEP);
				string arvPort = shipment.DocumentSupporter.ForeignPort(contact, DocumentDirection.ARV);
				builder.Append(code);
				builder.Append(' ', 4 - code.Length);
				builder.Append(anyPort);
				builder.Append(' ', 6 - anyPort.Length);
				builder.Append(depPort);
				builder.Append(' ', 6 - depPort.Length);
				builder.Append(arvPort);
				builder.Append(' ', 6 - arvPort.Length);
				builder.AppendLine();
			}

			const string expected = @"
     ANY   DEP   ARV  
SHP       HKHKG AUBNE 
FWD       HKHKG AUBNE 
CNR HKHKG HKHKG HKHKG 
FEA HKHKG HKHKG HKHKG 
FWE HKHKG HKHKG HKHKG 
FES HKHKG HKHKG HKHKG 
CNE AUBNE AUBNE AUBNE 
FIA AUBNE AUBNE AUBNE 
FWI AUBNE AUBNE AUBNE 
FIS AUBNE AUBNE AUBNE 
";
			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestNotifyParty()
		{
			OrgHeader notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "Notify Party";
			notifyParty.MainAddress.OA_Address1 = "np address1";
			OrgHeader notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2.OH_FullName = "Notify Party2";
			notifyParty2.MainAddress.OA_Email = "np2 email";
			OrgHeader notifyParty3 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty3.OH_FullName = "Notify Party3";
			notifyParty3.MainAddress.OA_Fax = "np3 fax";
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.MainAddress.OA_Address1 = "cne address1";
			// consignee
			AgencyBooking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			AssertEquals(consignee.PK, DocumentSupporter.GetContactOrganisation("", ContactType.NotifyParty, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(null, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.NotifyParty, DocumentDirection.ANY));
			AgencyBooking.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(null, DocumentSupporter.GetContactOrganisation("", ContactType.NotifyParty, DocumentDirection.ANY).OrgHeader);
			AssertEquals(AgencyBooking.ConsigneeDocumentaryAddress, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.NotifyParty, DocumentDirection.ANY));
			// notify party 3
			AgencyBooking.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;
			AssertEquals(notifyParty3.PK, DocumentSupporter.GetContactOrganisation("", ContactType.NotifyParty, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(null, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.NotifyParty, DocumentDirection.ANY));
			AgencyBooking.NotifyParty3DocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(null, DocumentSupporter.GetContactOrganisation("", ContactType.NotifyParty, DocumentDirection.ANY).OrgHeader);
			AssertEquals(AgencyBooking.NotifyParty3DocumentaryAddress, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.NotifyParty, DocumentDirection.ANY));
			// notify party 2
			AgencyBooking.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
			AssertEquals(notifyParty2.PK, DocumentSupporter.GetContactOrganisation("", ContactType.NotifyParty, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(null, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.NotifyParty, DocumentDirection.ANY));
			AgencyBooking.NotifyParty2DocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(null, DocumentSupporter.GetContactOrganisation("", ContactType.NotifyParty, DocumentDirection.ANY).OrgHeader);
			AssertEquals(AgencyBooking.NotifyParty2DocumentaryAddress, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.NotifyParty, DocumentDirection.ANY));
			// notify party
			AgencyBooking.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			AssertEquals(notifyParty.PK, DocumentSupporter.GetContactOrganisation("", ContactType.NotifyParty, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(null, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.NotifyParty, DocumentDirection.ANY));
			AgencyBooking.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			AgencyBooking.NotifyPartyDocumentaryAddress.E2_Address1 = "Notify Party";
			AssertEquals(null, DocumentSupporter.GetContactOrganisation("", ContactType.NotifyParty, DocumentDirection.ANY).OrgHeader);
			AssertEquals(AgencyBooking.NotifyPartyDocumentaryAddress, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.NotifyParty, DocumentDirection.ANY));
		}

		public void TestExportFreightAgent()
		{
			OrgHeader bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_FullName = "Booking Party";
			bookingParty.MainAddress.OA_Address1 = "booking party address";
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Consignor";
			consignor.MainAddress.OA_Address1 = "consignor address";
			AgencyBooking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			AssertEquals(consignor.PK, DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(null, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.ExportFreightAgent, DocumentDirection.ANY));
			AssertEquals(consignor.PK, DocumentSupporter.GetContactOrganisation("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(null, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY));
			AgencyBooking.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(null, DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(AgencyBooking.ConsignorDocumentaryAddress, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.ExportFreightAgent, DocumentDirection.ANY));
			AssertEquals(null, DocumentSupporter.GetContactOrganisation("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(AgencyBooking.ConsignorDocumentaryAddress, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY));
			AgencyBooking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			AssertEquals(bookingParty.PK, DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(null, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.ExportFreightAgent, DocumentDirection.ANY));
			AssertEquals(bookingParty.PK, DocumentSupporter.GetContactOrganisation("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY).OrgHeader.PK);
			AssertEquals(null, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY));
			AgencyBooking.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			AgencyBooking.BookingPartyDocumentaryAddress.E2_Address1 = "booking party address";
			AssertEquals(null, DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(AgencyBooking.BookingPartyDocumentaryAddress, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.ExportFreightAgent, DocumentDirection.ANY));
			AssertEquals(null, DocumentSupporter.GetContactOrganisation("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(AgencyBooking.BookingPartyDocumentaryAddress, DocumentSupporter.GetOverriddenDeliveryDetails("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY));
		}

		public void TestGetDocBusinessObjects_Shipment()
		{
			AssertHasWrappersFor(Constants.DataContext.AgencyShipment, DocumentDirection.ARV, DocAgencyShipment, AgencyBooking);
			AssertHasWrappersFor(Constants.DataContext.Shipment, DocumentDirection.DEP, DocAgencyShipment, AgencyBooking);
			AssertHasWrappersFor(Constants.DataContext.PreAlert, DocumentDirection.ARV, DocAgencyShipment, AgencyBooking);
		}

		public void TestGetDocBusinessObjects_Container()
		{
			AgencyBooking.JS_PackingMode = Constants.ContainerModes.FCL;
			AgencyBookingContainer container1 = AgencyBooking.BookedContainers.AddNew();
			AgencyBookingContainer container2 = AgencyBooking.BookedContainers.AddNew();
			AssertHasWrappersFor(Constants.DataContext.AgencyContainer, DocumentDirection.DEP, DocAgencyContainer, container1, container2);
			AssertHasWrappersFor(Constants.DataContext.Container, DocumentDirection.DEP, DocAgencyContainer, container1, container2);
		}

		public void TestGetDocBusinessObjects_CartageAdvice()
		{
			AgencyBooking.JS_PackingMode = Constants.ContainerModes.Bulk;
			AssertHasWrappersFor(Constants.DataContext.CartageAdvice, DocumentDirection.ARV, DocAgencyShipment, AgencyBooking);
			AgencyBooking.JS_PackingMode = Constants.ContainerModes.FCL;
			AgencyBookingContainer container = AgencyBooking.BookedContainers.AddNew();
			AssertHasWrappersFor(Constants.DataContext.CartageAdvice, DocumentDirection.ARV, DocAgencyContainer, container);
		}

		public void TestGetDocBusinessObjects_GenericFreightJob()
		{
			AssertHasWrappersFor(Constants.DataContext.GenericFreightJob, DocumentDirection.ARV, "FreightWrapperFromAgencyShipment", AgencyBooking);
		}

		public void TestGetDocBusinessObjects_IMO()
		{
			AgencyBooking.JS_PackingMode = Constants.ContainerModes.FCL;
			AgencyBookingContainer container1 = AgencyBooking.BookedContainers.AddNew();
			AgencyBookingContainer container2 = AgencyBooking.BookedContainers.AddNew();
			AgencyBookingContainer container3 = AgencyBooking.BookedContainers.AddNew();
			AgencyBookingContainer container4 = AgencyBooking.BookedContainers.AddNew();
			AgencyBookingPackLine packLine1 = AgencyBooking.OuterPackLines.AddNew();
			AgencyBookingPackLine packLine2 = AgencyBooking.OuterPackLines.AddNew();
			AgencyBookingPackLine packLine3 = AgencyBooking.OuterPackLines.AddNew();
			AgencyBookingPackLine packLine4 = AgencyBooking.OuterPackLines.AddNew();
			AgencyBookingPackLine packLine5 = AgencyBooking.OuterPackLines.AddNew();
			packLine5.JL_JC = container3.PK;
			GetExpectedWrapperNameDelegate wrapperName = wrappedObject =>
			{
				if (wrappedObject is AgencyShipmentContainer)
				{
					return DocAgencyContainer;
				}

				if (wrappedObject is AgencyShipment)
				{
					return DocAgencyShipment;
				}

				return "<Error>";
			};
			AssertHasWrappersFor(Constants.DataContext.IMO, DocumentDirection.ARV, wrapperName);
			packLine1.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;
			packLine2.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;
			packLine4.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;
			AssertHasWrappersFor(Constants.DataContext.IMO, DocumentDirection.ARV, wrapperName, AgencyBooking);
			packLine1.JL_JC = container1.PK;
			packLine4.JL_JC = container4.PK;
			AssertHasWrappersFor(Constants.DataContext.IMO, DocumentDirection.ARV, wrapperName, container1, container4, AgencyBooking);
			packLine2.JL_JC = container1.PK;
			AssertHasWrappersFor(Constants.DataContext.IMO, DocumentDirection.ARV, wrapperName, container1, container4);
		}

		public void TestSupportedDataContexts()
		{
			AssertEquals("Constants.DataContext.Container is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Container)));
			AssertEquals("Constants.DataContext.Shipment is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Shipment)));
			AssertEquals("Constants.DataContext.IMO is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.IMO)));
			AssertEquals("Constants.DataContext.CartageAdvice is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CartageAdvice)));
			AssertEquals("Constants.DataContext.PreAlert is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.PreAlert)));
			AssertEquals("Constants.DataContext.GenericFreightJob is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		public void TestGetFilterValue()
		{
			AgencyBooking.JS_PackingMode = Constants.ContainerModes.FCL;
			AssertEquals(Constants.ContainerModes.FCL, DocumentSupporter.GetFilterValue(DocumentFilters.CNT));
			AgencyBooking.JS_PackingMode = Constants.ContainerModes.LCL;
			AssertEquals(Constants.ContainerModes.LCL, DocumentSupporter.GetFilterValue(DocumentFilters.CNT));
		}

		public void TestGetMenuTemplateFilterValue()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			OrgHeader principal3 = NewPrincipal();
			Factory.Save();
			PrincipalBrandingCollection brandingCollection = new PrincipalBrandingCollection();
			PrincipalBranding branding1 = brandingCollection.AddNew();
			branding1.PrincipalPK = principal1.PK;
			branding1.Code = "PB1";
			branding1.BrandName = "Brand 1";
			branding1.BrandEmailAddress = "generic@brand1.com";
			branding1.Image = new BitMap(10, 10);
			PrincipalBranding branding2 = brandingCollection.AddNew();
			branding2.PrincipalPK = principal2.PK;
			branding2.Code = "PB2";
			branding2.BrandName = "Brand 2";
			branding2.BrandEmailAddress = "generic@brand2.com";
			branding2.Image = new BitMap(10, 10);
			DocumentsDataRegistry.Instance.PrincipalDocumentBrand.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brandingCollection);
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			AssertEquals("DEFAULT", shipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.OBL, null));
			shipment.JS_OH_DeliveryAgent = principal1.PK;
			AssertEquals(branding1.Code, shipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.OBL, null));
			shipment.JS_OH_DeliveryAgent = principal2.PK;
			AssertEquals(branding2.Code, shipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.OBL, null));
			shipment.JS_OH_DeliveryAgent = principal3.PK;
			AssertEquals("DEFAULT", shipment.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.OBL, null));
		}

		public void TestGetAlternateBranding()
		{
			OrgHeader principal1 = NewPrincipal();
			OrgHeader principal2 = NewPrincipal();
			OrgHeader principal3 = NewPrincipal();
			Factory.Save();
			PrincipalBrandingCollection brandingCollection = new PrincipalBrandingCollection();
			PrincipalBranding branding1 = brandingCollection.AddNew();
			branding1.PrincipalPK = principal1.PK;
			branding1.Code = "PB1";
			branding1.BrandName = "Brand 1";
			branding1.BrandEmailAddress = "generic@brand1.com";
			branding1.Image = new BitMap(10, 10);
			PrincipalBranding branding2 = brandingCollection.AddNew();
			branding2.PrincipalPK = principal2.PK;
			branding2.Code = "PB2";
			branding2.BrandName = "Brand 2";
			branding2.BrandEmailAddress = "generic@brand2.com";
			branding2.Image = new BitMap(10, 10);
			DocumentsDataRegistry.Instance.PrincipalDocumentBrand.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brandingCollection);
			BillOfLading shipment = Factory.New<BillOfLading>();
			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			AssertEquals(null, shipment.DocumentSupporter.GetAlternativeBranding());
			shipment.JS_OH_DeliveryAgent = principal1.PK;
			AssertEquals(branding1.Code, shipment.DocumentSupporter.GetAlternativeBranding().Code);
			shipment.JS_OH_DeliveryAgent = principal2.PK;
			AssertEquals(branding2.Code, shipment.DocumentSupporter.GetAlternativeBranding().Code);
			shipment.JS_OH_DeliveryAgent = principal3.PK;
			AssertEquals(null, shipment.DocumentSupporter.GetAlternativeBranding());
		}

		public void TestBusinessContext_AgencyBooking()
		{
			AssertEquals(CargoWise.Definitions.BusinessContext.AgencyBooking, DocumentSupporter.BusinessContext);
		}

		public void TestBusinessContext_BillOfLading()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			AgencyShipmentDocumentSupporter docSupporter = new AgencyShipmentDocumentSupporter(shipment);
			AssertEquals(CargoWise.Definitions.BusinessContext.AgencyDocumentation, docSupporter.BusinessContext);
		}

		public void TestGetMenuItemForVisualisationData()
		{
			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty,Guid.Empty, Guid.Empty, false))
			{
				var documentSupporter = new AgencyShipmentDocumentSupporter(Factory.New<BillOfLading>());
				var query = new ZQuery();
				query.AddToFilter(StmMenuItemSchema.SU_MenuName, "Send Electronic Original Bill of Lading");
				query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.AgencyDocumentation));

				var originDocumentMenuItem = Factory.LoadTop1<StmMenuItem>(query);
				AssertNotNull(originDocumentMenuItem);

				var copyDocumentMenuItem = documentSupporter.GetMenuItemForVisualisationData(originDocumentMenuItem);
				AssertNotEquals(originDocumentMenuItem, copyDocumentMenuItem);
				AssertEquals("Bill of Lading", copyDocumentMenuItem.SU_MenuName);
				AssertEquals("Export", copyDocumentMenuItem.SU_MenuPath);
				AssertEquals("AgencyDocumentation", copyDocumentMenuItem.SU_BusinessContext);
				AssertEquals(true, copyDocumentMenuItem.SU_IsSystemDefined);
				AssertEquals(false, copyDocumentMenuItem.SU_IsClientSpecific);
				AssertEquals("\"<UseNewFormBuilderBillOfLading>\" != \"Y\"", copyDocumentMenuItem.SU_FilterList);
				AssertEquals(ZString.Empty, copyDocumentMenuItem.SU_GS_NKStaffCode);

				documentSupporter = new AgencyShipmentDocumentSupporter(Factory.New<AgencyBooking>());
				AssertEquals(originDocumentMenuItem, documentSupporter.GetMenuItemForVisualisationData(originDocumentMenuItem));
			}

			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentSupporter = new AgencyShipmentDocumentSupporter(Factory.New<BillOfLading>());
				var query = new ZQuery();
				query.AddToFilter(StmMenuItemSchema.SU_MenuName, "Send Electronic Original Bill of Lading");
				query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(CargoWise.Definitions.BusinessContext.AgencyDocumentation));

				var originDocumentMenuItem = Factory.LoadTop1<StmMenuItem>(query);
				AssertNotNull(originDocumentMenuItem);

				var copyDocumentMenuItem = documentSupporter.GetMenuItemForVisualisationData(originDocumentMenuItem);
				AssertNotEquals(originDocumentMenuItem, copyDocumentMenuItem);
				AssertEquals("Bill of Lading", copyDocumentMenuItem.SU_MenuName);
				AssertEquals("Legacy Documents/Export", copyDocumentMenuItem.SU_MenuPath);
				AssertEquals("AgencyDocumentation", copyDocumentMenuItem.SU_BusinessContext);
				AssertEquals(true, copyDocumentMenuItem.SU_IsSystemDefined);
				AssertEquals(false, copyDocumentMenuItem.SU_IsClientSpecific);
				AssertEquals("\"<UseNewFormBuilderBillOfLading>\" == \"Y\" && \"<UseDocBuilderLinerAndAgencyDocs>\" != \"Y\"", copyDocumentMenuItem.SU_FilterList);
				AssertEquals(ZString.Empty, copyDocumentMenuItem.SU_GS_NKStaffCode);

				documentSupporter = new AgencyShipmentDocumentSupporter(Factory.New<AgencyBooking>());
				AssertEquals(originDocumentMenuItem, documentSupporter.GetMenuItemForVisualisationData(originDocumentMenuItem));
			}
		}

		#region Implementation
		delegate string GetExpectedWrapperNameDelegate(BusinessObject wrappedObject);
		void AssertHasWrappersFor(Constants.DataContext context, DocumentDirection direction, string wrapperName, params BusinessObject[] expected)
		{
			GetExpectedWrapperNameDelegate getWrapperNameDelegate = wrappedObject => wrapperName;
			AssertHasWrappersFor(context, direction, getWrapperNameDelegate, expected);
		}

		void AssertHasWrappersFor(Constants.DataContext context, DocumentDirection direction, GetExpectedWrapperNameDelegate wrapperName, params BusinessObject[] expected)
		{
			List<BusinessObject> wrapped = new List<BusinessObject>();
			DocumentWrapper[] wrapers = DocumentSupporter.GetDocumentWrappers(context, StmMenuItem.GetForTesting(direction)) ?? Array.Empty<DocumentWrapper>();
			foreach (DocumentWrapper wrapper in wrapers)
			{
				BusinessObject wrappedObject = (BusinessObject)wrapper.WrappedObject;
				AssertEquals(string.Format("Wrapper type name for '{0}'", wrappedObject), wrapperName(wrappedObject), wrapper.GetType().Name);
				wrapped.Add(wrappedObject);
			}

			AssertContainsExactElementsInAnyOrder(expected, wrapped);
		}

		AgencyBooking AgencyBooking
		{
			get
			{
				return agencyBooking ?? (agencyBooking = Factory.New<AgencyBooking>());
			}
		}

		AgencyBooking agencyBooking;
		AgencyShipmentDocumentSupporter DocumentSupporter
		{
			get
			{
				return documentSupporter ?? (documentSupporter = new AgencyShipmentDocumentSupporter(AgencyBooking));
			}
		}

		AgencyShipmentDocumentSupporter documentSupporter;
		#endregion
	}
}
