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
	public class CompanyCredentialsPlugIn : ZMutexedPlugIn
	{
		public CompanyCredentialsPlugIn(GlbCompany company)
			: this(company, GlbCompanyWrapperProvider.GetProvider(company.GC_RN_NKCountryCode))
		{
		}

		public CompanyCredentialsPlugIn(GlbCompany group, GlbCompanyWrapperProvider provider)
			: base(group)
		{
			this.provider = provider;
			Enabled = Wrapper?.IsValidWrapper ?? false;
		}

		#region Override

		public override string Name => Res.GetString("24BB5E9A-ADF5-4084-8121-B6FDC819553D", "Credentials");

		protected override string TextOverride => provider?.PluginTextOverride;

		protected override ZBool HasUserControl => provider != null;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return Wrapper;
		}

		public override ZGlobalMutex Mutex => fMutext ?? (fMutext = new ZGlobalMutex(MutexIDs.CompanyCredentialsPlugInBeingCreated, HostBusinessEntity.PK.ToString()));
		ZGlobalMutex fMutext;

		protected override Control GetNewUserControl()
		{
			var userControl = new CompanyCredentialsUserControl();
			userControl.SetCompanyCredentialsDetailsLayout(provider.GetNewCompanyCredentialsLayout());
			return userControl;
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			if (!Mutex.IsLocked)
			{
				Mutex.Lock();
			}
			var isLocked = Mutex.IsLocked && !Mutex.HasLock;

			CoveringLabelText = isLocked
				? Res.GetString("{E8C6FE55-9F2D-4CB5-A5FA-88EDE36CA330}", "Another user is currently creating the Credentials details.\r\nYou will be able to access the tab once they have saved the record. Please try again later.")
				: string.Empty;

			return !isLocked;
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get { return CoveringLabelText; }
		}

		#endregion

		#region Implement

		public new GlbCompany HostBusinessEntity => (GlbCompany)base.HostBusinessEntity;

		GlbCompanyWrapper Wrapper => wrapper ?? (wrapper = provider?.GetWrapper(HostBusinessEntity));
		GlbCompanyWrapper wrapper;

		readonly GlbCompanyWrapperProvider provider;

		protected ZString CoveringLabelText;

		#endregion
	}
}
