using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class PermitTestDataHelper : Customs.Business.Testing.PermitTestDataHelper
	{
		public PermitTestDataHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CusPermitHeader CreatePermitHeader(ZGuid permitHolder, ZString permitNumber, ZDate startDate, ZDate endDate, ZString qtyValIndicator, ZString type, string subType = "", decimal qtyBalance = 0.0M, decimal valBalance = 0.0M, string uom = "")
		{
			return (CusPermitHeader)base.CreatePermitHeader(Core.Constants.CountryCodes.SouthAfrica, permitHolder, permitNumber, startDate, endDate, qtyValIndicator, type, subType, qtyBalance, valBalance, uom);
		}

		public ZQuery GetPermitLineTransactionQuery(CusEntryHeader entryHeader, EDIMessage message, ZString transactionCategory, ZString transactionType)
		{
			return GetPermitLineTransactionQuery(PermitHelper.GetPermitReferenceForEntry(entryHeader), PermitHelper.GetPermitComment(entryHeader, null), ZAPermitHelper.GetPermitAppIdForMessage(message), transactionCategory, transactionType);
		}

		public BaseCusPermitLineTransaction CreatePermitLineTransaction(CusPermitHeader permit, CusEntryHeader entryHeader, EDIMessage message, ZString transactionCategory, ZString transactionType, ZDecimal value, ZDecimal quantity, string transactionStatus = PermitTransactionStatusList.Codes.Pending)
		{
			return CreatePermitLineTransaction(permit, PermitHelper.GetPermitReferenceForEntry(entryHeader), PermitHelper.GetPermitComment(entryHeader, null), ZAPermitHelper.GetPermitAppIdForMessage(message), transactionCategory, transactionType, value, quantity, transactionStatus, PermitHelper.GetPermitReferenceNumberLineForEntry(entryHeader));
		}
	}
}
