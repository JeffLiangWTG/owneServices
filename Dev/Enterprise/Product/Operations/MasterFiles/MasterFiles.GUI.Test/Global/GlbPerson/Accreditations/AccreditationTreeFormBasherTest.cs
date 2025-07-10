using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccreditationTreeFormForTest))]
	sealed class AccreditationTreeFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new AccreditationTreeFormForTest();
		}

		public class AccreditationTreeFormForTest : ZForm
		{
			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				Controls.Add(SalesRelationTree);
				CaptionRenderingEnabled = true;
			}

			public readonly AccreditationTree SalesRelationTree = new AccreditationTree();
		}

		#endregion
	}
}
