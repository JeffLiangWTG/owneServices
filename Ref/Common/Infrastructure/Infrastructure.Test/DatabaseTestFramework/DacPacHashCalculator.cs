using System;
using System.IO.Packaging;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test;

static class DacPacHashCalculator
{
	internal static string GetHash(string dacPacFilePath)
	{
		if (string.IsNullOrEmpty(dacPacFilePath))
		{
			return string.Empty;
		}
		using Package dacpac = Package.Open(dacPacFilePath);
		var modelPart = dacpac.GetPart(new Uri("/model.xml", UriKind.Relative));
		return StreamHashCalculator.CalculateHash(modelPart.GetStream());
	}
}
