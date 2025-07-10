using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	class WorkerUniversalCustomsApplicationTypeMessageProcessor : UniversalCustomsApplicationTypeMessageProcessor, IUniversalCustomsMessageProcessorHelper
	{
		public WorkerUniversalCustomsApplicationTypeMessageProcessor(LoggingInformation logger, string applicationCode, IUniversalCustomsMessageProcessor customsMessageProcessor, int maxConcurrentHandles)
			: base(logger, applicationCode, customsMessageProcessor)
		{
			this.GrEngine = new EDIMessageGrEngine(logger, applicationCode, GrEngineServiceSetting.Worker, maxConcurrentHandles: maxConcurrentHandles);
		}
		public readonly EDIMessageGrEngine GrEngine;

		protected override ZString[] StatusesToInclude => new ZString[] { EDIMessage.Status.PreProcessedOK };
		protected sealed override bool RequiresPreProcessingCore => false;

		protected sealed override void ProcessMessageCore(EDIMessage message)
		{
			try
			{
				message.EM_LinkTableInfo.ValueChanged += LinkDataInfo_ValueChanged;
				message.EM_LinkUniqueIDInfo.ValueChanged += LinkDataInfo_ValueChanged;
				reportSettingEM_LinkedObjectIndex = 0;
				customsMessageProcessor.ProcessMessage(message, Logger, this);
			}
			finally
			{
				message.EM_LinkTableInfo.ValueChanged -= LinkDataInfo_ValueChanged;
				message.EM_LinkUniqueIDInfo.ValueChanged -= LinkDataInfo_ValueChanged;
			}
		}

		void LinkDataInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsReportSettingEM_LinkedObjectSuspended && e is ValueChangedEventArgs vce && !vce.OldValue.Equals(vce.NewValue))
			{
				var additionalDetails = string.Empty;
				if (customsMessageProcessor is IUniversalCustomsMessageProcessorWithAdditionalErrorReportDetails customsMessageProcessorWithAdditionalErrorReportDetails)
				{
					var message = vce.Info.BizObj as EDIMessage;
					additionalDetails = customsMessageProcessorWithAdditionalErrorReportDetails.GetAdditionalErrorReportDetails(message);
				}
				ErrorReporter.ReportDeveloperExceptionOnce($"{customsMessageProcessor.GetType().FullName}{additionalDetails} is setting EM_LinkedObject not in UCK", $"{customsMessageProcessor.GetType().FullName}{additionalDetails} is setting EM_LinkedObject data in ProcessMessage when it should have done in UCK", null);
			}
		}

		bool IsReportSettingEM_LinkedObjectSuspended => reportSettingEM_LinkedObjectIndex > 0;
		int reportSettingEM_LinkedObjectIndex;

		IDisposable IUniversalCustomsMessageProcessorHelper.SuspendReportSettingEM_LinkedObject() => new DisposableAction(() => reportSettingEM_LinkedObjectIndex++, () => reportSettingEM_LinkedObjectIndex--);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task name")]
		protected override string MessageFriendlyNameCore => "Worker Universal Customs Messaging";
	}
}
