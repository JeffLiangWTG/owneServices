using Enterprise.Integration.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	public class EmptyMoney : Money
	{
		internal EmptyMoney() : base(0, null, true)
		{
		}

		protected override void CheckValidForInstantiation(bool overrideIsValid)
		{
			// Don't want to check Currency is valid.... We've overridden the damn thing!
		}

		public override Money Round()
		{
			return this;
		}

		public override Money Round(int decimalPlaces)
		{
			return this;
		}

		public override ICurrency Currency
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency; }
		}
	}
}
