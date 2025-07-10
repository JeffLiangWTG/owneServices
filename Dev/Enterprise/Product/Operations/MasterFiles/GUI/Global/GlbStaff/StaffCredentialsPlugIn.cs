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
	public class StaffCredentialsPlugIn : ZMutexedPlugIn
	{
		public StaffCredentialsPlugIn(GlbStaff staff)
			: base(staff)
		{
			provider = GlbStaffWrapperProvider.GetProvider();
		}

		public StaffCredentialsPlugIn(GlbStaff group, GlbStaffWrapperProvider provider)
			: base(group)
		{
			this.provider = provider;
		}

		public new GlbStaff HostBusinessEntity => (GlbStaff)base.HostBusinessEntity;

		protected readonly GlbStaffWrapperProvider provider;

		public override string Name => Res.GetString("31833594-4D6C-480B-AA9B-CA73B26AEB8E", "Credentials");

		protected override ZBool HasUserControl => provider != null;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Wrapper;
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();

			if (result == ContinueWithSave.Yes && provider != null && Wrapper != null)
			{
				result = provider.ShowPreSaveDialogs(Wrapper);
			}

			return result;
		}

		GlbStaffWrapper Wrapper => wrapper ?? (wrapper = provider?.GetWrapper(HostBusinessEntity));
		GlbStaffWrapper wrapper;

		public override ZGlobalMutex Mutex => fMutext ?? (fMutext = new ZGlobalMutex(MutexIDs.StaffCredentialsPlugInBeingCreated, HostBusinessEntity.PK.ToString()));
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
				? Res.GetString("43E38F1E-0328-4315-93FE-7C801D79C672", "Someone else is already in the process of creating the details for the Credentials tab.\r\nYou should be able to access it when the person has saved the record. Please try later.")
				: string.Empty;

			return !isLocked;
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return provider?.GetNewTopLevelMenu(Wrapper);
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return CoveringLabelText; }
		}

		protected ZString CoveringLabelText;
	}
}
