using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	class BizObjWithIAddInfoManagerWithSchema : BizObjWithIAddInfoManager, IAddInfoManagerWithSchema
	{
		public BizObjWithIAddInfoManagerWithSchema(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZDecimal UZ_Decimal
		{
			get => AddInfo.UZ_Decimal;
			set => AddInfo.UZ_Decimal = value;
		}

		public ZPropertyInfo UZ_DecimalInfo => GetWrappedZPropertyInfo(nameof(UZ_Decimal), (x) => AddInfo.UZ_DecimalInfo);

		public ZDateTime UZ_Date
		{
			get => AddInfo.UZ_Date;
			set => AddInfo.UZ_Date = value;
		}

		public ZPropertyInfo UZ_DateInfo => GetWrappedZPropertyInfo(nameof(UZ_Date), (x) => AddInfo.UZ_DateInfo);

		ITableSchema IAddInfoManagerWithSchema.AddInfoSchema => TestAddInfoSchema.Instance;
	}
}
