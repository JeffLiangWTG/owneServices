using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class StatementPaymentAction : NonPersistentBusinessObject, IPaymentAuthorisation, IObsoleteValidation
	{
		public StatementPaymentAction(CusStatementHeader statementHeader)
			: base(statementHeader.Factory)
		{
			this.statementHeader = statementHeader;

			using (SuspendSettingHasChanges())
			{
				ZString defaultACHPaymentType = statementHeader.GetACHPayMethodToDefault();

				if (defaultACHPaymentType.IsEmpty)
				{
					defaultACHPaymentType = ACHPaymentTypeList.Codes.ACHDebit;
				}

				if (defaultACHPaymentType != ACHPaymentTypeList.Codes.ImporterCheck)
				{
					ACHPaymentType = defaultACHPaymentType;
				}

				if (ACHPaymentType == ACHPaymentTypeList.Codes.ACHDebit)
				{
					PayerUnitNo = statementHeader.GetPayerUnitNoToDefault().SubstringSafe(0, 6);
				}
			}
		}

		public CusStatementHeader StatementHeader
		{
			get { return statementHeader; }
		}
		readonly CusStatementHeader statementHeader;

		public ZString StatementNumber
		{
			get
			{
				if (statementNumber.IsEmpty)
				{
					statementNumber = StatementHeader.B2_StatementNumber;
				}
				return statementNumber;
			}
		}
		ZString statementNumber;

		public ZPropertyInfo StatementNumberInfo
		{
			get { return GetZPropertyInfo(nameof(StatementNumber)); }
		}

		[MaxLength(6)]
		public ZString PayerUnitNo
		{
			get { return fPayerUnitNo; }
			set
			{
				SetNonPersistentPropertyValue(PayerUnitNoInfo, ref fPayerUnitNo, value);
				ValidatePayerUnitNo();

				ValidatePaymentParty();
				PaymentPartyInfo.RefreshBinding();
			}
		}
		ZString fPayerUnitNo;

		public ZPropertyInfo PayerUnitNoInfo
		{
			get { return GetZPropertyInfo(nameof(PayerUnitNo)); }
		}

		public void ValidatePayerUnitNo()
		{
			if (!IsValidationSuspended)
			{
				PayerUnitNoInfo.ClearAllNotifications();

				if (ACHPaymentType == ACHPaymentTypeList.Codes.ACHDebit)
				{
					if (PayerUnitNo.IsEmpty)
					{
						PayerUnitNoInfo.AddMessageError(ValidationConstants.Statement.PayerUnitNoIsMandatory);
					}
					else if (PayerUnitNo.Length != 6)
					{
						PayerUnitNoInfo.AddMessageError(ValidationConstants.Statement.PayerUnitNoLength);
					}
				}
				else if (!ACHPaymentType.IsEmpty && !PayerUnitNo.IsEmpty)
				{
					PayerUnitNoInfo.AddMessageError(ValidationConstants.Statement.PayerUnitNoIsNotRequired);
				}

				ValidateACHPaymentType();
			}
		}

		[List(nameof(ACHTypeList))]
		[MaxLength(3)]
		public ZString ACHPaymentType
		{
			get { return fACHPaymentType; }
			set
			{
				SetNonPersistentPropertyValue(ACHPaymentTypeInfo, ref fACHPaymentType, value);
				ValidateACHPaymentType();
			}
		}
		ZString fACHPaymentType;

		public ZPropertyInfo ACHPaymentTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ACHPaymentType)); }
		}

		public void ValidateACHPaymentType()
		{
			if (!IsValidationSuspended)
			{
				ACHPaymentTypeInfo.ClearAllNotifications();

				ListValidation.MessageErrorIfInvalidCodeOrEmpty(ACHPaymentTypeInfo, ACHTypeList);

				var paymentType = statementHeader.GetACHPayMethodForPaymentParty(PaymentParty);
				if (paymentType.IsEmpty)
				{
					if (ACHPaymentType == ACHPaymentTypeList.Codes.ACHCredit)
					{
						ACHPaymentTypeInfo.AddMessageError(string.Format(ACHPaymentTypeShouldBeDebitWhenPartyIsNotConfigured, paymentType));
					}
				}
				else if (paymentType != ACHPaymentType)
				{
					ACHPaymentTypeInfo.AddMessageError(string.Format(PaymentTypeDifferentToPaymentPartyOrgConfiguration, paymentType));
				}

				if (PaymentTypeList.IsDailyPayment(statementHeader.B2_PaymentType) && ACHPaymentType == ACHPaymentTypeList.Codes.ACHCredit)
				{
					ACHPaymentTypeInfo.AddMessageError(ValidationConstants.Statement.PayByACHCredit);
				}
			}
		}

		internal const string ACHPaymentTypeShouldBeDebitWhenPartyIsNotConfigured = "Payment Party is configured with no ACH pay method and ACH Credit is not allowed.";
		internal const string PaymentTypeDifferentToPaymentPartyOrgConfiguration = "Payment Party is configured with a different ACH payment type, {0}.";

		public CodeDescriptionPairList ACHTypeList
		{
			get
			{
				return Factory.GetCachedValue("ACHTypeList on paymentAction",
					delegate
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList();
						result.Add(new CodeDescriptionPair(ACHPaymentTypeList.Codes.ACHCredit, ACHPaymentTypeList.Descriptions.ACHCredit));
						result.Add(new CodeDescriptionPair(ACHPaymentTypeList.Codes.ACHDebit, ACHPaymentTypeList.Descriptions.ACHDebit));

						return result;
					});
			}
		}

		public bool IsACHCredit
		{
			get { return ACHPaymentType == ACHPaymentTypeList.Codes.ACHCredit; }
		}

		[List(nameof(PayPartyList))]
		[MaxLength(3)]
		public ZString PaymentParty
		{
			get
			{
				return new PaymentPartyCalculator().CalculatePaymentParty(statementHeader.Company, PayerUnitNo, statementHeader.B2_PaymentType, statementHeader.B2_BranchDesignation);
			}
		}

		public ZPropertyInfo PaymentPartyInfo
		{
			get { return GetZPropertyInfo(nameof(PaymentParty)); }
		}

		public void ValidatePaymentParty()
		{
			if (!IsValidationSuspended)
			{
				PaymentPartyInfo.ClearAllNotifications();

				if (PaymentParty == PaymentPartyList.Codes.Importer && PaymentTypeList.IsPaidByBroker(statementHeader.B2_PaymentType))
				{
					if (ACHPaymentType != ACHPaymentTypeList.Codes.ImporterCheck)
					{
						PaymentPartyInfo.AddWarning(string.Format(PaymentPartyCalculatedAsImporter, statementHeader.B2_PaymentType, PayerUnitNo));
					}
				}
			}
		}

		public const string PaymentPartyCalculatedAsImporter =
