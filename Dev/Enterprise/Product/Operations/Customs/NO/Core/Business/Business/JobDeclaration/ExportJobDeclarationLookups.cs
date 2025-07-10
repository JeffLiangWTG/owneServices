using System.Globalization;
using System.Linq;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

sealed class ExportJobDeclarationLookups : JobDeclarationLookups
{
	public ExportJobDeclarationLookups(JobDeclaration parent) : base(parent)
	{
	}

	public new CodeDescriptionPairList LocationOfGoodsCollection => Factory.GetCachedValue<ExportGoodsLocationCodeList>();

	public override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<ShipmentTypeExport>();

	public override ICodeDescriptionPairList CustomsOfficeList
	{
		get
		{
			var transportMode = Parent.JE_TransportMode;
			return Factory.GetCachedValue($"NO.JobDeclarationLookups.CustomsOfficeList_{transportMode}", () =>
			{
				var list = new CodeDescriptionPairList();
				var offices = new OfficeTransportModeMap(Factory).MapValues
				.Where(x => x.Value.Contains(transportMode.ToString()))
				.Select(x => x.Key)
				.ToArray();

				MergeWithCustomsOfficeToList(list, offices);
				return list;
			});
		}
	}

	void MergeWithCustomsOfficeToList(CodeDescriptionPairList list, string[] listOfOffices)
	{
		var cachedList = Factory.GetCachedValue<NOCustomsOfficesList>();

		var originalCulture = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = new CultureInfo("nb-NO", false);
		foreach (var officeCode in listOfOffices)
		{
			list.InsertInSortOrderByDescription(new CodeDescriptionPair(officeCode, cachedList.GetDescriptionFromCode(officeCode)));
		}
		CultureInfo.CurrentCulture = originalCulture;
	}
}
