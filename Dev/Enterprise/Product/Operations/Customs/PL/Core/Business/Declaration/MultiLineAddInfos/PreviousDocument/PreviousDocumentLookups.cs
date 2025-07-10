using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class PreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
{
	public PreviousDocumentLookups(PreviousDocument parent) : base(parent)
	{
	}

	protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

	public override CodeDescriptionPairList SubTypeList => Parent.IsExport ? Factory.GetCachedValue<ExportPreviousDocSubTypeList>() : base.SubTypeList;

	public override CodeDescriptionPairList UnitOfQuantityList => RefCusCodeListTypes.GetCachedList(
		Factory,
		Core.Constants.CountryCodes.Poland,
		Parent.IsExport
			? UniversalReferenceConstants.RefCusCodeListType.Codes.ExportCustomsDeclarationUnitsOfQuantity
			: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ,
		ZDateTime.Today);

	public override CodeDescriptionPairList PackTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Poland,
		Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, ZDateTime.Today);

	public override ICollection CodeList => Parent.IsExport
		? GetExportPreviousDocumentCodeList()
		: new ImportPreviousDocumentCodeList(Factory);

	CodeDescriptionPairList GetExportPreviousDocumentCodeList() => Parent.ParentIsJobComInvoiceLine
		? new ExportPreviousDocumentCodeList(Factory)
		: new StandardExportDocumentCodeList(Factory);
}
