using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MasterFiles.GUI
{
	public class DocAddressesPlugIn : ZPlugIn
	{
		public DocAddressesPlugIn(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		#region Overrides

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		public override string Name
		{
			get { return (NoResString)"Addresses"; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override ZBool IsActive
		{
			get
			{
				var isActive = base.IsActive;
				if (!isActive && Enabled)
				{
					isActive = Host?.DocAddresses?.Count > 0;
				}
				return isActive;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!isDocAddressesPopulated)
			{
				UpdateCollectionWithCurrent();
			}
		}

		#endregion

		protected override Control GetNewUserControl()
		{
			var result = new DocAddressUserControl(HostBusinessEntity);
			result.Dock = DockStyle.Fill;
			return result;
		}

		protected override void OnCurrentChanged()
		{
			base.OnCurrentChanged();
			UpdateCollectionWithCurrent();
		}

		void UpdateCollectionWithCurrent()
		{
			if (DocAddresses.Count > 0)
			{
				using (DocAddresses.ForceNotSuspendListChanged())
				{
					DocAddresses.RemoveAll();
				}
			}

			if (Host != null)
			{
				RefreshAddresses();
			}
		}

		public void RefreshAddresses()
		{
			var hostDocAddresses = Host.DocAddresses.ToArray();
			foreach (JobDocAddress address in hostDocAddresses)
			{
				if (!address.IsEmpty && !DocAddresses.Contains(address))
				{
					DocAddresses.Add(address);
				}
			}

			isDocAddressesPopulated = true;
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return DocAddresses;
		}

		protected override bool RegisterPlugInBusinessEntityAsEditable
		{
			get { return false; }
		}

		internal IDocAddresses Host
		{
			get { return IsCurrentDependent ? (IDocAddresses)Current : HostBusinessEntity as IDocAddresses; }
		}

		public JobDocAddressCollectionForPlugin DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					var addressOverrideSupporter = HostBusinessEntity as IJobDocAddressOverrideSupporter;
					if (addressOverrideSupporter != null)
					{
						fDocAddresses = addressOverrideSupporter.GetJobDocAddressCollectionForPlugin(Factory);
					}
					if (fDocAddresses == null)
					{
						fDocAddresses = new JobDocAddressCollectionForPlugin(new JobDocAddressCollection(Factory));
					}

					fDocAddresses.HostParentBizo = HostBusinessEntity;
				}
				return fDocAddresses;
			}
		}
		JobDocAddressCollectionForPlugin fDocAddresses;
		bool isDocAddressesPopulated;

		public override void OnUserControlShown()
		{
			base.OnUserControlShown();
			UpdateCollectionWithCurrent();
		}

		public override void RefreshData()
		{
			UpdateCollectionWithCurrent();
		}
	}
}
