using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAESLicenseCodeTest : TestCaseWithFactory
	{
		public void TestIsDDTCDataRequired()
		{
			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94,
					USAESLicenseCode.Codes.VDS
				};

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString code in requireDDTCDataList)
			{
				AssertEquals(code, true, USAESLicenseCode.IsDDTCDataRequired(code));
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					AssertEquals(code.Code, false, USAESLicenseCode.IsDDTCDataRequired(code.Code));
				}
			}
		}

		public void TestIsDDTCITARExemptionRequired()
		{
			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00
				};

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString code in requireDDTCDataList)
			{
				AssertEquals(code, true, USAESLicenseCode.IsDDTCITARExemptionRequired(code));
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					AssertEquals(code.Code, false, USAESLicenseCode.IsDDTCITARExemptionRequired(code.Code));
				}
			}
		}

		public void TestIsDDTCPartyCertIndicatorRequired()
		{
			ZString[] requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S00
				};

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString code in requireDDTCDataList)
			{
				AssertEquals(code, true, USAESLicenseCode.IsDDTCPartyCertIndicatorRequired(code));
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					AssertEquals(code.Code, false, USAESLicenseCode.IsDDTCPartyCertIndicatorRequired(code.Code));
				}
			}
		}

		public void TestIsLicenseValueRequired()
		{
			var requiredList = new ZString[]
				{
					USAESLicenseCode.Codes.C30,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94
				};

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (var code in requiredList)
			{
				AssertEquals(code, true, USAESLicenseCode.IsLicenseValueRequired(code, Factory, ZDateTime.Today));
			}

			foreach (ICodeDescription code in list)
			{
				if (!requiredList.Contains(code.Code))
				{
					AssertEquals(code.Code, false, USAESLicenseCode.IsLicenseValueRequired(code.Code, Factory, ZDateTime.Today));
				}
			}
		}

		public void TestIsDDTCRegistrationNumberRequired()
		{
			var requireDDTCDataList = new ZString[]
				{
					USAESLicenseCode.Codes.SAG,
					USAESLicenseCode.Codes.SAU,
					USAESLicenseCode.Codes.SCA,
					USAESLicenseCode.Codes.SGB,
					USAESLicenseCode.Codes.S05,
					USAESLicenseCode.Codes.S61,
					USAESLicenseCode.Codes.S73,
					USAESLicenseCode.Codes.S85,
					USAESLicenseCode.Codes.S94,
					USAESLicenseCode.Codes.VDS
				};

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString code in requireDDTCDataList)
			{
				Assert($"DDTC registry number is mandatory for '{code}'", USAESLicenseCode.IsDDTCRegistrationNumberRequired(code));
			}

			foreach (ICodeDescription code in list)
			{
				if (!requireDDTCDataList.Contains(code.Code))
				{
					Assert($"DDTC registration number is NOT mandatory for '{code.Code}'", !USAESLicenseCode.IsDDTCRegistrationNumberRequired(code.Code));
				}
			}
		}

		public void TestIsDDTCRegistrationNumberAllowed()
		{
			var allowedDDTCDataList = new ZString[]
			{
				USAESLicenseCode.Codes.S00
			};

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (ZString code in allowedDDTCDataList)
			{
				Assert($"DDTC registration number is allowed for '{code}'", USAESLicenseCode.IsDDTCRegistrationNumberAllowed(code));
			}

			foreach (ICodeDescription code in list)
			{
				if (!allowedDDTCDataList.Contains(code.Code))
				{
					Assert($"DDTC registration number is NOT allowed for {code.Code}", !USAESLicenseCode.IsDDTCRegistrationNumberAllowed(code.Code));
				}
			}
		}

		public void TestIs600SeriesDotYECCN()
		{
			var requiredList = new ZString[]
			{
				USAESLicenseCode.Codes.C32,
				USAESLicenseCode.Codes.C33
			};

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			foreach (var code in requiredList)
			{
				AssertEquals(code, true, USAESLicenseCode.Is600SeriesDotYECCN(code));
			}

			foreach (ICodeDescription code in list)
			{
				if (!requiredList.Contains(code.Code))
				{
					AssertEquals(code.Code, false, USAESLicenseCode.Is600SeriesDotYECCN(code.Code));
				}
			}
		}

		public void TestNoExceptionCreatedWithInvalidDate()
		{
			var requiredList = new ZString[]
				{
					USAESLicenseCode.Codes.C30
				};

			var list = new USAESLicenseCodeCollection(Factory);
			list.Load();
			AssertEquals("No exception thrown", false, USAESLicenseCode.IsLicenseValueRequired(USAESLicenseCode.Codes.C30, Factory, ZDateTime.Invalid));
		}

		protected override void SetUp()
		{
			base.SetUp();
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C31, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C35, USAESLicenseCode.Codes.C38, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C45,
				USAESLicenseCode.Codes.C46, USAESLicenseCode.Codes.C53, USAESLicenseCode.Codes.C54, USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.C63,
				USAESLicenseCode.Codes.E01, USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00, USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61,
				USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94, USAESLicenseCode.Codes.VDS });
		}
	}
}
