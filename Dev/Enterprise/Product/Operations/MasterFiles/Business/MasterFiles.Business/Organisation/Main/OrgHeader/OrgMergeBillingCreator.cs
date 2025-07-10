using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Integration.Licensing;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgMergeBillingCreator
	{
		public static void CreateBillingTransaction(DbConnection dbConnection, string action, string sourceWindow,
			string duplicateConfidence, params string[] references)
		{
			Argument.NotNull(action, nameof(action));
			Argument.NotNull(sourceWindow, nameof(sourceWindow));
			Argument.NotNull(duplicateConfidence, nameof(duplicateConfidence));
			var actionSourceInfo = BuildActionSourceInfo(action, sourceWindow, duplicateConfidence);
			CreateBillingTransactionCore(dbConnection, actionSourceInfo, references);
		}

		public static void CreateBillingTransaction(DbConnection dbConnection, string action, string sourceWindow, params string[] references)
		{
			Argument.NotNull(action, nameof(action));
			Argument.NotNull(sourceWindow, nameof(sourceWindow));
			var actionSourceInfo = BuildActionSourceInfo(action, sourceWindow);
			CreateBillingTransactionCore(dbConnection, actionSourceInfo, references);
		}

		static void CreateBillingTransactionCore(DbConnection dbConnection, string actionSourceInfo, params string[] references)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var billingTransaction = new BillingTransaction();
			billingTransaction.Category = OrgMergeBillingConstants.Category;
			billingTransaction.PriceItemCode = OrgMergeBillingConstants.PriceCode;
			billingTransaction.BillableCount = 1;
			billingTransaction.ReportingSource = OrgMergeBillingConstants.ReportingSource;
			billingTransaction.ServiceOccuredUTC = ZDateTime.UtcNow.ToDateTime();
			billingTransaction.ClientID = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			billingTransaction.Branch = GlbBranch.CurrentBranch.GB_Code;
			billingTransaction.ClientNumber = $"{registrationKey.SystemId}.{GlbCompany.CurrentCompany.GC_Code}";
			billingTransaction.ClientStaffCode = GlbStaff.CurrentUser.GS_Code;
			billingTransaction.Version = 1;
			billingTransaction.Reference1 = actionSourceInfo;
			if (references != null)
			{
				billingTransaction.Reference2 = GetSafeValue(references, 0);
				billingTransaction.Reference3 = GetSafeValue(references, 1);
				billingTransaction.Reference4 = GetSafeValue(references, 2);
				billingTransaction.Reference5 = GetSafeValue(references, 3);
			}

			new BillingManager().AddTransactions(new[] { billingTransaction }, dbConnection);
		}

		static string BuildActionSourceInfo(string actionCode, string sourceCode, string duplicateConfidence = null)
		{
			var keyValueStrings = new List<string>();
			keyValueStrings.Add($"{OrgMergeBillingConstants.BillingAction}={actionCode}");
			keyValueStrings.Add($"{OrgMergeBillingConstants.ActionSource}={sourceCode}");

			var actionsContainsConfidence = new[] { OrgMergeBillingConstants.Action.IgnoreForEveryone, OrgMergeBillingConstants.Action.MergeSuccess, OrgMergeBillingConstants.Action.MergeFailure, OrgMergeBillingConstants.Action.MergeCancel };
			if (actionsContainsConfidence.Contains(actionCode) && !string.IsNullOrEmpty(duplicateConfidence))
			{
				keyValueStrings.Add($"{OrgMergeBillingConstants.Confidence}={duplicateConfidence}");
			}

			return string.Join("|", keyValueStrings);
		}

		static string GetSafeValue(string[] array, int index) => array.Length > index ? array[index] : null;
	}
}
