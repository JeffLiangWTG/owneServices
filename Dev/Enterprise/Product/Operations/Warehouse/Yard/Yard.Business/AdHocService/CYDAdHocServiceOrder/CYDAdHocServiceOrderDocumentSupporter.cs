using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Business
{
	public  class CYDAdHocServiceOrderDocumentSupporter : DocumentSupporter
	{
		public CYDAdHocServiceOrderDocumentSupporter(CYDAdHocServiceOrder adHocServiceOrder)
			: base(adHocServiceOrder)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CYDAdHocServiceOrder; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, AdHocServiceOrder);

			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.CYDAdHocServiceOrder, AdHocServiceOrder) };
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Core.Constants.DataContext.CYDAdHocServiceOrder };
		}

		#region SecurityCheckPoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region AdHocServiceOrder

		protected CYDAdHocServiceOrder AdHocServiceOrder
		{
			get { return (CYDAdHocServiceOrder)BusinessObject; }
		}

		#endregion
	}
}
