using System;
using System.IO;

namespace CargoWise.RefDbRepo.Staging.Common
{
	public interface IResponseStream : IDisposable
	{
		Stream GetResponseStream();
	}
}
