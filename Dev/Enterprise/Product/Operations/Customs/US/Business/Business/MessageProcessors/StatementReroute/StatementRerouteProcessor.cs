using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatementRerouteResponse)]
	public class StatementRerouteProcessor : RerouteProcessor<PMSQR, QXCommon>
	{
		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			return IsPeriodicMonthly ? USCustomsDataRegistry.Instance.PeriodicMonthlyStatementsMessagesGroup : USCustomsDataRegistry.Instance.DailyStatementsMessagesGroup;
		}

		protected override string RerouteSubject
		{
			get { return IsPeriodicMonthly ? PeriodicMonthlyStatementRerouteSubject : DailyStatementRerouteSubject; }
		}

		internal const string PeriodicMonthlyStatementRerouteSubject = "Reroute of Periodic Monthly Statement response";
		internal const string DailyStatementRerouteSubject = "Reroute of Daily Statement response";

		protected override ZString GetErrorDescription(ZString code)
		{
			var result = base.GetErrorDescription(code);
			if (result.IsEmpty)
			{
				var firstNotificationBlock = messageBlocks.OfType<IStatementRerouteResponse>().FirstOrDefault();
				if (firstNotificationBlock != null)
				{
					if (Factory.GetCachedValue<ACENotificationErrorsList>().ContainsCode(firstNotificationBlock.ErrorCode))
					{
						result = Factory.GetCachedValue<ACENotificationErrorsList>().GetDescriptionFromCode(code);
					}
					else
					{
						result = Factory.GetCachedValue<NotificationErrorsList>().GetDescriptionFromCode(code);
					}
				}
			}
			return result;
		}
	}
}
