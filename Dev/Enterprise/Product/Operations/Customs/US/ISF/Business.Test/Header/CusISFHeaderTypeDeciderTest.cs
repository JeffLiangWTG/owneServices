using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestCorrectlyTypeIsReturn()
		{
			var trackingType = ObjectFactory.GetType<Integration.Customs.US.ISF.ITrackingCusISFHeader>();
			var normalType = typeof(CusISFHeader);
			CusISFHeaderTypeDecider typeDecider = new CusISFHeaderTypeDecider();
			var header = Factory.New<CusISFHeader>();
			var row = ((INeedRow)header).Row;
			var oldValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = false;
				AssertEquals(normalType, typeDecider.GetTypeForBinding());
				AssertEquals(normalType, typeDecider.GetTypeForLoad(row, Factory));
				AssertEquals(normalType, typeDecider.GetTypeForNew());
				Globals.IsWeb = true;
				AssertEquals(trackingType, typeDecider.GetTypeForBinding());
				AssertEquals(trackingType, typeDecider.GetTypeForLoad(row, Factory));
				AssertEquals(trackingType, typeDecider.GetTypeForNew());
			}
			finally
			{
				Globals.IsWeb = oldValue;
			}
		}
	}
}
