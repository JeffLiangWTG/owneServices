using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class WarehouseCustomsLinePackDetails : IWarehouseCustomsLinePackDetails
	{
		#region IWarehouseCustomsLinePackDetails Members

		public ZString PackID
		{
			get;
			set;
		}

		public ZInt PackageQty
		{
			get;
			set;
		}

		public ZDecimal PackedQty
		{
			get;
			set;
		}

		#endregion
	}
}
