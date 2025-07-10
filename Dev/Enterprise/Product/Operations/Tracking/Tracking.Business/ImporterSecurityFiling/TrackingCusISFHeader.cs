using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataTransfer;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business.ImporterSecurityFiling
{
	public class TrackingCusISFHeader : CusISFHeader,
		IBizOChangesEmailNotification,
		IWebDocumentsWithUploadSupport,
		IWebUserEditableNoteSupport,
		IAccessControlled,
		Integration.Customs.US.ISF.ITrackingCusISFHeader,
		IUpdatableMilestoneEventsProvider,
		IMilestonesProvider,
		ITrackingEventsProvider,
		IEventReferenceProvider
	{
		public abstract new class Schema : CusISFHeader.Schema
		{
			public const string BondTypeDescription = "BondTypeDescription";
			public const string BondActivityCodeDescription = "BondActivityCodeDescription";
			public const string ShipmentSubTypeDescription = "ShipmentSubTypeDescription";
			public const string ActionReasonCodeDescription = "ActionReasonCodeDescription";
			public const string TransportModeCodeDescription = "TransportModeCodeDescription";
		}

		public TrackingCusISFHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (SiteUser != null)
			{
				Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
			}
		}

		public TrackingCusISFHeader Duplicate()
		{
			TrackingCusISFHeader result = ((ITemplateCopyable)this).TemplateCopy() as TrackingCusISFHeader;
			foreach (var line in result.Lines)
			{
				line.HasChanges = true;
			}
			foreach (var address in result.DocAddresses)
			{
				address.HasChanges = true;
			}
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (SiteUserOrganisation != null)
			{
				if (SiteUserOrganisation.OH_IsConsignee)
				{
					this.BF_OH_Importer = SiteUser.CurrentOrg;
				}
			}
		}

		#region DocAddresses

		public List<ISFDocAddress> ManufacturerAddressesList
		{
			get
			{
				var result = new List<ISFDocAddress>();
				result.AddRange(ManufacturerAddresses);
				return result;
			}
		}

		public ISFDocAddressesExcludeManufacturerCollection DocAddressesExcludeManufacturer
		{
			get
			{
				if (fAddresses == null)
				{
					fAddresses = new ISFDocAddressesExcludeManufacturerCollection(this);
					fAddresses.ReBuild();
				}

				return fAddresses;
			}
		}
		ISFDocAddressesExcludeManufacturerCollection fAddresses;

		#endregion

		#region Properties

		public string XmlData
		{
			get
			{
				ImporterSecurityFilingDataAdapter dataAdapter = new ImporterSecurityFilingDataAdapter();
				NotificationBuffer notify = new NotificationBuffer();
				TextWriter result = new StringWriter();
				XmlValueObjectSerializer xmlSerialiser = new XmlValueObjectSerializer(typeof(DataTransfer.Xml.XsdVersion1.XmlInterchange));
				xmlSerialiser.Serialize(result, dataAdapter.ToXmlInterchange(this, new ValueObjectExportContext(notify)));
				return result.ToString();
			}
		}

		#region TrackingEvents

		public StmALogCollection TrackingEvents
		{
			get { return this.GetTrackingEvents(SiteUser); }
		}

		public bool CanViewTrackingEvents
		{
			get { return SiteUser?.CanViewEvents ?? false; }
		}

		#endregion

		#region Milestones

		public void ReloadMilestones()
		{
			fMilestones = null;
		}

		public TrackingMilestoneCollection Milestones
		{
			get { return fMilestones ?? (fMilestones = new TrackingMilestoneCollection(this)); }
		}
		TrackingMilestoneCollection fMilestones;

		public TrackingMilestoneCollection EditableMilestones
		{
			get { return editableMilestones ?? (editableMilestones = new TrackingMilestoneCollection(this, true)); }
		}
		TrackingMilestoneCollection editableMilestones;

		#endregion

		#region IUpdatableMilestoneEventsProvider

		public List<string> UpdatableMilestoneEventCodes
		{
			get
			{
				if (SiteUser != null)
				{
					return (new UpdateableMilestoneEventsHelper(SiteUser)).GetUpdateableMilestoneEvents(WebDataRegistry.Instance.ISFMilestoneEventUpdates.Value, WebParties);
				}
				return new List<string>();
			}
		}

		#endregion

		#region IEventReferenceProvider

		public string EventReference
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
				{
					return (new EventReferenceHelper(WebEnv.AppInstance.SiteUser as TrackingSiteUser)).GetEventReferences(WebParties);
				}
				return string.Empty;
			}
		}

		#endregion

		#region WebParties

		WebPartyTypeOrgPairCollection WebParties
		{
			get
			{
				if (webParties == null)
				{
					webParties = new WebPartyTypeOrgPairCollection();

					webParties.Add(WebPartyType.Importer, Importer);
					webParties.Add(WebPartyType.ShipToLocation, MainShipToParty);
					webParties.Add(WebPartyType.BuyingParty, BuyingParty);
					webParties.Add(WebPartyType.SellingParty, SellingParty);
					webParties.Add(WebPartyType.StuffingLocation, StuffingLocation);
					webParties.Add(WebPartyType.Consolidator, Consolidator);
					foreach (JobDocAddress manufacturerAddress in ManufacturerAddresses)
					{
						webParties.Add(WebPartyType.Manufacturer, manufacturerAddress);
					}
					if (IsSendingAgent(SiteUser.LoggedInOrganisation.PK))
					{
						webParties.Add(WebPartyType.SendingAgent, SiteUser.LoggedInOrganisation);
					}
					webParties.Add(WebPartyType.BookingParty, BookingParty);
				}

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		#endregion

		public CodeDescriptionPairList SupportedDocAddressesList
		{
			get
			{
				if (fSupportedDocAddressesList == null)
				{
					fSupportedDocAddressesList = new CodeDescriptionPairList();
					foreach (DocAddressType addressType in ((IDocAddresses)this).SupportedAddressTypes)
					{
						if (addressType != DocAddressType.Manufacturer)
						{
							fSupportedDocAddressesList.Add(DocAddressTypes.GetPair(Factory, addressType));
						}
					}
				}
				return fSupportedDocAddressesList;
			}
		}
		CodeDescriptionPairList fSupportedDocAddressesList;

		#region Descriptions For Binding

		public ZString TransportModeCodeDescription
		{
			get { return Lookups.TransportModes.GetDescriptionFromCode(BF_TransportMode) ?? ZString.Empty; }
		}

		public ZPropertyInfo TransportModeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.TransportModeCodeDescription); }
		}

		public ZString BondTypeDescription
		{
			get { return Lookups.BondTypeList.GetDescriptionFromCode(BF_BondType) ?? ZString.Empty; }
		}

		public ZPropertyInfo BondTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BondTypeDescription); }
		}

		public ZString BondActivityCodeDescription
		{
			get { return Lookups.BondActivityCodeList.GetDescriptionFromCode(BF_BondActivityCode) ?? ZString.Empty; }
		}

		public ZPropertyInfo BondActivityCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BondActivityCodeDescription); }
		}

		public ZString ShipmentSubTypeDescription
		{
			get { return Lookups.ShipmentSubTypeList.GetDescriptionFromCode(BF_ShipmentSubType) ?? ZString.Empty; }
		}

		public ZPropertyInfo ShipmentSubTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ShipmentSubTypeDescription); }
		}

		public ZString ActionReasonCodeDescription
		{
			get { return Lookups.ActionReasonCodeList.GetDescriptionFromCode(BF_ActionReasonCode) ?? ZString.Empty; }
		}

		public ZPropertyInfo ActionReasonCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ActionReasonCodeDescription); }
		}

		#endregion

		#region Transport

		public TrackingCusISFBillCollection BillNumbersReferences
		{
			get
			{
				if (fBillNumbersReferences == null)
				{
					fBillNumbersReferences = new TrackingCusISFBillCollection(this);
				}
				return fBillNumbersReferences;
			}
		}
		TrackingCusISFBillCollection fBillNumbersReferences;

		public bool HasTransportsInfoForWeb
		{
			get
			{
				var firstTransport = FirstTransport;
				if (firstTransport.IsDeleted)
				{
					return false;
				}

				return !(firstTransport.JW_ETD.IsEmpty &&
						firstTransport.JW_ETA.IsEmpty &&
						firstTransport.JW_Vessel.IsEmpty &&
						firstTransport.JW_VoyageFlight.IsEmpty &&
						firstTransport.JW_RL_NKLoadPort.IsEmpty &&
						firstTransport.JW_RL_NKDiscPort.IsEmpty);
			}
		}

		public Transport FirstTransport
		{
			get
			{
				if (fFirstTransport == null)
				{
					if (Transports.Count > 0)
					{
						fFirstTransport = Transports[0];
					}
					else
					{
						fFirstTransport = Transports.AddNew();
						fFirstTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
					}
				}
				return fFirstTransport;
			}
		}
		Transport fFirstTransport;

		#endregion

		#endregion

		#region Implementation

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (Transports.Count > 0 && !FirstTransport.IsDeleted && !HasTransportsInfoForWeb && !FirstTransport.IsInDatabase)
			{
				Transports.RemoveAndDelete(FirstTransport);
			}
		}

		public OrgHeader SiteUserOrganisation
		{
			get
			{
				if (SiteUser != null && fSUOrg == null)
				{
					fSUOrg = Factory.Load<OrgHeader>(SiteUser.CurrentOrg);
				}
				return fSUOrg;
			}
		}
		OrgHeader fSUOrg;

		public TrackingSiteUser SiteUser
		{
			get
			{
				if (fSiteUser == null)
				{
					if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null)
					{
						fSiteUser = (WebEnv.AppInstance.SiteUser as TrackingSiteUser);
					}
				}
				return fSiteUser;
			}
			set
			{
				fSiteUser = value;

				if (SiteUser != null &&
					!(Logs.AutoCreatedLogDefaultSL_Reference == SiteUser.ContactAndCompanyReference))
				{
					Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
				}
			}
		}
		TrackingSiteUser fSiteUser;

		public static TrackingCusISFHeader GetFromISFHeaderPK(BusinessObjectFactory factory, ZGuid iSFHeaderPK, TrackingSiteUser siteUser)
		{
			TrackingCusISFHeader result = null;
			if (siteUser != null && siteUser.LoggedInOrganisation != null)
			{
				result = factory.Load<TrackingCusISFHeader>(iSFHeaderPK);
			}
			return result;
		}

		public ZGuid ISFHeaderPK
		{
			get { return this.PK; }
		}

		public ZGuid CurrentOrg
		{
			get { return SiteUser != null && SiteUser.IsLoggedIn ? SiteUser.LoggedInOrganisation.PK : ZGuid.Empty; }
		}

		#endregion

		#region Interfaces

		#region IEmailNotification Members

		ZString IBizOChangesEmailNotification.Number
		{
			get { return BF_JobReference; }
		}

		ZBool IBizOChangesEmailNotification.IsCancelled
		{
			get { return false; }
		}

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get
			{
				GlbBranch result = Factory.Load<GlbBranch>(this.BF_GB);
				return result ?? GlbBranch.FindControllingBranchWithFallBackToAnyCompany(((IBizOChangesEmailNotification)this).RelatedOrg);
			}
		}

		GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.ISFNotificationEmailGroup; }
		}

		ControllerID IBizOChangesEmailNotification.ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.ImporterSecurityFiling; }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get
			{
				return this.Importer;
			}
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			ZString staffNK = staffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsLookups.ContainerYardServices);
			GlbStaff staff = staffAssignments.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
			if (staff != null)
			{
				return staff.PK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.ISFNotificationStaffRoles; }
		}

		CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.ISFNotificationOptions; }
		}

		#region Email Reporting

		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("0356996c-2c60-4af1-a4a3-2bc990032d41", "Job Reference"), BF_JobReference);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("012aec56-0bf8-4e0d-99ed-8c78a91fc87e", "Customs Reference"), BF_CustomsReference);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("94541961-7110-4319-911a-f158cbc6a355", "Entry Type"), BF_EntryType);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("46c2d45d-772a-415e-a0f7-7b9f7f41dda7", "Shipment Type"), BF_ShipmentType);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("8a4ee31a-0611-40b3-bbfb-943a449bfc55", "Transport Mode"), BF_TransportMode);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("17798342-e6e5-48ac-9b7f-9089e156b713", "Carrier SCAC"), BF_SCAC);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("9849186b-9127-49eb-b632-8c2ae374cf34", "Importer"), Importer == null ? ZString.Empty : Importer.OH_FullNameTruncated);

			if (BF_EntryType == SubmissionTypeList.Codes.ISF10)
			{
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("77d48a24-1e44-4126-a6c4-e784f8e8f433", "Importer ID Type"), BF_ImporterCodeType);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("0dfa1b3c-9997-410c-9fc7-220faae31e24", "Importer ID"), BF_ImporterCode);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("d4875b42-412e-4bb0-be14-1cbe42260eb0", "DOB"), (ZString)BF_DateOfBirth.ToShortDateString());
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("155cbcff-34ce-462c-97af-bb190778bbef", "Country/Region of Issue"), BF_CountryOfIssue);

				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("2737b41f-3167-4bfd-aedd-bfcd3543a309", "Consignee ID Type"), BF_ConsigneeCodeType);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("42f914da-de8e-4f98-ad31-92bf87d02179", "Consignee ID"), BF_ConsigneeCode);

				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("5e4aafeb-473a-4fdd-8cea-b8401d9b3954", "Bond Holder"), BF_BondNumberOrHolder);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("1cf99f6f-2d80-42e8-b84e-6a159b014bac", "Surety Code"), BF_SuretyCode);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("efea5301-7ba4-41ce-a12b-e244dfe1090b", "Entry Number"), BF_EntryNumber);
			}
			if (BF_EntryType == SubmissionTypeList.Codes.ISF5)
			{
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("498dedb9-9de7-42e9-9aaa-206de4351332", "Unload Port"), BF_RL_NKPortOfUnload);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("58d80713-63e7-406e-8c02-11b37abe39fb", "Delivery Port"), BF_RL_NKPlaceOfDelivery);
			}
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("6ec5af12-0f2c-4048-bc89-eeb017e4c7d7", "Ship To Party"), MainShipToParty.E2_CompanyNameTruncated);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("8fa86bb4-e621-48ab-8291-2b6261460fbf", "Buying Party"), BuyingParty.E2_CompanyNameTruncated);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("2f21ef71-e992-421a-b33c-6f697a71e65a", "Selling Party"), SellingParty.E2_CompanyNameTruncated);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("8e1705f5-6990-4e81-bd01-8771de1b1d2c", "Stuffing Location"), StuffingLocation.E2_CompanyNameTruncated);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("74b281c6-a948-4c54-983b-4e8290a11e1a", "Consolidator"), Consolidator.E2_CompanyNameTruncated);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("26faaa79-a708-463a-a1af-c46611dee597", "Booking Party"), BookingParty.E2_CompanyNameTruncated);

			GenerateReferencesForEmailReporting(state, ReferenceDatas);
			GenerateLinesForReporting(state, Lines);
			GenerateContainersForReporting(state, Equipments);
			GenerateAddressForEmailReporting(state, DocAddresses);
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
		}

		bool IBizOChangesEmailNotification.HasChanges
		{
			get { return this.HasChanges; }
		}

		#region Collections For Reporting

		#region References

		void GenerateReferencesForEmailReporting(DataState state, ActiveBusinessObjectCollection<CusISFBill> aBOCollection)
		{
			int i = 0;
			foreach (CusISFBill @ref in aBOCollection)
			{
				var value = GenerateReferencesDetailsForEmailReporting(@ref);
				MultilingualString propertyName;
				switch (@ref.BB_BillType)
				{
					case "BM":
						propertyName = ResString.GetMultilingualString("c4c7f0b9-2547-490d-b789-41033a61614d", "House Bill of Lading");
						break;
					case "MB":
						propertyName = ResString.GetMultilingualString("4bbf119c-cac0-4480-a67c-b303089364b4", "Master Bill of Lading");
						break;
					case "OB":
						propertyName = ResString.GetMultilingualString("c174cad5-f97e-4799-bc25-43b5db6961bb", "Ocean Bill of Lading");
						break;
					default:
						propertyName = ResString.GetMultilingualString("a92b67b4-73de-4cb7-bd3f-d48362920d8b", "Reference Data#{0}", i);
						break;
				}
				PropertiesForEmailReporting.Add(state, propertyName, value);
				i++;
			}
		}

