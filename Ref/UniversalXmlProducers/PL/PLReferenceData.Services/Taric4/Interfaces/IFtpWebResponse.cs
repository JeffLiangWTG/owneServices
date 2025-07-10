using System;
using System.IO;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;

interface IFtpWebResponse : IDisposable
{
	Stream GetResponseStream();
}
