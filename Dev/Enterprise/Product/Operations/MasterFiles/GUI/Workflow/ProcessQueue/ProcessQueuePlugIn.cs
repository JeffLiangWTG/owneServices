using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MasterFiles.GUI
{
	public class ProcessQueuePlugIn : ZPlugIn
	{
		public ProcessQueuePlugIn(IProcessQueueParent processQueueParent) : base(processQueueParent)
		{
		}

		#region Overrides

		// May need to have a ProcessManagement Licence when we expose this to other parts of the system (generic)
		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		public override string Name
		{
			get { return (NoResString)"Process Queue"; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			Control result = new ProcessQueueUserControl();
			result.Dock = DockStyle.Fill;
			return result;
		}

		#endregion
	}
}
