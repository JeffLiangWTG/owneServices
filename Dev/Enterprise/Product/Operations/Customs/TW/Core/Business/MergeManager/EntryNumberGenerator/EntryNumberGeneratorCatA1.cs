using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class EntryNumberGeneratorCatA1 : EntryNumberGeneratorCatA
	{
		public EntryNumberGeneratorCatA1(IEntryNumberGeneratorProvider provider)
			: base(provider)
		{
		}

		protected override ZString Category => Constants.RangeTypes.A1;

		public override bool IsAutoGenerateEntryNumberAllowed => false;

		public override ZString CannotAutoGenerateEntryNumberMessage => Res.GetString("D9187E86-2C64-448D-8515-D2C3920D629C", "When the Sea Office of Receipt transships to the Air Office of Lading, please enter the Entry number manually.");
	}
}
