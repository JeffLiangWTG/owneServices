using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class EntryNumberGeneratorCatD : EntryNumberGenerator
	{
		public EntryNumberGeneratorCatD(IEntryNumberGeneratorProvider provider) : base(provider)
		{
		}

		public override ZString Part2 => new ZString("  ");

		protected override ZString Category => RangeTypeList.Codes.D;

		protected override IEnumerable<ZPropertyInfo> GetValidationPropertyInfosCore()
		{
			yield return Provider.EntryNumberPart1Info;
			yield return Provider.CustomsBrokerageBoxNumberInfo;
		}
	}
}
