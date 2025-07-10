using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class JobServiceDocumentSupporter : DocumentSupporter
	{
		public JobServiceDocumentSupporter(JobService service)
			: base(service)
		{
		}

		protected internal JobService Service
		{
			get { return (JobService)BusinessObject; }
		}

		#region Overrides

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Core.Constants.DataContext.RequestForService,
				Core.Constants.DataContext.GenericFreightJob
			};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.JobService; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;

			if (dataContext == Constants.DataContext.GenericFreightJob)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Service.RequestForServiceParent, Service);
			}
			else
			{
				var documentSupportable = Service.RequestForServiceParent as IDocumentSupportable;
				if (documentSupportable != null)
				{
					var documentWrappers = documentSupportable.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Service, null);
					var wrapper = DocumentWrapperFactory.CreateServiceWrapperWithParent(Service, documentWrappers[0]);
					result = new DocumentWrapper[] { wrapper };
				}
				else
				{
					result = System.Array.Empty<DocumentWrapper>();
				}
			}

			return result;
		}

		public override bool ShowDocumentsInDynamicMenu
		{
			get
			{
				var result = false;

				if (Service.RequestForServiceParent is IDocumentSupportable parentDocSupportable)
				{
					var docSupporter = parentDocSupportable.DocumentSupporter;
					result = docSupporter.GetDocumentWrappers(docSupporter.DefaultDataContext, null) != null;
				}

				return result;
			}
		}

		#endregion
	}
}
