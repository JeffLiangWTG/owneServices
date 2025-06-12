using System;
using System.Collections.Generic;
using CargoWise.eHub.Core.PipelineComponents.Mutex;
using CargoWise.eHub.Core.Pipelines;
using CargoWise.eHub.Core.Schemas;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting;

namespace CargoWise.eHub.Core.Tests.Pipelines
{
	[TestClass]
	public class MutexZombieKillerTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MutexZombieKiller_Pipeline()
		{
			ReceivePipelineWrapper pipeline = PipelineFactory.CreateReceivePipeline(typeof(Core_MutexZombieKiller));
			pipeline.AddDocSpec(typeof(BizTalkMsgBoxDb_MutexZombieKiller));

			string messageText =
@"<TypedPolling xmlns='http://schemas.microsoft.com/Sql/2008/05/TypedPolling/Core_MutexZombieKiller'>
	<TypedPollingResultSet0>
		<TypedPollingResultSet0>
			<uidInstanceID>de1b73d9-b712-4927-96e8-2e384038e8a7</uidInstanceID>
		</TypedPollingResultSet0>
		<TypedPollingResultSet0>
			<uidInstanceID>bee5c31b-ab8d-4c3b-a24a-56c0d95533c8</uidInstanceID>
		</TypedPollingResultSet0>
		<TypedPollingResultSet0>
			<uidInstanceID>f70732a4-8bff-427b-9c61-163a70d8cad7</uidInstanceID>
		</TypedPollingResultSet0>
	</TypedPollingResultSet0>
</TypedPolling>";
			IBaseMessage inputMessage = MessageHelper.CreateFromString(messageText);

			Queue<Func<CompletionStatus>> terminateInstanceQueue = new Queue<Func<CompletionStatus>>();
			terminateInstanceQueue.Enqueue(() => CompletionStatus.Succeeded);
			terminateInstanceQueue.Enqueue(() => CompletionStatus.Failed);
			terminateInstanceQueue.Enqueue(() => { throw new Exception("ERROR"); });
			List<string> instanceIds = new List<string>();
			MutexZombieKiller.TerminateInstance = (Guid instanceId) => { instanceIds.Add(instanceId.ToString()); return terminateInstanceQueue.Dequeue()(); };
			MutexZombieKiller.GetLogger = (IBaseMessage pInMsg) => new TraceLogger(false, "MutexZombieKiller_Pipeline", LogLevel.All, true, true, false, "s");

			var outputMessages = pipeline.Execute(inputMessage);

			Assert.IsNotNull(outputMessages);
			Assert.IsTrue(outputMessages.Count == 3);
			CollectionAssert.AreEqual(new[] { "de1b73d9-b712-4927-96e8-2e384038e8a7", "bee5c31b-ab8d-4c3b-a24a-56c0d95533c8", "f70732a4-8bff-427b-9c61-163a70d8cad7" }, instanceIds.ToArray());
		}
	}
}
