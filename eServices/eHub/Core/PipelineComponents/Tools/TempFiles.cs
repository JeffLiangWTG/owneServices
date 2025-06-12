using System;
using System.IO;
namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	class TempFiles
	{
		public static void CleanupBizTalkTempFiles()
		{
			// Path.GetTempPath() is the path used by BTSXslTransform, this can be seen by disassembling the dll
			// SXN is the 3 character string used by BTSXslTransform, this can be seen by disassembling the dll
			foreach (string f in Directory.EnumerateFiles(Path.GetTempPath(), "SXN*.tmp"))
			{
				try
				{
					if (File.GetLastWriteTime(f) < DateTime.Now.AddDays(-1))
					{
						File.Delete(f);
					}
				}
				catch (IOException ex) { Tracer.TraceError(ex); }
				catch (UnauthorizedAccessException ex) { Tracer.TraceError(ex); }
			}
		}
	}
}
