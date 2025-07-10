using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(PersonAssociationsTreeFormForTest))]
	sealed class PersonAssociationsTreeFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new PersonAssociationsTreeFormForTest();
		}

		public class PersonAssociationsTreeFormForTest : ZForm
		{
			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				Controls.Add(PersonAssociationsTree);
				CaptionRenderingEnabled = true;
			}

			public readonly PersonAssociationsTree PersonAssociationsTree = new PersonAssociationsTree();
		}

		#endregion
	}
}