@"The statement's payment type is '{0}', but the payment party is calculated as 'IMP'(importer pays). 
System calculates the payment party using Payer's Unit Number or Payer's Unit Number and Client Branch Designation, {1} and compares it with Payer's Unit Numbers 
entered against the following locations. If not matched, then it is calculated as 'IMP'. Please check if this
Payer's Unit Number should be entered at the following location:

Registry > Customs > United States of America > Import > ABI > Statements > Broker's Bank Accounts.
";

		public CodeDescriptionPairList PayPartyList
		{
			get { return Factory.GetCachedValue<PaymentPartyList>(); }
		}

		public ZDecimal TotalAmountPayable
		{
			get { return StatementHeader.TotalAmountsPayable; }
		}

		public ZPropertyInfo TotalAmountPayableInfo
		{
			get { return GetZPropertyInfo(nameof(TotalAmountPayable)); }
		}

		public void ValidateTotalAmountPayable()
		{
			if (StatementHeader.B2_PaymentParty == PaymentPartyList.Codes.Importer && PaymentParty == PaymentPartyList.Codes.Broker)
			{
				if (StatementHeader.StatementLines.Cast<CusStatementLine>().Any(line => line.Declaration != null && line.B3_CustomsFeesTotal != line.ARTotalAmount))
				{
					TotalAmountPayableInfo.AddMessageError(APAmountDontMatch);
				}
				if (StatementHeader.StatementLines.Cast<CusStatementLine>().Any(line => line.Declaration != null && line.B3_CustomsFeesTotal == line.ARTotalAmount && line.ARUnPostedAmount > 0))
				{
					TotalAmountPayableInfo.AddWarning(SomePostedArAmounts);
				}
			}
		}
		internal const string APAmountDontMatch = "The AR amounts of some entries do not match Customs Fee Total of this statement.";
		internal const string SomePostedArAmounts = "There are unposted AR amounts for some entries.";

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePayerUnitNo();
			ValidateACHPaymentType();
			ValidatePaymentParty();
			ValidateTotalAmountPayable();
			StatementHeader.StatementLines.Cast<CusStatementLine>().ForEach(line => line.Validation.ValidateAll());
		}

		#region IPaymentAuthorisation Members

		ZString IPaymentAuthorisation.PayersUnitNumber
		{
			get { return !IsACHCredit ? PayerUnitNo : ZString.Empty; }
			set { throw new InvalidOperationException("This should not be triggered for this class"); }
		}

		ZString IPaymentAuthorisation.PaymentType
		{
			get { throw new InvalidOperationException("This should not be triggered for this class"); }
			set { throw new InvalidOperationException("This should not be triggered for this class"); }
		}

		ZString IPaymentAuthorisation.StatementFiler
		{
			get { return StatementHeader.B2_EntryFilerCode; }
			set { throw new InvalidOperationException("This should not be triggered for this class"); }
		}

		ZString IPaymentAuthorisation.StatementBillNumber
		{
			get { return StatementHeader.B2_StatementNumber; }
			set { throw new InvalidOperationException("This should not be triggered for this class"); }
		}

		ZDecimal IPaymentAuthorisation.PaymentAmount
		{
			get { return StatementHeader.TotalAmountsPayable; }
			set { throw new InvalidOperationException("This should not be triggered for this class"); }
		}

		//for some reason customs expect this to be the preparer port code....ie RLF processing port 1704 and entry port 1704 and
		//preparer district port is 2904, then 2904 should go in position 4-7 of the B Block
		ZString IPaymentAuthorisation.ProcessingPortCode
		{
			get
			{
				ZString preparerDistrictPort = GetPreparerDistrictPort();
				return preparerDistrictPort.IsEmpty ? StatementHeader.B2_ProcessPort : preparerDistrictPort;
			}
		}

		public ZString GetPreparerDistrictPort()
		{
			ZString result = StatementHeader.B2_PreparerDistrictPort;

			if (result.IsEmpty)
			{
				MQEDIMessage inMessage = (MQEDIMessage)StatementHeader.Messages.GetLastMessage(EDIMessage.ApplicationCodes.USCustomsImport, ApplicationIdentifierCodeList.Codes.DailyStatement, EDIMessage.Direction.Receive);

				if (inMessage != null && inMessage.MessageBlock != null)
				{
					result = ((IABIControlMessageBlockB)inMessage.MessageBlock.B).PreparerDistrictPort;
				}
			}

			return result;
		}

		ZString IPaymentAuthorisation.ClientBranchDesignation
		{
			get { return StatementHeader.B2_BranchDesignation; }
		}

		ZString IPaymentAuthorisation.NegationCode
		{
			get => isNegated ? YesNoDefaultList.Codes.Yes : " ";
			set { throw new InvalidOperationException(); }
		}
		bool isNegated;

		public static StatementPaymentAction NewForNegation(CusStatementHeader statementHeader)
		{
			var action = new StatementPaymentAction(statementHeader);
			action.isNegated = true;
			action.negationDate = ZDate.Today;
			return action;
		}

		ZDate IPaymentAuthorisation.NegationDate
		{
			get { return ((IPaymentAuthorisation)this).NegationCode == YesNoDefaultList.Codes.Yes ? negationDate : ZDate.Empty; }
			set { negationDate = value; }
		}
		ZDate negationDate;

		#endregion

	}
}
