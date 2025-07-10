namespace Enterprise.Customs.Module
{
	using Enterprise.Environment;
	using Enterprise.Licensing;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Modules;

	public abstract class SendDiagnosticMessageModule<T> : ZPopupModule where T : SendDiagnosticMessageController, new()
	{
		protected override sealed ZPopupController GetNewController()
		{
			return new T();
		}

		public override sealed ModuleIdentifier ID
		{
			get { return ModuleIDs.SendTestCustomsMessage; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override sealed SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.SendTestCustomsMessage; }
		}
	}
}
