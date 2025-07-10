using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class ControlCollectionTest
{
	[Test]
	public async Task AddRangePerformLayoutAfterAllControlsAdded()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() => {
			Control owner = new Control();
			List<int> controlCountOfPerformLayout = new List<int>();

			owner.Layout += OnOwnerPerformLayout;

			void OnOwnerPerformLayout(object sender, LayoutEventArgs e)
			{
				controlCountOfPerformLayout.Add(owner.Controls.Count);
			}

			List<Control> childrenControls = new List<Control>();
			int controlsCount = 5;
			for (int i = 0; i < controlsCount; i++)
			{
				childrenControls.Add(new Control());
			}

			owner.Controls.AddRange(childrenControls);

			Assert.That(controlCountOfPerformLayout.Count, Is.EqualTo(1));
			Assert.That(controlCountOfPerformLayout, Has.Exactly(1).EqualTo(controlsCount));
		});
	}
}
