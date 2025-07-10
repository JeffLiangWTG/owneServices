using System.Collections.Generic;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentRerouteResponse)]
	public class ABIStatementACHPaymentRerouteProcessor : RerouteProcessor<DSTQR, DSTQX>
	{
		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			return USCustomsDataRegistry.Instance.DailyStatementsMessagesGroup;
		}

		protected override IReadOnlyList<string> SendMessageDataHeading
		{
			get
			{
				List<string> result = new List<string>(base.SendMessageDataHeading);
				result.Add("ACH Payment");
				result.Add("Periodic Payment");
				return result.ToArray();
			}
		}

		protected override object[] WriteQRBlock(DSTQR qr)
		{
			List<object> result = new List<object>(base.WriteQRBlock(qr));
			result.Add(qr.ACHPaymentRequest);
			result.Add(qr.PeriodicStatementPaymentAuthorizationRequest);
			return result.ToArray();
		}

		internal const string ABIStatementACHPaymentRerouteSubject = "Reroute of statement and/or ACH payment response";
		protected override string RerouteSubject
		{
			get { return ABIStatementACHPaymentRerouteSubject; }
		}
	}
}
