using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.ServiceTasks
{
	[Serializable]
	public class HVLVReadyLogSubscriber : LogSubscriber
	{
		public override string Name => nameof(HVLVReadyLogSubscriber);

		public override string[] EventTypes => new[] { AutoEvents.HVLVReadyCode };

		public override string[] TableNames => new[] { JobShipmentSchema.Constants.TableName };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (var log in queuedLogs)
			{
				var branch = log.Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, log.SJ_GB_NKBranch));
				var department = log.Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, log.SJ_GE_NKDepartment));

				using (branch != null && department != null
					? Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid())
					: null)
				{
					ProcessLog(log);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task logging")]
		void ProcessLog(IQueuedLog log)
		{
			string infoLog;
			var shipment = log.Factory.Load<ForwardingShipment>(log.SJ_ParentID);

			StmALog.GetParametersFromReference(log.SJ_Reference).TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, out var reason);
			if (reason.IsNullOrEmpty())
			{
				infoLog = string.Format(CultureInfo.InvariantCulture,
					"{0} {1} Event ignored. No reason specified.",
					shipment.HumanReadableName, AutoEvents.HVLVReadyCode);
				DefaultLogger.Log(LogType.Information, infoLog);
			}
			else if (reason == Core.Constants.EventReferenceParameterReasons.CargoReporting
					 || reason == Core.Constants.EventReferenceParameterReasons.Outturn)
			{
				ProcessShipment(shipment, reason);
			}
			else
			{
				infoLog = string.Format(CultureInfo.InvariantCulture,
					"{0} {1} Event ignored. Reason '{2}' not recognized.",
					shipment.HumanReadableName, AutoEvents.HVLVReadyCode, reason);
				DefaultLogger.Log(LogType.Information, infoLog);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task logging")]
		void ProcessShipment(ForwardingShipment shipment, string reason)
		{
			if (shipment.JS_ShipmentType != Core.Constants.ShipmentTypes.HighVolumeLowValue)
			{
				var log = string.Format(CultureInfo.InvariantCulture,
					"{0} {1} Event ignored. Shipment Type is '{2}', only '{3}' Shipment Type will be processed.",
					shipment.HumanReadableName, AutoEvents.HVLVReadyCode, shipment.JS_ShipmentType, Core.Constants.ShipmentTypes.HighVolumeLowValue);
				DefaultLogger.Log(LogType.Information, log);
				return;
			}

			RecipientRoleType recipientRole;
			DataContextType targetContext;

			if (shipment.JS_TransportMode == Core.Constants.TransportModes.Air)
			{
				recipientRole = RecipientRoleType.HCA;
				targetContext = DataContextType.AirManifest;

				if (reason == Core.Constants.EventReferenceParameterReasons.Outturn)
				{
					targetContext = DataContextType.UnderBond;
				}
			}
			else if (shipment.JS_TransportMode == Core.Constants.TransportModes.Sea)
			{
				recipientRole = RecipientRoleType.HSA;
				targetContext = DataContextType.SeaOceanBill;
			}
			else
			{
				var log = string.Format(CultureInfo.InvariantCulture,
					"{0} {1} Event ignored. Transport Mode is '{2}', only '{3}' and '{4}' Transport Modes are supported.",
					shipment.HumanReadableName, AutoEvents.HVLVReadyCode, shipment.JS_TransportMode, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea);
				DefaultLogger.Log(LogType.Information, log);
				return;
			}

			if (reason == Core.Constants.EventReferenceParameterReasons.Outturn)
			{
				recipientRole = RecipientRoleType.COA;
			}

			var actionInfo = new ActionInfo(recipientRole.ToRecipientRoleDetails(), shipment) { ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML };
			var outboundSessionTracker = new DataWritingManager(actionInfo);

			var dataObjectWriter = ((IShipmentDataContextManager)shipment.GetUniversalDataContextManager()).GetShipmentDataObjectWriter(outboundSessionTracker);

			ITopLevelDataObject shipmentDataObject;
			try
			{
				shipmentDataObject = dataObjectWriter.GetDataObject(shipment);
			}
			catch (DataObjectValidationException e)
			{
				shipment.Logs.AddNew(AutoEvents.DataExportFailure,
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, e.Message));

				DefaultLogger.Log(LogType.Error, e.Message);

				return;
			}

			shipmentDataObject.DataContext.AddDataTarget(targetContext, null);
			shipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo() { RecipientRoles = actionInfo.RecipientRoleDetails });
			CreateIEDIMessage(shipment, shipmentDataObject, outboundSessionTracker);

			var eventProcessedlog = string.Format(CultureInfo.InvariantCulture,
					"{0} {1} Event queued for processing for Customs.",
					shipment.HumanReadableName, AutoEvents.HVLVReadyCode);
			DefaultLogger.Log(LogType.Information, eventProcessedlog);
		}

		static void CreateIEDIMessage(BusinessObject parentBO, ITopLevelDataObject exportedData, IDataWritingManager dataWritingManager)
		{
			var message = parentBO.Factory.New<IXmlEDIMessage>();
			var subStreamableStream = new CargoWise.IO.Shim.SubStreamableStream();
			new XmlWriter().WriteXML(exportedData, subStreamableStream, dataWritingManager.Schema?.Namespace);

			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;

			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.SetEM_MessageTextOrDataSource(subStreamableStream);

			var parentInfo = EntityInfo.New(parentBO);
			var parentBOWithLogs = parentBO.Factory.Load(parentInfo.Type, parentInfo.InternalPK) as IStmALogParent;
			new MessageDataExportImportLogLinker(AutoEvents.DataExport, parentBO.Factory).LinkMessageToParentBOLogs(message, parentBOWithLogs);
		}

#if DEBUG
		public override bool EnableFactorySaveAlerterInTesting => true;
#endif
	}
}
