using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IBookingConfirmationLinkedProcessor
	{
		void Process(IBusiness bizObj, IStmALog log);
	}
}
