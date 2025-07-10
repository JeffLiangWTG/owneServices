using System.Collections;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IXmlDataTransferExporter
	{
		void PromptUserAndExport(IList selectedElements);
		void PromptUserAndExport(ZQuery exportQuery);
	}
}
