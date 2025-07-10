using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Provides configuration and information for Job Invoicing purposes based on the individual consumer.
	/// </summary>
	[Immutable]
	[ImmutableObject(true)]
	public abstract class JobInvoicingConsumerType : CodeDescriptionPair
	{
		protected JobInvoicingConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		/// <summary>
		/// Determines whether this type is available in user visible lists. Default false.
		/// </summary>
		public virtual bool ExcludeFromClientVisibleOption { get { return false; } }

		/// <summary>
		/// Determines whether charges can be posted to the Miscellaneous department. Default true.
		/// </summary>
		public virtual bool ValidateJobForMiscellaneousDepartment { get { return true; } }

		/// <summary>
		/// The default value for whether or not a job should be rated
		/// </summary>
		public virtual bool ShouldExcludeFromPeriodicBillingByDefault => false;

		/// <summary>
		/// The controller ID used to show the parent job information
		/// </summary>
		public abstract ControllerID ControllerID { get; }

		/// <summary>
		/// The business object type of the host
		/// </summary>
		public abstract Type BizoType { get; }

		/// <summary>
		/// Determines whether WIPs should be created based on the Invoice Type and Host information provided.
		/// Default true.
		/// </summary>
		public virtual bool ShouldCreateWIPs(IJobInvoicingPlugIn host, string invoiceType) { return true; }

		/// <summary>
		/// Determines whether Accruals should be created based on the Invoice Type and Host information provided.
		/// Default true.
		/// </summary>
		public virtual bool ShouldCreateAccruals(IJobInvoicingPlugIn host, string invoiceType) { return true; }

		public virtual bool ShouldCreateCostJRJ(IJobInvoicingPlugIn host) { return true; }

		public virtual bool ShouldCreateSellJRJ(IJobInvoicingPlugIn host) { return true; }

		/// <summary>
		/// Determines whether the ClientContractNumber is visible on the control. Default is false.
		/// </summary>
		public virtual bool ShouldDisplayClientContractNumber(IJobInvoicingPlugIn host) { return false; }

		/// <summary>
		/// Determines whether charges are actually posted (ie ACTUALS, not Accruals) based on the Invoice Type and Host information provided.
		/// Default to true unless InvoiceType is "Do Not Post".
		/// </summary>
		public virtual PostChargesAllowedInformation ShouldPostCharges(IJobInvoicingPlugIn host, string invoiceType, bool isForAPLine)
		{
			if (!isForAPLine && invoiceType == InvoiceTypesList.Codes.DoNotPost)
			{
				return new PostChargesAllowedInformation(false, Res.GetString("769a5a5d-1ad3-4e8b-8de9-8425e2b1eb01", "The Invoice Type for charges is set to 'Do Not Post'."));
			}

			return new PostChargesAllowedInformation();
		}

		/// <summary>
		/// A list of available invoice types. A default list is provided.
		/// </summary>
		public virtual CodeDescriptionPairList InvoiceTypeList { get { return new InvoiceTypesList(); } }

		/// <summary>
		/// Calculates the invoice type based on the Host information and Charge Code information provided.
		/// Null is returned if the default behaviour should be used.
		/// </summary>
		public virtual string GetOverriddenInvoiceType(IJobInvoicingPlugIn host, AccChargeCode chargeCode, RefCurrency chargeCurrency, ZString postingStyle, ZString currentInvoiceType) { return null; }

		/// <summary>
		/// Calculates the invoice type if the Debtor is not set. Based on the current Invoice Type information provided.
		/// Empty string is returned if the default behaviour should be used.
		/// </summary>
		public virtual ZString GetInvoiceTypeWithNoDebtor(ZString currentInvoiceType) { return ZString.Empty; }

		/// <summary>
		/// Specifies whether the Overseas Agent applies to this job type.
		/// </summary>
		public virtual bool OverseasAgentApplicable { get { return false; } }

		public virtual string OverseasAgentText { get { return Res.GetString("MasterFiles|JobInvoicingConsumerType|OverseasAgent", "Overseas Agent"); } }

		public virtual string LocalClientText { get { return Res.GetString("MasterFiles|JobInvoicingConsumerType|LocalClient", "Local Client"); } }

		public virtual string PrepaidBillToPartyText { get { return Res.GetString("MasterFiles|JobInvoicingConsumerType|PrepaidBillToParty", "Prepaid Bill-To Party"); } }

		public virtual string CollectBillToPartyText { get { return Res.GetString("MasterFiles|JobInvoicingConsumerType|CollectBillToParty", "Collect Bill-To Party"); } }

		/// <summary>
		/// Specifies whether Invoice Printing is applicable to this job type.
		/// </summary>
		//public virtual bool InvoicingPrintingApplicable { get { return true; } }
		public virtual bool InvoicingPrintingApplicable(IJobInvoicingPlugIn host) { return true; }

		/// <summary>
		/// Specifies whether Credit Status is applicable to this job type.
		/// </summary>
		//public virtual bool CreditStatusApplicable { get { return true; } }
		public virtual bool CreditStatusApplicable(IJobInvoicingPlugIn host) { return true; }

		/// <summary>
		/// Specifies whether Profit/Loss is applicable to this job type.
		/// </summary>
		//public virtual bool ProfitLossApplicable { get { return true; } }
		public virtual bool ProfitLossApplicable(IJobInvoicingPlugIn host) { return true; }

		/// <summary>
		/// Specifies whether revenue can be posted
		/// </summary>
		public virtual bool AllowRevenuePosting(IJobInvoicingPlugIn host) { return true; }

		/// <summary>
		/// Specifies whether cost can be posted
		/// </summary>
		public virtual bool AllowCostPosting(IJobInvoicingPlugIn host) { return true; }

		/// <summary>
		/// Is the job active for billing purposes.
		/// </summary>
		public virtual bool IsActive { get { return true; } }

		public static class ChargeBranchDefaultingRulesBase
		{
			public static readonly CodeDescriptionPair SpecificBranchAlways = new CodeDescriptionPair(Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways, ResString.GetMultilingualString("c525b637-e10f-4653-9c34-a200df466964", "Specific Branch Always"));
		}

		public static class ChargeCreditorDefaultingRulesBase
		{
			public static readonly CodeDescriptionPair SpecificCreditorAlways = new CodeDescriptionPair(Constants.ChargeCreditorDefaultingRule.DefaultingRule, ResString.GetMultilingualString("96118482-eba5-4ce8-b27c-be4d5fff1b4a", "Specific Creditor/Role Always"));
		}

		public virtual bool IsTransportModeSupported
		{
			get { return false; }
		}

		public virtual bool IsDirectionSupported
		{
			get { return false; }
		}

		/// <summary>
		/// Distance Calculation Security Checkpoint
		/// </summary>
		public abstract SecurityCheckpoint DistanceCalculationCheckpoint { get; }

		public virtual SecurityCheckpoint JobInvoicingCheckPoint => Env.Security.None;

		public virtual string DisplayName(IJobInvoicingPlugIn host)
		{
			return Res.GetString("e2203a14-87bd-416d-8be8-b16ba980d951", "Billing");
		}

		public virtual ResourceString MenuName(IJobInvoicingPlugIn host)
		{
			return null;
		}

		public virtual string RevenueChargeDescription(IJobInvoicingPlugIn host)
		{
			return ResString.GetMultilingualString("Accounting.RevenueChargeDescription", "Revenue");
		}

		public virtual bool SupportsWiseRates
		{
			get { return false; }
		}

		public virtual bool SupportGlowBilling => false;
	}
}
