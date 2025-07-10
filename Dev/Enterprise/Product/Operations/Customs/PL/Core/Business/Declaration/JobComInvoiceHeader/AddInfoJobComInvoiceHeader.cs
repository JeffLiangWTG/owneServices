using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AddInfoJobComInvoiceHeader : EU.Business.Declaration.AddInfoJobComInvoiceHeader
{
	public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoPropertyInfo)
		: base(addInfoPropertyInfo)
	{
	}

	public new AddInfoJobComInvoiceHeaderValidation Validation =>
		(AddInfoJobComInvoiceHeaderValidation)base.Validation;

	public new AddInfoJobComInvoiceHeaderLookups Lookups => (AddInfoJobComInvoiceHeaderLookups)base.Lookups;

	[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.ValuationMethodList))]
	public override ZString ZG_ValuationMethod
	{
		get => base.ZG_ValuationMethod;
		set => base.ZG_ValuationMethod = value;
	}

	[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.TransportMethodOfPaymentList))]
	public override ZString ZG_TransportChargesMethodOfPayment
	{
		get => base.ZG_TransportChargesMethodOfPayment;
		set => base.ZG_TransportChargesMethodOfPayment = value;
	}

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceHeaderLookups(this);

	protected override EUAddInfoValidation GetNewValidation() => IsImport
		? new ImportAddInfoJobComInvoiceHeaderValidation(this)
		: IsExport
			? new ExportAddInfoJobComInvoiceHeaderValidation(this)
			: new AddInfoJobComInvoiceHeaderValidation(this);

	ZBool IsImport => ((ICanBeImportOrExport)Parent).IsImport;

	ZBool IsExport => ((ICanBeImportOrExport)Parent).IsExport;

	protected override ZBool ShouldClearJZ_IncoTermPlace => !IsImport && base.ShouldClearJZ_IncoTermPlace;
}
