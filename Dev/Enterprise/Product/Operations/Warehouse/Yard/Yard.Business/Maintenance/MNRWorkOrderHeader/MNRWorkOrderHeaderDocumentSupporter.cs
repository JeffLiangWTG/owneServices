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
	public class MNRWorkOrderHeaderDocumentSupporter : DocumentSupporter
	{
		public MNRWorkOrderHeaderDocumentSupporter(MNRWorkOrderHeader workOrderHeader)
			: base(workOrderHeader)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.MNRWorkOrder; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, MNRWorkOrder);

			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(DataContext.MNRWorkOrder, MNRWorkOrder) };
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new[] { DataContext.MNRWorkOrder };
		}

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region MNRWorkOrder

		protected MNRWorkOrderHeader MNRWorkOrder
		{
			get { return (MNRWorkOrderHeader)BusinessObject; }
		}

		#endregion
	}
}
