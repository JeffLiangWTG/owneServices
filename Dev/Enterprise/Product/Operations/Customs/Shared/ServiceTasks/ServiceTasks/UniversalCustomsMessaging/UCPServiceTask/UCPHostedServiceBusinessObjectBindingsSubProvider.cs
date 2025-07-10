using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public sealed class UCPHostedServiceBusinessObjectBindingsSubProvider : IHostedServiceBusinessObjectBindingsSubProvider
	{
		IEnumerable<IHostedServiceBusinessObjectBinding> IHostedServiceBusinessObjectBindingsSubProvider.BusinessObjectBindings
			=> businessObjectBindings ?? (businessObjectBindings = CreateBusinessObjectBindings());
		IEnumerable<IHostedServiceBusinessObjectBinding> businessObjectBindings;

		IEnumerable<IHostedServiceBusinessObjectBinding> CreateBusinessObjectBindings()
		{
			var applicationCodes = UCPSubscribers
				.GetApplicationCodes()
				.OrderBy(x => x)
				.Select(code => FormattableString.Invariant($"'{code}'"))
				.ToArray();

			if (applicationCodes.Length == 0)
			{
				yield break;
			}

			yield return CreateOutgoingMessageBusinessObjectBinding(applicationCodes);
		}

		static IHostedServiceBusinessObjectBinding CreateOutgoingMessageBusinessObjectBinding(string[] applicationCodes)
		{
			var appCodePredicate = FormattableString.Invariant($"EM_ApplicationCode IN ({string.Join(", ", applicationCodes)})");
			var predicates = new[]
			{
				appCodePredicate,
				"EM_ReceiveTransmit=TRX",
				"EM_Status=QUE",
				"EM_IsActive=1",
				"EM_HeldUntilDate IS NULL OR EM_HeldUntilDate < GETUTCNOW()",
			};

			return new UCMPBusinessObjectBinding(EDIMessage.Schema.TableName,
				UniversalCustomsMessagingConstants.ServiceTaskCodes.Packing,
				(ZArchitecture.Core.NoResString)"UCP Outgoing Message Packing",
				predicates);
		}
	}
}
