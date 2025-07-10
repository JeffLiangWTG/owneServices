using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IDraftBOLLinkedProcessor
	{
		void Process(IBusiness bizObj, IStmALog log);
	}
}
