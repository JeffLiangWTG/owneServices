using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Integration.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DOT))]
	public class DOTTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DOT>
	{
		public void TestDOT()
		{
			AssertEquals("DOT", DOT.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().DOTs.AddNew();
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			InvoiceLine.JI_JZ = invoice.PK;
			ICusAddInfoTypeSupporter supporter = DOT;
			supporter.AssertType(typeof(DOTVIN), CusAddInfoTypeAttribute.Codes.USDOTVIN);
			supporter.AssertType(null, "ZZ!");

			var vin = DOT.DOTVINs.AddNew();
			vin.US_DOTMake = "1";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var addInfo = newFactory.Load<CusAddInfo>(vin.PK);
			AssertEquals(typeof(DOTVIN), addInfo.GetType());
		}

		public void TestUS_DOTBoxNo_Defaults()
		{
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, DOT.US_DOTCountryOfOrigin);

			DOT.US_DOTCountryOfOrigin = "";
			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._11;
			AssertEquals("", DOT.US_DOTCountryOfOrigin);
		}

		public void TestIDOT()
		{
			DOT.US_DOTCommercialDesc = "TEST";
			AssertEquals(DOT.US_DOTCommercialDesc, iDOT.CommercialDescription);

			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._03;
			AssertEquals(DOT.US_DOTBoxNo, iDOT.BoxNumber);

			AssertEquals("Y", iDOT.BoxCertification);

			DOT.US_DOTPassport = "1234";
			AssertEquals(DOT.US_DOTPassport, iDOT.PassportNumber);

			DOT.US_DOTCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals(DOT.US_DOTCountryOfOrigin, iDOT.CountryISO);

			DOT.US_DOTBondSuretyCode = "999";
			AssertEquals("999", iDOT.DOTBondSuretyCode);

			DOT.US_DOTPriorApproval = true;
			AssertEquals(DOT.US_DOTPriorApproval, iDOT.NHTSAPermissionLetterOfficialOrdersCertification);

			DOT.US_DOTImpSubstStatement = true;
			AssertEquals(DOT.US_DOTPriorApproval, iDOT.ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter);

			DOT.US_DOTClarCode = ClarificationCodeList.Codes.Equipment;
			AssertEquals(DOT.US_DOTClarCode, iDOT.ClarificationCode);

			DOT.US_DOTTireID = "ASD";
			AssertEquals(DOT.US_DOTTireID, iDOT.TireManufacturerIDCode);

			DOT.US_DOTTireBrandName = "TEST";
			AssertEquals(DOT.US_DOTTireBrandName, iDOT.TireManufacturerBrandName);

			DOT.DOTVINs.AddNew();
			DOT.DOTVINs.AddNew();
			DOT.DOTVINs.AddNew();

			int i = 0;
			foreach (IDOTVIN idotvin in iDOT.VINs)
			{
				i++;
			}
			AssertEquals(3, i);
		}

		public void TestDOTVINs()
		{
			DOT.DOTVINs.AddNew();
			Factory.Save();
			AssertEquals(false, DOT.HasChanges);
			var dotVins = DOT.DOTVINs.AddNew();
			dotVins.US_DOTMake = "ZS";
			AssertEquals(true, DOT.HasChanges);
		}

		public void TestClone()
		{
			DOT.DOTVINs.AddNew();
			DOT.DOTVINs.AddNew();

			DOT newDot = (DOT)DOT.Clone();

			AssertEquals(2, newDot.DOTVINs.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (DOT)DOT.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(DOT), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.DOTVINs[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", DOT.Factory.GetHashCode(), newFacClone.DOTVINs[0].Factory.GetHashCode());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return DOT;
		}

		protected override IEnumerable<DOT> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().DOTs.AddNew();
		}

		IDOT iDOT
		{
			get { return DOT; }
		}

		DOT DOT
		{
			get { return dot ?? (dot = InvoiceLine.DOTs.AddNew()); }
		}
		DOT dot;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var dec = Factory.New<JobDeclaration>();
					var invoice = dec.Invoices.AddNew();
					invoiceLine = invoice.InvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
