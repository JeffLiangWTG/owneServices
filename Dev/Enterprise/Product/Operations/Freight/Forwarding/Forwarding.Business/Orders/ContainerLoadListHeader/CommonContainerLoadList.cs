using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[UserDefinedValues]
	[CodeProperty(Schema.CLH_LoadListId), DescriptionProperty(Schema.CLH_LoadListId)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.CommonContainerLoadList)]
	public class CommonContainerLoadList : AutoContainerLoadListHeader
		, ICommonContainerLoadList
		, IJobNumber
		, IDocAddresses
		, IUniversalXMLNoteParent
	{
		public CommonContainerLoadList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Type Decider
		[ThreadSafe]
		public static readonly ContainerLoadListTypeDecider TypeDecider = new ContainerLoadListTypeDecider();

		#endregion

		public new class Schema : AutoContainerLoadListHeader.Schema
		{
			public const string ControllingCustomerNameOrPK = "ControllingCustomerNameOrPK";
			public const string CLH_DetailedGoodsDescription = "CLH_DetailedGoodsDescription";
		}

		public JobSupplierBooking Booking
		{
			get
			{
				if (booking == null)
				{
					booking = Factory.Load<JobSupplierBooking>(CLH_JSB_Booking);
				}

				return booking;
			}
		}

		JobSupplierBooking booking;

		public ContainerLoadListLineCollection LoadListLines
		{
			get
			{
				if (loadListLines == null)
				{
					loadListLines = new ContainerLoadListLineCollection(this);
				}

				return loadListLines;
			}
		}

		ContainerLoadListLineCollection loadListLines;

		[RelatedBusinessObject("Booking")]
		public override ZGuid CLH_JSB_Booking
		{
			get => base.CLH_JSB_Booking;
			set => base.CLH_JSB_Booking = value;
		}

		public override ZString CLH_Status
		{
			get => base.CLH_Status;
			set
			{
				if (base.CLH_Status is ZString originalStatus && originalStatus != value)
				{
					base.CLH_Status = value;

					UpdateQtyPackedIfNecessary(originalStatus, value);

					var isNewStatusIncludedInCalculation = ShouldUpdateToBePackedColumns();

					if (isNewStatusIncludedInCalculation != ShouldUpdateToBePackedColumns(originalStatus))
					{
						foreach (var containerLoadListLine in LoadListLines)
						{
							if (containerLoadListLine.SupplierBookingLine is JobSupplierBookingLine supplierBookingLine)
							{
								if (isNewStatusIncludedInCalculation)
								{
									supplierBookingLine.JSL_RemainingQuantityToBePacked -= containerLoadListLine.ActiveQuantity();
									supplierBookingLine.JSL_RemainingPackagesToBePacked -= containerLoadListLine.ActivePackages();
									supplierBookingLine.JSL_RemainingWeightToBePacked -= containerLoadListLine.ActiveWeight();
									supplierBookingLine.JSL_RemainingVolumeToBePacked -= containerLoadListLine.ActiveVolume();
								}
								else
								{
									supplierBookingLine.JSL_RemainingQuantityToBePacked += containerLoadListLine.ActiveQuantity(originalStatus);
									supplierBookingLine.JSL_RemainingPackagesToBePacked += containerLoadListLine.ActivePackages(originalStatus);
									supplierBookingLine.JSL_RemainingWeightToBePacked += containerLoadListLine.ActiveWeight(originalStatus);
									supplierBookingLine.JSL_RemainingVolumeToBePacked += containerLoadListLine.ActiveVolume(originalStatus);
								}
							}
						}
					}
					else if (CLH_LoadMode == Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation && IsConfirmed(originalStatus) && CheckUsePlanned(CLH_LoadMode, originalStatus) != CheckUsePlanned(CLH_LoadMode, value))
					{
						foreach (var line in LoadListLines)
						{
							line.SupplierBookingLine.JSL_RemainingQuantityToBePacked -= line.ActiveQuantity() - line.ActiveQuantity(originalStatus);
							line.SupplierBookingLine.JSL_RemainingPackagesToBePacked -= line.ActivePackages() - line.ActivePackages(originalStatus);
							line.SupplierBookingLine.JSL_RemainingWeightToBePacked -= line.ActiveWeight() - line.ActiveWeight(originalStatus);
							line.SupplierBookingLine.JSL_RemainingVolumeToBePacked -= line.ActiveVolume() - line.ActiveVolume(originalStatus);
						}
					}
				}
			}
		}

		#region CLH_DetailedGoodsDescription

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString CLH_DetailedGoodsDescription
		{
			get
			{
				return CLH_DetailedGoodsDescriptionNote?.ST_NoteText ?? ZString.Empty;
			}
			set
			{
				var note = CLH_DetailedGoodsDescriptionNote;
				if (note == null)
				{
					note = Notes.AddNew();
					note.ST_ParentID = PK;
					note.ST_Table = TableName;
					note.ST_IsCustomDescription = false;
					note.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Code;
				}
				CheckMaximumLength(CLH_DetailedGoodsDescriptionInfo, value);
				note.ST_NoteText = value;
				CLH_DetailedGoodsDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CLH_DetailedGoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CLH_DetailedGoodsDescription); }
		}

		protected virtual StmNote CLH_DetailedGoodsDescriptionNote
		{
			get
			{
				var filter = new ZQuery(StmNoteSchema.ST_ParentID, PK);
				filter.FetchOnlyFromLocalCache = !IsInDatabase;
				filter.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Code);
				filter.AddToFilter(StmNoteSchema.ST_Table, TableName);
				return Factory.LoadTop1<StmNote>(filter);
			}
		}

		#endregion

		#region CLH_MarksAndNumbers

		[MaxLength(10000)]
		public override ZString CLH_MarksAndNumbers
		{
			get => base.CLH_MarksAndNumbers;
			set => base.CLH_MarksAndNumbers = value;
		}

		#endregion

		public bool ShouldUpdateToBePackedColumns(string statusToCheck = null) => !NotApplicableStatusesForToBePackedCalculation.Contains(statusToCheck ?? CLH_Status.ToString());

		ImmutableArray<string> NotApplicableStatusesForToBePackedCalculation => ImmutableArray.Create(
			Constants.ContainerLoadListHeaderStatus.Incomplete,
			Constants.ContainerLoadListHeaderStatus.Rejected,
			Constants.ContainerLoadListHeaderStatus.Cancelled
		);

		public static bool CheckUsePlanned(string loadMode, string status) => loadMode == Constants.ContainerLoadListHeaderLoadMode.ContainerFreightStation && (status != Constants.ContainerLoadListHeaderStatus.Converted && status != Constants.ContainerLoadListHeaderStatus.Shipped);

		protected override ZString HumanReadableNameCore => Res.GetString("1F86A4C6-8268-4BB7-858C-8A1E12D419D7", "Container Load List {0}", this.CLH_LoadListId);

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region IJobNumber

		public string JobNumber => CLH_LoadListId;

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				if (LoadListLines.Count > 0)
				{
					LoadListLines.DeleteAll();
				}
				base.Delete();
			}
		}

		#endregion

		#region QtyPacked

		void UpdateQtyPackedIfNecessary(ZString oldStatus, ZString newStatus)
		{
			if (IsConvertedOrShipped(newStatus) && !IsConvertedOrShipped(oldStatus))
			{
				foreach (var line in LoadListLines)
				{
					line.SupplierBookingLine.OrderLine.JO_QtyPacked += line.PackedQuantity();
				}
			}
			if (IsConvertedOrShipped(oldStatus) && !IsConvertedOrShipped(newStatus))
			{
				foreach (var line in LoadListLines)
				{
					line.SupplierBookingLine.OrderLine.JO_QtyPacked -= line.PackedQuantity();
				}
			}
		}

		public bool IsConfirmed()
		{
			return IsConfirmed(CLH_Status);
		}

		static bool IsConfirmed(ZString status)
		{
			return status != Constants.ContainerLoadListHeaderStatus.Incomplete
				&& status != Constants.ContainerLoadListHeaderStatus.Rejected
				&& status != Constants.ContainerLoadListHeaderStatus.Cancelled;
		}

		public static bool IsConvertedOrShipped(ZString status)
		{
			return status == Constants.ContainerLoadListHeaderStatus.Converted
				|| status == Constants.ContainerLoadListHeaderStatus.Shipped;
		}

		#endregion

		#region ControllingCustomer

		[List("Lookups.Organisations")]
		public ZString ControllingCustomerNameOrPK
		{
			get { return ControllingCustomerAddress.OrganisationNameOrPK; }
			set { ControllingCustomerAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo ControllingCustomerNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ControllingCustomerNameOrPK, x => ControllingCustomerAddress.OrganisationNameOrPKInfo); }
		}

		public JobDocAddress ControllingCustomerAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(controllingCustomerAddress))
				{
					controllingCustomerAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ControllingCustomer);
					controllingCustomerAddress.HasChangesChanged += ControllingCustomerAddressChanged; // Mark as needing validation on changes 
				}

				return controllingCustomerAddress;
			}
		}
		JobDocAddress controllingCustomerAddress;

		protected virtual void ControllingCustomerAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		#endregion

		#region IDocAddresses

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new DocAddressType[] { DocAddressType.ControllingCustomer };

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => null;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => null;

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CLH_LoadMode = Booking == null ? CommonContainerLoadListLoadModeList.Codes.CFS : CommonContainerLoadListLoadModeList.Codes.CY;
			CLH_PlannedTransportMode = "SEA";
		}

#endif
	}
}
