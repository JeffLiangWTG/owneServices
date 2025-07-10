using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public partial class FormForCurrentQueueUserControlBasherTest : ZChildForm
	{
		public FormForCurrentQueueUserControlBasherTest(IBusiness businessEntity) : base(businessEntity)
		{
		}

		public virtual string BindToPrefix
		{
			get { return ""; }
		}
	}
}
