using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed partial class FormForProcessQueueUserControlBasherTest : ZChildForm
	{
		public FormForProcessQueueUserControlBasherTest(IProcessQueueParent businessEntity) : base(businessEntity)
		{
		}
	}
}
