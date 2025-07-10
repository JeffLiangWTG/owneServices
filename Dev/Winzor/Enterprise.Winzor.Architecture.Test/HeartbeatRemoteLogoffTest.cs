using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class HeartbeatRemoteLogoffTest
{
	[Test]
	public async Task NoMessageBoxOnRemoteUpgradeLogoffWhenNoOpenForms()
	{
		Application.OpenForms.Clear();
		using var ctx = new EnterpriseTestContext();
		var helper = new HeartbeatRemoteLogoff();
		var utc = new DateTime(2024, 3, 14, 12, 34, 56);
		Assert.That(Application.OpenForms, Is.Empty);

		helper.OnRemoteUpgradeLogoff(utc, () => false);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			Application.DoEvents();
			var messageBox = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox;
			Assert.That(messageBox, Is.Null);
		});
	}
}
