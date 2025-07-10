using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	abstract class CusStatementHeaderIAccIntegrationDataProviderBase : IAccIntegrationDataProvider, ICustomsPaymentDataProvider
	{
		public CusStatementHeaderIAccIntegrationDataProviderBase(CusStatementHeader statement, BusinessObjectFactory billingFactory)
		{
			this.billingFactory = billingFactory;

			this.statementInBillingFactory = billingFactory != statement.Factory ? billingFactory.Load<CusStatementHeader>(statement.PK) : statement;
			statementInBillingFactory.ServiceTaskLogger = statement.ServiceTaskLogger;
		}

		protected readonly CusStatementHeader statementInBillingFactory;
		protected readonly BusinessObjectFactory billingFactory;

		public abstract ChargePosterBehaviours Action { get; }

		public abstract bool ShouldMakePayment { get; }

		public abstract bool ShouldSaveAfterIntegration { get; }

		public ZGuid[] DisbursementChargeCodes
		{
			get { return billingFactory.GetCachedValue<Registry.Business.Customs.US.EntryChargeTypeList>().GetAllChargeCodePKsOf(statementInBillingFactory.B2_GC); }
		}

		BusinessObjectFactory IAccIntegrationDataProvider.Factory
		{
			get { return billingFactory; }
		}

		public IEnumerable<IAccInvoiceDataProvider> InvDataProviderCandidates
		{
			get { return InvDataProviders; }
		}

		public IAccInvoiceDataProvider[] InvDataProviders
		{
			get { return GetAllLines(statementInBillingFactory).ToArray(); }
		}

		public IEnumerable<IAPPaymentGroup> APPaymentGroups
		{
			get
			{
				return AccInvoiceDataProviderAPPaymentGroupFactory.GetAPPaymentGroups(statementInBillingFactory);
			}
		}

		public ZGuid JobPK
		{
			get { return statementInBillingFactory.PK; }
		}

		public ZString JobType
		{
			get { return "Statement"; }
		}

		public ZGuid AutoPostingEmailRecipient
		{
			get { return EmailGroupPK; }
		}

		public GlbCompany Company
		{
			get { return statementInBillingFactory.Company; }
		}

		Action IAccIntegrationDataProvider.OnIntegrated
		{
			get { return OnAccInvoicingIntegrated; }
		}

		void OnAccInvoicingIntegrated()
		{
		}

		public ZString ReferenceID
		{
			get { return statementInBillingFactory.B2_StatementNumber; }
		}

		public bool SupportIntegration
		{
			get
			{
				return statementInBillingFactory.IsPaidByBroker && statementInBillingFactory.B2_Status != StatementHeaderStatusList.Codes.Deleted && SupportIntegrationCore;
			}
		}

		protected abstract bool SupportIntegrationCore { get; }

		void IAccIntegrationDataProvider.LogPostingResult(string message)
		{
			if (statementInBillingFactory != null && statementInBillingFactory.ServiceTaskLogger != null)
			{
				statementInBillingFactory.ServiceTaskLogger.Log(message);
			}
		}

		public static IEnumerable<CusStatementLine> GetAllLines(CusStatementHeader statement)
		{
			List<CusStatementLine> result = new List<CusStatementLine>();

			if (statement.IsMonthlyStatement)
			{
				foreach (CusStatementHeader daily in statement.DailyStatements)
				{
					result.AddRange(daily.GetStatementLinesToAutorate());
				}
			}
			else if (statement.IsDailyStatement)
			{
				result.AddRange(statement.GetStatementLinesToAutorate());
			}

			return result;
		}

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return statementInBillingFactory; }
		}

		public ControllerID ControllerID
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.ControllerID : null;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.BusinessObjectPK : Guid.Empty;
			}
		}

		#endregion

		ZGuid ICustomsPaymentDataProvider.BankAccountPK
		{
			get { return new BankAccountCalculator().CalculateBankAccount(statementInBillingFactory.Company, statementInBillingFactory.B2_AccountNo, statementInBillingFactory.B2_BranchDesignation); }
		}

		ZString ICustomsPaymentDataProvider.BankAccountUniqueID
		{
			get { return statementInBillingFactory.B2_StatementNumber; }
		}

		OrgHeader ICustomsPaymentDataProvider.Creditor
		{
			get { return billingFactory.Load<OrgHeader>(Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.GetFallBackValueAtAllLevels(statementInBillingFactory.B2_GC.ToGuid(), Guid.Empty, Guid.Empty)); }
		}

		public ZGuid EmailGroupPK
		{
			get
			{
				var branch = statementInBillingFactory.GetRelatedBranch();

				if (branch == null && statementInBillingFactory.Company.Branches.Count > 0)
				{
					branch = statementInBillingFactory.Company.Branches[0];
				}

				return CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.GetFallBackValueAtAllLevels(statementInBillingFactory.Company.PK.ToGuid(), branch != null ? branch.PK.ToGuid() : Guid.Empty, Guid.Empty).SendGroupPK;
			}
		}

		ZString ICustomsPaymentDataProvider.HyperLinkText
		{
			get
			{
				string url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(this);
				return "<a href=\"" + url + "\">" + statementInBillingFactory.B2_StatementNumber + "</a>";
			}
		}

		public abstract bool SendEmail { get; }

		GlbCompany ICustomsPaymentDataProvider.Company
		{
			get { return statementInBillingFactory.Company; }
		}

		ZDateTime ICustomsPaymentDataProvider.PaymentDate => statementInBillingFactory.PaymentDateCalculated;
	}

	class UserInvokedStatementAccInvoiceIntegrationDataProvider : CusStatementHeaderIAccIntegrationDataProviderBase
	{
		public UserInvokedStatementAccInvoiceIntegrationDataProvider(CusStatementHeader statement)
			: base(statement, statement.Factory)
		{
		}

		protected override bool SupportIntegrationCore
		{
			get { return true; }
		}

		public override bool ShouldSaveAfterIntegration
		{
			get { return false; }
		}

		public override ChargePosterBehaviours Action
		{
			get
			{
				var result = ChargePosterBehaviours.None;

				if (statementInBillingFactory.PostAPInvoices)
				{
					result |= ChargePosterBehaviours.AutoRateDSB;
					result |= ChargePosterBehaviours.APPostDSB;

					if (statementInBillingFactory.IsFinal)
					{
						result |= ChargePosterBehaviours.PostNegativeCost;
					}
				}

				if (statementInBillingFactory.PostARInvoices)
				{
					result |= ChargePosterBehaviours.AutoRateDSB;
					result |= ChargePosterBehaviours.ARPostDSB;
				}

				return result;
			}
		}

		public override bool ShouldMakePayment
		{
			get { return statementInBillingFactory.MakePayment; }
		}

		public override bool SendEmail
		{
			get { return false; }
		}
	}

	class MessageProcessingStatementAccInvoiceIntegrationDataProvider : CusStatementHeaderIAccIntegrationDataProviderBase
	{
		public MessageProcessingStatementAccInvoiceIntegrationDataProvider(CusStatementHeader statement)
			: base(statement, new BusinessObjectFactory())
		{
			this.actions = statement.GetActions();
			this.shouldMakePayment = statement.ShouldMakePayment();
		}

		readonly ChargePosterBehaviours actions;
		readonly bool shouldMakePayment;

		protected override bool SupportIntegrationCore
		{
			get
			{
				var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetValueWithoutFallback(statementInBillingFactory.B2_GC.ToGuid(), Guid.Empty, Guid.Empty);

				return options.EnableAccountingIntegration;
			}
		}

		public override bool ShouldSaveAfterIntegration
		{
			get { return true; }
		}

		public override ChargePosterBehaviours Action
		{
			get { return actions; }
		}

		public override bool ShouldMakePayment
		{
			get { return shouldMakePayment; }
		}

		public override bool SendEmail
		{
			get { return ShouldMakePayment || (Action & ChargePosterBehaviours.SendEmail) == ChargePosterBehaviours.SendEmail; }
		}
	}

	static class StatementAccIntegrationExtensionMethods
	{
		public static ChargePosterBehaviours GetActions(this CusStatementHeader statement)
		{
			var result = ChargePosterBehaviours.None;

			var options = CustomsDataRegistry.Instance.EnableAccountingIntegration.GetValueWithoutFallback(statement.B2_GC.ToGuid(), Guid.Empty, Guid.Empty);

			if (options.EnableAccountingIntegration)
			{
				if (statement.IsDailyStatement && statement.B2_StatementAmount > 0m)
				{
					if (statement.HasStatusJustBeenChanged || statement.HasPaymentJustBeenAccepted)
					{
						//report AR discrepancies or report unposted AR
						result |= ChargePosterBehaviours.AutoRateDSB;
					}
				}

				if (statement.HasStatementLiabilityJustBeenFinalised())
				{
					result |= ChargePosterBehaviours.APPostDSB;
					result |= ChargePosterBehaviours.PostNegativeCost;
				}

				if (result > 0)
				{
					result |= ChargePosterBehaviours.SendEmail;
				}
			}

			return result;
		}

		public static bool HasStatementLiabilityJustBeenFinalised(this CusStatementHeader statement)
		{
			return statement.HasStatusJustBeenChanged && statement.IsFinal && statement.B2_StatementAmount > 0m;
		}

		public static bool ShouldMakePayment(this CusStatementHeader statement)
		{
			return !statement.IsPeriodicDailyStatement && statement.HasStatementLiabilityJustBeenFinalised();
		}

		public static IEnumerable<CusStatementLine> GetStatementLinesToAutorate(this CusStatementHeader statement)
		{
			return from CusStatementLine line in statement.StatementLines
				   where line.B3_Status != StatementLineStatusList.Codes.Deleted
				   && line.B3_CustomsFeesTotal > 0m
				   select line;
		}
	}

	static class AccInvoiceDataProviderAPPaymentGroupFactory
	{
		public static IEnumerable<IAPPaymentGroup> GetAPPaymentGroups(CusStatementHeader statement)
		{
			if (statement.IsMonthlyStatement && USCustomsDataRegistry.Instance.MonthlyStatementPaymentPosting.GetFallBackValueAtAllLevels(statement.Company.PK.ToGuid(), Guid.Empty, Guid.Empty) == MonthlyStatementPaymentPostingOptionList.Codes._1)
			{
				foreach (CusStatementHeader dailyStatement in statement.DailyStatements)
				{
					if (dailyStatement.B2_StatementAmount > 0 && dailyStatement.GetStatementLinesToAutorate().Any())
					{
						yield return new AccInvoiceDataProviderAPPaymentGroup(dailyStatement);
					}
				}
			}
			else
			{
				yield return new AccInvoiceDataProviderAPPaymentGroup(statement);
			}
		}
	}

	class AccInvoiceDataProviderAPPaymentGroup : IAPPaymentGroup
	{
		public AccInvoiceDataProviderAPPaymentGroup(CusStatementHeader statement)
		{
			this.statement = statement;
		}

		readonly CusStatementHeader statement;

		public ZString APPaymentNumber
		{
			get { return statement.B2_StatementNumber; }
		}

		public IEnumerable<IAccInvoiceDataProvider> InvoiceDataProviders
		{
			get { return CusStatementHeaderIAccIntegrationDataProviderBase.GetAllLines(statement); }
		}
	}
}
