using CargoWise.IO;

namespace Enterprise.MasterFiles.GUI
{
	public static class HtmlEmailTransformation
	{
		public static string ConvertFileWithExtensionPath(string filePath)
		{
			return PathValidation.GetSafeFilename(filePath + ".html", '_');
		}
	}
}
