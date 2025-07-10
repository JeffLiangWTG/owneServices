using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(LaceyCountry))]
	public class LaceyCountryTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<LaceyCountry>
	{
		public void TestUS_CountryCode()
		{
			LaceyCountry.US_CountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var laceyReload = newFactory.Load<LaceyCountry>(LaceyCountry.PK);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, laceyReload.US_CountryCode);
		}

		#region Implementation

		LaceyCountry LaceyCountry
		{
			get
			{
				if (laceyCountry == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var pga = invoiceLine.LaceyActLines.AddNew();
					laceyCountry = pga.LaceyCountries.AddNew();
				}
				return laceyCountry;
			}
		}
		LaceyCountry laceyCountry;

		protected override BusinessObject GetNewBusinessObject()
		{
			return LaceyCountry;
		}

		protected override IEnumerable<LaceyCountry> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var pga = invoiceLine.LaceyActLines.AddNew();
			yield return pga.LaceyCountries.AddNew();
		}

		#endregion
	}
}
