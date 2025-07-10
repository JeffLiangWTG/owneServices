using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MasterFiles.GUI
{
	public class GroupCredentialsPlugIn : ZMutexedPlugIn
	{
		public GroupCredentialsPlugIn(GlbGroup group)
			: base(group)
		{
			provider = GlbGroupForPluginWrapperProvider.GetProvider();
		}

		public GroupCredentialsPlugIn(GlbGroup group, GlbGroupForPluginWrapperProvider provider)
			: base(group)
		{
			this.provider = provider;
		}

		public new GlbGroup HostBusinessEntity => (GlbGroup)base.HostBusinessEntity;

		protected readonly GlbGroupForPluginWrapperProvider provider;

		public override string Name => Res.GetString("3a0f66db-b23a-4a51-a5e2-cfbff832d526", "Credentials");

		protected override ZBool HasUserControl => provider != null;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Wrapper;
		}

		IBusiness Wrapper => wrapper ?? (wrapper = provider?.GetWrapperWithMutex(HostBusinessEntity));
		IBusiness wrapper;

		public override ZGlobalMutex Mutex => fMutext ?? (fMutext = new ZGlobalMutex(MutexIDs.GroupCredentialsPlugInBeingCreated, HostBusinessEntity.PK.ToString()));
		ZGlobalMutex fMutext;

		protected override Control GetNewUserControl()
		{
			return provider?.GetNewUserControl();
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			if (!Mutex.IsLocked)
			{
				Mutex.Lock();
			}

			var isLocked = Mutex.IsLocked && !Mutex.HasLock;

			CoveringLabelText = isLocked
				? Res.GetString("56fcd212-aba0-43c2-94b6-24c3ab983860", "Another user is currently creating the Credentials details.\r\nYou will be able to access the tab once they have saved the record. Please try again later.")
				: string.Empty;

			return !isLocked;
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return CoveringLabelText; }
		}

		protected ZString CoveringLabelText;
	}
}
