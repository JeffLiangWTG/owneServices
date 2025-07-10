using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class TariffFormattedFindBoxListProvider : FindBoxListProvider
	{
		public TariffFormattedFindBoxListProvider(IBusinessObjectCollection collection)
			: base(collection)
		{
		}

		protected override void AddCodeEqualsFilter(ZQuery query, string code)
		{
			base.AddCodeEqualsFilter(query, TariffFormatter.Format(code));
		}

		protected override void AddCodeStartsWithFilter(ZQuery query, string code)
		{
			base.AddCodeStartsWithFilter(query, TariffFormatter.Format(code));
		}

		#region TariffFormatter
		protected TariffFormatter TariffFormatter
		{
			get
			{
				if (fTariffFormatter == null)
				{
					fTariffFormatter = new TariffFormatter();
				}
				return fTariffFormatter;
			}
		}
		TariffFormatter fTariffFormatter;
		#endregion
	}
}
