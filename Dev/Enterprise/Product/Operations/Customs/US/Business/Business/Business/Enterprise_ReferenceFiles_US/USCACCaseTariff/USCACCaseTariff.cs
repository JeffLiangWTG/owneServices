using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCACCaseTariff : AutoUSCACCaseTariff
	{
		public USCACCaseTariff(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public ZString FormattedTariff
		{
			get { return new TariffFormatter().DisplayFormat(U9_TariffNumber); }
		}

		internal bool RemoveOnFactorySaving;

		protected override void OnFactorySaving()
		{
			if (RemoveOnFactorySaving && !IsDeleted)
			{
				Delete();
			}
			base.OnFactorySaving();
		}
	}
}
