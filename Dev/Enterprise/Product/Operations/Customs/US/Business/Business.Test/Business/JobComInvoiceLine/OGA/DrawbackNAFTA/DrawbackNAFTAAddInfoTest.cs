using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackNAFTAAddInfo))]
	public class DrawbackNAFTAAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			DrawbackNAFTA drawbackNAFTA = Factory.New<DrawbackNAFTA>();
			DrawbackNAFTAAddInfo addInfo = new DrawbackNAFTAAddInfo(drawbackNAFTA.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
