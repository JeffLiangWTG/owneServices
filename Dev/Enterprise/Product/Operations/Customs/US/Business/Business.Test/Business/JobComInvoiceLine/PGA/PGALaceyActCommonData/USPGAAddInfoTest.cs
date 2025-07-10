using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USPGAAddInfo))]
	public class USPGAAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			PGA pga = Factory.New<PGA>();
			USPGAAddInfo addInfo = new USPGAAddInfo(pga.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
