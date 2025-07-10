using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using InvoiceLineDependentCollection = Enterprise.Customs.Business.InvoiceLineDependentCollection;

namespace Enterprise.Customs.PL.Business.Declaration;

public class JobComInvoiceHeader : AutoJobComInvoiceHeader
	, Integration.Customs.PL.IJobComInvoiceHeader
	, ICusCodeDataTypeSupporter
	, ITranCircumstanceSupporter
{
	public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Poland;

	public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

	public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

	public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

	public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

	public new AddInfoJobComInvoiceHeader AddInfo => (AddInfoJobComInvoiceHeader)base.AddInfo;

	public new AddInfoJobComInvoiceHeaderLookups AddInfoLookups => (AddInfoJobComInvoiceHeaderLookups)base.AddInfoLookups;

	public new AddInfoJobComInvoiceHeaderValidation AddInfoValidation => (AddInfoJobComInvoiceHeaderValidation)base.AddInfoValidation;

	protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() =>
		IsExport ? new ExportJobComInvoiceHeaderLookups(this) : new JobComInvoiceHeaderLookups(this);

	protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation() => IsExport
		? new ExportJobComInvoiceHeaderValidation(this)
		: IsImport
			? new ImportJobComInvoiceHeaderValidation(this)
			: new JobComInvoiceHeaderValidation(this);

	protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
	{
		var dec = JobDeclaration;
		return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
	}

	#region TranCircumstances

	public ZString TranCircumstanceFieldType => nameof(FieldType.TextDropEdit);

	#region TranCircumstanceCode1

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.TranCircumstancesList))]
	[MaxLength(TranCircumstance.Schema.CY_CodeMaxLength)]
	[ResourceStringData("3741A2E7-E32E-4421-A3C8-95A6987C3251", ShortCaption = "Tran. Circ.", MediumCaption = "Tran. Circumstances", Caption = "Transaction Circumstances")]
	public ZString TranCircumstanceCode1
	{
		get => TranCircumstancesCode1?.CY_Code ?? ZString.Empty;
		set
		{
			var oldValue = TranCircumstanceCode1;
			TranCircumstanceHelper.TranCircumstanceSetter(this, TranCircumstanceCode1Info, value, TranCircumstancesCode1, 1);
			if (!IsCopying && oldValue != TranCircumstanceCode1)
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateTranCircumstance1();
				}
			}
		}
	}

	public ZPropertyInfo TranCircumstanceCode1Info => GetZPropertyInfo(nameof(TranCircumstanceCode1));

	TranCircumstance TranCircumstancesCode1
	{
		get
		{
			if (tranCircumstancesCode1 == null || tranCircumstancesCode1.IsDeleted)
			{
				tranCircumstancesCode1 = TranCircumstance.Load(this, 1);
				if (tranCircumstancesCode1 != null)
				{
					RegisterEditableChildObject(tranCircumstancesCode1);
				}
			}
			return tranCircumstancesCode1;
		}
	}
	TranCircumstance tranCircumstancesCode1;

	#endregion

	#region AdditionalTranCircumstanceCodes

	[ReadOnlyMember(nameof(AdditionalTranCircumstanceCodesAsString_ReadOnly))]
	public ZString AdditionalTranCircumstanceCodesAsString => AdditionalTranCircumstanceCodes.AsString;
	public ZPropertyInfo AdditionalTranCircumstanceCodesAsStringInfo => GetZPropertyInfo(nameof(AdditionalTranCircumstanceCodesAsString));
	public bool AdditionalTranCircumstanceCodesAsString_ReadOnly => true;

	[ChildEditable(true)]
	[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
	public TranCircumstanceCollection AdditionalTranCircumstanceCodes
	{
		get
		{
			if (additionalTranCircumstances == null)
			{
				additionalTranCircumstances = TranCircumstanceCollection.New(this, AdditionalTranCircumstanceCodesAsStringInfo, 2);
				additionalTranCircumstances.TranCircumstancesChanged += (object sender, EventArgs e) => Validation.ValidateAdditionalTranCircumstanceCodesAsString();
				RegisterEditableChildObject(additionalTranCircumstances);
			}

			return additionalTranCircumstances;
		}
	}
	TranCircumstanceCollection additionalTranCircumstances;

	public IEnumerable<TranCircumstance> TranCircumstances => AdditionalTranCircumstanceCodes.OfType<TranCircumstance>().Union(new TranCircumstance[] { TranCircumstancesCode1 }).Where(x => x != null);

	#endregion

	#endregion

	#region ICusCodeDataTypeSupporter Members

	protected override Dictionary<ZString, Type> GetCusCodeDataTypes()
	{
		var result = base.GetCusCodeDataTypes();
		result[CusCodeDataTypeList.Codes.DV1] = typeof(TranCircumstance);
		return result;
	}

	#endregion

	protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
	{
		var collection = new InvoiceLineDependentCollection(this);
		collection.Load();
		return new JobComInvoiceLineViewCollection(this, collection);
	}

	public new InvoiceChargeCollection<InvoiceCharge> Charges
	{
		get { return (InvoiceChargeCollection<InvoiceCharge>)base.Charges; }
	}

	protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges()
	{
		return new InvoiceChargeCollection<InvoiceCharge>(this);
	}

	protected override ZString DefaultInvoiceDocument => JobDeclaration.IsExport
		? EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N380
		: EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935;

	protected override HashSet<ZString> ApplicableForUpdateDefaultSupportingDocumentsCodes =>
		JobDeclaration.IsExport
			? base.ApplicableForUpdateDefaultSupportingDocumentsCodes
			: applicableForUpdateDefaultSupportingDocumentsCodesNotExport ??= new () { EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935, };

	HashSet<ZString> applicableForUpdateDefaultSupportingDocumentsCodesNotExport;

	protected override EU.Business.Declaration.AddInfoJobComInvoiceHeader GetNewAddInfo() => new AddInfoJobComInvoiceHeader(JZ_AddInfoInfo);

	public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

	#region CusSupportingInfoTypes

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;
	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;
	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;
	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	#endregion

	public override ZString JZ_RX_NKInvoice_Currency
	{
		get => base.JZ_RX_NKInvoice_Currency;
		set
		{
			var oldValue = JZ_RX_NKInvoice_Currency;
			base.JZ_RX_NKInvoice_Currency = value;
			if (!IsCopying && oldValue != JZ_RX_NKInvoice_Currency)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	public override ZDecimal JZ_Weight
	{
		get => base.JZ_Weight;
		set
		{
			var oldValue = JZ_Weight;
			base.JZ_Weight = value;
			if (!IsCopying && oldValue != JZ_Weight)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	[ReadOnlyMember(nameof(ExitSummaryReadOnly))]
	public override ZString JZ_ValuationCode
	{
		get => base.JZ_ValuationCode;
		set
		{
			var oldValue = JZ_ValuationCode;
			base.JZ_ValuationCode = value;
			if (!IsCopying && oldValue != JZ_ValuationCode)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("PLJobComInvoiceHeader|ZG_AgreedPlaceCode", Caption = "Incoterm Place Code", ShortCaption = "Incoterm Place")]
	[ReadOnlyMember(nameof(ExitSummaryReadOnly))]
	public override ZString ZG_AgreedPlaceCode { get => base.ZG_AgreedPlaceCode; set => base.ZG_AgreedPlaceCode = value; }

	[ResourceStringData("PLJobComInvoiceHeader|ZG_ValuationMethod", Caption = "Valuation Method")]
	public override ZString ZG_ValuationMethod { get => base.ZG_ValuationMethod; set => base.ZG_ValuationMethod = value; }

	[ResourceStringData("PLJobComInvoiceHeader|ZG_TransportChargesMethodOfPayment", ShortCaption = "MoP", MediumCaption = "Tran. Charges MoP", Caption = "Transport Charges Method of Payment")]
	public override ZString ZG_TransportChargesMethodOfPayment { get => base.ZG_TransportChargesMethodOfPayment; set => base.ZG_TransportChargesMethodOfPayment = value; }

	[ReadOnlyMember(nameof(ExitSummaryReadOnly))]
	public override ZString JZ_IncoTerm { get => base.JZ_IncoTerm; set => base.JZ_IncoTerm = value; }

	[ReadOnlyMember(nameof(ExitSummaryReadOnly))]
	public override ZDecimal JZ_NetWeight { get => base.JZ_NetWeight; set => base.JZ_NetWeight = value; }

	[ReadOnlyMember(nameof(ExitSummaryReadOnly))]
	public override ZString JZ_NetWeightUQ { get => base.JZ_NetWeightUQ; set => base.JZ_NetWeightUQ = value; }

	protected override bool JZ_IncoTermPlace_ReadOnly => (!IsImport && base.JZ_IncoTermPlace_ReadOnly) || ExitSummaryReadOnly;

	protected override void ClearIncoTermPlacesIfNeeded()
	{
		if (!AgreedPlaceCodeSupportAndVisible && AgreedPlaceCodeSupport)
		{
			ZG_AgreedPlaceCode = ZString.Empty;
		}
	}

	protected override ZAddress GetNewJZ_OA_SupplierAddress_ZAddress()
	{
		var address = base.GetNewJZ_OA_SupplierAddress_ZAddress();
		address.GetDefaultAddress = GetDefaultAddress;
		return address;
	}

	protected override ZAddress GetNewJZ_OA_BuyerAddress_ZAddress()
	{
		var address = base.GetNewJZ_OA_BuyerAddress_ZAddress();
		address.GetDefaultAddress = GetDefaultAddress;
		return address;
	}

	protected override ZAddress GetNewJZ_OA_ExporterAddress_ZAddress()
	{
		var address = base.GetNewJZ_OA_ExporterAddress_ZAddress();
		address.GetDefaultAddress = GetDefaultAddress;
		return address;
	}

	protected override ZAddress GetNewJZ_OA_SellerAddress_ZAddress()
	{
		var address = base.GetNewJZ_OA_SellerAddress_ZAddress();
		address.GetDefaultAddress = GetDefaultAddress;
		return address;
	}

	protected override ZAddress GetNewJZ_OA_ConsigneeAddress_ZAddress()
	{
		var address = base.GetNewJZ_OA_ConsigneeAddress_ZAddress();
		address.GetDefaultAddress = GetDefaultAddress;
		return address;
	}

	protected override ZAddress GetNewJZ_OA_InvoicerAddress_ZAddress()
	{
		var address = base.GetNewJZ_OA_InvoicerAddress_ZAddress();
		address.GetDefaultAddress = GetDefaultAddress;
		return address;
	}

	protected override ZAddress GetNewJZ_OA_ManufacturerAddress_ZAddress()
	{
		var address = base.GetNewJZ_OA_ManufacturerAddress_ZAddress();
		address.GetDefaultAddress = GetDefaultAddress;
		return address;
	}

	ZGuid GetDefaultAddress(IOrgHeader orgHeader) => orgHeader is OrgHeader organisation ? organisation.MainAddress.PK : ZGuid.Empty;

	bool ExitSummaryReadOnly => JobDeclaration?.IsExitSummary ?? false;
}
