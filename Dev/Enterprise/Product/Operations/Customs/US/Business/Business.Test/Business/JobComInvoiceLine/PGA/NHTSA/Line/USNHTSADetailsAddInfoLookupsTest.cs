using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	public class USNHTSADetailsAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMonthList()
		{
			AssertNotNull(Lookups.MonthList);
		}

		public void TestNumberTypes()
		{
			AssertNotNull(Lookups.NumberTypes);
		}

		public void TestCategoryCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var header = invoiceLine.NHTSALines.AddNew();
			var nhtsaDetail = header.NHTSADetails.AddNew();

			void AssertCategoryCodes<T>(string program) where T : CodeDescriptionPairList
			{
				header.US_NHTProgramCode = program;

				var message = $"Should return {typeof(T).Name} when the program code is {program}.";
				var expectedList = Factory.GetCachedValue<CodeDescriptionPairList>($"ImportCodeList{program}TYP", () => System.Activator.CreateInstance<T>());

				AssertSame(message, expectedList, nhtsaDetail.AddInfoLookups.CategoryCodes);
			}

			CombineAssertions(() =>
			{
				AssertCategoryCodes<NHTSACategoryCode_MVSTYPList>("MVS");
				AssertCategoryCodes<NHTSACategoryCode_REITYPList>("REI");
				AssertCategoryCodes<NHTSACategoryCode_TPETYPList>("TPE");
				AssertCategoryCodes<NHTSACategoryCode_OEITYPList>("OEI");
				AssertCategoryCodes<NHTSACategoryCode_OFFTYPList>("OFF");
			});
		}

		public void TestDriveSides()
		{
			AssertNotNull(Lookups.DriveSides);
		}

		public void TestLPCOTypes()
		{
			AssertNotNull(Lookups.LPCOTypes);
		}

		public void TestLPCODateTypes()
		{
			AssertNotNull(Lookups.LPCODateTypes);
		}

		#region Implementation

		USNHTSADetailsAddInfoLookups Lookups
		{
			get { return Details.AddInfoLookups; }
		}

		NHTSADetails Details
		{
			get { return fDetails ?? (fDetails = Factory.New<NHTSADetails>()); }
		}
		NHTSADetails fDetails;

		#endregion
	}
}
