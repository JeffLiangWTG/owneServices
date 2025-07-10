using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DOTVIN))]
	public class DOTVINTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DOTVIN>
	{
		public void TestHumanReadableNameCore()
		{
			AssertEquals("DOT Vehicle Details", DOTVIN.HumanReadableName);
		}

		public void TestIDOTVIN()
		{
			DOTVIN.US_DOTMake = "MAKE";
			AssertEquals("MAKE", iDOTVIN.MakeOfVehicle);

			DOTVIN.US_DOTModel = "MODEL";
			AssertEquals("MODEL", iDOTVIN.Model);

			DOTVIN.US_DOTYear = 1999;
			AssertEquals(1999, iDOTVIN.Year);

			DOTVIN.US_DOTVIN = "VIN";
			AssertEquals("VIN", iDOTVIN.VehicleIdentificationNumber);

			DOTVIN.US_DOTVEN = "VEN";
			AssertEquals("VEN", iDOTVIN.VehicleEligibilityNumber);

			DOTVIN.US_DOTRINo = "RINO";
			AssertEquals("RINO", iDOTVIN.NHTSARegisteredImporterRINumber);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return DOTVIN;
		}

		protected override IEnumerable<DOTVIN> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().DOTs.AddNew().DOTVINs.AddNew();
		}

		#endregion

		#region Implementation

		IDOTVIN iDOTVIN
		{
			get { return DOTVIN; }
		}

		DOTVIN DOTVIN
		{
			get { return dotvin ?? (dotvin = DOT.DOTVINs.AddNew()); }
		}
		DOTVIN dotvin;

		DOT DOT
		{
			get { return dot ?? (dot = Factory.New<DOT>()); }
		}
		DOT dot;

		#endregion
	}
}
