using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business
{
	public class CusStatementHeader : BaseCusStatementHeader, Integration.Customs.TR.ICusStatementHeader, IDocumentSupportable, ICancellable
	{
		#region Constants

		public static class Constants
		{
			internal const string PaymentTypeBankTransfer = "1";
			internal const string Preliminary = "PRE";
			internal const string Paid = "PAD";
			internal const string Broker = "BRK";
		}

		#endregion

		#region Loader
		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusStatementHeader LoadMonthlyStatementWithPeriodStartDate(ZString statementType, ZDateTime periodEndDate, ZGuid companyPK)
			{
				var filter = CreateMonthlyStatementFilter(statementType, periodEndDate, companyPK);
				return Factory.LoadTop1<CusStatementHeader>(filter);
			}

			ZQuery CreateMonthlyStatementFilter(ZString statementType, ZDateTime periodEndDate, ZGuid companyPK)
			{
				var result = new ZQuery(CusStatementHeaderSchema.B2_StatementType, statementType);
				result.AddToFilter(CusStatementHeaderSchema.B2_GC, companyPK);
				result.AddToFilter(CusStatementHeaderSchema.B2_PeriodEndDate, SQLComparisonOperator.GreaterThanOrEqualTo, periodEndDate);
				result.AddToFilter(CusStatementHeaderSchema.B2_PeriodStartDate, SQLComparisonOperator.LessThanOrEqualTo, periodEndDate);
				result.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);

				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusStatementHeader);
			}
		}

		#endregion

		public CusStatementHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B2_PaymentType = Constants.PaymentTypeBankTransfer;
			B2_Status = Constants.Preliminary;
			B2_PaymentStatus = Constants.Paid;
			B2_PaymentParty = Constants.Broker;
			B2_PaymentType = CusStatementHeader.Constants.PaymentTypeBankTransfer;
			var localTime = Env.Time.CurrentLocalDate;
			this.B2_PeriodStartDate = new ZDate(localTime.Year, localTime.Month, 1);
			this.B2_PeriodEndDate = new ZDate(localTime.Year, localTime.Month, 1).AddMonths(1).AddDays(-1);
			this.B2_DueDate = new ZDate(localTime.Year, localTime.Month, 20).AddMonths(1);
		}

		public new CusStatementHeaderValidation Validation => (CusStatementHeaderValidation)base.Validation;
		protected override Customs.Business.CusStatementHeaderValidation GetNewValidation() => new CusStatementHeaderValidation(this);

		public new CusStatementHeaderLookups Lookups => (CusStatementHeaderLookups)base.Lookups;
		protected override Customs.Business.CusStatementHeaderLookups GetNewLookups() => new CusStatementHeaderLookups(this);

		[ResourceStringData("6528244F-6FB9-4445-861C-67395CF360E0", ShortCaption = "Payment Party", Caption = "Payment Party")]
		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentPartyList))]
		public override ZString B2_PaymentParty { get => base.B2_PaymentParty; set => base.B2_PaymentParty = value; }

		[ResourceStringData("F1F6E112-04F0-4585-9E32-4A7965ED3813", ShortCaption = "Statement Number", Caption = "Statement Number")]
		[ReadOnly(true)]
		public override ZString B2_StatementNumber { get => base.B2_StatementNumber; set => base.B2_StatementNumber = value; }

		[ResourceStringData("DEB0868B-779F-4059-8F4C-9E76D9096EE5", ShortCaption = "Period Start Date", Caption = "Period Start Date")]
		public override ZDate B2_PeriodStartDate { get => base.B2_PeriodStartDate; set => base.B2_PeriodStartDate = value; }

		[ResourceStringData("93723C3F-051E-4490-8886-E3E893F5A105", ShortCaption = "Period End Date", Caption = "Period End Date")]
		public override ZDate B2_PeriodEndDate { get => base.B2_PeriodEndDate; set => base.B2_PeriodEndDate = value; }

		[ResourceStringData("BC9BFEF6-6DD3-4364-891C-D0CC8192A0CD", ShortCaption = "Statement Type", Caption = "Statement Type")]
		public override ZString B2_StatementType { get => base.B2_StatementType; set => base.B2_StatementType = value; }

		[ResourceStringData("D8BF0B61-3BFF-46E0-8EF4-C13BA3180FE6", ShortCaption = "Payment Type", Caption = "Payment Type")]
		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentTypesList))]
		public override ZString B2_PaymentType { get => base.B2_PaymentType; set => base.B2_PaymentType = value; }

		[ResourceStringData("2BD996B9-2AE9-41A4-AA9D-33F4CAF59462", ShortCaption = "Print Date", Caption = "Print Date")]
		[ReadOnly(true)]
		public override ZDateTime B2_PrintDate { get => base.B2_PrintDate; set => base.B2_PrintDate = value; }

		[ResourceStringData("BDE276AE-9910-48A2-B698-C678049EE88B", ShortCaption = "Statement Status", Caption = "Statement Status")]
		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.StatementHeaderStatusList))]
		public override ZString B2_Status { get => base.B2_Status; set => base.B2_Status = value; }

		[ResourceStringData("F8C5F8BA-8E05-4F91-B6F3-5F13BDFF6238", ShortCaption = "Payment Status", Caption = "Payment Status")]
		[List(nameof(Lookups) + "." + nameof(CusStatementHeaderLookups.PaymentStatusList))]
		public override ZString B2_PaymentStatus { get => base.B2_PaymentStatus; set => base.B2_PaymentStatus = value; }

		[ResourceStringData("22D67B0B-3593-444A-A41F-6C10FD797688", ShortCaption = "Total Amount", Caption = "Total Amount")]
		public ZDecimal TotalChargeAmount
		{
			get
			{
				if (totalChargeAmountCached == null)
				{
					totalChargeAmountCached = CalcTotalChargeAmount;
				}
				return totalChargeAmountCached.Value;
			}
		}
		ZDecimal? totalChargeAmountCached;

		ZDecimal CalcTotalChargeAmount
		{
			get
			{
				ZDecimal totalChargeAmount = ZDecimal.Zero;
				if (StatementLines != null)
				{
					foreach (CusStatementLine line in StatementLines)
					{
						totalChargeAmount += line.Charges.OfType<CusStatementLineCharge>().Sum(x => x.B4_ChargeAmount);
					}
				}
				return totalChargeAmount;
			}
		}

		public void ReCalcTotalChargeAmount()
		{
			totalChargeAmountCached = null;
			B2_StatementAmountInfo.RefreshBinding();
		}

		[ChildEditable(true)]
		public CusStatementLineCollection StatementLines
		{
			get
			{
				if (fStatementLines == null)
				{
					fStatementLines = new CusStatementLineCollection(this);
					fStatementLines.Load();
					RegisterEditableChildObject(fStatementLines);
				}
				return fStatementLines;
			}
		}

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = new CusStatementHeaderDocumentSupporter(this));
		DocumentSupporter documentSupporter;

		CusStatementLineCollection fStatementLines;

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateNumberPropertyIfRequired(B2_StatementNumberInfo, GetNewStatementNumber);
		}

		ZString GetNewStatementNumber(BusinessObjectFactory factory)
		{
			var companyCode = this.Company.GC_Code;
			return Env.NumberFountains.GetTRCusStatementNumber(companyCode).GetNextFormatted(factory);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				B2_StatementNumber = ZString.Empty;
			}

			base.OnSaved(saveSucceeded);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("B55F4569-AA39-49BB-B436-9C31A4D2E747", "Statements / Stamp Duty {0}", B2_StatementNumber.TrimEnd());

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusStatementHeader statementHeader)
				: base(statementHeader)
			{
			}

			CusStatementHeader StatementHeader
			{
				get { return (CusStatementHeader)BusinessObject; }
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(CusStatementLineSchema.B3_B2, StatementHeader.PK);
			}
		}

		#endregion

		#region ICancellable Members

		public override string CanCancel()
		{
			var result = base.CanCancel();
			if (StatementLines.Count > 0)
			{
				result = Res.GetString("8977844B-654A-4379-944A-EDD4E278981B", "You cannot deactivate a statement with the entries.");
			}
			return result;
		}

		#endregion
	}
}
