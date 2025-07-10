using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class US7501DocPrinting : AutoUS7501DocPrinting
	{
		public US7501DocPrinting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
