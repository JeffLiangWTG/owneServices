using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class AccAccountFee : AutoAccAccountFee, IDataVersionLoggingSupported
	{
		public AccAccountFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new abstract class Schema : AutoAccAccountFee.Schema
		{
			public const string AAF_FeeAmountDecimalPlaces = "AAF_FeeAmountDecimalPlaces";
		}

		#region AAF_Rule

		[List("Lookups.AccountFeeCalculationRuleList")]
		public override ZString AAF_Rule
		{
			get { return base.AAF_Rule; }
			set { base.AAF_Rule = value; }
		}

		#endregion

		#region AAF_FeeAmount

		[DecimalPlaces(Schema.AAF_FeeAmountDecimalPlaces)]
		public override ZDecimal AAF_FeeAmount
		{
			get { return base.AAF_FeeAmount; }
			set { base.AAF_FeeAmount = value; }
		}

		public ZInt AAF_FeeAmountDecimalPlaces
		{
			get { return FeeCurrency != null ? FeeCurrency.Decimals : 2; }
		}

		#endregion

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		public static class AccountFeeCalculationRuleType
		{
			public const string DoNotChargeAccountFee = "NON";
			public const string WhenTransactionPosted = "TCR";
			public const string WhenOutstandingBalacneExists = "OSB";
			public const string WhenEitherTransactionPostedOrOutstandingBalanceExists = "TCB";
		}
	}
}
