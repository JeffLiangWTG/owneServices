
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSRequiredDocument : JobRequiredDocument
	{
		public CFSRequiredDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
