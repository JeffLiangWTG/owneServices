using CargoWise.Application;
using Enterprise.Integration.RemoteDesktopServices;
using NUnit.Framework;

namespace WinzorFramework.RemoteClientServices.Test
{
	internal class RemoteFileIntegrationTest
	{
		[Test]
		public void TestConstructIRemoteFileDefaultConstructor()
		{
			var remoteDesktopServices = ObjectFactory.Get<IRemoteFile>();
			Assert.That(remoteDesktopServices, Is.TypeOf<CargoWiseClientFile>());
		}

		[Test]
		public void TestConstructIRemoteFile()
		{
			var remoteFile = ObjectFactory.Get<IRemoteFile>(nameof(IRemoteFile), string.Empty, System.Array.Empty<byte>(), false, false);
			Assert.That(remoteFile, Is.TypeOf<CargoWiseClientFile>());
		}
	}
}
