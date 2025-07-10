using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using CargoWise.Data;
using Enterprise.ZArchitecture.GUI;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.JSInterop;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class ZNotifyIconExTest
{
	[Test, WithPlaywrightPage(IgnoreHTTPSErrors = true)]
	public async Task NotificationServiceDisplayNotification()
	{
		await using var ctx = new InMemoryAppServerTestContext(useHttps: true);
		ZNotifyIconEx zNotifyIconEx = null;
		var rendered = await ctx.LoadFormAsync(() =>
		{
			using (Db.DisposableActionForDbConnection())
			{
				var form = new ZForm();
				var button = new ZButton();
				zNotifyIconEx = new ZNotifyIconEx();
				button.Text = "Notification";
				button.Click += (s, e) =>
				{
					zNotifyIconEx.DisplayToastNotification("test", "test notification");
				};
				form.Controls.Add(button);
				return form;
			}
		});

		_ = await rendered.EvaluateAsync(@"() => {
				const NotificationCore = window.Notification;

				function MockNotification(title, options) {
				  console.log('MockNotification', title, options);
				  MockNotification.calls.push({ title, options });
				  return new NotificationCore(title, options);
				}
				MockNotification.calls = [];
                MockNotification.requestPermission = () => Promise.resolve('granted');

				window.Notification = MockNotification;
			}");
		await rendered.Locator("button").GetByText("Notification").ClickAsync();
		Assert.That(async () => await rendered.EvaluateAsync<int>("() => window.Notification.calls.length"), Is.EqualTo(1).After(5000, 100));
	}
}
