using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.ZA.Module
{
	class CusStatementController : Customs.Module.StatementController
	{
		public CusStatementController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ZAModuleIDs.CustomsStatement; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusStatementLineCharge); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			throw new NotSupportedException("No PlugIns");
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotImplementedException();
		}
	}
}
