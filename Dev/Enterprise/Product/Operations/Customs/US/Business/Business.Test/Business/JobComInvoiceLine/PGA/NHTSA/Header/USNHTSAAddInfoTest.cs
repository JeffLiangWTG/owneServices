using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USNHTSAAddInfo))]
	public class USNHTSAAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<NHTSAHeader>();
			var addInfo = new USNHTSAAddInfo(header.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
