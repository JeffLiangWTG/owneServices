using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DutyFeeInformationDocWrapper : NonPersistentBusinessObject, IDutyFeeInformation
	{
		public DutyFeeInformationDocWrapper(IDutyFeeInformation input)
		{
			if (input != null)
			{
				this.Code = input.Code;
				this.Value = input.Value;
			}
		}

		public DutyFeeInformationDocWrapper(ZString code, ZString valueString)
		{
			this.Code = code;
			this.Value = ZDecimal.ParseSafe(valueString, ZDecimal.Zero);
		}

		public DutyFeeInformationDocWrapper(ZString code, ZDecimal value)
		{
			this.Code = code;
			this.Value = value;
		}

		#region IDutyFeeInformation

		public ZString Code { get; private set; }

		public ZDecimal Value { get; private set; }

		#endregion

		#region Implementation

		internal void AddAmountToValue(ZDecimal amount)
		{
			Value += amount;
		}

		#endregion
	}
}
