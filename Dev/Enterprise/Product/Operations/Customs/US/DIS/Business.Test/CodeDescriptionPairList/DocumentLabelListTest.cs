using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class DocumentLabelListTest : TestCase
	{
		public void TestGetOptionalDataTypesNewCodes()
		{
			var factory = new BusinessObjectFactory();
			var cusCode = factory.New<ZZRefCusCodeListCombined>();
			var bondAttrib = cusCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.OptionalData, "BND");
			AssertEquals(OptionalDataTypes.BondData, DocumentLabelList.GetOptionalDataTypes(cusCode));
			cusCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.OptionalData, "CER");
			AssertEquals(OptionalDataTypes.BondData | OptionalDataTypes.Certificate, DocumentLabelList.GetOptionalDataTypes(cusCode));
			AssertHasOptionalData(cusCode, "CER", OptionalDataTypes.Certificate);
			AssertHasOptionalData(cusCode, "COM", OptionalDataTypes.Commodity);
			AssertHasOptionalData(cusCode, "INV", OptionalDataTypes.Invoice);
			AssertHasOptionalData(cusCode, "PCK", OptionalDataTypes.PackingList);
			AssertHasOptionalData(cusCode, "PER", OptionalDataTypes.Permit);
			AssertHasOptionalData(cusCode, "TOX", OptionalDataTypes.ToxicSubstance);
		}

		public void TestIsOptionalDataVisible()
		{
			Assert(DocumentLabelList.IsOptionalDataVisible(OptionalDataTypes.Certificate | OptionalDataTypes.Commodity, OptionalDataTypes.Certificate));
			Assert(DocumentLabelList.IsOptionalDataVisible(OptionalDataTypes.Certificate | OptionalDataTypes.Commodity, OptionalDataTypes.Commodity));
			Assert(!DocumentLabelList.IsOptionalDataVisible(OptionalDataTypes.Certificate | OptionalDataTypes.Commodity, OptionalDataTypes.Invoice));
		}

		public void TestTryToGetDefaultFormType()
		{
			AssertEquals("CBP02", DocumentLabelList.TryToGetDefaultFormType(Core.Constants.RefDocTypes.CommercialInvoice));
			AssertEquals("CBP02", DocumentLabelList.TryToGetDefaultFormType(Core.Constants.RefDocTypes.Invoice));
			AssertEquals("CBP01", DocumentLabelList.TryToGetDefaultFormType(Core.Constants.RefDocTypes.PackingList));
			AssertEquals("CBP11", DocumentLabelList.TryToGetDefaultFormType(Core.Constants.RefDocTypes.MasterBill));
			AssertEquals(string.Empty, DocumentLabelList.TryToGetDefaultFormType(Core.Constants.RefDocTypes.CertificateOfOrigin));
			AssertEquals(string.Empty, DocumentLabelList.TryToGetDefaultFormType(Core.Constants.RefDocTypes.ArrivalNotice));
		}

		public void TestIsNMFSForms()
		{
			AssertEquals(true, DocumentLabelList.IsNMFSForms("NMF01"));
			AssertEquals(true, DocumentLabelList.IsNMFSForms("NMF02"));
			AssertEquals(true, DocumentLabelList.IsNMFSForms("NMF03"));
			AssertEquals(true, DocumentLabelList.IsNMFSForms("NMF04"));
			AssertEquals(true, DocumentLabelList.IsNMFSForms("NMF05"));
			AssertEquals(true, DocumentLabelList.IsNMFSForms("NMF06"));
			AssertEquals(true, DocumentLabelList.IsNMFSForms("NMF07"));
			AssertEquals(true, DocumentLabelList.IsNMFSForms("NMF08"));
			AssertEquals(true, DocumentLabelList.IsNMFSForms("NMF09"));
			AssertEquals(true, DocumentLabelList.IsNMFSForms("NMF10"));
			AssertEquals(false, DocumentLabelList.IsNMFSForms("CBP01"));
		}

		void AssertHasOptionalData(ZZRefCusCodeListCombined cusCode, string code, OptionalDataTypes value)
		{
			cusCode.Attributes.RemoveAndDeleteAll();
			cusCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.OptionalData, code);
			AssertEquals(value, DocumentLabelList.GetOptionalDataTypes(cusCode));
		}
	}
}
