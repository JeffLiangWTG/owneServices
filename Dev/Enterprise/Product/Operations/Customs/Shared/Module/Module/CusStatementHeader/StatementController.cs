using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	[ControllerDoesNotSupportForm]
	public class StatementController : ZController
	{
		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.CustomsStatement;

		public override ModuleIdentifier ModuleID => null;

		public override Type TypeOfTopLevelBusinessObject => typeof(BaseCusStatementHeader);

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			if (CheckStatementBelongsToThisCompany(sourceEntity))
			{
				result = base.ShowEditForm(sourceEntity);
			}
			return result;
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			if (CheckStatementBelongsToThisCompany(sourceEntity))
			{
				result = base.ShowViewForm(sourceEntity);
			}
			return result;
		}

		bool CheckStatementBelongsToThisCompany(BusinessObject sourceEntity)
		{
			bool result = true;
			if (sourceEntity is BaseCusStatementHeader statementHeader &&
				statementHeader.Company != null &&
				statementHeader.Company.PK != GlbCompany.CurrentCompany.PK)
			{
				Globals.Message.ShowError(Res.GetString("7ecc4f6d-4b65-4b41-8e98-1c8066d0e9c4", "You are trying to view a statement that belongs to a different company. Please log into the company '{0}' and try again.", statementHeader.Company.GC_Name));
				result = false;
			}
			return result;
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("");
		}
	}
}
