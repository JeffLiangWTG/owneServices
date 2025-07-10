using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class ProvisionalPaymentEntryPayInfoCollection : ActiveBusinessObjectCollection<ProvisionalPaymentCusEntryPayInfo>
	{
		public ProvisionalPaymentEntryPayInfoCollection(BusinessObjectFactory factory, IEnumerable<CusEntryHeader> cusEntryHeaders)
			: base(factory, GetQueryForProvisionalPaymentPayInfos(factory, cusEntryHeaders))
		{
		}

		static ZQuery GetQueryForProvisionalPaymentPayInfos(BusinessObjectFactory factory, IEnumerable<CusEntryHeader> cusEntryHeaders)
		{
			var query = GetQueryForProvisionalPaymentPayInfos(factory);
			if (cusEntryHeaders != null)
			{
				query.AddToFilter(CusEntryPayInfoSchema.C9_CH, SQLComparisonOperator.Equal, cusEntryHeaders.Select(x => x.PK));
			}
			return query;
		}

		public static ZQuery GetFetchHintQueryForProvisionalPaymentPayInfos(BusinessObjectFactory factory, CusEntryHeader cusEntryHeader)
		{
			var query = GetQueryForProvisionalPaymentPayInfos(factory);
			query.AddToFilter(CusEntryPayInfoSchema.C9_CH, SQLComparisonOperator.Equal, cusEntryHeader.PK);
			return query;
		}

		static ZQuery GetQueryForProvisionalPaymentPayInfos(BusinessObjectFactory factory)
		{
			var query = new ZQuery();
			query.AddToFilter(CusEntryPayInfoSchema.C9_PaymentReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.AddToFilter(CusEntryPayInfoSchema.C9_PaymentParty, SQLComparisonOperator.Equal, PaymentMethodCodeList.Codes.Cash);
			query.AddToFilter(CusEntryPayInfoSchema.C9_TransactionType, SQLComparisonOperator.Equal, factory.GetAllProvisionalPaymentTypes());
			return query;
		}

		protected override bool AllowNew { get { return false; } }

		public override void Delete(ProvisionalPaymentCusEntryPayInfo businessObject)
		{
		}

		internal ZBool HasPPTypeBeenLiquidated(ZString type, ZShort lineNumber)
		{
			return this.Any(x => x.C9_RemAdvReceived && x.C9_TransactionType == type && x.C9_IncomingPayResponseNo == lineNumber.ToString());
		}
	}
}
