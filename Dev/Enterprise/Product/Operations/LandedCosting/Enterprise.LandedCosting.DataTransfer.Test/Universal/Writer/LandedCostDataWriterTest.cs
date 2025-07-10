using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalData = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.LandedCosting.DataTransfer.Universal.Testing
{
	sealed class LandedCostDataWriterTest : OrganizationAddressTestHelper
	{
		public void TestLandCostInputMappings()
		{
			var writer = new LandCostInputDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Declaration as BusinessObject)));
			AssertContents(writer.GetDataObject(SetupLandCostInput(Header.CostInputs.AddNew(), InvoiceLine as BusinessObject)));
			AssertContents2(writer.GetDataObject(SetupLandCostInput2(Header.CostInputs.AddNew(), Invoice as BusinessObject)));
		}

		public void TestILandedCostDataWriterImplementation()
		{
			SetupLandCostInput(Header.CostInputs.AddNew(), Invoice as BusinessObject);
			SetupLandCostInput2(Header.CostInputs.AddNew(), InvoiceLine as BusinessObject);
			SetupLandCostInput(Header.CostInputs.AddNew(), InvoiceLine as BusinessObject);

			var history = SetupLandedCostHistoryOnly(Header.Histories.AddNew(), InvoiceLine as BusinessObject, 15m, 20m, 25m);
			SetupLandedLineCostItem(history.LandedLineCostItems.AddNew(), "ST1", 100m);
			SetupLandedLineCostItem(history.LandedLineCostItems.AddNew(), "ST2", 200m);

			var dataWriter = ObjectFactory.New<ILandedCostDataWriter>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Declaration as BusinessObject)), Header);
			AssertEquals(typeof(LandedCostDataWriter), dataWriter.GetType());

			AssertNull("No LandedCostDetail for Invoice", dataWriter.PopulateLandedCostDetail(Invoice as BusinessObject));
			var landedCostDetail = dataWriter.PopulateLandedCostDetail(InvoiceLine as BusinessObject);
			AssertContents(landedCostDetail, 100m, 30m, 0m, 15m, 20m, 25m);
			AssertEquals("landedCostDetail.LandedLineCostItemCollection.Count", 2, landedCostDetail.LandedLineCostItemCollection.Count);
			AssertContents(landedCostDetail.LandedLineCostItemCollection[0], CodeDescriptionPairForTesting.New("ST1", "MPF"), 100m);
			AssertContents(landedCostDetail.LandedLineCostItemCollection[1], CodeDescriptionPairForTesting.New("ST2", "HMF"), 200m);

			AssertNull("No TransportLogisticsCostCollection for GroupHeader", dataWriter.PopulateTransportLogisticsCostCollection(GroupHeader as BusinessObject));
			var transportLogisticsCostCollection = dataWriter.PopulateTransportLogisticsCostCollection(Invoice as BusinessObject);
			AssertEquals("transportLogisticsCostCollection.Count for Invoice", 1, transportLogisticsCostCollection.Count);
			AssertContents(transportLogisticsCostCollection[0]);
			transportLogisticsCostCollection = dataWriter.PopulateTransportLogisticsCostCollection(InvoiceLine as BusinessObject);
			AssertEquals("transportLogisticsCostCollection.Count for InvoiceLine", 2, transportLogisticsCostCollection.Count);
			AssertContents2(transportLogisticsCostCollection[0]);
			AssertContents(transportLogisticsCostCollection[1]);
		}

		public void TestLandedCostGroupMapping()
		{
			var collection = new LandedCostingGroupCollection();
			var landedCostingGroup1 = collection.AddNew();
			landedCostingGroup1.GroupID = 1;
			landedCostingGroup1.GroupName = "TEST 1";
			landedCostingGroup1.CostDistributionCode = landedCostingGroup1.CostDistributionList[0].Code;

			var landedCostingGroup2 = collection.AddNew();
			landedCostingGroup2.GroupID = 2;
			landedCostingGroup2.GroupName = "TEST 2";
			landedCostingGroup2.CostDistributionCode = landedCostingGroup2.CostDistributionList[0].Code;

			var landedCostingGroup3 = collection.AddNew();
			landedCostingGroup3.GroupID = 3;
			landedCostingGroup3.GroupName = "TEST 3";
			landedCostingGroup3.CostDistributionCode = landedCostingGroup3.CostDistributionList[0].Code;

			var landedCostingGroup4 = collection.AddNew();
			landedCostingGroup4.GroupID = 4;
			landedCostingGroup4.GroupName = "TEST 4";
			landedCostingGroup4.CostDistributionCode = landedCostingGroup4.CostDistributionList[0].Code;

			var landedCostingGroup5 = collection.AddNew();
			landedCostingGroup5.GroupID = 5;
			landedCostingGroup5.GroupName = "TEST 5";
			landedCostingGroup5.CostDistributionCode = landedCostingGroup5.CostDistributionList[0].Code;

			var landedCostingGroup6 = collection.AddNew();
			landedCostingGroup6.GroupID = 6;
			landedCostingGroup6.GroupName = "TEST 6";
			landedCostingGroup6.CostDistributionCode = landedCostingGroup6.CostDistributionList[0].Code;
			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var history = SetupLandedCostHistoryOnly(Header.Histories.AddNew(), InvoiceLine as BusinessObject, 10m, 20m, 30m);
			history.LH_LandedCostGroup1 = 10m;
			history.LH_LandedCostGroup2 = 20m;
			history.LH_LandedCostGroup3 = 30m;
			history.LH_LandedCostGroup4 = 40m;
			history.LH_LandedCostGroup5 = 50m;
			history.LH_LandedCostGroup6 = 60m;
			var writer = new LandedCostHistoryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Declaration as BusinessObject)));
			var historyData = writer.GetDataObject(history);
			AssertNotNull("historyData.LandedLineCostItemCollection", historyData.LandedLineCostItemCollection);
			AssertEquals("historyData.LandedLineCostItemCollection.Count", 6, historyData.LandedLineCostItemCollection.Count);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup1), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup1, "TEST 1"), 10m);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup2), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup2, "TEST 2"), 20m);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup3), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup3, "TEST 3"), 30m);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup4), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup4, "TEST 4"), 40m);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup5), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup5, "TEST 5"), 50m);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup6), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup6, "TEST 6"), 60m);

			collection.Remove(landedCostingGroup2);
			collection.Remove(landedCostingGroup5);
			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			historyData = writer.GetDataObject(history);
			AssertNotNull("historyData.LandedLineCostItemCollection", historyData.LandedLineCostItemCollection);
			AssertEquals("historyData.LandedLineCostItemCollection.Count", 6, historyData.LandedLineCostItemCollection.Count);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup1), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup1, "TEST 1"), 10m);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup2), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup2, LandedLineCostType.Descriptions.LandedCostGroup2), 20m);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup3), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup3, "TEST 3"), 30m);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup4), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup4, "TEST 4"), 40m);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup5), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup5, LandedLineCostType.Descriptions.LandedCostGroup5), 50m);
			AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == LandedLineCostType.Codes.LandedCostGroup6), CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup6, "TEST 6"), 60m);
		}

		public void TestLandedCostSpecialTaxMapping_AU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var auJobDeclaration = Factory.BOFactory.New<Integration.Customs.AU.IJobDeclaration>();
				var auGroupHeader = Factory.BOFactory.New<Integration.Customs.AU.IJobComInvoiceGroupHeader>();
				auGroupHeader.JZ_JE = auJobDeclaration.PK;
				var auInvoice = Factory.BOFactory.New<Integration.Customs.AU.IJobComInvoiceHeader>();
				auInvoice.JZ_JZ_GroupInvoiceFK = auGroupHeader.PK;
				auInvoice.JZ_JE = auJobDeclaration.PK;
				auInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Austria;
				var auInvoiceLine = Factory.BOFactory.New<Integration.Customs.AU.IJobComInvoiceLine>();
				auInvoiceLine.JI_JZ = auInvoice.PK;
				auInvoiceLine.JI_LinePrice = 1000m;
				auInvoiceLine.JI_InvoiceQuantity = 10m;
				var auHeader = Factory.New<LandedCostHeader>();
				auHeader.SynchroniseAll((ILandedCostHeader)auJobDeclaration);
				var history = SetupLandedCostHistoryOnly(auHeader.Histories.AddNew(), (BusinessObject)auInvoiceLine, 10m, 20m, 30m);
				history.SetLineValue(10m, "ST1");
				history.SetLineValue(20m, "ST2");
				history.SetLineValue(30m, "ST3");
				var writer = new LandedCostHistoryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, (BusinessObject)auJobDeclaration)));
				var historyData = writer.GetDataObject(history);
				AssertNotNull("historyData.LandedLineCostItemCollection", historyData.LandedLineCostItemCollection);
				AssertEquals("historyData.LandedLineCostItemCollection.Count", 3, historyData.LandedLineCostItemCollection.Count);
				AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == "ST1"), CodeDescriptionPairForTesting.New("ST1", "WET"), 10m);
				AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == "ST2"), CodeDescriptionPairForTesting.New("ST2", "LCT"), 20m);
				AssertContents(historyData.LandedLineCostItemCollection.FirstOrDefault(x => x.CostType.GetCodeAsUpperCase() == "ST3"), CodeDescriptionPairForTesting.New("ST3", "Wood Levy"), 30m);
			}
		}

		public void TestLandedCostHistoryMappings()
		{
			var history = SetupLandedCostHistory(Header.Histories.AddNew(), InvoiceLine as BusinessObject, 10m, 20m, 30m);
			var writer = new LandedCostHistoryDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Declaration as BusinessObject)));
			AssertContents(writer.GetDataObject(history));
		}

		AccChargeCode chargeCode1;
		AccChargeCode ChargeCode1 => chargeCode1 ?? (chargeCode1 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK)));

		AccChargeCode chargeCode2;
		AccChargeCode ChargeCode2
		{
			get
			{
				if (chargeCode2 == null)
				{
					var query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
					query.AddToFilter(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, ChargeCode1.PK);
					chargeCode2 = Factory.LoadTop1<AccChargeCode>(query);
				}
				return chargeCode2;
			}
		}

		LandCostInput SetupLandCostInput(LandCostInput costInput, BusinessObject parent, ZGuid chargeCodePK, ZString chargeDescription, ZDecimal costAmount, ZString costCurrency, ZString distributeCostBy, ZByte landedCostGroup, ZDecimal serviceExRate)
		{
			costInput.LI_ParentID = parent.PK;
			costInput.LI_ParentTableCode = parent.TablePrefix;
			costInput.LI_AC_ChargeCode = chargeCodePK;
			costInput.LI_ChargeDescription = chargeDescription;
			costInput.LI_CostAmount = costAmount;
			costInput.LI_RX_NKCostCurrency = costCurrency;
			costInput.LI_DistributeCostBy = distributeCostBy;
			costInput.LI_LandedCostGroup = landedCostGroup;
			costInput.LI_ServiceExRate = serviceExRate;
			return costInput;
		}

		LandCostInput SetupLandCostInput(LandCostInput history, BusinessObject parent)
		{
			return SetupLandCostInput(history, parent, ChargeCode1.PK, "BOB THE BUILDER", 1000m, Core.Constants.CurrencyCodes.Australia, CostDistributionMechanismList.Codes.Item, 1, 0.9232m);
		}

		LandCostInput SetupLandCostInput2(LandCostInput history, BusinessObject parent)
		{
			return SetupLandCostInput(history, parent, ChargeCode2.PK, "WENDY THE DESTROYER", 2000m, Core.Constants.CurrencyCodes.NewZealand, CostDistributionMechanismList.Codes.LineValue, 2, 0.8854m);
		}

		void AssertContents(TransportLogisticsCost transportLogisticsCostData)
		{
			AssertContents(transportLogisticsCostData, CodeDescriptionPairForTesting.New(ChargeCode1.AC_Code, ChargeCode1.AC_Desc), "BOB THE BUILDER", 1000m, CodeDescriptionPairForTesting.New(Core.Constants.CurrencyCodes.Australia, "Australian Dollar"), CodeDescriptionPairForTesting.New(CostDistributionMechanismList.Codes.Item, CostDistributionMechanismList.Descriptions.Item), CodeDescriptionPairForTesting.New("1", "Origin Charges"), 0.9232m);
		}

		void AssertContents2(TransportLogisticsCost transportLogisticsCostData)
		{
			AssertContents(transportLogisticsCostData, CodeDescriptionPairForTesting.New(ChargeCode2.AC_Code, ChargeCode2.AC_Desc), "WENDY THE DESTROYER", 2000m, CodeDescriptionPairForTesting.New(Core.Constants.CurrencyCodes.NewZealand, "New Zealand Dollar"), CodeDescriptionPairForTesting.New(CostDistributionMechanismList.Codes.LineValue, CostDistributionMechanismList.Descriptions.LineValue), CodeDescriptionPairForTesting.New("2", "Intl Freight Charges"), 0.8854m);
		}

		void AssertContents(TransportLogisticsCost transportLogisticsCostData, ICodeDescription chargeCode, ZString? chargeDescription, ZDecimal? costAmount, ICodeDescription costCurrency, ICodeDescription distributeCostBy, ICodeDescription landedCostGroup, ZDecimal? serviceExRate)
		{
			AssertNotNull("Precondition: transportLogisticsCostData", transportLogisticsCostData);

			CombineAssertions(delegate
			{
				if (chargeCode == null)
				{
					AssertNull("transportLogisticsCostData.ChargeCode", transportLogisticsCostData.ChargeCode);
				}
				else
				{
					AssertNotNull("transportLogisticsCostData.ChargeCode", transportLogisticsCostData.ChargeCode);
					AssertEquals("transportLogisticsCostData.ChargeCode.Code", chargeCode.Code, transportLogisticsCostData.ChargeCode.Code);
					AssertEquals("transportLogisticsCostData.ChargeCode.Description", chargeCode.Description, transportLogisticsCostData.ChargeCode.Description);
				}
				AssertEquals("transportLogisticsCostData.ChargeDescription", chargeDescription, transportLogisticsCostData.ChargeDescription);
				AssertEquals("transportLogisticsCostData.CostAmount", costAmount, transportLogisticsCostData.CostAmount);
				if (costCurrency == null)
				{
					AssertNull("transportLogisticsCostData.CostCurrency", transportLogisticsCostData.CostCurrency);
				}
				else
				{
					AssertNotNull("transportLogisticsCostData.CostCurrency", transportLogisticsCostData.CostCurrency);
					AssertEquals("transportLogisticsCostData.CostCurrency.Code", costCurrency.Code, transportLogisticsCostData.CostCurrency.Code);
					AssertEquals("transportLogisticsCostData.CostCurrency.Description", costCurrency.Description, transportLogisticsCostData.CostCurrency.Description);
				}
				if (distributeCostBy == null)
				{
					AssertNull("transportLogisticsCostData.DistributeCostBy", transportLogisticsCostData.DistributeCostBy);
				}
				else
				{
					AssertNotNull("transportLogisticsCostData.DistributeCostBy", transportLogisticsCostData.DistributeCostBy);
					AssertEquals("transportLogisticsCostData.DistributeCostBy.Code", distributeCostBy.Code, transportLogisticsCostData.DistributeCostBy.Code);
					AssertEquals("transportLogisticsCostData.DistributeCostBy.Description", distributeCostBy.Description, transportLogisticsCostData.DistributeCostBy.Description);
				}
				if (landedCostGroup == null)
				{
					AssertNull("transportLogisticsCostData.LandedCostGroup", transportLogisticsCostData.LandedCostGroup);
				}
				else
				{
					AssertNotNull("transportLogisticsCostData.LandedCostGroup", transportLogisticsCostData.LandedCostGroup);
					AssertEquals("transportLogisticsCostData.LandedCostGroup.Code", landedCostGroup.Code, transportLogisticsCostData.LandedCostGroup.Code);
					AssertEquals("transportLogisticsCostData.LandedCostGroup.Description", landedCostGroup.Description, transportLogisticsCostData.LandedCostGroup.Description);
				}
				AssertEquals("transportLogisticsCostData.ServiceExRate", serviceExRate, transportLogisticsCostData.ServiceExRate);
			});
		}

		Integration.Customs.US.IJobDeclaration declaration;
		Integration.Customs.US.IJobDeclaration Declaration => declaration ?? (declaration = Factory.BOFactory.New<Integration.Customs.US.IJobDeclaration>());

		Integration.Customs.US.IJobComInvoiceGroupHeader groupHeader;
		Integration.Customs.US.IJobComInvoiceGroupHeader GroupHeader
		{
			get
			{
				if (groupHeader == null)
				{
					groupHeader = Factory.BOFactory.New<Integration.Customs.US.IJobComInvoiceGroupHeader>();
					groupHeader.JZ_JE = Declaration.PK;
				}
				return groupHeader;
			}
		}

		Integration.Customs.US.IJobComInvoiceHeader invoice;
		Integration.Customs.US.IJobComInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = Factory.BOFactory.New<Integration.Customs.US.IJobComInvoiceHeader>();
					invoice.JZ_JZ_GroupInvoiceFK = GroupHeader.PK;
					invoice.JZ_JE = Declaration.PK;
					invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				}
				return invoice;
			}
		}

		Integration.Customs.US.IJobComInvoiceLine invoiceLine;
		Integration.Customs.US.IJobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = Factory.BOFactory.New<Integration.Customs.US.IJobComInvoiceLine>();
					invoiceLine.JI_JZ = Invoice.PK;
					invoiceLine.JI_LinePrice = 1000m;
					invoiceLine.JI_InvoiceQuantity = 10m;
				}
				return invoiceLine;
			}
		}

		LandedCostHeader header;
		LandedCostHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<LandedCostHeader>();
					header.SynchroniseAll(Declaration as ILandedCostHeader);
				}
				return header;
			}
		}

		LandedCostHistory SetupLandedCostHistoryOnly(LandedCostHistory history, BusinessObject parent, ZDecimal markup1, ZDecimal markup2, ZDecimal markup3)
		{
			history.LH_ParentID = parent.PK;
			history.LH_ParentTableCode = parent.TablePrefix;
			history.LH_LandedCostMarginPercent1 = markup1;
			history.LH_LandedCostMarginPercent2 = markup2;
			history.LH_LandedCostMarginPercent3 = markup3;
			return history;
		}

		LandedCostHistory SetupLandedCostHistory(LandedCostHistory history, BusinessObject parent, ZDecimal markup1, ZDecimal markup2, ZDecimal markup3)
		{
			history = SetupLandedCostHistoryOnly(history, parent, markup1, markup2, markup3);
			SetupLandedLineCostItem(history.LandedLineCostItems.AddNew(), "ENT", 102.23m);
			SetupLandedLineCostItem(history.LandedLineCostItems.AddNew(), LandedLineCostType.Codes.LandedCostGroup1, 689.35m);
			return history;
		}

		Business.LandedLineCostItem SetupLandedLineCostItem(Business.LandedLineCostItem landedLineCostItem, ZString costType, ZDecimal costAmount)
		{
			landedLineCostItem.LZ_CostType = costType;
			landedLineCostItem.LZ_CostAmount = costAmount;
			return landedLineCostItem;
		}

		void AssertContents(LandedCostDetail landedCostDetailData)
		{
			AssertContents(landedCostDetailData, 100m, 10.223m, 68.935m, 10m, 20m, 30m);
			AssertEquals("landedCostDetailData.LandedLineCostItemCollection.Count", 2, landedCostDetailData.LandedLineCostItemCollection.Count);
			var costItem1 = landedCostDetailData.LandedLineCostItemCollection[0];
			var costItem2 = landedCostDetailData.LandedLineCostItemCollection[1];
			if (costItem2.CostType.GetCodeAsUpperCase() == "ENT")
			{
				costItem1 = landedCostDetailData.LandedLineCostItemCollection[1];
				costItem2 = landedCostDetailData.LandedLineCostItemCollection[0];
			}
			AssertContents(costItem1, CodeDescriptionPairForTesting.New("ENT", "Entry Fees"), 102.23m);
			AssertContents(costItem2, CodeDescriptionPairForTesting.New(LandedLineCostType.Codes.LandedCostGroup1, "Origin Charges"), 689.35m);
		}

		void AssertContents(LandedCostDetail landedCostDetailData, ZDecimal goodsItemCostPerUnit, ZDecimal customsCostPerUnit, ZDecimal transportAndLogisticsCostPerUnit, ZDecimal markUp1, ZDecimal markUp2, ZDecimal markUp3)
		{
			AssertNotNull("Precondition: landedCostDetailData", landedCostDetailData);

			CombineAssertions(delegate
			{
				AssertEquals("landedCostDetailData.GoodsItemCostPerUnit", goodsItemCostPerUnit, landedCostDetailData.GoodsItemCostPerUnit);
				AssertEquals("landedCostDetailData.CustomsCostPerUnit", customsCostPerUnit, landedCostDetailData.CustomsCostPerUnit);
				AssertEquals("landedCostDetailData.TransportAndLogisticsCostPerUnit", transportAndLogisticsCostPerUnit, landedCostDetailData.TransportAndLogisticsCostPerUnit);
				AssertEquals("landedCostDetailData.MarkUp1", markUp1, landedCostDetailData.MarkUp1);
				AssertEquals("landedCostDetailData.MarkUp2", markUp2, landedCostDetailData.MarkUp2);
				AssertEquals("landedCostDetailData.MarkUp3", markUp3, landedCostDetailData.MarkUp3);
			});
		}

		void AssertContents(UniversalData.LandedLineCostItem landedLineCostItemData, ICodeDescription costType, ZDecimal costAmount)
		{
			AssertNotNull("Precondition: landedLineCostItemData", landedLineCostItemData);

			CombineAssertions(delegate
			{
				AssertNotNull("landedLineCostItemData.CostType", landedLineCostItemData.CostType);
				AssertEquals("landedLineCostItemData.CostType.Code", costType.Code, landedLineCostItemData.CostType.Code);
				AssertEquals("landedLineCostItemData.CostType.Description", costType.Description, landedLineCostItemData.CostType.Description);
				AssertEquals("landedLineCostItemData.CostAmount", costAmount, landedLineCostItemData.CostAmount);
			});
		}

		sealed class CodeDescriptionPairForTesting : ICodeDescription
		{
			CodeDescriptionPairForTesting(string code, string description)
			{
				Code = code;
				Description = description;
			}

			public static ICodeDescription New(string code, string description, int maxLength = 0)
			{
				if (!string.IsNullOrEmpty(description) && maxLength > 0 && description.Length > maxLength)
				{
					description = description.Substring(0, maxLength);
				}

				return new CodeDescriptionPairForTesting(code, description);
			}

			public object PK { get; private set; }

			public string Code { get; private set; }

			public string Description { get; private set; }
		}
	}
}
