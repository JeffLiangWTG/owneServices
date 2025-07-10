using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectDocumentSupporter : DocumentSupporter
	{
		public ProjectDocumentSupporter(Project project)
			: base(project)
		{
		}

		protected Project Project
		{
			get { return (Project)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Project; }
		}

		public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.ProjectCustomiseDocuments; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.GenericFreightJob };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Project);
			if (genericWrappers != null)
			{
				return genericWrappers;
			}
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.GenericFreightJob, Project) };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			OrgHeaderContact result = new OrgHeaderContact(Project.ClientOrganisation, null);
			return result;
		}
	}
}
