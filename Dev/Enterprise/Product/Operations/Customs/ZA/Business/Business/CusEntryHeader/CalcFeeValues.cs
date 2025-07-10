using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Business
{
	internal struct CalcFeeValues
	{
		public IEnumerable<IDutyFeeInformation> CustomsDutiesExcluding12B;
		public ZDecimal CustomsDutyExcluding12B;
		public ZDecimal S1P2BDuty;
		public ZDecimal ValueAddedTax;
		public ZDecimal ProvisionalPayment;
		public ZDecimal Penalty;
		public ZDecimal CustomsDutiesSchedule1P1andSchedule2;
		public IEnumerable<IDutyFeeInformation> Penalties;
		public IEnumerable<IDutyFeeInformation> ProvisionalPayments;
	}
}
