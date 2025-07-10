using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataTransfer.Universal.Extensions;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using DeclarationDetail = Enterprise.Customs.US.DataTransfer.Universal.WarehouseCustomsLineDetails.DeclarationDetail;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineDetailsTest : TestCaseWithFactory
	{
		public void TestPackDetailsForEntrySummary()
		{
			CombineAssertions(() =>
			{
				DeclarationDetail.IsExWarehouse = ZBool.False;
				DeclarationDetail.FallbackAddInfos = FallbackAddInfos;
				var line1 = new WarehouseCustomsLineDetails(Factory, InvoiceLine1, Invoice, Dictionary, DeclarationDetail);
				AssertEquals((short)2, line1.EntryLineNumber);
				AssertEquals("XJ5-EINV1_2", line1.EntryNumber);
				AssertNull(line1.PreviousEntryLineNumber);
				AssertNull(line1.PreviousEntryNumber);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				AssertEquals("AU", line1.CountryOfOrigin.Code);
				AssertEquals("AgricultureLicNo=SF323*CAExportCertificate=CA1*UC_NKCountryOfOrigin=AU*WHSEntryLineNo=1*WHSEntryNumber=INB123*ZoneStatus=D", line1.AddInfos);
				var line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(2, line1PackDetails.Count);
				var line1PackDetail1 = line1PackDetails[0];
				AssertEquals("1", line1PackDetail1.PackID);
				AssertEquals(10, line1PackDetail1.PackageQty);
				AssertEquals(50m, line1PackDetail1.PackedQty);
				var line1PackDetail2 = line1PackDetails[1];
				AssertEquals("2", line1PackDetail2.PackID);
				AssertEquals(3, line1PackDetail2.PackageQty);
				AssertEquals(6m, line1PackDetail2.PackedQty);
				var extraClassificationDetails = new List<ZString>(line1.ExtraClassificationDetails);
				AssertEquals(3, extraClassificationDetails.Count);
				AssertEquals("LineNo=3*Tariff=30303030*InvoiceQuantity=20*InvoiceQuantityUnit=PCS*CustomsQuantity=15*CustomsQuantityUnit=NO*LinePrice=1500*CottonFeeExempt=C323*ZoneStatus=N", extraClassificationDetails[0]);
				AssertEquals("LineNo=4*Tariff=40404040*InvoiceQuantity=40*InvoiceQuantityUnit=BS*CustomsQuantity=30*CustomsQuantityUnit=KG*LinePrice=3000*CottonFeeExempt=C434*ZoneStatus=Z*ParentProductLineNo=1", extraClassificationDetails[1]);
				AssertEquals("Invoice Line5 has SecondQty and ThirdQty Information", "LineNo=5*Tariff=40404040*InvoiceQuantity=40*InvoiceQuantityUnit=BS*CustomsQuantity=30*CustomsQuantityUnit=KG*SecondQty=100*SecondUQ=KG*ThirdQty=950*ThirdUQ=NO*LinePrice=3000*CottonFeeExempt=C434*ParentProductLineNo=1", extraClassificationDetails[2]);

				var line2 = new WarehouseCustomsLineDetails(Factory, InvoiceLine2, Invoice, Dictionary, DeclarationDetail);
				AssertEquals((short)1, line2.EntryLineNumber);
				AssertEquals("XJ5-EINV2_1", line2.EntryNumber);
				AssertNull(line2.PreviousEntryLineNumber);
				AssertNull(line2.PreviousEntryNumber);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				AssertEquals("NZ", line2.CountryOfOrigin.Code);
				AssertEquals("AgricultureLicNo=SF798*CAExportCertificate=CA1*CottonFeeExempt=C232*UC_NKCountryOfOrigin=NZ*WHSEntryLineNo=2*WHSEntryNumber=INB891*ZoneStatus=P", line2.AddInfos);
				var line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(2, line2PackDetails.Count);
				var line2PackDetail1 = line2PackDetails[0];
				AssertEquals("1", line2PackDetail1.PackID);
				AssertEquals(10, line2PackDetail1.PackageQty);
				AssertEquals(60m, line2PackDetail1.PackedQty);
				var line2PackDetail2 = line2PackDetails[1];
				AssertEquals("2", line2PackDetail2.PackID);
				AssertEquals(3, line2PackDetail2.PackageQty);
				AssertEquals(9m, line2PackDetail2.PackedQty);
				extraClassificationDetails = new List<ZString>(line2.ExtraClassificationDetails);
				AssertEquals(0, extraClassificationDetails.Count);

				DeclarationDetail.IsExWarehouse = ZBool.True;
				line1 = new WarehouseCustomsLineDetails(Factory, InvoiceLine1, Invoice, Dictionary, DeclarationDetail);
				AssertEquals((short)2, line1.EntryLineNumber);
				AssertEquals("XJ5-EINV1_2", line1.EntryNumber);
				AssertEquals((short)1, line1.PreviousEntryLineNumber);
				AssertEquals("SV9-EN349834", line1.PreviousEntryNumber);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				AssertEquals("AU", line1.CountryOfOrigin.Code);
				AssertEquals("", line1.AddInfos);
				line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(0, line1PackDetails.Count);
				extraClassificationDetails = new List<ZString>(line1.ExtraClassificationDetails);
				AssertEquals(0, extraClassificationDetails.Count);

				line2 = new WarehouseCustomsLineDetails(Factory, InvoiceLine2, Invoice, Dictionary, DeclarationDetail);
				AssertEquals((short)1, line2.EntryLineNumber);
				AssertEquals("XJ5-EINV2_1", line2.EntryNumber);
				AssertEquals((short)2, line2.PreviousEntryLineNumber);
				AssertEquals("SV9-EN349834", line2.PreviousEntryNumber);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				AssertEquals("NZ", line2.CountryOfOrigin.Code);
				AssertEquals("", line1.AddInfos);
				line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(0, line2PackDetails.Count);
				extraClassificationDetails = new List<ZString>(line2.ExtraClassificationDetails);
				AssertEquals(0, extraClassificationDetails.Count);
			});
		}

		public void TestPackDetailsForInBond()
		{
			CombineAssertions(() =>
			{
				DeclarationDetail.IsInBond = ZBool.True;
				DeclarationDetail.FallbackAddInfos = FallbackAddInfos;
				var line1 = new WarehouseCustomsLineDetails(Factory, InvoiceLine1, Invoice, Dictionary, DeclarationDetail);
				AssertEquals((short)2, line1.EntryLineNumber);
				AssertEquals("INB499345", line1.EntryNumber);
				AssertEquals((short)1, line1.PreviousEntryLineNumber);
				AssertEquals("INV-INB123", line1.PreviousEntryNumber);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				AssertEquals("AU", line1.CountryOfOrigin.Code);
				AssertEquals("AgricultureLicNo=SF323*CAExportCertificate=CA1*UC_NKCountryOfOrigin=AU*WHSEntryLineNo=1*WHSEntryNumber=INB123*ZoneStatus=D", line1.AddInfos);
				var line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(0, line1PackDetails.Count);
				var extraClassificationDetails = new List<ZString>(line1.ExtraClassificationDetails);
				AssertEquals(0, extraClassificationDetails.Count);

				var line2 = new WarehouseCustomsLineDetails(Factory, InvoiceLine2, Invoice, Dictionary, DeclarationDetail);
				AssertEquals((short)1, line2.EntryLineNumber);
				AssertEquals("INB499345", line2.EntryNumber);
				AssertEquals((short)2, line2.PreviousEntryLineNumber);
				AssertEquals("INB891", line2.PreviousEntryNumber);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				AssertEquals("NZ", line2.CountryOfOrigin.Code);
				AssertEquals("AgricultureLicNo=SF798*CAExportCertificate=CA1*CottonFeeExempt=C232*UC_NKCountryOfOrigin=NZ*WHSEntryLineNo=2*WHSEntryNumber=INB891*ZoneStatus=P", line2.AddInfos);
				var line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(0, line2PackDetails.Count);
				extraClassificationDetails = new List<ZString>(line2.ExtraClassificationDetails);
				AssertEquals(0, extraClassificationDetails.Count);
			});
		}

		public void TestPackDetailsForConsumptionFTZ()
		{
			CombineAssertions(() =>
			{
				DeclarationDetail.IsConsumptionFTZ = ZBool.True;
				DeclarationDetail.FallbackAddInfos = FallbackAddInfos;
				var line1 = new WarehouseCustomsLineDetails(Factory, InvoiceLine1, Invoice, Dictionary, DeclarationDetail);
				AssertEquals((short)2, line1.EntryLineNumber);
				AssertEquals("XJ5-EINV1_2", line1.EntryNumber);
				AssertEquals((short)1, line1.PreviousEntryLineNumber);
				AssertEquals("INB123", line1.PreviousEntryNumber);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				AssertEquals("AU", line1.CountryOfOrigin.Code);
				AssertEquals("AgricultureLicNo=SF323*CAExportCertificate=CA1*UC_NKCountryOfOrigin=AU*WHSEntryLineNo=1*WHSEntryNumber=INB123*ZoneStatus=D", line1.AddInfos);
				var line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(0, line1PackDetails.Count);
				var extraClassificationDetails = new List<ZString>(line1.ExtraClassificationDetails);
				AssertEquals(0, extraClassificationDetails.Count);

				var line2 = new WarehouseCustomsLineDetails(Factory, InvoiceLine2, Invoice, Dictionary, DeclarationDetail);
				AssertEquals((short)1, line2.EntryLineNumber);
				AssertEquals("XJ5-EINV2_1", line2.EntryNumber);
				AssertEquals((short)2, line2.PreviousEntryLineNumber);
				AssertEquals("INB891", line2.PreviousEntryNumber);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				AssertEquals("NZ", line2.CountryOfOrigin.Code);
				AssertEquals("AgricultureLicNo=SF798*CAExportCertificate=CA1*CottonFeeExempt=C232*UC_NKCountryOfOrigin=NZ*WHSEntryLineNo=2*WHSEntryNumber=INB891*ZoneStatus=P", line2.AddInfos);
				var line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(0, line2PackDetails.Count);
				extraClassificationDetails = new List<ZString>(line2.ExtraClassificationDetails);
				AssertEquals(0, extraClassificationDetails.Count);
			});
		}

		public void TestPackDetailsForImportByExternalBroker()
		{
			CombineAssertions(() =>
			{
				DeclarationDetail.IsImportByExternalBroker = ZBool.True;
				DeclarationDetail.FallbackAddInfos = FallbackAddInfos;
				var line1 = new WarehouseCustomsLineDetails(Factory, InvoiceLine1, Invoice, Dictionary, DeclarationDetail);
				AssertEquals((short)1, line1.EntryLineNumber);
				AssertEquals("XJ5-EINV1_2", line1.EntryNumber);
				AssertNull(line1.PreviousEntryLineNumber);
				AssertNull(line1.PreviousEntryNumber);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				AssertEquals("AU", line1.CountryOfOrigin.Code);
				AssertEquals("AgricultureLicNo=SF323*CAExportCertificate=CA1*UC_NKCountryOfOrigin=AU*WHSEntryLineNo=1*WHSEntryNumber=INB123*ZoneStatus=D", line1.AddInfos);
				var line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(2, line1PackDetails.Count);
				var line1PackDetail1 = line1PackDetails[0];
				AssertEquals("1", line1PackDetail1.PackID);
				AssertEquals(10, line1PackDetail1.PackageQty);
				AssertEquals(50m, line1PackDetail1.PackedQty);
				var line1PackDetail2 = line1PackDetails[1];
				AssertEquals("2", line1PackDetail2.PackID);
				AssertEquals(3, line1PackDetail2.PackageQty);
				AssertEquals(6m, line1PackDetail2.PackedQty);
				var extraClassificationDetails = new List<ZString>(line1.ExtraClassificationDetails);
				AssertEquals(3, extraClassificationDetails.Count);
				AssertEquals("LineNo=3*Tariff=30303030*InvoiceQuantity=20*InvoiceQuantityUnit=PCS*CustomsQuantity=15*CustomsQuantityUnit=NO*LinePrice=1500*CottonFeeExempt=C323*ZoneStatus=N", extraClassificationDetails[0]);
				AssertEquals("LineNo=4*Tariff=40404040*InvoiceQuantity=40*InvoiceQuantityUnit=BS*CustomsQuantity=30*CustomsQuantityUnit=KG*LinePrice=3000*CottonFeeExempt=C434*ZoneStatus=Z*ParentProductLineNo=1", extraClassificationDetails[1]);
				AssertEquals("Invoice Line5 has SecondQty and ThirdQty Information", "LineNo=5*Tariff=40404040*InvoiceQuantity=40*InvoiceQuantityUnit=BS*CustomsQuantity=30*CustomsQuantityUnit=KG*SecondQty=100*SecondUQ=KG*ThirdQty=950*ThirdUQ=NO*LinePrice=3000*CottonFeeExempt=C434*ParentProductLineNo=1", extraClassificationDetails[2]);

				var line2 = new WarehouseCustomsLineDetails(Factory, InvoiceLine2, Invoice, Dictionary, DeclarationDetail);
				AssertEquals((short)2, line2.EntryLineNumber);
				AssertEquals("XJ5-EINV2_1", line2.EntryNumber);
				AssertNull(line2.PreviousEntryLineNumber);
				AssertNull(line2.PreviousEntryNumber);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				AssertEquals("NZ", line2.CountryOfOrigin.Code);
				AssertEquals("AgricultureLicNo=SF798*CAExportCertificate=CA1*CottonFeeExempt=C232*UC_NKCountryOfOrigin=NZ*WHSEntryLineNo=2*WHSEntryNumber=INB891*ZoneStatus=P", line2.AddInfos);
				var line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(2, line2PackDetails.Count);
				var line2PackDetail1 = line2PackDetails[0];
				AssertEquals("1", line2PackDetail1.PackID);
				AssertEquals(10, line2PackDetail1.PackageQty);
				AssertEquals(60m, line2PackDetail1.PackedQty);
				var line2PackDetail2 = line2PackDetails[1];
				AssertEquals("2", line2PackDetail2.PackID);
				AssertEquals(3, line2PackDetail2.PackageQty);
				AssertEquals(9m, line2PackDetail2.PackedQty);
				extraClassificationDetails = new List<ZString>(line2.ExtraClassificationDetails);
				AssertEquals(0, extraClassificationDetails.Count);
			});
		}

		public void TestSimplePackingGroupIsAssignedAPackageGroupId()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var data = (ZArchitecture.Environment.RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
				addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var inwardDeclaration = helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "BD32432", "SV9", "ENT213", ZDecimal.Zero);
				var inwardInvoice = inwardDeclaration.Invoices[0];
				inwardInvoice.JobComInvoiceLines.RemoveAndDeleteAll();
				var inwardInvoiceLine = inwardInvoice.JobComInvoiceLines.AddNew();
				inwardInvoiceLine.JI_PartNo = helper.Part.OP_PartNum;
				inwardInvoiceLine.JI_InvoiceQuantity = 1000m;
				inwardInvoiceLine.JI_InvoiceUQ = "NO";
				inwardInvoiceLine.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine.JI_CustomsQuantity = 100m;
				inwardInvoiceLine.JI_LinePrice = 10000m;
				inwardInvoiceLine.JI_BondedWhsQuantity = 10m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				var result = inwardDeclaration.PublishShipmentForWHSInward(false);
				AssertEquals(UniversalResult.Internal, result.ResultType);
				var whsReceive = (Warehouse.Integration.IWhsReceive)result.FindJobIfExists();
				AssertNotNull(whsReceive);
				var whsReceiveLines = Factory.Load<Warehouse.Integration.IWhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_WD, whsReceive.PK));
				AssertEquals(1, whsReceiveLines.Length);
				var whsReceiveLine = whsReceiveLines[0];
				AssertNotEquals("WE_PackageGroupId", ZString.Empty, whsReceiveLine.WE_PackageGroupId);
				AssertEquals("WE_PerPackageQty", 100m, whsReceiveLine.WE_PerPackageQty);
			}
		}

		public void TestAddInfoAndExtraClassificationDetails()
		{
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				DeclarationDetail.FallbackAddInfos = FallbackAddInfos;
				var line1 = new WarehouseCustomsLineDetails(factory, InvoiceLine1, Invoice, Dictionary, DeclarationDetail);
				AssertEquals("AgricultureLicNo=SF323*CAExportCertificate=CA1*UC_NKCountryOfOrigin=AU*WHSEntryLineNo=1*WHSEntryNumber=INB123*ZoneStatus=D", line1.AddInfos);
				var extraClassificationDetails = new List<ZString>(line1.ExtraClassificationDetails);
				AssertEquals(3, extraClassificationDetails.Count);
				AssertEquals("LineNo=3*Tariff=30303030*InvoiceQuantity=20*InvoiceQuantityUnit=PCS*CustomsQuantity=15*CustomsQuantityUnit=NO*LinePrice=1500*CottonFeeExempt=C323*ZoneStatus=N", extraClassificationDetails[0]);
				AssertEquals("LineNo=4*Tariff=40404040*InvoiceQuantity=40*InvoiceQuantityUnit=BS*CustomsQuantity=30*CustomsQuantityUnit=KG*LinePrice=3000*CottonFeeExempt=C434*ZoneStatus=Z*ParentProductLineNo=1", extraClassificationDetails[1]);
				AssertEquals("Invoice Line5 has SecondQty and ThirdQty Information", "LineNo=5*Tariff=40404040*InvoiceQuantity=40*InvoiceQuantityUnit=BS*CustomsQuantity=30*CustomsQuantityUnit=KG*SecondQty=100*SecondUQ=KG*ThirdQty=950*ThirdUQ=NO*LinePrice=3000*CottonFeeExempt=C434*ParentProductLineNo=1", extraClassificationDetails[2]);

				var expectedList = new List<string>(Enterprise.Customs.US.DataTransfer.Universal.Extensions.Testing.UniversalExtensionsTest.ExpectedList.Select(x => x.Substring(3)));
				var invoiceLineSchemaDictionary = factory.GetAddInfoSchemaDictionary<JobComInvoiceLine>(USAddInfoSchema.Instance);
				var invoiceLine1AddInfos = InvoiceLine1.AddInfoCollection;
				var invoiceLine3AddInfos = InvoiceLine3.AddInfoCollection;
				InvoiceLine4.AddInfoCollection.Clear();
				DeclarationDetail.FallbackAddInfos = null;
				foreach (var pair in invoiceLineSchemaDictionary)
				{
					factory = new BusinessObjectFactory();
					invoiceLine1AddInfos.Clear();
					invoiceLine1AddInfos.Add(new UniversalAddInfo() { Key = pair.Key, Value = "A" });
					invoiceLine3AddInfos.Clear();
					invoiceLine3AddInfos.Add(new UniversalAddInfo() { Key = pair.Key, Value = "B" });
					if (expectedList.Contains(pair.Key))
					{
						line1 = new WarehouseCustomsLineDetails(factory, InvoiceLine1, Invoice, Dictionary, DeclarationDetail);
						AssertEquals(string.Format("line1.AddInfos should contain '{0}'", pair.Key), pair.Key + "=A", line1.AddInfos);
						extraClassificationDetails = new List<ZString>(line1.ExtraClassificationDetails);
						AssertEquals(2, extraClassificationDetails.Count);
						AssertEquals(string.Format("extraClassificationDetails[0] should contain '{0}'", pair.Key), string.Format("LineNo=3*Tariff=30303030*InvoiceQuantity=20*InvoiceQuantityUnit=PCS*CustomsQuantity=15*CustomsQuantityUnit=NO*LinePrice=1500*{0}=B", pair.Key), extraClassificationDetails[0]);
					}
					else
					{
						line1 = new WarehouseCustomsLineDetails(factory, InvoiceLine1, Invoice, Dictionary, DeclarationDetail);
						AssertEquals(string.Format("line1.AddInfos should not contain '{0}'", pair.Key), "", line1.AddInfos);
						extraClassificationDetails = new List<ZString>(line1.ExtraClassificationDetails);
						AssertEquals(2, extraClassificationDetails.Count);
						AssertEquals(string.Format("extraClassificationDetails[0] should not contain '{0}'", pair.Key), "LineNo=3*Tariff=30303030*InvoiceQuantity=20*InvoiceQuantityUnit=PCS*CustomsQuantity=15*CustomsQuantityUnit=NO*LinePrice=1500", extraClassificationDetails[0]);
					}
				}
			});
		}

		CommercialInvoiceLine invoiceLine1;
		CommercialInvoiceLine InvoiceLine1
		{
			get
			{
				if (invoiceLine1 == null)
				{
					invoiceLine1 = new CommercialInvoiceLine()
					{
						LineNo = 1,
						EntryLineNumber = 2,
						EntryNumber = "EINV1_2",
						Description = "DONGS",
						BondedWarehouseQuantity = 56,
						AddInfoCollection = new List<UniversalAddInfo>(new[]
						{
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3), Value = "1" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_UC_NKCountryOfOrigin.Substring(3), Value = "AU" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_AgricultureLicNo.Substring(3), Value = "SF323" },
							new UniversalAddInfo() { Key = JobDeclaration.Schema.US_WHSEntryFilerCode.Substring(3), Value = "INV" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_WHSEntryNumber.Substring(3), Value = "INB123" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3), Value = ZoneStatusList.Codes.Domestic }
						}),
						AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										new AddInfoGroup()
										{
											Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPackLine },
											AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=1*{1}=50", CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID, CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty))
										},
										new AddInfoGroup()
										{
											Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPackLine },
											AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=2*{1}=6", CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID, CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty))
										}
									}),
						OrganizationAddressCollection = new List<OrganizationAddress>(new[]
						{
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeAddress), OrganizationCode = "ABC32" },
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.Manufacturer), OrganizationCode = "MAN1" }
						})
					};
				}
				return invoiceLine1;
			}
		}

		CommercialInvoiceLine invoiceLine2;
		CommercialInvoiceLine InvoiceLine2
		{
			get
			{
				if (invoiceLine2 == null)
				{
					invoiceLine2 = new CommercialInvoiceLine()
					{
						LineNo = 2,
						EntryLineNumber = 1,
						EntryNumber = "EINV2_1",
						Description = "BOOKS",
						BondedWarehouseQuantity = 69,
						AddInfoCollection = new List<UniversalAddInfo>(new[]
							{
								new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3), Value = "2" },
								new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_CottonFeeExempt.Substring(3), Value = "C232" },
								new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_WHSEntryNumber.Substring(3), Value = "INB891" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3), Value = ZoneStatusList.Codes.PrivilegedForeign }
							}),
						AddInfoGroupCollection = new List<AddInfoGroup>(new[]
										{
											new AddInfoGroup()
											{
												Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPackLine },
												AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=1*{1}=60", CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID, CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty))
											},
											new AddInfoGroup()
											{
												Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPackLine },
												AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=2*{1}=9", CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID, CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty))
											}
										}),
						OrganizationAddressCollection = new List<OrganizationAddress>(new[]
						{
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeAddress), OrganizationCode = "ABC32" },
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.Manufacturer), OrganizationCode = "MAN2" }
						})
					};
				}
				return invoiceLine2;
			}
		}

		CommercialInvoiceLine invoiceLine3;
		CommercialInvoiceLine InvoiceLine3
		{
			get
			{
				if (invoiceLine3 == null)
				{
					invoiceLine3 = new CommercialInvoiceLine()
					{
						LineNo = 3,
						ParentLineNo = 1,
						Description = "BOOKS 3",
						HarmonisedCode = "30303030",
						CustomsQuantity = 15m,
						CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "NO" },
						InvoiceQuantity = 20m,
						InvoiceQuantityUnit = new CodeDescriptionPair() { Code = "PCS" },
						LinePrice = 1500m,
						AddInfoCollection = new List<UniversalAddInfo>(new[]
							{
								new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_CottonFeeExempt.Substring(3), Value = "C323" },
								new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3), Value = ZoneStatusList.Codes.NonPrivilegedForeign }
							}),
						OrganizationAddressCollection = new List<OrganizationAddress>(new[]
						{
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeAddress), OrganizationCode = "ABC32" },
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.Manufacturer), OrganizationCode = "MAN3" }
						})
					};
				}
				return invoiceLine3;
			}
		}

		CommercialInvoiceLine invoiceLine4;
		CommercialInvoiceLine InvoiceLine4
		{
			get
			{
				if (invoiceLine4 == null)
				{
					invoiceLine4 = new CommercialInvoiceLine()
					{
						LineNo = 4,
						Description = "BOOKS 4",
						HarmonisedCode = "40404040",
						CustomsQuantity = 30m,
						CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "KG" },
						InvoiceQuantity = 40m,
						InvoiceQuantityUnit = new CodeDescriptionPair() { Code = "BS" },
						LinePrice = 3000m,
						AddInfoCollection = new List<UniversalAddInfo>(new[]
							{
								new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_CottonFeeExempt.Substring(3), Value = "C434" },
								new UniversalAddInfo() { Key = Constants.AddInfoKeys.InvoiceLine.ParentProductLineNo, Value = "1" },
								new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3), Value = ZoneStatusList.Codes.ZoneRestricted }
							})
							,
						OrganizationAddressCollection = new List<OrganizationAddress>(new[]
						{
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeAddress), OrganizationCode = "ABC32" },
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.Manufacturer), OrganizationCode = "MAN4" }
						})
					};
				}
				return invoiceLine4;
			}
		}

		CommercialInvoiceLine invoiceLine5;
		CommercialInvoiceLine InvoiceLine5
		{
			get
			{
				if (invoiceLine5 == null)
				{
					invoiceLine5 = new CommercialInvoiceLine()
					{
						LineNo = 5,
						Description = "BOOKS 4",
						HarmonisedCode = "40404040",
						CustomsQuantity = 30m,
						CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "KG" },
						InvoiceQuantity = 40m,
						InvoiceQuantityUnit = new CodeDescriptionPair() { Code = "BS" },
						LinePrice = 3000m,
						CustomsSecondQuantity = 100m,
						CustomsSecondQuantityUnit = new CodeDescriptionPair6Char() { Code = "KG" },
						CustomsThirdQuantity = 950m,
						CustomsThirdQuantityUnit = new CodeDescriptionPair6Char() { Code = "NO" },
						AddInfoCollection = new List<UniversalAddInfo>(new[]
							{
								new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_CottonFeeExempt.Substring(3), Value = "C434" },
								new UniversalAddInfo() { Key = Constants.AddInfoKeys.InvoiceLine.ParentProductLineNo, Value = "1" },
							})
					};
				}
				return invoiceLine5;
			}
		}

		CommercialInvoiceHeader invoice;
		CommercialInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV123",
						AddInfoCollection = new List<UniversalAddInfo>(new[]
						{
							new UniversalAddInfo() { Key = JobComInvoiceHeader.Schema.US_UC_NKCountryOfOrigin.Substring(3), Value = "NZ" },
						})
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<CommercialInvoiceLine>(new[] { InvoiceLine1, InvoiceLine2, InvoiceLine3, InvoiceLine4, InvoiceLine5 })));
				}
				return invoice;
			}
		}

		Dictionary<ZString, ZInt> dictionary;
		Dictionary<ZString, ZInt> Dictionary
		{
			get
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<ZString, ZInt>();
					dictionary.Add("1", 10);
					dictionary.Add("2", 3);
				}
				return dictionary;
			}
		}

		DeclarationDetail declarationDetail;
		DeclarationDetail DeclarationDetail
		{
			get
			{
				if (declarationDetail == null)
				{
					declarationDetail = new DeclarationDetail()
					{
						EntryFilerCode = "XJ5",
						EntryNumber = "EN43523",
						WHSEntryFilerCode = "SV9",
						WHSEntryNumber = "EN349834",
						InBondNumber = "INB499345",
						InvoiceLineAddInfosApplicableForInwardWarehousing = Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing()
					};
				}
				return declarationDetail;
			}
		}

		Dictionary<ZString, UniversalAddInfo> fallbackAddInfos;
		Dictionary<ZString, UniversalAddInfo> FallbackAddInfos
		{
			get
			{
				if (fallbackAddInfos == null)
				{
					fallbackAddInfos = new Dictionary<ZString, UniversalAddInfo>();
					fallbackAddInfos.Add(JobComInvoiceLine.Schema.US_CAExportCertificate.Substring(3), new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_CAExportCertificate.Substring(3), Value = "CA1" });
					fallbackAddInfos.Add(JobComInvoiceLine.Schema.US_AgricultureLicNo.Substring(3), new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_AgricultureLicNo.Substring(3), Value = "SF798" });
					fallbackAddInfos.Add(JobComInvoiceLine.Schema.US_UC_NKCountryOfOrigin.Substring(3), new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_UC_NKCountryOfOrigin.Substring(3), Value = "NZ" });
				}
				return fallbackAddInfos;
			}
		}
	}
}
