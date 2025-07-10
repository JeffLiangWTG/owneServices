using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	abstract class ILResetToOriginalMessageCommandBaseTest : ILCustomCommandBaseTest
	{
		public void TestIsEnabled()
		{
			CombineAssertions("When Message Reference Is Empty => !IsEnabled", () =>
			{
				UpdateMessageReference(string.Empty);
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-10));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-20));
				shipment.Factory.Save();

				Assert(!command.IsEnabled);
			});

			CombineAssertions("When the latest event is MessageSent, and Message Reference Is not Empty => IsEnabled", () =>
			{
				UpdateMessageReference("10003");
				shipment.Logs.AddNew(Events.MessageSent, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-1));
				shipment.Logs.AddNew(Events.MessageRejected, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-2));
				shipment.Logs.AddNew(Events.MessageAccepted, GetEventParameters(), ZDateTimeOffset.Now.AddDays(-3));
				shipment.Factory.Save();

				Assert(command.IsEnabled);
			});
		}

		protected override string CommandId => "ResetToOriginal";

		protected override string CommandCaption => "Reset To Original";

		protected string GetLastLog(IStmALogParent logParent)
		{
			return logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent != Events.WorkflowTemplateAppliedCode)
				.OrderBy(l => l.SL_PostedTimeUtc)
				.Select(l => $"{l.SL_SE_NKEvent} {l.SL_Reference}".TrimEnd())
				.Last();
		}

		protected override string GetDataContext() => "ILDeliveryOrder";
	}
}
