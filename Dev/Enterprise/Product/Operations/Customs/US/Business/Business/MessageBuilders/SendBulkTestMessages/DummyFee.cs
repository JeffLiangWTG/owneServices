using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class DummyFee : IFee
	{
		#region IFee Members

		public ZString Code
		{
			get;
			set;
		}

		public ZDecimal Amount
		{
			get;
			set;
		}

		public void Delete()
		{
		}

		public ZString SelectedRateType
		{
			get;
			set;
		}

		public ZBool IsOverridden => false;

		#endregion
	}
}
