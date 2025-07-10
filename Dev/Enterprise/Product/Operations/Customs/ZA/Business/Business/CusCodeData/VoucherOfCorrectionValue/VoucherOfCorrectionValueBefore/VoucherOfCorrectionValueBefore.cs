using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class VoucherOfCorrectionValueBefore : VoucherOfCorrectionValue
	{
		public VoucherOfCorrectionValueBefore(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString VOCValueType
		{
			get { return CusCodeDataTypeList.Codes.VOCValueBefore; }
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(ZAMessage), typeof(CusEntryHeader)); }
		}
	}
}
