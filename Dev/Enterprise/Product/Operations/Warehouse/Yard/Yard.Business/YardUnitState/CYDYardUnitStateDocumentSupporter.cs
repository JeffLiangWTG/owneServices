using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardUnitStateDocumentSupporter : DocumentSupporter
	{
		public CYDYardUnitStateDocumentSupporter(CYDYardUnitState yardUnitState)
			: base(yardUnitState)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CYDYardUnitState; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, YardUnitState);

			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.CYDYardUnitState, YardUnitState) };
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Core.Constants.DataContext.CYDYardUnitState };
		}

		#region SecurityCheckPoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region YardUnitState

		protected CYDYardUnitState YardUnitState
		{
			get { return (CYDYardUnitState)BusinessObject; }
		}

		#endregion
	}
}
