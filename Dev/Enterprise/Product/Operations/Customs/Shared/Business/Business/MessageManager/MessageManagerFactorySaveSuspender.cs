using System;
using CargoWise.Common;
using Enterprise.Customs.Business.MessageManagers;

namespace Enterprise.Customs.Business;

public class MessageManagerFactorySaveSuspender : IDisposable
{
	public MessageManagerFactorySaveSuspender(MessageManager messageManager)
	{
		messageManager.ShouldSuspendFactorySave = true;
		disposable = new DisposableAction(() => messageManager.ShouldSuspendFactorySave = false);
	}

	public MessageManagerFactorySaveSuspender(EDIFACTMessageManager messageManager)
	{
		messageManager.ShouldSuspendFactorySave = true;
		disposable = new DisposableAction(() => messageManager.ShouldSuspendFactorySave = false);
	}

	public void Dispose() => disposable.Dispose();

	readonly IDisposable disposable;
}
