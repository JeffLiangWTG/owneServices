using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Testing
{
	sealed class ProgressLogTest : TestCaseWithFactory
	{
		public void TestProgress()
		{
			using (var form = new ZForm())
			using (ControlExtensions.ForceInvokeCalls())
			{
				var log = new ProgressLog();
				form.Controls.Add(log);

				form.Show();

				log.SetProgressMax(3);
				Application.DoEvents();
				AssertProgress("SetProgressMax", log, 0, 3);

				log.BumpProgress();
				Application.DoEvents();
				AssertProgress("BumpProgress", log, 1, 3);

				log.BumpProgress();
				Application.DoEvents();
				AssertProgress("BumpProgress again", log, 2, 3);

				log.BumpProgress();
				Application.DoEvents();
				AssertProgress("BumpProgress final", log, 3, 3);
			}
		}

		public void TestNotifyFormat()
		{
			using (var form = new ZForm())
			using (ControlExtensions.ForceInvokeCalls())
			{
				var log = new ProgressLog();
				form.Controls.Add(log);

				form.Show();

				var textBox = (RichTextBox)log.Controls["logTextBox"];

				log.NotifyFormat(NotificationType.Information, "{1} {0} {1}", new LogUrlLink("Link", new Uri("http://www.cargowise.com")), "Text");
				Application.DoEvents();
				AssertEquals("Text Link#KEY0000 Text\n", textBox.Text);

				textBox.Select(0, 5);
				Application.DoEvents();
				AssertEquals(false, textBox.GetSelectionLink());

				textBox.Select(5, 12);
				Application.DoEvents();
				AssertEquals(true, textBox.GetSelectionLink());

				textBox.Select(17, 5);
				Application.DoEvents();
				AssertEquals(false, textBox.GetSelectionLink());
			}
		}

		public void TestNotify()
		{
			using (var form = new ZForm())
			using (ControlExtensions.ForceInvokeCalls())
			{
				var log = new ProgressLog();
				form.Controls.Add(log);

				form.Show();

				var textBox = (RichTextBox)log.Controls["logTextBox"];

				log.Notify(NotificationType.Information, "Text 1");
				Application.DoEvents();

				AssertEquals("Text 1\n", textBox.Text);

				log.Notify(NotificationType.Information, "Text 2");
				Application.DoEvents();

				AssertEquals("Text 1\nText 2\n", textBox.Text);
			}
		}

		void AssertProgress(string message, ProgressLog log, int count, int max)
		{
			var mainProgress = (ProgressBar)log.Controls["progressBar"];

			AssertEquals($"{message}: Value", count, mainProgress.Value);
			AssertEquals($"{message}: Maximum", max, mainProgress.Maximum);
		}
	}
}
