using System;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Telematics.ServiceTasks.PreDriveChecklist
{
	class TelematicsPreDriveChecklistAlertProcessor
	{
		public TelematicsPreDriveChecklistAlertProcessor(ILogger logger)
			: this(
				new BusinessObjectFactory { NameForDebugging = "Telematics checklist alert processor" },
				new TelematicsChecklistAccessor(),
				new TelematicsChecklistProcessor(),
				logger)
		{
		}

		internal TelematicsPreDriveChecklistAlertProcessor(BusinessObjectFactory factory, IChecklistAccessor checklistAccessor, IChecklistProcessor checklistProcessor, ILogger logger)
		{
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
			this.checklistAccessor = checklistAccessor ?? throw new ArgumentNullException(nameof(checklistAccessor));
			this.checklistProcessor = checklistProcessor ?? throw new ArgumentNullException(nameof(checklistProcessor));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public void Run(CancellationToken cancellationToken)
		{
			logger.Log(LogType.Information, "Start processing Checklists");
			var checklists = checklistAccessor.GetChecklists(factory, cancellationToken);
			logger.Log(LogType.Information, $"Processing {checklists.Count} Checklists");
			var checklistsProcessed = checklistProcessor.ProcessChecklists(factory, checklists, cancellationToken);
			logger.Log(LogType.Information, $"Processed {checklistsProcessed} Checklists");
			factory.Save();
		}

		readonly BusinessObjectFactory factory;
		readonly IChecklistAccessor checklistAccessor;
		readonly IChecklistProcessor checklistProcessor;
		readonly ILogger logger;
	}
}
