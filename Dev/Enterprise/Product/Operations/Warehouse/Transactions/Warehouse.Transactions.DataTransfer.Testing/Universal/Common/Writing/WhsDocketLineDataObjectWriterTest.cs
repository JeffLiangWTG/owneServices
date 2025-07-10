using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsDocketLineDataObjectWriterTest<TDocketLine, TWriter> : WhsUniversalTestCase
		where TDocketLine : WhsDocketLine
		where TWriter : WhsDocketLineDataObjectWriter<TDocketLine>
	{
		#region TestCustomsData

		public void TestCustomsData()
		{
			var docketLine = GetNewDocketLine();
			EnableCustomsTransactions(docketLine);
			WhsBondedWarehouseAttributeDataObjectWriterTest.GetCustomsData(docketLine);

			var writer = GetNewDataObjectWriter(docketLine.Docket);
			var docketLineData = writer.GetDataObject(docketLine);
			AssertCustomsData(docketLineData.CustomsData);
		}

		protected virtual void AssertCustomsData(CustomsEntryInfo customsData)
		{
			WhsBondedWarehouseAttributeDataObjectWriterTest.AssertContents(customsData);
		}

		protected virtual void EnableCustomsTransactions(TDocketLine docketLine)
		{
			docketLine.Docket.WD_DocketSubType = "CUS";
		}

		#endregion

		#region TestCustomsDataIsNotCreatedIfOrderLineHasNoCustomsData

		public void TestCustomsDataIsNotCreatedIfOrderLineHasNoCustomsData()
		{
			if (!IsCustomsTransactionByDefault)
			{
				var docketLine = GetNewDocketLine();
				var writer = GetNewDataObjectWriter(docketLine.Docket);
				var docketLineData = writer.GetDataObject(docketLine);

				AssertNotNull("docketLineData", docketLineData);
				AssertNull("docketLineData.CustomsData", docketLineData.CustomsData);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool IsCustomsTransactionByDefault => false;

		#endregion

		#region TestOrganisationLevelCustomFieldsAreExported

		public void TestOrganisationLevelCustomFieldsAreExported()
		{
			if (IsCustomFieldsSupported)
			{
				var docketLine = GetNewDocketLine();
				var docket = docketLine.Docket;
				Data.SetupCustomLabels(docket);
				Data.AddCustomFieldsToBizO(new WhsDocketLine.CustomLabelsProvider(docket), docketLine);

				var writer = GetNewDataObjectWriter(docket);
				var docketLineDataObject = writer.GetDataObject(docketLine);
				AssertNotNull("Precondition: docketLineDataObject", docketLineDataObject);

				var customFields = docketLineDataObject.CustomizedFieldCollection;
				AssertNotNull(customFields);

				CombineAssertions(delegate
				{
					AssertEquals("customFields.Count", 6, customFields.Count);
					AssertCustomFieldsAreExported(customFields);
				});
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool IsCustomFieldsSupported => true;

		#endregion

		#region TestUNDGCollection

		public void TestUNDGCollection()
		{
			var docketLine = GetNewDocketLine();
			var product = docketLine.SupplierPart;
			AssertEquals("Precondition: No UNDGs", 0, product.UNDGs.Count);

			var contact = docketLine.Docket.Client.Contacts.AddNew();
			contact.OC_ContactName = "JohnSmith";
			contact.OC_Phone = "123456789";

			var undg1 = product.UNDGs.AddNew();
			var substance = product.Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "3000";
			substance.DG_Variant = "c";
			substance.DG_FlashPoint = "100 C";
			substance.DG_Class = "Clas";
			substance.DG_PG = "Gr1";
			substance.DG_PSN = "Name1";
			substance.DG_TechName = "T";
			substance.DG_MP = "Y";
			substance.DG_SubLabel1 = "TEST";
			substance.DG_SubLabel2 = "AAA";

			undg1.DI_OC_DGContact = contact.PK;
			undg1.DI_DG = substance.PK;
			undg1.DI_DGFlashPoint = 0.1m;
			undg1.DI_DGVolume = 1m;
			undg1.DI_DGWeight = 2m;
			undg1.DI_MPMarinePollutant = "Y";
			undg1.DI_UnitOfWeight = "kg";
			undg1.DI_UnitOfVolume = "m3";
			undg1.DI_TechnicalName = "Tech1";
			undg1.DI_IsLimitedQuantity = true;

			var undg2 = product.UNDGs.AddNew();
			undg2.DI_DG = substance.PK;
			undg2.DI_DGVolume = 2m;

			var writer = GetNewDataObjectWriter(docketLine.Docket);
			var docketLineDataObject = writer.GetDataObject(docketLine);
			AssertNotNull("Precondition: docketLineDataObject", docketLineDataObject);
			AssertNotNull("Precondition: Should export if UNDGs", docketLineDataObject.UNDGCollection);
			AssertEquals("docketLineDataObject.UNDGCollection count", 2, docketLineDataObject.UNDGCollection.Count);

			var undgDO1 = docketLineDataObject.UNDGCollection[0];
			CombineAssertions(() =>
			{
				AssertEquals("Contact.FullName", "JohnSmith", undgDO1.Contact.FullName);
				AssertEquals("Contact.Phone", "123456789", undgDO1.Contact.Phone);
				AssertEquals("FlashPoint", "0.1", undgDO1.FlashPoint);
				AssertEquals("IMOClass", "Clas", undgDO1.IMOClass);
				AssertEquals("MarinePollutant.Code", "Y", undgDO1.MarinePollutant.Code);
				AssertEquals("PackedInLimitedQuantity", true, undgDO1.PackedInLimitedQuantity);
				AssertEquals("PackingGroup", "Gr1", undgDO1.PackingGroup);
				AssertEquals("ProperShippingName", "Name1", undgDO1.ProperShippingName);
				AssertEquals("TechicalName", "Tech1", undgDO1.TechicalName);
				AssertEquals("UNDGCode", "3000c", undgDO1.UNDGCode);
				AssertEquals("Volume", 1m, undgDO1.Volume);
				AssertEquals("Weight", 2m, undgDO1.Weight);
				AssertEquals("WeightUQ", "kg", undgDO1.WeightUQ.Code);
				AssertEquals("VolumeUQ", "m3", undgDO1.VolumeUQ.Code);
				AssertEquals("SubLabel1", "TEST", undgDO1.SubLabel1);
				AssertEquals("SubLabel2", "AAA", undgDO1.SubLabel2);
			});

			var undgDO2 = docketLineDataObject.UNDGCollection[1];
			CombineAssertions(() =>
			{
				AssertEquals("IMOClass", "Clas", undgDO2.IMOClass);
				AssertEquals("Volume", 2m, undgDO2.Volume);
			});
		}

		#endregion

		#region TestUNDGCollection_Empty

		public void TestUNDGCollection_Empty()
		{
			var docketLine = GetNewDocketLine();
			var product = docketLine.SupplierPart;
			AssertEquals("Precondition: No UNDGs", 0, product.UNDGs.Count);

			var writer = GetNewDataObjectWriter(docketLine.Docket);
			var docketLineDataObject = writer.GetDataObject(docketLine);
			AssertNotNull("Precondition: docketLineDataObject", docketLineDataObject);

			AssertNull("Should not export if no UNDGs", docketLineDataObject.UNDGCollection);
		}

		#endregion

		#region Implementation

		protected abstract TDocketLine GetNewDocketLine();
		protected abstract TWriter GetNewDataObjectWriter(BusinessObject topLevelBO);

		#endregion
	}
}
