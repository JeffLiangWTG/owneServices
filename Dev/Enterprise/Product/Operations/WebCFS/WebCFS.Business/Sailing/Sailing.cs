using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.WebCFS.Business
{
	public class Sailing : JobSailing
	{
		public Sailing(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
