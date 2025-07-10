using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobRequiredDocumentHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetValueByAttributeName()
		{
			var master = Factory.New<JobRequiredDocument>();
			var attrib1 = master.Attributes.AddNew();
			attrib1.D0_AttribName = "BOB";
			attrib1.D0_AttribValue = "BOB val";
			var attrib2 = master.Attributes.AddNew();
			attrib2.D0_AttribName = "WENDY";
			attrib2.D0_AttribValue = "WENDY val";
			var attrib3 = master.Attributes.AddNew();
			attrib3.D0_AttribName = "SMITH";
			attrib3.D0_AttribValue = "SMITH val";
			NUnit.Framework.Assert.That(master.Attributes.GetValueByAttributeName("BOB"), NUnit.Framework.Is.EqualTo("BOB val").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(master.Attributes.GetValueByAttributeName("WENDY"), NUnit.Framework.Is.EqualTo("WENDY val").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(master.Attributes.GetValueByAttributeName("SMITH"), NUnit.Framework.Is.EqualTo("SMITH val").Using(CustomComparers.TypeComparison));
		}

		[TestDate(2019, 12, 26)]
		[ExpectNoExceptions]
		public void TestGetJobRequiredDocument()
		{
			var poaDoc = docs.AddNew();

			poaDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			poaDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			poaDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			poaDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			poaDoc.EQ_DocNumber = "111111";
			var poaCustomsDistrictDocAttr = poaDoc.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			poaCustomsDistrictDocAttr.D0_AttribValue = "A";
			var poaBoxNumberDocAttr = poaDoc.Attributes[JobRequiredDocAttribTypeList.Codes.BoxNumber];
			poaBoxNumberDocAttr.D0_AttribValue = "123";
			var bondedIdDocAttr = poaDoc.Attributes[JobRequiredDocAttribTypeList.Codes.BondedID];
			bondedIdDocAttr.D0_AttribValue = "446";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.PowerOfAttorney, "A", "123", ZDateTime.Today), NUnit.Framework.Is.EqualTo(default(JobRequiredDocument)));
				NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.PowerOfAttorney, "A", "123", ZDateTime.Today, "446").EQ_DocNumber, NUnit.Framework.Is.EqualTo("111111").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.PowerOfAttorney, "B", "123", ZDateTime.Today), NUnit.Framework.Is.EqualTo(default(JobRequiredDocument)));
				NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.PowerOfAttorney, "A", "121", ZDateTime.Today), NUnit.Framework.Is.EqualTo(default(JobRequiredDocument)));
			});
			var pocDoc = docs.AddNew();
			pocDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			pocDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			pocDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			pocDoc.EQ_ValidToDate = ZDateTime.Now.AddDays(-1);
			pocDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			pocDoc.EQ_DocNumber = "333333";
			var pocCustomsDistrictDocAttr = pocDoc.Attributes.AddNew();
			pocCustomsDistrictDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			pocCustomsDistrictDocAttr.D0_AttribValue = "A";
			var pocBoxNumberDocAttr = pocDoc.Attributes.AddNew();
			pocBoxNumberDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			pocBoxNumberDocAttr.D0_AttribValue = "123";
			NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, "A", "123", ZDateTime.Today), NUnit.Framework.Is.EqualTo(default(JobRequiredDocument)));
			var arnDoc = docs.AddNew();
			arnDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			arnDoc.EQ_DocType = Core.Constants.RefDocTypes.ArrivalNotice;
			arnDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			arnDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			arnDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			arnDoc.EQ_DocNumber = "222222";
			var arnCustomsDistrictAttr = arnDoc.Attributes.AddNew();
			arnCustomsDistrictAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			arnCustomsDistrictAttr.D0_AttribValue = "C";
			var arnBoxNumberAttr = arnDoc.Attributes.AddNew();
			arnBoxNumberAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			arnBoxNumberAttr.D0_AttribValue = "123";
			NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.ArrivalNotice, "C", "123", ZDateTime.Today).EQ_DocNumber, NUnit.Framework.Is.EqualTo("222222").Using(CustomComparers.TypeComparison));
			arnBoxNumberAttr.D0_AttribValue = "125";
			NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.ArrivalNotice, "C", "123", ZDateTime.Today), NUnit.Framework.Is.EqualTo(default(JobRequiredDocument)));
			NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.ArrivalNotice, "C", "125", ZDateTime.Today).EQ_DocNumber, NUnit.Framework.Is.EqualTo("222222").Using(CustomComparers.TypeComparison));
			var atdDoc = docs.AddNew();
			atdDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			atdDoc.EQ_DocType = Core.Constants.RefDocTypes.AuthorityToDeal;
			atdDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			atdDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			atdDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			atdDoc.EQ_DocNumber = "999999";
			NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.AuthorityToDeal, "A", "123", ZDateTime.Today), NUnit.Framework.Is.EqualTo(default(JobRequiredDocument)));
			var pofDoc = docs.AddNew();
			pofDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			pofDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			pofDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			pofDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			pofDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			pofDoc.EQ_DocNumber = "444444";
			var pofCustomsDistrictDocAttr = pofDoc.Attributes.AddNew();
			pofCustomsDistrictDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			pofCustomsDistrictDocAttr.D0_AttribValue = "A";
			var pofBoxNumberDocAttr = pofDoc.Attributes.AddNew();
			pofBoxNumberDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			pofBoxNumberDocAttr.D0_AttribValue = "123";
			NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.PowerOfAttorneyForwarding, "A", "123", ZDateTime.Today), NUnit.Framework.Is.EqualTo(default(JobRequiredDocument)));
			var pkdDoc = docs.AddNew();
			pkdDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			pkdDoc.EQ_DocType = Core.Constants.RefDocTypes.PackingDeclaration;
			pkdDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			pkdDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			pkdDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			pkdDoc.EQ_DocNumber = "555555";
			var pkdCustomsDistrictDocAttr = pkdDoc.Attributes.AddNew();
			pkdCustomsDistrictDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			pkdCustomsDistrictDocAttr.D0_AttribValue = "A";
			var pkdBoxNumberDocAttr = pkdDoc.Attributes.AddNew();
			pkdBoxNumberDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			pkdBoxNumberDocAttr.D0_AttribValue = "123";
			NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.PackingDeclaration, "A", "123", ZDateTime.Today), NUnit.Framework.Is.EqualTo(default(JobRequiredDocument)));
			var pklDoc = docs.AddNew();
			pklDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.Accounting;
			pklDoc.EQ_DocType = Core.Constants.RefDocTypes.PackingList;
			pklDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			pklDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			pklDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			pklDoc.EQ_DocNumber = "666666";
			var pklCustomsDistrictDocAttr = pklDoc.Attributes.AddNew();
			pklCustomsDistrictDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			pklCustomsDistrictDocAttr.D0_AttribValue = "A";
			var pklBoxNumberDocAttr = pklDoc.Attributes.AddNew();
			pklBoxNumberDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			pklBoxNumberDocAttr.D0_AttribValue = "123";
			NUnit.Framework.Assert.That(docs.GetJobRequiredDocument(Core.Constants.RefDocTypes.PackingList, "A", "123", ZDateTime.Today), NUnit.Framework.Is.EqualTo(default(JobRequiredDocument)));
		}

		IDocsAndCartageParent shipment;
		JobRequiredDocumentDependentCollection docs;
		protected override void SetUp()
		{
			base.SetUp();
			shipment = (IDocsAndCartageParent)Factory.New<Integration.Forwarding.IForwardingShipment>();
			docs = shipment.RequiredDocumentsProvider.RequiredDocuments;
		}
	}
}
