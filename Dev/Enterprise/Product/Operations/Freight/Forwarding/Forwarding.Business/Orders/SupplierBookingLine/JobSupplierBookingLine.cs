using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[CodeProperty(nameof(JobSupplierBookingLine.Schema.BookingAndBookingLineId)), DescriptionProperty(JobSupplierBookingLine.Schema.BookingAndBookingLineId)]
	[DependentBusinessObject(typeof(JobSupplierBooking), "SupplierBookingLines")]
	public class JobSupplierBookingLine : AutoJobSupplierBookingLine, IDocAddresses, IExternalRequestGenerationProvider, IWorkflowProvider
	{
		public JobSupplierBookingLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoJobSupplierBookingLine.Schema
		{
			public const string ManufacturerNameOrPK = "ManufacturerNameOrPK";
			public const string BookingAndBookingLineId = "BookingAndBookingLineId";
		}

		public ZString BookingAndBookingLineId
		{
			get { return SupplierBooking?.JSB_BookingId + " - " + JSL_BookingLineId; }
		}

		#region Related Entities

		public OrderLine OrderLine => Factory.Load<OrderLine>(JSL_JO_OrderLine);
		public JobSupplierBooking SupplierBooking => Factory.Load<JobSupplierBooking>(JSL_JSB_Booking);

		#endregion

		#region Properties

		public override ZDecimal JSL_BookedQuantity
		{
			get => base.JSL_BookedQuantity;
			set
			{
				if (base.JSL_BookedQuantity != value)
				{
					var diff = base.JSL_BookedQuantity - value;
					base.JSL_BookedQuantity = value;

					if (ShouldUpdateOpenQuantity())
					{
						OrderLine.JO_OpenQuantity += diff;
					}
				}
			}
		}

		public override ZGuid JSL_JO_OrderLine
		{
			get => base.JSL_JO_OrderLine;
			set
			{
				if (base.JSL_JO_OrderLine != value)
				{
					if (ShouldUpdateOpenQuantity())
					{
						OrderLine.JO_OpenQuantity += JSL_BookedQuantity;
					}

					base.JSL_JO_OrderLine = value;

					if (ShouldUpdateOpenQuantity())
					{
						OrderLine.JO_OpenQuantity -= JSL_BookedQuantity;
					}
				}
			}
		}

		public override ZGuid JSL_JSB_Booking
		{
			get => base.JSL_JSB_Booking;
			set
			{
				if (base.JSL_JSB_Booking != value)
				{
					if (ShouldUpdateOpenQuantity())
					{
						OrderLine.JO_OpenQuantity += JSL_BookedQuantity;
					}

					base.JSL_JSB_Booking = value;

					if (ShouldUpdateOpenQuantity())
					{
						OrderLine.JO_OpenQuantity -= JSL_BookedQuantity;
					}
				}
			}
		}

		public override ZDecimal JSL_ReceivedQuantity
		{
			get => base.JSL_ReceivedQuantity;
			set
			{
				var difference = value - base.JSL_ReceivedQuantity;

				base.JSL_ReceivedQuantity = value;
				JSL_RemainingQuantityToBePacked += difference;
			}
		}

		public override ZInt JSL_ReceivedPackages
		{
			get => base.JSL_ReceivedPackages;
			set
			{
				var difference = value - base.JSL_ReceivedPackages;

				base.JSL_ReceivedPackages = value;
				JSL_RemainingPackagesToBePacked += difference;
			}
		}

		public override ZDecimal JSL_ReceivedWeight
		{
			get => base.JSL_ReceivedWeight;
			set
			{
				var difference = value - base.JSL_ReceivedWeight;

				base.JSL_ReceivedWeight = value;
				JSL_RemainingWeightToBePacked += difference;
			}
		}

		public override ZDecimal JSL_ReceivedVolume
		{
			get => base.JSL_ReceivedVolume;
			set
			{
				var difference = value - base.JSL_ReceivedVolume;

				base.JSL_ReceivedVolume = value;
				JSL_RemainingVolumeToBePacked += difference;
			}
		}

		bool ShouldUpdateOpenQuantity() => JSL_JO_OrderLine.IsValid && JSL_JSB_Booking.IsValid && (SupplierBooking?.ShouldUpdateOpenQuantity() ?? false);

		ContainerLoadListLineCollection containerLoadListLines;
		public ContainerLoadListLineCollection ContainerLoadListLines =>
			containerLoadListLines ?? (containerLoadListLines = new ContainerLoadListLineCollection(Factory, new ZQuery(ContainerLoadListLineSchema.CLL_JSL_BookingLine, PK)));

		#endregion

		#region Calcuated Properties

		public ZDecimal RemainingPackagesToBeReceived => JSL_BookedPackages - JSL_ReceivedPackages;

		public ZDecimal RemainingQuantityToBeReceived => JSL_BookedQuantity - JSL_ReceivedQuantity;

		public ZDecimal RemainingVolumeToBeReceived => JSL_Volume - JSL_ReceivedVolume;

		public ZDecimal RemainingWeightToBeReceived => JSL_GrossWeight - JSL_ReceivedWeight;

		public ZDecimal OpenPackages => (JSL_BookedPackages - JSL_ReceivedPackages) + JSL_RemainingPackagesToBePacked;

		public ZDecimal OpenQuantity => (JSL_BookedQuantity - JSL_ReceivedQuantity) + JSL_RemainingQuantityToBePacked;

		public ZDecimal OpenVolume => (JSL_Volume - JSL_ReceivedVolume) + JSL_RemainingVolumeToBePacked;

		public ZDecimal OpenWeight => (JSL_GrossWeight - JSL_ReceivedWeight) + JSL_RemainingWeightToBePacked;

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				WorkflowItems.RemoveAndDeleteAll();
				base.Delete();
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			base.BeforeSuccessfulDelete();

			if ((SupplierBooking?.ShouldUpdateOpenQuantity() ?? false) && JSL_JO_OrderLine.IsValid)
			{
				OrderLine.JO_OpenQuantity += JSL_BookedQuantity;
			}
		}

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
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

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new[] { DocAddressType.Manufacturer };

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.Manufacturer:
					{
						return ManufacturerDocAddressRequirement;
					}
				default:
					return null;
			}
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			OrgHeaderCollection result = null;
			if (addressType == DocAddressType.Manufacturer)
			{
				result = new OrgHeaderCollection(Factory);
			}
			return result;
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		public void OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		#endregion

		#region Manufacturer

		JobDocAddressRequirement ManufacturerDocAddressRequirement
		{
			get
			{
				if (manufacturerDocAddressRequirement == null)
				{
					manufacturerDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Manufacturer);
				}

				return manufacturerDocAddressRequirement;
			}
		}
		JobDocAddressRequirement manufacturerDocAddressRequirement;

		[List("Lookups.Organisations")]
		public ZString ManufacturerNameOrPK
		{
			get { return ManufacturerAddress.OrganisationNameOrPK; }
			set { ManufacturerAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo ControllingCustomerNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ManufacturerNameOrPK, x => ManufacturerAddress.OrganisationNameOrPKInfo); }
		}

		public JobDocAddress ManufacturerAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(manufacturerAddress))
				{
					manufacturerAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Manufacturer);
					manufacturerAddress.HasChangesChanged += ManufacturerAddressChanged; // Mark as needing validation on changes 
				}

				return manufacturerAddress;
			}
		}
		JobDocAddress manufacturerAddress;

		protected virtual void ManufacturerAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		#endregion

		public ForwardingPackLine LooseCargoPackLine =>
			looseCargoPackLine ??=
				SupplierBooking.JSB_Status == Constants.SupplierBookingStatus.Converted && SupplierBooking.JSB_ContainerMode == SupplierBookingLoadModeList.Codes.LSE
				? Factory.LoadTop1<ForwardingPackLine>(new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, PK))
				: null;

		#region Workflow Provider

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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		[ChildEditable]
		public JobSupplierBookingLineProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new JobSupplierBookingLineProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		JobSupplierBookingLineProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.JobSupplierBookingLineWorkflowDescriptorCode;

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder());
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, SupplierBooking.JSB_TransportMode, ZString.Empty);

			return result;
		}

		internal IZType[] GetClientsInTemplateSelectionOrder()
		{
			var result = new List<IZType>();

			var buyerPK = OrderLine?.Order?.Buyer?.PK;
			if (buyerPK != null)
			{
				result.Add(buyerPK);
			}

			var ccPK = SupplierBooking.ControllingCustomerAddress?.Organisation?.PK;
			if (ccPK != null)
			{
				result.Add(ccPK);
			}

			result.Add(ZGuid.Empty);

			return result.ToArray();
		}

		#endregion

		ForwardingPackLine looseCargoPackLine;

		public override void OnSaving()
		{
			if (!this.IsInDatabase)
			{
				if (JSL_BookingLineId.IsEmpty)
				{
					JSL_BookingLineId = Environment.Env.NumberFountains.JobSupplierBookingLineID.GetNextFormatted(Factory);
				}
			}

			base.OnSaving();
		}

		public void LogEventOnShipmentWindowDatesIfNeeded()
		{
			if (SupplierBooking != null && SupplierBooking.JSB_Status == Constants.SupplierBookingStatus.Placed && OrderLine != null)
			{
				var bookingChangesToPlaced = SupplierBooking.IsInDatabase && SupplierBooking.JSB_StatusInfo.HasChanges && (SupplierBooking.JSB_StatusInfo.OriginalValue.ToString() == Constants.SupplierBookingStatus.Incomplete || SupplierBooking.JSB_StatusInfo.OriginalValue.ToString() == Constants.SupplierBookingStatus.Rejected);

				var shipmentWindowStart = OrderLine.JO_ShipmentWindowStart.IsValid ? OrderLine.JO_ShipmentWindowStart : OrderLine.Order?.JD_ShipmentWindowStart;
				if ((!IsInDatabase || JSL_ShipmentWindowStartInfo.HasChanges || bookingChangesToPlaced)
					&& shipmentWindowStart.HasValue
					&& !shipmentWindowStart.Value.IsEmpty
					&& (JSL_ShipmentWindowStart.IsEmpty || shipmentWindowStart.Value != JSL_ShipmentWindowStart))
				{
					var parameters = new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(Params.Type, (NoResString)"Ship Window Start"), // System event info
						new KeyValuePair<string, string>(Params.Reason, $"This Order Line has a Ship Window Start mismatch with its linked Supplier Booking. Please check and review the linked Supplier Booking ({SupplierBooking.JSB_BookingId})."),
					};
					OrderLine.Logs.CreateOrRecreateEventLog(
						Events.ExceptionRaised,
						EstimateActual.Actual,
						ZDateTimeOffset.Now,
						string.Empty,
						parameters);
					SupplierBooking.AddLogOnShipmentWindowDatesIfNeeded(true, false);
				}

				var shipmentWindowEnd = OrderLine.JO_ShipmentWindowEnd.IsValid ? OrderLine.JO_ShipmentWindowEnd : OrderLine.Order?.JD_ShipmentWindowEnd;
				if ((!IsInDatabase || JSL_ShipmentWindowEndInfo.HasChanges || bookingChangesToPlaced)
					&& shipmentWindowEnd.HasValue
					&& !shipmentWindowEnd.Value.IsEmpty
					&& (JSL_ShipmentWindowEnd.IsEmpty || shipmentWindowEnd.Value != JSL_ShipmentWindowEnd))
				{
					var parameters = new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(Params.Type, (NoResString)"Ship Window End"), // System event info
						new KeyValuePair<string, string>(Params.Reason, $"This Order Line has a Ship Window End mismatch with its linked Supplier Booking. Please check and review the linked Supplier Booking ({SupplierBooking.JSB_BookingId})."),
					};

					OrderLine.Logs.CreateOrRecreateEventLog(
						Events.ExceptionRaised,
						EstimateActual.Actual,
						ZDateTimeOffset.Now,
						string.Empty,
						parameters);
					SupplierBooking.AddLogOnShipmentWindowDatesIfNeeded(false, true);
				}
			}
		}

		#region IExternalRequestGenerationProvider

		public ZString GetRequestJobID() => SupplierBooking?.JSB_BookingId ?? ZString.Empty;

		public ZString GetRequestTypeCode() => ExternalRequestTypes.Codes.SupplierBookingLine;

		public (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(ZString addressType) => addressType.ToString() switch
		{
			DocAddressTypes.Codes.BookingPartyDocumentaryAddress or DocAddressTypes.Codes.SupplierDocumentaryAddress or DocAddressTypes.Codes.ControllingCustomer => SupplierBooking?.GetRequestSupportedAddressInfo(addressType) ?? (ZGuid.Empty, ZGuid.Empty),
			DocAddressTypes.Codes.Manufacturer => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScripForGetRequestManufacturerAddressInfo, PK.ToString()), OrgAddressSchema.Constants.OA_OH, JobDocAddressSchema.Constants.E2_Contact),
			DocAddressTypes.Codes.BuyerDocumentaryAddress => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScriptForGetRequestBuyerDocumentaryAddress, PK.ToString()), OrgAddressSchema.Constants.OA_OH, JobOrderHeaderSchema.Constants.JD_OC_BuyerContact, false),
			_ => (ZGuid.Empty, ZGuid.Empty)
		};

		string ScripForGetRequestManufacturerAddressInfo =>
			"""
				SELECT TOP 1 OA_OH, E2_Contact
				FROM dbo.JobSupplierBookingLine
				INNER JOIN dbo.JobDocAddress ON JSL_PK = E2_ParentID AND E2_AddressOverride = 0 AND E2_AddressSequence = 0 AND E2_OA_Address IS NOT NULL AND E2_AddressType = 'MAN'
				INNER JOIN dbo.OrgAddress ON OA_PK = E2_OA_Address
				WHERE JSL_PK = '{0}'
			""";

		string ScriptForGetRequestBuyerDocumentaryAddress => 
			"""
				SELECT TOP 1 OA_OH, JD_OC_BuyerContact
				FROM dbo.JobSupplierBookingLine
				INNER JOIN dbo.JobOrderLine ON JSL_JO_OrderLine = JO_PK
				INNER JOIN dbo.JobOrderHeader ON JO_JD = JD_PK
				INNER JOIN dbo.OrgAddress ON OA_PK = JD_OA_BuyerAddress
				WHERE JSL_PK = '{0}'
			""";

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();

			JSL_JSB_Booking = supplierBooking.PK;
			JSL_JO_OrderLine = orderLine.PK;
		}
#endif
	}
}
