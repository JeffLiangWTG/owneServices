namespace Enterprise.Customs.Module
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Environment;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.GUI;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Modules;

	public abstract class SendDiagnosticMessageController : ZSingletonController
	{
		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SendTestCustomsMessage; }
		}

		protected override sealed IZForm GetForm(IBusiness businessEntity)
		{
			var factory = new BusinessObjectFactory();
			var bizo = GetNewBusinessEntity(factory);
			return new DiagnosticConsolForm(bizo);
		}

		public override sealed ControllerID ID
		{
			get { return ControllerIDs.Customs.SendTestCustomsMessage; }
		}

		public override sealed Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DiagnosticConsol); }
		}

		protected sealed override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.Browse;
		}

		protected abstract DiagnosticConsol GetNewBusinessEntity(BusinessObjectFactory factory);
	}
}
