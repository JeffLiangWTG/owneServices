
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class SupportingDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentLookups
{
	public SupportingDocumentLookups(SupportingDocument parent) : base(parent)
	{
	}

	protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

	public override ICollection CodeList
	{
		get
		{
			var importExportParent = Parent.ImportExportParent as BusinessObject;
			if (importExportParent == null || importExportParent.IsDeleted)
			{
				return new CodeDescriptionPairList();
			}

			return Factory.GetSupportingDocumentList(Parent.ImportExportParent.DataGroupingCode, Parent.ParentDirection, Parent.ImportExportParent.Level, GetAdditionalCodeListAttributeFilters(), RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildOnly);
		}
	}

	public override CodeDescriptionPairList UnitOfQuantityList => RefCusCodeListTypes.GetCachedList(
		Factory,
		Core.Constants.CountryCodes.Poland,
		Parent?.Declaration?.IsImport ?? false
			? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ
			: UniversalReferenceConstants.RefCusCodeListType.Codes.ExportCustomsDeclarationUnitsOfQuantity,
		ZDateTime.Today);
}
