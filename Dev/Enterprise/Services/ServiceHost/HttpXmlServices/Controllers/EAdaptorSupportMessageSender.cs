using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;

namespace Enterprise.Services.ServiceHost
{
	public sealed class EAdaptorSupportMessageSender : IEAdaptorSupportMessageSender
	{
		string SendInternal(string message, Func<eAdaptorController, string> execute)
		{
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			using (RevertUserContextAfterSendingTestEAdaptorMessage())
			{
				request.Content = new StringContent(message);
				controller.Request = request;

				return execute(controller);
			}
		}

		public string Send(string message)
		{
			return SendInternal(message, controller =>
			{
				using (var response = controller.Post())
				{
					return response.Content.ReadAsStringAsync().Result;
				}
			});
		}

		public string SendInRollbackMode(string message)
		{
			return SendInternal(message, controller =>
			{
				using (DataRefreshManager.BeginDisableRefresh())
				using (var transactionManager = Db.Connection.BeginTransactionWithManager())
				{
					var stopwatch = Stopwatch.StartNew();
					using (var response = eAdaptorRequestProcessor.Post(controller.Request, controller.Config))
					{
						stopwatch.Stop();
						transactionManager.RollbackTransaction();
						return response.Content.ReadAsStringAsync().Result + $"\n\nProcessing Time: {stopwatch.Elapsed}";
					}
				}
			});
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Reverting back to original user context.")]
		IDisposable RevertUserContextAfterSendingTestEAdaptorMessage()
		{
			var context = Env.CurrentUserContext;

			return new DisposableAction(() =>
			{
				using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
				{
					Env.SetUserContext(context);
				}
			});
		}
	}
}
