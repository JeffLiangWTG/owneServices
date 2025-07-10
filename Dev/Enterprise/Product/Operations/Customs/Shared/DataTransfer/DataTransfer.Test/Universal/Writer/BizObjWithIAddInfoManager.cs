using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	class BizObjWithIAddInfoManager : DummyBizObjWithAddInfoChildSupporter, IAddInfoManager
	{
		public BizObjWithIAddInfoManager(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		TestAddInfo addInfo;
		public TestAddInfo AddInfo => addInfo ?? (addInfo = new TestAddInfo(Z0_VarCharMaxInfo));

		IAddInfo IAddInfoManager.AddInfo => AddInfo;
	}
}
