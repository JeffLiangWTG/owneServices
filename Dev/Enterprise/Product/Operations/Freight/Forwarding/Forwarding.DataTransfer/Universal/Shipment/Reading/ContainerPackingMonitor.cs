using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	sealed class ContainerPackingMonitor : IDisposable
	{
		public static IDisposable MonitorContainerTareWeightChanges(UniversalShipment dataObject, ForwardingShipment shipment)
		{
			IDisposable NoOp() => new DisposableAction(() => { });

			if (shipment == null
				|| dataObject?.DataContext?.GetMatchingDataTarget(DataContextType.ForwardingShipment) == null // targeting ForwardingShipment
				|| dataObject?.DataContext?.GetMatchingDataTarget(DataContextType.ForwardingConsol) != null // not targeting ForwardingConsol
				|| dataObject.PackingLineCollection == null
				|| dataObject.PackingLineCollection.Any(p => p.ContainerLink != null)) // we need to monitor only when we we have only loose packlines in UXml
			{
				return NoOp();
			}

			var consol = shipment
				.OuterPackLines
				.CurrentConsol;

			if (consol == null)
			{
				return NoOp();
			}

			var containers = consol
				.Containers
				.OfType<ForwardingContainer>()
				.ToArray();

			return new ContainerPackingMonitor(containers);
		}

		ContainerPackingMonitor(ForwardingContainer[] containers)
		{
			this.containers = new List<ForwardingContainer>(containers);
			InitializeMonitoring();
		}

		readonly List<ForwardingContainer> containers;
		readonly Dictionary<ZGuid, List<string>> stackTraces = new Dictionary<ZGuid, List<string>>();

		void InitializeMonitoring()
		{
			foreach (var container in containers)
			{
				container.JC_TareWeightInfo.ValueChanged += OnTareWeightValueChanged;
			}
		}

		void OnTareWeightValueChanged(object sender, EventArgs args)
		{
			if (sender is ForwardingContainer container)
			{
				if (!stackTraces.TryGetValue(container.PK, out var list))
				{
					list = new List<string>();
					stackTraces[container.PK] = list;
				}

				list.Add(new StackTrace(2).ToString());
			}
		}

		public void Dispose()
		{
			foreach (var container in containers)
			{
				container.JC_TareWeightInfo.ValueChanged -= OnTareWeightValueChanged;
			}

			containers.Clear();

			if (stackTraces.Count == 0)
			{
				return;
			}

			var messages = new List<string>();

			foreach (var stackTrace in stackTraces)
			{
				var stackTracesForMessage = string.Join("\r\n", stackTrace.Value);
				var message = $@"Container PK:{stackTrace.Key}\r\n{stackTracesForMessage}";
				messages.Add(message);
			}

			var finalMessage = $"Container Tare Weight has been updated during UniversalXml import\r\n{string.Join("\r\n", messages)}";
			ErrorReporter.ReportOnce("Container-TareWeight-UniversalXml", finalMessage);
		}
	}
}
