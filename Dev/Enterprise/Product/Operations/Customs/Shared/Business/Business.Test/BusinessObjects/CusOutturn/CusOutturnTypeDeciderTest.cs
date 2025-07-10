using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusOutturnTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeByApplication()
		{
			var typeDecider = new CusOutturnTypeDecider();
			var cusOutturn = GetNewBusinessObjectForLoadTest();
			var row = ((INeedRow)cusOutturn).Row;

			cusOutturn.C5_ApplicationCode = CusOutturnApplicationCodeList.Codes.CUK;
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should return GB CCSUK for ApplicationCode CUK", Core.Constants.CountryCodes.Eritrea, ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusOutturn>(), DefaultType);

			cusOutturn.C5_ApplicationCode = CusOutturnApplicationCodeList.Codes.EMC;
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should return EMCS InvoiceLineCusOutturn for ApplicationCode EMC", Core.Constants.CountryCodes.Eritrea, ObjectFactory.GetType<Integration.Customs.EUEMCS.IInvoiceLineCusOutturn>(), DefaultType);

			cusOutturn.C5_ApplicationCode = CusOutturnApplicationCodeList.Codes.ZAC;
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should return ZA CusOutturn for ApplicationCode ZAC", Core.Constants.CountryCodes.Eritrea, ObjectFactory.GetType<Integration.Customs.ZA.ICusOutturn>(), DefaultType);

			cusOutturn.C5_ApplicationCode = CusOutturnApplicationCodeList.Codes.CMR;
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should return AU CusOutturn for ApplicationCode CMR", Core.Constants.CountryCodes.Eritrea, ObjectFactory.GetType<Integration.Customs.AU.ICusOutturn>(), DefaultType);

			cusOutturn.C5_ApplicationCode = "XXX";
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should return Base CusOutturn for invalid ApplicationCode", Core.Constants.CountryCodes.Eritrea, DefaultType, DefaultType);
		}

		public void TestGetTypeByCountry()
		{
			var typeDecider = new CusOutturnTypeDecider();
			var cusOutturn = GetNewBusinessObjectForLoadTest();
			var row = ((INeedRow)cusOutturn).Row;

			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should fallback to Base CusOutturn", Core.Constants.CountryCodes.Eritrea, DefaultType);
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should go straight to AU", Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusOutturn>());
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should go straight to ZA", Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusOutturn>());
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should fall through EU to Base CusOutturn", Core.Constants.CountryCodes.Italy, DefaultType);

			cusOutturn.C5_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should return the EU (EMCS Invoice Line) CusOutturn", Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.EUEMCS.IInvoiceLineCusOutturn>(), DefaultType);
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should return the EU (EMCS Invoice Line) CusOutturn", Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.EUEMCS.IInvoiceLineCusOutturn>(), DefaultType);
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should return the EU (EMCS Invoice Line) CusOutturn", Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.EUEMCS.IInvoiceLineCusOutturn>(), DefaultType);
			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should return the EU (EMCS Invoice Line) CusOutturn", country, ObjectFactory.GetType<Integration.Customs.EUEMCS.IInvoiceLineCusOutturn>(), DefaultType);
			}

			cusOutturn.C5_ParentTableCode = CusHAWBSchema.Constants.Prefix;
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should fall through EU to Base CusOutturn", Core.Constants.CountryCodes.Italy, DefaultType);
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should fall through EU to Base CusOutturn", Core.Constants.CountryCodes.Germany, DefaultType);
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should return the GB CcsUK CusOutturn", Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusOutturn>(), DefaultType);
			AssertTypeDeciderReturnsExpectedType(typeDecider, row, "Should fallback to Base CusOutturn", Core.Constants.CountryCodes.Eritrea, DefaultType);
		}

		void AssertTypeDeciderReturnsExpectedType(CusOutturnTypeDecider typeDecider, DataRow row, string message, string countryCode, Type expectedType, Type expectedTypeForBindingAndNewIfDifferent = null)
		{
			AssertNotNull(expectedType);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				AssertEquals(message, expectedType, typeDecider.GetTypeForLoad(row, Factory));
				AssertEquals("NOTE: Binding may differ from load as no information can be extracted from the DataRow. \n" + message, expectedTypeForBindingAndNewIfDifferent ?? expectedType, typeDecider.GetTypeForBinding());
				AssertEquals("NOTE: New may differ from load as no information can be extracted from the DataRow. \n" + message, expectedTypeForBindingAndNewIfDifferent ?? expectedType, typeDecider.GetTypeForNew());
			}
		}

		CusOutturn GetNewBusinessObjectForLoadTest()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			return outturnHeader.Outturns.AddNew();
		}
		Type DefaultType => typeof(CusOutturn);
	}
}
