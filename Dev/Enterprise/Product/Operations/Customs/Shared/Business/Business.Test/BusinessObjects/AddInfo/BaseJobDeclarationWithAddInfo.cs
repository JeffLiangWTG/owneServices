using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobDeclarationWithAddInfo : BaseJobDeclaration, IAddInfoManager
	{
		public BaseJobDeclarationWithAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public TestAddInfo AddInfo
		{
			get
			{
				if (addInfo == null)
				{
					addInfo = new TestAddInfo(this);
					RegisterEditableChildObject(addInfo);
				}
				return addInfo;
			}
		}
		TestAddInfo addInfo;

		IAddInfo IAddInfoManager.AddInfo => AddInfo;
	}
}
