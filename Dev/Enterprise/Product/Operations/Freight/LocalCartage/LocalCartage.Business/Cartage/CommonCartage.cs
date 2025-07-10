using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.CFS;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using CommonShipment = Enterprise.Freight.Business.CommonShipment;
using Constants = Enterprise.Core.Constants;
using IJobSailing = Enterprise.Integration.Freight.IJobSailing;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.Freight.LocalCartage.Business
{
	[UniversalDataContext(DataContextType.LocalTransport)]
	[CodeProperty(CommonCartage.Schema.JJ_ConsignmentID), DescriptionProperty(CommonCartage.Schema.JJ_ConsignmentID)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.CommonCartage)]
	[UserDefinedValues]
	public class CommonCartage : AutoJobCartage,
		Integration.ICommonCartage,
		IDocAddresses,
		IJobInvoicingPlugIn,
		IEDocsProvider,
		ISendEmailSource,
		ITemplateCopyable,
		ICDArchive,
		IRatingSupporter,
		ISupportDataImporting,
		ISailingManaged,
		IWorkflowProvider,
		ISailingParentFindBox,
		IRelatedJob,
		ICreditControlledDocumentDelivery,
		IAdditionalReferenceNumberSupporter,
		IAdditionalReferenceNumberTypeProvider,
		IImportExport,
		ICustomFieldProvider,
		IRelatableActivity,
		IUniversalXMLNoteParent,
		IConsignmentService
	{
		public CommonCartage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoJobCartage.Schema
		{
			public const string PortOfLoading = "PortOfLoading";
			public const string PortOfDischarge = "PortOfDischarge";
			public const string VoyageFlight = "VoyageFlight";
			public const string Vessel = "Vessel";
			public const string E_ARV = "E_ARV";
			public const string E_DEP = "E_DEP";
			public const string A_ARV = "A_ARV";
			public const string A_DEP = "A_DEP";

			public const string FCLReceivalCommences = "FCLReceivalCommences";
			public const string LCLReceivalCommences = "LCLReceivalCommences";
			public const string FCLCutOff = "FCLCutOff";
			public const string LCLCutOff = "LCLCutOff";
			public const string FCLAvailabilityDate = "FCLAvailabilityDate";
			public const string LCLAvailabilityDate = "LCLAvailabilityDate";
			public const string FCLStorageDate = "FCLStorageDate";
			public const string LCLStorageDate = "LCLStorageDate";

			public const string ContainerNumber = "ContainerNumber";
			public const string LocalClientAddressPK = "LocalClientAddressPK";

			public const string GrossWeight = "GrossWeight";
		}

		[ChildEditableTestExclude()]
		[ChildEditable]
		public CommonBookedCtgMoveCollection LooseBookedMoves
		{
			get
			{
				if (looseBookedMoves == null)
				{
					DependentRelationship looseRelationship = new DependentRelationship(
						 this,
						 typeof(CommonBookedCtgMove),
						 new ZQuery(JobBookedCtgMoveSchema.EW_JC_Container, null),
						 JobBookedCtgMoveSchema.EW_JJ);
					looseBookedMoves = new CommonBookedCtgMoveCollection(Factory, looseRelationship);
					UpdateRegisteredEditable(looseBookedMoves);
				}
				return looseBookedMoves;
			}
		}
		CommonBookedCtgMoveCollection looseBookedMoves;

		[ChildEditableTestExclude()]
		[ChildEditable]
		public CommonBookedCtgMoveCollection ContainerBookedMoves
		{
			get
			{
				if (containerBookedMoves == null)
				{
					var relationship = new ContainerBookedMovesRelationship(this);
					containerBookedMoves = new CommonBookedCtgMoveCollection(Factory, relationship, !HasParent);
					UpdateRegisteredEditable(containerBookedMoves);
				}
				return containerBookedMoves;
			}
		}
		CommonBookedCtgMoveCollection containerBookedMoves;

		class ContainerBookedMovesRelationship : DependentRelationship
		{
			public ContainerBookedMovesRelationship(CommonCartage cartage)
				: base(cartage, typeof(CommonBookedCtgMove), new ZQuery(JobBookedCtgMoveSchema.EW_JC_Container, SQLComparisonOperator.NotEqual, null), JobBookedCtgMoveSchema.EW_JJ)
			{
			}

			protected override void AddToRelationship(BusinessObject businessObject)
			{
				var cartage = (CommonCartage)Master;
				var move = (CommonBookedCtgMove)businessObject;
				CommonContainer container;

				if (move.EW_JC_Container.IsEmpty)
				{
					container = cartage.Factory.New<CommonContainer>();
					move.EW_JC_Container = container.PK;
				}

				// we have to set EW_JC_Container before calling base.AddToRelationship (move.IsContainerised)
				base.AddToRelationship(businessObject);
			}
		}

		public CommonBookedCtgMoveCollection BookedMovesCollection
		{
			get { return bookedMoves ?? (bookedMoves = new CommonBookedCtgMoveCollection(this)); }
		}
		CommonBookedCtgMoveCollection bookedMoves;

		public CommonBookedCtgMove[] GetBookedMoves(CommonContainer container)
		{
			return ContainerBookedMoves.Where(m => m.Container == container).ToArray();
		}

		public CommonCartageLegCollection CartageLegs
		{
			get
			{
				if (cartageLegs == null)
				{
					//Hit other Collections to trigger editable children
					foreach (var containerMoveHit in ContainerBookedMoves)
					{
						var legHits = containerMoveHit.CartageLegs;
					}

					foreach (var looseMoveHit in LooseBookedMoves)
					{
						var legHits = looseMoveHit.CartageLegs;
					}

					cartageLegs = new CommonCartageLegCollection(BookedMovesCollection);
					cartageLegs.CountChanged += new EventHandler(delegate
					{ if (!IsDeleted && !JJ_IsCancelled) { CheckJobCompletion(); } MarkAsNeedingTotalDemurrageCheck(); });
					UpdateRegisteredEditable(cartageLegs);
				}
				return cartageLegs;
			}
		}
		CommonCartageLegCollection cartageLegs;

		IEnumerable<Integration.ICommonCartageLeg> Integration.ICommonCartage.Legs
		{
			get { return CartageLegs; }
		}

		public IEnumerable<CommonContainer> Containers
		{
			get
			{
				if (containers == null)
				{
					containers = ContainerBookedMoves.Select(m => m.Container);
				}
				return containers;
			}
		}
		IEnumerable<CommonContainer> containers;

		public CommonCartageType CartageType
		{
			get
			{
				if (fCartageType == null)
				{
					fCartageType = new CachedProperty<CommonCartageType>(Factory, delegate
					{
						if (!JJ_E3_NKJobType.IsEmpty)
						{
							return Factory.LoadTop1<CommonCartageType>(new ZQuery(LocalCartageJobTypeSchema.E3_JobType, JJ_E3_NKJobType));
						}
						return null;
					});
				}
				return fCartageType.Value;
			}
		}
		CachedProperty<CommonCartageType> fCartageType;

		public CommonCartageBookingInformation BookingInformation
		{
			get
			{
				if (bookingInformation == null)
				{
					bookingInformation = new CommonCartageBookingInformation(this);
				}
				return bookingInformation;
			}
		}
		CommonCartageBookingInformation bookingInformation;

		public JobDocsAndCartage DocsAndCartageParent
		{
			get
			{
				IShipmentWithDocsAndCartage cartageDocParent = CartageParent as IShipmentWithDocsAndCartage;
				return (cartageDocParent != null) ? cartageDocParent.DocsAndCartage : null;
			}
		}

		[ChildEditable()]
		public CusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					additionalReferenceNumbers = new CusEntryNumAdditionalReferenceCollection(this);
					additionalReferenceNumbers.Load();
					RegisterEditableChildObject(additionalReferenceNumbers);
				}

				return additionalReferenceNumbers;
			}
		}
		CusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		public OrgHeader BookingParty
		{
			get
			{
				var docAddress = DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
				var result = docAddress != null && !docAddress.E2_AddressOverride ? docAddress.Organisation : null;
				if (result == null && ParentBooking != null)
				{
					// instead import into LT from uxml
					var parentBookingConsolidation = Factory.Load<IDtbBookingConsolidation>(ParentBooking.KM_KB_Booking);
					if (parentBookingConsolidation != null)
					{
						docAddress = ((IDocAddresses)parentBookingConsolidation).DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
						result = docAddress != null && !docAddress.E2_AddressOverride ? docAddress.Organisation : null;
						if (result == null) // if there is a parent booking, but no booking party, booked by self
						{
							result = GlbCompany.CurrentCompany.OrgProxy;
						}
					}
				}

				return result;
			}
		}

		public RelatedJobCollection RelatedJobs
		{
			get { return relatedJobs ?? (relatedJobs = GetRelatedJobs()); }
		}

		RelatedJobCollection GetRelatedJobs()
		{
			var idtbParentBooking = ParentBooking;
			var result = new RelatedJobCollection(Factory);
			var parentBookingAsBizO = idtbParentBooking as BusinessObject;
			if (parentBookingAsBizO != null)
			{
				result.Add(parentBookingAsBizO);
				result.AddRange(idtbParentBooking.RelatedJobs.Cast<IRelatedJob>().Where(rj => rj != this));
			}
			return result;
		}

		RelatedJobCollection relatedJobs;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			using (SuspendBehavior())
			{
				JJ_Status = "v2"; // Created using Cartage 2.0 ... To ensure internal Cartages don't use Parent Job Header
				JJ_E3_NKJobType = GetDefaultCartageType();
				DefaultModesFromJobTypeTemplate();
				JJ_F3_NKPackType = BehaviorStrategy.DefaultPackageType(this);
				JJ_VolumeUQ = Env.Registry.FreightVolumeUnit;
				JJ_WeightUQ = Env.Registry.FreightWeightUnit;
				JJ_GB = GlbBranch.CurrentBranch.PK;
			}
		}

		string GetDefaultCartageType()
		{
			return BindToLists.NewCartageJobTypes.ContainsCode(Constants.CartageJobType.NEW_FCLImportToCNE) ? Constants.CartageJobType.NEW_FCLImportToCNE : "";
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (JJ_IsCancelled)
			{
				UpdateReadOnlyForWhenCancelled();
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				JobHeader.DeleteAllJobs(this);
				BookedMovesCollection.DeleteAll();

				((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
				RelatedChildActivityPivotCollection.DeleteAll();
				RelatedParentActivityPivotCollection.DeleteAll();

				base.Delete();
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				JJ_ConsignmentID = "";
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			if (HasChanges)
			{
				if (!JJ_ConsignmentIDInfo.OriginalValue.IsEmpty && JJ_ConsignmentIDInfo.HasChanges)
				{
					UpdateJobNumForRelatedJobs();
				}
			}

			if (MarkedAsNeedingTotalDemurrageCheck)
			{
				PopulateParentDemurrage();
				MarkedAsNeedingTotalDemurrageCheck = false;
			}
		}

		public void MarkAsNeedingTotalDemurrageCheck()
		{
			MarkedAsNeedingTotalDemurrageCheck = true;
		}

		bool MarkedAsNeedingTotalDemurrageCheck;

		void PopulateParentDemurrage()
		{
			if (HasParent && CartageInternalType != null)
			{
				var totalDemurrageTime = new TimeSpan();
				Array.ForEach(CartageLegs.ToArray(), l => totalDemurrageTime += l.GetTotalDemurrage());
				CartageInternalType.SetTotalDemurrage(totalDemurrageTime);
			}
		}

		public override void OnSaving()
		{
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			PopulateJJ_ConsignmentIDIfNeeded();
			ProcessNewCartageWithValidNonTransportBookingParent();
			DeactivateJobHeaderWhenIsCancelled();
			base.OnSaving();
		}

		void ProcessNewCartageWithValidNonTransportBookingParent()
		{
			if (CartageParent != null && !IsInDatabase && !IsDeleted)
			{
				InternalCartageManagerHelper.LogCartageCreationOnParent(this);
			}
		}

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return string.Format(CultureInfo.CurrentCulture, "{0} {1}", DescriptionWithoutJobNo, JJ_ConsignmentID); }
		}

		ZString DescriptionWithoutJobNo
		{
			get { return Res.GetString("CommonCartage|DescriptionWithoutJobNo", "Port Transport"); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CommonCartageFetchStrategy(this);
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>();

				if (!this.IsDeleted)
				{
					if (HasParent)
					{
						result.Add(GetParentWithRelatedNotes());
					}
					foreach (JobDocAddress docAddress in DocAddresses)
					{
						var org = docAddress.Organisation;
						if (org != null && !result.Contains(org))
						{
							result.Add(org);
						}
					}
				}

				return result.ToArray();
			}
		}

		BusinessObject GetParentWithRelatedNotes()
		{
			BusinessObject result;

			if (JJ_ParentTableCode == JobShipmentSchema.Constants.Prefix)
			{
				result = Factory.Load<CommonShipment>(JJ_ParentID); // Notes.GetElementsCollectionFromRelatedBizObject blows up cause QuotedBookings doesn't use a StmNoteCollection, just get it directly from the Shipment
			}
			else
			{
				result = (BusinessObject)CartageParent;
			}

			return result;
		}

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts noteContexts = base.NoteContextsForRelatedNotes;

				noteContexts.Module |= StmNoteContextModule.T;
				AddNoteContextsForRelatedNotes_Directions(noteContexts);
				AddNoteContextsForRelatedNotes_FreightModes(noteContexts);

				return noteContexts;
			}
		}

		void AddNoteContextsForRelatedNotes_Directions(StmNoteContexts noteContexts)
		{
			noteContexts.Direction |= StmNoteContextDirection.D;

			if (IsImportOrDestination)
			{
				noteContexts.Direction |= StmNoteContextDirection.I;
				noteContexts.Direction |= StmNoteContextDirection.B;
			}
			else if (IsExportOrOrigin)
			{
				noteContexts.Direction |= StmNoteContextDirection.E;
				noteContexts.Direction |= StmNoteContextDirection.B;
			}
		}

		void AddNoteContextsForRelatedNotes_FreightModes(StmNoteContexts noteContexts)
		{
			noteContexts.FreightMode |= StmNoteContextFreightMode.R;
			if (IsAir)
			{
				noteContexts.FreightMode |= StmNoteContextFreightMode.I;
				noteContexts.FreightMode |= StmNoteContextFreightMode.B;
			}
			else if (IsSea)
			{
				noteContexts.FreightMode |= StmNoteContextFreightMode.S;
				noteContexts.FreightMode |= StmNoteContextFreightMode.B;
			}
			else if (IsRail)
			{
				noteContexts.FreightMode |= StmNoteContextFreightMode.W;
			}

			if (IsContainerised)
			{
				noteContexts.FreightMode |= StmNoteContextFreightMode.F;
			}

			if (IsLoose)
			{
				noteContexts.FreightMode |= StmNoteContextFreightMode.L;
			}
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(Containers.ToArray());
				result.AddRange(CartageLegs.ToArray());

				if (Job != null)
				{
					result.Add(Job);
				}

				if (IsInDatabase)
				{
					var invoiceLoader = new InvoiceLoader(Factory);
					result.AddRange(invoiceLoader.GetInvoicesForUniqueRef(JJ_ConsignmentID));
				}

				var parent = CartageParent; // tested in ICartageParentTestCase.TestRelatedLocalTransportObjectsWithEvents
				if (parent != null)
				{
					var boForRelatedEvents = parent.BusinessObjectForRelatedEvents as BusinessObject;
					if (boForRelatedEvents != null)
					{
						result.Add(boForRelatedEvents);
					}
				}

				return result.ToArray();
			}
		}

		public override ZDateTime JJ_A_JCL
		{
			get { return base.JJ_A_JCL; }
			set
			{
				base.JJ_A_JCL = value;
				Logs.CreateRecreateOrUpdateEventLog(AutoEvents.DeliveryCartageCompleteFinalised, EstimateActual.Actual, value.ToOffset());
			}
		}

		[List("Lookups.DropModes")]
		public override ZString JJ_DropMode
		{
			[DebuggerStepThrough()]
			get { return base.JJ_DropMode; }
			set
			{
				if (base.JJ_DropMode != value)
				{
					base.JJ_DropMode = value;
					QueryUserToSetMoves();
				}
			}
		}

		void QueryUserToSetMoves()
		{
			if (!IsImportingData && !JJ_DropMode.IsEmpty && !JJ_DropModeInfo.HasErrors() && HasMovesToUpdate)
			{
				var message = Res.GetString("9906e08b-8839-4e92-a7bf-67c49982e3ba", "Would you like to override ALL Booked Movement Drop Modes with '{0}'?", JJ_DropMode);
				var caption = Res.GetString("94cad7be-2088-4ec2-a226-e68e3a1bb920", "Populate Booked Movement Drop Mode");
				var e = new QueryUserYesNoEventArgs(caption, message, false);

				NotificationsQueryUser(e);

				if (e.Response)
				{
					if (BindToLists.DropModes(true).ContainsCode(JJ_DropMode))
					{
						SetContainerisedMovesDropMode(JJ_DropMode);
					}

					if (BindToLists.DropModes(false).ContainsCode(JJ_DropMode))
					{
						SetLooseMovesDropMode(JJ_DropMode);
					}
				}
			}
		}

		bool HasMovesToUpdate
		{
			get
			{
				bool result;

				if (IsMixed)
				{
					result = (BindToLists.DropModes(true).ContainsCode(JJ_DropMode) && HasContainerizedMovesToUpdate(JJ_DropMode))
					|| (BindToLists.DropModes(false).ContainsCode(JJ_DropMode) && HasLooseMovesToUpdate(JJ_DropMode));
				}
				else if (IsLoose)
				{
					result = HasLooseMovesToUpdate(JJ_DropMode);
				}
				else
				{
					result = HasContainerizedMovesToUpdate(JJ_DropMode);
				}

				return result;
			}
		}

		bool HasLooseMovesToUpdate(ZString dropMode)
		{
			return LooseBookedMoves.Any(move => move.EW_DropMode != dropMode);
		}

		bool HasContainerizedMovesToUpdate(ZString dropMode)
		{
			foreach (var container in Containers)
			{
				if (GetBookedMoves(container).Any(move => move.EW_DropMode != dropMode))
				{
					return true;
				}
			}

			return false;
		}

		public event CancelEventHandler OnJobTypeChanging;

		public void RaiseOnJobTypeChanging(CancelEventArgs e)
		{
			if (OnJobTypeChanging != null)
			{
				OnJobTypeChanging(this, e);
			}
		}

		[List("JobTypeList")]
		public override ZString JJ_E3_NKJobType
		{
			[DebuggerStepThrough()]
			get { return base.JJ_E3_NKJobType; }
			set
			{
				if (IsBehaviorSuspended)
				{
					base.JJ_E3_NKJobType = value;
				}
				else
				{
					var e = new CancelEventArgs { Cancel = false };
					RaiseOnJobTypeChanging(e);
					if (!e.Cancel)
					{
						BehaviorStrategy.BeforeCartageTypeChange(this, value);

						base.JJ_E3_NKJobType = value;

						if (!JJ_E3_NKJobType.IsEmpty)
						{
							DefaultModesFromJobTypeTemplate();
							BehaviorStrategy.AfterCartageTypeChange(this);
							DefaultDropModeOnJobTypeChange();
						}
					}
				}
				RefreshBindingIncludingChildren();
			}
		}

		internal void DefaultModesFromJobTypeTemplate()
		{
			var cartageType = CartageType;
			if (cartageType != null)
			{
				JJ_ShippingTransportMode = cartageType.E3_ShippingTransportMode;
				JJ_Direction = cartageType.Direction;
				JJ_ContainerMode = GetContainerModeFromJobTypeTemplate(cartageType.ContainerMode);
			}
		}

		ZString GetContainerModeFromJobTypeTemplate(string cartageTypeContainerMode)
		{
			switch (cartageTypeContainerMode)
			{
				case Constants.CartageContainerMode.Loose:
				case Constants.CartageContainerMode.FTL:
					return Constants.CartageContainerMode.Loose;
				case Constants.CartageContainerMode.Containerized:
				case Constants.CartageContainerMode.EmptyContainer:
				case Constants.CartageContainerMode.FCL:
					return Constants.CartageContainerMode.Containerized;
				case Constants.CartageContainerMode.Mixed:
					return Constants.CartageContainerMode.Mixed;
				default:
					return "";
			}
		}

		abstract class DefaultingHelper
		{
			protected DefaultingHelper(CommonCartage cartage)
			{
				this.cartage = cartage;
			}

			protected CommonCartage Cartage
			{
				get { return cartage; }
			}

			readonly CommonCartage cartage;

			internal ZBool DropModeIsDifferent
			{
				get
				{
					var defaultDropMode = Cartage.IsLoose ? LooseDropMode : ContainerisedDropMode;
					return
						IsMovesContainerisedDropModeDifferent ||
						IsMovesLooseDropModeDifferent ||
						(!defaultDropMode.IsEmpty && defaultDropMode != Cartage.JJ_DropMode);
				}
			}

			internal string Text
			{
				get
				{
					var result = new ZStringBuilder();

					if (DropModeIsDifferent)
					{
						if (IsMovesContainerisedDropModeDifferent)
						{
							result.Append(Res.GetString("93d17bec-98f7-4356-8e6d-fbb045aeebf1", "Set Container moves with {0} Drop Mode of '{1}'", ContainerisedDropModeType, ContainerisedDropMode));
						}

						if (IsMovesLooseDropModeDifferent)
						{
							result.Append(Res.GetString("1d3ae64d-e157-4ab4-9660-9233b82667a8", "Set Loose moves with {0} Drop Mode of '{1}'", LooseDropModeType, LooseDropMode));
						}

						if (result.IsEmpty)
						{
							var dropMode = Cartage.IsLoose ? LooseDropMode : ContainerisedDropMode;
							var dropModeType = Cartage.IsLoose ? LooseDropModeType : ContainerisedDropModeType;
							result.Append(Res.GetString("cb30c59d-eb51-40e0-8c0d-6cd2d29eb061", "Set Cartage Drop Mode with the {0} Drop Mode '{1}'", dropModeType, dropMode));
						}
					}

					return result.IsEmpty ? null : result.ToStringWithNewLineBetweenAppends();
				}
			}

			public ZString LooseDropMode
			{
				get
				{
					if (!looseDropMode.HasValue)
					{
						looseDropMode = GetLooseDropMode();
					}

					return looseDropMode.Value;
				}
			}

			ZString? looseDropMode;
			protected abstract ZString GetLooseDropMode();

			internal ZBool IsMovesLooseDropModeDifferent
			{
				get
				{
					if (!isMovesLooseDropModeDifferent.HasValue)
					{
						isMovesLooseDropModeDifferent = Cartage.IsLoose && GetIsMovesLooseDropModeDifferent();
					}

					return isMovesLooseDropModeDifferent.Value;
				}
			}

			ZBool? isMovesLooseDropModeDifferent;
			protected abstract ZBool GetIsMovesLooseDropModeDifferent();

			public ZString ContainerisedDropMode
			{
				get
				{
					if (!containerisedDropMode.HasValue)
					{
						containerisedDropMode = GetContainerisedDropMode();
					}

					return containerisedDropMode.Value;
				}
			}

			ZString? containerisedDropMode;
			protected abstract ZString GetContainerisedDropMode();

			internal ZBool IsMovesContainerisedDropModeDifferent
			{
				get
				{
					if (!isMovesContainerisedDropModeDifferent.HasValue)
					{
						isMovesContainerisedDropModeDifferent = Cartage.IsContainerised && GetIsMovesContainerisedDropModeDifferent();
					}

					return isMovesContainerisedDropModeDifferent.Value;
				}
			}

			ZBool? isMovesContainerisedDropModeDifferent;
			protected abstract ZBool GetIsMovesContainerisedDropModeDifferent();

			protected abstract ZString ContainerisedDropModeType { get; }
			protected abstract ZString LooseDropModeType { get; }
		}

		class AddressDefaultingHelper : DefaultingHelper
		{
			internal AddressDefaultingHelper(CommonCartage cartage)
				: base(cartage)
			{ }

			protected override ZString GetLooseDropMode()
			{
				ZString result = "";

				if (HasAddress(RelevantLooseDocAddress))
				{
					result = Cartage.IsAir ? RelevantLooseDocAddress.Address.OA_AIREquipmentNeeded : RelevantLooseDocAddress.Address.OA_LCLEquipmentNeeded;
				}

				return result;
			}

			protected override ZString GetContainerisedDropMode()
			{
				return HasAddress(RelevantContainerisedDocAddress) ? RelevantContainerisedDocAddress.Address.OA_FCLEquipmentNeeded : ZString.Empty;
			}

			bool HasAddress(JobDocAddress relevantDocAddress)
			{
				return relevantDocAddress != null && relevantDocAddress.Address != null;
			}

			protected override ZBool GetIsMovesContainerisedDropModeDifferent()
			{
				return Cartage.Containers.Any()
				&& !ContainerisedDropMode.IsEmpty
				&& Cartage.HasContainerisedMoveWithAddressType(RelevantContainerisedDocAddress.DocAddressType)
				&& Cartage.HasContainerizedMovesToUpdate(ContainerisedDropMode);
			}

			protected override ZBool GetIsMovesLooseDropModeDifferent()
			{
				return Cartage.LooseBookedMoves.Any()
				&& !LooseDropMode.IsEmpty
				&& Cartage.HasLooseMoveWithAddressType(RelevantLooseDocAddress.DocAddressType)
				&& Cartage.HasLooseMovesToUpdate(LooseDropMode);
			}

			protected override ZString ContainerisedDropModeType
			{
				get { return RelevantContainerisedDocAddress.AddressCaption; }
			}

			protected override ZString LooseDropModeType
			{
				get { return RelevantLooseDocAddress.AddressCaption; }
			}

			JobDocAddress RelevantLooseDocAddress
			{
				get { return relevantLooseDocAddress ?? (relevantLooseDocAddress = Cartage.GetJobDocAddress(Cartage.RelevantLooseDocAddressType)); }
			}

			JobDocAddress relevantLooseDocAddress;

			JobDocAddress RelevantContainerisedDocAddress
			{
				get { return relevantContainerisedDocAddress ?? (relevantContainerisedDocAddress = Cartage.GetJobDocAddress(Cartage.RelevantContainerisedDocAddressType)); }
			}

			JobDocAddress relevantContainerisedDocAddress;
		}

		class JobTypeDefaultingHelper : DefaultingHelper
		{
			internal JobTypeDefaultingHelper(CommonCartage cartage)
				: base(cartage)
			{ }

			protected override ZString GetLooseDropMode()
			{
				return Cartage.CartageTypeLooseDropMode;
			}

			protected override ZString GetContainerisedDropMode()
			{
				return Cartage.CartageTypeContainerisedDropMode;
			}

			protected override ZBool GetIsMovesContainerisedDropModeDifferent()
			{
				return Cartage.Containers.Any() && !ContainerisedDropMode.IsEmpty && Cartage.HasContainerizedMovesToUpdate(ContainerisedDropMode);
			}

			protected override ZBool GetIsMovesLooseDropModeDifferent()
			{
				return Cartage.LooseBookedMoves.Count > 0 && !LooseDropMode.IsEmpty && Cartage.HasLooseMovesToUpdate(LooseDropMode);
			}

			protected override ZString ContainerisedDropModeType
			{
				get { return DropModeAddressType; }
			}

			protected override ZString LooseDropModeType
			{
				get { return DropModeAddressType; }
			}

			ZString DropModeAddressType
			{
				get { return Res.GetString("c2d41dcc-908b-4efa-93e3-750a4bfb1978", "Job Type"); }
			}
		}

		void DefaultDropModeOnJobTypeChange()
		{
			if (!IsImportingData && CartageType != null)
			{
				if (JJ_DropMode.IsEmpty)
				{
					SetCartageDropModeWithOutSettingMoves();
					QueryUserToSetMoves();
				}
				else
				{
					var addressHelper = new AddressDefaultingHelper(this);
					var jobTypeHelper = new JobTypeDefaultingHelper(this);
					if (addressHelper.DropModeIsDifferent || jobTypeHelper.DropModeIsDifferent)
					{
						var message = Res.GetString("442dc898-de4b-4a5d-aa70-132e851ebc36",
							"The Address and/or Job Type Drop Modes are different from the current Drop Mode. Would you like to keep the current Cartage Drop Mode of '{0}' or default to a new Drop Mode?", JJ_DropMode);
						var dropModeEventArgs = new QueryUserCartageTypeDropModeEventArgs(message, addressHelper.Text, jobTypeHelper.Text);

						NotificationsQueryUser(dropModeEventArgs);
						if (dropModeEventArgs.Response != DropMode.None)
						{
							var helper = dropModeEventArgs.Response == DropMode.CartageType ? jobTypeHelper : (DefaultingHelper)addressHelper;
							SetCartageDropModeWithOutSettingMoves(dropModeEventArgs.Response);
							SetLooseAndContainerisedMovesDropModes(helper);
						}
					}
				}
			}
		}

		void SetLooseAndContainerisedMovesDropModes(DefaultingHelper defaultingHelper)
		{
			if (defaultingHelper.IsMovesLooseDropModeDifferent)
			{
				SetLooseMovesDropMode(defaultingHelper.LooseDropMode);
			}

			if (defaultingHelper.IsMovesContainerisedDropModeDifferent)
			{
				SetContainerisedMovesDropMode(defaultingHelper.ContainerisedDropMode);
			}
		}

		void SetLooseMovesDropMode(ZString dropMode)
		{
			if (!dropMode.IsEmpty)
			{
				foreach (var move in LooseBookedMoves)
				{
					if (move.EW_DropMode != dropMode)
					{
						move.EW_DropMode = dropMode;
					}
				}
			}
		}

		void SetContainerisedMovesDropMode(ZString dropMode)
		{
			if (!dropMode.IsEmpty)
			{
				foreach (var container in Containers)
				{
					foreach (var move in GetBookedMoves(container))
					{
						if (move.EW_DropMode != dropMode)
						{
							move.EW_DropMode = dropMode;
						}
					}
				}
			}
		}

		protected ZBool JJ_E3_NKJobType_ReadOnly
		{
			get { return HasParent; }
		}

		public CodeDescriptionPairList JobTypeList
		{
			get { return BehaviorStrategy.GetJobTypeList(this); }
		}

		public override ZGuid JJ_JX_Sailing
		{
			get { return base.JJ_JX_Sailing; }
			set
			{
				if (base.JJ_JX_Sailing == value)
				{
					base.JJ_JX_Sailing = value;
				}
				else
				{
					base.JJ_JX_Sailing = value;

					if (!JJ_JX_Sailing.IsEmpty)
					{
						BehaviorStrategy.AfterSailingPKChange(this);
					}
				}
			}
		}

		[ReadOnly(true)]
		public override ZString JJ_ConsignmentID
		{
			get { return base.JJ_ConsignmentID; }
			set { base.JJ_ConsignmentID = value; }
		}

		public void UpdateJobNumForRelatedJobs()
		{
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, PK);
			query.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobCartageSchema.Constants.Prefix);

			var jobs = Factory.Load<JobHeader>(query);
			if (jobs != null && jobs.Length > 0)
			{
				foreach (var jobHeader in jobs)
				{
					jobHeader.JH_JobNum = JJ_ConsignmentID.SubstringSafe(0, jobHeader.JH_JobNumInfo.MaxLength);
				}
			}
		}

		[ReadOnlyMember(nameof(HasParentOperationalJob))]
		public override ZString JJ_WaybillNumber
		{
			get { return base.JJ_WaybillNumber; }
			set { base.JJ_WaybillNumber = value; }
		}

		[ReadOnlyMember(nameof(HasParentOperationalJob))]
		public override ZInt JJ_OuterPacks
		{
			get { return base.JJ_OuterPacks; }
			set
			{
				base.JJ_OuterPacks = value;
				SetupDefaultLooseBookedMove();
			}
		}

		[List("Lookups.OuterPackTypes")]
		[ReadOnlyMember(nameof(HasParentOperationalJob))]
		public override ZString JJ_F3_NKPackType
		{
			[DebuggerStepThrough()]
			get { return base.JJ_F3_NKPackType; }
			set
			{
				base.JJ_F3_NKPackType = value;
				SetupDefaultLooseBookedMove();
			}
		}

		[ReadOnly(true)]
		public ZDecimal GrossWeight
		{
			get { return GetGrossWeight(); }
		}

		public ZPropertyInfo GrossWeightInfo
		{
			get { return GetZPropertyInfo(Schema.GrossWeight); }
		}

		[ReadOnlyMember(nameof(HasParentOperationalJob))]
		[MeasureUnit(AutoJobCartage.Schema.JJ_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal JJ_Weight
		{
			[DebuggerStepThrough()]
			get { return base.JJ_Weight; }
			set
			{
				base.JJ_Weight = value;
				SetupDefaultLooseBookedMove();
			}
		}

		[List("Lookups.WeightUnits")]
		[ReadOnlyMember(nameof(HasParentOperationalJob))]
		public override ZString JJ_WeightUQ
		{
			[DebuggerStepThrough()]
			get { return base.JJ_WeightUQ; }
			set
			{
				base.JJ_WeightUQ = value;
				SetupDefaultLooseBookedMove();
			}
		}

		[ReadOnlyMember(nameof(HasParentOperationalJob))]
		[MeasureUnit(AutoJobCartage.Schema.JJ_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal JJ_Volume
		{
			[DebuggerStepThrough()]
			get { return base.JJ_Volume; }
			set
			{
				base.JJ_Volume = value;
				SetupDefaultLooseBookedMove();
			}
		}

		[List("Lookups.VolumeUnits")]
		[ReadOnlyMember(nameof(HasParentOperationalJob))]
		public override ZString JJ_VolumeUQ
		{
			[DebuggerStepThrough()]
			get { return base.JJ_VolumeUQ; }
			set
			{
				base.JJ_VolumeUQ = value;
				SetupDefaultLooseBookedMove();
			}
		}

		public override ZDateTime JJ_EstimatedPickup
		{
			get { return base.JJ_EstimatedPickup; }
			set
			{
				ZDateTime originalValue = base.JJ_EstimatedPickup;
				base.JJ_EstimatedPickup = value;
				BehaviorStrategy.EstimatedPickupChanged(this, originalValue);
			}
		}

		public override ZDateTime JJ_EstimatedDelivery
		{
			get { return base.JJ_EstimatedDelivery; }
			set
			{
				ZDateTime originalValue = base.JJ_EstimatedDelivery;
				base.JJ_EstimatedDelivery = value;
				BehaviorStrategy.EstimatedDeliveryChanged(this, originalValue);
			}
		}

		[List("BindToLists+ShippingTransportModeList")]
		public override ZString JJ_ShippingTransportMode
		{
			get { return base.JJ_ShippingTransportMode; }
			set
			{
				var oldTransportMode = JJ_ShippingTransportMode;

				base.JJ_ShippingTransportMode = value;

				if (oldTransportMode != JJ_ShippingTransportMode)
				{
					BehaviorStrategy.AfterTransportModeChange(this);
				}
			}
		}

		[List("BindToLists+CartageContainerModes")]
		public override ZString JJ_ContainerMode
		{
			get { return base.JJ_ContainerMode; }
			set
			{
				var oldValue = JJ_ContainerMode;
				var newValue = value;
				var canProceed = true;

				if (oldValue != newValue)
				{
					canProceed = QueryUser(newValue);
				}

				if (canProceed)
				{
					if (oldValue != newValue)
					{
						RemoveMovesAndLegsUnsupportedByContainerMode(newValue);
					}

					base.JJ_ContainerMode = newValue;
				}
			}
		}

		void RemoveMovesAndLegsUnsupportedByContainerMode(ZString newJJ_ContainerMode)
		{
			CommonBookedCtgMove[] movesToDelete;
			switch (newJJ_ContainerMode)
			{
				case Constants.ContainerModes.Containerised:
					movesToDelete = LooseBookedMoves.ToArray();
					break;
				case Constants.ContainerModes.Loose:
					movesToDelete = ContainerBookedMoves.ToArray();
					break;
				default:
					movesToDelete = Array.Empty<CommonBookedCtgMove>();
					break;
			}

			foreach (var move in movesToDelete)
			{
				move.Delete();
			}
		}

		bool QueryUser(ZString newValue)
		{
			var returnValue = true;

			if (!IsImportingData &&
				((newValue == Constants.CartageContainerMode.Containerized && LooseBookedMoves.Any(m => m.EW_BookedPackCount != 0)
				|| (newValue == Constants.CartageContainerMode.Loose && ContainerBookedMoves.Any(c => !string.IsNullOrWhiteSpace(c.Container.JC_ContainerNum)))
				)))
			{
				var looseMovementsString = Res.GetString("7288FF7C-9379-4FFE-8E9C-F04EB693A56E", "Loose Movements");
				var containersString = Res.GetString("26A3D399-CC2D-44E1-996E-26103EFB0623", "Containers");
				var message = Res.GetString("D5EF0AB2-66C6-4A57-A0A7-F5E926EAB80F", "Changing the Container Mode to '{0}' will remove the {1} from this Job. Proceed?", newValue, newValue == Constants.CartageContainerMode.Containerized ? looseMovementsString : containersString);
				var caption = Res.GetString("EB803E17-90CA-410A-B998-584CCF58F456", "Warning!");
				var e = new QueryUserYesNoEventArgs(caption, message, false);

				NotificationsQueryUser(e);

				returnValue = e.Response;
			}
			return returnValue;
		}

		[List("BindToLists+Directions")]
		public override ZString JJ_Direction
		{
			get { return base.JJ_Direction; }
			set { base.JJ_Direction = value; }
		}

		void SetupDefaultLooseBookedMove()
		{
			if (!IsUpdatingCartageFromBookedMoves.IsSuspended && !IsImportingData)
			{
				SetupDefaultLooseBookedMoveFromCartageDetails();
			}
		}

		void SetupDefaultLooseBookedMoveFromCartageDetails()
		{
			if (CartageType != null && CartageType.IsLoose)
			{
				if (LooseBookedMoves.Count == 0)
				{
					LooseBookedMoves.AddNew();
				}

				if (LooseBookedMoves.Count == 1)
				{
					CommonBookedCtgMove move = LooseBookedMoves[0];
					move.EW_BookedPackCount = JJ_OuterPacks;
					move.EW_F3_NKPackType = CartagePackageUnit;
					move.EW_BookedWeight = JJ_Weight;
					move.EW_WeightUQ = CartageWeightUnit;
					move.EW_BookedVolume = JJ_Volume;
					move.EW_VolumeUQ = CartageVolumeUnit;
				}
			}
		}

		public void SetAutoLogOverride()
		{
			ShouldCreateAutoLogIfOnlyChildrenHaveChangesOverride = true;
		}

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges
		{
			get { return ShouldCreateAutoLogIfOnlyChildrenHaveChangesOverride; }
		}
		bool ShouldCreateAutoLogIfOnlyChildrenHaveChangesOverride;

		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges
		{
			get { return ShouldCreateAutoLogIfOnlyChildrenHaveChangesOverride; }
		}

		public ZBool IsRoot
		{
			get { return isRoot; }
			set
			{
				if (isRoot != value)
				{
					isRoot = value;

					UpdateRegisteredEditable(looseBookedMoves);
					UpdateRegisteredEditable(containerBookedMoves);
					UpdateRegisteredEditable(cartageLegs);
				}
			}
		}
		ZBool isRoot;

		void UpdateRegisteredEditable(IBusiness register)
		{
			if (register != null)
			{
				if (IsRoot)
				{
					RegisterEditableChildObject(register);
				}
				else
				{
					UnRegisterEditableChildObject(register);
				}
			}
		}

		public override void RegisterEditableChildObject(IBusiness child)
		{
			base.RegisterEditableChildObject(child);

			if (child is CommonCartageLegCollection legs)
			{
				foreach (var leg in legs)
				{
					leg.IsRoot = false;
				}
			}
		}

		public override void UnRegisterEditableChildObject(IBusiness child)
		{
			base.UnRegisterEditableChildObject(child);

			if (child is CommonCartageLegCollection legs)
			{
				foreach (var leg in legs)
				{
					leg.IsRoot = true;
				}
			}
		}

		public JobHeader Job
		{
			get
			{
				if (job == null || job.IsDeleted)
				{
					job = new JobHeader.Loader(this).Load(true, false);
					RegisterEditableChildObject(job);
				}
				return job;
			}
		}
		JobHeader job;

		IJobHeader Integration.ICommonCartage.Job
		{
			get { return Job; }
		}

		public OrgHeader LocalClient
		{
			get
			{
				var jobHeader = Job;
				return jobHeader != null ? jobHeader.LocalCharges : null;
			}
		}

		public ZGuid LocalClientPK
		{
			get
			{
				var localClient = LocalClient;
				return localClient != null ? localClient.PK : ZGuid.Empty;
			}
			set
			{
				var jobHeader = Job;
				if (jobHeader != null)
				{
					jobHeader.LocalChargesPK = value;
				}

				LocalClientAddressPKInfo.RefreshBinding();
			}
		}

		public ZAddress LocalClientAddressPK_ZAddress
		{
			get
			{
				ZAddress result = new ZAddress(LocalClientAddressPKInfo);
				result.DefaultAddressType = AddressType.ARM;
				result.GetDefaultAddress = JobHeader.GetDefaultAddressByHeaderInCommonLanguage;
				return result;
			}
		}

		public OrgAddress LocalClientAddress
		{
			get
			{
				var jobHeader = Job;
				return jobHeader != null ? jobHeader.LocalChargesAddr : null;
			}
		}

		[RelatedBusinessObject("LocalClientAddress")]
		[List("LocalClientList")]
		public ZGuid LocalClientAddressPK
		{
			get
			{
				var jobHeader = Job;
				return jobHeader != null ? jobHeader.JH_OA_LocalChargesAddr : ZGuid.Empty;
			}
			set
			{
				var jobHeader = Job;
				if (jobHeader != null)
				{
					jobHeader.JH_OA_LocalChargesAddr = value;
				}

				LocalClientAddressPKInfo.RefreshBinding();
			}
		}

		public OrgHeaderCollection LocalClientList
		{
			get
			{
				var jobHeader = Job;
				if (jobHeader != null)
				{
					return jobHeader.Lookups.LocalCharges;
				}
				else
				{
					return new OrgHeaderCollection(Factory);
				}
			}
		}

		protected bool LocalClientPK_ReadOnly
		{
			get { return Job == null; }
		}

		public ZPropertyInfo LocalClientAddressPKInfo
		{
			get
			{
				if (Job == null)
				{
					return GetZPropertyInfo(Schema.LocalClientAddressPK);
				}
				else
				{
					return GetWrappedZPropertyInfo(Schema.LocalClientAddressPK, x => Job.JH_OA_LocalChargesAddrInfo);
				}
			}
		}

		[BusinessObjectTestExclude]
		public ZGuid JJ_ForeignKeyToConsol
		{
			get
			{
				if (JJ_ParentTableCode == JobConsolSchema.Constants.Prefix)
				{
					return JJ_ParentID;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
			set
			{
				JJ_ParentTableCode = !value.IsEmpty ? JobConsolSchema.Constants.Prefix : string.Empty;
				JJ_ParentID = value;
			}
		}

		public static ZQuery ActiveFilter
		{
			get { return new ZQuery(JobCartageSchema.JJ_IsCancelled, ZBool.False); }
		}

		public ZString CartageWeightUnit
		{
			get { return FreightUtilities.IsValidWeightUnit(JJ_WeightUQ) ? JJ_WeightUQ.ToString() : Env.Registry.FreightWeightUnit; }
		}

		public ZString CartageVolumeUnit
		{
			get { return FreightUtilities.IsValidVolumeUnit(JJ_VolumeUQ) ? JJ_VolumeUQ.ToString() : Env.Registry.FreightVolumeUnit; }
		}

		public ZString CartagePackageUnit
		{
			get { return !JJ_F3_NKPackType.IsEmpty ? JJ_F3_NKPackType.ToString() : FreightPacksDataRegistry.Instance.OuterPackUnit.Value; }
		}

		public ZInt TotalLooseBookedPackages
		{
			get
			{
				ZInt result = 0;
				foreach (CommonBookedCtgMove move in LooseBookedMoves)
				{
					result += move.EW_BookedPackCount;
				}
				return result;
			}
		}

		public ZString TotalLooseBookedPackagesUnit
		{
			get
			{
				ZString result = "";
				foreach (CommonBookedCtgMove move in LooseBookedMoves)
				{
					if (result.IsEmpty)
					{
						result = move.EW_F3_NKPackType;
					}
					else if (result != move.EW_F3_NKPackType)
					{
						result = Core.Constants.PkgUnit.Piece;
						break;
					}
				}
				return result.IsEmpty ? FreightPacksDataRegistry.Instance.OuterPackUnit.Value : result.ToString();
			}
		}

		public ZDecimal TotalLooseBookedWeight
		{
			get
			{
				ZDecimal result = 0;
				var bookedWeightUnit = CalculateLooseBookedWeightUnit();
				foreach (CommonBookedCtgMove move in LooseBookedMoves)
				{
					var sourceUnit = FreightUtilities.IsValidWeightUnit(move.EW_WeightUQ) ? move.EW_WeightUQ.ToString() : Env.Registry.FreightWeightUnit;
					result += Core.Constants.Weight.ConvertSafe(move.EW_BookedWeight, sourceUnit, bookedWeightUnit);
				}
				return Utilities.Round(result, JobBookedCtgMoveSchema.EW_BookedWeight.Scale);
			}
		}

		public ZString TotalLooseBookedWeightUnit
		{
			get
			{
				return CalculateLooseBookedWeightUnit();
			}
		}

		ZString CalculateLooseBookedWeightUnit()
		{
			ZString result = "";
			foreach (CommonBookedCtgMove move in LooseBookedMoves)
			{
				if (result.IsEmpty)
				{
					result = move.EW_WeightUQ;
				}
				else if (result != move.EW_WeightUQ)
				{
					result = CartageWeightUnit;
					break;
				}
			}
			return result.IsEmpty ? CartageWeightUnit : result;
		}

		public ZDecimal TotalLooseBookedVolume
		{
			get
			{
				ZDecimal result = 0;
				var bookedVolumeUnit = CalculateTotalLooseBookedVolumeUnit();
				foreach (CommonBookedCtgMove move in LooseBookedMoves)
				{
					var sourceUnit = FreightUtilities.IsValidVolumeUnit(move.EW_VolumeUQ) ? move.EW_VolumeUQ.ToString() : Env.Registry.FreightVolumeUnit;
					result += Core.Constants.Volume.ConvertSafe(move.EW_BookedVolume, sourceUnit, bookedVolumeUnit);
				}
				return Utilities.Round(result, JobBookedCtgMoveSchema.EW_BookedVolume.Scale);
			}
		}

		public ZString TotalLooseBookedVolumeUnit
		{
			get
			{
				return CalculateTotalLooseBookedVolumeUnit();
			}
		}

		ZString CalculateTotalLooseBookedVolumeUnit()
		{
			ZString result = "";
			foreach (CommonBookedCtgMove move in LooseBookedMoves)
			{
				if (result.IsEmpty)
				{
					result = move.EW_VolumeUQ;
				}
				else if (result != move.EW_VolumeUQ)
				{
					result = CartageVolumeUnit;
					break;
				}
			}
			return result.IsEmpty ? CartageVolumeUnit : result;
		}

		[BusinessObjectTestExclude()]
		[List("BindToLists.RefUNLOCOs")]
		[MaxLength(VoyageOrigin.Schema.JA_RL_NKPortOfLoadingMaxLength)]
		[ReadOnlyMember(nameof(HasParentOrParentJobThatIsNotAnExternalTransportBooking))]
		public ZString PortOfLoading
		{
			get { return BehaviorStrategy.GetPortOfLoading(this); }
			set { BehaviorStrategy.SetPortOfLoading(this, value); }
		}

		public ZPropertyInfo PortOfLoadingInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfLoading); }
		}

		[BusinessObjectTestExclude()]
		[List("BindToLists.RefUNLOCOs")]
		[MaxLength(VoyageDestination.Schema.JB_RL_NKPortOfDischargeMaxLength)]
		[ReadOnlyMember(nameof(HasParentOrParentJobThatIsNotAnExternalTransportBooking))]
		public ZString PortOfDischarge
		{
			get { return BehaviorStrategy.GetPortOfDischarge(this); }
			set { BehaviorStrategy.SetPortOfDischarge(this, value); }
		}

		public ZPropertyInfo PortOfDischargeInfo
		{
			get { return GetZPropertyInfo(Schema.PortOfDischarge); }
		}

		[ResourceStringData("CommonCartage|Vessel", Caption = "Vessel", FullDescription = "Schedule Vessel.")]
		[BusinessObjectTestExclude()]
		[List("BindToLists.RefVessels")]
		[MaxLength(JobVoyage.Schema.JV_RV_NKVesselMaxLength)]
		[ReadOnlyMember(nameof(HasParentOrParentJobThatIsNotAnExternalTransportBooking))]
		public ZString Vessel
		{
			get { return BehaviorStrategy.GetVessel(this); }
			set { BehaviorStrategy.SetVessel(this, value); }
		}

		public ZPropertyInfo VesselInfo
		{
			get { return GetZPropertyInfo(Schema.Vessel); }
		}

		[ResourceStringData("CommonCartage|VoyageFlight", ShortCaption = "Voy./Flight", Caption = "Voyage/Flight", FullDescription = "Schedule Voyage/Flight.")]
		[BusinessObjectTestExclude()]
		[MaxLength(JobVoyage.Schema.JV_VoyageFlightMaxLength)]
		[ReadOnlyMember(nameof(HasParentOrParentJobThatIsNotAnExternalTransportBooking))]
		public ZString VoyageFlight
		{
			get { return BehaviorStrategy.GetVoyageFlight(this); }
			set { BehaviorStrategy.SetVoyageFlight(this, value); }
		}

		public ZPropertyInfo VoyageFlightInfo
		{
			get { return GetZPropertyInfo(Schema.VoyageFlight); }
		}

		[ResourceStringData("CommonCartage|E_DEP", ShortCaption = "ETD", Caption = "Schedule ETD", FullDescription = "Schedule Estimated Time of Departure.")]
		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(HasParentOrParentJobThatIsNotAnExternalTransportBooking))]
		public ZDateTime E_DEP
		{
			get { return BehaviorStrategy.GetE_DEP(this); }
			set { BehaviorStrategy.SetE_DEP(this, value); }
		}

		public ZPropertyInfo E_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.E_DEP); }
		}

		[ResourceStringData("CommonCartage|E_ARV", ShortCaption = "ETA", Caption = "Schedule ETA", FullDescription = "Schedule Estimated Time of Arrival.")]
		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(HasParentOrParentJobThatIsNotAnExternalTransportBooking))]
		public ZDateTime E_ARV
		{
			get { return BehaviorStrategy.GetE_ARV(this); }
			set { BehaviorStrategy.SetE_ARV(this, value); }
		}

		public ZPropertyInfo E_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.E_ARV); }
		}

		[ResourceStringData("CommonCartage|A_DEP", ShortCaption = "ATD", Caption = "Schedule ATD", FullDescription = "Schedule Actual Time of Departure.")]
		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(HasParent))]
		public ZDateTime A_DEP
		{
			get { return BehaviorStrategy.GetA_DEP(this); }
		}

		public ZPropertyInfo A_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.A_DEP); }
		}

		[ResourceStringData("CommonCartage|A_ARV", ShortCaption = "ATA", Caption = "Schedule ATA", FullDescription = "Schedule Actual Time of Arrival.")]
		[BusinessObjectTestExclude()]
		[ReadOnlyMember(nameof(HasParent))]
		public ZDateTime A_ARV
		{
			get { return BehaviorStrategy.GetA_ARV(this); }
		}

		public ZPropertyInfo A_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.A_ARV); }
		}

		[ResourceStringData("CommonCartage|CTOCutOff", Caption = "CTO Cutoff", FullDescription = "Schedule CTO Cutoff.")]
		[BusinessObjectTestExclude()]
		public ZDateTime FCLCutOff
		{
			get { return BehaviorStrategy.GetFCLCutOff(this); }
			set { BehaviorStrategy.SetFCLCutOff(this, value); }
		}

		public ZPropertyInfo FCLCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.FCLCutOff); }
		}

		protected bool FCLCutOff_ReadOnly
		{
			get { return HasParentOrParentJobThatIsNotAnExternalTransportBooking || SailingStandalone == null; }
		}

		[ResourceStringData("CommonCartage|CFSCutOff", Caption = "CFS Cutoff")]
		[BusinessObjectTestExclude()]
		public ZDateTime LCLCutOff
		{
			get { return BehaviorStrategy.GetLCLCutOff(this); }
			set { BehaviorStrategy.SetLCLCutOff(this, value); }
		}

		public ZPropertyInfo LCLCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.LCLCutOff); }
		}

		protected bool LCLCutOff_ReadOnly
		{
			get { return HasParentOrParentJobThatIsNotAnExternalTransportBooking || SailingStandalone == null; }
		}

		[ResourceStringData("CommonCartage|CTOReceivalCommences", ShortCaption = "CTO Recv.", MediumCaption = "CTO Receival", Caption = "CTO Receival Start", FullDescription = "Schedule CTO Receival Start.")]
		[BusinessObjectTestExclude()]
		public ZDateTime FCLReceivalCommences
		{
			get { return BehaviorStrategy.GetFCLReceivalCommences(this); }
			set { BehaviorStrategy.SetFCLReceivalCommences(this, value); }
		}

		public ZPropertyInfo FCLReceivalCommencesInfo
		{
			get { return GetZPropertyInfo(Schema.FCLReceivalCommences); }
		}

		protected bool FCLReceivalCommences_ReadOnly
		{
			get { return HasParentOrParentJobThatIsNotAnExternalTransportBooking || SailingStandalone == null; }
		}

		[ResourceStringData("CommonCartage|CFSReceivalCommences", ShortCaption = "CFS Recv.", MediumCaption = "CFS Receival", Caption = "CFS Receival Start")]
		[BusinessObjectTestExclude()]
		public ZDateTime LCLReceivalCommences
		{
			get { return BehaviorStrategy.GetLCLReceivalCommences(this); }
			set { BehaviorStrategy.SetLCLReceivalCommences(this, value); }
		}

		public ZPropertyInfo LCLReceivalCommencesInfo
		{
			get { return GetZPropertyInfo(Schema.LCLReceivalCommences); }
		}

		protected bool LCLReceivalCommences_ReadOnly
		{
			get { return HasParentOrParentJobThatIsNotAnExternalTransportBooking || SailingStandalone == null; }
		}

		[ResourceStringData("CommonCartage|CTOAvailabilityDate", ShortCaption = "CTO Avail.", Caption = "CTO Availability", FullDescription = "Schedule CTO Availability Date.")]
		[BusinessObjectTestExclude()]
		public ZDateTime FCLAvailabilityDate
		{
			get { return BehaviorStrategy.GetFCLAvailabilityDate(this); }
			set { BehaviorStrategy.SetFCLAvailabilityDate(this, value); }
		}

		public ZPropertyInfo FCLAvailabilityDateInfo
		{
			get { return GetZPropertyInfo(Schema.FCLAvailabilityDate); }
		}

		protected bool FCLAvailabilityDate_ReadOnly
		{
			get { return HasParentOrParentJobThatIsNotAnExternalTransportBooking || SailingStandalone == null; }
		}

		[ResourceStringData("CommonCartage|CFSAvailabilityDate", ShortCaption = "CFS Avail.", MediumCaption = "CFS Availability", Caption = "CFS Availability Date", FullDescription = "Cartage CFS Availability Date.")]
		[BusinessObjectTestExclude()]
		public ZDateTime LCLAvailabilityDate
		{
			get { return BehaviorStrategy.GetLCLAvailabilityDate(this); }
			set { BehaviorStrategy.SetLCLAvailabilityDate(this, value); }
		}

		public ZPropertyInfo LCLAvailabilityDateInfo
		{
			get { return GetZPropertyInfo(Schema.LCLAvailabilityDate); }
		}

		protected bool LCLAvailabilityDate_ReadOnly
		{
			get { return HasParentOrParentJobThatIsNotAnExternalTransportBooking || SailingStandalone == null; }
		}

		[ResourceStringData("CommonCartage|CTOStorageDate", ShortCaption = "CTO Stor.", Caption = "CTO Storage", FullDescription = "Schedule CTO Storage Start Date.")]
		[BusinessObjectTestExclude()]
		public ZDateTime FCLStorageDate
		{
			get { return BehaviorStrategy.GetFCLStorageDate(this); }
			set { BehaviorStrategy.SetFCLStorageDate(this, value); }
		}

		public ZPropertyInfo FCLStorageDateInfo
		{
			get { return GetZPropertyInfo(Schema.FCLStorageDate); }
		}

		protected bool FCLStorageDate_ReadOnly
		{
			get { return HasParentOrParentJobThatIsNotAnExternalTransportBooking || SailingStandalone == null; }
		}

		[ResourceStringData("CommonCartage|CFSStorageDate", ShortCaption = "CFS Store", MediumCaption = "CFS Storage", Caption = "CFS Storage Date")]
		[BusinessObjectTestExclude()]
		public ZDateTime LCLStorageDate
		{
			get { return BehaviorStrategy.GetLCLStorageDate(this); }
			set { BehaviorStrategy.SetLCLStorageDate(this, value); }
		}

		public ZPropertyInfo LCLStorageDateInfo
		{
			get { return GetZPropertyInfo(Schema.LCLStorageDate); }
		}

		protected bool LCLStorageDate_ReadOnly
		{
			get { return HasParentOrParentJobThatIsNotAnExternalTransportBooking || SailingStandalone == null; }
		}

		/// <summary>
		/// Is Public because used in DocumentWrapper too
		/// </summary>
		[Obsolete("Use JJ_ShippingTransportMode")]
		public ZString CartageTypeTransportMode
		{
			get { return JJ_ShippingTransportMode; }
		}

		public ZBool IsAir
		{
			get { return JJ_ShippingTransportMode == Constants.TransportModes.Air; }
		}

		public ZBool IsSea
		{
			get { return JJ_ShippingTransportMode == Constants.TransportModes.Sea; }
		}

		public ZBool IsRoad
		{
			get { return JJ_ShippingTransportMode == Constants.TransportModes.Road; }
		}

		public ZBool IsRail
		{
			get { return JJ_ShippingTransportMode == Constants.TransportModes.Rail; }
		}

		public MasterFiles.Business.Directions JobDirection
		{
			get
			{
				return IsImport ? Enterprise.MasterFiles.Business.Directions.Import
						: IsExport ? Enterprise.MasterFiles.Business.Directions.Export
						: IsDomestic ? Enterprise.MasterFiles.Business.Directions.Domestic
						: Enterprise.MasterFiles.Business.Directions.Unknown;
			}
		}

		public ZBool IsImportOrDestination
		{
			get { return IsImport || IsDestination; }
		}

		public ZBool IsExportOrOrigin
		{
			get { return IsExport || IsOrigin; }
		}

		public ZBool IsImport
		{
			get { return JJ_Direction == Constants.CartageDirection.Import; }
		}

		public ZBool IsExport
		{
			get { return JJ_Direction == Constants.CartageDirection.Export; }
		}

		public ZBool IsDomestic
		{
			get { return JJ_Direction == Constants.CartageDirection.Local || JJ_Direction == Constants.CartageDirection.LineHaul || IsOrigin || IsDestination; }
		}

		public ZBool IsOrigin
		{
			get { return JJ_Direction == Constants.CartageDirection.Origin; }
		}

		public ZBool IsDestination
		{
			get { return JJ_Direction == Constants.CartageDirection.Destination; }
		}

		public ZBool IsMixed
		{
			get { return IsLoose && IsContainerised; }
		}

		public ZBool IsContainerised
		{
			get { return JJ_ContainerMode == Constants.ContainerModes.Containerised || JJ_ContainerMode == Constants.CartageContainerMode.Mixed; }
		}

		public ZBool IsLoose
		{
			get { return JJ_ContainerMode == Constants.ContainerModes.Loose || JJ_ContainerMode == Constants.CartageContainerMode.Mixed; }
		}

		public ZBool HasJobAlreadyCommenced
		{
			get
			{
				foreach (CommonCartageLeg leg in CartageLegs)
				{
					if (leg.HasLegAreadyCommenced)
					{
						return true;
					}
				}

				return false;
			}
		}

		public JobSailing SailingStandalone
		{
			get { return Factory.Load<JobSailing>(JJ_JX_Sailing); }
		}

		internal BaseSailingManager SailingManager
		{
			get
			{
				if (fSailingManager == null && !IsDeleted)
				{
					fSailingManager = BaseSailingManager.New(this);
				}
				return fSailingManager;
			}
		}
		internal BaseSailingManager fSailingManager;

		IJobSailing Integration.ICommonCartage.SailingStandalone
		{
			get { return SailingStandalone; }
		}

		[ResourceStringData("CommonCartage|ContainerNumber", Caption = "Containers", FullDescription = "Port Transport Job Containers.")]
		public ZString ContainerNumber
		{
			get
			{
				if (!Containers.Any())
				{
					return "";
				}
				else
				{
					return Containers.Count() == 1 ? Containers.First().JC_ContainerNum.ToString() : Res.GetString("3ec93bc5-9cb8-48d1-a0d6-b4a149c5de46", "Many");
				}
			}
		}

		public ZPropertyInfo ContainerNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerNumber); }
		}

		public ZString ContainersType
		{
			get
			{
				ZString result;

				var firstRefContainer = Containers.Any() ? Containers.First().RefContainer : null;
				if (firstRefContainer == null)
				{
					result = "";
				}
				else
				{
					result = Containers.All(c => c.JC_RC.IsValid && c.JC_RC == firstRefContainer.PK)
						? firstRefContainer.RC_Code
						: (ZString)Res.GetString("3ec93bc5-9cb8-48d1-a0d6-b4a149c5de46", "Many");
				}

				return result;
			}
		}

		public OrgHeader LocalTransportProvider
		{
			get
			{
				return LocalTransportProviderAddress != null ? LocalTransportProviderAddress.Header : null;
			}
		}

		public OrgAddress LocalTransportProviderAddress
		{
			get
			{
				OrgAddress result;

				if (CartageInternalType != null)
				{
					result = CartageInternalType.LocalTransportProviderAddress;
				}
				else
				{
					OrgHeader orgProxy = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;
					result = orgProxy != null ? orgProxy.MainAddress : null;
				}

				return result;
			}
		}

		public bool ShouldReorderAddresses
		{
			get { return shouldReorderAddresses; }
		}

		bool shouldReorderAddresses;

		public void MarkAsHavingAddressesReordered()
		{
			shouldReorderAddresses = false;
		}

		public void MarkAsNeededAddressReorder()
		{
			shouldReorderAddresses = true;
		}

		public ZString TransportBookingPartyReference
		{
			get
			{
				var externalTB = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
				if (externalTB != null && !CurrentlyPublishingAnEventToTheOrgProxy)
				{
					return externalTB.CE_EntryNum;
				}
				else if (CurrentlyPublishingAnEventToTheOrgProxy && ParentBooking != null && !ParentBooking.KM_JobID.IsEmpty)
				{
					return ParentBooking.KM_JobID;
				}
				else
				{
					return JJ_OrderReferenceNumber;
				}
			}
		}

		internal bool CurrentlyPublishingAnEventToTheOrgProxy { get; set; }

		public ZString ClientReference
		{
			get
			{
				var internalTBReference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.TransportReference);
				return internalTBReference != null ? internalTBReference.CE_EntryNum : JJ_OrderReferenceNumber;
			}
		}

		public ViewJobCartageParents ParentView
		{
			get
			{
				return GetViewJobCartageParentRecord();
			}
		}

		public ZString ParentJobNumber
		{
			get
			{
				return GetViewJobCartageParentRecord()?.VCP_JobNumber ?? ZString.Empty;
			}
		}

		public ZString ParentJobType
		{
			get
			{
				return GetViewJobCartageParentRecord()?.VCP_JobType ?? ZString.Empty;
			}
		}

		public bool HasParentOperationalJob
		{
			get
			{
				var result = false;

				if (ParentJobType == CartageBindToLists.TransportBookingCode && ParentBooking != null)
				{
					var consol = Factory.Load<IDtbTransportConsolidation>(ParentBooking.KM_KB_Booking);

					if (consol != null && consol.KB_ParentID != ZGuid.Empty && consol.KB_ParentTableCode != DtbAgentBookingSchema.Constants.Prefix)
					{
						result = true;
					}
				}
				else if (ParentJobType != ZString.Empty && ParentJobType != CartageBindToLists.TransportBookingCode)
				{
					result = true;
				}

				return result;
			}
		}

		public bool HasParentOrParentJobThatIsNotAnExternalTransportBooking
		{
			get
			{
				return (ParentJob != null && !ParentJobIsExternalTransportBooking) || HasParent;
			}
		}

		bool ParentJobIsExternalTransportBooking
		{
			get
			{
				return ParentJobType == CartageBindToLists.TransportBookingCode && ((ITransportAdditionalReferenceNumbers)ParentJob).AdditionalReferenceNumbers.ToArray().Select(n => ((ICusEntryNumber)n).CE_EntryType).Contains(AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber);
			}
		}

		public BusinessObject ParentJob
		{
			get
			{
				switch (ParentJobType)
				{
					case CartageBindToLists.TransportBookingCode:
						return Factory.Load<IDtbBooking>(JJ_ParentID) as BusinessObject;

					case CartageBindToLists.ForwardingShipmentCode:
						return Factory.Load<IForwardingShipment>(JJ_ParentID) as BusinessObject;

					case CartageBindToLists.CFSShipmentCode:
						return Factory.Load<ICFSShipment>(JJ_ParentID) as BusinessObject;

					case CartageBindToLists.CFSLoadListConsolCode:
						return Factory.Load<ICFSLoadListConsol>(JJ_ParentID) as BusinessObject;

					case CartageBindToLists.WarehouseOrderCode:
						return Factory.Load<IWhsOrder>(JJ_ParentID) as BusinessObject;

					case CartageBindToLists.CustomsDeclarationCode:
						return Factory.Load<IBaseJobDeclaration>(JJ_ParentID) as BusinessObject;

					case "":
						return null;

					default:
						throw new InvalidOperationException("Invalid Parent Job Type " + ParentJobType);
				}
			}
		}

		ViewJobCartageParents GetViewJobCartageParentRecord()
		{
			return JJ_ParentID.IsEmpty ? null : Factory.Load<ViewJobCartageParents>(JJ_ParentID);
		}

		public void PickupCompleted(CommonCartageLeg leg, DocAddressType addressType, ZDateTime timeOutForLeg)
		{
			if (HasParent)
			{
				if (leg != null && leg.Container != null && CartageInternalType != null)
				{
					CartageInternalType.PickupCompleted(leg.Container, addressType, timeOutForLeg);
				}
				else
				{
					bool complete = true;
					foreach (CommonBookedCtgMove move in LooseBookedMoves)
					{
						foreach (CommonCartageLeg looseLeg in move.CartageLegs)
						{
							if (looseLeg.PickupDocAddressType == addressType && looseLeg.JU_PickupTimeOut.IsEmpty)
							{
								complete = false;
							}
						}
					}

					if (complete && CartageInternalType != null)
					{
						CartageInternalType.PickupCompleted(addressType, LastLegPickupTimeOut);
					}
				}
			}
		}

		public void DeliveryCompleted(CommonCartageLeg leg, DocAddressType addressType, ZDateTime timeOutForLeg)
		{
			if (HasParent)
			{
				if (leg != null && leg.Container != null && CartageInternalType != null)
				{
					CartageInternalType.DeliveryCompleted(leg.Container, addressType, timeOutForLeg);
				}
				else
				{
					bool complete = true;
					foreach (CommonBookedCtgMove move in LooseBookedMoves)
					{
						foreach (CommonCartageLeg looseLeg in move.CartageLegs)
						{
							if (looseLeg.DeliverToDocAddressType == addressType && looseLeg.JU_DeliverTimeOut.IsEmpty)
							{
								complete = false;
							}
						}
					}

					if (complete && CartageInternalType != null)
					{
						CartageInternalType.DeliveryCompleted(addressType, LastLegDeliveryTimeOut);
					}
				}
			}
		}

		public void CheckJobCompletion()
		{
			CheckJobCompletion(LastLegDeliveryTimeOut);
		}

		public void CheckJobCompletion(ZDateTime lastLegTimeOutBeforeChange)
		{
			if (AreAllContainerLegsComplete && (JJ_A_JCL.IsEmpty || JJ_A_JCL == lastLegTimeOutBeforeChange))
			{
				JJ_A_JCL = LastLegDeliveryTimeOut;
			}
			else if (JJ_A_JCL == lastLegTimeOutBeforeChange)
			{
				JJ_A_JCL = ZDateTime.Empty;
			}
		}

		public bool AreAllContainerLegsComplete
		{
			get
			{
				foreach (CommonCartageLeg leg in CartageLegs)
				{
					if (!leg.IsComplete && !leg.IsFutile)
					{
						return false;
					}
				}
				return true;
			}
		}

		public ZDateTime LastLegDeliveryTimeOut
		{
			get
			{
				var lastLegDeliveryTime = ZDateTime.Empty;
				foreach (CommonCartageLeg leg in CartageLegs)
				{
					if (!leg.IsFutile && leg.JU_DeliverTimeOut.IsValid && (lastLegDeliveryTime.IsEmpty || lastLegDeliveryTime < leg.JU_DeliverTimeOut))
					{
						lastLegDeliveryTime = leg.JU_DeliverTimeOut;
					}
				}
				return lastLegDeliveryTime;
			}
		}

		public ZDateTime LastLegPickupTimeOut
		{
			get
			{
				var lastLegPickupTime = ZDateTime.Empty;
				foreach (CommonCartageLeg leg in CartageLegs)
				{
					if (!leg.IsFutile && leg.JU_PickupTimeOut.IsValid && (lastLegPickupTime.IsEmpty || lastLegPickupTime < leg.JU_PickupTimeOut))
					{
						lastLegPickupTime = leg.JU_PickupTimeOut;
					}
				}
				return lastLegPickupTime;
			}
		}

		void PopulateJJ_ConsignmentIDIfNeeded()
		{
			PopulateFormattedNumberPropertyIfRequired(JJ_ConsignmentIDInfo, NumberFountainForUniqueConsignID);
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new JobCartageNumberFountainUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		protected class JobCartageNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public JobCartageNumberFountainUniqueIndexFailureHandler(CommonCartage cartage)
				: base(JobCartageSchema.Constants.Indexes.NR_UX__JJ_ConsignmentID_JJ_GB, cartage)
			{
				Cartage = cartage;
			}
			readonly CommonCartage Cartage;

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return Cartage.NumberFountainForUniqueConsignID; }
			}

			protected override DbCommand CommandToFindMaxValueInDatabase(DbConnection connection)
			{
				var sqlCommand = string.Format(CultureInfo.InvariantCulture, "SELECT substring(MAX({0}), 2, 8) From {1} Where {2} Is Null OR {3} = 'KM'",
					JobCartageSchema.Constants.JJ_ConsignmentID,
					JobCartageSchema.Constants.TableName,
					JobCartageSchema.Constants.JJ_ParentID,
					JobCartageSchema.Constants.JJ_ParentTableCode);

				return connection.Command(sqlCommand);
			}
		}

		protected INumberFountainProxy NumberFountainForUniqueConsignID
		{
			get { return Env.NumberFountains.JobCartageNumber; }
		}

		[ChildEditable()]
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

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			switch (docAddress.E2_AddressType)
			{
				case DocAddressTypes.Codes.LocalCartageCFS:
					return Env.Security.TransportJobMISCDetailsCFS;
				case DocAddressTypes.Codes.LocalCartageCTO:
					return Env.Security.TransportJobMISCDetailsCTO;
				case DocAddressTypes.Codes.LocalCartageExporter:
					return Env.Security.TransportJobMISCDetailsConsignor;
				case DocAddressTypes.Codes.LocalCartageImporter:
					return Env.Security.TransportJobMISCDetailsConsignee;
				case DocAddressTypes.Codes.LocalCartageYard:
					return Env.Security.TransportJobMISCDetailsContYard;
				case DocAddressTypes.Codes.LocalCartageService:
				case DocAddressTypes.Codes.LocalCartageMSC:
				default:
					return Env.Security.TransportJobMISCDetails;
			}
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.LocalCartageCFS,
					DocAddressType.LocalCartageCTO,
					DocAddressType.LocalCartageExporter,
					DocAddressType.LocalCartageImporter,
					DocAddressType.LocalCartageYard,
					DocAddressType.LocalCartageService,
					DocAddressType.LocalCartageMSC,
					DocAddressType.BookingPartyDocumentaryAddress,
					DocAddressType.ClientRequestedBillingParty,
				};
			}
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.LocalCartageCFS:
					return AddressRequirementCFS;
				case DocAddressType.LocalCartageCTO:
					return AddressRequirementCTO;
				case DocAddressType.LocalCartageExporter:
					return AddressRequirementExporter;
				case DocAddressType.LocalCartageImporter:
					return AddressRequirementImporter;
				case DocAddressType.LocalCartageYard:
					return AddressRequirementYard;
				case DocAddressType.LocalCartageService:
					return AddressRequirementService;
				case DocAddressType.LocalCartageWarehouse:
					return AddressRequirementWarehouse;
				case DocAddressType.LocalCartageMSC:
					return AddressRequirementMSC;
				default:
					return null;
			}
		}

		JobDocAddressRequirement AddressRequirementCFS
		{
			get { return addressRequirementCFS ?? (addressRequirementCFS = new JobDocAddressRequirement(DocAddressType.LocalCartageCFS, AddressType.PIC, ContactType.LocalTransport, true, 0)); }
		}
		JobDocAddressRequirement addressRequirementCFS;

		JobDocAddressRequirement AddressRequirementCTO
		{
			get { return addressRequirementCTO ?? (addressRequirementCTO = new JobDocAddressRequirement(DocAddressType.LocalCartageCTO, IsImportOrDestination ? AddressType.PIC : AddressType.DLV, ContactType.LocalTransport, true, 0)); }
		}
		JobDocAddressRequirement addressRequirementCTO;

		JobDocAddressRequirement AddressRequirementExporter
		{
			get { return addressRequirementExporter ?? (addressRequirementExporter = new JobDocAddressRequirement(DocAddressType.LocalCartageExporter, AddressType.PIC, ContactType.LocalTransport, true, 0)); }
		}
		JobDocAddressRequirement addressRequirementExporter;

		JobDocAddressRequirement AddressRequirementImporter
		{
			get { return addressRequirementImporter ?? (addressRequirementImporter = new JobDocAddressRequirement(DocAddressType.LocalCartageImporter, AddressType.DLV, ContactType.LocalTransport, true, 0)); }
		}
		JobDocAddressRequirement addressRequirementImporter;

		JobDocAddressRequirement AddressRequirementYard
		{
			get { return addressRequirementYard ?? (addressRequirementYard = new JobDocAddressRequirement(DocAddressType.LocalCartageYard, IsImportOrDestination ? AddressType.DLV : AddressType.PIC, ContactType.LocalTransport, true, 0)); }
		}
		JobDocAddressRequirement addressRequirementYard;

		JobDocAddressRequirement AddressRequirementService
		{
			get { return addressRequirementService ?? (addressRequirementService = new JobDocAddressRequirement(DocAddressType.LocalCartageService, AddressType.DLV, ContactType.LocalTransport, true, 0)); }
		}
		JobDocAddressRequirement addressRequirementService;

		JobDocAddressRequirement AddressRequirementWarehouse
		{
			get { return addressRequirementWarehouse ?? (addressRequirementWarehouse = new JobDocAddressRequirement(DocAddressType.LocalCartageWarehouse, AddressType.DLV, ContactType.LocalTransport, true, 0)); }
		}
		JobDocAddressRequirement addressRequirementWarehouse;

		JobDocAddressRequirement AddressRequirementMSC
		{
			get { return addressRequirementMSC ?? (addressRequirementMSC = new JobDocAddressRequirement(DocAddressType.LocalCartageMSC, AddressType.PIC, ContactType.LocalTransport, true, 0)); }
		}
		JobDocAddressRequirement addressRequirementMSC;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
			// UI Refresh Issue only, see WI-WI00030674
			// Do not use RefreshBindingIncludingChildren(), it refreshes collections and we get an index change, see WI00031633

			DefaultDropModeIfAllowed(docAddress);

			foreach (var move in BookedMovesCollection)
			{
				move.RefreshBinding();
			}

			foreach (var leg in CartageLegs)
			{
				leg.RefreshBinding();
			}
		}

		void DefaultDropModeIfAllowed(JobDocAddress docAddress)
		{
			if (!IsImportingData && !IsBehaviorSuspended && triggerDropModeDefaulting && (JJ_E3_NKJobType.IsEmpty || CartageType != null))
			{
				try
				{
					var useAddressDropMode = UseAddressDropMode(docAddress);
					if (useAddressDropMode)
					{
						CurrentChangingDocAddressType = docAddress.DocAddressType;
					}
					else
					{
						useAddressDropMode = docAddress.DocAddressType == RelevantDocAddressType;
					}

					if (useAddressDropMode)
					{
						DefaultDropModeOnAddressChange(docAddress);
					}
				}
				finally
				{
					CurrentChangingDocAddressType = null;
				}
			}

			triggerDropModeDefaulting = true;
		}

		DocAddressType? CurrentChangingDocAddressType;

		bool UseAddressDropMode(JobDocAddress docAddress)
		{
			bool result;

			if (IsMixed)
			{
				result = HasLooseMoveWithAddressType(docAddress.DocAddressType) || HasContainerisedMoveWithAddressType(docAddress.DocAddressType);
			}
			else if (IsLoose)
			{
				result = HasLooseMoveWithAddressType(docAddress.DocAddressType);
			}
			else
			{
				result = HasContainerisedMoveWithAddressType(docAddress.DocAddressType);
			}

			return result;
		}

		bool HasLooseMoveWithAddressType(DocAddressType docAddressType)
		{
			return LooseBookedMoves.Any(move => move.RequestedAddressType == docAddressType);
		}

		bool HasContainerisedMoveWithAddressType(DocAddressType docAddressType)
		{
			foreach (var container in Containers)
			{
				if (GetBookedMoves(container).Any(move => move.RequestedAddressType == docAddressType))
				{
					return true;
				}
			}

			return false;
		}

		void DefaultDropModeOnAddressChange(JobDocAddress docAddress)
		{
			var shouldChangeDropMode = false;
			var dropMode = AddressDropMode;
			if (!JJ_DropMode.IsEmpty && !dropMode.IsEmpty && JJ_DropMode != dropMode)
			{
				var message = Res.GetString("100b8dc9-6086-4266-ad90-121a7ed6a047", "Would you like to set the Cartage Drop Mode with the {0} Drop Mode '{1}'?", docAddress.AddressCaption, dropMode);
				var caption = Res.GetString("e6de8992-8bcc-44c4-9e26-b82d198cc9f0", "Populate Cartage Drop Mode");
				var e = new QueryUserYesNoEventArgs(caption, message, false);

				NotificationsQueryUser(e);
				shouldChangeDropMode = e.Response;
			}

			if (JJ_DropMode.IsEmpty || shouldChangeDropMode)
			{
				JJ_DropMode = dropMode;
			}
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
			BookedMovesCollection.ForEach(m => m.ClearDeletedDocAddress(docAddress.PK));
			CartageLegs.ForEach(l => l.ClearDeletedDocAddress(docAddress.PK));
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
			MarkAsNeededAddressReorder();
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
			if ((FirstDocAddress != null && docAddress.PK == FirstDocAddress.PK)
			|| (SecondDocAddress != null && docAddress.PK == SecondDocAddress.PK)
			|| (ThirdDocAddress != null && docAddress.PK == ThirdDocAddress.PK)
			|| (FourthDocAddress != null && docAddress.PK == FourthDocAddress.PK))
			{
				PopulateClientIDFromOrgTypeBillToPartyIfApplicable();
			}
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return PiggyBackedDocAddressValidation(addressToValidate);
		}

		protected virtual CommonCartageDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new CommonCartageDocAddressValidation(addressToValidate);
		}

		public const short MaxNumberOfJobDocAddresses = 4;

		public JobDocAddressCollection FirstDocAddresses
		{
			get
			{
				if (firstDocAddresses == null)
				{
					firstDocAddresses = new JobDocAddressCollection(Factory);
				}

				if (firstDocAddresses.Count == 0)
				{
					FindAndAddAddressIfRequired(firstDocAddresses, 1);
				}

				return firstDocAddresses;
			}
		}
		JobDocAddressCollection firstDocAddresses;

		public JobDocAddress FirstDocAddress
		{
			get { return (JobDocAddress)FirstDocAddresses.FirstOrDefault(); }
		}

		public JobDocAddressCollection SecondDocAddresses
		{
			get
			{
				if (secondDocAddresses == null)
				{
					secondDocAddresses = new JobDocAddressCollection(Factory);
				}

				if (secondDocAddresses.Count == 0)
				{
					FindAndAddAddressIfRequired(secondDocAddresses, 2);
				}

				return secondDocAddresses;
			}
		}
		JobDocAddressCollection secondDocAddresses;

		public JobDocAddress SecondDocAddress
		{
			get { return (JobDocAddress)SecondDocAddresses.FirstOrDefault(); }
		}

		public JobDocAddressCollection ThirdDocAddresses
		{
			get
			{
				if (thirdDocAddresses == null)
				{
					thirdDocAddresses = new JobDocAddressCollection(Factory);
				}

				if (thirdDocAddresses.Count == 0)
				{
					FindAndAddAddressIfRequired(thirdDocAddresses, 3);
				}

				return thirdDocAddresses;
			}
		}
		JobDocAddressCollection thirdDocAddresses;

		public JobDocAddress ThirdDocAddress
		{
			get { return (JobDocAddress)ThirdDocAddresses.FirstOrDefault(); }
		}

		public JobDocAddressCollection FourthDocAddresses
		{
			get
			{
				if (fourthDocAddresses == null)
				{
					fourthDocAddresses = new JobDocAddressCollection(Factory);
				}

				if (fourthDocAddresses.Count == 0)
				{
					FindAndAddAddressIfRequired(fourthDocAddresses, 4);
				}

				return fourthDocAddresses;
			}
		}
		JobDocAddressCollection fourthDocAddresses;

		public JobDocAddress FourthDocAddress
		{
			get { return (JobDocAddress)FourthDocAddresses.FirstOrDefault(); }
		}

		public void MakeAddressesPersistentButDeleteEmpty()
		{
			MakeAddressesPersistentButDeleteEmpty(DocAddresses.ToArray<JobDocAddress>());
			ResetMainAddressesForBinding();
		}

		void MakeAddressesPersistentButDeleteEmpty(JobDocAddress[] addresses)
		{
			foreach (var address in addresses)
			{
				if (!address.IsInDatabase && address.IsEmpty)
				{
					address.Delete();
				}
				else
				{
					address.MakePersistentIfNotEmpty();
				}
			}
		}

		public void ResetMainAddressesForBinding()
		{
			ResetMainAddressesForBinding(firstDocAddresses);
			ResetMainAddressesForBinding(secondDocAddresses);
			ResetMainAddressesForBinding(thirdDocAddresses);
			ResetMainAddressesForBinding(fourthDocAddresses);
		}

		void ResetMainAddressesForBinding(JobDocAddressCollection addresses)
		{
			if (addresses != null)
			{
				addresses.RemoveAll();
			}
		}

		public void RefreshAddresses()
		{
			var poke = FirstDocAddress;
			poke = SecondDocAddress;
			poke = ThirdDocAddress;
			poke = FourthDocAddress;
		}

		public void FindAndAddAddressIfRequired(JobDocAddressCollection addresses, int addressNumber)
		{
			triggerDropModeDefaulting = false;

			try
			{
				var docAddress = BehaviorStrategy.FindOrCreateMainDocAddress(this, addressNumber);
				if (docAddress != null)
				{
					addresses.Add(docAddress);
				}
			}
			finally
			{
				triggerDropModeDefaulting = true;
			}
		}

		void PopulateClientIDFromOrgTypeBillToPartyIfApplicable()
		{
			if (LocalClientAddressPK.IsEmpty && !HasParent)
			{
				var billTo = InvoicingSupporter.Consignor;
				if (billTo != null)
				{
					LocalClientPK = billTo.PK;
				}
			}
		}

		internal JobDocAddress AddJobDocAddress(ZGuid orgAddressPK)
		{
			triggerDropModeDefaulting = false;

			var address = Factory.Load<OrgAddress>(orgAddressPK);
			if (address != null)
			{
				foreach (var jobDoc in DocAddresses.ToArray<JobDocAddress>())
				{
					if (jobDoc.Organisation != null && jobDoc.Organisation.Addresses.Contains(address))
					{
						return DocAddresses.AddNew(address, jobDoc.DocAddressType);
					}
				}

				return DocAddresses.AddNew(address, DocAddressType.LocalCartageCFS);
			}

			return null;
		}

		bool triggerDropModeDefaulting = true;

		public void RemoveJobDocAddressIfNotInUse(ZGuid jobDocAddressPK)
		{
			JobDocAddress jobDocAddress = Factory.Load<JobDocAddress>(jobDocAddressPK);

			if (jobDocAddress != null)
			{
				bool found = FirstDocAddress == jobDocAddress;
				found |= SecondDocAddress == jobDocAddress;
				found |= ThirdDocAddress == jobDocAddress;
				found |= FourthDocAddress == jobDocAddress;

				if (!found)
				{
					foreach (CommonBookedCtgMove move in BookedMovesCollection)
					{
						found |= move.PickupFromDocAddress == jobDocAddress;
						found |= move.WaitPointDocAddress == jobDocAddress;
						found |= move.DeliverToDocAddress == jobDocAddress;

						foreach (CommonCartageLeg leg in move.CartageLegs)
						{
							found |= leg.PickupFromDocAddress == jobDocAddress;
							found |= leg.WaitPointDocAddress == jobDocAddress;
							found |= leg.DeliverToDocAddress == jobDocAddress;
						}
					}
				}

				if (!found)
				{
					jobDocAddress.Delete();
				}
			}
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return docAddress.Organisation == null;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (CartageFactorySaved != null)
				{
					CartageFactorySaved(this, EventArgs.Empty);
				}

				if (!isInOnFactorySaved)
				{
					if (PublishEventsOnSaved.Any(l => l != null))
					{
						if (ParentBooking != null)
						{
							var old = isInOnFactorySaved;
							isInOnFactorySaved = true;
							try
							{
								var factory = new BusinessObjectFactory();
								using (factory.AddDisposableService())
								{
									foreach (var log in PublishEventsOnSaved.Where(l => !(l?.IsDeleted ?? true)))
									{
										PublishUniversalEventCore.PublishUniversalEvent(factory, this, (BusinessObject)ParentBooking, log);
									}
									factory.Save();
								}
							}
							finally
							{
								isInOnFactorySaved = old;
							}
						}

						PublishEventsOnSaved.Clear();
					}
				}
			}
		}
		public event EventHandler CartageFactorySaved;

		[ThreadStatic]
		static bool isInOnFactorySaved;

		List<StmALog> PublishEventsOnSaved
		{
			get { return publishEventsOnSaved ?? (publishEventsOnSaved = new List<StmALog>()); }
		}
		List<StmALog> publishEventsOnSaved;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event EventHandler<DocumentContainerEventArgs> OnGetContainersToPrint;
		public void RaiseOnGetContainersToPrint(DocumentContainerEventArgs e)
		{
			if (OnGetContainersToPrint != null)
			{
				OnGetContainersToPrint(this, e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event EventHandler<DocumentCartageLegEventArgs> OnGetCartageLegsToPrint;
		public void RaiseOnGetCartageLegsToPrint(DocumentCartageLegEventArgs e)
		{
			if (OnGetCartageLegsToPrint != null)
			{
				OnGetCartageLegsToPrint(this, e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event EventHandler<DocumentCartageLegEventArgs> OnGetNonContainerisedCartageLegsToPrint;
		public void RaiseOnGetNonContainerisedCartageLegsToPrint(DocumentCartageLegEventArgs e)
		{
			if (OnGetNonContainerisedCartageLegsToPrint != null)
			{
				OnGetNonContainerisedCartageLegsToPrint(this, e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event EventHandler<WorkSheetLegLinkEventArgs> OnWorkSheetLegLinkAdded;
		public void RaiseWorkSheetLegLinkAdded(WorkSheetLegLinkEventArgs e)
		{
			if (OnWorkSheetLegLinkAdded != null)
			{
				OnWorkSheetLegLinkAdded(this, e);
			}
		}

		public event CancelEventHandler UpdateCartageTotalsPackQuantityVariation;

		public void CheckTotalsDiffer()
		{
			//allways check even for containerised
			if (PackageWeightVolumeTotalsDiffer() && UpdateCartageTotalsPackQuantityVariation != null)
			{
				CancelEventArgs e = new CancelEventArgs();
				UpdateCartageTotalsPackQuantityVariation(this, e);
				if (!e.Cancel)
				{
					UpdateCartageFromLooseBookedMovesOrContainers();
				}
			}
		}

		bool PackageWeightVolumeTotalsDiffer()
		{
			if (IsLoose)
			{
				return OuterPacksTotalsDiffer();
			}
			else
			{
				return ContainersTotalsDiffer();
			}
		}

		public void UpdateCartageFromLooseBookedMovesOrContainers()
		{
			using (new SemaphoreManager(IsUpdatingCartageFromBookedMoves))
			{
				ZInt outerPacks = 0;
				ZString packType = ZString.Empty;

				ZDecimal weight = 0;
				ZString weightUnit = ZString.Empty;

				ZDecimal volume = 0;
				ZString volumeUnit = ZString.Empty;

				if (IsLoose)
				{
					packType = TotalLooseBookedPackagesUnit;
					weightUnit = TotalLooseBookedWeightUnit;
					volumeUnit = TotalLooseBookedVolumeUnit;

					outerPacks = TotalLooseBookedPackages;
					weight = TotalLooseBookedWeight;
					volume = TotalLooseBookedVolume;

					new WeightConversionStrategy().ReScale(ref weight, ref weightUnit, 9, 3);
					new VolumeConversionStrategy().ReScale(ref volume, ref volumeUnit, 9, 3);
				}
				else
				{
					weight = TotalContainerWeight;
					weightUnit = TotalContainerWeightUnit;

					volume = TotalContainerVolume;
					volumeUnit = TotalContainerVolumeUnit;

					outerPacks = TotalContainerPacks;
					packType = TotalContainerPackType;

					if (Containers.Any())
					{
						new WeightConversionStrategy().ReScale(ref weight, ref weightUnit, 9, 3);
						new VolumeConversionStrategy().ReScale(ref volume, ref volumeUnit, 9, 3);
					}
				}

				JJ_F3_NKPackType = packType;
				JJ_WeightUQ = weightUnit;
				JJ_VolumeUQ = volumeUnit;

				JJ_OuterPacks = outerPacks;
				JJ_Weight = weight;
				JJ_Volume = volume;
			}
		}

		public ZDecimal TotalContainerWeight
		{
			get
			{
				ZDecimal result = 0;
				var unit = TotalContainerWeightUnit;
				foreach (CommonContainer container in Containers)
				{
					result += Constants.Weight.Convert(container.GoodsWeight, container.GoodsWeightUQ, unit);
				}
				return result;
			}
		}

		public ZString TotalContainerWeightUnit
		{
			get
			{
				return Containers.FirstOrDefault()?.GoodsWeightUQ ?? Constants.Weight.Kilograms;
			}
		}

		public ZDecimal TotalContainerVolume
		{
			get
			{
				ZDecimal result = 0;
				var unit = TotalContainerVolumeUnit;
				foreach (CommonContainer container in Containers)
				{
					result += Constants.Volume.Convert(container.JC_Calc_TotalVolume, container.JC_Calc_TotalVolumeUnit, unit);
				}
				return result;
			}
		}

		public ZString TotalContainerVolumeUnit
		{
			get
			{
				return Containers.FirstOrDefault()?.JC_Calc_TotalVolumeUnit ?? Constants.Volume.CubicMetres;
			}
		}

		public ZInt TotalContainerPacks
		{
			get
			{
				ZInt result = 0;
				foreach (CommonContainer container in Containers)
				{
					result += container.JC_Calc_TotalPackages;
				}
				return result;
			}
		}

		public ZString TotalContainerPackType
		{
			get
			{
				return Containers.FirstOrDefault()?.JC_Calc_TotalPackagesUnit ?? (ZString)Constants.PkgUnit.Package;
			}
		}

		ZDecimal GetGrossWeight()
		{
			ZDecimal result = 0;
			foreach (CommonContainer container in Containers)
			{
				result += container.JC_GrossWeight;
			}
			return result;
		}

		Semaphore IsUpdatingCartageFromBookedMoves
		{
			get { return isUpdatingCartageFromBookedMoves ?? (isUpdatingCartageFromBookedMoves = new Semaphore()); }
		}

		Semaphore isUpdatingCartageFromBookedMoves;

		ZBool OuterPacksTotalsDiffer()
		{
			return TotalLooseBookedPackages != JJ_OuterPacks
				|| !Constants.Weight.ContainsCode(JJ_WeightUQ) || Utilities.Round(Constants.Weight.Convert(TotalLooseBookedWeight, TotalLooseBookedWeightUnit, JJ_WeightUQ, applyDefaultRounding: false), 3) != Utilities.Round(JJ_Weight, 3)
				|| !Constants.Volume.ContainsCode(JJ_VolumeUQ) || Utilities.Round(Constants.Volume.Convert(TotalLooseBookedVolume, TotalLooseBookedVolumeUnit, JJ_VolumeUQ, applyDefaultRounding: false), 3) != Utilities.Round(JJ_Volume, 3);
		}

		ZBool ContainersTotalsDiffer()
		{
			if (HasParent)
			{
				return TotalContainerPacks != JJ_OuterPacks
					|| !Constants.Weight.ContainsCode(JJ_WeightUQ) || Utilities.Round(Constants.Weight.Convert(TotalContainerWeight, TotalContainerWeightUnit, JJ_WeightUQ, applyDefaultRounding: false), 3) != Utilities.Round(JJ_Weight, 3)
					|| !Constants.Volume.ContainsCode(JJ_VolumeUQ) || Utilities.Round(Constants.Volume.Convert(TotalContainerVolume, TotalContainerVolumeUnit, JJ_VolumeUQ, applyDefaultRounding: false), 3) != Utilities.Round(JJ_Volume, 3);
			}
			else
			{
				return false;
			}
		}

		public CartageBindToLists BindToLists
		{
			get { return bindToLists ?? (bindToLists = new CartageBindToLists(Factory)); }
		}
		CartageBindToLists bindToLists;

		public CommonCartageBehaviorStrategy BehaviorStrategy
		{
			get
			{
				if (!HasParent)
				{
					return CommonCartageBehaviorStrategyProvider.GetCartageProvider(Factory).CartageBehaviorStrategy;
				}
				else
				{
					return CommonCartageBehaviorStrategyProvider.GetCartageProvider(Factory).InternalCartageBehaviorStrategy;
				}
			}
		}

		public IDisposable SuspendBehavior()
		{
			return new BehaviorSuspender(this);
		}

		public ZBool IsBehaviorSuspended
		{
			get { return behaviorSuspenderLevel > 0; }
		}

		sealed class BehaviorSuspender : IDisposable
		{
			public BehaviorSuspender(CommonCartage cartage)
			{
				if (cartage == null)
				{
					throw new ArgumentNullException(nameof(cartage));
				}

				this.cartage = cartage;
				cartage.behaviorSuspenderLevel++;
			}
			readonly CommonCartage cartage;

			public void Dispose()
			{
				if (!disposed)
				{
					disposed = true;
					cartage.behaviorSuspenderLevel--;
				}
			}

			bool disposed;
		}
		int behaviorSuspenderLevel;

		public IDtbBooking ParentBooking
		{
			get
			{
				if (parentBooking == null && !JJ_ParentID.IsEmpty && JJ_ParentTableCode == DtbBookingSchema.Constants.Prefix)
				{
					parentBooking = Factory.Load<IDtbBooking>(JJ_ParentID);
				}

				return parentBooking;
			}
		}

		IDtbBooking parentBooking;

		public ZBool HasParent
		{
			get { return !JJ_ParentID.IsEmpty && CartageParent != null; }
		}

		public ICartageParent CartageParent
		{
			get
			{
				if (cartageParent == null)
				{
					if (JJ_ParentTableCode == JobShipmentSchema.Constants.Prefix)
					{
						CommonShipment shipmentParent = Factory.Load<CommonShipment>(JJ_ParentID);
						if (shipmentParent != null && shipmentParent.JS_IsBooking && !shipmentParent.JS_IsForwardRegistered)
						{
							cartageParent = ObjectFactory.Get<IQuotedBookingBuilder>().InitializeFrom(shipmentParent.PK, Factory) as ICartageParent;
						}
						else
						{
							cartageParent = shipmentParent as ICartageParent;
						}
					}
					else if (JJ_ParentTableCode == JobDeclarationSchema.Constants.Prefix)
					{
						cartageParent = (ICartageParent)Factory.Load<IBaseJobDeclaration>(JJ_ParentID);
					}
					else if (JJ_ParentTableCode == JobConsolSchema.Constants.Prefix)
					{
						cartageParent = (ICartageParent)Factory.Load<ICFSLoadListConsol>(JJ_ParentID);
					}
					else if (JJ_ParentTableCode == WhsDocketSchema.Constants.Prefix)
					{
						cartageParent = (ICartageParent)Factory.Load<IWhsOrder>(JJ_ParentID);
					}
				}
				return cartageParent;
			}
		}

		ICartageParent cartageParent;

		public CartageType CartageInternalType
		{
			get { return Common.Business.CartageType.GetCartageType(CartageParent, this); }
		}

		internal bool ShouldUseParentJob
		{
			get { return CartageParent != null && JJ_Status == "v1"; }
		}

		CommonCartageInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new CommonCartageInvoicingSupporter(this)); }
		}

		ZGuid IJobHeaderParentCore.PK
		{
			get
			{
				if (ShouldUseParentJob)
				{
					return CartageParent.CartageParentID;
				}
				else
				{
					return base.PK;
				}
			}
		}

		string IJobHeaderParentCore.TableName
		{
			get
			{
				if (ShouldUseParentJob)
				{
					return ((BusinessObject)CartageParent).TableName;
				}
				else
				{
					return base.TableName;
				}
			}
		}

		void IJobHeaderParent.OnJobCreating(JobHeader jobHeader)
		{
			if (JobCreating != null)
			{
				JobCreating(this, EventArgs.Empty);
			}
		}

		void IJobHeaderParent.OnJobCreated(JobHeader jobHeader)
		{
			// done here and not in OnJobCreating() as in some cases may have issues when job.Parent has been set but job.JH_ParentID has not been set yet. For an example see unit test in NewCartageUserControlTest.TestShouldNotThrowWhenBindingNewCartageToUserControl()
			if (!jobHeader.IsDeleting)
			{
				if (HasParent)
				{
					if (!jobHeader.IsInDatabase)
					{
						jobHeader.JH_OA_LocalChargesAddr = CartageParent.LocalClientAddressPK;
					}
					CartageHelper.AttachCartageJobToParentJob(this, jobHeader, this.CartageParent.JobHeaderPK);
				}
				else if (ParentBooking != null)
				{
					jobHeader.JH_JH_ParentJob = ParentBooking.JobHeaderPK;
				}
			}
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader jobHeader)
		{
			if (JobDeleting != null)
			{
				JobDeleting(this, EventArgs.Empty);
			}
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader jobHeader)
		{
		}

		public event EventHandler JobCreating;
		public event EventHandler JobDeleting;

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			PopulateJJ_ConsignmentIDIfNeeded();
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		string IJobNumber.JobNumber
		{
			get { return ShouldUseParentJob ? CartageParent.UniqueConsignmentID : JJ_ConsignmentID; }
		}

		public override string CanCancel()
		{
			return JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(PK, HumanReadableName);
		}

		public override string CanReactivate()
		{
			if (JJ_IsCancelled && ParentBooking != null && (ParentBooking.KM_Status == TransportStatuses.Codes.Booked || ParentBooking.KM_Status == TransportStatuses.Codes.ServiceCommenced || !ParentBooking.KM_IsActive))
			{
				return Res.GetString("801279f8-cc5b-421f-889f-16817a0989c7", "The parent booking of this cartage is not available, so this cartage cannot be reactivated.");
			}

			return base.CanReactivate();
		}

		public override bool IsCancelledHasChanged
		{
			get { return JJ_IsCancelledInfo.HasChanges; }
		}

		public override ZBool JJ_IsCancelled
		{
			get { return base.JJ_IsCancelled; }
			set
			{
				var originalValue = base.JJ_IsCancelled;
				base.JJ_IsCancelled = value;
				UpdateReadOnlyForWhenCancelled();
				if (value != originalValue)
				{
					LogServiceStatusOnActivateDeactivate();
				}
			}
		}

		void UpdateReadOnlyForWhenCancelled()
		{
			if (!IsDeleted)
			{
				SetReadOnlyIncludingChildren(JJ_IsCancelled);
			}
		}

		void LogServiceStatusOnActivateDeactivate()
		{
			StmALog log;
			if (JJ_IsCancelled)
			{
				log = LogServicesCancelled();
			}
			else
			{
				log = LogServicesCommenced();
			}
			PublishEventsOnSaved.Add(log);
		}

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			var addressesByOld = new Dictionary<ZGuid, JobDocAddress>();

			var newCartage = Factory.New<CommonCartage>();
			using (newCartage.SuspendBehavior())
			{
				newCartage.JJ_E3_NKJobType = this.JJ_E3_NKJobType;

				newCartage.DocAddresses.RemoveAndDeleteAll();
				foreach (JobDocAddress cartageAddress in this.DocAddresses)
				{
					JobDocAddress newAddress = (JobDocAddress)cartageAddress.Clone();
					newCartage.DocAddresses.Add(newAddress);
					addressesByOld.Add(cartageAddress.PK, newAddress);
				}

				if (this.LocalClientAddress != null)
				{
					// Local Client is stored on JobHeader and is required. It is ok to create the Job without mutex as the Cartage Job is new.
					new JobHeader.Loader(newCartage).TryLoadOrCreate();
					newCartage.LocalClientAddressPK = this.LocalClientAddressPK;
				}

				newCartage.JJ_RS_NKServiceLevel = this.JJ_RS_NKServiceLevel;
				newCartage.JJ_DropMode = this.JJ_DropMode;
				newCartage.JJ_ContainerMode = this.JJ_ContainerMode;
				newCartage.JJ_Direction = this.JJ_Direction;
				newCartage.JJ_ShippingTransportMode = this.JJ_ShippingTransportMode;

				CopyBookedMoves(this.BookedMovesCollection, newCartage.BookedMovesCollection, addressesByOld);
			}

			return newCartage;
		}

		void CopyBookedMoves(CommonBookedCtgMoveCollection from, CommonBookedCtgMoveCollection to, Dictionary<ZGuid, JobDocAddress> addressList)
		{
			to.DeleteAll();
			foreach (CommonBookedCtgMove move in from)
			{
				CommonBookedCtgMove newMove = to.AddNew();
				if (move.EW_JC_Container.IsValid)
				{
					var newContainer = Factory.New<CommonContainer>();
					newContainer.JC_RC = move.Container.JC_RC;
					newMove.EW_JC_Container = newContainer.PK;
				}

				newMove.EW_E2PickupAddressID = GetJobDocAddressPK(addressList, move.EW_E2PickupAddressID);
				newMove.EW_E2WaitPointAddressID = GetJobDocAddressPK(addressList, move.EW_E2WaitPointAddressID);
				newMove.EW_E2DeliveryAddressID = GetJobDocAddressPK(addressList, move.EW_E2DeliveryAddressID);
				newMove.EW_DropMode = move.EW_DropMode;

				newMove.CartageLegs.DeleteAll();

				// Call FirstCartageLeg so that move.CartageLegs is ordered by JU_DisplayOrder
				_ = move.FirstCartageLeg;
				foreach (CommonCartageLeg leg in move.CartageLegs)
				{
					CommonCartageLeg newLeg = newMove.CartageLegs.AddNew();
					newLeg.JU_E2PickupAddressID = GetJobDocAddressPK(addressList, leg.JU_E2PickupAddressID);
					newLeg.JU_E2WaitPointAddressID = GetJobDocAddressPK(addressList, leg.JU_E2WaitPointAddressID);
					newLeg.JU_E2DeliveryAddressID = GetJobDocAddressPK(addressList, leg.JU_E2DeliveryAddressID);
					newLeg.JU_IsEmptyContainer = leg.JU_IsEmptyContainer;
					newLeg.JU_DisplayOrder = leg.JU_DisplayOrder;
				}
				newMove.EW_DisplayOrder = move.EW_DisplayOrder;
			}
		}

		ZGuid GetJobDocAddressPK(Dictionary<ZGuid, JobDocAddress> list, ZGuid key)
		{
			JobDocAddress found;
			list.TryGetValue(key, out found);
			return found != null ? found.PK : ZGuid.Empty;
		}

		string ISendEmailSource.EmailSubject
		{
			get
			{
				string result = "";
				if (Job != null)
				{
					result = Res.GetString("03918e87-3628-4fe6-bd54-007aeb64d10a", "Transport - {0}", Job.JH_JobNum);
				}
				return result;
			}
		}

		string ISendEmailSource.TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.PortTransport; }
		}

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();

			if (FirstDocAddress != null)
			{
				result.AddRecipient(FirstDocAddress.Organisation);
			}
			if (SecondDocAddress != null)
			{
				result.AddRecipient(SecondDocAddress.Organisation);
			}
			if (ThirdDocAddress != null)
			{
				result.AddRecipient(ThirdDocAddress.Organisation);
			}
			if (FourthDocAddress != null)
			{
				result.AddRecipient(FourthDocAddress.Organisation);
			}

			result.AddRecipient(Job == null ? null : Job.LocalCharges);
			result.AddRecipient(Job == null ? null : Job.AgentCollect);
			return result;
		}

		string ISendEmailSource.DefaultFromDisplayName
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		string ISendEmailSource.OverridingDefaultFromEmailAddress
		{
			get { return null; }
		}

		Type ISendEmailSource.DocWrapperType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocCommonCartage>(); }
		}

		CDArchiveInfo ICDArchive.CDArchiveInfo
		{
			get { return new CartageCDArchiveInfo(this); }
		}

		class CartageCDArchiveInfo : CDArchiveInfo
		{
			public CartageCDArchiveInfo(CommonCartage cartage)
				: base(cartage)
			{
			}

			CommonCartage Cartage
			{
				get { return (CommonCartage)BusinessEntity; }
			}

			public override ZString ConsigneeCode
			{
				get { return Cartage.SecondDocAddress.Organisation != null ? Cartage.SecondDocAddress.Organisation.OH_Code : ZString.Empty; }
			}

			public override ZString ConsignorCode
			{
				get { return ZString.Empty; }
			}

			public override ZString[] ContainerNumbersList
			{
				get
				{
					return Cartage.Containers.Select(c => c.JC_ContainerNum).ToArray();
				}
			}

			public override ZString Destination
			{
				get { return ZString.Empty; }
			}

			public override ZString[] EntryNumbersList
			{
				get { return Array.Empty<ZString>(); }
			}

			public override ZDateTime ETA
			{
				get { return ZDateTime.Empty; }
			}

			public override ZDateTime ETD
			{
				get { return ZDateTime.Empty; }
			}

			public override ZString VoyageFlight
			{
				get { return ""; }
			}

			public override ZString HouseBill
			{
				get { return ZString.Empty; }
			}

			public override ZString[] InvoiceNumbersList
			{
				get
				{
					ZGuid orgPK = (Cartage.SecondDocAddress.Organisation != null) ? Cartage.SecondDocAddress.Organisation.PK : ZGuid.Empty;
					AccTransactionHeaderCollection transactions = new InvoiceLoader(Factory).GetInvoicesForUniqueRef(new ZGuid[] { orgPK }, Cartage.JJ_ConsignmentID);
					ZString[] headers = new ZString[transactions.Count];
					for (int i = 0; i < transactions.Count; i++)
					{
						headers[i] = transactions[i].AH_ConsolidatedInvoiceRef;
					}
					return headers;
				}
			}

			public override ZString JobNumber
			{
				get { return Cartage.JJ_ConsignmentID; }
			}

			public override ZString MasterBill
			{
				get { return ZString.Empty; }
			}

			public override ZString[] OrderNumbersList
			{
				get { return Array.Empty<ZString>(); }
			}

			public override ZString Origin
			{
				get
				{
					return ZString.Empty;
				}
			}

			public override ZString Vessel
			{
				get { return ""; }
			}
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_GB, JJ_GB, null);
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, JJ_E3_NKJobType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, LocalClient != null ? LocalClient.PK : ZGuid.Empty, ZGuid.Empty);

			return result;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CartageProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		CartageProcessTaskCollection workflowItems;

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return JobInvoicingConsumerTypes.LocalCartage.Code; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return WorkflowInformationProvider;
		}

		WorkflowInformationProvider WorkflowInformationProvider
		{
			get
			{
				if (workflowInformationProvider == null)
				{
					workflowInformationProvider = new WorkflowInformationProvider(new ZGuid[] { JJ_GB });
				}
				workflowInformationProvider.Origin = ZString.Empty;
				workflowInformationProvider.Destination = ZString.Empty;
				workflowInformationProvider.BusinessContext = TrackingConstants.BusinessContext.Cartage;

				return workflowInformationProvider;
			}
		}

		WorkflowInformationProvider workflowInformationProvider;

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		public virtual DocumentSupporter DocumentSupporter
		{
			get { return new CartageDocumentSupporter(this); }
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new CommonCartageDocManagerInfo(this, Core.Constants.DocManagerCodes.TransportJob);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		bool ISailingManaged.AllowScheduleCreation
		{
			get { return FreightUtilities.AllowCreateScheduleFromJob(JJ_ShippingTransportMode); }
		}

		bool ISailingManaged.AllowScheduleDatesChanging
		{
			get
			{
				var checkpoint = FreightUtilities.GetEditScheduleSecurityCheckpoint(JJ_ShippingTransportMode);
				return checkpoint != null && checkpoint.IsAllowed;
			}
		}

		ZString ISailingManaged.TransportMode
		{
			get { return JJ_ShippingTransportMode; }
		}

		ZString ISailingManaged.Load
		{
			get { return fPortOfLoading; }
			set { fPortOfLoading = value; }
		}

		ZString ISailingManaged.Discharge
		{
			get { return fPortOfDischarge; }
			set { fPortOfDischarge = value; }
		}

		void ISailingManaged.SetLCLStorageDate(ZDateTime value) { }

		void ISailingManaged.SetLCLReceivalCommences(ZDateTime value) { }

		void ISailingManaged.SetLCLCutOff(ZDateTime value) { }

		void ISailingManaged.SetLCLAvailabilityDate(ZDateTime value) { }

		ZBool ISailingManaged.IsCharter
		{
			get { return false; }
			set { }
		}

		ZString ISailingManaged.AircraftType
		{
			get { return ZString.Empty; }
			set { }
		}

		ZBool ISailingManaged.IsImportingData
		{
			get { return IsImportingData; }
		}

		void ISailingManaged.SetFCLReceivalCommences(ZDateTime value) { }

		void ISailingManaged.SetFCLCutOff(ZDateTime value) { }

		void ISailingManaged.SetVGMCutOff(ZDateTime value) { }

		ZString ISailingManaged.Vessel
		{
			get { return fVessel; }
			set { fVessel = value; }
		}

		ZString ISailingManaged.Voyage
		{
			get { return fVoyage; }
			set { fVoyage = value; }
		}

		void ISailingManaged.SetStorageDate(ZDateTime value) { }

		void ISailingManaged.SetAvailabilityDate(ZDateTime value) { }

		ZGuid ISailingManaged.SailingPK
		{
			get { return JJ_JX_Sailing; }
			set { JJ_JX_Sailing = value; }
		}

		void ISailingManaged.SetDocsCutOff(ZDateTime value) { }

		ZGuid ISailingManaged.ShippingLine
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString ISailingManaged.RegistrationNo
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString ISailingManaged.OnlineScheduleStatus
		{
			get { return ZString.Empty; }
			set { }
		}

		ZBool ISailingManaged.IsCargoOnly
		{
			get { return false; }
			set { }
		}

		ZDateTime ISailingManaged.ATA
		{
			get
			{
				var result = ZDateTime.Empty;
				if (SailingStandalone != null)
				{
					result = SailingStandalone.Destination.JB_A_ARV;
				}
				return result;
			}
			set { }
		}

		ZDateTime ISailingManaged.ETD
		{
			get { return fE_DEP; }
			set { fE_DEP = value; }
		}

		ZDateTime ISailingManaged.ETA
		{
			get { return fE_ARV; }
			set { fE_ARV = value; }
		}

		ZDateTime ISailingManaged.STD
		{
			get
			{
				var result = ZDateTime.Empty;
				if (SailingStandalone != null)
				{
					result = SailingStandalone.Origin.JA_S_DEP;
				}
				return result;
			}
			set { }
		}

		ZDateTime ISailingManaged.STA
		{
			get
			{
				var result = ZDateTime.Empty;
				if (SailingStandalone != null)
				{
					result = SailingStandalone.Destination.JB_S_ARV;
				}
				return result;
			}
			set { }
		}

		ZDateTime ISailingManaged.ATD
		{
			get
			{
				var result = ZDateTime.Empty;
				if (SailingStandalone != null)
				{
					result = SailingStandalone.Origin.JA_A_DEP;
				}
				return result;
			}
			set { }
		}

		void ISailingManaged.SetLoadETA(ZDateTime value) { }

		void ISailingManaged.SetLoadATA(ZDateTime value) { }

		void ISailingManaged.SetOA_DepartureLocation(ZGuid value) { }

		void ISailingManaged.SetOA_ArrivalLocation(ZGuid value) { }

		void ISailingManaged.SetEmptyCutOff(ZDateTime value) { }

		void ISailingManaged.SetEmptyReceivalCommences(ZDateTime value) { }

		void ISailingManaged.SetDGCutOff(ZDateTime value) { }

		void ISailingManaged.SetDGReceivalCommences(ZDateTime value) { }

		void ISailingManaged.SetReeferCutOff(ZDateTime value) { }

		void ISailingManaged.SetReeferReceivalCommences(ZDateTime value) { }

		void ISailingManaged.SetServiceString(ZString value) { }

		void ISailingManaged.SetArrivalPortRouteId(ZString value) { }

		void ISailingManaged.SetDeparturePortRouteId(ZString value) { }

		void ISailingManaged.LogDateEvents(object sender) { }

		JobSailingValidation ISailingManaged.SailingAdditionalValidation => null;

		JobVoyOriginValidation ISailingManaged.VoyOriginAdditionalValidation => null;

		JobVoyDestinationValidation ISailingManaged.VoyDestinationAdditionalValidation => null;

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			JJ_ConsignmentID = "";
		}

