using CargoWise.Glow.CW1.ApplicationCheckpoints;
using Enterprise.Core.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	sealed public class GlowApplicationCheckpoint : SecurityCheckpoint
	{
		public GlowApplicationCheckpoint(string code, MultilingualString displayText, ISecurityCheckpoint parent, IZSecurity security)
			: base(ApplicationCheckpointConstants.StoragePrefix + code, displayText, parent, security, addToLookUpTable: true)
		{
		}

		public override bool IsAllowed
		{
			get => IsGroupAllowed == SecurityState.Granted;
		}
	}
}
