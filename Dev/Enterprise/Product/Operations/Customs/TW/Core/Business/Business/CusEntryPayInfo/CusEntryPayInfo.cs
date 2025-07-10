using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.TW.Business
{
	[SystemDefinedValues]
	public partial class CusEntryPayInfo : Customs.Business.CusEntryPayInfo
	{
		public CusEntryPayInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : Customs.Business.AutoCusEntryPayInfo.Schema
		{
			public const string TypeDescription = "TypeDescription";
			public const string ReasonDescription = "ReasonDescription";
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|C9_IncomingPayResponseNo", Caption = "Memo ID")]
		public override ZString C9_IncomingPayResponseNo { get => base.C9_IncomingPayResponseNo; set => base.C9_IncomingPayResponseNo = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|C9_PaymentReference", Caption = "Reference")]
		public override ZString C9_PaymentReference { get => base.C9_PaymentReference; set => base.C9_PaymentReference = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryPayInfoLookups.TransactionTypeList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|C9_TransactionType", Caption = "Type Code")]
		public override ZString C9_TransactionType { get => base.C9_TransactionType; set => base.C9_TransactionType = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|TypeDescription", Caption = "Type Description")]
		public ZString TypeDescription => Lookups.TransactionTypeList.GetDescriptionFromCode(C9_TransactionType);

		public ZPropertyInfo TypeDescriptionInfo => GetZPropertyInfo(Schema.TypeDescription);

		[DecimalPlaces(0)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|C9_PaymentAmount", Caption = "Amount")]
		public override ZDecimal C9_PaymentAmount { get => base.C9_PaymentAmount; set => base.C9_PaymentAmount = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|C9_PaymentDate", Caption = "Due Date")]
		public override ZDateTime C9_PaymentDate { get => base.C9_PaymentDate; set => base.C9_PaymentDate = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|C9_ReceiptDate", Caption = "Receipt Date")]
		public override ZDate C9_ReceiptDate { get => base.C9_ReceiptDate; set => base.C9_ReceiptDate = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|C9_PaymentParty", Caption = "Paid By")]
		public override ZString C9_PaymentParty { get => base.C9_PaymentParty; set => base.C9_PaymentParty = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryPayInfoLookups.ReasonOfPaymentList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|C9_PaymentReasonCode", Caption = "Reason Code")]
		public override ZString C9_PaymentReasonCode { get => base.C9_PaymentReasonCode; set => base.C9_PaymentReasonCode = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|ReasonDescription", Caption = "Reason Description")]
		public ZString ReasonDescription => Lookups.ReasonOfPaymentList.GetDescriptionFromCode(C9_PaymentReasonCode);

		public ZPropertyInfo ReasonDescriptionInfo => GetZPropertyInfo(Schema.ReasonDescription);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryPayInfo|C9_BankAccount", Caption = "Customs Bank Account #")]
		public override ZString C9_BankAccount { get => base.C9_BankAccount; set => base.C9_BankAccount = value; }

		public ZDecimal OtherChargeDeductionAmount
		{
			get => this.GetSystemDefinedValue<ZDecimal>(Constants.GenAddOnColumnFieldName.OtherChargeDeductionAmount);
			set => this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.OtherChargeDeductionAmount, value);
		}
	}
}
