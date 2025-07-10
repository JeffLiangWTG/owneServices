using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class AdditionalLineTariffDetailDataObjectWriterTest : TestCaseWithUniversalObjectFactory
	{
		public void TestPopulateDataObject()
		{
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "22A");
			var testInput = (CusLineTariffDetail)Factory.BOFactory.New<Integration.Customs.ZA.ICusLineTariffDetail>();
			testInput.BZ_Type = "22A";
			testInput.BZ_Tariff = "TRF";
			testInput.BZ_Value = 500;
			testInput.BZ_Qty1 = 20;
			testInput.BZ_UQ1 = "CT";

			var writer = new AdditionalLineTariffDetailDataObjectWriter(new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>())));
			var result = writer.GetDataObject(testInput);

			CombineAssertions(() =>
			{
				AssertEquals("22A", result.Type.Code.Value);
				AssertEquals("22A DESC", result.Type.Description.Value);
				AssertEquals("TRF", result.Tariff.Value);
				AssertEquals(500m, result.Value.Value);
				AssertEquals(20m, result.CustomsQuantity.Value);
				AssertEquals("CT", result.CustomsQuantityUnit.Code);
				AssertEquals("Carat", result.CustomsQuantityUnit.Description);
			});

			var testInput2 = (CusLineTariffDetail)Factory.BOFactory.New<CusLineTariffDetailForTest>();
			testInput2.BZ_Type = "22A";
			testInput2.BZ_Tariff = "TRF";
			testInput2.BZ_Value = 500;
			testInput2.BZ_Qty1 = 20;
			testInput2.BZ_UQ1 = "KG";

			var writer2 = new AdditionalLineTariffDetailDataObjectWriter(new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>())));
			var result2 = writer2.GetDataObject(testInput2);

			CombineAssertions(() =>
			{
				AssertEquals("KG", result2.CustomsQuantityUnit.Code);
				AssertNull(result2.CustomsQuantityUnit.Description);
			});
		}

		public void TestPopulateAddInfoCollection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var cusLineTariff = invoiceLine.CusLineTariffDetails.AddNew();
				cusLineTariff.BZ_NAddInfo = $"{BRCusLineTariffDetailSchema.Constants.BZ_LegalActSubject.Substring(3)}=1";

				var writer = new AdditionalLineTariffDetailDataObjectWriter(new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>())));
				var result = writer.GetDataObject(cusLineTariff);

				CombineAssertions(() =>
				{
					AssertEquals("AddInfoCollection.Count", 1, result.AddInfoCollection.Count);
					AssertEquals("AddInfo.Key", "LegalActSubject", result.AddInfoCollection[0].Key);
					AssertEquals("AddInfo.Value", "1", result.AddInfoCollection[0].Value);
				});
			}
		}

		sealed class CusLineTariffDetailForTest : CusLineTariffDetail
		{
			public CusLineTariffDetailForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new CusLineTariffDetailLookupsForTest Lookups => (CusLineTariffDetailLookupsForTest)base.Lookups;

			protected override CusLineTariffDetailLookups GetNewLookups() => new CusLineTariffDetailLookupsForTest(this);
		}

		sealed class CusLineTariffDetailLookupsForTest : CusLineTariffDetailLookups
		{
			public CusLineTariffDetailLookupsForTest(CusLineTariffDetailForTest parent)
				: base(parent)
			{
			}

			public new CusLineTariffDetailForTest Parent => (CusLineTariffDetailForTest)base.Parent;

			public override ICollection QuantityUnitList => new List<string>() { "KG" };
		}
	}
}
