using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class BillDetail
	{
		public ZString? BillNumber;
		public List<AddInfo> AddInfoCollection;
	}
}
