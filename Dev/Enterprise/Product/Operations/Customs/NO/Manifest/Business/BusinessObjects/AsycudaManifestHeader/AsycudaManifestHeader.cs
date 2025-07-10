using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.NO.Manifest.Business;

public class AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : ASYCUDA.Business.AsycudaManifestHeader(factory, row),
	Integration.Customs.ASYCUDA.NOManifest.IAsycudaManifestHeader,
	IDocumentUploadSupporter,
	IMovementReferenceNumberSupporter,
	IMovementRequestIdProvider,
	ITransportModeProvider
{
	static class GenAddOnColumnConstants
	{
		public const string DriverCommunicationIdColumnName = "DMODriverCommunicationId";
		public const int DriverCommunicationIdMaxLength = 70;
		public const string DriverNameColumnName = "DMODriverName";
		public const int DriverNameMaxLength = 70;
		public const string ScheduledDateOfAddCustOffColumnName = "DMOScheduledDateOfAddCustOff";
	}

	public new class Schema : ManifestBase.AutoAsycudaManifestHeader.Schema
	{
		public const string MovementReferenceNumber = nameof(AsycudaManifestHeader.MovementReferenceNumber);
	}

	public new AsycudaManifestHeaderLookups Lookups => base.Lookups as AsycudaManifestHeaderLookups;

	protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

	public new AsycudaManifestHeaderValidation Validation => base.Validation as AsycudaManifestHeaderValidation;

	protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

	public new IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;

	protected override Type GetBillTypeCore() => typeof(AsycudaBill);

	protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection()
		=> new AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

	protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);

	protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Norway;

	protected override ASYCUDA.Business.ZZDatabaseValidationHelper GetNewZZValidationHelper() => new ZZDatabaseValidationHelper(this);

	public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

	[ResourceStringData("NO.AsycudaManifestHeader.AMA_RN_NKConveyanceNationality", Caption = "Nationality", FullDescription = "Nationality/Country code for Means of transport at Border Crossing.")]
	public override ZString AMA_RN_NKConveyanceNationality
	{
		get => base.AMA_RN_NKConveyanceNationality;
		set => base.AMA_RN_NKConveyanceNationality = value;
	}

	public override ZString AMA_ManifestType
	{
		get => base.AMA_ManifestType;
		set
		{
			var oldValue = AMA_ManifestType;
			base.AMA_ManifestType = value;

			if (!IsCopying && oldValue != AMA_ManifestType)
			{
				SetDefaultValueForRepresentative();
			}
		}
	}

	public override ZString AMA_ApplicationCode
	{
		get => base.AMA_ApplicationCode;
		set
		{
			var oldValue = AMA_ApplicationCode;
			base.AMA_ApplicationCode = value;

			if (!IsCopying && oldValue != AMA_ApplicationCode)
			{
				SetDefaultValueForRepresentative();
			}
		}
	}

	void SetDefaultValueForRepresentative()
	{
		if (AMA_ManifestType == NOManifestTypes.Codes.DMO && IsConsolidator)
		{
			MasterBill?.SetDefaultValueForRepresentative();
		}
	}

	[ResourceStringData("NO.AsycudaManifestHeader.AMA_DateAtCustomsOffice", Caption = "ETA Cust. Office", FullDescription = "Estimated Time of Arrival at Customs Office/Border Crossing. Update the time as needed when changes occur.")]
	public override ZDateTime AMA_DateAtCustomsOffice
	{
		get => base.AMA_DateAtCustomsOffice;
		set => base.AMA_DateAtCustomsOffice = value;
	}

	[ResourceStringData("NO.AsycudaManifestHeader.AMA_DriverCommunicationId", Caption = "Phone NO./E-Mail", FullDescription = "Telephone number (or mail address) of operator/driver of Means of transport at Border Crossing.")]
	[MaxLength(GenAddOnColumnConstants.DriverCommunicationIdMaxLength)]
	public ZString AMA_DriverCommunicationId
	{
		get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnConstants.DriverCommunicationIdColumnName);
		set
		{
			var oldValue = AMA_DriverCommunicationId;
			if (oldValue != value)
			{
				CheckMaximumLength(AMA_DriverCommunicationIdInfo, value);
				this.SetSystemDefinedValue(GenAddOnColumnConstants.DriverCommunicationIdColumnName, value);
				AMA_DriverCommunicationIdInfo.RefreshBinding(oldValue);
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateAMA_DriverCommunicationId();
			}
		}
	}

	public ZPropertyInfo AMA_DriverCommunicationIdInfo => GetZPropertyInfo(nameof(AMA_DriverCommunicationId));

	[ResourceStringData("NO.AsycudaManifestHeader.AMA_DriverName", Caption = "Operator/Driver", FullDescription = "Name of operator/driver of Means of transport at Border Crossing.")]
	[MaxLength(GenAddOnColumnConstants.DriverNameMaxLength)]
	public ZString AMA_DriverName
	{
		get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnConstants.DriverNameColumnName);
		set
		{
			var oldValue = AMA_DriverName;
			if (oldValue != value)
			{
				CheckMaximumLength(AMA_DriverCommunicationIdInfo, value);
				this.SetSystemDefinedValue(GenAddOnColumnConstants.DriverNameColumnName, value);
				AMA_DriverNameInfo.RefreshBinding(oldValue);
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateAMA_DriverName();
			}
		}
	}

	public ZPropertyInfo AMA_DriverNameInfo => GetZPropertyInfo(nameof(AMA_DriverName));

	[ResourceStringData("NO.AsycudaManifestHeader.AMA_ScheduledDateOfAddCustOff", Caption = "Sched. Arr Cust.Off", FullDescription = "Scheduled Time of Arrival at Customs Office/Border Crossing (set at first submission).")]
	[ReadOnly(true)]
	public ZDateTime AMA_ScheduledDateOfAddCustOff
	{
		get => this.GetSystemDefinedValue<ZDateTime>(GenAddOnColumnConstants.ScheduledDateOfAddCustOffColumnName);
		set
		{
			var oldValue = AMA_ScheduledDateOfAddCustOff;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.ScheduledDateOfAddCustOffColumnName, value);
				AMA_ScheduledDateOfAddCustOffInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo AMA_ScheduledDateOfAddCustOffInfo => GetZPropertyInfo(nameof(AMA_ScheduledDateOfAddCustOff));

	[ResourceStringData("NO.AsycudaManifestHeader.AMA_TransportMeans", Caption = "Type of Means", FullDescription = "Type of Means of transport at Border Crossing.")]
	[List($"{nameof(Lookups)}.{nameof(AsycudaManifestHeaderLookups.TransportMeansCodeList)}")]
	public override ZString AMA_TransportMeans
	{
		get => base.AMA_TransportMeans;
		set => base.AMA_TransportMeans = value;
	}

	[ResourceStringData("NO.AsycudaManifestHeader.AMA_VehicleRegistration|Road", Caption = "Transport ID", FullDescription = "The license plate for the truck/active means of transport at border crossing.", IsApplicableMember = nameof(IsRoad))]
	[ResourceStringData("NO.AsycudaManifestHeader.AMA_VehicleRegistration|Air", Caption = "Aircraft Reg. No.", FullDescription = "The aircraft registration number - as in tail number.", IsApplicableMember = nameof(IsAir))]
	[ResourceStringData("NO.AsycudaManifestHeader.AMA_VehicleRegistration|Sea", Caption = "IMO Ship No.", FullDescription = "The unique IMO registration number of the vessel.", IsApplicableMember = nameof(IsSea))]
	[ResourceStringData("NO.AsycudaManifestHeader.AMA_VehicleRegistration|Rail", Caption = "Train No.", FullDescription = "The train number.", IsApplicableMember = nameof(IsRail))]
	public override ZString AMA_VehicleRegistration
	{
		get => base.AMA_VehicleRegistration;
		set => base.AMA_VehicleRegistration = value;
	}

	[List($"{nameof(Lookups)}.{nameof(AsycudaManifestHeaderLookups.TransportModeList)}")]
	public override ZString AMA_TransportMode
	{
		get => base.AMA_TransportMode;
		set => base.AMA_TransportMode = value;
	}

	[MaxLength(35)]
	public ZString MovementReferenceNumber
	{
		get
		{
			if (movementReferenceNumber is null)
			{
				CusEntryNumberManager.Load(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, AMA_RN_NKCountry, ref movementReferenceNumber);
			}

			return movementReferenceNumber?.CE_EntryNum ?? ZString.Empty;
		}
		set
		{
			var isMovementRefNumberFieldNull = movementReferenceNumber is null;
			var oldValue = isMovementRefNumberFieldNull ? ZString.Empty : movementReferenceNumber.CE_EntryNum;

			CusEntryNumberManager.CreateOrUpdateEntry(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, value, AMA_RN_NKCountry, ref movementReferenceNumber);

			if (isMovementRefNumberFieldNull && movementReferenceNumber is not null)
			{
				RegisterEditableChildObject(movementReferenceNumber);
			}

			MovementReferenceNumberInfo.RefreshBinding(oldValue);
		}
	}

	CusEntryNumber movementReferenceNumber;

	public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(Schema.MovementReferenceNumber);

	ZString IDocumentUploadSupporter.DeclarationId => GetDeclarationId();
	protected virtual ZString GetDeclarationId() => string.Empty;

	ZString IMovementReferenceNumberSupporter.MovementReferenceNumber
	{
		get => MovementReferenceNumber;
		set => MovementReferenceNumber = value;
	}

	ZString IMovementRequestIdProvider.RequestId => GetRequestId();
	protected virtual ZString GetRequestId() => string.Empty;

	ZString ITransportModeProvider.TransportMode => AMA_TransportMode;

#if DEBUG

	protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
	{
		base.FillWithValidTestDataCore(kind, propertyPath);
		AMA_ManifestType = NOManifestTypes.Codes.DMO;
	}
#endif
}
