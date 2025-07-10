using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using WTG.RTUS.Interface;
using WTG.RTUS.Interface.TestFramework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	class PkgPackageUniversalShipmentDataObjectWriterTest : PackingTestCaseWithFactory
	{
		#region TestConstructor_DoesNotTakeInvalidArguments

		public void TestConstructor_DoesNotTakeInvalidArguments()
		{
			var package = Factory.New<PkgPackage>();
			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
			AssertExceptionThrown<ArgumentNullException>(() => new PkgPackageUniversalShipmentDataObjectWriter(writeManager, null));
		}

		#endregion

		#region TestGetDataObject_WithParent

		public void TestGetDataObject_WithParent()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			var mockHandle = new DummyHandle(() => testShipment);
			Data.CreatePackingData();

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("BOX", "123");
				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
				var pkgPackageDataObject = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(package);
				AssertNotNull(pkgPackageDataObject);
				AssertEquals("Package Writer should use the Universal Shipment returned from the Mock Writer.", testShipment, pkgPackageDataObject);
				AssertEquals("Package Writer should use the Universal Shipment returned from the Mock Writer.", "TEST", pkgPackageDataObject.BookingConfirmationReference);
				AssertEquals("Mock Writer should be given the correct Package.", package, mockHandle.PackagePassedIntoPopulateDataObject);

				var packageDataSource = pkgPackageDataObject.DataContext.GetMatchingDataSource(DataContextType.PkgPackage);
				AssertNotNull("PkgPackage data source should not be null.", packageDataSource);
				AssertEquals("PkgPackage data source key should match.", "123", packageDataSource.Key);

				var dummyDataSource = pkgPackageDataObject.DataContext.GetMatchingDataSource(DataContextType.DummyBusinessObject);
				AssertNotNull("Parent's data source should not be null.", dummyDataSource);
				AssertEquals("Parent's data source key should match.", "REF123", dummyDataSource.Key);

				var ids = pkgPackageDataObject.DataContext.GetEnterpriseServerAndCompanyIDs();
				AssertNotNullOrEmpty("Company code should not be null or empty.", ids.CompanyCode);
				AssertNotNullOrEmpty("Enterprise ID should not be null or empty.", ids.EnterpriseID);
				AssertNotNullOrEmpty("Server ID should not be null or empty.", ids.ServerID);
			}
		}

		#endregion

		#region TestGetDataObject_PkgPackage

		public void TestGetDataObject_PkgPackage()
		{
			Data.CreatePackingData();
			Data.PackageJob.Packages.AddNew("PLT");
			var package = Data.PackageJob.Packages.AddNew("BOX");

			package.KP_DimensionUQ = "M";
			package.KP_Height = 1m;
			package.KP_Length = 2m;
			package.KP_PackageID = "PACKAGE123";
			package.KP_PackageQty = 3;
			package.KP_VolumeUQ = "M3";
			package.KP_Weight = 5m;
			package.KP_WeightUQ = "T";
			package.KP_Width = 6m;
			package.KP_MarksAndNumbers = "MARK123";
			package.KP_TransportRef = "TRANSPORT REF";
			package.KP_GoodsDescription = "GOODS DESC";
			package.KP_HSCode = "HARMON CODE";
			package.KP_ExternalReference = "PackLineID";
			package.KP_Volume = 12m;
			package.KP_RH_NKCommodityCode = "GEN";
			package.KP_RequiresTemperatureControl = true;
			package.KP_RequiredTemperatureMinimum = -10;
			package.KP_RequiredTemperatureMaximum = -5;
			package.KP_RequiredTemperatureUnit = Core.Constants.Temperature.Centigrade;

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
			var pkgPackageDataObject = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(package);
			AssertEquals("Should only have one package.", 1, pkgPackageDataObject.PackingLineCollection.Count);
			AssertEquals("Content should be Partial.", CollectionContent.Partial, pkgPackageDataObject.PackingLineCollection.Content);

			var packageDataObject = pkgPackageDataObject.PackingLineCollection.Single();
			AssertEquals(nameof(package.KP_DimensionUQ), "M", packageDataObject.LengthUnit.Code);
			AssertEquals(nameof(package.KP_Height), 1m, packageDataObject.Height);
			AssertEquals(nameof(package.KP_Length), 2m, packageDataObject.Length);
			AssertEquals(nameof(package.KP_PackageID), "PACKAGE123", packageDataObject.ReferenceNumber);
			AssertEquals(nameof(package.KP_PackageQty), 3L, packageDataObject.PackQty);
			AssertEquals(nameof(package.KP_F3_NKPackType), "BOX", packageDataObject.PackType.Code);
			AssertEquals(nameof(package.KP_Volume), 12m, packageDataObject.Volume);
			AssertEquals(nameof(package.KP_VolumeUQ), "M3", packageDataObject.VolumeUnit.Code);
			AssertEquals(nameof(package.KP_Weight), 5m, packageDataObject.Weight);
			AssertEquals(nameof(package.KP_WeightUQ), "T", packageDataObject.WeightUnit.Code);
			AssertEquals(nameof(package.KP_Width), 6m, packageDataObject.Width);
			AssertEquals(nameof(package.KP_TransportRef), "TRANSPORT REF", packageDataObject.TransportReference);
			AssertEquals(nameof(package.KP_MarksAndNumbers), "MARK123", packageDataObject.MarksAndNos);
			AssertEquals(nameof(package.KP_GoodsDescription), "GOODS DESC", packageDataObject.GoodsDescription);
			AssertEquals(nameof(package.KP_HSCode), "HARMON CODE", packageDataObject.HarmonisedCode);
			AssertEquals(nameof(package.KP_PackageID), "PackLineID", packageDataObject.PackingLineID);
			AssertEquals(nameof(package.KP_RH_NKCommodityCode), "GEN", packageDataObject.Commodity.Code);

			// temperatures
			AssertEquals(nameof(package.KP_RequiresTemperatureControl), true, packageDataObject.RequiresTemperatureControl);
			AssertEquals(nameof(package.KP_RequiredTemperatureMinimum), -10m, packageDataObject.RequiredTemperatureMinimum);
			AssertEquals(nameof(package.KP_RequiredTemperatureMaximum), -5m, packageDataObject.RequiredTemperatureMaximum);
			AssertEquals(nameof(package.KP_RequiredTemperatureUnit), "C", packageDataObject.RequiredTemperatureUnit.Code);

			AssertEquals("Package should have no link.", 0, packageDataObject.Link);
			AssertNull("Package has no container parent, so Link to it should not exist.", packageDataObject.ContainerLink);
		}

		#endregion

		#region TestGetDataObject_PkgPackage_WithPackableItems

		public void TestGetDataObject_PkgPackage_WithPackableItems()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => testShipment);

			Data.CreatePackingData();
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("BOX", "123");
				package.Pack(Data.DummyPackableItemOnLine1, Data.DummyLine1);

				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
				var pkgPackageDataObject = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(package);
				AssertNotNull(pkgPackageDataObject);

				var packLineDataObject = pkgPackageDataObject.PackingLineCollection.Single();
				var packedItemData = packLineDataObject.PackedItemCollection[0];
				AssertEquals("packedItemOnPackageData.Description", "TV - Size: 63in", packedItemData.Description);
				AssertEquals("packedItemOnPackageData.PackedQuantity", 100m, packedItemData.PackedQuantity);
				AssertEquals("packedItemOnPackageData.UnitOfQuantity.Code", "UNT", packedItemData.UnitOfQuantity.Code);
				AssertEquals("packedItemOnPackageData.CommercialInvoiceLineLink", 0, packedItemData.CommercialInvoiceLineLink);
				AssertEquals("Package Writer should use the Universal Shipment returned from the Parent Writer.", "TEST", pkgPackageDataObject.BookingConfirmationReference);
			}
		}

		public void TestGetDataObject_PkgPackage_WithPackableItems_LinePrice()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => testShipment);

			Data.CreatePackingData();
			var usCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			Data.DummyLine1.UnitPrice = new Money(5m, usCurrency);

			GlbCompany.CurrentCompany.SetCurrency("AUD");
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDate.Today.AddDays(-7), ZDate.Today.AddDays(7), 2m, usCurrency);
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("BOX", "123");
				package.Pack(Data.DummyLine1, 10m);

				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
				var pkgPackageDataObject = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(package);
				AssertNotNull(pkgPackageDataObject);

				var packLineDataObject = pkgPackageDataObject.PackingLineCollection.Single();
				AssertEquals(50m, packLineDataObject.LinePrice);
				AssertEquals("Packed item currency is used, local currency is ignored.", "USD", packLineDataObject.LinePriceCurrency.Code);
			}
		}

		public void TestGetDataObject_PkgPackage_WithPackableItems_LinePrice_WithChildPackages()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => testShipment);

			Data.CreatePackingData();
			var usCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			Data.DummyLine1.UnitPrice = new Money(5m, usCurrency);
			Data.DummyLine2.UnitPrice = new Money(10m, usCurrency);
			Data.DummyLine3.UnitPrice = new Money(1m, usCurrency);

			GlbCompany.CurrentCompany.SetCurrency("AUD");
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDate.Today.AddDays(-7), ZDate.Today.AddDays(7), 2m, usCurrency);
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("BOX", "123");
				package.Pack(Data.DummyLine1, 10m);

				var childPackage1 = package.Packages.AddNew("BOX", "456");
				childPackage1.Pack(Data.DummyLine2, 10m);

				var childPackage2 = childPackage1.Packages.AddNew("BOX", "789");
				childPackage2.Pack(Data.DummyLine3, 10m);

				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
				var pkgPackageDataObject1 = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(package);
				AssertNotNull(pkgPackageDataObject1);

				var packLineDataObject1 = pkgPackageDataObject1.PackingLineCollection.Single();
				AssertEquals(160m, packLineDataObject1.LinePrice);
				AssertEquals("Packed item currency is used, local currency is ignored.", "USD", packLineDataObject1.LinePriceCurrency.Code);

				var pkgPackageDataObject2 = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(childPackage1);
				AssertNotNull(pkgPackageDataObject2);

				var packLineDataObject2 = pkgPackageDataObject2.PackingLineCollection.Single();
				AssertEquals(110m, packLineDataObject2.LinePrice);
				AssertEquals("Packed item currency is used, local currency is ignored.", "USD", packLineDataObject2.LinePriceCurrency.Code);

				var pkgPackageDataObject3 = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(childPackage2);
				AssertNotNull(pkgPackageDataObject3);

				var packLineDataObject3 = pkgPackageDataObject3.PackingLineCollection.Single();
				AssertEquals(10m, packLineDataObject3.LinePrice);
				AssertEquals("Packed item currency is used, local currency is ignored.", "USD", packLineDataObject3.LinePriceCurrency.Code);
			}
		}

		public void TestGetDataObject_PkgPackage_WithPackableItems_LinePrice_MultipleCurrencies()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => testShipment);

			Data.CreatePackingData();
			var usCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			Data.DummyLine1.UnitPrice = new Money(5m, usCurrency);
			Data.DummyLine2.UnitPrice = new Money(10m, usCurrency);

			var nzCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			Data.DummyLine3.UnitPrice = new Money(1m, nzCurrency);

			GlbCompany.CurrentCompany.SetCurrency("AUD");
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDate.Today.AddDays(-7), ZDate.Today.AddDays(7), 2m, usCurrency);
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDate.Today.AddDays(-7), ZDate.Today.AddDays(7), 0.5m, nzCurrency);
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("BOX", "123");
				package.Pack(Data.DummyLine1, 10m);
				package.Pack(Data.DummyLine2, 2m);
				package.Pack(Data.DummyLine3, 20m);

				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
				var pkgPackageDataObject = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(package);
				AssertNotNull(pkgPackageDataObject);

				var packLineDataObject = pkgPackageDataObject.PackingLineCollection.Single();
				AssertEquals(75m, packLineDataObject.LinePrice);
				AssertEquals("Local currency is used for multi currency package.", "AUD", packLineDataObject.LinePriceCurrency.Code);
			}
		}

		public void TestGetDataObject_PkgPackage_WithPackableItems_LinePrice_MultipleCurrencies_MultipleCommonCurrency()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => testShipment);

			Data.CreatePackingData();
			var usCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			Data.DummyLine1.UnitPrice = new Money(5m, usCurrency);
			Data.DummyLine2.UnitPrice = new Money(10m, usCurrency);

			var nzCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			Data.DummyLine3.UnitPrice = new Money(1m, nzCurrency);

			var dummyLine4 = Data.Dummy.Lines.AddNew();
			dummyLine4.TotalQty = 100;
			dummyLine4.TotalQtyUQ = "UNT";
			dummyLine4.UnitPrice = new Money(2m, nzCurrency);

			GlbCompany.CurrentCompany.SetCurrency("AUD");
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDate.Today.AddDays(-7), ZDate.Today.AddDays(7), 2m, usCurrency);
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDate.Today.AddDays(-7), ZDate.Today.AddDays(7), 0.5m, nzCurrency);
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("BOX", "123");
				package.Pack(Data.DummyLine1, 10m);
				package.Pack(Data.DummyLine2, 2m);
				package.Pack(Data.DummyLine3, 20m);
				package.Pack(dummyLine4, 30m);

				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
				var pkgPackageDataObject = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(package);
				AssertNotNull(pkgPackageDataObject);

				var packLineDataObject = pkgPackageDataObject.PackingLineCollection.Single();
				AssertEquals(195m, packLineDataObject.LinePrice);
				AssertEquals("Local currency is used for multi currency package.", "AUD", packLineDataObject.LinePriceCurrency.Code);
			}
		}

		public void TestGetDataObject_PkgPackage_WithPackableItems_LinePrice_MultipleCurrencies_WithChildPackages()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => testShipment);

			Data.CreatePackingData();
			var usCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			Data.DummyLine1.UnitPrice = new Money(5m, usCurrency);

			var nzCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			Data.DummyLine2.UnitPrice = new Money(10m, nzCurrency);
			Data.DummyLine3.UnitPrice = new Money(1m, nzCurrency);

			GlbCompany.CurrentCompany.SetCurrency("AUD");
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDate.Today.AddDays(-7), ZDate.Today.AddDays(7), 2m, usCurrency);
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDate.Today.AddDays(-7), ZDate.Today.AddDays(7), 0.5m, nzCurrency);
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("BOX", "123");
				package.Pack(Data.DummyLine1, 10m);

				var childPackage1 = package.Packages.AddNew("BOX", "456");
				childPackage1.Pack(Data.DummyLine2, 10m);

				var childPackage2 = childPackage1.Packages.AddNew("BOX", "789");
				childPackage2.Pack(Data.DummyLine3, 10m);

				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
				var pkgPackageDataObject1 = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(package);
				AssertNotNull(pkgPackageDataObject1);

				var packLineDataObject1 = pkgPackageDataObject1.PackingLineCollection.Single();
				AssertEquals(245m, packLineDataObject1.LinePrice);
				AssertEquals("Local currency is used for multi currency package.", "AUD", packLineDataObject1.LinePriceCurrency.Code);

				var pkgPackageDataObject2 = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(childPackage1);
				AssertNotNull(pkgPackageDataObject2);

				var packLineDataObject2 = pkgPackageDataObject2.PackingLineCollection.Single();
				AssertEquals(110m, packLineDataObject2.LinePrice);
				AssertEquals("Package currency is used, local currency is ignored.", "NZD", packLineDataObject2.LinePriceCurrency.Code);

				var pkgPackageDataObject3 = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(childPackage2);
				AssertNotNull(pkgPackageDataObject3);

				var packLineDataObject3 = pkgPackageDataObject3.PackingLineCollection.Single();
				AssertEquals(10m, packLineDataObject3.LinePrice);
				AssertEquals("Package currency is used, local currency is ignored.", "NZD", packLineDataObject3.LinePriceCurrency.Code);
			}
		}

		void SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_GC = company.PK;
			exchangeRate.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			exchangeRate.RE_StartDate = startDate;
			exchangeRate.RE_ExpiryDate = endDate;
			exchangeRate.RE_SellRate = rate;
			exchangeRate.RE_ExRateType = rateType;
		}

		#endregion

		#region TestGetDataObject_PkgPackage_CancelPackage

		public void TestGetDataObject_PkgPackage_CancelPackage()
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");

			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};
			var mockHandle = new DummyHandle(() => testShipment);

			Data.CreatePackingData();
			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, Data.Dummy))
			{
				var package = Data.PackageJob.Packages.AddNew("BOX", "123");
				package.Pack(Data.DummyPackableItemOnLine1, Data.DummyLine1);

				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
				var pkgPackageDataObject = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, RequestType.Cancellation).GetDataObject(package);
				AssertNotNull(pkgPackageDataObject);
				AssertEquals("Package Writer should use the Universal Shipment returned from the Parent Writer.", "TEST", pkgPackageDataObject.BookingConfirmationReference);
				AssertEquals("Should have no packages to denote a Cancel.", 0, pkgPackageDataObject.PackingLineCollection.Count);
				AssertEquals("Content should be Partial.", CollectionContent.Partial, pkgPackageDataObject.PackingLineCollection.Content);
				AssertNull("No Packed Item Information should be generated.", pkgPackageDataObject.CommercialInfo);
			}
		}

		#endregion

		#region TestGetDataObject_ConsolidationHandlingUnit

		public void TestGetDataObject_ConsolidationHandlingUnit()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew("BOX");

			package.KP_DimensionUQ = "M";
			package.KP_Height = 1m;
			package.KP_Length = 2m;
			package.KP_PackageID = "PACKAGE123";
			package.KP_PackageQty = 3;
			package.KP_VolumeUQ = "M3";
			package.KP_Weight = 5m;
			package.KP_WeightUQ = "T";
			package.KP_Width = 6m;
			package.KP_MarksAndNumbers = "MARK123";
			package.KP_TransportRef = "TRANSPORT REF 1";
			package.KP_GoodsDescription = "GOODS DESC 1";
			package.KP_HSCode = "HARMON CODE 1";
			package.KP_ExternalReference = "PackLineID 1";
			package.KP_Volume = 12m;
			package.KP_RH_NKCommodityCode = "GEN";
			package.KP_RequiresTemperatureControl = true;
			package.KP_RequiredTemperatureMinimum = -10;
			package.KP_RequiredTemperatureMaximum = -5;
			package.KP_RequiredTemperatureUnit = Core.Constants.Temperature.Centigrade;

			var handlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitJob = Factory.NewWithValidTestData<PkgPackageJob>();
			handlingUnitJob.KJ_ParentID = handlingUnit.PK;
			handlingUnitJob.KJ_ParentTableCode = PkgHandlingUnitSchema.Constants.Prefix;

			var handlingUnitPackage = Factory.NewWithValidTestData<PkgPackage>();
			handlingUnitPackage.KP_KJ_ParentPackageJob = handlingUnitJob.PK;
			handlingUnitPackage.KP_DimensionUQ = "CM";
			handlingUnitPackage.KP_Height = 100m;
			handlingUnitPackage.KP_Length = 200m;
			handlingUnitPackage.KP_PackageID = "HU123";
			handlingUnitPackage.KP_PackageQty = 30;
			handlingUnitPackage.KP_VolumeUQ = "C3";
			handlingUnitPackage.KP_Weight = 50m;
			handlingUnitPackage.KP_WeightUQ = "KG";
			handlingUnitPackage.KP_Width = 60m;
			handlingUnitPackage.KP_MarksAndNumbers = "MARK456";
			handlingUnitPackage.KP_TransportRef = "TRANSPORT REF 2";
			handlingUnitPackage.KP_GoodsDescription = "GOODS DESC 2";
			handlingUnitPackage.KP_HSCode = "HARMON CODE 2";
			handlingUnitPackage.KP_ExternalReference = "PackLineID 2";
			handlingUnitPackage.KP_Volume = 240m;
			handlingUnitPackage.KP_RH_NKCommodityCode = "GEN";
			handlingUnitPackage.KP_RequiresTemperatureControl = true;
			handlingUnitPackage.KP_RequiredTemperatureMinimum = 10;
			handlingUnitPackage.KP_RequiredTemperatureMaximum = 20;
			handlingUnitPackage.KP_RequiredTemperatureUnit = Core.Constants.Temperature.Fahrenheit;

			Helper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			handlingUnitPackage.KP_IsClosed = true;
			handlingUnitPackage.KP_ClosedTimeUtc = DateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "E";

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, handlingUnitPackage));
			var handlingUnitDataObject = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(handlingUnitPackage);

			var handlingUnitPackingLine = handlingUnitDataObject.PackingLineCollection.Single();
			AssertDataObjectProperties(handlingUnitPackage, handlingUnitPackingLine);

			AssertEquals("Should only have one inner package.", 1, handlingUnitDataObject.PackingLineCollection.Count);
			AssertEquals("Content should be Partial.", CollectionContent.Partial, handlingUnitDataObject.PackingLineCollection.Content);

			AssertEquals("There should only be a single item in SubShipmentCollection", 1, handlingUnitDataObject.SubShipmentCollection.Count);
			var innerPackageUniversalShipment = handlingUnitDataObject.SubShipmentCollection[0];
			var innerPackagePackingLine = innerPackageUniversalShipment.PackingLineCollection.Single();
			AssertDataObjectProperties(package, innerPackagePackingLine);

			AssertEquals("Package should have no link.", 0, innerPackagePackingLine.Link);
			AssertNull("Package has no container parent, so Link to it should not exist.", innerPackagePackingLine.ContainerLink);
		}

		void AssertDataObjectProperties(PkgPackage expectedPackage, PackingLine packageDataObject)
		{
			AssertEquals(nameof(expectedPackage.KP_DimensionUQ), expectedPackage.KP_DimensionUQ, packageDataObject.LengthUnit.Code);
			AssertEquals(nameof(expectedPackage.KP_Height), expectedPackage.KP_Height, packageDataObject.Height);
			AssertEquals(nameof(expectedPackage.KP_Length), expectedPackage.KP_Length, packageDataObject.Length);
			AssertEquals(nameof(expectedPackage.KP_PackageID), expectedPackage.KP_PackageID, packageDataObject.ReferenceNumber);
			AssertEquals(nameof(expectedPackage.KP_PackageQty), expectedPackage.KP_PackageQty.ToString(), packageDataObject.PackQty.ToString());
			AssertEquals(nameof(expectedPackage.KP_F3_NKPackType), expectedPackage.KP_F3_NKPackType, packageDataObject.PackType.Code);
			AssertEquals(nameof(expectedPackage.KP_Volume), expectedPackage.KP_Volume, packageDataObject.Volume);
			AssertEquals(nameof(expectedPackage.KP_VolumeUQ), expectedPackage.KP_VolumeUQ, packageDataObject.VolumeUnit.Code);
			AssertEquals(nameof(expectedPackage.KP_Weight), expectedPackage.KP_Weight, packageDataObject.Weight);
			AssertEquals(nameof(expectedPackage.KP_WeightUQ), expectedPackage.KP_WeightUQ, packageDataObject.WeightUnit.Code);
			AssertEquals(nameof(expectedPackage.KP_Width), expectedPackage.KP_Width, packageDataObject.Width);
			AssertEquals(nameof(expectedPackage.KP_TransportRef), expectedPackage.KP_TransportRef, packageDataObject.TransportReference);
			AssertEquals(nameof(expectedPackage.KP_MarksAndNumbers), expectedPackage.KP_MarksAndNumbers, packageDataObject.MarksAndNos);
			AssertEquals(nameof(expectedPackage.KP_GoodsDescription), expectedPackage.KP_GoodsDescription, packageDataObject.GoodsDescription);
			AssertEquals(nameof(expectedPackage.KP_HSCode), expectedPackage.KP_HSCode, packageDataObject.HarmonisedCode);
			AssertEquals(nameof(expectedPackage.KP_ExternalReference), expectedPackage.KP_ExternalReference, packageDataObject.PackingLineID);
			AssertEquals(nameof(expectedPackage.KP_RH_NKCommodityCode), expectedPackage.KP_RH_NKCommodityCode, packageDataObject.Commodity.Code);

			AssertEquals(nameof(expectedPackage.KP_RequiresTemperatureControl), expectedPackage.KP_RequiresTemperatureControl, packageDataObject.RequiresTemperatureControl);
			AssertEquals(nameof(expectedPackage.KP_RequiredTemperatureMinimum), expectedPackage.KP_RequiredTemperatureMinimum, packageDataObject.RequiredTemperatureMinimum);
			AssertEquals(nameof(expectedPackage.KP_RequiredTemperatureMaximum), expectedPackage.KP_RequiredTemperatureMaximum, packageDataObject.RequiredTemperatureMaximum);
			AssertEquals(nameof(expectedPackage.KP_RequiredTemperatureUnit), expectedPackage.KP_RequiredTemperatureUnit, packageDataObject.RequiredTemperatureUnit.Code);
		}

		#endregion

		#region IOrderLineDictionaryProvider

		public void TestGetDataObject_ParentWriterIsIOrderLineDictionaryProvider()
		{
			var parentJob = Factory.New<DummyWithPacking>();
			var pkgPackageJob = Factory.New<PkgPackageJob>();
			pkgPackageJob.KJ_JobID = "PJ00000001";
			pkgPackageJob.KJ_ParentTableCode = parentJob.TablePrefix;
			pkgPackageJob.KJ_ParentID = parentJob.PK;

			var packableItemParent = parentJob.Lines.AddNew();
			packableItemParent.Description = "HELLO";
			packableItemParent.DescriptionSupplement = "Greeting";

			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.DummyBusinessObject, "REF123");
			var testShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				BookingConfirmationReference = "TEST"
			};

			var orderLinesDictionary = new Dictionary<ZGuid, ZInt>();
			orderLinesDictionary.Add(((BusinessObject)packableItemParent).PK, 2);
			var mockHandle = new DummyHandle((p) => testShipment, orderLinesDictionary);

			using (PackageDummyParentWriterHelper.MockDummyWriter(mockHandle, parentJob))
			{
				var package = pkgPackageJob.Packages.AddNew("BOX");
				package.Pack(packableItemParent, 1m);

				var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, package));
				var pkgPackageDataObject = new PkgPackageUniversalShipmentDataObjectWriter(writeManager, DummyRequest.New()).GetDataObject(package);
				AssertNotNull(pkgPackageDataObject);

				var commercialInvoiceLineDataObject = pkgPackageDataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single();
				AssertEquals("commercialInvoiceLineDataObject.OrderLineLink", 2, commercialInvoiceLineDataObject.OrderLineLink);
				AssertEquals("commercialInvoiceLineDataObject.Description", "HELLO", commercialInvoiceLineDataObject.Description);
			}
		}

		#endregion
	}
}
