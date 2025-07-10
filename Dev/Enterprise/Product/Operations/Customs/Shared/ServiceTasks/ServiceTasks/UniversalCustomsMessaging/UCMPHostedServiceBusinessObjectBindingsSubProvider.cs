using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public sealed class UCMPHostedServiceBusinessObjectBindingsSubProvider : IHostedServiceBusinessObjectBindingsSubProvider
	{
		IEnumerable<IHostedServiceBusinessObjectBinding> IHostedServiceBusinessObjectBindingsSubProvider.BusinessObjectBindings
			=> businessObjectBindings ?? (businessObjectBindings = CreateBusinessObjectBindings());
		IEnumerable<IHostedServiceBusinessObjectBinding> businessObjectBindings;

		IEnumerable<IHostedServiceBusinessObjectBinding> CreateBusinessObjectBindings()
		{
			var applicationCodes = UniversalCustomsMessagingSubscribers
				.GetApplicationCodes()
				.OrderBy(x => x)
				.Select(code => FormattableString.Invariant($"'{code}'"))
				.ToArray();

			if (applicationCodes.Length == 0)
			{
				yield break;
			}

			yield return CreateIncomingMessageBusinessObjectBinding(applicationCodes);
		}

		static IHostedServiceBusinessObjectBinding CreateIncomingMessageBusinessObjectBinding(string[] applicationCodes)
		{
			var appCodePredicate = FormattableString.Invariant($"EM_ApplicationCode IN ({string.Join(", ", applicationCodes)})");
			var predicates = new[]
			{
				appCodePredicate,
				"EM_ReceiveTransmit=RCV",
				"EM_Status=QUE",
				"EM_IsActive=1",
				"EM_HeldUntilDate IS NULL OR EM_HeldUntilDate < GETUTCNOW()",
			};

			return new UCMPBusinessObjectBinding(EDIMessage.Schema.TableName,
				UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen,
				IncomingMessageProcessingQueueName,
				predicates);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task name")]
		const string IncomingMessageProcessingQueueName = "UCM Incoming Message Processing";
	}
}
