using System;
using System.Reflection;
using CargoWise.Application;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class FileOpenerTest
{
	[Test]
	public void TestFileOpener()
	{
		var mockRemoteFile = new Mock<IRemoteFile>();
		mockRemoteFile.Setup(r => r.Open()).Returns(true);
		using (ObjectFactory.Substitute(mockRemoteFile.Object))
		{
			var name = Assembly.GetExecutingAssembly().Location;
			var uri = new Uri(name);
			FileOpener.Open(uri.LocalPath);
			mockRemoteFile.Verify(r => r.Open(), Times.Once());
		}
	}
}
