using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TR.Module
{
	public class StatementsStampDutyController : Customs.Module.StatementController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.TR.StatementsStampDuty;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusStatementHeader);

		protected override SecurityCheckpoint CheckPointForView => Environment.Env.Security.StatementsStampDuty;

		protected override SecurityCheckpoint CheckPointForNew => Environment.Env.Security.StatementsStampDuty;

		protected override SecurityCheckpoint CheckPointForEdit => Environment.Env.Security.StatementsStampDuty;

		protected override SecurityCheckpoint CheckPointForDelete => Environment.Env.Security.StatementsStampDuty;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var shallCreateNewForm = true;
			if (businessEntity != null && !businessEntity.IsInDatabase)
			{
				var localDate = Env.Time.CurrentLocalDate;
				var statementHeader = new CusStatementHeader.Loader(new BusinessObjectFactory()).LoadMonthlyStatementWithPeriodStartDate(CusStatementHeaderTypes.Codes.GlobalManifest, localDate, GlbCompany.CurrentCompany.PK);
				if (statementHeader != null)
				{
					var statementNumber = statementHeader.B2_StatementNumber;
					Globals.Message.ShowWarning(Res.GetString("3A810FEC-5D9A-4E51-A973-21A5718C637A", "The monthly statement has been created for this month.\r\nPlease check {0}", statementNumber));
					shallCreateNewForm = false;
				}
			}
			return shallCreateNewForm ? new StatementsStampDutyForm((CusStatementHeader)businessEntity) : null;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var header = base.GetNewBusinessEntityInLocalFactory() as CusStatementHeader;
			header.B2_StatementType = CusStatementHeaderTypes.Codes.GlobalManifest;
			header.B2_IsMonthlyStatement = true;
			return header;
		}
	}
}
