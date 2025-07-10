using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Winzor.Architecture.Test;
using NUnit.Framework;

namespace WinzorFramework.RemoteClientServices.Test
{
	class CargoWiseClientFileTest
	{
		[Test]
		public async Task CargoWiseClientFileDisposeShouldNotThrowInvalidOperationExceptionWhenNoAvailableFormsAsync()
		{
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderFormAsync(() =>
			{
				form = new Form();
				return form;
			});

			var name = "Sample";
			var bytes = Encoding.UTF8.GetBytes(name);
#pragma warning disable CA2000 // Dispose objects before losing scope
			var clientFile = new CargoWiseClientFile(name, bytes, false, false);
#pragma warning disable CA2000 // Dispose objects before losing scope

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				form.Close();
				Assert.DoesNotThrow(() => clientFile.Dispose(), "Form is unavailable");
			});
		}
	}
}
