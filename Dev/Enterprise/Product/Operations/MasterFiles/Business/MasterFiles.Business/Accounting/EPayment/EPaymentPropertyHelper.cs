using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class EPaymentPropertyHelper
	{
		#region PaymentReference

		public static DefaultEPaymentReference GetDefaultEPaymentReferenceForEPaymentMethod(ZString paymentMethod)
		{
			var providerCode = GetProviderFromPaymentMethod(paymentMethod);
			return AccountingMasterFilesRegistry.Instance.DefaultPaymentReference.Value.Cast<DefaultEPaymentReference>().FirstOrDefault(r => r.ProviderCode == providerCode);
		}

		#endregion

		#region PaymentReasons

		public static CodeDescriptionPairList GetPaymentReasonsForEPaymentMethod(Guid companyPK, ZString paymentMethod)
		{
			var providerCode = GetProviderFromPaymentMethod(paymentMethod);
			return GetPaymentReasonsCore(companyPK, providerCode);
		}

		public static CodeDescriptionPairList GetPaymentReasonsForProvider(Guid companyPK, string providerCode)
		{
			return GetPaymentReasonsCore(companyPK, providerCode);
		}

		public static ZString GetPaymentReasonDescriptionForProvider(Guid companyPk, string providerCode, string reasonCode)
		{
			var reasonsForAllProviders = AccountingMasterFilesRegistry.Instance.PaymentReasons.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty);
			var payReasonForProvider = reasonsForAllProviders.Cast<EPaymentReason>().FirstOrDefault(x => x.ProviderCode == providerCode && x.ReasonCode == reasonCode);
			return payReasonForProvider?.ReasonDescription ?? ZString.Empty;
		}

		static CodeDescriptionPairList GetPaymentReasonsCore(Guid companyPK, string providerCode)
		{
			var reasons = new CodeDescriptionPairList();
			var reasonsForAllProviders = AccountingMasterFilesRegistry.Instance.PaymentReasons.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			var reasonsForProvider = reasonsForAllProviders.Cast<EPaymentReason>().Where(r => r.ProviderCode == providerCode);
			reasonsForProvider.ForEach(r => reasons.AddPair(r.ReasonCode, r.ReasonDescription));
			return reasons;
		}

		#endregion

		#region DefaultPaymentReason

		public static ZString GetDefaultPaymentReasonForEPaymentMethod(Guid companyPK, Guid branchPK, ZString paymentMethod)
		{
			var providerCode = GetProviderFromPaymentMethod(paymentMethod);
			return GetDefaultPaymentReasonCore(companyPK, branchPK, providerCode)?.ReasonCode ?? ZString.Empty;
		}

		public static ZString GetDefaultPaymentReasonDescriptionForProvider(Guid companyPk, Guid branchPk, string providerCode)
		{
			return GetDefaultPaymentReasonCore(companyPk, branchPk, providerCode)?.ReasonDescription ?? ZString.Empty;
		}

		static DefaultEPaymentReason GetDefaultPaymentReasonCore(Guid companyPk, Guid branchPk, string providerCode)
		{
			var defaultReasonsForAllProviders = AccountingMasterFilesRegistry.Instance.DefaultPaymentReason.GetFallBackValueAtAllLevels(companyPk, branchPk, Guid.Empty);
			var payReasonForProvider = defaultReasonsForAllProviders.Cast<DefaultEPaymentReason>().FirstOrDefault(x => x.ProviderCode == providerCode);
			return payReasonForProvider;
		}

		#endregion

		static ZString GetProviderFromPaymentMethod(ZString paymentMethod)
		{
			var provider = ZString.Empty;
			if (paymentMethod == EPaymentMethods.EPaymentViaOFX)
			{
				provider = EPaymentProviderCodes.Codes.OFX;
			}
			return provider;
		}
	}
}
