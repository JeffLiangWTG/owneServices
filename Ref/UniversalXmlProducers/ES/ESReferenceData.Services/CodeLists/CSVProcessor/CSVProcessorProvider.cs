using System.IO;
using System.Reflection;
using System.Text;

namespace CargoWise.RefDbRepo.ESReferenceData.Services;

public static class CSVProcessorProvider
{
	public static string RequestCodes(string csvProcessorCode)
	{
		var result = string.Empty;
		using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.RefDbRepo.ESReferenceData.Services.CodeLists.CSVProcessor.RefCusCodeList_{csvProcessorCode}.csv"))
		using (var reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8")))
		{
			result = reader.ReadToEnd();
		}
		return result;
	}
}
