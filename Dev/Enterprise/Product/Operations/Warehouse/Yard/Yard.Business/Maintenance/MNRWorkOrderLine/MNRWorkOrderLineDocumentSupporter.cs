using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderLineDocumentSupporter : DocumentSupporter
	{
		public MNRWorkOrderLineDocumentSupporter(MNRWorkOrderLine workOrderLine)
			: base(workOrderLine)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.MNRWorkOrderLine; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, MNRWorkOrderLine);

			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(DataContext.MNRWorkOrderLine, MNRWorkOrderLine) };
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new[] { DataContext.MNRWorkOrderLine };
		}

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region MNRWorkOrder

		protected MNRWorkOrderLine MNRWorkOrderLine
		{
			get { return (MNRWorkOrderLine)BusinessObject; }
		}

		#endregion
	}
}
