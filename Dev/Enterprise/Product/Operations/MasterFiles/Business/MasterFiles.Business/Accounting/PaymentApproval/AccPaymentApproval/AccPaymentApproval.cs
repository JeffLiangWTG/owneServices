using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccPaymentApproval : AutoAccPaymentApproval
	{
		#region Schema

		public new abstract class Schema : AutoAccPaymentApproval.Schema
		{
			public const string AV_Code = "AV_Code";
		}

		#endregion

		public AccPaymentApproval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[ReadOnly(true)]
		public override ZString AV_GS_NKApproval1st
		{
			get { return base.AV_GS_NKApproval1st; }
			set { base.AV_GS_NKApproval1st = value; }
		}

		[ReadOnly(true)]
		public override ZString AV_GS_NKApproval2nd
		{
			get { return base.AV_GS_NKApproval2nd; }
			set { base.AV_GS_NKApproval2nd = value; }
		}

		[ReadOnly(true)]
		public override ZString AV_GS_NKApproval3rd
		{
			get { return base.AV_GS_NKApproval3rd; }
			set { base.AV_GS_NKApproval3rd = value; }
		}

		[List("Lookups.TransactionHeaders")]
		public override ZGuid AV_AH
		{
			get { return base.AV_AH; }
			set { base.AV_AH = value; }
		}

		public ZString AV_Code
		{
			get { return PK.ToStringKey(); }
		}

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public override ZDecimal AV_PayExRate
		{
			get => base.AV_PayExRate;
			set => base.AV_PayExRate = value;
		}

		#endregion

		#region BusinessObject Overrides

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = string.Empty;

				if (Header != null)
				{
					result += Header.OH_Code;
				}

				if (!AV_PaymentDate.IsEmpty)
				{
					result += (string.IsNullOrEmpty(result) ? string.Empty : " - ") + AV_PaymentDate.ToShortDateString();
				}

				if (!AV_PaymentComment.IsEmpty)
				{
					result += (string.IsNullOrEmpty(result) ? string.Empty : " - ") + AV_PaymentComment;
				}

				return result;
			}
		}

		#endregion

		#region Decimals

		public int ExchangeRateDecimals => TransactionHeader?.Company.ExchangeRateDecimalPlaces ?? GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		#endregion
	}
}
