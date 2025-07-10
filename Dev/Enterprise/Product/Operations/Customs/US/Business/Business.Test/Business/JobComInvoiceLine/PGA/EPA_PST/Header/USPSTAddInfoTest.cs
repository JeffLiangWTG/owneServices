using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USPSTAddInfo))]
	public class USPSTAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUS_ProducerEstNoMaxLength()
		{
			var usPSTAddInfo = GetNewBusinessObject() as USPSTAddInfo;
			AssertEquals("US_ProducerEstNo MaxLength", 12, usPSTAddInfo.US_ProducerEstNoInfo.MaxLength);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var pesticide = Factory.New<Pesticide>();
			var addInfo = new USPSTAddInfo(pesticide.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
