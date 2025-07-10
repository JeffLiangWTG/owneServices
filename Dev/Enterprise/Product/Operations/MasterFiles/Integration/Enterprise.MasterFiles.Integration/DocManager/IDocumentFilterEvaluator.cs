using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDocumentFilterEvaluator
	{
		bool IsApplicable(BusinessObject bizObj, ZString filter, params BusinessObject[] otherDataSources);
	}
}