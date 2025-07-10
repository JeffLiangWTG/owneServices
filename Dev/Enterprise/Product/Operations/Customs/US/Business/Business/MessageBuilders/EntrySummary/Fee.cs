using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class Fee : IFee
	{
		#region IFee Members

		public ZString Code
		{
			get { return classCode; }
			set
			{
				classCode = value;
			}
		}

		public ZDecimal Amount
		{
			get { return amount; }
			set
			{
				amount = value;
			}
		}

		public ZBool IsOverriden
		{
			get { return false; }
		}

		public void Delete()
		{
			//do nothing
		}

		public ZString SelectedRateType { get; set; }

		public ZBool IsOverridden => false;

		#endregion

		#region Implementation

		ZString classCode;
		ZDecimal amount;

		#endregion
	}
}
