using System;

namespace CargoWise.Billing.API
{
	public class ELKTransaction
	{
		public string AdditionalRefs { get; set; }
		public DateTime ServiceOccuredUTC { get; set; }
	}
}
