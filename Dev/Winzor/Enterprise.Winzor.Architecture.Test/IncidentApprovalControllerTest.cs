using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CustomerService.Module;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Xml;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

class IncidentApprovalControllerTest
{
	[Test]
	[WithSnapshotProtection]
	public async Task NewFormSendScreenShortMessage([Values]bool accept)
	{
		Form form = null;
		var controller = new IncidentApprovalController();
		BusinessObjectFactory factory = null;
		Func<string, Task> capturedCallback = null;

		using var ctx = new EnterpriseTestContext();
		var windowServiceMock = new Mock<IWindowService>();
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(windowService: windowServiceMock.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			factory = new BusinessObjectFactory();
			var docs = factory.Load<EDIInterchange>(new ZQuery());
			docs.DeleteAll();
			factory.Save();

			if (accept)
			{
				UnitTestUserNotification.Instance.AddOKAnswer();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			}
			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			return form;
		}, cargowiseClientServices);

		windowServiceMock.Setup(f => f.ScreenShotAsync(It.IsAny<ScreenShotSetting>())).Returns((ScreenShotSetting setting) => {
			capturedCallback = setting.Callback;
			return Task.FromResult(new RequestSentResult());
		});

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var serviceController = controller as IServiceRequestController;
			serviceController.SetParentForm(form);
			controller.ShowNewForm();
		});

		if (accept)
		{
			using var bitmap = new Bitmap(1, 1);
			var base64 = ConvertImageToBase64(bitmap, ImageFormat.Png);
			await capturedCallback(base64);

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				Assert.That(windowServiceMock.Invocations.Count, Is.GreaterThanOrEqualTo(1));
				Assert.That(windowServiceMock.Invocations.Count(i => i.Method.Name == "ScreenShotAsync"), Is.EqualTo(1));

				var interchange = factory.Load<EDIInterchange>(new ZQuery()).Single();
				Assert.That(interchange.EI_BodyText.ToString(), Contains.Substring("<ERequestDocument "));

				var ediMsg = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
				var serializer = ZXmlSerializer.New(typeof(ERequestDocument));
				var requestDoc = (ERequestDocument)SystemMessage.Deserialize(ediMsg, serializer);

				Assert.That(new ZGuid(requestDoc.ReferenceId).IsValid, Is.EqualTo(true));
				Assert.That(() => requestDoc.Attachments.Count, Is.EqualTo(2).After(1000, 200));

				var screenShot = requestDoc.Attachments.OfType<ERequestDocumentAttachment>().Single(x => x.FileName.StartsWith($"ScreenShot_"));
				Assert.That(screenShot.IsPublished, Is.True);

				var image = Image.FromStream(new MemoryStream(screenShot.Data));
				Assert.That(image.Size.IsEmpty, Is.False);
				Assert.That(image.RawFormat, Is.EqualTo(ImageFormat.Png));
			});
		}
		else
		{
			Assert.That(windowServiceMock.Invocations.Count(i => i.Method.Name == "ScreenShotAsync"), Is.EqualTo(0));
		}
	}

	static string ConvertImageToBase64(Image image, ImageFormat format)
	{
		byte[] imageArray;

		using (var imageStream = new MemoryStream())
		{
			image.Save(imageStream, format);
			imageArray = new byte[imageStream.Length];
			imageStream.Seek(0, System.IO.SeekOrigin.Begin);
			imageStream.Read(imageArray, 0, (int)imageStream.Length);
		}

		return Convert.ToBase64String(imageArray);
	}
}
