using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using SupplierBookingStatus = Enterprise.Freight.Forwarding.Orders.Business.SupplierBookingStatusList.Codes;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.JobSupplierBooking)]
	[UserDefinedValues]
	[UniversalDataContext(DataContextType.JobSupplierBooking)]
	[CodeProperty(Schema.JSB_BookingId), DescriptionProperty(Schema.JSB_GoodsDescription)]
	public class JobSupplierBooking : AutoJobSupplierBooking
		, Integration.Forwarding.IJobSupplierBooking
		, IDocAddresses
		, ISupportDataImporting
		, IJobNumber
		, IWorkflowProvider
		, IDocManagerSupport
		, IDocumentSupportable
		, ICustomFieldProvider
		, IExternalRequestGenerationProvider
		, IUniversalXMLNoteParent
	{
		public new class Schema : AutoJobSupplierBooking.Schema
		{
			public const string SupplierNameOrPK = "SupplierNameOrPK";
			public const string ControllingCustomerNameOrPK = "ControllingCustomerNameOrPK";
			public const string LocalCartageCFSNameOrPK = "LocalCartageCFSNameOrPK";
			public const string ConsigneeDocumentaryNameOrPK = "ConsigneeDocumentaryNameOrPK";
			public const string JSB_DetailedGoodsDescription = "JSB_DetailedGoodsDescription";
		}

		public JobSupplierBooking(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static JobSupplierBooking New(BusinessObjectFactory factory)
		{
			return factory.New<JobSupplierBooking>();
		}

		#region Properties

		public override ZString JSB_Status
		{
			get
			{
				return base.JSB_Status;
			}
			set
			{
				if (base.JSB_Status != value)
				{
					var oldStatus = base.JSB_Status;
					base.JSB_Status = value;

					if (this.ShouldUpdateOpenQuantity() != ShouldUpdateOpenQuantity(oldStatus))
					{
						AdjustQuantitiesOnOrderLines(isAdding: this.ShouldUpdateOpenQuantity());
					}
				}
			}
		}

		[MaxLength(10000)]
		public override ZString JSB_MarksAndNumbers
		{
			get => base.JSB_MarksAndNumbers;
			set => base.JSB_MarksAndNumbers = value;
		}

		void AdjustQuantitiesOnOrderLines(bool isAdding)
		{
			if (isAdding)
			{
				foreach (var bookingLine in SupplierBookingLines)
				{
					if (bookingLine.JSL_JO_OrderLine.IsValid)
					{
						bookingLine.OrderLine.JO_OpenQuantity -= bookingLine.JSL_BookedQuantity;
					}
				}
			}
			else
			{
				foreach (var bookingLine in SupplierBookingLines)
				{
					if (bookingLine.JSL_JO_OrderLine.IsValid)
					{
						bookingLine.OrderLine.JO_OpenQuantity += bookingLine.JSL_BookedQuantity;
					}
				}
			}
		}

		#region JSB_DetailedGoodsDescription

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString JSB_DetailedGoodsDescription
		{
			get
			{
				return JSB_DetailedGoodsDescriptionNote?.ST_NoteText ?? ZString.Empty;
			}
			set
			{
				var note = JSB_DetailedGoodsDescriptionNote;
				if (note == null)
				{
					note = Notes.AddNew();
					note.ST_ParentID = PK;
					note.ST_Table = TableName;
					note.ST_IsCustomDescription = false;
					note.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Code;
				}
				CheckMaximumLength(JSB_DetailedGoodsDescriptionInfo, value);
				note.ST_NoteText = value;
				JSB_DetailedGoodsDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JSB_DetailedGoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JSB_DetailedGoodsDescription); }
		}

		protected virtual StmNote JSB_DetailedGoodsDescriptionNote
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

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => GetSupportedAddressTypes().ToArray();

		static IEnumerable<DocAddressType> GetSupportedAddressTypes()
		{
			yield return DocAddressType.ControllingCustomer;
			yield return DocAddressType.SupplierDocumentaryAddress;
			yield return DocAddressType.LocalCartageCFS;
			yield return DocAddressType.NotifyParty;
			yield return DocAddressType.NotifyParty2;
			yield return DocAddressType.NotifyParty3;

			if (AdvOrmFeatureHelper.IsEnabled)
			{
				yield return DocAddressType.ConsigneeDocumentaryAddress;
			}
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => null;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => null;

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

		#region CFS Address

		[List("Lookups.Organisations")]
		public ZString LocalCartageCFSNameOrPK
		{
			get { return LocalCartageCFSAddress.OrganisationNameOrPK; }
			set { LocalCartageCFSAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo LocalCartageCFSNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.LocalCartageCFSNameOrPK, x => LocalCartageCFSAddress.OrganisationNameOrPKInfo); }
		}

		public JobDocAddress LocalCartageCFSAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(localCartageCFSAddress))
				{
					localCartageCFSAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.LocalCartageCFS);
					localCartageCFSAddress.HasChangesChanged += LocalCartageCFSAddressChanged; // Mark as needing validation on changes 
				}

				return localCartageCFSAddress;
			}
		}
		JobDocAddress localCartageCFSAddress;

		protected virtual void LocalCartageCFSAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		[MaxLength(3)]
		public ZString LocalCartageCFSFieldType
		{
			get
			{
				return LocalCartageCFSAddress.E2_AddressOverride ?
					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo LocalCartageCFSFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(LocalCartageCFSFieldType)); }
		}

		#endregion

		#region Supplier

		[List("Lookups.Organisations")]
		public ZString SupplierNameOrPK
		{
			get { return SupplierAddress.OrganisationNameOrPK; }
			set { SupplierAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo SupplierNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SupplierNameOrPK, x => SupplierAddress.OrganisationNameOrPKInfo); }
		}

		public JobDocAddress SupplierAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(supplierAddress))
				{
					supplierAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupplierDocumentaryAddress);
					supplierAddress.HasChangesChanged += SupplierAddressChanged; // Mark as needing validation on changes 
				}

				return supplierAddress;
			}
		}
		JobDocAddress supplierAddress;

		protected virtual void SupplierAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		[MaxLength(3)]
		public ZString SupplierFieldType
		{
			get
			{
				return SupplierAddress.E2_AddressOverride ?
					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo SupplierFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(SupplierFieldType)); }
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

		#region ConsigneeDocumentaryAddress

		[List("Lookups.Organisations")]
		public ZString ConsigneeDocumentaryNameOrPK
		{
			get { return ConsigneeDocumentaryAddress.OrganisationNameOrPK; }
			set { ConsigneeDocumentaryAddress.OrganisationNameOrPK = value; }
		}

		public ZPropertyInfo ConsigneeDocumentaryNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeDocumentaryNameOrPK, x => ConsigneeDocumentaryAddress.OrganisationNameOrPKInfo); }
		}

		public JobDocAddress ConsigneeDocumentaryAddress
		{
			get
			{
				if (BusinessObject.IsNullOrDeleted(consigneeDocumentaryAddress))
				{
					consigneeDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
					consigneeDocumentaryAddress.HasChangesChanged += ConsigneeDocumentaryAddressChanged; // Mark as needing validation on changes 
				}

				return consigneeDocumentaryAddress;
			}
		}
		JobDocAddress consigneeDocumentaryAddress;

		protected virtual void ConsigneeDocumentaryAddressChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		#endregion

		#region Notify Party

		public JobDocAddress NotifyPartyDocAddress
		{
			get { return DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty); }
		}

		#endregion

		#region Notify Party 2

		public JobDocAddress NotifyParty2DocAddress
		{
			get { return DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty2); }
		}

		#endregion

		#region Notify Party 3

		public JobDocAddress NotifyParty3DocAddress
		{
			get { return DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty3); }
		}

		#endregion

		#region Workflow

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, GetClientsInTemplateSelectionOrder());
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, JSB_TransportMode, ZString.Empty);

			return result;
		}

		IZType[] GetClientsInTemplateSelectionOrder()
		{
			var result = new List<IZType>();

			var buyerPKs = SupplierBookingLines
				.Select(x => x?.OrderLine?.Order?.Buyer?.PK)
				.Where(x => x.HasValue)
				.Distinct();

			if (buyerPKs.Count() > 1)
			{
				ErrorReporter.ReportOnce("This functionality assumes there will only be one buyer for a given supplier booking. If this is not the case, please enhance the selection criteria logic.");
			}

			var buyerPK = buyerPKs.FirstOrDefault();

			if (buyerPK.HasValue)
			{
				result.Add(buyerPK.Value);
			}

			if (ControllingCustomerAddress?.Organisation != null)
			{
				result.Add(ControllingCustomerAddress.Organisation.PK);
			}

			result.Add(ZGuid.Empty);

			return result.ToArray();
		}

		[MaxLength(3)]
		public ZString ControllingCustomerFieldType
		{
			get
			{
				return ControllingCustomerAddress.E2_AddressOverride ?
					nameof(FieldType.Text) :
					nameof(FieldType.OrganisationGuid);
			}
		}

		public ZPropertyInfo ControllingCustomerFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ControllingCustomerFieldType)); }
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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.JobSupplierBookingWorkflowDescriptorCode;

		[ChildEditable(true)]
		public SupplierBookingProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new SupplierBookingProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		SupplierBookingProcessTaskCollection workflowItems;

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("a5870b77-f4d7-47df-b595-7c2bdc912768", "Supplier Booking {0}", this.JSB_BookingId);

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = NewDocManagerInfo();
				}
				return docManagerInfo;
			}
		}

		public DocumentSupporter DocumentSupporter => new JobSupplierBookingDocumentSupporter(this);

		#region IJobNumber

		public string JobNumber => JSB_BookingId;

		#endregion

		protected virtual DocManagerInfo NewDocManagerInfo()
		{
			return new DocManagerInfo(this, Core.Constants.DocManagerCodes.JobSupplierBooking);
		}

		DocManagerInfo docManagerInfo;

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			JSB_TransportMode = "AIR";
			JSB_LoadMode = "CY";
			JSB_Status = "PLN";
		}

