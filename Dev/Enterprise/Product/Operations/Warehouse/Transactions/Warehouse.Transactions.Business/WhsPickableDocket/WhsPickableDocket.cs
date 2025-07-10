using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsPickableDocket : WhsDocket, IEDocsProvider, IWhsJobTemplateCopyable, IJobWithTransportCompany, ICreateDocketLineFromInventory, IWhsPickableDocket
	{
		#region Schema

		public new abstract class Schema : WhsDocket.Schema
		{
			public const string WD_HandlingInstructions = "WD_HandlingInstructions";
			public const string WD_IsOrderSelectedForFinalisation = "WD_IsOrderSelectedForFinalisation";
			public const string VehicleNo = "VehicleNo";
		}

		#endregion

		#region TypeDecider

		new public static readonly WhsPickableDocketTypeDecider TypeDecider = new WhsPickableDocketTypeDecider();

		#endregion

		#region Constructors

		protected WhsPickableDocket(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WD_PickOption = WhsPickOption.Codes.Auto;
		}

		#endregion

		#region Related Entities

		#region Consignee

		public OrgHeader Consignee => this.LoadJobDocAddressQuickly(ConsigneeDocAddressRequirement.DefaultDocAddressType)?.GetOrganisation();

		public ZGuid ConsigneePK
		{
			get { return ConsigneeDocAddress.OrganisationPK; }
			set
			{
				ConsigneeDocAddress.OrganisationPK = value;
				ValidateConsigneePK();
			}
		}

		public OrgAddress ConsigneeAddress => !ConsigneeDocAddress.E2_AddressOverride ? ConsigneeDocAddress.Address : null;

		public ZGuid ConsigneeAddressPK
		{
			get => !ConsigneeDocAddress.E2_AddressOverride ? ConsigneeDocAddress.E2_OA_Address : ZGuid.Empty;
			set
			{
				ConsigneeDocAddress.E2_OA_Address = value;
				ValidateConsigneePK();
			}
		}

		void OnConsigneeDocAddressChanged(object sender, EventArgs e)
		{
			BeforeOnConsigneeDocAddressChanged();

			ConsigneeDocAddressChanged?.Invoke(sender, e);

			OnConsigneeDocAddressChangedCore();
		}

		protected virtual void BeforeOnConsigneeDocAddressChanged()
		{
		}

		protected virtual void OnConsigneeDocAddressChangedCore()
		{
		}

		public event EventHandler ConsigneeDocAddressChanged;

		public JobDocAddress ConsigneeDocAddress
		{
			get { return ConsigneeDocAddressCore; }
		}

		protected virtual JobDocAddress ConsigneeDocAddressCore
		{
			get
			{
				if (consigneeDocAddress == null || consigneeDocAddress.IsDeleted)
				{
					consigneeDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsigneeDocAddressRequirement);
					consigneeDocAddress.DocAddressChanged += OnConsigneeDocAddressChanged;
					consigneeDocAddress.E2_AddressOverrideInfo.ValueChanged += Consignee_E2_AddressOverrideInfo_ValueChanged;
					consigneeDocAddress.E2_OA_AddressInfo.ValueChanged += Consignee_E2_OA_AddressInfo_ValueChanged;
					consigneeDocAddress.ReadOnlyStrategy = GetConsigneeDocAddressReadOnlyStrategy();
				}

				return consigneeDocAddress;
			}
		}

		void Consignee_E2_AddressOverrideInfo_ValueChanged(object sender, EventArgs e)
		{
			OnConsigneeAddressOverrideChanged(e);
		}

		protected virtual void OnConsigneeAddressOverrideChanged(EventArgs e)
		{
		}

		void Consignee_E2_OA_AddressInfo_ValueChanged(object sender, EventArgs e)
		{
			OnConsigneeAddressChanged(e);
		}

		protected virtual void OnConsigneeAddressChanged(EventArgs e)
		{
		}

		JobDocAddress consigneeDocAddress;

		protected bool IsConsigneeDocAddressInitialised
		{
			get { return consigneeDocAddress != null; }
		}

		protected abstract IJobDocAddressReadOnlyStrategy GetConsigneeDocAddressReadOnlyStrategy();

		#endregion

		public new WhsPickableDocket ParentDocket
		{
			get { return (WhsPickableDocket)base.ParentDocket; }
		}

		public WhsPick Pick
		{
			get { return Factory.Load<WhsPick>(WD_WP); }
		}

		public WhsLocation CrossDockLocation
		{
			get { return Factory.Load<WhsLocation>(WD_WL_CrossDock); }
		}

		#region WorkOrders

		public IEnumerable<WhsWorkOrder> CurrentWorkOrders
		{
			get { return Factory.Load<WhsWorkOrder>(CurrentWorkOrdersQuery); }
		}

		ZQuery CurrentWorkOrdersQuery
		{
			get
			{
				ZQuery result = new ZQuery();
				result.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.WorkOrder);
				result.AddToFilter(WhsDocketSchema.WD_WD_ParentDocket, PK);
				result.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);
				result.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Cancelled);

				return result;
			}
		}

		WhsWorkOrder[] GetWorkOrdersIncludingChildrenFromDb()
		{
			#region Common Table Expression (SQL)

			string commonTableExpressionSql =

				@"
				WITH WhsDocketTree
				(
					WD_PK,
					WD_WD_ParentDocket,
					DocketLevel
				)
				AS
				(
					-- Anchor member definition
					SELECT
							Docket.WD_PK,
							Docket.WD_WD_ParentDocket,
							0 AS DocketLevel
					FROM
							dbo.WhsDocket AS Docket
					WHERE
							WD_WD_ParentDocket = @ParentPK
				            
					UNION ALL
				    
					-- Recursive member definition
					SELECT
							Docket.WD_PK,
							Docket.WD_WD_ParentDocket,
							ParentDocket.DocketLevel + 1
					FROM
							dbo.WhsDocket Docket
							INNER JOIN WhsDocketTree AS ParentDocket
							ON ParentDocket.WD_PK = Docket.WD_WD_ParentDocket
				)

				-- Statement that executes the CTE
				SELECT
					  WhsDocket.WD_PK
				FROM
					  WhsDocketTree
					  INNER JOIN dbo.WhsDocket ON WhsDocket.WD_PK = WhsDocketTree.WD_PK
				WHERE
					  WhsDocket.WD_DocketType = @WorkOrderType
				";

			#endregion

			var childWorkOrdersPkCollection = new DynamicBusinessObjectCollection(Factory);
			childWorkOrdersPkCollection.Load(commonTableExpressionSql, new ZSqlParameter[]
			{
				ZSqlParameter.New("@ParentPK", PK, WhsDocketSchema.PK),
				ZSqlParameter.New("@WorkOrderType", DocketType.Codes.WorkOrder, WhsDocketSchema.WD_DocketType)
			});

			WhsWorkOrder[] result;

			if (childWorkOrdersPkCollection.Count > 0)
			{
				var query = new ZQuery(WhsDocketSchema.PK, childWorkOrdersPkCollection.Select(c => (ZGuid)c[WhsDocketSchema.PK]));
				result = Factory.Load<WhsWorkOrder>(query);
			}
			else
			{
				result = Array.Empty<WhsWorkOrder>();
			}

			return result;
		}

		#endregion

		#region Lines, AllLines, SelectedOrderLines

		public new WhsPickableDocketLineCollection Lines
		{
			get { return (WhsPickableDocketLineCollection)base.Lines; }
		}

		[ChildEditableTestExclude]
		public WhsPickableDocketLineCollection AllLines
		{
			get { return allLines ?? (allLines = GetNewAllLines()); }
		}

		public List<WhsPickableDocketLine> SelectedOrderLines
			=> Pick?.ParentForm?.SelectedOrderLines
				?? new List<WhsPickableDocketLine>(LinesForSelectedOrderLines());

		protected virtual WhsPickableDocketLine[] LinesForSelectedOrderLines()
		{
			return Lines.ToArray<WhsPickableDocketLine>();
		}

		protected sealed override WhsDocketLineCollection GetNewDocketLineCollection()
		{
			return GetNewPickableDocketLineCollection();
		}

		protected abstract WhsPickableDocketLineCollection GetNewPickableDocketLineCollection();

		protected abstract WhsPickableDocketLineCollection GetNewAllLines();

		WhsPickableDocketLineCollection allLines;

		#region AddFetchHintsForPickLines

		public void AddFetchHintsForPickLines()
		{
			// We do not want to add the fetch hints for the pickable docket if we have already added them previously when called from pickable docket line.
			// This cache will be cleared after factory save.
			Factory.GetCachedValue("WhsPickableDocket|AddFetchHintsForPickLines|" + PK, () => AddFetchHintsForPickLinesCore(this), CacheStalenessPolicy.StaleOnFactorySave);
		}

		static bool AddFetchHintsForPickLinesCore(WhsPickableDocket pickableDocket)
		{
			var allPickLines = pickableDocket.Factory.Load<WhsPickLine>(GetPickLinesForAllDocketLinesQuery(pickableDocket));
			var hasPick = !pickableDocket.WD_WP.IsEmpty;

			foreach (var pickLine in allPickLines)
			{
				// Tested in many DBHits tests, including some in WhsPickTest and WhsOrderTest.FinaliseDocketDBHits
				pickableDocket.Factory.AddFetchHint(WhsDocketLineSchema.Constants.TableName, pickLine.WZ_WE_InventoryLine);
			}
			return true;
		}

		static ZQuery GetPickLinesForAllDocketLinesQuery(WhsPickableDocket pickableDocket)
		{
			var pickLineQuery = new ZQuery { AllowTableValuedParameters = true };
			pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_WE_TransactionLine, pickableDocket.AllLines.Select(bizO => bizO.PK));
			return pickLineQuery;
		}

		#endregion

		#endregion

		#region RelatedJobs

		protected override List<IRelatedJob> GetRelatedJobsCore()
		{
			var result = base.GetRelatedJobsCore();

			if (CanHaveChildWorkOrders)
			{
				result.AddRange(GetWorkOrdersIncludingChildrenFromDb());
			}

			return result;
		}

		protected abstract bool CanHaveChildWorkOrders { get; }

		#endregion

		#region TransportCoDocAddress

		public JobDocAddress TransportCoDocAddress => TransportCoDocAddressCore;
		protected abstract JobDocAddress TransportCoDocAddressCore { get; }

		#endregion

		#region TransportCo

		protected override OrgHeader TransportCoCore => this.GetTransportCo();

		#endregion

		#endregion

		#region Properties

		#region TransportCoPK

		public ZGuid TransportCoPK
		{
			get => this.GetTransportCoPK();
			set => this.SetTransportCoPK(value);
		}

		#endregion

		#region TransportCoName

		protected override ZString TransportCoNameCore => this.GetTransportCoName();

		#endregion

		#region TransportCoNameOrPK For Grid Binding

		public ZString TransportCoFieldType => this.GetTransportCoFieldType();

		protected override ZString TransportCoNameOrPKCore
		{
			get => this.GetTransportCoNameOrPK();
			set => this.SetTransportCoNameOrPK(value);
		}

		public ZPropertyInfo TransportCoNameOrPKInfo => this.GetTransportCoNameOrPKInfo(GetZPropertyInfo);

		protected override int TransportCoNameOrPKMaxLength => this.GetTransportCoNameOrPKMaxLength();

		#endregion

		#region Flags

		#region IsAttachingToPick

		public bool IsAttachingToPick
		{
			get { return isAttachingToPickSemaphore != null && isAttachingToPickSemaphore.IsSuspended; }
		}

		#endregion

		#region IsDetachingFromPick

		public bool IsDetachingFromPick
		{
			get { return isDetachingToPickSemaphore != null && isDetachingToPickSemaphore.IsSuspended; }
		}

		#endregion

		#region IsFinalisedOrCancelledOrNotPicked

		public bool IsFinalisedOrCancelledOrNotPicked => IsFinalisedOrCancelled || !IsAttachedToPickButNotFinalised;

		#endregion

		#region IsPickFinalising

		public bool IsPickFinalising => Pick?.IsFinalising ?? false;

		#endregion

		#region IsPartiallyOrFullyPickedFromPutawayLocation

		public bool IsPartiallyOrFullyPickedFromPutawayLocation => AllLines.Cast<WhsPickableDocketLine>().Any(l => l.IsPartiallyOrFullyPickedFromPutawayLocation);

		#endregion

		#region IsCurrentlyBeingPickedFromPutawayLocation

		public bool IsCurrentlyBeingPickedFromPutawayLocation => AllLines.Cast<WhsPickableDocketLine>().Any(l => l.IsCurrentlyBeingPickedFromPutawayLocation);

		#endregion

		#region HasDangerousGoods

		[ResourceStringData("WhsPickableDocket|HasDangerousGoods", Caption = "Has Dangerous Goods", ShortCaption = "Has DGs")]
		public ZBool HasDangerousGoods => Lines.Cast<WhsPickableDocketLine>().Any(l => l.IsDangerousGood);

		#endregion

		#endregion

		#region WD_IsOrderSelectedForFinalisation

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public ZBool WD_IsOrderSelectedForFinalisation
		{
			get { return wd_IsOrderSelectedForFinalisation; }
			set { SetNonPersistentPropertyValue(WD_IsOrderSelectedForFinalisationInfo, ref wd_IsOrderSelectedForFinalisation, value); }
		}

		public ZPropertyInfo WD_IsOrderSelectedForFinalisationInfo
		{
			get { return GetZPropertyInfo(Schema.WD_IsOrderSelectedForFinalisation); }
		}

		ZBool wd_IsOrderSelectedForFinalisation;

		#endregion

		#region WD_HandlingInstructions

		[MaxLength(50000)]
		public ZString WD_HandlingInstructions
		{
			get
			{
				// Special instructions are basically obsolete in Whs but we need to still pull them from
				// related parties because many of our clients have Special Instructions setup on their Orgs.
				// They are replaced by Goods Handling.
				ZString result = "";

				AppendNote(this, PredefinedNoteTypes.Instance.HandlingInstructions, ref result);

				var client = Client;
				AppendNote(client, PredefinedNoteTypes.Instance.HandlingInstructions, ref result);
				AppendNote(client, PredefinedNoteTypes.Instance.SpecialInstructions, ref result);

				var consignee = Consignee;
				if (consignee != client)
				{
					AppendNote(consignee, PredefinedNoteTypes.Instance.HandlingInstructions, ref result);
					AppendNote(consignee, PredefinedNoteTypes.Instance.SpecialInstructions, ref result);
				}

				var transportCo = this.GetTransportCo();
				if (transportCo != client && transportCo != consignee)
				{
					AppendNote(transportCo, PredefinedNoteTypes.Instance.HandlingInstructions, ref result);
					AppendNote(transportCo, PredefinedNoteTypes.Instance.SpecialInstructions, ref result);
				}

				return result;
			}
			set
			{
				var handlingInstructionNote = Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description).FirstOrDefault();
				if (value.IsEmpty)
				{
					if (handlingInstructionNote != null)
					{
						handlingInstructionNote.Delete();
					}
				}
				else
				{
					if (handlingInstructionNote == null)
					{
						Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, value);
					}
					else
					{
						RegisterEditableChildObject(handlingInstructionNote);
						handlingInstructionNote.ST_NoteText = value;
					}
				}
				WD_HandlingInstructionsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo WD_HandlingInstructionsInfo
		{
			get { return GetZPropertyInfo(Schema.WD_HandlingInstructions); }
		}

		void AppendNote(BusinessObject bizO, PredefinedNoteType noteTypeOnBizO, ref ZString noteToAppendTo)
		{
			if (bizO != null)
			{
				foreach (StmNote note in bizO.GetNotes().FindByDescription(noteTypeOnBizO.Description).Where(note => NoteContextsForRelatedNotes.Module.ToString().IndexOf(note.ST_NoteContextModule, StringComparison.Ordinal) != -1)) // tested in TestWD_HandlingInstructions_OnlyAppendInstructionIfModuleMatches
				{
					if (!noteToAppendTo.IsEmpty)
					{
						noteToAppendTo += @"
";
					}
					noteToAppendTo += note.ST_NoteText;
				}
			}
		}

		#endregion

		#region WD_ParentOrderNo

		public ZString WD_ParentOrderNo
		{
			get
			{
				WhsPickableDocket parentDocket = ParentDocket;
				return (parentDocket != null) ? parentDocket.WD_ExternalReference : ZString.Empty;
			}
		}

		#endregion

		#region WD_WP

		public override ZGuid WD_WP
		{
			get { return base.WD_WP; }
			set
			{
				var previousValue = WD_WP;
				base.WD_WP = value;

				if (previousValue != WD_WP)
				{
					if (!IsAttachingToPick && !IsDetachingFromPick) // We refresh collections at the end while bulk changing orders
					{
						// Reserved Pick Lines are dependent on whether the Docket is Picked or not.
						ReservedPickLineCollection.InvalidateAll(Factory);
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateWD_DocketStatus();
					}
				}
			}
		}

		#endregion

		#region WD_DocketStatus

		public override ZString WD_DocketStatus
		{
			get { return base.WD_DocketStatus; }
			set
			{
				var previousValue = WD_DocketStatus;
				base.WD_DocketStatus = value;

				if (previousValue != value)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateWD_WP();
					}
				}
			}
		}

		#endregion

		#region Consignee Name Or PK For Grid Binding

		public ZString ConsigneeFieldType
		{
			get { return ConsigneeDocAddress.E2_AddressOverride ? nameof(FieldType.Text) : nameof(FieldType.Guid); }
		}

		[BusinessObjectTestExclude]
		[List("Lookups.Consignees")]
		[RelatedBusinessObject("Consignee")]
		[ReadOnlyMember(nameof(IsOnlyTheConsigneeOrgReadOnly))]
		public ZString ConsigneeNameOrPK
		{
			get
			{
				ZString result = "";

				if (ConsigneeDocAddress.E2_AddressOverride)
				{
					result = ConsigneeDocAddress.E2_CompanyNameTruncated;
				}
				else if (ConsigneeDocAddress.Organisation != null)
				{
					result = ConsigneeDocAddress.Organisation.PK.ToString();
				}
				else
				{
					result = ZGuid.Empty.ToString();
				}

				return result;
			}
			set
			{
				if (ConsigneeDocAddress.E2_AddressOverride)
				{
					ConsigneeDocAddress.E2_CompanyName = value;
				}
				else
				{
					try
					{
						ConsigneeDocAddress.OrganisationPK = new Guid(value);
					}
					catch (FormatException)
					{
					}
				}
				ConsigneeNameOrPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneeNameOrPKInfo
		{
			get { return GetZPropertyInfo(nameof(ConsigneeNameOrPK), ConsigneeDocAddress.OrganisationPKInfo.HumanReadableName); }
		}

		public int ConsigneeNameOrPK_MaxLength
		{
			get { return ConsigneeDocAddress.E2_AddressOverride ? JobDocAddressSchema.E2_CompanyName.MaxLength : 36; }
		}

		protected virtual bool IsOnlyTheConsigneeOrgReadOnly
		{
			get { return true; }
		}

		#endregion

		#region PickingInstructions

		public ZString PickingInstructions // tested in WhsOrder and needed on WhsPickableDocket for docwrappers
		{
			get
			{
				ZString result = "";

				StmNote[] notes = Notes.FindByDescription(PredefinedNoteTypes.Instance.PickingInstructions.Description);
				if (notes.Length > 0)
				{
					result = notes[0].ST_NoteText;
				}

				return result;
			}
		}

		#endregion

		#region ShortfallExists

		/// <summary>
		/// This property does not update the Shortfall Cache
		/// </summary>
		public ZBool ShortfallExists
		{
			get
			{
				foreach (WhsPickableDocketLine line in GetLinesToCalculateShortfall())
				{
					if (line.GetShortfallExistsStatus_WithoutUpdatingCache())
					{
						return true;
					}
				}
				return false;
			}
		}

		protected abstract WhsDocketLine[] GetLinesToCalculateShortfall();

		#endregion

		#region CanCreateInventoryCore

		protected override bool CanCreateInventoryCore
		{
			get { return false; }
		}

		#endregion

		#region WD_OH_Client

		protected override void OnClientChanged()
		{
			base.OnClientChanged();

			if (WD_OH_Client.IsValid)
			{
				SetClientOrderOptions();
				Lines.Cast<WhsPickableDocketLine>().ForEach(line => line.CalculateExtendedLinePrice());
			}
		}

		void SetClientOrderOptions()
		{
			var clientMiscServ = Client?.MiscServ;
			if (clientMiscServ != null)
			{
				WD_PickOption = clientMiscServ.OM_IMDefaultWarehousePickOption;
				SetClientOrderOptionsCore(clientMiscServ);
			}
		}

		protected virtual void SetClientOrderOptionsCore(OrgMiscServ clientMiscServ)
		{
		}

		#endregion

		#region WD_RequiredDate

		[ReadOnlyMember(nameof(RequiredDateReadOnly))]
		public override ZDateTimeOffset WD_RequiredDate
		{
			get { return base.WD_RequiredDate; }
			set { base.WD_RequiredDate = value; }
		}

		#endregion

		#region WD_AddPalletWeightToOrder

		// this is only in WhsPickableDocket because of binding
		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZBool WD_AddPalletWeightToOrder
		{
			get => base.WD_AddPalletWeightToOrder;
			set => base.WD_AddPalletWeightToOrder = value;
		}

		#endregion

		#region WD_PalletsSent

		// this is only in WhsPickableDocket because of binding
		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZShort WD_PalletsSent
		{
			get => base.WD_PalletsSent;
			set => base.WD_PalletsSent = value;
		}

		#endregion

		#region WD_WeightSentUserEntered

		// this is only in WhsPickableDocket because of binding
		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZDecimal WD_WeightSentUserEntered
		{
			get => base.WD_WeightSentUserEntered;
			set => base.WD_WeightSentUserEntered = value;
		}

		#endregion

		#region WD_WeightSent

		// this is only in WhsPickableDocket because of binding
		[ReadOnly(true)]
		public override ZDecimal WD_WeightSent
		{
			get => base.WD_WeightSent;
			set => base.WD_WeightSent = value;
		}

		#endregion

		#region WD_CubicSent

		// this is only in WhsPickableDocket because of binding
		[ReadOnlyMember(nameof(CubicSentReadOnly))]
		public override ZDecimal WD_CubicSent
		{
			get { return base.WD_CubicSent; }
			set { base.WD_CubicSent = value; }
		}

		protected virtual bool CubicSentReadOnly => NonStandardReadOnly1;

		#endregion

		#region WD_F3_NKTotalPackType

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZString WD_F3_NKTotalPackType
		{
			get => base.WD_F3_NKTotalPackType;
			set => base.WD_F3_NKTotalPackType = value;
		}

		#endregion

		#region WD_PackagesSent

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZInt WD_PackagesSent
		{
			get => base.WD_PackagesSent;
			set => base.WD_PackagesSent = value;
		}

		#endregion

		#region WD_TotalPallets

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZShort WD_TotalPallets
		{
			get => base.WD_TotalPallets;
			set => base.WD_TotalPallets = value;
		}

		#endregion

		#region WD_TotalUnits

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZDecimal WD_TotalUnits
		{
			get => base.WD_TotalUnits;
			set => base.WD_TotalUnits = value;
		}

		#endregion

		#region UsePackingWeightAndVolume

		// this is only in WhsPickableDocket because of binding
		[ResourceStringData("WhsPickableDocket|UsePackingWeightAndVolume", Caption = "Use Packing Weight and Volume")]
		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public ZBool UsePackingWeightAndVolume
		{
			get => base.WD_AddPalletWeightToOrder;
			set
			{
				base.WD_AddPalletWeightToOrder = value;
				OnUsePackingWeightAndVolumeSet(value);
				UsePackingWeightAndVolumeInfo.RefreshBinding();
			}
		}

		protected virtual void OnUsePackingWeightAndVolumeSet(bool value)
		{
		}

		public ZPropertyInfo UsePackingWeightAndVolumeInfo => GetZPropertyInfo(nameof(UsePackingWeightAndVolume));

		#endregion

		#region GrossWeightSent

		// this is only in WhsPickableDocket because of binding
		[ResourceStringData("WhsPickableDocket|GrossWeightSent", Caption = "Gross Weight")]
		[ReadOnly(true)]
		public ZDecimal GrossWeightSent
		{
			get => grossWeightSent ?? (grossWeightSent = GetGrossWeightSent()).Value;
			protected set => SetNonPersistentPropertyValue(GrossWeightSentInfo, ref grossWeightSent, value);
		}

		ZDecimal? grossWeightSent;

		protected virtual decimal GetGrossWeightSent() => 0m;

		public ZPropertyInfo GrossWeightSentInfo => GetZPropertyInfo(nameof(GrossWeightSent));

		#endregion

		#region TareWeight

		// this is only in WhsPickableDocket because of binding
		[ResourceStringData("WhsPickableDocket|TareWeight", Caption = "Tare Weight")]
		public ZDecimal TareWeight => Math.Max(GrossWeightSent - WD_WeightSentUserEntered, 0m);

		#endregion

		#region VehicleNo

		[ResourceStringData("WhsPickableDocket|VehicleNumber", Caption = "Vehicle Number", MediumCaption = "Vehicle No")]
		[ActionField(MaxLength = 25)]
		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		[MaxLength(25)]
		public ZString VehicleNo
		{
			get { return GetReference("VHN"); }
			set
			{
				SetReference("VHN", value);
				VehicleNoInfo.RefreshBinding();
			}
		}

		#endregion

		#region VehicleNoInfo

		public ZPropertyInfo VehicleNoInfo => GetZPropertyInfo(Schema.VehicleNo);

		public override void RefreshProxyProperties()
		{
			base.RefreshProxyProperties();
			VehicleNoInfo.RefreshBinding();
		}

		#endregion

		#region SalesChannelCode

		public ZString SalesChannelCode => SalesChannelCodeCore;
		protected virtual ZString SalesChannelCodeCore => string.Empty;

		#endregion

		#region IsHeldInventoryOrder

		public ZBool IsHeldInventoryOrder => Lines.Select(l => !l.WE_WHC_NKOrderedHeldCode.IsEmpty).FirstOrDefault();

		#endregion

		#endregion

		#region ResetOrderAfterDetachingFromPick

		internal void ResetOrderAfterDetachingFromPick()
		{
			ResetOrderAfterDetachingFromPickCore();
		}

		protected virtual void ResetOrderAfterDetachingFromPickCore()
		{
		}

		#endregion

		#region IsRecalculateOrderPricing

		public ZBool IsRecalculateOrderPricing => IsRecalculateOrderPricingCore;

		protected virtual ZBool IsRecalculateOrderPricingCore => Client?.MiscServ?.OM_WhsIsRecalculateOrderPricing ?? false;

		#endregion

		#region CancelledPickNo

		public ZString CancelledPickNo { get; set; }

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsPickableDocketFetchStrategy(this);
		}

		#endregion

		#region ReadOnly

		protected override bool StandardReadOnly
		{
			get { return IsAttachedToPickButNotFinalised || IsFinalisedOrCancelled; }
		}

		protected override bool NonStandardReadOnly1
		{
			get { return IsCancelled || (IsPickFinalised && !IsPostFinalizeEditAllowed); }
		}

		#endregion

		#region IsFulfillmentRuleMet

		public bool IsFulfillmentRuleMet
		{
			get { return IsFulfillmentRuleMetCore; }
		}

		protected virtual bool IsFulfillmentRuleMetCore
		{
			get { return true; }
		}

		#endregion

		#region Shortfalls

		public ShortfallManager ShortfallManager => shortfallManager ?? (shortfallManager = new ShortfallManager(this));
		ShortfallManager shortfallManager;

		public bool GetShortfallExistsStatus()
		{
			foreach (WhsPickableDocketLine line in GetLinesToPick().ToArray())
			{
				if (!line.IsComponentLineOnSalesOrder && !line.IsBOMProductPickedOnSalesOrder && line.GetShortfallExistsStatus())
				{
					return true;
				}
			}
			return false;
		}

		public Semaphore IsCalculatingShortfallForAllLinesSemaphore
		{
			get { return isCalculatingShortfallForAllLinesSemaphore ?? (isCalculatingShortfallForAllLinesSemaphore = new Semaphore()); }
		}

		internal bool CalculateShortfallForAllLinesInOneDbHit_WasCalled
		{
			get { return calculateShortfallForAllLinesInOneDbHit_WasCalled; }
			set
			{
				calculateShortfallForAllLinesInOneDbHit_WasCalled = value;

				#region Test
#if DEBUG
				if (value)
				{
					CalculateShortfallForAllLinesInOneDbHit_WasCalled_HitCountForTest++;
				}
#endif
				#endregion
			}
		}

		#region Test
#if DEBUG
		public int CalculateShortfallForAllLinesInOneDbHit_WasCalled_HitCountForTest
		{
			get;
			private set;
		}
#endif
		#endregion

		Semaphore isCalculatingShortfallForAllLinesSemaphore;
		bool calculateShortfallForAllLinesInOneDbHit_WasCalled;

		#endregion

		#region Status Change

		protected override void OnDocketStatusChangedCore()
		{
			if (IsConsigneeDocAddressInitialised)
			{
				ConsigneeDocAddress.RefreshBinding();
			}

			base.OnDocketStatusChangedCore();
		}

		protected override WhsDocketLineCollection LinesToPropagateStatusAndFinDateChange
		{
			get { return AllLines; }
		}

		#endregion

		#region Attached To Pick

		#region IsAttachedToPickButNotFinalised

		public bool IsAttachedToPickButNotFinalised => IsAttachedToPick && !IsFinalised;

		#endregion

		#region IsAttachedToPick

		public bool IsAttachedToPick => WD_WP.IsValid;

		#endregion

		#region IsPickFinalised

		public bool IsPickFinalised
		{
			get
			{
				var pick = Pick;
				return pick != null && pick.IsFinalised;
			}
		}

		#endregion

		#region GetLinesToPick

		/// <summary>
		/// The lines are cached, so if a line's data changes such that it now or no longer meets the filter criteria --
		/// this collection won't reflect the change. Use GetLinesToPick() instead because it is not cached.
		/// 
		/// (This could probably be replaced with an ActiveBusinessObjectCollection at some stage)
		/// </summary>
		public WhsPickableDocketLineCollection LinesToPickForBinding
		{
			get { return linesToPickForBinding ?? (linesToPickForBinding = GetParentLinesToPickForBinding()); }
		}

		public WhsPickableDocketLineCollection GetLinesToPick()
		{
			return GetLinesToPickCore();
		}

		protected virtual WhsPickableDocketLineCollection GetParentLinesToPickForBinding()
		{
			return GetLinesToPickCore();
		}

		protected abstract WhsPickableDocketLineCollection GetLinesToPickCore();
		WhsPickableDocketLineCollection linesToPickForBinding;

		#endregion

		#region GetPickability

		public WhsPick.DocketPickabilityEventArgs GetPickabilityWithoutPick()
		{
			return GetPickabilityCore((pickability, errorMessage) => GetPickabilityWithoutPick(pickability, errorMessage));
		}

		void GetPickabilityWithoutPick(WhsPick.DocketPickabilityEventArgs pickability, ZStringBuilder errorMessage)
		{
			if (WD_DocketStatus != DocketStatus.Codes.New && WD_DocketStatus != DocketStatus.Codes.Entered)
			{
				errorMessage.Append(Res.GetString("e9816e5e-e312-4e5d-a52c-5192e97e5791", "This {0} is {2}. Only {3} {1} can be attached to a Pick.",
					Description, Grammar.Instance.Pluralize(Description), WD_DocketStatusDescription, DocketStatus.Descriptions.Entered));
			}

			if (!WD_RequiredDate.IsValid) // Generating Orders from Rcv/Inv -- does not set the date.
			{
				errorMessage.Append(Res.GetString("937f5976-b28e-4d3a-939b-40cac348f16a", "This {0} has no Required By Date.", Description));
			}

			if (Lines.Count == 0)
			{
				errorMessage.Append(Res.GetString("2363f76b-5e5d-46df-bd9e-1cda840bce87", "This {0} has no Lines.", Description));
			}
			else if (WD_TotalUnitsFromLines <= 0)
			{
				errorMessage.Append(Res.GetString("ed4cce1b-2ac4-44e9-b8ab-9ea4dea34ad5", "This {0} has no Units.", Description));
			}
			else
			{
				GetPickabilityCore(pickability, errorMessage); // allow subclasses to add extra validation
			}
		}

		/// <summary>
		/// Determines whether the Order can be picked if attached to newPick, and builds a list of errors if the Order is not pickable.
		/// </summary>
		public WhsPick.DocketPickabilityEventArgs GetPickability(WhsPick newPick)
		{
			return GetPickabilityCore((pickability, errorMessage) =>
			{
				// not 'NEW', 'BIL' or Is Awaiting Replenishment
				if (newPick.WP_PickStatus != PickStatus.Codes.Created && newPick.WP_PickStatus != PickStatus.Codes.Building && !newPick.WP_IsAwaitingReplenishment)
				{
					errorMessage.Append(Res.GetString("9a24f01b-7237-4f07-884c-22f9e06f7970", "Pick {0} is Finalized, Canceled or has had it's Pick Slip printed.", newPick.WP_PickNo));
					if (newPick.WP_PickStatus != PickStatus.Codes.Cancelled)
					{
						pickability.PickAlreadyExists = true;
					}
				}
				else if (!WD_WP.IsEmpty)
				{
					if (WD_WP != newPick.PK)
					{
						errorMessage.Append(Res.GetString("d2f3aa56-cf0f-4972-ab0d-e0d698adeced", "This {0} is attached to another Pick.", Description));
					}
				}
				else
				{
					if (!newPick.WP_WW_Whs.IsEmpty && newPick.WP_WW_Whs != WD_WW_Whs)
					{
						errorMessage.Append(Res.GetString("a0f8ebbd-1c89-4e9a-a543-10cc0d4e67ff", "This {0} is for a different Warehouse.", Description));
					}

					if (GetPickabilityForNewPickCore(pickability, errorMessage, newPick))
					{
						GetPickabilityWithoutPick(pickability, errorMessage);
					}
				}
			});
		}

		WhsPick.DocketPickabilityEventArgs GetPickabilityCore(Action<WhsPick.DocketPickabilityEventArgs, ZStringBuilder> getPickability)
		{
			var pickability = new WhsPick.DocketPickabilityEventArgs(false, "", NotificationTypes.Error);
			var errorMessage = new ZStringBuilder();

			if (WD_PickOption != WhsPickOption.Codes.Auto && !Env.Security.WhsPicking.IsAllowed)
			{
				errorMessage.Append(WhsErrorTypes.NoSecurityRights.Message);
			}
			else
			{
				getPickability(pickability, errorMessage);
			}

			if (!errorMessage.IsEmpty)
			{
				pickability.Message = errorMessage.ToStringWithNewLineBetweenAppends();
			}
			else // no error
			{
				pickability.IsDocketPickable = true;
				pickability.PickAlreadyExists = IsInDatabase;
				pickability.MessageType = NotificationTypes.None;
			}

			return pickability;
		}

		protected virtual void GetPickabilityCore(WhsPick.DocketPickabilityEventArgs pickability, ZStringBuilder errorMessage)
		{
			// allow subclasses to add extra validation
		}

		protected virtual bool GetPickabilityForNewPickCore(WhsPick.DocketPickabilityEventArgs pickability, ZStringBuilder errorMessage, WhsPick newPick)
		{
			// allow subclasses to add extra validation with access to attributes of the pick
			return true;
		}

		#endregion

		#region SetIsAttachingToPick

		public IDisposable SetIsAttachingToPick()
		{
			return new SemaphoreManager(IsAttachingToPickSemaphore);
		}

		Semaphore IsAttachingToPickSemaphore
		{
			get { return isAttachingToPickSemaphore ?? (isAttachingToPickSemaphore = new Semaphore()); }
		}

		Semaphore isAttachingToPickSemaphore;

		#endregion

		#region SetIsDetachingToPick

		public IDisposable SetIsDetachingToPick()
		{
			return new SemaphoreManager(IsDetachingToPickSemaphore);
		}

		Semaphore IsDetachingToPickSemaphore
		{
			get { return isDetachingToPickSemaphore ?? (isDetachingToPickSemaphore = new Semaphore()); }
		}

		Semaphore isDetachingToPickSemaphore;

		#endregion

		#endregion

		#region Finalisation

		public bool FinaliseDocketAlwaysFinalisingPick()
		{
			var pick = Pick;
			var pickAlreadyExists = pick != null;
			if (!pickAlreadyExists)
			{
				pick = Factory.New<WhsPick>();
				pick.Orders.Add(this);
				pick.AutoAllocateItems(NotificationSubscriber);
			}

			using (new SemaphoreManager(AlwaysFinalisePickOnOrderFinaliseSemaphore))
			{
				FinaliseDocket();
			}

			var success = IsFinalised && pick.IsFinalised;

			if (!success && !pickAlreadyExists)
			{
				pick.CancelPick(); // cancelling the pick will remove the order's reference so store it first
				pick.Delete();
			}

			return success;
		}

		protected override bool FinaliseDocketCore()
		{
			return true;
		}

		protected override void OnFinaliseSucceeded()
		{
			base.OnFinaliseSucceeded();

			var pick = Pick;
			if (!pick.IsFinalised && (AlwaysFinalisePickOnOrderFinalise || WarehouseDataRegistry.Instance.AutoFinalizePick.Value) && pick.AllOrdersAreFinalised)
			{
				pick.FinalisePick();
			}
		}

		protected override bool RunPreFinaliseValidationCore()
		{
			Argument.NotNull(Pick, "Pick");
			return base.RunPreFinaliseValidationCore();
		}

		protected override bool AddFetchHintsForInventory
		{
			get { return false; }
		}

		#region FinaliseConfirmationNotification

		protected override bool FinaliseConfirmationNotification(ZString finaliseConfirmationMessage)
		{
			return true;
		}

		protected override string FinaliseConfirmationMessage
		{
			get { return ""; }
		}

		#endregion

		bool AlwaysFinalisePickOnOrderFinalise
		{
			get { return AlwaysFinalisePickOnOrderFinaliseSemaphore.IsSuspended; }
		}

		Semaphore AlwaysFinalisePickOnOrderFinaliseSemaphore
		{
			get { return alwaysFinalisePickOnOrderFinaliseSemaphore ?? (alwaysFinalisePickOnOrderFinaliseSemaphore = new Semaphore()); }
		}

		Semaphore alwaysFinalisePickOnOrderFinaliseSemaphore;

		#endregion

		#region DelayUpdatingReleaseTotals

		public IDisposable DelayUpdatingReleaseTotals()
		{
			return DelayUpdatingReleaseTotalsCore();
		}

		protected virtual IDisposable DelayUpdatingReleaseTotalsCore() => null;

		#endregion

		#region UpdateReleaseTotals

		public void UpdateReleaseTotals(OrgSupplierPart part, ZDecimal quantity)
		{
			if (part != null && quantity != 0m && !IsUpdatingReleaseTotalsSuspended)
			{
				UpdateReleaseTotalsCore(part, quantity);
			}
		}

		protected virtual void UpdateReleaseTotalsCore(OrgSupplierPart part, ZDecimal quantity)
		{
		}

		/// <summary>
		/// This method will *NOT* update release totals on disposal, you probably want to use DelayUpdatingReleaseTotals
		/// </summary>
		/// <returns></returns>
		public IDisposable SuspendUpdatingReleaseTotals_DoNotUse() => new SemaphoreManager(SuspendUpdatingReleaseTotalsDoNotUseSemaphore);

		protected bool IsUpdatingReleaseTotalsSuspended => suspendUpdatingReleaseTotalsDoNotUseSemaphore != null && suspendUpdatingReleaseTotalsDoNotUseSemaphore.IsSuspended;

		Semaphore SuspendUpdatingReleaseTotalsDoNotUseSemaphore
		{
			get { return suspendUpdatingReleaseTotalsDoNotUseSemaphore ?? (suspendUpdatingReleaseTotalsDoNotUseSemaphore = new Semaphore()); }
		}

		Semaphore suspendUpdatingReleaseTotalsDoNotUseSemaphore;

		#endregion

		#region Security

		protected override bool IsPostFinalizeEditAllowedCore
		{
			get { return Env.Security.WhsReleasePostFinaliseEdit.IsAllowed; }
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			// attached to Pick in memory
			if (!IsInDatabase && WD_WP.IsValid ||
				(
					IsInDatabase &&
					(
						// Detached/Attached to Pick
						WD_WPInfo.HasChanges ||
						// Order has changed while on a Pick
						(WD_WP.IsValid && ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(p => p.IsPersistent && !PropertiesToExcludeForCriticalChanges.Contains(p.Name) && p.HasChanges))
					)
				))
			{
				UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(Factory, this);
			}
		}

		[ThreadSafe] // This Hashset property is Immutable in real usage, ImmutableHashSet is not as performant and not worth using.
		readonly static HashSet<string> PropertiesToExcludeForCriticalChanges = new()
		{
			WhsDocketSchema.Constants.WD_AddPalletWeightToOrder,
			WhsDocketSchema.Constants.WD_ArrivalDate,
			WhsDocketSchema.Constants.WD_BookedWithCBADateTimeUtc,
			WhsDocketSchema.Constants.WD_BookingDate,
			WhsDocketSchema.Constants.WD_CanceledTimeUtc, // requires Status Change so not worth checking on its own
			WhsDocketSchema.Constants.WD_CODPayMethod,
			WhsDocketSchema.Constants.WD_ContainerMode,
			WhsDocketSchema.Constants.WD_CriticalChangesVersionID,
			WhsDocketSchema.Constants.WD_CubicSent,
			WhsDocketSchema.Constants.WD_CustomAttrib1,
			WhsDocketSchema.Constants.WD_CustomAttrib2,
			WhsDocketSchema.Constants.WD_CustomAttrib3,
			WhsDocketSchema.Constants.WD_CustomAttrib4,
			WhsDocketSchema.Constants.WD_CustomAttrib5,
			WhsDocketSchema.Constants.WD_CustomDate1,
			WhsDocketSchema.Constants.WD_CustomDate2,
			WhsDocketSchema.Constants.WD_CustomDecimal1,
			WhsDocketSchema.Constants.WD_CustomDecimal2,
			WhsDocketSchema.Constants.WD_CustomDecimal3,
			WhsDocketSchema.Constants.WD_CustomDecimal4,
			WhsDocketSchema.Constants.WD_CustomDecimal5,
			WhsDocketSchema.Constants.WD_CustomerReference,
			WhsDocketSchema.Constants.WD_CustomFlag1,
			WhsDocketSchema.Constants.WD_CustomFlag2,
			WhsDocketSchema.Constants.WD_CustomFlag3,
			WhsDocketSchema.Constants.WD_CustomFlag4,
			WhsDocketSchema.Constants.WD_CustomFlag5,
			WhsDocketSchema.Constants.WD_DocketID,
			WhsDocketSchema.Constants.WD_DocketType, // trigger prevents change
			WhsDocketSchema.Constants.WD_DropMode,
			WhsDocketSchema.Constants.WD_ETA,
			WhsDocketSchema.Constants.WD_ETD,
			WhsDocketSchema.Constants.WD_ExternalReference,
			WhsDocketSchema.Constants.WD_ExternalReferenceSplit,
			WhsDocketSchema.Constants.WD_ExWhsJobGuid,
			WhsDocketSchema.Constants.WD_F3_NKTotalPackType,
			WhsDocketSchema.Constants.WD_GoodsDescription,
			WhsDocketSchema.Constants.WD_GS_NKCanceledBy, // requires Status Change so not worth checking on its own
			WhsDocketSchema.Constants.WD_GS_NKFinalizedBy, // requires Finalised Time changing which is already checked
			WhsDocketSchema.Constants.WD_HoldPalletIDPutaway,
			WhsDocketSchema.Constants.WD_INCO,
			WhsDocketSchema.Constants.WD_IsAuthorisedToLeave,
			WhsDocketSchema.Constants.WD_IsPickFaceReplenishment,
			WhsDocketSchema.Constants.WD_IsPutawayTransfer,
			WhsDocketSchema.Constants.WD_LocalCartInsuranceCost,
			WhsDocketSchema.Constants.WD_OH_Client, // trigger prevents change
			WhsDocketSchema.Constants.WD_OH_Forwarder,
			WhsDocketSchema.Constants.WD_PackagesSent,
			WhsDocketSchema.Constants.WD_PalletsSent,
			WhsDocketSchema.Constants.WD_PickPriority,
			WhsDocketSchema.Constants.WD_PL_NKCarrierServiceLevel,
			WhsDocketSchema.Constants.WD_ReceiveCategory,
			WhsDocketSchema.Constants.WD_RequiredDate,
			WhsDocketSchema.Constants.WD_RS_NKServiceLevel,
			WhsDocketSchema.Constants.WD_RX_NKTotalOrderCurrency,
			WhsDocketSchema.Constants.WD_ShipperCODAmount,
			WhsDocketSchema.Constants.WD_StartedReceivingTimeUtc,
			WhsDocketSchema.Constants.WD_SystemCreateBranch,
			WhsDocketSchema.Constants.WD_SystemCreateDepartment,
			WhsDocketSchema.Constants.WD_SystemCreateTimeUtc,
			WhsDocketSchema.Constants.WD_SystemCreateUser,
			WhsDocketSchema.Constants.WD_SystemLastEditTimeUtc,
			WhsDocketSchema.Constants.WD_SystemLastEditUser,
			WhsDocketSchema.Constants.WD_TaskPlanningStatus,
			WhsDocketSchema.Constants.WD_TotalCubic,
			WhsDocketSchema.Constants.WD_TotalCubicUnit,
			WhsDocketSchema.Constants.WD_TotalOrderValue,
			WhsDocketSchema.Constants.WD_TotalPallets,
			WhsDocketSchema.Constants.WD_TotalUnits,
			WhsDocketSchema.Constants.WD_TotalWeight,
			WhsDocketSchema.Constants.WD_TotalWeightUnit,
			WhsDocketSchema.Constants.WD_TransportMode,
			WhsDocketSchema.Constants.WD_TransportReference,
			WhsDocketSchema.Constants.WD_TZ_TransportZone,
			WhsDocketSchema.Constants.WD_UnitsSent,
			WhsDocketSchema.Constants.WD_WD_ParentDocket,
			WhsDocketSchema.Constants.WD_WD_Split,
			WhsDocketSchema.Constants.WD_WeightSent,
			WhsDocketSchema.Constants.WD_WeightSentUserEntered,
			WhsDocketSchema.Constants.WD_WeightVolSetFromImport,
			WhsDocketSchema.Constants.WD_WP, // we already check this separately
			WhsDocketSchema.Constants.WD_WP_ParentPickForReceive,
			WhsDocketSchema.Constants.WD_WP_ParentPickForTransfer,
			WhsDocketSchema.Constants.WD_WP_PickBeingReplenished,
		};

		#endregion

		#region Saving

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded && ReloadRelatedJobsOnNextSave)
			{
				ReloadRelatedJobs();
				ReloadRelatedJobsOnNextSave = false;
			}
		}

		protected virtual bool ReloadRelatedJobsOnNextSave { get; set; }

		#endregion

		#region CreateUniqueReferenceIfRequired

		protected override void CreateUniqueReferenceIfRequiredCore()
		{
			if (WD_ExternalReference.IsEmpty || CheckExternalReferenceForDuplicates())
			{
				base.CreateUniqueReferenceIfRequiredCore();
			}
		}

		#endregion

		#region DataRefresh

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();

			if (WD_DocketStatusInfo.HasChanges)
			{
				OrderStatusCache = WD_DocketStatus;
			}

			if (WD_FinalisedDateInfo.HasChanges)
			{
				OrderFinalisedDateCache = WD_FinalisedDate;
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			if (!OrderStatusCache.IsEmpty && OrderStatusCache != WD_DocketStatus)
			{
				if (OrderStatusCache == DocketStatus.Codes.Entered)
				{
					WD_WP = ZGuid.Empty;
				}
				WD_DocketStatus = OrderStatusCache;
			}

			OrderStatusCache = "";

			if (!OrderFinalisedDateCache.IsEmpty && ZDateTimeOffset.DifferentMinutes(OrderFinalisedDateCache, WD_FinalisedDate))
			{
				WD_FinalisedDate = OrderFinalisedDateCache;
			}

			OrderFinalisedDateCache = ZDateTimeOffset.Empty;
		}

		ZString OrderStatusCache;
		ZDateTimeOffset OrderFinalisedDateCache;

		#endregion

		#region Validation

		public void ValidateConsigneePK()
		{
			if (!IsValidationSuspended && !ConsigneeDocAddress.IsValidationSuspended)
			{
				ConsigneeDocAddress.Validation.ValidateOrganisationPK();
			}
		}

		public new WhsPickableDocketValidation Validation
		{
			get { return (WhsPickableDocketValidation)base.Validation; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		protected override WhsDocketInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new WhsPickableDocketInvoicingSupporter(this);
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocAddresses

		protected override JobDocAddressRequirement GetDocAddressRequirementCore(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ConsigneeAddress:
					return ConsigneeDocAddressRequirement;
				default:
					return this.GetTransportCoRequirement(addressType) ?? base.GetDocAddressRequirementCore(addressType);
			}
		}

		protected override DocAddressType[] SupportedAddressTypesCore()
		{
			return base.SupportedAddressTypesCore().Concat(new[] { DocAddressType.ConsigneeAddress }).Concat(TransportCoConstants.GetSupportedAddressTypes()).ToArray();
		}

		#region ConsigneeDocAddressRequirement

		public JobDocAddressRequirement ConsigneeDocAddressRequirement
		{
			get { return consigneeDocAddressRequirement ?? (consigneeDocAddressRequirement = GetConsigneeDocAddressRequirement()); }
		}

		protected virtual JobDocAddressRequirement GetConsigneeDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.ConsigneeAddress, ContactType.Consignee);
		}

		JobDocAddressRequirement consigneeDocAddressRequirement;

		#endregion

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = GetNewDocumentSupporter()); }
		}

		DocumentSupporter documentSupporter;

		protected abstract DocumentSupporter GetNewDocumentSupporter();

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = GetNewDocManagerInfo()); }
		}

		protected abstract DocManagerInfo GetNewDocManagerInfo();

		DocManagerInfo docManagerInfo;

		#endregion

		#region IJobWithTransportCompany

		bool IJobWithTransportCompany.GetTransportCoDocAddressReadOnly() => NonStandardReadOnly1;

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return TemplateCopy(copyLines: true);
		}

		IBusiness IWhsJobTemplateCopyable.TemplateCopyWithoutLines()
		{
			return TemplateCopy(copyLines: false);
		}

		IBusiness TemplateCopy(bool copyLines)
		{
			var args = new BusinessObjectCloneArgs(new[] { WhsDocketSchema.Constants.WD_WP }, true);
			var copy = (WhsPickableDocket)this.Clone(args);

			copy.WD_DocketStatus = DocketStatus.Codes.New;
			copy.WD_FinalisedDate = ZDateTimeOffset.Empty;
			copy.WD_TransportReference = "";
			copy.WD_WL_CrossDock = ZGuid.Empty;

			copy.Containers.DeleteAll();
			copy.Pallets.DeleteAll();
			copy.CopyJobDocAddressesFrom(this);

			using (new SemaphoreManager(copy.UpdatingWeightAndVolumeSemaphore))
			{
				if (copyLines)
				{
					TemplateCopyLines(copy);
				}

				TemplateCopyCore(copy);
			}
			return copy;
		}

		protected virtual void TemplateCopyLines(WhsPickableDocket copy)
		{
			foreach (WhsPickableDocketLine line in AllLines.ToArray())
			{
				var copiedLine = (WhsPickableDocketLine)line.Clone();
				copy.Lines.Add(copiedLine);
			}
		}

		protected virtual void TemplateCopyCore(WhsPickableDocket copy)
		{
		}

		#endregion

		#region UpdatingPickOrderedInventories

		public IDisposable SuspendUpdatingPickOrderedInventories() => new SemaphoreManager(UpdatingPickOrderedInventories);

		internal bool IsUpdatingPickOrderedInventoriesSuspended => UpdatingPickOrderedInventories.IsSuspended;

		Semaphore UpdatingPickOrderedInventories => updatingPickOrderedInventories ?? (updatingPickOrderedInventories = new Semaphore());
		Semaphore updatingPickOrderedInventories;

		#endregion

		#region PickableDocketLineFromInventory

		PickableDocketLineFromInventoryHelper<WhsPickableDocketLine> PickableDocketLineFromInventoryHelper
		{
			get { return pickableDocketLineFromInventoryHelper ?? (pickableDocketLineFromInventoryHelper = new PickableDocketLineFromInventoryHelper<WhsPickableDocketLine>(NotificationSubscriber, this)); }
		}
		PickableDocketLineFromInventoryHelper<WhsPickableDocketLine> pickableDocketLineFromInventoryHelper;

		public WhsDocketLine CreateDocketLineFromInventory(WhsInventoryView inventoryList)
		{
			return CreateDocketLineFromInventoryCore(inventoryList);
		}

		protected virtual WhsDocketLine CreateDocketLineFromInventoryCore(WhsInventoryView inventoryList)
		{
			return PickableDocketLineFromInventoryHelper.CreateDocketLineFromInventory(this.Lines, inventoryList);
		}

		public void AcceptInventoryLinesFromSearchGrid(BusinessObject[] inventoryList)
		{
			AcceptInventoryLinesFromSearchGridCore(inventoryList);
		}

		protected virtual void AcceptInventoryLinesFromSearchGridCore(BusinessObject[] inventoryList)
		{
			PickableDocketLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(this.Lines, inventoryList);
		}

		#endregion
	}
}