#endif

		public new CommonCartageLookups Lookups
		{
			get { return lookups ?? (lookups = (CommonCartageLookups)GetNewLookups()); }
		}
		CommonCartageLookups lookups;

		protected override JobCartageLookups GetNewLookups()
		{
			return new CommonCartageLookups(this);
		}

		public new CommonCartageValidation Validation
		{
			get { return (CommonCartageValidation)GetNewValidation(); }
		}

		protected override JobCartageValidation GetNewValidation()
		{
			return new CommonCartageValidation(this);
		}

		internal ZString fPortOfLoading;
		internal ZString fPortOfDischarge;
		internal ZString fVessel;
		internal ZString fVoyage;
		internal ZDateTime fE_DEP;
		internal ZDateTime fE_ARV;

		public void SetCartageDropModeWithOutSettingMoves(DropMode forceDropMode = DropMode.None)
		{
			if (!IsImportingData)
			{
				ZString result = ZString.Empty;
				switch (forceDropMode)
				{
					case DropMode.None:
						result = GetDropMode();
						break;
					case DropMode.Address:
						result = AddressDropMode;
						break;
					case DropMode.CartageType:
						result = CartageTypeDropMode;
						break;
				}

				if (!result.IsEmpty)
				{
					base.JJ_DropMode = result;
				}
			}
		}

		ZString GetDropMode()
		{
			var result = ParentDropMode;
			if (result.IsEmpty)
			{
				result = AddressDropMode;
			}

			if (result.IsEmpty)
			{
				result = CartageTypeDropMode;
			}

			return result;
		}

		ZString ParentDropMode
		{
			get
			{
				var dropMode = CartageInternalType != null ? CartageInternalType.DropMode : ZString.Empty;
				return !dropMode.IsEmpty && Lookups.DropModes.ContainsCode(dropMode) ? dropMode : ZString.Empty;
			}
		}

		ZString AddressDropMode
		{
			get
			{
				var result = ZString.Empty;

				var relevantDocAddress = GetJobDocAddress(RelevantDocAddressType);
				if (relevantDocAddress != null && relevantDocAddress.Address != null)
				{
					if (CurrentChangingDocAddressType.HasValue && IsMixed)
					{
						result = GetMixedDropMode(relevantDocAddress);
					}
					else if (IsAir)
					{
						result = relevantDocAddress.Address.OA_AIREquipmentNeeded;
					}
					else if (IsLoose)
					{
						result = relevantDocAddress.Address.OA_LCLEquipmentNeeded;
					}
					else
					{
						result = relevantDocAddress.Address.OA_FCLEquipmentNeeded;
					}
				}

				return result;
			}
		}

		JobDocAddress GetJobDocAddress(DocAddressType docAddressType)
		{
			return DocAddresses.FindByDocAddressType(docAddressType);
		}

		DocAddressType RelevantDocAddressType
		{
			get
			{
				var result = DocAddressType.None;

				if (CurrentChangingDocAddressType.HasValue)
				{
					result = CurrentChangingDocAddressType.Value;
				}
				else if (IsLoose)
				{
					result = RelevantLooseDocAddressType;
				}
				else
				{
					result = RelevantContainerisedDocAddressType;
				}

				return result;
			}
		}

		DocAddressType RelevantLooseDocAddressType
		{
			get { return HasLooseCartageType ? CommonCartageAddressHelper.GetRelevantDocAddressType(CartageType.LooseBooking) : DocAddressType.None; }
		}

		DocAddressType RelevantContainerisedDocAddressType
		{
			get { return HasContainerisedCartageType ? CommonCartageAddressHelper.GetRelevantDocAddressType(CartageType.ContainerizedBooking) : DocAddressType.None; }
		}

		ZString GetMixedDropMode(JobDocAddress relevantDocAddress)
		{
			ZString result = "";
			if (relevantDocAddress != null && relevantDocAddress.Address != null)
			{
				var hasLooseMovesWithAddress = HasLooseMoveWithAddressType(relevantDocAddress.DocAddressType);
				var hasContainerisedMovesWithAddress = HasContainerisedMoveWithAddressType(relevantDocAddress.DocAddressType);
				if (hasLooseMovesWithAddress && hasContainerisedMovesWithAddress)
				{
					result = relevantDocAddress.Address.OA_FCLEquipmentNeeded;
				}
				else if (hasLooseMovesWithAddress)
				{
					result = relevantDocAddress.Address.OA_LCLEquipmentNeeded;
				}
				else if (hasContainerisedMovesWithAddress)
				{
					result = relevantDocAddress.Address.OA_FCLEquipmentNeeded;
				}
			}

			return result;
		}

		ZString CartageTypeDropMode
		{
			get
			{
				ZString result = "";
				if (CartageType != null)
				{
					if (HasLooseCartageType)
					{
						result = CartageType.LooseBooking.E4_EquipmentGroup;
					}
					else if (HasContainerisedCartageType)
					{
						result = CartageType.ContainerizedBooking.E4_EquipmentGroup;
					}
				}
				return result;
			}
		}

		bool HasLooseCartageType
		{
			get { return IsLoose && CartageType != null && CartageType.LooseBooking != null; }
		}

		bool HasContainerisedCartageType
		{
			get { return IsContainerised && CartageType != null && CartageType.ContainerizedBooking != null; }
		}

		ZString CartageTypeLooseDropMode
		{
			get { return HasLooseCartageType ? CartageType.LooseBooking.E4_EquipmentGroup : ZString.Empty; }
		}

		ZString CartageTypeContainerisedDropMode
		{
			get { return HasContainerisedCartageType ? CartageType.ContainerizedBooking.E4_EquipmentGroup : ZString.Empty; }
		}

		bool ISupportDataImporting.IsImportingData
		{
			get { return isImportingData; }
			set { isImportingData = value; }
		}

		public bool IsImportingData
		{
			get { return isImportingData; }
		}
		bool isImportingData;

		ZString ISailingParentFindBox.LoadPort
		{
			get { return PortOfLoading; }
		}

		ZString ISailingParentFindBox.DischargePort
		{
			get { return PortOfDischarge; }
		}

		ZString ISailingParentFindBox.Origin
		{
			get { return PortOfLoading; }
		}

		ZString ISailingParentFindBox.Destination
		{
			get { return PortOfDischarge; }
		}

		ZGuid ISailingParentFindBox.SailingPK
		{
			get { return JJ_JX_Sailing; }
			set
			{
				if (HasParent)
				{
					throw new NotSupportedException("Can not set Sailing on an Internal Port Transport");
				}

				JJ_JX_Sailing = value;
			}
		}

		ZString ISailingParentFindBox.TransportMode
		{
			get { return !JJ_ShippingTransportMode.IsEmpty ? JJ_ShippingTransportMode.ToString() : Constants.TransportModes.Sea; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Cartage; }
		}

		ZString IRelatedJob.JobDescription
		{
			get { return HumanReadableName; }
		}

		ZString IRelatedJob.JobNumber
		{
			get { return JJ_ConsignmentID; }
		}

		ZString IRelatedJob.JobStatus
		{
			get { return JJ_IsCancelled ? Res.GetString("72149d22-05ac-4020-8683-571228871175", "Canceled") : Res.GetString("ebcaf5b8-213e-4ff1-a9e6-06f8b524bec1", "Active"); }
		}

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit
		{
			get { return Res.GetString("2614CA1A-11B8-4637-86CF-ABF7FC4CF316", "Consignee, Consignor or Local Client for Billing"); }
		}

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback { get; set; }

		event EventHandler<SecurityLoginEventArgs> ICreditControlledDocumentDelivery.GetDocumentLogin
		{
			add { documentLogin += value; }
			remove { documentLogin -= value; }
		}

		event EventHandler<SecurityLoginEventArgs> documentLogin;

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			if (documentLogin != null)
			{
				documentLogin(this, e);
			}
		}

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get
			{
				return GetOrganisationsForCreditChecks();
			}
		}

		OrgHeader[] GetOrganisationsForCreditChecks()
		{
			var result = new List<OrgHeader>();
			if (LocalClient != null && this.IsCreditLimitCheckRequired(OrgCodes.LocalClient))
			{
				result.Add(LocalClient);
			}
			if (this.IsCreditLimitCheckRequired(OrgCodes.Consignor))
			{
				result.AddRange(DocAddresses.FindDocAddressesByType(DocAddressType.LocalCartageExporter).Select(d => d.Organisation).Where(o => o != null));
			}
			if (this.IsCreditLimitCheckRequired(OrgCodes.Consignee))
			{
				result.AddRange(DocAddresses.FindDocAddressesByType(DocAddressType.LocalCartageImporter).Select(d => d.Organisation).Where(o => o != null));
			}
			return result.ToArray();
		}

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get { return false; }
		}

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => false;

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties()
		{
			return Array.Empty<ScreeningParty>();
		}

		string[] IRelatedJobNumber.JobNumber
		{
			get { return new string[] { JJ_ConsignmentID }; }
		}

		public void SetNotificationSubscriber(INotifications iNotify)
		{
			this.notify = iNotify;
		}

		internal void NotificationsQueryUser(IQueryUserEventArgs e)
		{
			NotificationSubscriber.QueryUser(e);
		}

		INotifications NotificationSubscriber
		{
			get { return notify ?? (notify = new Notify()); }
		}

		INotifications notify;

		class Notify : INotifications, INotificationSubscriberQueryUser
		{
			void INotifications.Add(INotification notification)
			{
			}

			void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
			{
				var queryUserEventArgs = e as QueryUserYesNoEventArgs;
				if (queryUserEventArgs != null)
				{
					queryUserEventArgs.Response = true;
				}

				var dropModeEventArgs = e as QueryUserCartageTypeDropModeEventArgs;
				if (dropModeEventArgs != null)
				{
					dropModeEventArgs.Response = DropMode.None;
				}
			}
		}

		CusEntryNumAdditionalReferenceCollection IAdditionalReferenceNumberSupporter.AdditionalReferenceNumbers
		{
			get { return AdditionalReferenceNumbers; }
		}

		void IAdditionalReferenceNumberSupporter.CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications iNotify)
		{
		}

		bool IAdditionalReferenceNumberSupporter.IncludeSpecialCustomsInstructionsItems
		{
			get { return false; }
		}

		void IAdditionalReferenceNumberSupporter.OnEntryNumChanged(CusEntryNumber additionalReferenceNumber)
		{
		}

		void IAdditionalReferenceNumberSupporter.AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number)
		{
		}

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			var result = new CodeDescriptionPairList();

			foreach (CodeDescriptionPair regType in WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.GetCodeDescriptionPairList())
			{
				var customType = new CustomsNumberTypeCodeDescription(regType.Code, regType.MultilingualDescription, false);
				result.Add(customType);
			}

			return result;
		}

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new CommonCartageRatingAdaptersProvider(this); }
		}

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.LocalTransport; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return false; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return false; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { JJ_E3_NKJobType, JJ_OrderReferenceNumber }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}

		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		void IConsignmentService.LogServicesCommenced()
		{
			LogServicesCommenced();
		}

		StmALog LogServicesCommenced()
		{
			return Logs.CreateRecreateOrUpdateEventLog(AutoEvents.ServiceCommenced, EstimateActual.Actual, ZDateTimeOffset.Now, JJ_ConsignmentID,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Transport));
		}

		StmALog LogServicesCancelled()
		{
			return Logs.CreateRecreateOrUpdateEventLog(AutoEvents.ServiceCancelled, EstimateActual.Actual, ZDateTimeOffset.Now, JJ_ConsignmentID,
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Transport));
		}

