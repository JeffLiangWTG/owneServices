using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.GUI;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	class WarehouseOperatorTransactionsController : ZController
	{
		public override ControllerID ID => ZAControllerIDs.WarehouseOperatorTransactions;

		public override ModuleIdentifier ModuleID => ZAModuleIDs.WarehouseOperatorTransactions;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusWHSOperatorTransaction);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAWarehouseOperatorTransactions);

		protected override SecurityCheckpoint CheckPointForNew => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAWarehouseOperatorTransactions);

		protected override SecurityCheckpoint CheckPointForEdit => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAWarehouseOperatorTransactions);

		protected override SecurityCheckpoint CheckPointForDelete => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAWarehouseOperatorTransactions);

		protected override IZForm GetForm(IBusiness businessEntity) => new WarehouseOperatorTransactionsForm((CusWHSOperatorTransaction)businessEntity);
	}
}
