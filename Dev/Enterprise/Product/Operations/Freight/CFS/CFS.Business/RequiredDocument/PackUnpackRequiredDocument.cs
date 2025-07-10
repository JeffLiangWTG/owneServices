
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackRequiredDocument : CFSRequiredDocument
	{
		public PackUnpackRequiredDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
