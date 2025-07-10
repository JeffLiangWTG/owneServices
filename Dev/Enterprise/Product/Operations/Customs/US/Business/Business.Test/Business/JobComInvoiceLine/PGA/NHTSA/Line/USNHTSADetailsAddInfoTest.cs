using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USNHTSADetailsAddInfo))]
	public class USNHTSADetailsAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDetails()
		{
			var details = Factory.New<NHTSADetails>();
			var addInfo = new USNHTSADetailsAddInfo(details.B7_AddInfoDataInfo);
			AssertEquals(details.PK, addInfo.Details.PK);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var details = Factory.New<NHTSADetails>();
			var addInfo = new USNHTSADetailsAddInfo(details.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
