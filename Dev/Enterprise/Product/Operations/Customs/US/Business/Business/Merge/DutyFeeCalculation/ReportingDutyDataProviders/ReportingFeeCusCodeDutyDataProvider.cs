using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class ReportingFeeDutyDataProvider : IFee
	{
		public bool IsDeleted { get; private set; }
		public ZString Code { get; set; }
		public ZDecimal Amount { get; set; }
		public ZString Preference { get; set; }
		public ZString SelectedRateType { get; set; }
		public ZBool IsOverridden { get; set; }

		void IFee.Delete()
		{
			IsDeleted = true;
		}
	}
}
