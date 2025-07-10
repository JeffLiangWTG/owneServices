using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	[DescriptionProperty(AutoRefVessel.Schema.RV_RadioCallSign)]
	public class TWRefVessel : RefVessel
	{
		public TWRefVessel(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
