using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVItemLine))]
	class HVLVItemLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIInvoicTestGivenItemLineHasNoShipmentOrBookingHeader_ThenAccessInvoiceLinePartDetailSupplierDoesNotThrow()
		{
			var header = Factory.NewWithValidTestData<HVLVConsignmentHeader>();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_HCH_Header = header.PK;
			consignment.HVC_ClusterKey = header.HCH_ClusterKey = 1;

			var itemLine = consignment.Items.AddNew().Lines.AddNew();
			itemLine.HVS_Quantity = 1;

			itemLine.ParentItem.HVI_JS_LoadedOnShipment = ZGuid.Empty;
			itemLine.ParentItem.Consignment.HVC_HVH_BookingHeader = ZGuid.Empty;

			var invoiceLinePartDetail = itemLine as IInvoiceLinePartDetails;

			AssertNoExceptionThrown(() => _ = invoiceLinePartDetail.Supplier);
		}

		public void TestIInvoiceLinePartDetailsMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.PuertoRico))
			{
				var item = Factory.New<HVLVItemLine>();
				IInvoiceLinePartDetails partDetails = item;
				AssertEquals(CountryCodes.UnitedStates, partDetails.CustomsCountryCode);
				AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgSupplierPart>(), partDetails.TypeOfPartUsed);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var item = Factory.New<HVLVItemLine>();
				IInvoiceLinePartDetails partDetails = item;
				AssertEquals(CountryCodes.Australia, partDetails.CustomsCountryCode);
				AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IOrgSupplierPart>(), partDetails.TypeOfPartUsed);
			}
		}

		public void TestSetTariffValue_OriginHSCode()
		{
			var itemLine = Factory.New<HVLVItemLine>();
			foreach (var countryCode in SupportHVLVCountrys)
			{
				itemLine.HVS_RN_NKOriginCountryCode = countryCode;
				itemLine.HVS_FormattedOriginTariff = "1234.56.78";
				var expectOriginTariff = "12345678";
				AssertEquals(expectOriginTariff, itemLine.HVS_OriginTariff);
			}
		}

		public void TestSetTariffValue_DestinationHSCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var itemLine = item.Lines.AddNew();

			foreach (var countryCode in SupportHVLVCountrys)
			{
				shipment.JS_RL_NKDestination = countryCode;
				itemLine.HVS_FormattedDestinationTariff = "1234.56.78";
				var expectDestiantionTariff = "12345678";
				AssertEquals(expectDestiantionTariff, itemLine.HVS_DestinationTariff);
			}
		}

		public void TestSetDestinationHSCode_OnlyKeepsNumericCharacters()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var itemLine = item.Lines.AddNew();

			foreach (var countryCode in SupportHVLVCountrys)
			{
				shipment.JS_RL_NKDestination = countryCode;
				itemLine.HVS_FormattedDestinationTariff = "1234.56.78";
				AssertEquals("12345678", itemLine.HVS_DestinationTariff);
			}
		}

		public void TestSetOriginHSCode_OnlyKeepsNumericCharacters()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var itemLine = item.Lines.AddNew();

			foreach (var countryCode in SupportHVLVCountrys)
			{
				shipment.JS_RL_NKDestination = countryCode;
				itemLine.HVS_FormattedOriginTariff = "1234.56.78";
				AssertEquals("12345678", itemLine.HVS_OriginTariff);
			}
		}

		public void TestSettingGrossWeightUnitRecalculatesItemManifestedWeight()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = "KG";
			var item = consignment.Items.AddNew();

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_GrossWeight = 123;
			itemLine.HVS_WeightUnit = "KG";
			AssertEquals("Consignment weight was used for conversion", 123M, item.HVI_ManifestedWeight);

			itemLine.HVS_WeightUnit = "G";
			AssertEquals("Item manifested weight recalculated", 0.123M, item.HVI_ManifestedWeight);
		}

		public void TestSettingGrossWeightUnit_DoNotRecalculatesItemManifestedWeight_WhenGrossWeightIsZero()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = "KG";
			var item = consignment.Items.AddNew();

			item.HVI_ManifestedWeight = 10;

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_GrossWeight = 0;
			itemLine.HVS_WeightUnit = "KG";

			AssertEquals(10m, item.HVI_ManifestedWeight);

			itemLine.HVS_WeightUnit = "G";
			AssertEquals(10m, item.HVI_ManifestedWeight);
		}

		public void TestUpdateGrossWeightRecalculatesItemManifestedWeight_WhenGrossWeightGreaterThanZero()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = "KG";
			var item = consignment.Items.AddNew();

			item.HVI_ManifestedWeight = 10;

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_GrossWeight = 0;
			itemLine.HVS_WeightUnit = "KG";

			AssertEquals("Would not change HVI_ManifestedWeight when HVS_GrossWeight is 0", 10m, item.HVI_ManifestedWeight);

			itemLine.HVS_GrossWeight = 5;
			AssertEquals("Would change HVI_ManifestedWeight when HVS_GrossWeight is greater than 0", 5m, item.HVI_ManifestedWeight);

			itemLine.HVS_GrossWeight = 0;
			AssertEquals("Would change HVI_ManifestedWeight when HVS_GrossWeight original value is not 0", 0m, item.HVI_ManifestedWeight);
		}

		public void TestUnitOfMeasureProperties_WhenSetValueLowerCase_GetIsUpperCase()
		{
			var itemLine = Factory.New<HVLVItemLine>();
			itemLine.HVS_WeightUnit = "kg";

			AssertEquals("KG", itemLine.HVS_WeightUnit);
		}

		public void TestClusterKeyIsCascadedDownFromHVLVBooking()
		{
			var booking = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = booking.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var itemLine = item.Lines.AddNew();

			itemLine.HVS_OriginTariff = "123456";
			itemLine.HVS_RN_NKOriginCountryCode = "AU";
			itemLine.HVS_CustomsValue = 100m;
			itemLine.HVS_DestinationTariff = "HS123456";
			itemLine.HVS_GoodsDescription = "This is something for description.";
			itemLine.HVS_OriginGoodsDescription = "This is something for origin description.";
			itemLine.HVS_GrossWeight = 2.1m;
			itemLine.HVS_IntrinsicValue = 100.2m;
			itemLine.HVS_ItemURL = "http://www.amazon.com/item/123456";
			itemLine.HVS_NetWeight = 2.0m;
			itemLine.HVS_ProductCode = "PROD1234";
			itemLine.HVS_Quantity = 3;
			itemLine.HVS_WeightUnit = "KG";

			Factory.Save();

			CombineAssertions(delegate
			{
				AssertEquals(booking.HVH_ClusterKey, consignment.HVC_ClusterKey);
				AssertEquals(consignment.HVC_ClusterKey, item.HVI_ClusterKey);
				AssertEquals(item.HVI_ClusterKey, itemLine.HVS_ClusterKey);
			});
		}

		public void TestDefaultFilledWithProductInfo()
		{
			var itemLine1 = CreateHVLVItemLineTestObject();
			var itemLine2 = CreateHVLVItemLineTestObject();

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "PROD001";
			product1.OP_Desc = "test product description here.";
			product1.OP_NetWeight = 5.0m;
			product1.OP_WeightUQ = "KG";
			product1.OP_Weight = 5.1m;

			var relation1 = product1.RelatedOrganisations.AddNew();
			relation1.OU_OH = itemLine1.ParentItem.Consignment.BookingHeader.BillToParty.Header.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product2.OP_PartNum = "PROD002";
			product2.OP_Desc = "second part description";
			product2.OP_NetWeight = 0.5m;
			product2.OP_WeightUQ = "KG";
			product2.OP_Weight = 1.5m;

			var relation2 = product2.RelatedOrganisations.AddNew();
			relation2.OU_OH = itemLine2.ParentItem.Consignment.BookingHeader.BillToParty.Header.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			Factory.Save();

			itemLine1.HVS_ProductCode = product1.OP_PartNum;

			CombineAssertions(() =>
			{
				AssertEquals(product1.OP_NetWeight, itemLine1.HVS_NetWeight);
				AssertEquals(product1.OP_Weight, itemLine1.HVS_GrossWeight);
				AssertEquals(product1.OP_WeightUQ, itemLine1.HVS_WeightUnit);
				AssertEquals("HS123456", itemLine1.HVS_DestinationTariff);
			});

			itemLine2.HVS_ProductCode = product2.OP_PartNum;

			CombineAssertions(() =>
			{
				AssertEquals(product2.OP_NetWeight, itemLine2.HVS_NetWeight);
				AssertEquals(product2.OP_Weight, itemLine2.HVS_GrossWeight);
				AssertEquals(product2.OP_WeightUQ, itemLine2.HVS_WeightUnit);
				AssertEquals("123456", itemLine2.HVS_OriginTariff);
			});
		}

		public void TestShipmentOriginCountryAndShipmentDestinationCountry()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = CountryCodes.China;
			shipment.JS_RL_NKDestination = CountryCodes.Australia;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var line = item.Lines.AddNew();

			AssertEquals(shipment.JS_RL_NKOrigin, line.ShipmentOriginCountryCode);
			AssertEquals(shipment.JS_RL_NKDestination, line.ShipmentDestinationCountryCode);
		}

		public void TestReadonlyAttributes()
		{
			var type = typeof(HVLVItemLine);

			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<ReadOnlyAttribute>(type, nameof(HVLVItemLine.ShipmentOriginCountryCode), false, a => a.IsReadOnly);
				AssertHasCustomAttribute<ReadOnlyAttribute>(type, nameof(HVLVItemLine.ShipmentDestinationCountryCode), false, a => a.IsReadOnly);
				AssertHasCustomAttribute<ReadOnlyMemberAttribute>(type, nameof(HVLVItemLine.HVS_ProductCode), false, a => a.Member == "ProductCodeReadonly");
			});
		}

		public void TestListAttributes()
		{
			var type = typeof(HVLVItemLine);

			CombineAssertions(() =>
			{
				Array.ForEach(new (string Name, string DataSource, string DisplayMember, string ValueMember)[] {
					(nameof(HVLVItemLine.HVS_WeightUnit), "Lookups.WeightUnitList", null,null),
					(nameof(HVLVItemLine.HVS_ProductCode), "Lookups.Products",  "OP_PartNum", "OP_PartNum"),
				}, item =>
				{
					AssertHasCustomAttribute<ListAttribute>(type, item.Name, false,
						a => a.ListDataSourceMember == item.DataSource
						&& (item.DisplayMember == null || a.DisplayMember == item.DisplayMember)
						&& (item.ValueMember == null || a.ValueMember == item.ValueMember));
				});
			});
		}

		public void TestHVS_GoodsDescriptionLength()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var line = Factory.NewWithValidTestData<HVLVItemLine>();
			AssertGreaterThanOrEqualTo("HVS_GoodsDescriptionInfo length should be greater than or equal to the length OrgSupplierPart.OP_Desc", line.HVS_GoodsDescriptionInfo.MaxLength, product.OP_DescInfo.MaxLength);
		}

		public void TestConstraintOriginCountryCodeNoAndOriginTariffNo()
		{
			var line = CreateHVLVItemLineTestObject();
			Factory.Save();

			line.HVS_RN_NKOriginCountryCode = string.Empty;
			line.HVS_OriginTariff = string.Empty;
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestConstraintOriginCountryCodeYesAndOriginTariffNo()
		{
			var line = CreateHVLVItemLineTestObject();
			Factory.Save();

			line.HVS_RN_NKOriginCountryCode = "AU";
			line.HVS_OriginTariff = string.Empty;
			line.RunPreSaveValidation();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestConstraintOriginCountryCodeYesAndOriginTariffYes()
		{
			var line = CreateHVLVItemLineTestObject();
			Factory.Save();

			line.HVS_RN_NKOriginCountryCode = "AU";
			line.HVS_OriginTariff = "123456";
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestConstraintOriginCountryCodeNoAndOriginTariffYes()
		{
			var line = CreateHVLVItemLineTestObject();
			Factory.Save();

			line.HVS_RN_NKOriginCountryCode = string.Empty;
			line.HVS_OriginTariff = "123456";
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		public void TestOriginAndDestinationTariffWhenClassificationSelected()
		{
			var importClassification = GetNewClassification("IMP");
			var exportClassification = GetNewClassification("EXP");
			var otherClassification = GetNewClassification("???");

			var itemLine = Factory.New<HVLVItemLine>();

			itemLine.HVS_CC_Lookup = importClassification.PK;
			AssertEquals("12345678", itemLine.HVS_DestinationTariff);
			AssertEquals("IMP DESC", itemLine.HVS_GoodsDescription);

			itemLine.HVS_CC_Lookup = ZGuid.Empty;
			AssertEquals("12345678", itemLine.HVS_DestinationTariff);
			AssertEquals(ZString.Empty, itemLine.HVS_GoodsDescription);

			itemLine.HVS_DestinationTariff = ZString.Empty;
			itemLine.HVS_OriginTariff = ZString.Empty;

			itemLine.HVS_CC_Lookup = exportClassification.PK;
			AssertEquals("12345678", itemLine.HVS_OriginTariff);
			AssertEquals("EXP DESC", itemLine.HVS_GoodsDescription);

			itemLine.HVS_CC_Lookup = ZGuid.Empty;
			AssertEquals("12345678", itemLine.HVS_OriginTariff);
			AssertEquals(ZString.Empty, itemLine.HVS_GoodsDescription);

			itemLine.HVS_DestinationTariff = ZString.Empty;
			itemLine.HVS_OriginTariff = ZString.Empty;

			itemLine.HVS_CC_Lookup = otherClassification.PK;
			AssertEquals("12345678", itemLine.HVS_OriginTariff);
			AssertEquals("12345678", itemLine.HVS_DestinationTariff);
			AssertEquals("??? DESC", itemLine.HVS_GoodsDescription);

			itemLine.HVS_CC_Lookup = ZGuid.Empty;
			AssertEquals("12345678", itemLine.HVS_DestinationTariff);
			AssertEquals("12345678", itemLine.HVS_OriginTariff);
			AssertEquals(ZString.Empty, itemLine.HVS_GoodsDescription);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
			{
				itemLine.HVS_DestinationTariff = ZString.Empty;
				itemLine.HVS_OriginTariff = ZString.Empty;
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_RN_NKConsigneeCountryCode = "AU";
				consignment.HVC_RN_NKShipperCountryCode = "US";
				AssertEquals(Directions.Import, consignment.DirectionOfTrade);
				itemLine.HVS_HVI_HVLVItem = consignment.Items.AddNew().PK;
				itemLine.HVS_CC_Lookup = otherClassification.PK;

				AssertEquals("12345678", itemLine.HVS_DestinationTariff);
				AssertEquals(ZString.Empty, itemLine.HVS_OriginTariff);

				itemLine.HVS_DestinationTariff = ZString.Empty;
				itemLine.HVS_OriginTariff = ZString.Empty;
				itemLine.HVS_CC_Lookup = ZGuid.Empty;

				consignment.HVC_RN_NKConsigneeCountryCode = "NZ";
				consignment.HVC_RN_NKShipperCountryCode = "AU";
				AssertEquals(Directions.Export, consignment.DirectionOfTrade);
				itemLine.HVS_CC_Lookup = otherClassification.PK;
				AssertEquals("12345678", itemLine.HVS_OriginTariff);
				AssertEquals(ZString.Empty, itemLine.HVS_DestinationTariff);
			}
		}

		public void TestGoodsDescriptionAndClassificationLookUpWhenTariffCleared()
		{
			var importClassification = GetNewClassification("IMP");
			var exportClassification = GetNewClassification("EXP");

			var itemLine = Factory.New<HVLVItemLine>();

			itemLine.HVS_CC_Lookup = importClassification.PK;
			AssertNotEquals(ZGuid.Empty, itemLine.HVS_CC_Lookup);
			AssertNotEquals(ZString.Empty, itemLine.HVS_GoodsDescription);

			itemLine.HVS_OriginTariff = ZString.Empty;
			AssertGoodsDescriptionAndClassificationLookUpClearUp();

			itemLine.HVS_CC_Lookup = exportClassification.PK;
			AssertNotEquals(ZGuid.Empty, itemLine.HVS_CC_Lookup);
			AssertNotEquals(ZString.Empty, itemLine.HVS_GoodsDescription);

			itemLine.HVS_DestinationTariff = ZString.Empty;
			AssertGoodsDescriptionAndClassificationLookUpClearUp();

			void AssertGoodsDescriptionAndClassificationLookUpClearUp()
			{
				AssertEquals(ZGuid.Empty, itemLine.HVS_CC_Lookup);
				AssertEquals(ZString.Empty, itemLine.HVS_GoodsDescription);
			}
		}

		public void TestPartSyncManagerActiveDeciderPK()
		{
			var itemLine = Factory.New<HVLVItemLine>();
			AssertEquals(ZGuid.Empty, ((IInvoiceLinePartDetails)itemLine).PartSyncManagerActiveDeciderPK);

			var item = Factory.New<HVLVItem>();
			itemLine.HVS_HVI_HVLVItem = item.PK;
			AssertEquals(ZGuid.Empty, ((IInvoiceLinePartDetails)itemLine).PartSyncManagerActiveDeciderPK);

			var consignment = Factory.New<HVLVConsignment>();
			item.HVI_HVC_Consignment = consignment.PK;
			AssertEquals(ZGuid.Empty, ((IInvoiceLinePartDetails)itemLine).PartSyncManagerActiveDeciderPK);

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			consignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			AssertEquals(bookingHeader.PK, ((IInvoiceLinePartDetails)itemLine).PartSyncManagerActiveDeciderPK);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			itemLine.RefreshPartSyncManagerActiveDeciderPK();
			AssertEquals(consignmentHeader.PK, ((IInvoiceLinePartDetails)itemLine).PartSyncManagerActiveDeciderPK);
		}

		public void TestNoAuditLog()
		{
			var itemLine = Factory.NewWithValidTestData<HVLVItemLine>();
			itemLine.HVS_Quantity = 1;
			Factory.Save();

			itemLine.HVS_Quantity = 2;
			Factory.Save();

			itemLine.Delete();
			Factory.Save();

			CombineAssertions("No audit log for HVLVItemLine", () =>
			{
				Assert("No ADD log when created", !itemLine.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.AddedARecordToTheSystemCode));
				Assert("No EDT log when edited", !itemLine.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode));
				Assert("No DEL log when deleted", !itemLine.Logs.HasLogWith(l => l.SL_SE_NKEvent == AutoEvents.DeletedARecordInTheSystemCode));
			});
		}

		public void TestBulkSaveHVLVItemLines()
		{
			for (var i = 0; i < Factory.DefaultBulkCopyThreshold() + 1; i++)
			{
				var hvlvItemLine = Factory.NewWithValidTestData<HVLVItemLine>();
			}

			using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
			{
				AssertNoExceptionThrown(Factory.Save);
				Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVItemLineSchema.Constants.TableName, ["FireTriggers"]));
				Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVItemSchema.Constants.TableName, ["CheckConstraints"]));
				Assert(bulkCopyEventTracker.HasBulkCopyEvent(HVLVConsignmentSchema.Constants.TableName, ["FireTriggers"]));
			}
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consignment = factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_OriginTariff = "123456";
			itemLine.HVS_RN_NKOriginCountryCode = "AU";
			itemLine.HVS_CustomsValue = 100m;
			itemLine.HVS_DestinationTariff = "HS123456";
			itemLine.HVS_GoodsDescription = "This is something for description.";
			itemLine.HVS_OriginGoodsDescription = "This is something for origin description.";
			itemLine.HVS_GrossWeight = 2.1m;
			itemLine.HVS_IntrinsicValue = 100.2m;
			itemLine.HVS_ItemURL = "http://www.amazon.com/item/3333";
			itemLine.HVS_NetWeight = 2.0m;
			itemLine.HVS_ProductCode = "PROD1234";
			itemLine.HVS_Quantity = 3;
			itemLine.HVS_WeightUnit = "KG";

			return itemLine;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateHVLVItemLineTestObject();
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return CreateHVLVItemLineTestObject();
		}

		HVLVItemLine CreateHVLVItemLineTestObject()
		{
			var booking = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = booking.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "AU";

			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_OriginTariff = "123456";
			itemLine.HVS_RN_NKOriginCountryCode = "AU";
			itemLine.HVS_CustomsValue = 100m;
			itemLine.HVS_DestinationTariff = "HS123456";
			itemLine.HVS_GoodsDescription = "This is something for description.";
			itemLine.HVS_OriginGoodsDescription = "This is something for origin description.";
			itemLine.HVS_GrossWeight = 2.1m;
			itemLine.HVS_IntrinsicValue = 100.2m;
			itemLine.HVS_ItemURL = "http://www.amazon.com/item/3333";
			itemLine.HVS_NetWeight = 2.0m;
			itemLine.HVS_ProductCode = "PROD1234";
			itemLine.HVS_Quantity = 3;
			itemLine.HVS_WeightUnit = "KG";

			return itemLine;
		}

		BaseCusClassification GetNewClassification(string type)
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = type;
			classification.CC_TariffNum = "1234.56.78";
			classification.CC_Description = type + " DESC";

			return classification;
		}

		string[] SupportHVLVCountrys => new string[]
		{
			CountryCodes.Australia,
			CountryCodes.UnitedStates,
			CountryCodes.NewZealand,
			CountryCodes.Singapore,
		};

		#endregion
	}
}
