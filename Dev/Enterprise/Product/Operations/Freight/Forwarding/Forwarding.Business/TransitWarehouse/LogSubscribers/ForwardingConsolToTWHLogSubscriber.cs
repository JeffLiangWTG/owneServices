using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Business.TransitWarehouseInstructionHelper;
using EventRefParams = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.Business
{
	[Serializable]
	public sealed class ForwardingConsolToTWHLogSubscriber : LogSubscriber
	{
		public override string Name => "ForwardingConsolToTWHLogSubscriber";

		public override string FriendlyName => (NoResString)"Sending forwarding consols to transit warehouses";

		public override string[] EventTypes => new[] { AutoEvents.MessageSendingRequestCode };

		public override string[] TableNames => new[] { AutoJobConsol.Schema.TableName };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs) => queuedLogs.ForEach(HandleLogEvent);

		void HandleLogEvent(IQueuedLog queuedLog)
		{
			var factory = queuedLog.Factory;
			var consol = factory.Load<ForwardingConsol>(queuedLog.SJ_ParentID);
			if (consol == null)
			{
				return;
			}

			var offset = queuedLog.EventTimeOffset.Offset;
			var parameters = StmALog.GetParametersFromReference(queuedLog.SJ_Reference);
			if (parameters.TryGetValue(EventRefParams.Direction, out var directionStr) && Enum.TryParse<Direction>(directionStr, out var direction) &&
				parameters.TryGetValue(EventRefParams.Service, out var serviceStr) && Enum.TryParse<ServiceRequest>(serviceStr, out var service))
			{
				var branch = factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, queuedLog.SJ_GB_NKBranch));
				if (branch != null)
				{
					using (Environment.DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						var notificationsParent = new LogNotifier();
						if (service == ServiceRequest.PrepareDispatch)
						{
							consol.Shipments.Reload(false);
							var shipmentLogQuery = new ZDBOnlyQuery(typeof(StmALog));
							shipmentLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.JobShipmentSelectedCode);
							shipmentLogQuery.AddToFilter(StmALogSchema.SL_Reference, $"|{EventRefParams.DeclarationID}={consol.JK_UniqueConsignRef}");
							shipmentLogQuery.AddToFilter(StmALogSchema.SL_EventTime, queuedLog.EventTime);
							shipmentLogQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);

							var shipmentPKs = factory.Load<StmALog>(shipmentLogQuery).Select(s => s.SL_Parent).ToHashSet();
							var selectedShipments = factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, shipmentPKs)).ToArray();
							var instruction = new ConsolPrepareForDispatchInstruction(consol, direction);
							instruction.ShipmentsForSelection.OfType<ShipmentPrepareForDispatchInstruction>().ForEach(s => s.SelectedForDelivery = shipmentPKs.Contains(s.Shipment.PK));

							Func<BusinessObjectFactory, ManualDataExport> func = factory => GetManualDataExportForSendingSelectedShipmentsForPrepareDispatch(factory, consol, instruction);
							new TransitWarehouseInstructionHelper(consol, notificationsParent, func, selectedShipments).SendTransitWarehouseInstructionInLogSubscriber(direction, service, factory, offset);
						}
						else
						{
							new TransitWarehouseInstructionHelper(consol, notificationsParent).SendTransitWarehouseInstructionInLogSubscriber(direction, service, factory, offset);
						}
					}
				} }
		}

		ManualDataExport GetManualDataExportForSendingSelectedShipmentsForPrepareDispatch(BusinessObjectFactory factory, ForwardingConsol consol, ConsolPrepareForDispatchInstruction instruction)
		{
			return new ManualDataExport(factory,
				new[] { consol },
				UniversalDataType.UniversalShipment,
				null, null, null,
				manager => ObjectFactory.Get<IConsolPrepareForDispatchDataObjectWriter>(nameof(IConsolPrepareForDispatchDataObjectWriter), manager, instruction));
		}
	}
}
