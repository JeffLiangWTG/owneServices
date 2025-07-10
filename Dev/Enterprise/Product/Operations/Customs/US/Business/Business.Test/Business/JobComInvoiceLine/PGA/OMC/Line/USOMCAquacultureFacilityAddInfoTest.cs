using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USOMCAquacultureFacilityAddInfo))]
	public class USOMCAquacultureFacilityAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<USOMCAquacultureFacility>();
			var addInfo = new USOMCAquacultureFacilityAddInfo(header.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
