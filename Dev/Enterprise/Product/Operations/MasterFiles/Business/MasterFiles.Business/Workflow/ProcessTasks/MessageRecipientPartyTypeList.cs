using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class MessageRecipientPartyTypeList : ZBoolDescriptionPairList
	{
		public MessageRecipientPartyTypeList(MessageRecipientPartyType supportedTypes)
		{
			// If certain mappings aren't added before other mappings, stuff breaks. It's not worth the headache trying to alphabetecize. I blame the Coalition.
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Consignee, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Consignee));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Consignor, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Consignor));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Broker, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Broker));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ImportBroker, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ImportBroker));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ExportBroker, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ExportBroker));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.BillToParty, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.BillToParty));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PickupCartage, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PickupCartage));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.DeliveryCartage, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.DeliveryCartage));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.SendingAgent, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.SendingAgent));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ReceivingAgent, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ReceivingAgent));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ControllingAgent, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ControllingAgent));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ControllingCustomer, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ControllingCustomer));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.OrgProxy, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.OrgProxy));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Client, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Client));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Email, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Email));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Print, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Print));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.TransportCo, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.TransportCo));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Carrier, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Carrier));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.NotifyParty, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.NotifyParty));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.DeliveryToParty, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.DeliveryToParty));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PickupParty, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PickupParty));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.EDICommunication, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.EDICommunication));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.InvoiceDebtor, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.InvoiceDebtor));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.DepartureCFS, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.DepartureCFS));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ArrivalCFS, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ArrivalCFS));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Forwarder, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Forwarder));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ArrivalCarrier, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ArrivalCarrier));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.DepartureCarrier, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.DepartureCarrier));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Principal, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Principal));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.DepartureCTO, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.DepartureCTO));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ArrivalCTO, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ArrivalCTO));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.DepartureContainerYard, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.DepartureContainerYard));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ArrivalContainerYard, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ArrivalContainerYard));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.WarehouseInwards, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.WarehouseInwards));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.WarehouseOutwards, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.WarehouseOutwards));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.BondedWarehouseInwards, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.BondedWarehouseInwards));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.BondedWarehouseOutwards, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.BondedWarehouseOutwards));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.HVLVAirClearanceAgent, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.HVLVAirClearanceAgent));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.SGAccess, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.SGAccess));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.USAirAMS, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.USAirAMS));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ShippingManager, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ShippingManager));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.BookingParty, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.BookingParty));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.DeConsolidator, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.DeConsolidator));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.AirCargoResponsibleParty, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.AirCargoResponsibleParty));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PickupAgent, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PickupAgent));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.DeliveryAgent, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.DeliveryAgent));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.CartageAgent, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.CartageAgent));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.HVLVSeaClearanceAgent, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.HVLVSeaClearanceAgent));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.JapanCustomsAFR, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.JapanCustomsAFR));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ASYCUDA, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ASYCUDA));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Warehouse, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Warehouse));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.SeaCargoResponsibleParty, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.SeaCargoResponsibleParty));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.DepartureTransitWarehouse, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.DepartureTransitWarehouse));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.IndianCustomsEDISystem, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.IndianCustomsEDISystem));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PortForExportManifest, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PortForExportManifest));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PortForExportRelease, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PortForExportRelease));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PortForImportManifest, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PortForImportManifest));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ContainerYard, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ContainerYard));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.CTO, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.CTO));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PortForImportRelease, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PortForImportRelease));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.YardForExportRelease, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.YardForExportRelease));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.YardForImportPreArrival, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.YardForImportPreArrival));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.NettingSystem, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.WiseNettingSystem));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ArrivalTransitWarehouse, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ArrivalTransitWarehouse));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.BondedWhsChangeOfOwnership, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.BondedWhsChangeOfOwnership));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.BondedWhsChangeOfRegime, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.BondedWhsChangeOfRegime));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.AutoDocumentDelivery, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.AutoDocumentDelivery));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.LastCompletedTaskResource, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.LastCompletedTaskResource));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.JobLevelWorkflowGroup, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.JobLevelWorkflowGroup));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.NotificationGroup, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.NotificationGroup));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.CustomsOutturnAgent, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.CustomsOutturnAgent));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.CreditControlledDocumentApproval, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.CreditControlledDocumentApproval));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.CarrierBookingAgent, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.CarrierBookingAgent));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.AssignedStaff, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.AssignedStaff));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.Staff, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.Staff));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.RequiredCapabilityMembers, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.RequiredCapabilityMembers));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.AssignedGroupMembers, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.AssignedGroupMembers));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PersonalEmail, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PersonalEmail));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PersonPrimaryWorkEmail, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PersonPrimaryWorkEmail));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PersonalFallbackPrimaryWorkEmail, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PersonalFallbackPrimaryWorkEmail));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.NVOCC, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.NVOCC));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.CurrentUser, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.CurrentUser));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.GroupOwners, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.GroupOwners));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.ExternalBroker, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.ExternalBroker));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.HVLVForwarder, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.HVLVForwarder));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.CarrierMessagingDebtor, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.CarrierMessagingDebtor));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.FirstApprovalTask, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.FirstApprovalTask));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.OnBoardingEmail, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.OnBoardingEmail));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.TransportJobRegistry, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.TransportJobRegistry));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.GlobalTradeManagement, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.GlobalTradeManagement));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.GateManagement, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.GateManagement));
			AddPairMappingIfSupported(supportedTypes, MessageRecipientPartyType.PortForTransitManifest, AllPossiblePartyTypes.GetDescriptionFromCode(Codes.PortForTransitManifest));
		}

		public static CodeDescriptionPairList AllPossiblePartyTypes => allPossiblePartyTypes.Value;
		public static CodeDescriptionPairList OrganisationPartyTypes => organisationPartyTypes.Value;
		public static CodeDescriptionPairList SpecialPartyTypes => specialPartyTypes.Value;

		static readonly LazyOverridable<CodeDescriptionPairList> allPossiblePartyTypes = new LazyOverridable<CodeDescriptionPairList>(() => new AllPossiblePartyTypeList(AllPossiblePartyTypeList.PartyTypeListFilter.All));
		static readonly LazyOverridable<CodeDescriptionPairList> organisationPartyTypes = new LazyOverridable<CodeDescriptionPairList>(() => new AllPossiblePartyTypeList(AllPossiblePartyTypeList.PartyTypeListFilter.Organisation));
		static readonly LazyOverridable<CodeDescriptionPairList> specialPartyTypes = new LazyOverridable<CodeDescriptionPairList>(() => new AllPossiblePartyTypeList(AllPossiblePartyTypeList.PartyTypeListFilter.SpecialCommunicationMode));

		/// <summary>
		/// MUST keep this in sync with Enterprise.UniversalDataBuss.Integration.RecipientRoleType
		/// </summary>
		class AllPossiblePartyTypeList : CodeDescriptionPairList
		{
			internal enum PartyTypeListFilter
			{
				All = 0,
				Organisation,
				SpecialCommunicationMode,
			}

			internal AllPossiblePartyTypeList(PartyTypeListFilter partyTypeFilter)
			{
				switch (partyTypeFilter)
				{
					case PartyTypeListFilter.All:
						AddOrganisationPartyTypes();
						AddSpecialPartyTypes();
						break;

					case PartyTypeListFilter.Organisation:
						AddOrganisationPartyTypes();
						break;

					case PartyTypeListFilter.SpecialCommunicationMode:
						AddSpecialPartyTypes();
						break;

					default:
						throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Argument value {0}", partyTypeFilter), nameof(partyTypeFilter));
				}
			}

			void AddSpecialPartyTypes()
			{
				AddPair(Codes.Email, ResString.GetMultilingualString("1b3443d7-6318-4719-bca6-580b4de530e7", "Email"));
				AddPair(Codes.Print, ResString.GetMultilingualString("6925c8aa-37ed-4924-ba9a-9eb41c10165b", "Print"));
				AddPair(Codes.EDICommunication, ResString.GetMultilingualString("a735184d-1f96-4821-999f-26a5dd1dfa07", "EDI Communication"));
				AddPair(Codes.AutoDocumentDelivery, ResString.GetMultilingualString("A9BF8C12-436C-4F1F-B71C-F08E851CA5EF", "Auto-Deliver using Document Config"));
			}

			void AddOrganisationPartyTypes()
			{
				// Things belong in this list if they are recipient types representing an organisation. (As opposed to special recipient types that choose the medium of messages.)
				// It is likely that some of the items in this list are incorrectly classified. 
				AddPair(Codes.AirCargoResponsibleParty, Res.GetString("6791f0ed-c2f1-4370-967c-6852500ffc5a", "AirCargo Responsible Party"));
				AddPair(Codes.ArrivalCFS, ResString.GetMultilingualString("82c8f858-018c-443c-9ead-79c0f03322ca", "Arrival CFS"));
				AddPair(Codes.ArrivalCTO, ResString.GetMultilingualString("00fdb970-9eb8-4cb0-bf94-f69d674814d4", "Arrival CTO"));
				AddPair(Codes.ArrivalCarrier, ResString.GetMultilingualString("303d079d-2bff-46ad-93b6-69cc2fa26e81", "Arrival Carrier"));
				AddPair(Codes.ArrivalContainerYard, ResString.GetMultilingualString("3f3ebab6-bf51-437c-902c-13003fdc08f2", "Arrival Container Yard"));
				AddPair(Codes.AssignedGroupMembers, ResString.GetMultilingualString("9a1a7028-dba1-4381-8f7f-98fdf03bd499", "Assigned Group Members"));
				AddPair(Codes.AssignedStaff, ResString.GetMultilingualString("d54239bd-1ce1-417b-a1c7-5da1a6a602fc", "Assigned Staff"));
				AddPair(Codes.Staff, ResString.GetMultilingualString("fd1af73f-3410-4404-97ef-8035aa21163d", "Staff"));
				AddPair(Codes.BillToParty, ResString.GetMultilingualString("16b31e3e-8d69-4b5e-a38b-74b1c0678be3", "Bill to Party(s)"));
				AddPair(Codes.BookingParty, ResString.GetMultilingualString("727442ab-b502-4fad-9149-c2e4318c5e62", "Booking Party"));
				AddPair(Codes.Bolero, ResString.GetMultilingualString("E3BE8F06-2D5A-453E-BE22-CFD95768B72E", "Bolero"));
				AddPair(Codes.Broker, ResString.GetMultilingualString("57ce90e4-4c08-442a-b7de-f95e0ee3ff38", "Broker"));
				AddPair(Codes.CreditControlledDocumentApproval, ResString.GetMultilingualString("CAC9663F-5210-4F9F-935F-F81CB673D9DF", "Credit Controlled Document Approval"));
				AddPair(Codes.CACustomsIIDD4StatusNotice, ResString.GetMultilingualString("e5212bd1-1bc0-4122-ac74-7160a2baf1d1", "CA Customs IID/D4 Status Notice"));
				AddPair(Codes.Carrier, ResString.GetMultilingualString("62df651e-d46b-481f-9ab6-8542746ad876", "Carrier"));
				AddPair(Codes.CarrierBookingAgent, ResString.GetMultilingualString("b1967437-d5b3-4ae4-a10d-f68c35ab81b5", "Carrier Booking Agent"));
				AddPair(Codes.CartageAgent, ResString.GetMultilingualString("5956c3a7-b020-4447-9121-d72524e4306a", "Cartage Agent"));
				AddPair(Codes.Client, ResString.GetMultilingualString("03f67267-21ee-496b-93c7-8374637fb88a", "Client"));
				AddPair(Codes.Consignee, ResString.GetMultilingualString("5f8819fe-695f-4efd-86c4-0665faae4905", "Consignee"));
				AddPair(Codes.Consignor, ResString.GetMultilingualString("0742e331-5b08-4192-9be1-73dcaa5d5d19", "Consignor"));
				AddPair(Codes.CustomsOutturnAgent, ResString.GetMultilingualString("b8a9ec7e-5bd0-4496-84e7-8a459152fda3", "Customs Outturn Agent"));
				AddPair(Codes.ContainerYard, ResString.GetMultilingualString("54367e42-a639-4dcc-b8b5-07d4fa56da7d", "Container Yard"));
				AddPair(Codes.ControllingAgent, ResString.GetMultilingualString("11b9dbc8-62bb-44a3-b4dc-4ecf43c40a18", "Controlling Agent"));
				AddPair(Codes.ControllingCustomer, ResString.GetMultilingualString("1c0e73fe-6559-464f-aeb3-ac2109b1c43a", "Controlling Customer"));
				AddPair(Codes.CTO, ResString.GetMultilingualString("b28def69-5c7a-43c1-9557-8712c0105440", "Container Terminal Operator"));
				AddPair(Codes.CurrentUser, ResString.GetMultilingualString("9071404a-50a3-48af-9afe-c24f4599d73b", "Current User"));
				AddPair(Codes.DeConsolidator, Res.GetString("D8C85524-6E4E-4FDB-BD88-DFE0089A30DB", "Air Cargo De-Consolidator"));
				AddPair(Codes.DeliveryAgent, ResString.GetMultilingualString("32eafc4c-371f-4d72-8abe-e9ad8107efd6", "Delivery Agent"));
				AddPair(Codes.DeliveryCartage, ResString.GetMultilingualString("c7d3180a-c6f2-4db3-ba7d-e9d75eaf971b", "Delivery Transport"));
				AddPair(Codes.DeliveryToParty, ResString.GetMultilingualString("de1101ab-6186-41b9-a6d9-d0130e36414d", "Delivery to Party"));
				AddPair(Codes.DepartureCFS, ResString.GetMultilingualString("37d986ec-904a-43cb-be0d-262ea3e45f77", "Departure CFS"));
				AddPair(Codes.DepartureCTO, ResString.GetMultilingualString("a3aea6af-b125-4a1b-b413-79b73d181746", "Departure CTO"));
				AddPair(Codes.DepartureCarrier, ResString.GetMultilingualString("e93d1bad-0152-430c-80d8-a0e1e4f28904", "Departure Carrier"));
				AddPair(Codes.DepartureContainerYard, ResString.GetMultilingualString("55a35e01-2ea6-4565-8474-5046b02e6377", "Departure Container Yard"));
				AddPair(Codes.ExportBroker, Res.GetString("89aa895a-acae-4097-b684-838815ec0b4e", "Export Broker"));
				AddPair(Codes.Forwarder, ResString.GetMultilingualString("014f8d45-e748-46ed-8f7b-68eaf03460f3", "Forwarder"));
				AddPair(Codes.GroupOwners, ResString.GetMultilingualString("e19d57b0-5e9d-45bf-b6ed-55406710f813", "Group Owners"));
				AddPair(Codes.HVLVAirClearanceAgent, ResString.GetMultilingualString("a6c7f1be-9bfe-4346-84de-e9c921b11495", "HVLV Air Clearance Agent"));
				AddPair(Codes.HVLVSeaClearanceAgent, ResString.GetMultilingualString("9f9bd3d1-c5fe-4494-a86f-9335484de2e8", "HVLV Sea Cargo Clearance Agent"));
				AddPair(Codes.IndianCustomsEDISystem, ResString.GetMultilingualString("12345678-c5fe-4494-a86f-9335484de2e8", "Indian Customs EDI System"));
				AddPair(Codes.ImportBroker, Res.GetString("e4626ce8-b679-4b04-9744-ccc7c0a42cba", "Import Broker"));
				AddPair(Codes.InvoiceDebtor, ResString.GetMultilingualString("85d4f01a-1b79-480e-8c46-8cb5d1b75ce8", "Invoice Debtor"));
				AddPair(Codes.JapanCustomsAFR, ResString.GetMultilingualString("69174A81-5C3A-4B63-A4EF-2D1E2EB5E826", "Japan Customs Advance Filing Rules"));
				AddPair(Codes.LastCompletedTaskResource, ResString.GetMultilingualString("5a969818-ab81-460e-b3be-d2f7c11ea566", "Last Completed Task Resource"));
				AddPair(Codes.JobLevelWorkflowGroup, ResString.GetMultilingualString("1c54d873-4e62-4a21-9ffe-427e023a27df", "Job-level Workflow Group"));
				AddPair(Codes.NotificationGroup, ResString.GetMultilingualString("6feb122f-62d8-43b5-a9c3-f8b754b30b49", "Notification Group"));
				AddPair(Codes.NotifyParty, ResString.GetMultilingualString("b41755bc-0508-4530-b548-717442c24ec6", "Notify Party"));
				AddPair(Codes.NVOCC, ResString.GetMultilingualString("b75dfd4a-42db-4d2a-a0dc-ecdd44d5b1ee", "NVOCC"));
				AddPair(Codes.OrgProxy, ResString.GetMultilingualString("644bbbb7-790d-4f12-b8da-80158010d93f", "Organization Proxy"));
				AddPair(Codes.PickupAgent, ResString.GetMultilingualString("d68585aa-99c5-46be-8571-e32a5449aead", "Pickup Agent"));
				AddPair(Codes.PickupCartage, ResString.GetMultilingualString("408da16c-815a-4bb9-b7a5-0150b613764e", "Pickup Transport"));
				AddPair(Codes.PickupParty, ResString.GetMultilingualString("20c9a9c2-27d8-498f-a753-136780eef271", "Pickup Party"));
				AddPair(Codes.PortForExportManifest, ResString.GetMultilingualString("384a6057-5ca6-4d02-b7bb-366082ecf02b", "Port For Export Manifest"));
				AddPair(Codes.PortForExportRelease, ResString.GetMultilingualString("5db6859e-f265-487e-a8df-20e56047ead9", "Port For Export Release"));
				AddPair(Codes.PortForImportManifest, ResString.GetMultilingualString("6099a5bd-12c3-4cb7-9913-d51adfaae5d3", "Port For Import Manifest"));
				AddPair(Codes.PortForImportRelease, ResString.GetMultilingualString("50d30a5c-8dbe-4b9a-8859-2822117f5441", "Port For Import Release"));
				AddPair(Codes.Principal, ResString.GetMultilingualString("babe5569-d130-4cb2-adf3-a4af7a8393a5", "Principal"));
				AddPair(Codes.ReceivingAgent, ResString.GetMultilingualString("669e9650-d055-424a-82ea-bcf25207beba", "Receiving Agent"));
				AddPair(Codes.RequiredCapabilityMembers, ResString.GetMultilingualString("eb212fc5-e069-4313-90a1-52ff8d178447", "Required Capability Members"));
				AddPair(Codes.SendingAgent, ResString.GetMultilingualString("b20c2e30-a988-47c9-8989-c204cf0fcd31", "Sending Agent"));
				AddPair(Codes.SeaCargoResponsibleParty, ResString.GetMultilingualString("E855BD4E-E4E2-43BD-90B6-A0B949518C06", "Sea Cargo Responsible Party"));
				AddPair(Codes.SGAccess, ResString.GetMultilingualString("1BE4C84C-3D53-49CF-9A02-455F2A56B1A3", "SG Access"));
				AddPair(Codes.ShippingManager, ResString.GetMultilingualString("3DC53728-CC9E-4C95-826B-2E7EC65AF7C3", "Shipping Manager"));
				AddPair(Codes.DepartureTransitWarehouse, ResString.GetMultilingualString("37859ABD-45A1-46BA-9F3E-C8F8AF365C99", "Departure Transit Warehouse"));
				AddPair(Codes.ArrivalTransitWarehouse, ResString.GetMultilingualString("A483FA82-E864-4C26-82EC-013AFC0051E5", "Arrival Transit Warehouse"));
				AddPair(Codes.TransportCo, ResString.GetMultilingualString("61c75e68-bcfd-4000-a588-00230dfdba2d", "Transport Co"));
				AddPair(Codes.USAirAMS, ResString.GetMultilingualString("4DD67797-C0EA-4F45-AE23-F99B451D5357", "US Air AMS"));
				AddPair(Codes.WarehouseInwards, ResString.GetMultilingualString("4d2fd720-2c14-4ee9-bb59-14dad860570b", "Warehouse Inwards"));
				AddPair(Codes.WarehouseOutwards, ResString.GetMultilingualString("35e125a7-2a3e-4f80-b289-693dae18da6b", "Warehouse Outwards"));
				AddPair(Codes.WarehouseDynamicWorkOrder, ResString.GetMultilingualString("96d2a927-5a15-4039-94c9-9b613e0221c3", "Warehouse Dynamic Work Order"));
				AddPair(Codes.WarehouseWorkOrder, ResString.GetMultilingualString("0efe4390-804a-4087-8581-d218efd64933", "Warehouse Work Order"));
				AddPair(Codes.BondedWarehouseInwards, ResString.GetMultilingualString("b771d472-248c-4bd3-b465-1c96fd9156ac", "Bonded Warehouse Inwards"));
				AddPair(Codes.BondedWarehouseOutwards, ResString.GetMultilingualString("beea0a4c-8620-4008-9996-9bf7595139b1", "Bonded Warehouse Outwards"));
				AddPair(Codes.ASYCUDA, ResString.GetMultilingualString("12345678-5C3A-4B63-A4EF-2D1E2EB5E826", "ASYCUDA Manifest"));
				AddPair(Codes.Warehouse, ResString.GetMultilingualString("047FE17C-515A-4759-A17B-4EBC81F0F595", "Warehouse"));
				AddPair(Codes.YardForExportRelease, ResString.GetMultilingualString("694cfe6b-041c-486a-9df6-2904ba859cf6", "Yard For Export Release"));
				AddPair(Codes.YardForImportPreArrival, ResString.GetMultilingualString("cd689cfa-e929-4132-b421-96545178a01a", "Yard For Import Pre-Arrival"));
				AddPair(Codes.WiseNettingSystem, ResString.GetMultilingualString("c2dbd615-789b-4283-9973-81126a48e190", "Wise Netting System"));
				AddPair(Codes.BondedWhsChangeOfOwnership, ResString.GetMultilingualString("8FF421E9-4715-422E-80FF-86265C1BF2C8", "Bonded Warehouse Change of Ownership"));
				AddPair(Codes.BondedWhsChangeOfRegime, ResString.GetMultilingualString("E80D4F7A-8486-4046-B04E-6CA66036DBEB", "Bonded Warehouse Change of Regime"));
				AddPair(Codes.PersonalEmail, ResString.GetMultilingualString("551AD5B9-487D-49B6-9056-F069128A2FDD", "Personal Email"));
				AddPair(Codes.PersonPrimaryWorkEmail, ResString.GetMultilingualString("3DBF0930-2DD5-4961-B5C3-DEF04312AB8F", "Person Primary Work Email"));
				AddPair(Codes.PersonalFallbackPrimaryWorkEmail, ResString.GetMultilingualString("37275B2D-BA1B-4B0B-9FA4-F33B8D85CA46", "Personal, Fallback Primary Work Email"));
				AddPair(Codes.ExternalBroker, ResString.GetMultilingualString("D3ACB260-5429-4512-9356-9AB551EB45B6", "External Broker"));
				AddPair(Codes.HVLVForwarder, ResString.GetMultilingualString("d5f8e0ff-cdb7-49b3-a57e-6dea01cefeb5", "HVLV Forwarder"));
				AddPair(Codes.CarrierMessagingDebtor, ResString.GetMultilingualString("b177f76b-b066-4204-b078-b51c479455a6", "Carrier Messaging Debtor"));
				AddPair(Codes.FirstApprovalTask, ResString.GetMultilingualString("32B87FA1-7F41-46F2-960B-EA811CF9996E", "All Approval Tasks"));
				AddPair(Codes.OnBoardingEmail, ResString.GetMultilingualString("793A0F36-F935-47EF-88BE-5F7966B1E126", "On-boarding Email"));
				AddPair(Codes.TransportJobRegistry, ResString.GetMultilingualString("2FFD49BA-D543-486E-BFF3-3AC2F8A16DDE", "PT/TB Registry"));
				AddPair(Codes.GlobalTradeManagement, ResString.GetMultilingualString("F224988E-D4DC-4B43-ABC7-AE34282FF060", "Global Trade Management"));
				AddPair(Codes.GateManagement, ResString.GetMultilingualString("6c4723b0-d28f-6da5-4f50-a472321abb8d", "Gate Management"));
				AddPair(Codes.PortForTransitManifest, ResString.GetMultilingualString("8e9565cf-1bef-48c8-a7f3-3a3736a0c68b", "Port For Transit Manifest"));
			}
		}

		public static class SpecialCodes
		{
			public const string Other = "OTH";
		}

		public static class Codes
		{
			public const string AirCargoResponsibleParty = "ARP";
			public const string ArrivalCarrier = "ACR";
			public const string ArrivalCFS = "ACF";
			public const string ArrivalCTO = "ACT";
			public const string ArrivalContainerYard = "ACY";
			public const string ArrivalTransitWarehouse = "ATW";
			public const string AssignedGroupMembers = "GRP";
			public const string AssignedStaff = "STF";
			public const string ASYCUDA = "ASY";
			public const string AutoDocumentDelivery = "ADV";
			public const string BondedWhsChangeOfOwnership = "BCO";
			public const string BondedWhsChangeOfRegime = "BCR";
			public const string BillToParty = "BTP";
			public const string BookingParty = "BKP";
			public const string Bolero = "BOR";
			public const string Broker = "BRO";
			public const string CarrierMessagingDebtor = "CMD";
			public const string CreditControlledDocumentApproval = "CCA";
			public const string CACustomsIIDD4StatusNotice = "CD4";
			public const string Carrier = "CAR";
			public const string CarrierBookingAgent = "CBA";
			public const string CartageAgent = "CTG";
			public const string Client = "CLI";
			public const string Consignee = "CNE";
			public const string Consignor = "CNR";
			public const string CustomsOutturnAgent = "COA";
			public const string ContainerYard = "CYD";
			public const string ControllingAgent = "CTA";
			public const string ControllingCustomer = "CTP";
			public const string CTO = "CTO";
			public const string CurrentUser = "CUR";
			public const string DeConsolidator = "AAD";
			public const string DeliveryAgent = "DAG";
			public const string DeliveryCartage = "DCA";
			public const string DeliveryToParty = "DTP";
			public const string DepartureCarrier = "DCR";
			public const string DepartureCFS = "DCF";
			public const string DepartureContainerYard = "DCY";
			public const string DepartureCTO = "DCT";
			public const string DepartureTransitWarehouse = "DTW";
			public const string EDICommunication = "EDI";
			public const string Email = "EML";
			public const string ExportBroker = "BRE";
			public const string Forwarder = "FOR";
			public const string FirstApprovalTask = "FAT";
			public const string GroupOwners = "OWN";
			public const string HVLVAirClearanceAgent = "HCA";
			public const string HVLVSeaClearanceAgent = "HSA";
			public const string IndianCustomsEDISystem = "ICE";
			public const string ImportBroker = "BRI";
			public const string InvoiceDebtor = "IDB";
			public const string JapanCustomsAFR = "AFR";
			public const string LastCompletedTaskResource = "LCT";
			public const string JobLevelWorkflowGroup = "JWG";
			public const string NotificationGroup = "NGP";
			public const string NotifyParty = "NFP";
			public const string NVOCC = "NVO";
			public const string OrgProxy = "ORP";
			public const string OnBoardingEmail = "OBE";
			public const string PickupAgent = "PAG";
			public const string PickupCartage = "PCA";
			public const string PickupParty = "PUP";
			public const string PortForExportManifest = "PEM";
			public const string PortForExportRelease = "PER";
			public const string PortForImportManifest = "PIM";
			public const string PortForImportRelease = "PIR";
			public const string Principal = "PRC";
			public const string Print = "PRN";
			public const string ReceivingAgent = "RAG";
			public const string RequiredCapabilityMembers = "CAP";
			public const string SeaCargoResponsibleParty = "SRP";
			public const string SendingAgent = "SAG";
			public const string SGAccess = "SGA";
			public const string ShippingManager = "SPM";
			public const string Staff = "STA";
			public const string TransportCo = "TPC";
			public const string USAirAMS = "UAM";
			public const string Warehouse = "WHS";
			public const string WarehouseInwards = "WIN";
			public const string WarehouseOutwards = "WAR";
			public const string WarehouseDynamicWorkOrder = "WDO";
			public const string WarehouseWorkOrder = "WWO";
			public const string BondedWarehouseInwards = "BWI";
			public const string BondedWarehouseOutwards = "BWR";
			public const string YardForExportRelease = "YER";
			public const string YardForImportPreArrival = "YIA";
			public const string WiseNettingSystem = "WNS";
			public const string PersonalEmail = "PSE";
			public const string PersonPrimaryWorkEmail = "PWK";
			public const string PersonalFallbackPrimaryWorkEmail = "PPW";
			public const string ExternalBroker = "BRX";
			public const string HVLVForwarder = "HVL";
			public const string TransportJobRegistry = "DEF";
			public const string GlobalTradeManagement = "GTM";
			public const string GateManagement = "GDM";
			public const string PortForTransitManifest = "PTM";
		}

		public static CodeDescriptionPairList GetAlternateMessageRecipientPartyTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AlternateMessageRecipientPartyTypeList", () => new CodeDescriptionPairList(MessageRecipientPartyTypeList.OrganisationPartyTypes));
		}

		public bool IsMatch(MessageRecipientPartyType partyType)
		{
			ZBoolDescriptionPair pair = FindByPartyType(partyType);
			return pair != null && pair.Value;
		}

		public ZBoolDescriptionPair FindByPartyType(MessageRecipientPartyType partyType)
		{
			foreach (ZBoolDescriptionPair next in this)
			{
				PartyTypeDescriptionPair enumPair = next as PartyTypeDescriptionPair;
				if (enumPair != null && enumPair.PartyType == partyType)
				{
					return next;
				}
			}
			return null;
		}

		public MessageRecipientPartyType PartyTypes
		{
			get
			{
				MessageRecipientPartyType result = MessageRecipientPartyType.None;
				foreach (ZBoolDescriptionPair pair in this)
				{
					PartyTypeDescriptionPair enumPair = pair as PartyTypeDescriptionPair;
					if (enumPair != null && enumPair.Value)
					{
						result = result | enumPair.PartyType;
					}
				}
				return result;
			}
			set
			{
				foreach (ZBoolDescriptionPair pair in this)
				{
					PartyTypeDescriptionPair enumPair = pair as PartyTypeDescriptionPair;
					if (enumPair != null)
					{
						bool newValue = (value & enumPair.PartyType) == enumPair.PartyType;
						if (enumPair.Value != newValue)
						{
							enumPair.Value = newValue;
						}
					}
				}
			}
		}

		public event EventHandler PartyTypesChanged;

		#region Implementation

		void AddPairMappingIfSupported(MessageRecipientPartyType supportedTypes, MessageRecipientPartyType value, string description)
		{
			if ((supportedTypes & value) == value)
			{
				PartyTypeDescriptionPair pair = new PartyTypeDescriptionPair(value, description, (PartyTypes & value) == value);
				pair.OnChanged += delegate
				{ OnPartyTypesChanged(EventArgs.Empty); };
				Add(pair);
			}
		}

		void OnPartyTypesChanged(EventArgs e)
		{
			if (PartyTypesChanged != null)
			{
				PartyTypesChanged(this, e);
			}
		}
		#endregion
	}

	public class AllPossibleRecipientTypesGetter : IAllPossibleRecipientTypesGetter
	{
		CodeDescriptionPairList IAllPossibleRecipientTypesGetter.GetRecipientList()
		{
			return MessageRecipientPartyTypeList.AllPossiblePartyTypes;
		}

		CodeDescriptionPairList IAllPossibleRecipientTypesGetter.GetListOfRecipientsWhichCannotReceiveUniversalXml()
		{
			var list = new CodeDescriptionPairList();

			foreach (ICodeDescription item in MessageRecipientPartyTypeList.AllPossiblePartyTypes)
			{
				switch (item.Code)
				{
					case MessageRecipientPartyTypeList.Codes.AutoDocumentDelivery:
					case MessageRecipientPartyTypeList.Codes.CurrentUser:
					case MessageRecipientPartyTypeList.Codes.EDICommunication:
					case MessageRecipientPartyTypeList.Codes.Email:
					case MessageRecipientPartyTypeList.Codes.GroupOwners:
					case MessageRecipientPartyTypeList.Codes.PersonalEmail:
					case MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail:
					case MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail:
					case MessageRecipientPartyTypeList.Codes.Print:
					case MessageRecipientPartyTypeList.Codes.FirstApprovalTask:
					case MessageRecipientPartyTypeList.Codes.OnBoardingEmail:
					case MessageRecipientPartyTypeList.Codes.Staff:
						list.Add(item);
						break;
				}
			}

			return list;
		}
	}

	public class PartyTypeDescriptionPair : ZBoolDescriptionPair
	{
		public PartyTypeDescriptionPair(MessageRecipientPartyType partyType, string description, bool value)
			: base(description, value)
		{
			this.partyType = partyType;
		}

		public MessageRecipientPartyType PartyType
		{
			get { return partyType; }
			set { partyType = value; }
		}
		MessageRecipientPartyType partyType;

		public string Code
		{
			get
			{
				return MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetCodeFromDescription(Description);
			}
		}
	}
}
