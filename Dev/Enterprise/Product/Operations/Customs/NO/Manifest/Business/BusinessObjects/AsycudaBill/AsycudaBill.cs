using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NO.Manifest.Business;

public class AsycudaBill : ASYCUDA.Business.AsycudaBill,
	ICusSupportingInfoTypeSupporter,
	IMovementReferenceNumberSupporter,
	ITransportModeProvider
{
	public AsycudaBill(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
	{
		public const string CustomsLevel = "CustomsLevel";
		public const string EmailAddress1 = "DMOBordPassEm1";
		public const string EmailAddress2 = "DMOBordPassEm2";
		public const string EmailAddress3 = "DMOBordPassEm3";
		public const string ImportProcedure = "DMOImportProcedure";
		public const string ExportProcedure = "DMOExportProcedure";
		public const string MovementReferenceNumber = nameof(AsycudaBill.MovementReferenceNumber);
		public const string TransportDocumentType = nameof(TransportDocumentType);
	}

	public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

	public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

	protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

	public new ASYCUDA.Business.AsycudaBillValidation Validation => base.Validation;

	protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
	protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

	#region Customs Level

	[ResourceStringData("NO.AsycudaBill.CustomsLevel", Caption = "Customs Level")]
	public ZString CustomsLevel => this switch
	{
		{ IsHouseBill: true } => houseBill,
		{ IsMasterBill: true } => masterBill,
		_ => ZString.Empty,
	};

	public ZPropertyInfo CustomsLevelInfo => GetZPropertyInfo(nameof(CustomsLevel));

	#endregion

	#region Email Addresses

	[ResourceStringData("152AA5F6-4D9F-47B2-834C-373EFBCFA0C5", Caption = "Address 1", FullDescription = "Email address for border passing confirmation.")]
	[EmailAddress]
	[MaxLength(70)]
	public ZString EmailAddress1
	{
		get { return this.GetSystemDefinedValue<ZString>(Schema.EmailAddress1); }
		set
		{
			var oldValue = EmailAddress1;
			if (oldValue != value)
			{
				CheckMaximumLength(EmailAddress1Info, value);
				this.SetSystemDefinedValue(Schema.EmailAddress1, AddOnColumnDataType.Codes.String, value);
				EmailAddress1Info.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo EmailAddress1Info => GetZPropertyInfo(nameof(EmailAddress1));

	[ResourceStringData("77BB20A5-7FC1-418D-A7D2-6A8BF6E7168C", Caption = "Address 2", FullDescription = "Email address for border passing confirmation.")]
	[EmailAddress]
	[MaxLength(70)]
	public ZString EmailAddress2
	{
		get { return this.GetSystemDefinedValue<ZString>(Schema.EmailAddress2); }
		set
		{
			var oldValue = EmailAddress2;
			if (oldValue != value)
			{
				CheckMaximumLength(EmailAddress2Info, value);
				this.SetSystemDefinedValue(Schema.EmailAddress2, AddOnColumnDataType.Codes.String, value);
				EmailAddress2Info.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo EmailAddress2Info => GetZPropertyInfo(nameof(EmailAddress2));

	[ResourceStringData("68231987-313C-4391-BD6A-8986046B5B19", Caption = "Address 3", FullDescription = "Email address for border passing confirmation.")]
	[EmailAddress]
	[MaxLength(70)]
	public ZString EmailAddress3
	{
		get { return this.GetSystemDefinedValue<ZString>(Schema.EmailAddress3); }
		set
		{
			var oldValue = EmailAddress3;
			if (oldValue != value)
			{
				CheckMaximumLength(EmailAddress3Info, value);
				this.SetSystemDefinedValue(Schema.EmailAddress3, AddOnColumnDataType.Codes.String, value);
				EmailAddress3Info.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo EmailAddress3Info => GetZPropertyInfo(nameof(EmailAddress3));

	#endregion

	#region Previous Documents

	[ChildEditable(true)]
	public ICusSupportingInfoCollection<PreviousDocument> PreviousDocuments
	{
		get
		{
			if (previousDocuments == null)
			{
				previousDocuments = new CusSupportingInfoCollection<PreviousDocument>(this, Common.NO.CusSupportingInfoTypeList.Codes.PreviousDocument);
				previousDocuments.Load();
				RegisterEditableChildObject(previousDocuments);
			}

			return previousDocuments;
		}
	}
	CusSupportingInfoCollection<PreviousDocument> previousDocuments;

	public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		return new Dictionary<ZString, Type>
		{
			{ Common.NO.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
		};
	}

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	#endregion

	#region Place of loading

	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.PortOfLoadingCodes))]
	[ResourceStringData("NO.AsycudaBill.ABL_RL_NKPortOfLoading", Caption = "Place of Loading", FullDescription = "Location of Loading.")]
	public override ZString ABL_RL_NKPortOfLoading
	{
		get => base.ABL_RL_NKPortOfLoading;
		set
		{
			var oldValue = ABL_RL_NKPortOfLoading;
			base.ABL_RL_NKPortOfLoading = value;
			if (!IsCopying && oldValue != ABL_RL_NKPortOfLoading)
			{
				PopulateUnLocoDescription(ABL_CustomsLoadPortInfo, value, oldValue);
			}
		}
	}

	[ResourceStringData("NO.AsycudaBill.ABL_CustomsLoadPort", FullDescription = "Place of Loading.")]
	public override ZString ABL_CustomsLoadPort
	{
		get => base.ABL_CustomsLoadPort;
		set => base.ABL_CustomsLoadPort = value;
	}

	#endregion

	#region Place of Unloading

	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.PortOfDischargeCodes))]
	[ResourceStringData("NO.AsycudaBill.ABL_RL_NKPortOfDischarge", Caption = "Place of Unloading", FullDescription = "Location of Unloading.")]
	public override ZString ABL_RL_NKPortOfDischarge
	{
		get => base.ABL_RL_NKPortOfDischarge;
		set
		{
			var oldValue = ABL_RL_NKPortOfDischarge;
			base.ABL_RL_NKPortOfDischarge = value;
			if (!IsCopying && oldValue != ABL_RL_NKPortOfDischarge)
			{
				PopulateUnLocoDescription(ABL_CustomsDischargePortInfo, value, oldValue);
			}
		}
	}

	[ResourceStringData("NO.AsycudaBill.ABL_CustomsDischargePort", FullDescription = "Place of Unloading.")]
	public override ZString ABL_CustomsDischargePort
	{
		get => base.ABL_CustomsDischargePort;
		set => base.ABL_CustomsDischargePort = value;
	}

	#endregion

	#region Place of Acceptance

	[ResourceStringData("NO.AsycudaBill.ABL_CustomsOriginPort", FullDescription = "Place of Acceptance.")]
	public override ZString ABL_CustomsOriginPort
	{
		get => base.ABL_CustomsOriginPort;
		set => base.ABL_CustomsOriginPort = value;
	}

	[ResourceStringData("NO.AsycudaBill.ABL_RL_NKOrigin", Caption = "Place of Acceptance", FullDescription = "Place of Acceptance code.")]
	public override ZString ABL_RL_NKOrigin
	{
		get => base.ABL_RL_NKOrigin;
		set => base.ABL_RL_NKOrigin = value;
	}

	#endregion

	#region Place of Delivery

	[ResourceStringData("NO.AsycudaBill.ABL_CustomsFinalDestinationPort", FullDescription = "Place of Delivery.")]
	public override ZString ABL_CustomsFinalDestinationPort
	{
		get => base.ABL_CustomsFinalDestinationPort;
		set => base.ABL_CustomsFinalDestinationPort = value;
	}

	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.FinalDestinationCodes))]
	[ResourceStringData("NO.AsycudaBill.ABL_RL_NKFinalDestination", Caption = "Place of Delivery", FullDescription = "Place of Delivery code.")]
	public override ZString ABL_RL_NKFinalDestination
	{
		get => base.ABL_RL_NKFinalDestination;
		set
		{
			var oldValue = ABL_RL_NKFinalDestination;
			base.ABL_RL_NKFinalDestination = value;
			if (!IsCopying && oldValue != ABL_RL_NKFinalDestination)
			{
				PopulateUnLocoDescription(ABL_CustomsFinalDestinationPortInfo, value, oldValue);
			}
		}
	}

	#endregion

	#region Transport Document Type

	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.TransportDocumentTypeList))]
	[ResourceStringData("NO.AsycudaBill.TransportDocumentType", Caption = "Trans. Doc. Type", FullDescription = "Transport document type.")]
	[MaxLength(4)]
	public ZString TransportDocumentType
	{
		get => this.GetSystemDefinedValue<ZString>(Schema.TransportDocumentType);
		set
		{
			var oldValue = TransportDocumentType;
			if (oldValue != value)
			{
				CheckMaximumLength(TransportDocumentTypeInfo, value);
				this.SetSystemDefinedValue(Schema.TransportDocumentType, value);

				if (!IsValidationSuspended && Validation is AsycudaBillValidationForRegularBill regularBillValidation)
				{
					regularBillValidation.ValidateTransportDocumentType();
				}

				TransportDocumentTypeInfo.RefreshBinding();
				Header?.RefreshBinding();
			}
		}
	}

	public ZPropertyInfo TransportDocumentTypeInfo => GetZPropertyInfo(nameof(TransportDocumentType));

	#endregion

	#region Import & Export Procedures

	[ResourceStringData("BA3FFB70-460E-4AEA-A05F-28A9FC7DD4C3", Caption = "Import Procedure", FullDescription = "Import Procedure tells how the goods are cleared into Norway.Varying procedures require different previous document codes.")]
	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ImportProcedureCodeList))]
	[MaxLength(30)]
	public ZString ImportProcedure
	{
		get { return this.GetSystemDefinedValue<ZString>(Schema.ImportProcedure); }
		set
		{
			var oldValue = ImportProcedure;
			if (oldValue != value)
			{
				CheckMaximumLength(ImportProcedureInfo, value);
				this.SetSystemDefinedValue(Schema.ImportProcedure, AddOnColumnDataType.Codes.String, value);

				if (!IsValidationSuspended
					&& Validation is AsycudaBillValidationForRegularBill regularBillValidation)
				{
					regularBillValidation.ValidateImportProcedure();
				}

				ImportProcedureInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo ImportProcedureInfo => GetZPropertyInfo(nameof(ImportProcedure));

	[ResourceStringData("AC1D64BF-32BD-4DAF-97B8-667A14AB9B0B", Caption = "Export Procedure", FullDescription = "Export Procedure tells how the goods are cleared out of EU/to the Norwegian border. Varying procedures require different previous document codes.")]
	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ExportProcedureCodeList))]
	[MaxLength(10)]
	public ZString ExportProcedure
	{
		get { return this.GetSystemDefinedValue<ZString>(Schema.ExportProcedure); }
		set
		{
			var oldValue = ExportProcedure;
			if (oldValue != value)
			{
				CheckMaximumLength(ExportProcedureInfo, value);
				this.SetSystemDefinedValue(Schema.ExportProcedure, AddOnColumnDataType.Codes.String, value);

				if (!IsValidationSuspended
					&& Validation is AsycudaBillValidationForRegularBill regularBillValidation)
				{
					regularBillValidation.ValidateExportProcedure();
				}

				ExportProcedureInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo ExportProcedureInfo => GetZPropertyInfo(nameof(ExportProcedure));

	#endregion

	#region MRN

	[MaxLength(35)]
	public ZString MovementReferenceNumber
	{
		get
		{
			if (movementReferenceNumber is null)
			{
				CusEntryNumberManager.Load(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode, ref movementReferenceNumber);
			}

			return movementReferenceNumber?.CE_EntryNum ?? ZString.Empty;
		}
		set
		{
			var isMovementRefNumberFieldNull = movementReferenceNumber is null;
			var oldValue = isMovementRefNumberFieldNull ? ZString.Empty : movementReferenceNumber.CE_EntryNum;

			CusEntryNumberManager.CreateOrUpdateEntry(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, value, CountryCode, ref movementReferenceNumber);

			if (isMovementRefNumberFieldNull && movementReferenceNumber is not null)
			{
				RegisterEditableChildObject(movementReferenceNumber);
			}

			MovementReferenceNumberInfo.RefreshBinding(oldValue);
		}
	}
	CusEntryNumber movementReferenceNumber;

	public ZPropertyInfo MovementReferenceNumberInfo => GetZPropertyInfo(Schema.MovementReferenceNumber);

	ZString IMovementReferenceNumberSupporter.MovementReferenceNumber
	{
		get => MovementReferenceNumber;
		set => MovementReferenceNumber = value;
	}

	ZString ITransportModeProvider.TransportMode => Header?.AMA_TransportMode ?? ZString.Empty;

	#endregion

	#region Forwarder Address

	[ResourceStringData("134C107D-AC9E-4B50-A12C-29BE303A7121", Caption = "Representative")]
	[List(nameof(ABL_OA_Forwarder_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
	public override ZGuid ABL_OA_Forwarder
	{
		get => base.ABL_OA_Forwarder;
		set
		{
			var oldValue = base.ABL_OA_Forwarder;

			if (oldValue != value)
			{
				base.ABL_OA_Forwarder = value;
				if (!IsCopying)
				{
					ResetForwarderAddressRegNumberAndType();
				}
			}
		}
	}

	[ReadOnlyMember(nameof(IsForwarderFieldsReadOnly))]
	[ResourceStringData("3CBAA14B-6FFD-4688-8C53-D8494250D021", Caption = "Name")]
	public ZString ABL_ForwarderCompanyName => Forwarder?.CompanyName ?? ZString.Empty;

	[ReadOnlyMember(nameof(IsForwarderFieldsReadOnly))]
	[ResourceStringData("DB9953F6-519C-4075-85E4-B8A02A41F205", Caption = "Street 1")]
	public ZString ABL_ForwarderStreet1 => Forwarder?.OA_Address1 ?? ZString.Empty;

	[ReadOnlyMember(nameof(IsForwarderFieldsReadOnly))]
	[ResourceStringData("B0950CE8-DA20-452F-9783-B2F3CCB595E9", Caption = "Street 2")]
	public ZString ABL_ForwarderStreet2 => Forwarder?.OA_Address2 ?? ZString.Empty;

	[ReadOnlyMember(nameof(IsForwarderFieldsReadOnly))]
	[ResourceStringData("82736053-BCF4-43A7-8A76-EC17DA6A2032", Caption = "City")]
	public ZString ABL_ForwarderCity => Forwarder?.OA_City ?? ZString.Empty;

	[ReadOnlyMember(nameof(IsForwarderFieldsReadOnly))]
	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ForwarderStatePairList))]
	[ResourceStringData("BF16E758-8D6F-48AD-839A-6E9DB759D74C", Caption = "State")]
	public ZString ABL_ForwarderState => Forwarder?.OA_State ?? ZString.Empty;

	[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Countries))]
	[ReadOnlyMember(nameof(IsForwarderFieldsReadOnly))]
	[ResourceStringData("2D4BE983-3558-4760-8892-12E575AFE28A", ShortCaption = "Ctry/Rgn.", Caption = "Country/Region", FullDescription = "The Country/Region code for the Representative address")]
	public ZString ABL_Forwarder_RN_NKCountryCode => Forwarder?.OA_RN_NKCountryCode ?? ZString.Empty;

	[ReadOnlyMember(nameof(IsForwarderFieldsReadOnly))]
	[ResourceStringData("870B2D6B-0956-4F8B-8249-E4FE0C2F1848", Caption = "Postcode")]
	public ZString ABL_ForwarderPostCode => Forwarder?.OA_PostCode ?? ZString.Empty;

	[ReadOnlyMember(nameof(ForwarderOverridableFieldsReadOnly))]
	[ResourceStringData("0660124B-1D61-4942-8659-4E5BCE1C672F", Caption = "Phone")]
	public override ZString ABL_ForwarderPhone
	{
		get => base.ABL_ForwarderPhone;
		set
		{
			var oldValue = base.ABL_ForwarderPhone;
			if (oldValue != value)
			{
				base.ABL_ForwarderPhone = value;
				Validation.ValidateABL_ForwarderEmail();
			}
		}
	}

	[ReadOnlyMember(nameof(IsForwarderFieldsReadOnly))]
	[ResourceStringData("54EB3105-E162-4F65-81C1-213778FC3BC6", Caption = "Reg.No")]
	public ZString ABL_ForwarderRegNumber { get; private set; }

	protected override ZAddress GetNewABL_OA_Forwarder_ZAddress()
	{
		var result = base.GetNewABL_OA_Forwarder_ZAddress();
		result.DefaultAddressType = ZArchitecture.Business.AddressType.OFC;
		return result;
	}

	[ReadOnlyMember(nameof(ForwarderOverridableFieldsReadOnly))]
	[ResourceStringData("0A608767-3023-49AA-B622-6FED9ECB2990", Caption = "Email Address")]
	public override ZString ABL_ForwarderEmail
	{
		get => base.ABL_ForwarderEmail;
		set
		{
			var oldValue = base.ABL_ForwarderEmail;
			if (oldValue != value)
			{
				base.ABL_ForwarderEmail = value;
				Validation.ValidateABL_ForwarderPhone();
			}
		}
	}

	void ResetForwarderAddressRegNumberAndType()
	{
		if (Forwarder != null)
		{
			var (regNumber, _) = GetPartyOrgAddressRegNoAndType(Forwarder, ManifestBase.AsycudaBillAddress.AddressType.FreightForwarder, ForwarderRegNoTypes());

			ABL_ForwarderRegNumber = regNumber;
		}
		else
		{
			ABL_ForwarderRegNumber = ZString.Empty;
			ABL_ForwarderEmail = ZString.Empty;
			ABL_ForwarderPhone = ZString.Empty;
		}
	}

	bool IsForwarderFieldsReadOnly => true;

	bool ForwarderOverridableFieldsReadOnly => ABL_OA_Forwarder.IsEmpty;

	static ZString[] ForwarderRegNoTypes() => Array.Empty<ZString>();

	#endregion

	#region Shipper

	[ReadOnlyMember(nameof(IsShipperFieldsReadOnly))]
	[ResourceStringData("5FE0D8DD-78C0-4D65-9144-49FE5F8342CC", Caption = "Email Address")]
	public override ZString ABL_ShipperEmail
	{
		get => base.ABL_ShipperEmail;
		set
		{
			var oldValue = base.ABL_ShipperEmail;
			if (oldValue != value)
			{
				base.ABL_ShipperEmail = value;
				Validation.ValidateABL_ShipperPhone();
			}
		}
	}

	[ReadOnlyMember(nameof(IsShipperFieldsReadOnly))]
	public override ZString ABL_ShipperPhone
	{
		get => base.ABL_ShipperPhone;
		set
		{
			var newValue = base.ABL_ShipperPhone;
			if (newValue != value)
			{
				base.ABL_ShipperPhone = value;
				Validation.ValidateABL_ShipperEmail();
			}
		}
	}

	[ReadOnlyMember(nameof(IsShipperFieldsReadOnly))]
	public override ZString ABL_ShipperRegNo { get => base.ABL_ShipperRegNo; set => base.ABL_ShipperRegNo = value; }

	bool IsShipperFieldsReadOnly => ShipperUseRealOrg && !IsShipperAndConsigneeFieldsOverriden;

	#endregion

	#region Consignee

	[ReadOnlyMember(nameof(IsConsigneeFieldsReadOnly))]
	[ResourceStringData("CFD9E98B-DA23-48FC-9D74-0B14E0309536", Caption = "Email Address")]
	public override ZString ABL_ConsigneeEmail
	{
		get => base.ABL_ConsigneeEmail;
		set
		{
			var oldValue = base.ABL_ConsigneeEmail;
			if (oldValue != value)
			{
				base.ABL_ConsigneeEmail = value;
				Validation.ValidateABL_ConsigneePhone();
			}
		}
	}

	[ReadOnlyMember(nameof(IsConsigneeFieldsReadOnly))]
	public override ZString ABL_ConsigneePhone
	{
		get => base.ABL_ConsigneePhone;
		set
		{
			var oldValue = base.ABL_ConsigneePhone;
			if (oldValue != value)
			{
				base.ABL_ConsigneePhone = value;
				Validation.ValidateABL_ConsigneeEmail();
			}
		}
	}

	[ReadOnlyMember(nameof(IsConsigneeFieldsReadOnly))]
	public override ZString ABL_ConsigneeRegNo { get => base.ABL_ConsigneeRegNo; set => base.ABL_ConsigneeRegNo = value; }

	bool IsConsigneeFieldsReadOnly => ConsigneeUseRealOrg && !IsShipperAndConsigneeFieldsOverriden;

	#endregion

	#region Bill Number

	[ResourceStringData(
		"NO.AsycudaBill.ABL_BillNumber",
		Caption = "Bill Number",
		FullDescription = "Master Bill Number. All houses (STD bills) on this master must refer to this transport document number when they are submitted (individually) to customs.",
		IsApplicableMember = nameof(IsChildMasterBill))]
	public override ZString ABL_BillNumber
	{
		get => base.ABL_BillNumber;
		set => base.ABL_BillNumber = value;
	}

	#endregion

	#region UN LocationCodes

	void PopulateUnLocoDescription(ZPropertyInfo descriptionInfo, ZString newLocationCode, ZString oldLocationCode)
	{
		var maxLength = descriptionInfo.MaxLength;
		var currentDescription = (ZString)descriptionInfo.Value;
		if (currentDescription.IsEmpty || GetUnLocoDescriptionFromCode(oldLocationCode).Left(maxLength) == currentDescription)
		{
			if (newLocationCode.Length == 5)
			{
				var description = GetUnLocoDescriptionFromCode(newLocationCode);
				if (!description.IsEmpty)
				{
					descriptionInfo.Value = description.Left(maxLength);
				}
			}
			else if (newLocationCode.IsEmpty)
			{
				descriptionInfo.Value = ZString.Empty;
			}
		}
	}

	ZString GetUnLocoDescriptionFromCode(ZString code) => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code)?.RL_NameWithDiacriticals ?? ZString.Empty;

	#endregion

	internal void SetDefaultValueForRepresentative()
	{
		if (ABL_OA_Forwarder_ZAddress is not { OrgPK.IsEmpty: true } forwarderAddress)
		{
			return;
		}

		forwarderAddress.OrgPK = this switch
		{
			_ when GlbBranch.CurrentBranch.GB_OH_OrgProxy is { IsEmpty: false } branchProxy => branchProxy,
			_ when GlbCompany.CurrentCompany.GC_OH_OrgProxy is { IsEmpty: false } companyProxy => companyProxy,
			_ => ZGuid.Empty
		};
	}

	public bool IsHouseBill => IsStandardHouseBill ||
			(IsCoLoadMasterBill && (ABL_OA_Forwarder.IsEmpty || ABL_OA_Forwarder == MasterBill.ABL_OA_Forwarder));

	public bool IsMasterBill => IsChildMasterBill ||
		(IsCoLoadMasterBill && !ABL_OA_Forwarder.IsEmpty && ABL_OA_Forwarder != MasterBill.ABL_OA_Forwarder);

	AsycudaBill MasterBill => Header.MasterBill;

	bool IsCoLoadMasterBill => ABL_BolType == Core.Constants.ShipmentTypes.CoLoadMaster;
	bool IsStandardHouseBill => ABL_BolType == Core.Constants.ShipmentTypes.StandardHouse;

	bool IsShipperAndConsigneeFieldsOverriden => (Header?.IsStandAlone ?? false) || (Header?.AMA_OverrideFreightDefaults ?? false);

	readonly string houseBill = (NoResString)"House";

	readonly string masterBill = (NoResString)"Master";
}
