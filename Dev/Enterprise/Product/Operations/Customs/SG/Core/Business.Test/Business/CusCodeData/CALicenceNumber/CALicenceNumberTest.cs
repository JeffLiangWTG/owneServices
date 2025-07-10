using CargoWise.EntityFramework;
using Enterprise.Customs.Common.SG;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CALicenceNumber))]
	public class CALicenceNumberTest : Customs.Business.Testing.CusCodeDataTest<CALicenceNumber>
	{
		public void TestSetDefaultValues()
		{
			CALicenceNumber cALicenceNumber = Factory.New<CALicenceNumber>();
			AssertEquals(CusCodeDataTypeList.Codes.CALicenceNumber, cALicenceNumber.CY_Type);
		}

		public void TestLicenceNumber()
		{
			CALicNo.CY_Data = "TestValue";
			AssertEquals("Licence Number", "TestValue", CusDocument.LicenceNumber);
		}

		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CALicenceNumber>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			return declaration.CALicences.AddNew();
		}

		#endregion
		#region Implementation
		ICusDocument CusDocument
		{
			get
			{
				return CALicNo;
			}
		}

		CALicenceNumber CALicNo
		{
			get
			{
				if (fCALicNo == null)
				{
					fCALicNo = Factory.New<CALicenceNumber>();
				}

				return fCALicNo;
			}
		}

		CALicenceNumber fCALicNo;
		#endregion
	}
}
