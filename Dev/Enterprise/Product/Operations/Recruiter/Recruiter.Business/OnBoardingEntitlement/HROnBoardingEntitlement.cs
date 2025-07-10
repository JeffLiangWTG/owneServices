using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business
{
	public class HROnBoardingEntitlement : AutoHROnBoardingEntitlement
	{
		public HROnBoardingEntitlement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("HROnBoarding")]
		[List("Lookups.HROnBoardings")]
		public override ZGuid HOC_HOB_HROnBoarding { get => base.HOC_HOB_HROnBoarding; set => base.HOC_HOB_HROnBoarding = value; }

		public HROnBoarding HROnBoarding => Factory.Load<HROnBoarding>(HOC_HOB_HROnBoarding);

		[RelatedBusinessObject("Currency")]
		[List("Lookups.Currencies")]
		public override ZString HOC_RX_NKCurrency { get => base.HOC_RX_NKCurrency; set => base.HOC_RX_NKCurrency = value; }
	}
}
