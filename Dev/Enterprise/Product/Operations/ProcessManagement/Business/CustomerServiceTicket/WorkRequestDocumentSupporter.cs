using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkRequestDocumentSupporter : DocumentSupporter
	{
		public WorkRequestDocumentSupporter(WorkRequest workRequest)
			: base(workRequest)
		{
		}

		protected WorkRequest WorkRequest
		{
			get { return (WorkRequest)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WorkRequest; }
		}

		public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomerServiceTicketCustomiseDocuments; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.GenericFreightJob, BusinessObject) };
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.GenericFreightJob };
		}
	}
}
