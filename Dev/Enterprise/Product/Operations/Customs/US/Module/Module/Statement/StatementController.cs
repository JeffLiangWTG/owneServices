using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.US.Module
{
	sealed class StatementController : Customs.Module.StatementController
	{
		public StatementController()
		{
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.USCustomsStatement;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusStatementHeader);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			IZForm result = null;

			var statementHeader = (CusStatementHeader)businessEntity;
			if (statementHeader.IsMonthlyStatement)
			{
				result = new PeriodicStatementForm(statementHeader);
			}
			else
			{
				result = new StatementForm(statementHeader);
			}
			return result;
		}

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.USCustomsImportStatementModify;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.USCustomsImportStatementView;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => throw new NotSupportedException("No PlugIns");
	}
}
