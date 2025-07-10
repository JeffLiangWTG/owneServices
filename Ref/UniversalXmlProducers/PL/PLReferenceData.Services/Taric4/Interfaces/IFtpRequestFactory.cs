using System;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;

interface IFtpRequestFactory
{
	IFtpWebResponse CreateAndRun(Uri ftpUri, string ftpMethod);
}
