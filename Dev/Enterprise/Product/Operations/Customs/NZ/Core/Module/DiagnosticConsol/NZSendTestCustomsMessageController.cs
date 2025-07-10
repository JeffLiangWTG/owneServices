namespace Enterprise.Customs.NZ.Module
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Module;
	using Enterprise.Customs.NZ.Business;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Modules;

	public class NZSendTestCustomsMessageController : SendDiagnosticMessageController
	{
		protected override DiagnosticConsol GetNewBusinessEntity(BusinessObjectFactory factory)
		{
			return new NZDiagnosticConsol(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
