using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbStaffManagementTreeFormForTest))]
	sealed class GlbStaffManagementTreeFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new GlbStaffManagementTreeFormForTest();
		}

		public class GlbStaffManagementTreeFormForTest : ZForm
		{
			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				Controls.Add(GlbStaffManagementTree);
				CaptionRenderingEnabled = true;
			}

			public readonly GlbStaffManagementTree GlbStaffManagementTree = new GlbStaffManagementTree();
		}

		#endregion
	}
}
