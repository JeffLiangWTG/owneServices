using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class UnknownCusCodeData : CusCodeData
	{
		public UnknownCusCodeData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection();

		public override bool SupportsNotes => false;
	}
}
