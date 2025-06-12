using System;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Operations;

namespace CargoWise.eHub.Core.PipelineComponents.Mutex
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[System.Runtime.InteropServices.Guid("da18ac74-b220-436a-8d1f-9690a1df09ba")]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	public class MutexZombieKiller : ComponentBase, IComponent
	{
		protected override Guid ClassID
		{
			get { return new Guid("da18ac74-b220-436a-8d1f-9690a1df09ba"); }
		}

		protected override string DisplayName
		{
			get { return "Mutex Orchestration Zombie Killer"; }
		}

		public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
		{
			var logger = GetLogger(pInMsg);
			LoggerHelpers.LogComponentStart(logger, this);
			try
			{
				new PipelineHelpers().CreateSeekableMessageStream(pContext, pInMsg, logger);
				var xDoc = XDocument.Load(pInMsg.BodyPart.GetOriginalDataStream());
				string instanceIdVal = xDoc.XPathSelectElement("/*[local-name()='TypedPollingResultSet0']/*[local-name()='uidInstanceID']").Value;
				pInMsg.BodyPart.GetOriginalDataStream().Position = 0;

				logger.DebugFormat("Terminating Zombie Instance ID '{0}'", instanceIdVal);

				Guid instanceId = new Guid(instanceIdVal);
				var result = TerminateInstance(instanceId);

				switch (result)
				{
					case CompletionStatus.Failed:
						logger.ErrorFormat("Failed to terminate Mutex zombie instance ID '{0}'", instanceId);
						break;
					case CompletionStatus.Succeeded:
						logger.DebugFormat("Instance terminated successfully");
						break;
					default:
						logger.WarnFormat("'{0}' result received when terminating Mutex zombie instance ID '{1}'", result, instanceId);
						break;
				}
			}
			catch (Exception ex)
			{
				logger.ErrorFormat("Error terminating Mutex orchestration zombie instance", ex);
			}

			LoggerHelpers.LogComponentEnd(logger, this);
			return pInMsg;
		}

		internal static Func<Guid, CompletionStatus> TerminateInstance = (Guid instanceId) =>
		{
			using (var ops = new BizTalkOperations())
				return ops.TerminateInstance(instanceId);
		};

		internal static Func<IBaseMessage, ILog> GetLogger = (IBaseMessage pInMsg) => LoggerHelpers.GetPipelineLogger(pInMsg);
	}
}

