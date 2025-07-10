using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class DummyBusinessObjectWithIAddInfoManager : DummyBusinessObject, IAddInfoManager
	{
		public DummyBusinessObjectWithIAddInfoManager(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public TestAddInfo AddInfo
		{
			get { return addInfo ?? (addInfo = new TestAddInfo(this)); }
		}
		TestAddInfo addInfo;

		#region IAddInfoManager Members

		IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion
	}
}
