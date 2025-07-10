using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceHeader : TypeSafeJobComInvoiceHeader, Integration.Customs.SG.IJobComInvoiceHeader
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SG_GSTRate = (ZInt)(GetGstRate() * 100);
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return JobDeclaration.LocalCurrencyConstantCode; }
		}

		protected override ZDateTime EffectiveValuationDateCore
		{
			get { return JZ_ValuationDateOverride.IsValid ? JZ_ValuationDateOverride : ZDateTime.Today; }
		}

		public bool HasPreferentialDuty
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in JobComInvoiceLines)
				{
					if (invoiceLine.PreferenceRateApplies)
					{
						return true;
					}
				}

				return false;
			}
		}

		public override ZGuid JZ_JE
		{
			get { return base.JZ_JE; }
			set
			{
				base.JZ_JE = value;
				//for when invoices are attached/detached. Validation runs inside CheckJZ_JE
				if (!IsDataChangeSuspendedByFakeDeclaration && JobDeclaration != null)
				{
					JobDeclaration.JE_Calc_InvoicesCountInfo.RefreshBinding();
				}

				if (JZ_RX_NKInvoice_Currency.IsEmpty && JobDeclaration != null && !JobDeclaration.IsPersistent)
				{
					JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				}
			}
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("99542B67-5C47-418C-851A-50662C5B25DF", Caption = "Valuation Date")]
		public override ZDateTime JZ_ValuationDateOverride
		{
			get => base.JZ_ValuationDateOverride;
			set
			{
				var overrideDate = value;
				if (overrideDate > ZDateTime.Today)
				{
					overrideDate = ZDateTime.Empty;
				}
				base.JZ_ValuationDateOverride = overrideDate;
			}
		}

		public override bool SupportsRelatedBill
		{
			get { return false; }
		}

		protected override ZBool IsImportForStandAloneInvoice
		{
			get
			{
				var messageType = this.JZ_MessageType;
				return messageType == MessageTypeCodeList.Codes.INP || messageType == MessageTypeCodeList.Codes.IPT || messageType == MessageTypeCodeList.Codes.TNP;
			}
		}

		protected override ZBool IsExportForStandAloneInvoice
		{
			get
			{
				var messageType = this.JZ_MessageType;
				return messageType == MessageTypeCodeList.Codes.OUT || messageType == MessageTypeCodeList.Codes.COO || messageType == MessageTypeCodeList.Codes.TNP;
			}
		}

		ZDecimal GetGstRate()
		{
			var assessmentDate = JobDeclaration != null ? JobDeclaration.DateForDutyRate : ZDateTime.Today;
			var fee = new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, assessmentDate);
			return fee?.ZZF_Value ?? ZDecimal.Zero;
		}

		#region Fetch Hints

		protected override ZArchitecture.Business.EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobComInvoiceHeaderFetchStrategy(this);
		}

		class JobComInvoiceHeaderFetchStrategy : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy
		{
			public JobComInvoiceHeaderFetchStrategy(JobComInvoiceHeader invoice)
				: base(invoice)
			{
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);
			}
		}

		#endregion

		#region Validation

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			Customs.Business.JobComInvoiceHeaderValidation result = null;

			if (JobDeclaration != null)
			{
				if (JobDeclaration.JE_MessageType == MessageTypeCodeList.Codes.IPT)
				{
					result = new JobComInvoiceHeaderValidation_IPT(this);
				}
				else if (JobDeclaration.JE_MessageType == MessageTypeCodeList.Codes.INP)
				{
					result = new JobComInvoiceHeaderValidation_INP(this);
				}
				else if (JobDeclaration.JE_MessageType == MessageTypeCodeList.Codes.TNP)
				{
					result = new JobComInvoiceHeaderValidation_TNP(this);
				}
				else if (JobDeclaration.IsOUTDEC)
				{
					result = new JobComInvoiceHeaderValidation_OUT(this);
				}
				else if (JobDeclaration.JE_MessageType == MessageTypeCodeList.Codes.COO)
				{
					result = new JobComInvoiceHeaderCOValidation(this);
				}
				else
				{
					result = new JobComInvoiceHeaderValidation(this);
				}
			}
			else
			{
				result = new JobComInvoiceHeaderValidation(this);
			}

			return result;
		}

		#endregion
	}
}
