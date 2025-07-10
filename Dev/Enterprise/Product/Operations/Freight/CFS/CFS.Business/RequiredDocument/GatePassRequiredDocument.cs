
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassRequiredDocument : CFSRequiredDocument
	{
		public GatePassRequiredDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
