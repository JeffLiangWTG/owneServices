using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class RateTariffDiscount : AutoRateTariffDiscount
	{
		public RateTariffDiscount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Header

		[RelatedBusinessObject("Header")]
		public override ZGuid TD_TH
		{
			get { return base.TD_TH; }
			set { base.TD_TH = value; }
		}

		public CompanyTariff Header
		{
			get { return Factory.Load<CompanyTariff>(TD_TH); }
		}

		#endregion
	}
}


