using System;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.Business
{
	static class ACHDetailsDefaulter
	{
		public static ZString GetPayerUnitNoToDefault(this CusStatementHeader statementHeader)
		{
			var result = statementHeader.B2_AccountNo;

			if (result.IsEmpty)
			{
				if (statementHeader.IsPaymentPartyBroker)
				{
					var brokerAccounts = USCustomsDataRegistry.Instance.BrokersAccounts.GetFallBackValueAtAllLevels(statementHeader.B2_GC.ToGuid(), Guid.Empty, Guid.Empty);

					if (brokerAccounts != null)
					{
						result = brokerAccounts.GetPayerUnitNoByBranchDesignation(statementHeader.B2_BranchDesignation);
					}
				}
				else
				{
					var importer = statementHeader.Importer;
					if (importer != null)
					{
						result = OrgHeaderWrapper.New(importer).ZO_AccountNo;
					}
				}
			}

			return result;
		}

		public static ZString GetACHPayMethodToDefault(this CusStatementHeader statementHeader)
		{
			return statementHeader.GetACHPayMethodForPaymentParty(statementHeader.B2_PaymentParty);
		}

		public static ZString GetACHPayMethodForPaymentParty(this CusStatementHeader statementHeader, ZString paymentParty)
		{
			var result = ACHPaymentTypeList.Codes.ACHDebit;

			var organisation = GetOrganizationToPay(statementHeader, paymentParty);
			if (organisation != null)
			{
				result = OrgHeaderWrapper.New(organisation).ZO_PayMethod;
			}

			return result;
		}

		static OrgHeader GetOrganizationToPay(CusStatementHeader statementHeader, ZString paymentParty)
		{
			OrgHeader organisation = null;

			if (paymentParty == PaymentPartyList.Codes.Broker)
			{
				var creditorPK = RatingDataRegistry.Instance.CustomsDisbursementCreditor.GetFallBackValueAtAllLevels(statementHeader.B2_GC.ToGuid(), Guid.Empty, Guid.Empty);

				organisation = statementHeader.Factory.Load<OrgHeader>(creditorPK);
			}
			else
			{
				organisation = statementHeader.Importer;
			}

			return organisation;
		}
	}
}
