using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class TelEdge : AutoTelEdge, ITelEdge
	{
		public TelEdge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
