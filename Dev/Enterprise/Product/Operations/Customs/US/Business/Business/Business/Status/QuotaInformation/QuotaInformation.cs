using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;

namespace Enterprise.Customs.US.Business
{
	public class QuotaInformation : NonPersistentBusinessObject
	{
		public QuotaInformation()
		{
		}

		public QuotaInformation(AESSE4 e4Block)
		{
			this.e4Block = e4Block;
		}

		readonly AESSE4 e4Block;

		public ZString LineItemIdentifier
		{
			get { return e4Block != null ? e4Block.LineItemIdentifier : ZString.Empty; }
		}

		public ZString QuotaLineStatusCode
		{
			get { return e4Block != null ? e4Block.QuotaLineStatusCode : ZString.Empty; }
		}

		public ZString QuotaLineStatusDescription
		{
			get { return e4Block != null ? QuotaLineStatusCodes.GetDescriptionFromCode(QuotaLineStatusCode) : string.Empty; }
		}

		public ZDecimal ReservedQuotaQuantity
		{
			get { return e4Block != null ? e4Block.ReservedQuotaQuantity : ZDecimal.Zero; }
		}

		public ZString ReservedQuotaQuantityUQ
		{
			get { return e4Block != null ? e4Block.ReservedQuotaUnitOfMeasureCode : ZString.Empty; }
		}

		public ZDecimal RequestedQuotaQuantity
		{
			get { return e4Block != null ? e4Block.RequestedQuotaQuantity : ZDecimal.Zero; }
		}

		public ZString RequestedQuotaQuantityUQ
		{
			get { return e4Block != null ? e4Block.QuotaRequestedUnitOfMeasureCode : ZString.Empty; }
		}

		QuotaLineStatusCodeList QuotaLineStatusCodes
		{
			get
			{
				if (quotaLineStatusCodes == null)
				{
					quotaLineStatusCodes = new QuotaLineStatusCodeList();
				}
				return quotaLineStatusCodes;
			}
		}
		QuotaLineStatusCodeList quotaLineStatusCodes;
	}
}
