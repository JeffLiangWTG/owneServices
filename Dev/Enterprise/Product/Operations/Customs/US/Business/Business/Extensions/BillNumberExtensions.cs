using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class BillNumberExtensions
	{
		public static ZBool IsValidSCAC(this ZString scac, BusinessObjectFactory factory, ZString transportMode)
		{
			return factory.GetCachedValue<ZBool>(scac + transportMode, () =>
			{
				var query = new ZQuery(USCarrierCombinedSchema.UI_Code, scac);
				var transportCodeList = TransportTypeList.ConvertFromTransportMode(transportMode);
				if (transportCodeList.Length > 0)
				{
					query.AddToFilter(USCarrierCombinedSchema.UI_ModeOfTransportation, transportCodeList);
				}
				return factory.Load<USCarrierCombined>(query).Length > 0;
			});
		}

		public static ZString GetSCAC(this ZString billNumber, IEnumerable<ZString> allValidSCACs)
		{
			var result = billNumber.Left(4);
			return allValidSCACs.Contains(result) ? result : allValidSCACs.FirstOrDefault();
		}

		public static ZString GetBillNumberTrimSCAC(this ZString billNumber) => billNumber.SubstringSafe(4);

		public static ZString KeepValidBillNumberCharacters(this ZString billNumber) => billNumber.KeepAlphanumericCharacters().TrimEnd(' ');
	}
}
