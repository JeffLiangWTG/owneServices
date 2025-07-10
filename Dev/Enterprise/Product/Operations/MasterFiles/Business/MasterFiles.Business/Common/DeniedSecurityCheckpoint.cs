using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Checkpoint which is never allowed.
	/// </summary>
	public sealed class DeniedSecurityCheckpoint : SecurityCheckpoint
	{
		public DeniedSecurityCheckpoint(string code = "GLOBALAccessDenied", MultilingualString displayText = null)
			: base(code ?? DefaultCode, displayText ?? DefaultDisplayText, null, null, false)
		{ }

		const string DefaultCode = "GLOBALAccessDenied";

		static MultilingualString DefaultDisplayText => ResString.GetMultilingualString("DeniedSecurityCheckpoint.AccessDenied", "Access Denied");

		public override void ShowError()
		{
			Globals.Message.Show(DisplayText);
		}

		public override bool IsAllowed
		{
			get { return false; }
#if DEBUG
			set { base.IsAllowed = value; }
#endif
		}
	}
}
