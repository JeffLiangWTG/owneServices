using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostingDocumentDeclarationSupporter : DocumentSupporter
	{
		public LandedCostingDocumentDeclarationSupporter(LandedCostHeader lCHeader)
			: base(lCHeader)
		{
			this.LCHeader = lCHeader;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.LandedCostHeader };
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.LandedCostHeader; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			switch (dataContext)
			{
				case DataContext.LandedCostHeader:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(DataContext.LandedCostHeader, LCHeader) };
					break;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState result = new DocumentSupporterDataState(true, "");
			if (commandAboutToBeRun.SU_MenuName.StartsWith("Landed Costing") && commandAboutToBeRun.SU_MenuName != "Landed Costing - Old")
			{
				if (LCHeader.Histories.Count == 0)
				{
					result.IsValid = false;
					result.ErrorMessage = Res.GetString("3cd26ae6-6ae1-48d6-abd8-ebad6db9e4d3", "There are no LC lines to print. Please click a menu Landed Costing -> Run Landed Costing");
				}
			}
			return result;
		}

		readonly LandedCostHeader LCHeader;
	}
}
