using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CMDPermitNumber : CusCodeData, Integration.Customs.SG.ICMDPermitNumber
	{
		public CMDPermitNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.CMD;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(ForwardingShipment)); }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new CMDPermitNumberValidation(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "CMD Permit Numnbers"; }
		}
	}
}
