using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.Business
{
	public class CusStatementHeaderDocumentSupporter : DocumentSupporter
	{
		public CusStatementHeaderDocumentSupporter(BusinessObject cusStatement) : base(cusStatement)
		{
		}

		public override BusinessContext BusinessContext => BusinessContext.Statement;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.CustomsDeclarationCustomiseDocument;

		protected CusStatementHeader StatementHeader => (CusStatementHeader)BusinessObject;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return new[] { BODocDataProvider.Get(new StampDutyLedgerDocumentWrapper(StatementHeader)) };
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DataContext[] GetSupportedDataContexts() => new[] { DataContext.Statement };

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState result;
			result = base.GetDataStateBeforeRun(commandAboutToBeRun);

			if (result.IsValid && commandAboutToBeRun != null && commandAboutToBeRun.SU_MenuName == PrintStampDutyLedgerMenuItemName)
			{
				var cusStatementHeaderDocumentSupporterConfigurator = ObjectFactory.Get<ITRCusStatmentHeaderDocumentSupporterConfigurator>();
				result = cusStatementHeaderDocumentSupporterConfigurator.PrintStampDutyLedgerConfig(StatementHeader, result);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Menu item name string")]
		const string PrintStampDutyLedgerMenuItemName = "Print Stamp Duty Ledger";
	}
}
