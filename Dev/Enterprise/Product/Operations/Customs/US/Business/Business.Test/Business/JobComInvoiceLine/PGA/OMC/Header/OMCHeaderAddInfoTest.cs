using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OMCHeaderAddInfo))]
	public class OMCHeaderAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<OMCHeader>();
			var addInfo = new OMCHeaderAddInfo(header.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
