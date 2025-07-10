using System;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	class CusUnderbondTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var bizObj = (CusUnderbond)Factory.New<Integration.Customs.AU.ICusUnderbond>();
			var row = ((INeedRow)bizObj).Row;
			var typeDecider = new CusUnderbondTypeDecider();
			foreach ((string code, Type type) testData in new[]
			{
				(CusUnderbondApplicationCodeList.Codes.AUUnderbond, ObjectFactory.GetType<Integration.Customs.AU.ICusUnderbond>()),
				(CusUnderbondApplicationCodeList.Codes.GBFallback, ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusUnderbond_Fallback>()),
				(CusUnderbondApplicationCodeList.Codes.GBInterAirportRemoval, ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusUnderbond_InterAirportRemoval>()),
				(CusUnderbondApplicationCodeList.Codes.GBInterShedRemoval, ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusUnderbond_InterShedRemoval>()),
				(CusUnderbondApplicationCodeList.Codes.GBTranshipmentRemoval, ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusUnderbond_TranshipmentRemoval>()),
				(CusUnderbondApplicationCodeList.Codes.NZTranshipmentRequest, ObjectFactory.GetType<Integration.Customs.NZ.ICusUnderbond>()),
				("#@$", typeof(CusUnderbond))
			})
			{
				bizObj.C4_ApplicationCode = testData.code;
				AssertEquals(testData.code, testData.type, typeDecider.GetTypeForLoad(row, Factory));
			}
		}
	}
}
