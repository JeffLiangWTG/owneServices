using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class EntryNumberGeneratorCatC : EntryNumberGenerator
	{
		public EntryNumberGeneratorCatC(IEntryNumberGeneratorProvider provider)
			: base(provider)
		{
		}

		protected override ZString Category => RangeTypeList.Codes.C;

		protected override IEnumerable<ZPropertyInfo> GetValidationPropertyInfosCore()
		{
			yield return Provider.EntryNumberPart1Info;
			yield return Provider.EntryNumberPart2Info;
			yield return Provider.CustomsBrokerageBoxNumberInfo;
		}
	}
}