#endif

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				this.OrderShipmentPlannings.DeleteAll();
				WorkflowItems.RemoveAndDeleteAll();
				base.Delete();
			}
		}

		#endregion

		#region OnFactorySavingBeforeTransaction

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region ICustomFieldProvider

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region ISupportDataImporting Members

		bool ISupportDataImporting.IsImportingData
		{
			get { return isImportingData; }
			set { isImportingData = value; }
		}

		bool isImportingData;

		#endregion

		#region Related Business Objects

		#region JobSupplierBookingLines

		public JobSupplierBookingLineCollection SupplierBookingLines
		{
			get
			{
				if (supplierBookingLines == null)
				{
					supplierBookingLines = new JobSupplierBookingLineCollection(this);
				}
				return supplierBookingLines;
			}
		}

		JobSupplierBookingLineCollection supplierBookingLines;

		#endregion

		#endregion

		#region custom calculate property

		public ZDecimal TotalVolume
		{
			get => SupplierBookingLines.Sum(bookingLine => bookingLine.JSL_VolumeUnit == TotalVolumeUnit
					? bookingLine.JSL_Volume
					: (ZDecimal)Constants.Volume.Convert(bookingLine.JSL_Volume, bookingLine.JSL_VolumeUnit, TotalVolumeUnit, false));
		}

		public ZString TotalVolumeUnit
		{
			get
			{
				var result = ZString.Empty;
				foreach (var bookingLine in SupplierBookingLines)
				{
					if (result.IsEmpty)
					{
						result = bookingLine.JSL_VolumeUnit;
					}
					else if (bookingLine.JSL_VolumeUnit != result)
					{
						result = Constants.Volume.CubicMetres;
						break;
					}
				}

				return result.IsEmpty ? (ZString)Constants.Volume.CubicMetres : result;
			}
		}

		public ZDecimal TotalWeight
		{
			get => SupplierBookingLines.Sum(bookingLine => bookingLine.JSL_GrossWeightUnit == TotalWeightUnit
					? bookingLine.JSL_GrossWeight
					: (ZDecimal)Constants.Weight.Convert(bookingLine.JSL_GrossWeight, bookingLine.JSL_GrossWeightUnit, TotalWeightUnit, false));
		}

		public ZString TotalWeightUnit
		{
			get
			{
				var result = ZString.Empty;

				foreach (var bookingLine in SupplierBookingLines)
				{
					if (result.IsEmpty)
					{
						result = bookingLine.JSL_GrossWeightUnit;
					}
					else if (bookingLine.JSL_GrossWeightUnit != result)
					{
						result = Constants.Weight.Kilograms;
					}
				}

				return result.IsEmpty ? (ZString)Constants.Weight.Kilograms : result;
			}
		}

		#endregion

		public override void OnSaving()
		{
			PopulateBookingIdIfNeeded();

			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded && !IsInDatabase)
			{
				JSB_BookingId = ZString.Empty;
			}
		}

		public void PopulateBookingIdIfNeeded()
		{
			if (!IsInDatabase && !IsDeleted && JSB_BookingId.IsEmpty)
			{
				JSB_BookingId = GenerateBookingId();
			}
		}

		public void AddLogOnShipmentWindowDatesIfNeeded(bool shipmentWindowStartError, bool shipmentWindowEndError)
		{
			if (shipmentWindowStartError)
			{
				AddExceptionRaisedEventOnBookingForMismatchedShipWindow((NoResString)"Ship Window Start", (NoResString)"This Supplier Booking has booking lines whose Ship Window Start dates do not match with their order line dates.");
			}
			if (shipmentWindowEndError)
			{
				AddExceptionRaisedEventOnBookingForMismatchedShipWindow((NoResString)"Ship Window End", (NoResString)"This Supplier Booking has booking lines whose Ship Window End dates do not match with their order line dates.");
			}
		}

		internal void AddExceptionRaisedEventOnBookingForMismatchedShipWindow(string type, string reason)
		{
			var parameters = new KeyValuePair<string, string>[]
			{
				new (Params.Type, type), // System event info
				new (Params.Reason, reason),
			};

			Logs.CreateOrRecreateEventLog(
				Events.ExceptionRaised,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				string.Empty,
				parameters);
		}

		ZString GenerateBookingId()
		{
			BookingIdNumberGenerator.Generate();
			BookingIdNumberGenerator.EnforceMaxLengths();

			return BookingIdNumberGenerator.PrimaryTarget.Value;
		}

		#region NumberGenerator

		NumberGenerator BookingIdNumberGenerator
		{
			get { return numberGenerator ?? (numberGenerator = GetNewNumberGenerator()); }
		}

		NumberGenerator numberGenerator;

		NumberGenerator GetNewNumberGenerator()
		{
			var generator = new NumberGenerator
			{
				Factory = Factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.JobSupplierBookingNumber,
				FountainGetter = Env.NumberFountains.GetJobSupplierBookingNumberGeneratorFountain,
				PrimaryTarget = new SupplierBookingNumberGeneratorTarget()
			};

			generator.ValueProviders.AddRange(new StandardValueSource());
			return generator;
		}

		#endregion

		#region PlannedContainers

		[ChildEditable(true)]
		public JobSupplierBookingPlannedContainerCollection PlannedContainers
		{
			get
			{
				if (plannedContainers == null)
				{
					plannedContainers = new JobSupplierBookingPlannedContainerCollection(this, Factory);
					plannedContainers.Load();
					RegisterEditableChildObject(plannedContainers);
				}
				return plannedContainers;
			}
		}
		JobSupplierBookingPlannedContainerCollection plannedContainers;

		#endregion

		#region Containers

		[ChildEditable(true)]
		public JobSupplierBookingContainerCollection Containers
		{
			get
			{
				if (containers == null)
				{
					containers = new JobSupplierBookingContainerCollection(this);
					containers.Load();
					RegisterEditableChildObject(containers);
				}

				return containers;
			}
		}

		JobSupplierBookingContainerCollection containers;

		#endregion

		#region OpenQuantity

		public bool ShouldUpdateOpenQuantity()
		{
			return ShouldUpdateOpenQuantity(JSB_Status);
		}

		static bool ShouldUpdateOpenQuantity(string status)
		{
			var includedStatuses = new[]
			{
				SupplierBookingStatus.APP,
				SupplierBookingStatus.PLC,
				SupplierBookingStatus.PLN,
				SupplierBookingStatus.REJ,
				SupplierBookingStatus.CNV,
			};

			return includedStatuses.Contains(status);
		}

		public void AddMismatchedPackingCompletedEvent(IEnumerable<string> shipmentIDs)
		{
#pragma warning disable CW1161 // Log event
			var overhead = "|RFN=|TYP=Mismatched".Length;
			var shipmentIDsString = string.Join(",", shipmentIDs.OrderBy(x => x));

			if (shipmentIDsString.Length > StmALogSchema.SL_Reference.MaxLength - overhead)
			{
				shipmentIDsString = shipmentIDsString.Substring(0, StmALogSchema.SL_Reference.MaxLength - overhead - 3) + "...";
			}
			Logs.AddNew(Events.PackingCompleted,
				new KeyValuePair<string, string>(Params.Type, "Mismatched"),
				new KeyValuePair<string, string>(Params.ReferenceNumber, shipmentIDsString));
#pragma warning restore CW1161 // Log event
		}

		#endregion

		public bool IsIncompleteOrCancelled => JSB_Status == Constants.SupplierBookingStatus.Incomplete || JSB_Status == Constants.SupplierBookingStatus.Cancelled;

		public OrderShipmentPlanningCollection OrderShipmentPlannings => orderShipmentPlannings ?? (orderShipmentPlannings = new OrderShipmentPlanningCollection(this));
		OrderShipmentPlanningCollection orderShipmentPlannings;

		public bool ReadOnlyWhileImporting => JSB_Status == Core.Constants.SupplierBookingStatus.Cancelled
			|| JSB_Status == Core.Constants.SupplierBookingStatus.Received
			|| JSB_Status == Core.Constants.SupplierBookingStatus.Shipped
			|| JSB_Status == Core.Constants.SupplierBookingStatus.Converted;

		#region IExternalRequestGenerationProvider

		public ZString GetRequestJobID() => JSB_BookingId;

		public ZString GetRequestTypeCode() => ExternalRequestTypes.Codes.SupplierBooking;

		public (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(ZString addressType) => addressType.ToString() switch
		{
			DocAddressTypes.Codes.BookingPartyDocumentaryAddress => (JSB_OH_BookingParty, ZGuid.Empty),
			DocAddressTypes.Codes.SupplierDocumentaryAddress => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(SupplierAddress),
			DocAddressTypes.Codes.ControllingCustomer => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(ControllingCustomerAddress),
			DocAddressTypes.Codes.Manufacturer => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScripForGetRequestManufacturerAddressInfo, PK.ToString()), OrgAddressSchema.Constants.OA_OH, JobDocAddressSchema.Constants.E2_Contact),
			DocAddressTypes.Codes.BuyerDocumentaryAddress => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScriptForGetRequestBuyerDocumentaryAddress, PK.ToString()), OrgAddressSchema.Constants.OA_OH, JobOrderHeaderSchema.Constants.JD_OC_BuyerContact, false),
			_ => (ZGuid.Empty, ZGuid.Empty)
		};

		string ScripForGetRequestManufacturerAddressInfo =>
			"""
				SELECT DISTINCT OA_OH, E2_Contact
				FROM dbo.JobSupplierBookingLine
				INNER JOIN dbo.JobDocAddress ON JSL_PK = E2_ParentID AND E2_AddressOverride = 0 AND E2_AddressSequence=0 AND E2_OA_Address IS NOT NULL AND E2_AddressType = 'MAN'
				INNER JOIN dbo.OrgAddress ON OA_PK = E2_OA_Address
				WHERE JSL_JSB_Booking = '{0}'
			""";

		string ScriptForGetRequestBuyerDocumentaryAddress => 
			"""
				SELECT DISTINCT OA_OH, JD_OC_BuyerContact
				FROM dbo.JobSupplierBookingLine
				INNER JOIN dbo.JobOrderLine ON JSL_JO_OrderLine = JO_PK
				INNER JOIN dbo.JobOrderHeader ON JO_JD = JD_PK
				INNER JOIN dbo.OrgAddress ON OA_PK = JD_OA_BuyerAddress
				WHERE JSL_JSB_Booking = '{0}'
			""";

		#endregion
	}
}
