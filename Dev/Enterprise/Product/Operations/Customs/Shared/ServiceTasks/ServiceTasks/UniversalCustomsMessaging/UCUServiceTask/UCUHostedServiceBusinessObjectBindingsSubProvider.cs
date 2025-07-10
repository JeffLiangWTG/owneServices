using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public sealed class UCUHostedServiceBusinessObjectBindingsSubProvider : IHostedServiceBusinessObjectBindingsSubProvider
	{
		IEnumerable<IHostedServiceBusinessObjectBinding> IHostedServiceBusinessObjectBindingsSubProvider.BusinessObjectBindings
			=> businessObjectBindings ?? (businessObjectBindings = CreateBusinessObjectBindings());
		IEnumerable<IHostedServiceBusinessObjectBinding> businessObjectBindings;

		IEnumerable<IHostedServiceBusinessObjectBinding> CreateBusinessObjectBindings()
		{
			var applicationCodes = UCUSubscribers
				.GetApplicationCodes()
				.OrderBy(x => x)
				.Select(code => FormattableString.Invariant($"'{code}'"))
				.ToArray();

			if (applicationCodes.Length == 0)
			{
				yield break;
			}

			yield return CreateInterchangeBusinessObjectBinding(applicationCodes);
		}

		static IHostedServiceBusinessObjectBinding CreateInterchangeBusinessObjectBinding(string[] applicationCodes)
		{
			var appCodePredicate = FormattableString.Invariant($"{EDIInterchange.Schema.EI_ApplicationCode} IN ({string.Join(", ", applicationCodes)})");
			var predicates = new[]
			{
				appCodePredicate, FormattableString.Invariant($"{EDIInterchange.Schema.EI_ReceiveTransmit}={EDIInterchange.Direction.Receive}"),
				FormattableString.Invariant($"{EDIInterchange.Schema.EI_Status}={EDIInterchange.Status.Queued}"),
				FormattableString.Invariant($"{EDIInterchange.Schema.EI_IsActive}=1"),
				FormattableString.Invariant($"{EDIInterchange.Schema.EI_TransportType}={EDIInterchange.TransportType.xT}"),
			};

			return new UCMPBusinessObjectBinding(EDIInterchange.Schema.TableName,
				UniversalCustomsMessagingConstants.ServiceTaskCodes.Unpacking,
				InterchangeUnpackingQueueName,
				predicates);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task name")]
		const string InterchangeUnpackingQueueName = "UCU Interchange Unpacking";
	}
}
