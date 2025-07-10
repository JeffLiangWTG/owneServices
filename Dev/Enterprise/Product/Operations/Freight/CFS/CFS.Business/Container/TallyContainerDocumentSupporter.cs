using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyContainerDocumentSupporter : CFSContainerDocumentSupporter
	{
		public TallyContainerDocumentSupporter(TallyContainer container)
			: base(container)
		{
		}

		public TallyContainer TallyContainer
		{
			get { return (TallyContainer)BusinessObject; }
		}

		#region Overrides

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Core.Constants.DataContext.PackUnpackContainerRego, Core.Constants.DataContext.TallyContainer, Core.Constants.DataContext.GenericFreightJob };
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CFSTallySheet; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Core.Constants.DataContext.PackUnpackContainerRego || dataContext == Core.Constants.DataContext.TallyContainer)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.TallyContainer, TallyContainer) };
			}

			if (dataContext == Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, TallyContainer);
			}

			return null;
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Core.Constants.DataContext.PackUnpackContainerRego
				&& dataContext != Core.Constants.DataContext.TallyContainer
				&& dataContext != Core.Constants.DataContext.GenericFreightJob
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#endregion
	}
}