#if DEBUG
		public
#endif
 MultilingualString GenerateReferencesDetailsForEmailReporting(CusISFBill @ref)
		{
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("bc4693e4-ee14-4284-aace-0acac5c5c9bd", "Type: {0}", @ref.BB_BillTypeDescription),
				ResString.GetMultilingualString("473c3ec9-3a12-47d9-bf26-b7fabf15d5aa", "No: {0}", @ref.BB_BillNum),
				ResString.GetMultilingualString("f7c68e05-5a50-46d6-8b7d-22fc283185ce", "Bill Status: {0}", @ref.BB_CustomsStatus));
		}

		#endregion

		#region Containers

		void GenerateContainersForReporting(DataState state, CusISFEquipCollection containers)
		{
			int i = 0;
			foreach (CusISFEquip container in containers)
			{
				var value = GenerateContinerDetailsForEmailReporting(container);
				var propertyName = ResString.GetMultilingualString("a7f7ec49-3d07-4e23-9bfc-013076d84b2d", "Equipment #{0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

#if DEBUG
		public
#endif
 MultilingualString GenerateContinerDetailsForEmailReporting(CusISFEquip container)
		{
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("bf03b364-db33-454d-b8ae-4c03dd87ed4c", "Code: {0}", container.BE_EquipCode),
				ResString.GetMultilingualString("520b85af-f867-4197-a0e0-34a8b24a88ba", "Container No: {0}", container.BE_ContainerNum),
				ResString.GetMultilingualString("b07643ba-d9ce-49f8-ba5c-031ee340178d", "ISO Code: {0}", container.BE_ContainerISO));
		}

		#endregion

		#region Lines

		void GenerateLinesForReporting(DataState state, CusISFLineCollection lines)
		{
			int i = 0;
			foreach (CusISFLine line in lines)
			{
				var value = GenerateLineDetailsForEmailReporting(line);
				var propertyName = ResString.GetMultilingualString("e890ac1e-a0ee-45b0-ab7f-0276716756f4", "Line {0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

#if DEBUG
		public
#endif
 MultilingualString GenerateLineDetailsForEmailReporting(CusISFLine line)
		{
			ZString manufacturer = line.ManufacturerDocAddress == null ? ZString.Empty : line.ManufacturerDocAddress.E2_CompanyNameTruncated;
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("4829155c-89a1-4b80-919f-5c7763c41c99", "Product: {0}", line.BL_TextProductCode),
				ResString.GetMultilingualString("65de2db4-e848-481b-acc9-18a98994b32b", "Origin: {0}", line.BL_RN_NKGoodsOrigin),
				ResString.GetMultilingualString("30122bb1-ec1b-4b87-a5a9-f10c5d58ae10", "Tariff: {0}", line.BL_FormattedHarmonisedNum),
				ResString.GetMultilingualString("77c2613e-7755-42a0-adbf-6642366f1f11", "Manufacturer: {0}", manufacturer));
		}

		#endregion

		#region Addresses

		void GenerateAddressForEmailReporting(DataState state, JobDocAddressDependentCollection addresses)
		{
			foreach (JobDocAddress jDAddress in addresses)
			{
				if (!jDAddress.E2_CompanyName.IsEmpty ||
					!jDAddress.E2_Address1.IsEmpty ||
					!jDAddress.E2_Address2.IsEmpty ||
					!jDAddress.E2_Postcode.IsEmpty ||
					!jDAddress.E2_City.IsEmpty ||
					!jDAddress.E2_State.IsEmpty ||
					!jDAddress.E2_RN_NKCountryCode.IsEmpty)
				{
					var value = GenerateAddressDetailsForEmailReporting(jDAddress);
					var propertyName = (NoResString)jDAddress.AddressDescription;

					PropertiesForEmailReporting.Add(state, propertyName, value);
				}
			}
		}

		MultilingualString GenerateAddressDetailsForEmailReporting(JobDocAddress jDAddress)
		{
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("ed76bc06-34c8-4f60-9cf3-d0ff4570a1a2", "Company Name: {0}", jDAddress.E2_CompanyNameTruncated),
				ResString.GetMultilingualString("4c0a8ec6-8b9e-4177-b7ce-38824968804e", "Address Line 1: {0}", jDAddress.E2_Address1),
				ResString.GetMultilingualString("349c044e-3e97-4fc4-b53e-bd830cdfc5b3", "Address Line 2: {0}", jDAddress.E2_Address2),
				ResString.GetMultilingualString("3ec0a29c-034f-4e2b-8b26-d12a51331cee", "Post: {0}", jDAddress.E2_Postcode),
				ResString.GetMultilingualString("3727ac95-b113-4527-a463-57d7f317fb43", "City: {0}", jDAddress.E2_City),
				ResString.GetMultilingualString("caeb9ca3-6463-4839-a2df-5c0e9e26ad75", "State: {0}", jDAddress.E2_State),
				ResString.GetMultilingualString("f25428bc-85c6-445f-8d83-05ffdf39e5d9", "Country/Region: {0}", jDAddress.E2_RN_NKCountryCode));
		}

		#endregion

		#endregion

		PropertyChangeInfo[] IBizOChangesEmailNotification.GetPropertiesForEmailReporting()
		{
			return PropertiesForEmailReporting.GetValuesAsArray();
		}

		PropertyChangeInfoCollection PropertiesForEmailReporting
		{
			get
			{
				if (fPropertiesForEmailReporting == null)
				{
					fPropertiesForEmailReporting = new PropertyChangeInfoCollection();
				}

				return fPropertiesForEmailReporting;
			}
		}
		PropertyChangeInfoCollection fPropertiesForEmailReporting;

		#endregion

		#endregion

		#region IWebUserEditableNoteSupport Members

		public IStmNoteParent NotesParentBO
		{
			get { return this; }
		}

		public WebUserEditableNote UserEditableNoteHelper
		{
			get
			{
				if (fUserEditableNoteHelper == null)
				{
					fUserEditableNoteHelper = GetNewUserEditableNoteHelper();
				}
				return fUserEditableNoteHelper;
			}
		}
		WebUserEditableNote fUserEditableNoteHelper;

		protected WebUserEditableNote GetNewUserEditableNoteHelper()
		{
			return new WebUserEditableNote(this, PredefinedNoteTypes.Instance.SpecialInstructions);
		}

		#endregion

		#region IWebDocumentsSupport Members

		public ZGuid DocParentPK
		{
			get { return PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get { return new List<ZGuid>(); }
		}

		public OrgContact LoggedInContact
		{
			get { return SiteUser != null ? SiteUser.LoggedInUser : null; }
		}

		public DocumentSupport DocumentHelper
		{
			get
			{
				return fDocumentHelper ?? (fDocumentHelper = new DocumentSupport(this));
			}
		}
		DocumentSupport fDocumentHelper;

		#endregion

		#endregion Interfaces

		#region IAccessControlled Members

		bool IAccessControlled.IsInTextSuppressionMode
		{
			get { return isInSuppressionMode; }
			set { isInSuppressionMode = value; }
		}
		bool isInSuppressionMode = true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property name")]
		string IAccessControlled.GetRegistryCaption(string boundPropertyName)
		{
			if (boundPropertyName == CusISFHeaderSchema.BF_EntryType.Name ||
				boundPropertyName == CusISFHeaderSchema.BF_ShipmentType.Name ||
				boundPropertyName == CusISFHeaderSchema.BF_TransportMode.Name ||
				boundPropertyName == CusISFHeaderSchema.BF_SCAC.Name)
			{
				return ISFAccessRules.Captions.ISF_Details;
			}

			if (boundPropertyName == CusISFHeaderSchema.BF_ImporterCodeType.Name ||
				boundPropertyName == CusISFHeaderSchema.BF_ImporterCode.Name ||
				boundPropertyName == CusISFHeaderSchema.BF_DateOfBirth.Name ||
				boundPropertyName == CusISFHeaderSchema.BF_CountryOfIssue.Name)
			{
				return ISFAccessRules.Captions.Importer_Details;
			}

			if (boundPropertyName == CusISFHeaderSchema.BF_ConsigneeCodeType.Name ||
				boundPropertyName == CusISFHeaderSchema.BF_ConsigneeCode.Name)
			{
				return ISFAccessRules.Captions.Consignee_Details;
			}

			if (boundPropertyName == CusISFHeaderSchema.BF_BondNumberOrHolder.Name ||
				boundPropertyName == BF_SuretyCodeInfo.Name ||
				boundPropertyName == BF_EntryNumberInfo.Name)
			{
				return ISFAccessRules.Captions.Bond;
			}

			ZBindToChecker.CheckBindTo(((TrackingCusISFHeader)null).MainShipToParty);
			if (boundPropertyName == "MainShipToParty")
			{
				return ISFAccessRules.Captions.Ship_To_Parties;
			}

			ZBindToChecker.CheckBindTo(((TrackingCusISFHeader)null).BuyingParty);
			if (boundPropertyName == "BuyingParty")
			{
				return ISFAccessRules.Captions.Buying_Party;
			}

			ZBindToChecker.CheckBindTo(((TrackingCusISFHeader)null).SellingParty);
			if (boundPropertyName == "SellingParty")
			{
				return ISFAccessRules.Captions.Selling_Party;
			}

			ZBindToChecker.CheckBindTo(((TrackingCusISFHeader)null).StuffingLocation);
			if (boundPropertyName == "StuffingLocation")
			{
				return ISFAccessRules.Captions.Stuffing_Location;
			}

			ZBindToChecker.CheckBindTo(((TrackingCusISFHeader)null).Consolidator);
			if (boundPropertyName == "Consolidator")
			{
				return ISFAccessRules.Captions.Consolidator;
			}

			ZBindToChecker.CheckBindTo(((TrackingCusISFHeader)null).ManufacturerAddresses);
			if (boundPropertyName == "ManufacturerAddresses")
			{
				return ISFAccessRules.Captions.Manufacturers;
			}

			ZBindToChecker.CheckBindTo(((TrackingCusISFHeader)null).Lines);
			if (boundPropertyName == "Lines")
			{
				return ISFAccessRules.Captions.Lines;
			}

			ZBindToChecker.CheckBindTo(((TrackingCusISFHeader)null).MainShipToParty);
			if (boundPropertyName == "Equipments")
			{
				return ISFAccessRules.Captions.Containers;
			}

			ZBindToChecker.CheckBindTo(((TrackingCusISFHeader)null).BuyingParty);
			if (boundPropertyName == "Equipments")
			{
				return ISFAccessRules.Captions.Buying_Party;
			}

			return null;
		}

		bool IAccessControlled.LoggedInOrgIs(string role)
		{
			ZGuid loggedInOrgPK = SiteUser.LoggedInOrganisation.PK;

			switch (role)
			{
				case ISFAccessRules.Roles.ISFBuyingParty:
					return CompareWithLoggedInOrg(BuyingParty);

				case ISFAccessRules.Roles.ISFConsolidator:
					return CompareWithLoggedInOrg(Consolidator);

				case ISFAccessRules.Roles.ISFImporter:
					return CompareWithLoggedInOrg(Importer);

				case ISFAccessRules.Roles.ISFManufacturer:
					foreach (JobDocAddress address in ManufacturerAddresses)
					{
						if (CompareWithLoggedInOrg(address))
						{
							return true;
						}
					}
					return false;

				case ISFAccessRules.Roles.ISFSellingParty:
					return CompareWithLoggedInOrg(SellingParty);

				case ISFAccessRules.Roles.ISFShipToLocation:
					return CompareWithLoggedInOrg(MainShipToParty);

				case ISFAccessRules.Roles.ISFStuffingLocation:
					return CompareWithLoggedInOrg(StuffingLocation);

				case ISFAccessRules.Roles.ISFBookingParty:
					return CompareWithLoggedInOrg(BookingParty);

				case ISFAccessRules.Roles.SendingAgent:
					return IsSendingAgent(loggedInOrgPK);

				default:
					return false;
			}
		}

		AccessControlRegistryItem IAccessControlled.SuppressionItem
		{
			get { return WebDataRegistry.Instance.ISFOrgsRolePropertySuppression; }
		}

		#region Implementation

		bool CompareWithLoggedInOrg(OrgHeader org)
		{
			if (org != null)
			{
				return SiteUser.OrganisationRelatedOrgPKs.Contains(org.PK);
			}
			return false;
		}

		bool CompareWithLoggedInOrg(JobDocAddress address)
		{
			if (address != null)
			{
				if (address.E2_AddressOverride)
				{
					ZGuid orgPK = (ZGuid)new OrgHeaderAutoCompleteHelper(Factory).GetKey(address.E2_CompanyNameTruncated);
					return SiteUser.OrganisationRelatedOrgPKs.Contains(orgPK);
				}

				return SiteUser.OrganisationRelatedOrgPKs.Contains(address.OrganisationPK);
			}
			return false;
		}

		public bool IsSendingAgent(ZGuid orgPK)
		{
			ZDBOnlyQuery oSBLFilter = new ZDBOnlyQuery(typeof(OrgSupplierBuyerLink));
			oSBLFilter.AddToFilter(JoinCondition.And, OrgSupplierBuyerLinkSchema.OL_OH_Supplier, SuppliersPKs);
			oSBLFilter.AddToFilter(JoinCondition.And, OrgSupplierBuyerLinkSchema.OL_OH_Buyer, BuyersPKs);

			ZDBOnlySubQuery oSBLTMFilter = new ZDBOnlySubQuery(typeof(OrgSupBuyLinkTrnMode), OrgSupBuyLinkTrnModeSchema.PF_OL);
			oSBLTMFilter.AddToFilter(OrgSupBuyLinkTrnModeSchema.PF_OH_SendingAgent, orgPK);

			oSBLFilter.AddSubQuery(oSBLTMFilter, JoinCondition.And);

			return Factory.LoadTop1<OrgSupplierBuyerLink>(oSBLFilter) != null;
		}

		List<ZGuid> BuyersPKs
		{
			get
			{
				List<ZGuid> result = new List<ZGuid>();

				AddIfValid(ExtraShipToPartyAddresses, result);
				AddIfValid(MainShipToParty, result);
				AddIfValid(Importer, result);
				AddIfValid(BuyingParty, result);

				return result;
			}
		}

		List<ZGuid> SuppliersPKs
		{
			get
			{
				List<ZGuid> result = new List<ZGuid>();

				AddIfValid(ManufacturerAddresses, result);
				AddIfValid(SellingParty, result);

				return result;
			}
		}

		void AddIfValid(OrgHeader org, List<ZGuid> orgPkList)
		{
			if (org != null && org.PK.IsValid)
			{
				orgPkList.Add(org.PK);
			}
		}

		void AddIfValid(IEnumerable<ISFDocAddress> addresses, List<ZGuid> orgPkList)
		{
			foreach (ISFDocAddress address in addresses)
			{
				AddIfValid(address, orgPkList);
			}
		}

		void AddIfValid(ISFDocAddress address, List<ZGuid> orgPkList)
		{
			if (address.OrganisationPK.IsValid)
			{
				orgPkList.Add(address.OrganisationPK);
			}
		}

		#endregion

		#endregion

		#region IWebDocumentsWithUploadSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return ((IDocManagerSupport)this).DocManagerInfo;
			}
		}

		public DocumentUploadSupport DocumentUploadHelper
		{
			get
			{
				if (documentUploadHelper == null)
				{
					documentUploadHelper = new DocumentUploadSupport(Factory);
				}
				return documentUploadHelper;
			}
		}

		DocumentUploadSupport documentUploadHelper;

		public void ResetDocumentHelper()
		{
			fDocumentHelper = null;
		}

		#endregion
	}
}
