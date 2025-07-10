using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWiseNext.Infrastructure.Installations;
using Moq;

namespace CargoWise.Blazor.Testing.Common
{
	public static class UrlHandlerProviderMock
	{
		public static Mock<IUrlHandlerProvider> GetMock()
		{
			Mock<IUrlHandlerProvider> mockUrlHandlerProvider = new();
			mockUrlHandlerProvider.Setup(p => p.GetUrlHandler()).Returns("cargowiseclient");
			return mockUrlHandlerProvider;
		}
		public static IUrlHandlerProvider GetObject()
		{
			return GetMock().Object;
		}
	}
}
