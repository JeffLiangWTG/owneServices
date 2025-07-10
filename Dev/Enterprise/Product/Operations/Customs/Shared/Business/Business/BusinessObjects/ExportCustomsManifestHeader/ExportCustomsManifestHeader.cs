using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class ExportCustomsManifestHeader : AutoExportCustomsManifestHeader, IDocAddresses
	{
		#region Constants

		public new class Schema : AutoExportCustomsManifestHeader.Schema
		{
			public const string MessageStatus = "MessageStatus";
		}

		#endregion

		public ExportCustomsManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.ExportCustomsManifestHeaderFetchStrategy(this);
		}

		#endregion

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public bool IsSea => ED_TransportMode == Core.Constants.TransportModes.Sea;

		[ReadOnly(true)]
		public override ZString ED_CAN
		{
			get { return base.ED_CAN; }
			set { base.ED_CAN = value; }
		}

		[ReadOnly(true)]
		public override ZString ED_DocumentStatus
		{
			get { return base.ED_DocumentStatus; }
			set { base.ED_DocumentStatus = value; }
		}

		[ReadOnly(true)]
		public override ZString ED_DocumentStatusConditions
		{
			get { return base.ED_DocumentStatusConditions; }
			set { base.ED_DocumentStatusConditions = value; }
		}

		[ReadOnly(true)]
		public override ZString ED_DepartureReportStatus
		{
			get { return base.ED_DepartureReportStatus; }
			set { base.ED_DepartureReportStatus = value; }
		}

		public void PopulateED_BGMReferenceIfNeeded()
		{
			PopulateFormattedNumberPropertyIfRequired(ED_BGMReferenceInfo, Env.NumberFountains.ManifestJobNo);
		}

		public override ZString ED_RL_NKPortOfDestination
		{
			get { return base.ED_RL_NKPortOfDestination; }
			set
			{
				bool isDifferent = base.ED_RL_NKPortOfDestination != value;
				base.ED_RL_NKPortOfDestination = value;
				if (isDifferent)
				{
					RefUNLOCO portOfDestination = this.PortOfDestination;
					if (portOfDestination != null)
					{
						ED_RN_NKCountryOfDestination = portOfDestination.RL_RN_NKCountryCode;
					}
				}
			}
		}

		[RelatedBusinessObject("Vessel")]
		[List(nameof(Lookups) + "." + nameof(ExportCustomsManifestHeaderLookups.Vessels))]
		public override ZString ED_VesselName
		{
			get { return base.ED_VesselName; }
			set
			{
				bool hasChanged = base.ED_VesselName != value;
				base.ED_VesselName = value;
				if (hasChanged && !IsCopying)
				{
					ED_LloydsIMO = ZString.Empty;
					DefaultLloydsIMO();
				}
			}
		}

		void DefaultLloydsIMO()
		{
			var lloydsNumber = Vessel?.RV_LloydsNumber ?? ZString.Empty;
			if (!lloydsNumber.IsEmpty && lloydsNumber != ED_LloydsIMO)
			{
				ED_LloydsIMO = lloydsNumber;
				Validation.ValidateED_VesselName();
			}
		}

		/// <summary>
		/// When replacing ED_RV_NKVessel with ED_VesselName with regen and removing the unique constraint on Vessel RV_Code(Name)
		/// This Vessel functionality will be effective and required. Until then, only 1 vessel will always be found.
		///
		///	For Export Manifest, the loading of the RefVessel is changed to take the Lloyds/IMO into consideration:
		///		By default, the VesselName and LloydsIMO will be used. Each country can choose to override if they decide.
		///		The logic should be:
		///		a)	try to load by VesselName + LloydsIMO, if a single vessel is found, then return that vessel;
		///		b)	If the LloydsIMO is not given, try to load by the VesselName only, if a single vessel is found, then return that vessel;
		///		c)	If multiple vessels are found, return null. (User will need to manually select the required vessel)
		/// 
		/// </summary>
		public RefVessel Vessel => Vessels.Length == 1 ? Vessels[0] : null;

		public bool VesselHasDuplicates => Vessels.Length > 1;

		RefVessel[] Vessels => Factory.GetValue(ref fVesselsCached, () => LoadVesselsCore());
		CachedProperty<RefVessel[]> fVesselsCached;

		protected virtual RefVessel[] LoadVesselsCore()
		{
			var vesselQuery = new ZQuery(RefVesselSchema.RV_Code, ED_VesselName);
			vesselQuery.IgnoreActiveFilter = true;
			if (!ED_LloydsIMO.IsEmpty)
			{
				vesselQuery.AddToFilter(RefVesselSchema.RV_LloydsNumber, ED_LloydsIMO);
			}

			return Factory.Load<RefVessel>(vesselQuery);
		}

		[MaxLength(100)]
		public ZString MessageStatus
		{
			get { return GetMessageStatus(Messages.LastMessage); }
		}

		protected ZString GetMessageStatus(EDIMessage lastMessage)
		{
			ZString result = ZString.Empty;
			if (lastMessage == null)
			{
				result = Res.GetString("1773df86-bdb5-42d7-a74d-a1c1ab5f6be1", "No Messages Sent");
			}
			else if (lastMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
			{
				if (lastMessage.EM_Status == EDIMessage.Status.Rejected)
				{
					result = Res.GetString("7e00fa25-bb50-41df-bdd4-1204a548d743", "{0} sent at {1} was rejected by Customs", lastMessage.EM_MessageType, lastMessage.EM_SystemCreateTimeUtc);
				}
				else
				{
					result = Res.GetString("964e9d11-293c-4688-8d97-56db166beadf", "Waiting for response to {0} sent at {1}", lastMessage.EM_MessageType, lastMessage.EM_SystemCreateTimeUtc);
				}
			}
			else if (lastMessage.EM_ReceiveTransmit == EDIMessage.Direction.Receive)
			{
				result = Res.GetString("7ef1c8e6-cda8-4d5c-90ca-3b3f8ecbaef3", "Response for {0} received at {1}", lastMessage.EM_MessageType, lastMessage.EM_SystemCreateTimeUtc);
			}
			return result;
		}

		public ZPropertyInfo MessageStatusInfo
		{
			get { return GetZPropertyInfo(Schema.MessageStatus); }
		}

		public EDIMessageFlattenedCollection MessagesIncludingInterchangeRejections
		{
			get { return fMessagesIncludingInterchangeRejections ?? (fMessagesIncludingInterchangeRejections = Messages.MessagesIncludingInterchangeRejections(ZString.Empty)); }
		}
		EDIMessageFlattenedCollection fMessagesIncludingInterchangeRejections;

		public virtual bool IsWaitingForManifestResponse
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsWaitingForDepartureReportResponse
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsManifestDeclaredAtCustoms
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsDepartureReportDeclaredAtCustoms
		{
			get
			{
				return false;
			}
		}

		#region Related Business Objects

		[ChildEditable(true)]
		public ExportCustomsManifestLinesCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = CreateNewExportCustomsManifestLinesCollection();
					fLines.Load();
					RegisterEditableChildObject(fLines);
				}
				return fLines;
			}
		}
		protected ExportCustomsManifestLinesCollection fLines;

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}
		protected EDIMessageCollection fMessages;

		#endregion

		#region Overridden Methods

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateED_BGMReferenceIfNeeded();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!IsInDatabase)
			{
				ED_BGMReference = ED_BGMReference = ZString.Empty;
			}
		}

		public override void Delete()
		{
			Lines.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Implementation

		protected virtual ExportCustomsManifestLinesCollection CreateNewExportCustomsManifestLinesCollection()
		{
			return new ExportCustomsManifestLinesCollection(this, Factory);
		}

		#endregion

		#region Pack Depot Address
		public void AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		public bool CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		public void DocAddressChanged(JobDocAddress docAddress)
		{
			PackDepotAddress.Validation.ValidateAll();
		}

		public Security.SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		public JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ConsignorDocumentaryAddress:
					return PackDepotAddressRequirement;
				default:
					return null;
			}
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		public void OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		public void OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new JobDocAddressCCPValidation(addressToValidate);
		}

		public IReadOnlyList<DocAddressType> SupportedAddressTypes
		{
			get { return Array.Empty<DocAddressType>(); }
		}

		public JobDocAddress PackDepotAddress
		{
			get { return CachedGetAddress(ref packDepotAddress, PackDepotAddressRequirement); }
		}

		JobDocAddress packDepotAddress;
		#region DocAddresses

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}
				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		#endregion

		JobDocAddress CachedGetAddress(ref JobDocAddress field, JobDocAddressRequirement requirement)
		{
			if (field == null || field.IsDeleted)
			{
				UnRegisterListChangedCalledRefreshBinding(field);
				field = DocAddresses.FindOrCreateWithRequirement(requirement);
				field.DefaultAddressType = AddressType.PIC;
				RegisterListChangedCalledRefreshBinding(field);
			}
			return field;
		}

		JobDocAddressRequirement PackDepotAddressRequirement
		{
			get
			{
				if (packDepotAddressRequirement == null)
				{
					packDepotAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.Consignor);
				}

				return packDepotAddressRequirement;
			}
		}
		JobDocAddressRequirement packDepotAddressRequirement;

		public OrgHeaderCollection OrgHeaderList
		{
			get
			{
				OrgHeaderCollection result = null;
				result = new OrgHeaderCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.PackingDepot));
				return result;
			}
		}

		#endregion
	}
}
