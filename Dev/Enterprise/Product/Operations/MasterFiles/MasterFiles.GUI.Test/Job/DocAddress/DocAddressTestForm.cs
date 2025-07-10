using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed partial class DocAddressTestForm : ZForm
	{
		public DocAddressTestForm(JobDocAddressParentForTesting parent)
			: base(parent.DocAddresses)
		{
		}

		internal DocAddressUserControl Control
		{
			get { return control ?? (control = new DocAddressUserControl(HostBusinessEntity)); }
		}
		DocAddressUserControl control;

		internal static IBusiness HostBusinessEntity;
	}
}
