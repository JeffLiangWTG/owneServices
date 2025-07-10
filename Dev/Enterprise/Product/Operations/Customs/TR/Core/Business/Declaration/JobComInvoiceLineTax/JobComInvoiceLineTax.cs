using System;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.TR.Business.Declaration
{
	[SystemDefinedValues]
	public class JobComInvoiceLineTax : EU.Business.Declaration.JobComInvoiceLineTax
	{
		public JobComInvoiceLineTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoJobComInvoiceLineTax.Schema
		{
			public const string NationalType = "NationalType";
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		[ResourceStringData("Enterprise.Customs.TR.Business.JobComInvoiceLineTax|JLT_TypeDescription", Caption = "Description")]
		public ZString JLT_TypeDescription => Lookups.TypeList.GetDescriptionFromCode(JLT_Type) ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.TR.Business.JobComInvoiceLineTax|JLT_RateOverrideReasonCode", Caption = "Action")]
		public override ZString JLT_RateOverrideReasonCode
		{
			get => base.JLT_RateOverrideReasonCode;
			set => base.JLT_RateOverrideReasonCode = value;
		}

		[ResourceStringData("Enterprise.Customs.TR.Business.JobComInvoiceLineTax|JLT_BaseValue", Caption = "Base Amount")]
		[DecimalPlaces(2)]
		public override ZDecimal JLT_BaseValue
		{
			get => base.JLT_BaseValue;
			set
			{
				base.JLT_BaseValue = value;
				CalculateAmount();
				JLT_BaseValueInfo.RefreshBinding();
			}
		}

		[ResourceStringData("Enterprise.Customs.TR.Business.JobComInvoiceLineTax|JLT_Rate", Caption = "Tax Rate")]
		[DecimalPlaces(2)]
		public override ZDecimal JLT_Rate
		{
			get => base.JLT_Rate;
			set
			{
				base.JLT_Rate = value;
				CalculateAmount();
				JLT_RateInfo.RefreshBinding();
			}
		}

		[ResourceStringData("Enterprise.Customs.TR.Business.JobComInvoiceLineTax|JLT_Amount", Caption = "Total Amount")]
		[DecimalPlaces(2)]
		public override ZDecimal JLT_Amount
		{
			get => base.JLT_Amount;
			set
			{
				base.JLT_Amount = value;
				CalculateRate();
				JLT_AmountInfo.RefreshBinding();
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.MethodOfCalculationList))]
		public override ZString JLT_MethodOfCalculation => base.JLT_MethodOfCalculation;

		public override ZString JLT_Type { get => base.JLT_Type; set => base.JLT_Type = value; }

		[ResourceStringData("73F8BFA8-7DD5-4C81-BB48-4E0C1C820504", Caption = "Duty Type")]
		[MaxLength(Schema.JLT_TypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineTaxLookups.TypeList))]
		public ZString NationalType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.NationalType);
			set
			{
				var oldValue = NationalType;
				if (value != oldValue && !IsCopying)
				{
					CheckMaximumLength(NationalTypeInfo, value);
					this.SetSystemDefinedValue(Schema.NationalType, value);
					NationalTypeInfo.RefreshBinding(oldValue);

					JLT_Type = NationalType == DeclarationHelper.NationalVatType.Code ? FeeTypeList.Codes.B00 : NationalType;
				}
			}
		}

		[ResourceStringData("46A01BFD-2FE7-4A31-83BB-CB0E55FB4FB0", Caption = "Description")]
		public ZString NationalTypeDescription => Lookups.TypeList.GetDescriptionFromCode(NationalType) ?? ZString.Empty;

		public ZPropertyInfo NationalTypeInfo => GetZPropertyInfo(Schema.NationalType);

		public new JobComInvoiceLineTaxLookups Lookups => (JobComInvoiceLineTaxLookups)base.Lookups;

		protected override Customs.Business.JobComInvoiceLineTaxLookups GetNewLookups() => new JobComInvoiceLineTaxLookups(this);

		public new JobComInvoiceLineTaxValidation Validation => (JobComInvoiceLineTaxValidation)base.Validation;

		protected override Customs.Business.JobComInvoiceLineTaxValidation GetNewValidation() => new JobComInvoiceLineTaxValidation(this);

		void CalculateAmount()
		{
			if (!IsOnCalculation)
			{
				using (SuspendOnCalculation())
				{
					if (JLT_Rate != 0)
					{
						JLT_Amount = new ZDecimal(JLT_BaseValue * JLT_Rate / 100.0m);
					}
				}
			}
		}

		void CalculateRate()
		{
			if (!IsOnCalculation)
			{
				using (SuspendOnCalculation())
				{
					if (!JLT_Amount.IsEmpty && !JLT_BaseValue.IsEmpty)
					{
						JLT_Rate = new ZDecimal(JLT_Amount / JLT_BaseValue * 100.0m);
					}
				}
			}
		}

		int inCalculation;

		public bool IsOnCalculation => inCalculation > 0;

		public IDisposable SuspendOnCalculation() => new DisposableAction(() => inCalculation++, () => inCalculation--);
	}
}
