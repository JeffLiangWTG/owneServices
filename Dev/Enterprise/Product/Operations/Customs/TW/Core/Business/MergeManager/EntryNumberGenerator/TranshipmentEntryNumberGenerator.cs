using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class TranshipmentEntryNumberGenerator : EntryNumberGenerator
	{
		public TranshipmentEntryNumberGenerator(IEntryNumberGeneratorProvider provider)
			: base(provider)
		{
		}

		public override ZString Part2 => EntryNumberPart2 == EntryNumberPart1 ? new ZString("  ") : EntryNumberPart2;

		protected override ZString Category => RangeTypeList.Codes.T;

		protected override IEnumerable<ZPropertyInfo> GetValidationPropertyInfosCore()
		{
			yield return Provider.EntryNumberPart1Info;
			yield return Provider.CustomsBrokerageBoxNumberInfo;
		}
	}
}
