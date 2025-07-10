using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCACCaseBondCash : AutoUSCACCaseBondCash
	{
		public USCACCaseBondCash(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public ZString IndicatorDesc
		{
			get { return Factory.GetCachedValue<BondCashIndicatorList>().GetDescriptionFromCode(U8_Indicator) ?? ZString.Empty; }
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
