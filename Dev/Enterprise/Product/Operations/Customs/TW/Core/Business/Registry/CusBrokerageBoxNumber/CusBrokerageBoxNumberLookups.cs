using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusBrokerageBoxNumberLookups : ZLookups
	{
		public CusBrokerageBoxNumberLookups(CusBrokerageBoxNumber parent)
			 : base(parent)
		{
		}

		protected new CusBrokerageBoxNumber Parent => (CusBrokerageBoxNumber)base.Parent;

		protected override BusinessObjectFactory Factory => Parent.Factory;

		public CodeDescriptionPairList CustomsOfficeAreaList => Factory.GetCachedValue<TaiwanCustomsDistrictList>();
	}
}
