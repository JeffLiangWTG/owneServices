using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Business
{
	public abstract class LinerAndAgencyBaseWebInterfacesHelper : NonPersistentBusinessObject,
		IBizOChangesEmailNotification,
		IWebUserVisibleNotesSupport,
		IWebUserEditableNoteSupport,
		IWebDocumentsSupport,
		IWebDocumentsWithUploadSupport,
		IAdditionalSource,
		IObsoleteValidation
	{
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		protected LinerAndAgencyBaseWebInterfacesHelper(AgencyShipment agencyShipment) : base(agencyShipment.Factory)
		{
			if (agencyShipment == null)
			{
				throw new NullReferenceException("A business object of AgencyShipment type is expected, but null was passed.");
			}

			AgencyShipment = agencyShipment;
			fKey = ZGuid.NewZGuid();
		}

		public readonly AgencyShipment AgencyShipment;

		// Member of IWebEmailNotificationBase, IWebUserVisibleNotesSupport, IWebDocumentsSupport interfaces
		public OrgContact LoggedInContact
		{
			get
			{
				return (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null) ?
					WebEnv.AppInstance.SiteUser.LoggedInUser as OrgContact : null;
			}
		}

		#region IBizOChangesEmailNotification members

		public new ZGuid PK
		{
			get { return AgencyShipment.PK; }
		}

		public new ZString HumanReadableName
		{
			get { return AgencyShipment.HumanReadableName; }
		}

		public ZString Number
		{
			get { return AgencyShipment.JS_UniqueConsignRef; }
		}

		public OrgHeader RelatedOrg
		{
			get { return AgencyShipment.BookingParty; }
		}

		public GlbBranch EventBranch
		{
			get
			{
				GlbBranch result = GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, AgencyShipment.CalcLoadPort);

				if (result == null && AgencyShipment.BookingParty != null)
				{
					result = GlbBranch.FindControllingBranchWithFallBackToAnyCompany(AgencyShipment.BookingParty);
				}
				if (result == null && AgencyShipment.BookingParty != null && AgencyShipment.BookingParty.ClosestPort != null)
				{
					result = GlbBranch.FindByHomePortWithFallBackToRelatedPort(Factory, AgencyShipment.BookingParty.ClosestPort);
				}

				return result;
			}
		}

		public abstract CodePairRegistryItem NotificationSendingRule { get; }

		public abstract CodeDescriptionBoolRegistryItem StaffRolesToNotify { get; }

		public abstract GuidRegistryItem EmailGroupRegistryItem { get; }

		public ZGuid GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			ZString staffNK = staffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsCollection.Direction.Export,
				OrgStaffAssignmentsCollection.AirSea.Sea);
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

		public new bool IsInDatabase
		{
			get { return AgencyShipment.IsInDatabase; }
		}

		public ZBool IsCancelled
		{
			get { return AgencyShipment.JS_IsCancelled; }
		}

		public new bool IsDeleted
		{
			get { return AgencyShipment.IsDeleted; }
		}

		public override bool HasChanges
		{
			get { return AgencyShipment.HasChanges || base.HasChanges; }
		}

		#region Email Reporting

		public abstract void AddPropertiesForEmailReporting(DataState state);

		protected void AddContainersForEmailReporting(DataState state, AgencyShipmentContainerDependentCollection collection)
		{
			int i = 1;
			foreach (AgencyShipmentContainer container in collection)
			{
				var value = GenerateContainerDetailsForEmailReporting(container);

				var propertyName = ResString.GetMultilingualString("56bbb1ee-ce04-49f7-a9af-f2e74c60d2d2", "Container {0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

		protected virtual MultilingualString GenerateContainerDetailsForEmailReporting(AgencyShipmentContainer container)
		{
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("8b75a9fa-7e9f-4b7c-9787-3a0606accd2f", "Container #: {0}", container.JC_ContainerNum),
				ResString.GetMultilingualString("815983b6-83a2-43fe-8551-cdd899ba097f", "Type: {0}", container.RefContainer != null ? container.RefContainer.RC_Code : ZString.Empty),
				ResString.GetMultilingualString("ebfef5c4-5ff1-4576-b781-e8728294554d", "Count: {0}", container.JC_ContainerCount),
				ResString.GetMultilingualString("adb9b8ab-6b80-456d-ae59-4547b2f6e2f8", "Gross Weight: {0} {1}", container.JC_GrossWeight, container.JC_GrossWeightUQ),
				ResString.GetMultilingualString("090a5b7a-ea46-4c9e-aa82-02df0826095c", "Is Shipper Owned: {0}", container.JC_IsShipperOwned));
		}

		protected void AddPackLinesForEmailReporting(DataState state, AgencyShipmentPackLineCollection collection)
		{
			int i = 1;
			foreach (AgencyShipmentPackLine line in collection)
			{
				var value = GeneratePackLineDetailsForEmailReporting(line);

				var propertyName = ResString.GetMultilingualString("c023988b-70c0-41e0-95dc-24a8b369e8af", "Pack Line {0}", i++);

				PropertiesForEmailReporting.Add(state, propertyName, value);
			}
		}

		protected virtual MultilingualString GeneratePackLineDetailsForEmailReporting(AgencyShipmentPackLine packLine)
		{
			return MultilingualString.Join(System.Environment.NewLine,
			ResString.GetMultilingualString("77fd63ff-3865-4ea1-bebd-e8c109e64654", "Container: {0}", packLine.Container != null ? packLine.Container.ContainerCode : ZString.Empty),
			ResString.GetMultilingualString("ce5a1c39-7d97-4dd2-94a8-974a078a58d1", "Packs: {0}", packLine.JL_PackageCount),
			ResString.GetMultilingualString("4d1967c2-422e-4cd6-bb81-87c5337175d9", "Pack Type: {0}", packLine.JL_F3_NKPackType),
			ResString.GetMultilingualString("e941f3e3-3625-40fb-895c-1ff3001d0555", "Length: {0} {1}", packLine.JL_Length, packLine.JL_UnitOfDimension),
			ResString.GetMultilingualString("6c93f590-4522-49c2-b0e5-ce866726aa7d", "Width: {0} {1}", packLine.JL_Width, packLine.JL_UnitOfDimension),
			ResString.GetMultilingualString("b7c881a8-f99b-4c7b-ab3c-b0b7a2c8d7e7", "Height: {0} {1}", packLine.JL_Height, packLine.JL_UnitOfDimension),
			ResString.GetMultilingualString("15d106ad-00d1-4dd9-97c1-d1be2a93feee", "Weight: {0} {1}", packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ),
			ResString.GetMultilingualString("8c50e53f-dafb-4522-95ea-774dc7c38261", "Volume: {0} {1}", packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ),
			ResString.GetMultilingualString("253a03ba-632c-482d-ae7b-3d7bc49c9d1f", "Commodity: {0}", packLine.JL_RH_NKCommodityCode),
			ResString.GetMultilingualString("e65c054a-2273-482b-bced-8104dd3cdc9e", "Description: {0}", packLine.JL_DetailedDescription),
			ResString.GetMultilingualString("6098f0b1-3178-41ac-8240-e34613104e2b", "Dangerous Goods: {0}", packLine.UNDGs.Count > 0 ? (packLine.UNDGs[0].Substance != null ? packLine.UNDGs[0].Substance.DG_Code + " " + packLine.UNDGs[0].Substance.DG_PSN : string.Empty) : string.Empty),
			ResString.GetMultilingualString("41e43a8d-d784-4899-bf44-0b20bf4a7fc3", "Harmonized Code: {0}", packLine.JL_HarmonisedCode));
		}

#if DEBUG

		internal MultilingualString GeneratePackLineDetailsForEmailReportingForTest(AgencyShipmentPackLine packLine)
		{
			return GeneratePackLineDetailsForEmailReporting(packLine);
		}

#endif

		public PropertyChangeInfo[] GetPropertiesForEmailReporting()
		{
			return PropertiesForEmailReporting.GetValuesAsArray();
		}

		protected PropertyChangeInfoCollection PropertiesForEmailReporting
		{
			get { return fPropertiesForEmailReporting ?? (fPropertiesForEmailReporting = new PropertyChangeInfoCollection()); }
		}
		PropertyChangeInfoCollection fPropertiesForEmailReporting;

		#endregion

		public abstract ControllerID ControllerForEnterpriseUrl { get; }

		#endregion

		#region IWebUserVisibleNotesSupport members

		public IStmNoteParent NotesParentBO
		{
			get { return AgencyShipment; }
		}

		public WebUserVisibleNotes NotesHelper
		{
			get { return notesHelper ?? (notesHelper = new WebUserVisibleNotes(this)); }
		}
		WebUserVisibleNotes notesHelper;

		public bool ShowAgentNotes
		{
			get { return false; }
		}

		#endregion

		#region IWebUserEditableNoteSupport members

		public WebUserEditableNote UserEditableNoteHelper
		{
			get { return editableNoteHelper ?? (editableNoteHelper = new WebUserEditableNote(this, PredefinedNoteTypes.Instance.WebUserNote)); }
		}
		WebUserEditableNote editableNoteHelper;

		#endregion

		#region IWebDocumentsSupport members

		public ZGuid DocParentPK
		{
			get { return AgencyShipment.PK; }
		}

		public List<ZGuid> DocRelatedPKs
		{
			get { return new List<ZGuid>(); }
		}

		public DocumentSupport DocumentHelper
		{
			get { return documentHelper ?? (documentHelper = new DocumentSupport(this)); }
		}
		DocumentSupport documentHelper;

		#endregion

		#region IWebDocumentsWithUploadSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return ((IDocManagerSupport)AgencyShipment).DocManagerInfo;
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
			documentHelper = null;
		}

		#endregion

		#region IAdditionalSource Members

		public ZGuid Key
		{
			get { return fKey; }
		}

		readonly ZGuid fKey;

		#endregion
	}
}
