using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDocManagerDataContextTester
	{
		void ExportEdocsViaUniversalXml(BusinessObject bizo);
	}
}
