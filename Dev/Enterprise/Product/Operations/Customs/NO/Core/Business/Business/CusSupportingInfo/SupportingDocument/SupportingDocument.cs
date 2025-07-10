using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NO.Business;

public class SupportingDocument : Customs.Business.CusSupportingInfo
{
	public SupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

	public new class Schema : Customs.Business.AutoCusSupportingInfo.Schema
	{
		public new const int CSI_ReferenceNumberMaxLength = 35;
		public new const int CSI_CodeMaxLength = 3;
		public const string DocumentDescription = nameof(SupportingDocument.DocumentDescription);
	}

	public JobDeclaration Declaration
	{
		get
		{
			JobDeclaration result = null;
			switch (Parent)
			{
				case JobDeclaration declaration:
					result = declaration;
					break;
				case JobComInvoiceHeader invoiceHeader:
					result = invoiceHeader.JobDeclaration;
					break;
				case JobComInvoiceLine invoiceLine:
					result = invoiceLine.Declaration;
					break;
			}
			return result;
		}
	}

	public ZBool IsImport => Factory.GetValue(ref isImport, () => Declaration?.IsImport ?? false);
	CachedProperty<ZBool> isImport;

	public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

	public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

	protected override bool IsLookupsCachedInBase => false;

	protected override ZString HumanReadableNameCore => Res.GetString("69E886D0-F959-472C-A41C-AD1AB84A9DA6", "Supporting Document");

	protected override Customs.Business.CusSupportingInfoValidation GetNewValidation()
	{
		return new SupportingDocumentValidation(this);
	}

	protected override Customs.Business.CusSupportingInfoLookups GetNewLookups() => IsImport ? new ImportSupportingDocumentLookups(this) : new ExportSupportingDocumentLookups(this);

	public override bool SupportsNotes => false;

	#region Properties

	[ResourceStringData("NOSupportingDocument|CSI_Code", Caption = "Type")]
	[List(nameof(Lookups) + "." + nameof(SupportingDocumentLookups.CodeList))]
	[MaxLength(Schema.CSI_CodeMaxLength)]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set
		{
			var oldValue = CSI_Code;
			base.CSI_Code = value;
			if (oldValue != value && !IsCopying)
			{
				documentDescriptionCache = null;
				DocumentDescriptionInfo.RefreshBinding();
				if(!IsValidationSuspended)
				{
					Validation.ValidateCSI_ReferenceNumber();
				}
			}
		}
	}

	[ResourceStringData("NOSupportingDocument|CSI_ReferenceNumber", Caption = "Reference", FullDescription = "Reference Number", ShortCaption = "Ref.")]
	[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber
	{
		get => base.CSI_ReferenceNumber;
		set => base.CSI_ReferenceNumber = value;
	}

	[ResourceStringData("NOSupportingDocument|DocumentDescription", Caption = "Description")]
	public ZString DocumentDescription => CachedValueHelper.GetValue(ref documentDescriptionCache, GetSupportingDocumentDescription);

	CachedValue<ZString> documentDescriptionCache;

	public ZPropertyInfo DocumentDescriptionInfo => GetZPropertyInfo(Schema.DocumentDescription);

	ZString GetSupportingDocumentDescription()
	{
		var code = CSI_Code;
		if(code.IsEmpty)
		{
			return ZString.Empty;
		}

		var query = ((BusinessObjectCollection)Lookups.CodeList).CompleteFilter;
		query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, code);
		return Factory.LoadTop1<ZZRefCusCodeListCombined>(query)?.ZZD_Description ?? ZString.Empty;
	}
	#endregion
}