#if DEBUG

		public void SetParent(ICartageParent parent)
		{
			JJ_ParentID = ((BusinessObject)parent).PK;
			JJ_ParentTableCode = parent.CartageParentTableCode;
			cartageParent = parent;
		}

#endif
	}

	public class CommonCartageInvoicingSupporter : JobInvoicingSupporter
	{
		public CommonCartageInvoicingSupporter(CommonCartage parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly CommonCartage parent;

		public override ZString ServiceLevel => parent.JJ_RS_NKServiceLevel;

		public override ZDecimal ActualVolume
		{
			get { return parent.JJ_Volume; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return parent.JJ_VolumeUQ; }
		}

		public override ZDecimal ActualWeight
		{
			get { return parent.JJ_Weight; }
		}

		public override ZString ActualWeightUnit
		{
			get { return parent.JJ_WeightUQ; }
		}

		public override ZDateTime ESP
		{
			get { return parent.JJ_EstimatedPickup; }
		}

		public override ZDateTime ESD
		{
			get { return parent.JJ_EstimatedDelivery; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.TransportJobAuditBilling;
		}

		/// <summary>
		/// For Port Transport, Billing uses the Consignor for the Bill To address
		/// </summary>
		public override OrgHeader Consignor
		{
			get
			{
				var client = parent.BookingParty;
				if (client == null)
				{
					var billTo = parent.CartageType != null ? GetCartageTypeBillToParty() : GetBillToPartyFallback();
					var billToRelatedOrg = GetRelatedTransportProviderBillTo(billTo);
					client = billToRelatedOrg ?? (billTo != null ? billTo.Organisation : null);
				}

				return client;
			}
		}

		public override ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
		{
			return GetOperationsSignificantDate(significantDateCode);
		}

		public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
		{
			if (parent.IsDeleted)
			{
				return ZDateTime.Empty;
			}
			else if (parent.JJ_A_JCL.IsValid)
			{
				return parent.JJ_A_JCL;
			}
			else if (parent.JJ_EstimatedDelivery.IsValid)
			{
				return parent.JJ_EstimatedDelivery;
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		JobDocAddress GetCartageTypeBillToParty()
		{
			JobDocAddress result = null;

			if (!parent.HasParent)
			{
				var cartageAddressHelper = CommonCartageAddressHelper.ByJobType(parent.CartageType);

				for (int i = 0; i < cartageAddressHelper.AddressOrgTypes.Count; i++)
				{
					if (cartageAddressHelper.AddressOrgTypes[i].E5_IsBillToParty)
					{
						result = GetDocAddressByNumber(i);
						break;
					}
				}
			}

			return result;
		}

		JobDocAddress GetBillToPartyFallback()
		{
			JobDocAddress result = null;

			var addresses = parent.DocAddresses;
			var priorities = new[] { DocAddressType.LocalCartageImporter, DocAddressType.LocalCartageExporter, DocAddressType.LocalCartageCFS };
			priorities.FirstOrDefault(d => (result = addresses.FindByDocAddressType(d)) != null);

			return result;
		}

		JobDocAddress GetDocAddressByNumber(int addressNumberZeroBased)
		{
			switch (addressNumberZeroBased)
			{
				case 0:
					return parent.FirstDocAddress;
				case 1:
					return parent.SecondDocAddress;
				case 2:
					return parent.ThirdDocAddress;
				case 3:
					return parent.FourthDocAddress;
			}

			return null;
		}

		OrgHeader GetRelatedTransportProviderBillTo(JobDocAddress docAddress)
		{
			OrgHeader result = null;

			var org = docAddress != null ? docAddress.Organisation : null;
			if (org != null)
			{
				var direction = GetRelatedOrgDirection();
				var transportMode = GetRelatedOrgTransportMode();
				var containerMode = GetRelatedOrgContainerMode();
				result = !direction.IsEmpty && !transportMode.IsEmpty ? org.GetRelatedParty(RelatedPartyTypeList.Codes.LocalTransportBillTo, direction, transportMode, containerMode) : null;
			}

			return result;
		}

		ZString GetRelatedOrgContainerMode()
		{
			var result = ZString.Empty;
			if (parent.IsContainerised)
			{
				result = Constants.ContainerModes.FCL;
			}
			else if (parent.IsLoose)
			{
				result = Constants.ContainerModes.LCL;
			}
			return result;
		}

		ZString GetRelatedOrgTransportMode()
		{
			var result = ZString.Empty;
			if (parent.IsAir)
			{
				result = Constants.TransportModes.Air;
			}
			else if (parent.IsContainerised || parent.IsLoose)
			{
				result = Constants.TransportModes.Sea;
			}
			return result;
		}

		ZString GetRelatedOrgDirection()
		{
			var result = "";

			if (parent.IsExportOrOrigin)
			{
				result = RelatedPartyDirectionList.Codes.Pickup;
			}
			else if (parent.IsImportOrDestination)
			{
				result = RelatedPartyDirectionList.Codes.Delivery;
			}

			return result;
		}

		public override ZString ConsolType
		{
			get { return Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.LocalCartage; }
		}

		public override ZString ContainerMode
		{
			get { return parent.IsContainerised ? Core.Constants.ContainerModes.FCL : Core.Constants.ContainerModes.LCL; }
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !parent.IsInDatabaseIncludingChildren; }
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			OrgHeader result = null;
			foreach (CommonBookedCtgMove move in parent.BookedMovesCollection)
			{
				foreach (CommonCartageLeg leg in move.CartageLegs)
				{
					if (leg.TransportCo != null)
					{
						result = leg.TransportCo;
						break;
					}
				}
			}
			return result;
		}

		public override bool IsImport
		{
			get
			{
				// This doesn't look right. Should be just IsImport as it is in other places of the system
				return parent.IsImportOrDestination;
			}
		}

		public override bool IsExport
		{
			get
			{
				// This doesn't look right. Should be just IsExport as it is in other places of the system
				return parent.IsExportOrOrigin;
			}
		}

		public override bool IsDomestic
		{
			get { return parent.IsDomestic(); }
		}

		public override RefUNLOCO Origin
		{
			get
			{
				RefUNLOCO origin = base.Origin;
				if (IsExport || IsDomestic)
				{
					origin = GlbBranch.CurrentBranch.HomePort;
				}
				return origin;
			}
		}

		public override RefUNLOCO Destination
		{
			get
			{
				RefUNLOCO destination = base.Destination;
				if (IsImport || IsDomestic)
				{
					destination = GlbBranch.CurrentBranch.HomePort;
				}
				return destination;
			}
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.TransportJobInvoicing;
		}

		public override GlbBranch OperationsBranch
		{
			get { return parent.Branch; }
		}

		public override ZGuid OverriddenDepartmentPK
		{
			get { return parent.CartageType != null ? parent.CartageType.E3_GE : GetDepartmentFallback(); }
		}

		/// <summary>
		/// TDD - Cartage Domestic Delivery
		/// TDP - Cartage Domestic Pickup
		/// TEA - Cartage Export Air
		/// TES - Cartage Export Sea
		/// TFC - Cartage FCL
		/// TFL - Cartage FCL Rail
		/// TIA - Cartage Import Air
		/// TIS - Cartage Import Sea
		/// TLA - Cartage Air
		/// TLC - Cartage LCL Sea
		/// TLL - Cartage LCL Rail
		/// TOT - Cartage Other
		/// </summary>
		ZGuid GetDepartmentFallback()
		{
			GlbDepartment resultDepartment = null;

			if (parent.IsAir)
			{
				if (parent.IsExport)
				{
					resultDepartment = GetDepartment("TEA");
				}
				else if (parent.IsImport)
				{
					resultDepartment = GetDepartment("TIA");
				}
				else
				{
					resultDepartment = GetDepartment("TLA");
				}
			}
			else if (parent.IsSea)
			{
				if (parent.IsExport)
				{
					resultDepartment = GetDepartment("TES");
				}
				else if (parent.IsImport)
				{
					resultDepartment = GetDepartment("TIS");
				}
				else if (parent.IsLoose)
				{
					resultDepartment = GetDepartment("TLC");
				}
			}
			else if (parent.IsRail)
			{
				if (parent.IsContainerised)
				{
					resultDepartment = GetDepartment("TFL");
				}
				else
				{
					resultDepartment = GetDepartment("TLL");
				}
			}
			else
			{
				if (parent.IsDomestic)
				{
					if (parent.IsOrigin)
					{
						resultDepartment = GetDepartment("TDP");
					}
					else if (parent.IsDestination)
					{
						resultDepartment = GetDepartment("TDD");
					}
				}
				else if (parent.IsContainerised)
				{
					resultDepartment = GetDepartment("TFC");
				}
			}

			resultDepartment = resultDepartment ?? GetDepartment("TOT");

			if (!resultDepartment.GE_IsActive)
			{
				resultDepartment = GetFirstActiveSystemLocalTransportDepartmentByCodeDescending() ?? resultDepartment;
			}

			return resultDepartment.PK;
		}

		GlbDepartment GetDepartment(ZString departmentCode)
		{
			return parent.Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode);
		}

		GlbDepartment GetFirstActiveSystemLocalTransportDepartmentByCodeDescending()
		{
			var query = new ZQuery(
				new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "T"),
				new ZQuery(
					new ZQuery(GlbDepartmentSchema.GE_IsActive, true),
					new ZQuery(GlbDepartmentSchema.GE_SystemCode, true)));
			query.OrderBy = GlbDepartmentSchema.Constants.GE_Code + " DESC";
			return parent.Factory.LoadTop1<GlbDepartment>(query);
		}

		public override ZString TransportMode
		{
			get { return parent.JJ_ShippingTransportMode; }
		}

		public override ZString EditSecurityMessage
		{
			get { return EditSecurityMessageCore; }
		}

		protected virtual ZString EditSecurityMessageCore
		{
			get { return ZString.Empty; }
		}

		public override bool EditSecurityLock
		{
			get { return EditSecurityLockCore; }
		}

		protected virtual bool EditSecurityLockCore
		{
			get { return false; }
		}

		public override int ContainerCount
		{
			get { return parent.Containers.Count(); }
		}

		public override ZDecimal TEUCount
		{
			get { return parent.Containers.Sum(c => c.JC_Calc_TEUCount); }
		}

		public override int OuterPackTotal
		{
			get { return parent.TotalLooseBookedPackages; }
		}

		public override ZString HouseBillNumber
		{
			get { return parent.JJ_WaybillNumber; }
		}

		public override ZString VoyageVesselOrFlightDate
		{
			get { return GetVoyageVesselOrFlightDatesCore(parent.JJ_ShippingTransportMode, parent.E_DEP, parent.Vessel, parent.VoyageFlight); }
		}

		protected override bool IncludeInConsolCostingCore(bool includeRelatedShipments)
		{
			return !parent.IsCancelled;
		}
	}
}

