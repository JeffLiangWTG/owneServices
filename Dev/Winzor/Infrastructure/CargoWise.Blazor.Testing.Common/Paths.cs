using System;
using System.IO;

namespace CargoWise.Blazor.Common.Test;

public static class Paths
{
	public static readonly string SourcePath = bool.TryParse(Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var datIsTesting) && datIsTesting
		? Environment.GetEnvironmentVariable("DAT_TestSourcePath")
		: GetPathRelativeToExecutable("../../");

	public static string GetPathRelativeToExecutable(string folderOrFileName)
	{
		var executingAssemblyDirectory = Path.GetDirectoryName(AppContext.BaseDirectory);
		return Path.Combine(executingAssemblyDirectory, folderOrFileName);
	}
}
