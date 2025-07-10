using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class DeferredSubmission : NonPersistentBusinessObject
	{
		public DeferredSubmission(DeferredSubmissionHelper helper)
			 : base(helper.Factory)
		{
			this.helper = helper;
			declaration = helper.Declaration;
			AgentPK = declaration.JE_OH_AgentOverride;
		}

		readonly DeferredSubmissionHelper helper;
		readonly JobDeclaration declaration;
		internal ZGuid AgentPK;

		public static class Schema
		{
			public const string DateOfArrival = "DateOfArrival";
			public const string DeferredAccount = "DeferredAccount";
			public const string PaymentMethod = "PaymentMethod";
			public const string SubmissionDate = "SubmissionDate";
			public const string IsOverwritten = "IsOverwritten";
			public const int PaymentMethodMaxLength = 1;
		}

		#region DateOfArrival

		public ZDateTime DateOfArrival => declaration.JE_DateOfArrival;
		public ZPropertyInfo DateOfArrivalInfo => GetZPropertyInfo(Schema.DateOfArrival);

		#endregion

		#region DeferredAccount

		[List(nameof(DeferredAccountsList))]
		[ReadOnlyMember(nameof(DeferredAccountReadOnly))]
		[MaxLength(FinancialAccountNumberPortMap.Schema.FinancialAccountNumberMaxLength)]
		[BusinessObjectTestExclude]
		public ZString DeferredAccount
		{
			get
			{
				var account = FANPortMaps.FirstOrDefault(acc => acc.OrganizationPK == AgentPK);
				return account?.FinancialAccountNumber ?? ZString.Empty;
			}
			set
			{
				var account = FANPortMaps.FirstOrDefault(acc => acc.FinancialAccountNumber == value);
				AgentPK = account?.OrganizationPK ?? ZGuid.Empty;
				Validation.ValidateDeferredAccount();
				DeferredAccountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeferredAccountInfo => GetZPropertyInfo(Schema.DeferredAccount);

		bool DeferredAccountReadOnly => !IsOverwritten;

		public virtual bool AgentChanged { get; set; }

		public CodeDescriptionPairList DeferredAccountsList
		{
			get
			{
				if (fDeferredAccountsList == null)
				{
					fDeferredAccountsList = new CodeDescriptionPairList();
					foreach (var acc in FANPortMaps)
					{
						var org = acc.Organization;
						if (org != null)
						{
							var orgCode = org.OH_Code.PadRight(OrgHeader.Schema.OH_CodeMaxLength);
							var startDay = acc.Cash ? "" : string.Format(CultureInfo.InvariantCulture, "Start Day:{0,2}  ", acc.AccountStartDay);
							var cash = acc.Cash ? "Cash" : "";
							var impCash = acc.ImporterPays && acc.Cash ? " + " : "";
							var importerPays = acc.ImporterPays ? "Imp. Pays" : "";

							var accountDescription = ZString.Format("{0}  {1}{2}{3}{4}", orgCode, startDay, cash, impCash, importerPays);
							fDeferredAccountsList.AddPair(acc.FinancialAccountNumber, accountDescription);
						}
					}
				}
				return fDeferredAccountsList;
			}
		}
		CodeDescriptionPairList fDeferredAccountsList;

		IEnumerable<FinancialAccountNumberPortMap> FANPortMaps => PaymentMethod == PaymentMethodCodeList.Codes.Cash ? helper.CashFANPortMaps : helper.DeferrableFANPortMaps;

		void RefreshDeferredAccount()
		{
			AgentChanged = false;
			fDeferredAccountsList = null;
			if (DeferredAccount.IsEmpty)
			{
				AgentPK = ZGuid.Empty;
			}
			DeferredAccountInfo.RefreshBinding();
		}

		#endregion

		#region Payment Method

		[List(nameof(PaymentMethodsList))]
		[ReadOnlyMember(nameof(PaymentMethodReadOnly))]
		[MaxLength(Schema.PaymentMethodMaxLength)]
		[BusinessObjectTestExclude]
		public ZString PaymentMethod
		{
			get
			{
				return fPaymentMethod.IsEmpty ? helper.FirstDutiableEntryHeader?.CH_PaymentMethod ?? ZString.Empty : fPaymentMethod;
			}
			set
			{
				var oldValue = PaymentMethod;
				fPaymentMethod = value;
				if (PaymentMethod != oldValue)
				{
					PaymentMethodInfo.RefreshBinding();
					SubmissionDateInfo.RefreshBinding();
					RefreshDeferredAccount();
				}
			}
		}
		ZString fPaymentMethod;

		public ZPropertyInfo PaymentMethodInfo => GetZPropertyInfo(Schema.PaymentMethod);

		internal bool PaymentMethodReadOnly => !(IsOverwritten && helper.IsDeclarationDeferrable);

		public CodeDescriptionPairList PaymentMethodsList
		{
			get
			{
				return Factory.GetCachedValue("ZA.DeferredSubmission.PaymentMethodsList", () =>
				{
					var methods = new CodeDescriptionPairList();
					methods.AddPair(PaymentMethodCodeList.Codes.Cash, ResString.GetMultilingualString("BF8C767B-C1CA-4062-AD0F-FBA4BE969C3F", PaymentMethodCodeList.Descriptions.Cash));
					methods.AddPair(PaymentMethodCodeList.Codes.Defer, ResString.GetMultilingualString("70081C8F-DF66-4B4D-AFB3-2E7F09F6738D", PaymentMethodCodeList.Descriptions.Defer));
					methods.AddPair(PaymentMethodCodeList.Codes.VATOnly, ResString.GetMultilingualString("64463675-B79C-42DD-A4E1-8542D396D35B", PaymentMethodCodeList.Descriptions.VATOnly));
					return methods;
				});
			}
		}

		#endregion

		#region SubmissionDate

		[ReadOnlyMember(nameof(SubmissionDateReadOnly))]
		[BusinessObjectTestExclude]
		public virtual ZDateTime SubmissionDate
		{
			get
			{
				var submissionDate = IsOverwritten && !fOverriddenSubmissionDate.IsEmpty ? fOverriddenSubmissionDate : helper.DeferralDate;
				return PaymentMethod != PaymentMethodCodeList.Codes.Cash ? submissionDate : ZDateTime.Today;
			}
			set
			{
				fOverriddenSubmissionDate = value;
				Validation.ValidateSubmissionDate();
				SubmissionDateInfo.RefreshBinding();
			}
		}
		ZDateTime fOverriddenSubmissionDate;

		public ZPropertyInfo SubmissionDateInfo => GetZPropertyInfo(Schema.SubmissionDate);

		internal bool SubmissionDateReadOnly => !(IsOverwritten && CanDeferMessages);

		public virtual bool CanDeferMessages => PaymentMethod != PaymentMethodCodeList.Codes.Cash && helper.IsDeferringMessagesEnabled;

		#endregion

		#region IsOverwritten

		[BusinessObjectTestExclude]
		public ZBool IsOverwritten
		{
			get
			{
				return fIsOverwritten;
			}
			set
			{
				var oldValue = IsOverwritten;
				fIsOverwritten = value;
				if (IsOverwritten != oldValue && !IsOverwritten)
				{
					AgentPK = declaration.JE_OH_AgentOverride;
					PaymentMethod = ZString.Empty;
					SubmissionDate = ZDateTime.Empty;
				}

				DeferredAccountInfo.RefreshBinding();
				PaymentMethodInfo.RefreshBinding();
				SubmissionDateInfo.RefreshBinding();
			}
		}
		ZBool fIsOverwritten;

		public ZPropertyInfo IsOverwrittenInfo => GetZPropertyInfo(Schema.IsOverwritten);

		#endregion

		public void UpdateDeclaration()
		{
			if (IsOverwritten)
			{
				var messageParts = new ZStringBuilder();

				if (SubmissionDate != helper.DeferralDate)
				{
					messageParts.Append(ZString.Format("Submission Date changed from {0} to {1}", helper.DeferralDate, SubmissionDate));
				}

				if (declaration.JE_OH_AgentOverride != AgentPK)
				{
					AgentChanged = true;
					var oldAgent = declaration.AgentCode;
					declaration.JE_OH_AgentOverride = AgentPK;
					var newAgent = declaration.AgentCode;
					messageParts.Append(ZString.Format("Agent changed from {0} to {1}", oldAgent, newAgent));
				}

				var oldPaymentMethod = helper.FirstDutiableEntryHeader?.CH_PaymentMethod ?? ZString.Empty;
				var paymentMethodChanged = false;
				foreach (var entry in helper.DutiableEntryHeaders)
				{
					if (entry.CH_PaymentMethod != PaymentMethod)
					{
						paymentMethodChanged = true;
						entry.CH_PaymentMethod = PaymentMethod;
					}
				}
				if (paymentMethodChanged)
				{
					messageParts.Append(ZString.Format("Payment changed from {0} to {1}", oldPaymentMethod, PaymentMethod));
				}

				if (!messageParts.IsEmpty)
				{
					var message = "Default Deferring Options were overwritten. " + messageParts.ToStringWithDelimiterBetweenAppends(", ");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					declaration.Logs.AddNew(AutoEvents.EditedARecord, message);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}

				declaration.Factory.Save();
			}
		}

		public DeferredSubmissionValidation Validation => new DeferredSubmissionValidation(this);
	}
}
