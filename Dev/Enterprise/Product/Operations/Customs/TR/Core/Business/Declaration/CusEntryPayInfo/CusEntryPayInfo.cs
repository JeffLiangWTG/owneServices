using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public partial class CusEntryPayInfo : Customs.Business.CusEntryPayInfo, Integration.Customs.TR.ICusEntryPayInfo
	{
		public CusEntryPayInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnSaving()
		{
			if (C9_PaymentAmount == 0m)
			{
				Delete();
			}

			base.OnSaving();
		}

		[ResourceStringData("72F235A0-56F0-44B7-B396-63F21BBFC9B0", Caption = "Current Loan")]
		[ReadOnlyMember(nameof(C9_PaymentAmount_ReadOnly))]
		[DecimalPlaces(nameof(C9_PaymentAmountDecimalPlace))]
		public override ZDecimal C9_PaymentAmount
		{
			get => base.C9_PaymentAmount;
			set => base.C9_PaymentAmount = value;
		}

		public int C9_PaymentAmountDecimalPlace => IsExportUnionPaymentParty ? 2 : 4;
		public bool C9_PaymentAmount_ReadOnly => IsExportUnionPaymentParty;

		[ResourceStringData("A92C37BA-7CED-4253-86A7-0B41B4D2658E", ShortCaption = "Union Rec.Num.", Caption = "Union Record Number")]
		[MaxLength(nameof(C9_PaymentReferenceMaxLength))]
		[ReadOnlyMember(nameof(C9_PaymentReference_ReadOnly))]
		public override ZString C9_PaymentReference
		{
			get => base.C9_PaymentReference;
			set => base.C9_PaymentReference = value;
		}
		public bool C9_PaymentReference_ReadOnly => IsExportUnionPaymentParty;
		public int C9_PaymentReferenceMaxLength => IsExportUnionPaymentParty ? 16 : CusEntryPayInfo.Schema.C9_PaymentReferenceMaxLength;

		[ResourceStringData("1D6F57F5-D8DB-464E-B8C1-D08D10CEA4E5", ShortCaption = "Union Appr.Code", Caption = "Union Approval Code")]
		[MaxLength(nameof(C9_IncomingPayResponseNoMaxLength))]
		[ReadOnlyMember(nameof(C9_IncomingPayResponseNo_ReadOnly))]
		public override ZString C9_IncomingPayResponseNo
		{
			get => base.C9_IncomingPayResponseNo;
			set => base.C9_IncomingPayResponseNo = value;
		}

		public bool C9_IncomingPayResponseNo_ReadOnly => IsExportUnionPaymentParty;
		public int C9_IncomingPayResponseNoMaxLength => IsExportUnionPaymentParty ? 19 : CusEntryPayInfo.Schema.C9_IncomingPayResponseNoMaxLength;

		[ResourceStringData("0A3E2E6E-A56E-4D4E-955B-35267D06D341", Caption = "TPS Reference")]
		[MaxLength(nameof(C9_BankAccountMaxLength))]
		[ReadOnlyMember(nameof(C9_BankAccount_ReadOnly))]
		public override ZString C9_BankAccount
		{
			get => base.C9_BankAccount;
			set => base.C9_BankAccount = value;
		}

		public bool C9_BankAccount_ReadOnly => IsExportUnionPaymentParty;
		public int C9_BankAccountMaxLength => IsExportUnionPaymentParty ? 20 : CusEntryPayInfo.Schema.C9_BankAccountMaxLength;

		public bool IsExportUnionPaymentParty => C9_PaymentParty == ExportUnionPaymentParty;
		const string ExportUnionPaymentParty = "EXU";
	}
}
