using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
{
	public AdditionalInfoLookups(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo parent) : base(parent)
	{
	}

	protected override ZBool OmitLevelAttribute => true;

	public override ICollection CodeList =>
		Parent.ImportExportParent.IsImport
			?
			RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Poland,
				UniversalReferenceConstants.RefCusCodeListType.Codes.ImportAdditionalInformationCodes, ZDate.Today, includeParentDataGrouping: false)
			: Parent.ImportExportParent.IsExport
				? base.CodeList
				: DefaultCodeListCollection;

	ICollection DefaultCodeListCollection => new ZZRefCusCodeListCombinedCollection(Factory);
}
