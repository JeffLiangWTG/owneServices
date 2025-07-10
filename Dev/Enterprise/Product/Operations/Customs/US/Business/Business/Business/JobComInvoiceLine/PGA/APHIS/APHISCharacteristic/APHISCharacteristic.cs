using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class APHISCharacteristic : IAPHISCharacteristic
	{
		public static APHISCharacteristic New(ZString value, ZString qualifier, ZString description, bool sendWithOutCharacteristic = false)
		{
			APHISCharacteristic result = null;
			if (!value.IsEmpty || sendWithOutCharacteristic)
			{
				result = new APHISCharacteristic()
				{
					CommodityQualifierCode = qualifier,
					CommodityCharacteristicQualifier = value,
					CommodityCharacteristicDescription = description
				};
			}
			return result;
		}

		#region IAPHISCharacteristic Members

		public ZString CommodityQualifierCode
		{
			get;
			set;
		}

		public ZString CommodityCharacteristicQualifier
		{
			get;
			set;
		}

		public ZString CommodityCharacteristicDescription
		{
			get;
			set;
		}

		#endregion
	}
}
