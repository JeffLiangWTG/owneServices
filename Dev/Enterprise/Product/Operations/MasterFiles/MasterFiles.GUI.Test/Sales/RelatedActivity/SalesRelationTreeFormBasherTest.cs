using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SalesRelationTreeFormForTest))]
	sealed class SalesRelationTreeFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new SalesRelationTreeFormForTest();
		}

		public class SalesRelationTreeFormForTest : ZForm
		{
			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				Controls.Add(SalesRelationTree);
				CaptionRenderingEnabled = true;
			}

			public readonly SalesRelationTree SalesRelationTree = new SalesRelationTree();
		}

		#endregion
	}
}
