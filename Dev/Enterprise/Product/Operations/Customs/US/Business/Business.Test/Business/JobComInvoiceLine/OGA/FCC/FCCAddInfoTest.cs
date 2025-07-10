using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FCCAddInfo))]
	public class FCCAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFCC()
		{
			FCC fcc = Factory.New<FCC>();
			FCCAddInfo addInfo = new FCCAddInfo(fcc.B7_AddInfoDataInfo);
			AssertEquals(fcc.PK, addInfo.Parent.PK);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			FCC fcc = Factory.New<FCC>();
			FCCAddInfo addInfo = new FCCAddInfo(fcc.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
