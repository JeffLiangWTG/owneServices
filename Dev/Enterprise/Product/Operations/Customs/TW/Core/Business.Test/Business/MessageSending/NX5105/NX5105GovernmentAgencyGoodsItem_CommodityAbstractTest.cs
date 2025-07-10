using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class NX5105GovernmentAgencyGoodsItem_CommodityAbstractTest<TCommodity> : TestCaseWithFactory
		where TCommodity : ICommodity
	{
		#region Commercial Categorization ID
		[ExpectNoExceptions]
		public void TestCommodity_CommercialCategorizationID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			CombineAssertions(() =>
			{
				invoiceLine.JI_Model = "";
				NUnit.Framework.Assert.That(commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Commodity.CommercialCategorizationID should be");
				invoiceLine.JI_Model = "model";
				NUnit.Framework.Assert.That(commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo("model").Using(CustomComparers.TypeComparison), "Commodity.CommercialCategorizationID should be");
				invoiceLine.JI_Model = "NO.008 ";
				NUnit.Framework.Assert.That(commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo("NO.008 ").Using(CustomComparers.TypeComparison), "Commodity.CommercialCategorizationID should be");
			});

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.L1;
			invoiceLine.JI_Model = ZString.Empty;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "Commodity.CommercialCategorizationID should be");
		}

		#endregion
		#region Description
		[ExpectNoExceptions]
		public virtual void TestCommodity_Description()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0000.00.00.00Y";
			line1.JI_Description = ZString.Empty;
			line1.JI_DeclarationGoodsDescription = "Declaration Goods Description";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var entryLine1 = (CusEntryLine)declaration.CustomsEntryHeaders[0].MergedLines[0];
			var commodity = GetCommodity(entryLine1, entryLine1.RandomLine);
			CombineAssertions(() =>
			{
				entryLine1.CL_Description = "EntryLine Description";
				NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo("EntryLine Description").Using(CustomComparers.TypeComparison));
				entryLine1.CL_Description = ZString.Empty;
				NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo("Declaration Goods Description").Using(CustomComparers.TypeComparison));
				entryLine1.CL_Description = "NO.008 ";
				NUnit.Framework.Assert.That(commodity.Description, NUnit.Framework.Is.EqualTo("NO.008 ").Using(CustomComparers.TypeComparison));
			});
		}

		#endregion
		#region Name
		[ExpectNoExceptions]
		public void TestCommodity_Name()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			CombineAssertions(() =>
			{
				invoiceLine.JI_BrandName = "";
				NUnit.Framework.Assert.That(commodity.Name, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Commodity.Name should be");
				invoiceLine.JI_BrandName = "brandname";
				NUnit.Framework.Assert.That(commodity.Name, NUnit.Framework.Is.EqualTo("brandname").Using(CustomComparers.TypeComparison), "Commodity.Name should be");
				invoiceLine.JI_BrandName = "NO.008 ";
				NUnit.Framework.Assert.That(commodity.Name, NUnit.Framework.Is.EqualTo("NO.008 ").Using(CustomComparers.TypeComparison), "Commodity.Name should be");
			});

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.L1;
			invoiceLine.JI_BrandName = ZString.Empty;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.Name, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "Commodity.Name should be");
		}

		#endregion
		#region Chinese Description
		[ExpectNoExceptions]
		public virtual void TestCommodity_ChineseDescription()
		{
			CombineAssertions(() =>
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				var entryLine = Factory.New<CusEntryLine>();
				invoiceLine.JI_CL = entryLine.PK;
				var commodity = GetCommodity(entryLine, entryLine.RandomLine);
				invoiceLine.JI_NDescription = "";
				NUnit.Framework.Assert.That(commodity.ChineseDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Commodity.ChineseDescription should be");
				invoiceLine.JI_NDescription = "chinese desc";
				NUnit.Framework.Assert.That(commodity.ChineseDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Commodity.ChineseDescription should be");
			});
		}

		#endregion
		#region English Description
		[ExpectNoExceptions]
		public virtual void TestCommodity_EnglishDescription()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.JI_Description = "";
			NUnit.Framework.Assert.That(commodity.EnglishDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");
			invoiceLine.JI_Description = "english desc";
			NUnit.Framework.Assert.That(commodity.EnglishDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Commodity.EnglishDescription should be");
		}

		#endregion
		#region CITES Import Permit ID
		[ExpectNoExceptions]
		public void TestCommodity_CITESImportPermitID()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.CitesPermit = "";
			NUnit.Framework.Assert.That(commodity.CITESImportPermitID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Commodity.CITESImportPermitID should be");
			invoiceLine.CitesPermit = "CITES";
			NUnit.Framework.Assert.That(commodity.CITESImportPermitID, NUnit.Framework.Is.EqualTo("CITES").Using(CustomComparers.TypeComparison), "Commodity.CITESImportPermitID should be");
		}

		#endregion
		#region FTA Tariff Code
		[ExpectNoExceptions]
		public void TestCommodity_FTATariffCode()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.FTATariffCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Commodity.FTATariffCode should be empty when JI_PrimaryPreference is empty.");
			invoiceLine.JI_PrimaryPreference = "PR2";
			NUnit.Framework.Assert.That(commodity.FTATariffCode, NUnit.Framework.Is.EqualTo("PT").Using(CustomComparers.TypeComparison), "Commodity.FTATariffCode should be 'PT' when JI_PrimaryPreference is 'PR2'.");
			invoiceLine.JI_PrimaryPreference = "PT2";
			NUnit.Framework.Assert.That(commodity.FTATariffCode, NUnit.Framework.Is.EqualTo("PT").Using(CustomComparers.TypeComparison), "Commodity.FTATariffCode should be 'PT' when JI_PrimaryPreference is 'PT2'.");
			invoiceLine.JI_PrimaryPreference = "PR1";
			NUnit.Framework.Assert.That(commodity.FTATariffCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Commodity.FTATariffCode should be empty when JI_PrimaryPreference is not 'PR2'.");
		}

		#endregion
		#region SHTC Import Permit ID
		[ExpectNoExceptions]
		public void TestCommodity_SHTCImportPermitID()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.HighTechLicense = "";
			NUnit.Framework.Assert.That(commodity.SHTCImportPermitID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Commodity.SHTCImportPermitID should be");
			invoiceLine.HighTechLicense = "high tech licence";
			NUnit.Framework.Assert.That(commodity.SHTCImportPermitID, NUnit.Framework.Is.EqualTo("high tech licence").Using(CustomComparers.TypeComparison), "Commodity.SHTCImportPermitID should be");
		}

		#endregion
		#region Additional Documents
		[ExpectNoExceptions]
		public void TestCommodity_AdditionalDocuments()
		{
			var invoiceLine1 = Factory.New<JobComInvoiceLine>();
			var invoiceLine2 = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine1.JI_EPTDigit1 = "A";
			invoiceLine1.JI_EPTDigit2 = "0";
			invoiceLine1.JI_EPTDigit3 = "1";
			var item1 = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
			item1.JG_ReferenceNumber = "AA";
			var item2 = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
			item2.JG_ReferenceNumber = "BB";
			var item3 = invoiceLine2.AssignedJobComInvLineRefsCollection.AddNew();
			item3.JG_ReferenceNumber = "AA";
			CombineAssertions(() =>
			{
				var commodity = GetCommodity(entryLine, entryLine.RandomLine);
				var firstDocument = commodity.AdditionalDocuments.ElementAt(0);
				var secondDocument = commodity.AdditionalDocuments.ElementAt(1);
				var thirdDocument = commodity.AdditionalDocuments.ElementAt(2);
				NUnit.Framework.Assert.That(commodity.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(3), "Commodity.AdditionalDocuments.Count() should be ");
				NUnit.Framework.Assert.That(firstDocument.ID, NUnit.Framework.Is.EqualTo("A01").Using(CustomComparers.TypeComparison), "Commodity.AdditionalDocuments[0].ID should be ");
				NUnit.Framework.Assert.That(secondDocument.ID, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison), "Commodity.AdditionalDocuments[1].ID should be ");
				NUnit.Framework.Assert.That(thirdDocument.ID, NUnit.Framework.Is.EqualTo("BB").Using(CustomComparers.TypeComparison), "Commodity.AdditionalDocuments[2].ID should be ");
			}

			);
			invoiceLine1.AssignedJobComInvLineRefsCollection.RemoveAndDeleteAll();
			invoiceLine2.AssignedJobComInvLineRefsCollection.RemoveAndDeleteAll();
			CombineAssertions(() =>
			{
				for (int i = 0; i < 20; i++)
				{
					var item = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
					item.JG_ReferenceNumber = ZString.Format("B{0}", i);
				}

				var commodity = GetCommodity(entryLine, entryLine.RandomLine);
				NUnit.Framework.Assert.That(commodity.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(10), "Commodity.AdditionalDocuments.Count() should be ");
				NUnit.Framework.Assert.That(commodity.AdditionalDocuments.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("A01").Using(CustomComparers.TypeComparison), "Commodity.AdditionalDocuments[0].ID should be ");
				NUnit.Framework.Assert.That(commodity.AdditionalDocuments.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("B0").Using(CustomComparers.TypeComparison), "Commodity.AdditionalDocuments[1].ID should be ");
			}

			);
			CombineAssertions(() =>
			{
				invoiceLine1.JI_EPTDigit1 = "";
				invoiceLine1.JI_EPTDigit2 = "";
				invoiceLine1.JI_EPTDigit3 = "";
				var commodity = GetCommodity(entryLine, entryLine.RandomLine);
				var firstDocument = commodity.AdditionalDocuments.ElementAt(0);
				NUnit.Framework.Assert.That(commodity.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(10), "Commodity.AdditionalDocuments.Count() should be ");
				NUnit.Framework.Assert.That(commodity.AdditionalDocuments.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("B0").Using(CustomComparers.TypeComparison), "Commodity.AdditionalDocuments[0].ID should be ");
				NUnit.Framework.Assert.That(commodity.AdditionalDocuments.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("B1").Using(CustomComparers.TypeComparison), "Commodity.AdditionalDocuments[1].ID should be ");
			}

			);
		}

		#endregion
		#region Classifications
		[ExpectNoExceptions]
		public void TestCommodity_Classifications()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.JI_Tariff = "87123456";
			NUnit.Framework.Assert.That(commodity.Classifications.Count(), NUnit.Framework.Is.EqualTo(1), "Commodity.Classifications.Cound() should be ");
			NUnit.Framework.Assert.That(commodity.Classifications.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("87123456").Using(CustomComparers.TypeComparison), "Commodity.Classifications[0].ID should be ");
			NUnit.Framework.Assert.That(commodity.Classifications.ElementAt(0).IdentificationTypeCode, NUnit.Framework.Is.EqualTo("HS").Using(CustomComparers.TypeComparison), "Commodity.Classifications[0].IdentificationTypeCode should be ");
			invoiceLine.JI_HazMatCode = "1100";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			NUnit.Framework.Assert.That(commodity.Classifications.Count(), NUnit.Framework.Is.EqualTo(2), "Commodity.Classifications.Cound() should be ");
			NUnit.Framework.Assert.That(commodity.Classifications.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("1100").Using(CustomComparers.TypeComparison), "Commodity.Classifications[1].ID should be ");
			NUnit.Framework.Assert.That(commodity.Classifications.ElementAt(1).IdentificationTypeCode, NUnit.Framework.Is.EqualTo("ZZZ").Using(CustomComparers.TypeComparison), "Commodity.Classifications[1].IdentificationTypeCode should be ");
			invoiceLine.UNDGs.AddNew().DI_DG = MasterFiles.Business.UNDGSubstanceLoader.LoadSubstances(Factory, "1100", "", "IMO").First().PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			NUnit.Framework.Assert.That(commodity.Classifications.Count(), NUnit.Framework.Is.EqualTo(2), "Commodity.Classifications.Cound() should be ");
			NUnit.Framework.Assert.That(commodity.Classifications.ElementAt(1).IdentificationTypeCode, NUnit.Framework.Is.EqualTo("SSO").Using(CustomComparers.TypeComparison), "Commodity.Classifications[0].IdentificationTypeCode should be ");
			NUnit.Framework.Assert.That(commodity.Classifications.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("1100").Using(CustomComparers.TypeComparison), "Commodity.Classifications[0].ID should be ");
		}

		#endregion
		#region Constituent
		[ExpectNoExceptions]
		public virtual void TestCommodity_Constituent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.JI_Compositions = ZString.Empty;
			invoiceLine.JI_ProductGrade = "LevelID";
			invoiceLine.JI_ProductThickness = "Thickness";

			IConstituent constituent;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(commodity.Constituent, NUnit.Framework.Is.EqualTo(default(IConstituent)));
				invoiceLine.JI_Compositions = "NO.008 ";
				constituent = commodity.Constituent;
				NUnit.Framework.Assert.That(constituent.ElementDescription, NUnit.Framework.Is.EqualTo("NO.008 ").Using(CustomComparers.TypeComparison), "Commodity.Constituent.ElementDescription should be");
				NUnit.Framework.Assert.That(constituent.LevelID, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.Constituent.LevelID should be");
				NUnit.Framework.Assert.That(constituent.Thickness, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.Constituent.Thickness should be");
			});

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.L1;
			invoiceLine.JI_Compositions = ZString.Empty;
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			constituent = commodity.Constituent;
			NUnit.Framework.Assert.That(constituent.ElementDescription, NUnit.Framework.Is.EqualTo("NIL").Using(CustomComparers.TypeComparison), "Commodity.Constituent.ElementDescription should be");
		}

		#endregion
		#region Duty Tax Fee
		[ExpectNoExceptions]
		public virtual void TestCommodity_DutyTaxFee()
		{
			var entryLine = Factory.New<CusEntryLine>();
			ICommodity goodsShipment = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(goodsShipment.DutyTaxFee, NUnit.Framework.Is.TypeOf(typeof(NX5105Commodity_DutyTaxFee)));
		}

		#endregion
		#region Government Procedure
		[ExpectNoExceptions]
		public void TestCommodity_GovernmentProcedure()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Procedure = "31";
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.GovernmentProcedure.CurrentCode, NUnit.Framework.Is.EqualTo("31").Using(CustomComparers.TypeComparison), "Commodity.GovernmentProcedure.CurrentCode should be");
		}

		#endregion
		#region Invoice Line
		[ExpectNoExceptions]
		public virtual void TestCommodity_InvoiceLine()
		{
			var entryLine = Factory.New<CusEntryLine>();
			ICommodity goodsShipment = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(goodsShipment.InvoiceLine, NUnit.Framework.Is.TypeOf(typeof(NX5105Commodity_InvoiceLine)));
		}

		#endregion
		#region Previous Document
		[ExpectNoExceptions]
		public void TestCommodity_PreviousDocument()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.PreviousPermitNo = ZString.Empty;
			NUnit.Framework.Assert.That(commodity.PreviousDocument, NUnit.Framework.Is.EqualTo(default(IPreviousDocument)));
			invoiceLine.PreviousPermitNo = "132456";
			NUnit.Framework.Assert.That(commodity.PreviousDocument.ID, NUnit.Framework.Is.EqualTo("132456").Using(CustomComparers.TypeComparison), "Commodity.PreviousDocument.ID should be");
		}

		#endregion
		#region Commodity Numbers
		[ExpectNoExceptions]
		public void TestCommodity_CommodityNumbers()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			var commodityNumbers = commodity.CommodityNumbers;
			NUnit.Framework.Assert.That(commodityNumbers.Count(), NUnit.Framework.Is.EqualTo(0), "Commodity.CommodityNumbers.Count() should be");
			invoiceLine.JI_CustomsOwnerPartNo = "owner";
			NUnit.Framework.Assert.That(commodityNumbers.Count(), NUnit.Framework.Is.EqualTo(1), "Commodity.CommodityNumbers.Count() should be");
			NUnit.Framework.Assert.That(commodityNumbers.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("owner").Using(CustomComparers.TypeComparison), "Commodity.CommodityNumbers[0].ID should be");
			NUnit.Framework.Assert.That(commodityNumbers.ElementAt(0).IdentifierTypeCode, NUnit.Framework.Is.EqualTo("BP").Using(CustomComparers.TypeComparison), "Commodity.CommodityNumbers[0].IdentifierTypeCode should be");
			invoiceLine.JI_CustomsSupplierPartNo = "suppl";
			NUnit.Framework.Assert.That(commodityNumbers.Count(), NUnit.Framework.Is.EqualTo(2), "Commodity.CommodityNumbers.Count() should be");
			NUnit.Framework.Assert.That(commodityNumbers.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("suppl").Using(CustomComparers.TypeComparison), "Commodity.CommodityNumbers[1].ID should be");
			NUnit.Framework.Assert.That(commodityNumbers.ElementAt(1).IdentifierTypeCode, NUnit.Framework.Is.EqualTo("SA").Using(CustomComparers.TypeComparison), "Commodity.CommodityNumbers[1].IdentifierTypeCode should be");
		}

		#endregion
		#region Vehicle
		[ExpectNoExceptions]
		public void TestCommodity_Vehicle()
		{
			var entryLine = Factory.New<CusEntryLine>();
			ICommodity goodsShipment = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(goodsShipment.Vehicle, NUnit.Framework.Is.EqualTo(default(IVehicle)));
			entryLine = Factory.New<CusEntryLine>();
			var invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			var chassis = invoiceLine.ChassisJobComInvLineRefsCollection.AddNew();
			chassis.JG_ReferenceNumber = "CHAS";
			goodsShipment = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(goodsShipment.Vehicle, NUnit.Framework.Is.Not.EqualTo(default(IVehicle)));
		}

		#endregion
		#region Wine
		[ExpectNoExceptions]
		public virtual void TestCommodity_Wine()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Wine, NUnit.Framework.Is.EqualTo(default(IWine)));
			invoiceLine.JI_AlcoholPercentage = 1;
			entryLine.RefreshInvoiceLines();
			commodity = GetCommodity(entryLine, invoiceLine);
			NUnit.Framework.Assert.That(commodity.Wine, NUnit.Framework.Is.TypeOf(typeof(NX5105CommodityWine)));
		}

		#endregion
		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				GetCommodity(null, null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var header = Factory.New<CusEntryHeader>();
				var line = header.MergedLines.AddNew();
				line.InvoiceLines.AddNew();
				GetCommodity(line, line.RandomLine);
			}

			);
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFeeQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.Invoices.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			TariffDataForTestHelper.NewData(Factory);
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			var entryLine = cusEntryHeader.MergedLines.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "87031000004";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_DtyPymntMthd = "CAS";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1m;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeQuantity.TaxRateNumeric, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeQuantity.DutyUnitCode, NUnit.Framework.Is.EqualTo("KGM").Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "87031000002";
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeQuantity, NUnit.Framework.Is.EqualTo(default(IDutyTaxFeeQuantity)));

			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "DTA";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 0.2m;
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeQuantity, NUnit.Framework.Is.EqualTo(default(IDutyTaxFeeQuantity)));

			lineFee.CF_ChargeType = "DTS";
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeQuantity.TaxRateNumeric, NUnit.Framework.Is.EqualTo(0.2m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeQuantity.DutyUnitCode, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFeeAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.Invoices.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			TariffDataForTestHelper.NewData(Factory);
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			var entryLine = cusEntryHeader.MergedLines.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "PR";
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "87031000004";
			invoiceLine.JI_PrimaryPreference = "STD";
			invoiceLine.JI_DtyPymntMthd = "CAS";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_EnteredUnitPrice = 1000m;
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1m;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeAmount.TaxRateNumeric, NUnit.Framework.Is.EqualTo(0.3m).Using(CustomComparers.TypeComparison));

			invoiceLine.JI_Tariff = "87031000005";
			commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(default(IDutyTaxFeeAmount)));

			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = "DTS";
			lineFee.CF_MethodOfCalculation = "A";
			lineFee.CF_Rate = 0.2m;
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeAmount, NUnit.Framework.Is.EqualTo(default(IDutyTaxFeeAmount)));

			lineFee.CF_ChargeType = "DTA";
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeAmount.TaxRateNumeric, NUnit.Framework.Is.EqualTo(0.2m).Using(CustomComparers.TypeComparison));

			lineFee.CF_MethodOfPayment = ZString.Empty;
			lineFee.CF_Rate = 0m;
			NUnit.Framework.Assert.That(commodity.DutyTaxFeeAmount.TaxRateNumeric, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDutyOtherTaxFees()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var rateType = helper.CreateCusRateType("TW", "DTY");
			rateType.ZZR_CustomsValueFormula = "CV";
			var comRateType = helper.CreateCusRateType("TW", "COM");
			comRateType.ZZR_CustomsValueFormula = "CV + DTA + DTS";
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, "DTA", rateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "DTS", rateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "TPF", comRateType.PK);
			Factory.Save();
			var header = Factory.New<CusEntryHeader>();
			var line = header.MergedLines.AddNew();
			var invoiceLine = line.InvoiceLines.AddNew() as JobComInvoiceLine;
			var fees = line.Fees;
			var fee1 = fees.AddNew();
			fee1.CF_Rate = 0.00001M;
			fee1.CF_ChargeType = RefCusTaxOrFeeCodes.TPF;
			fee1.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			fee1.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
			var fee2 = fees.AddNew();
			fee2.CF_Rate = 0.00001M;
			fee2.CF_ChargeType = "XXX";
			fee2.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			fee2.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
			var fee3 = line.Fees.AddNew();
			fee3.CF_ChargeType = "HWS";
			fee3.CF_MethodOfCalculation = "A";
			fee3.CF_Rate = 1M;
			fee3.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			var commodity = GetCommodity(line, line.RandomLine);
			var dutyOtherTaxFees = commodity.DutyOtherTaxFees;
			NUnit.Framework.Assert.That(dutyOtherTaxFees.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.TypeCode == "XXX"), NUnit.Framework.Is.EqualTo(default(IDutyOtherTaxFee)));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.TypeCode == "B51" && x.MethodCode == "1" && x.TaxRateNumeric == 0.00001M), NUnit.Framework.Is.Not.EqualTo(default(IDutyOtherTaxFee)));
			NUnit.Framework.Assert.That(dutyOtherTaxFees.SingleOrDefault(x => x.TypeCode == "B32" && x.MethodCode == "2" && x.TaxRateNumeric == 1M), NUnit.Framework.Is.Not.EqualTo(default(IDutyOtherTaxFee)));
			fee1.CF_Rate = 0M;
			fee3.CF_Rate = 0M;
			commodity = GetCommodity(line, line.RandomLine);
			dutyOtherTaxFees = commodity.DutyOtherTaxFees;
			NUnit.Framework.Assert.That(dutyOtherTaxFees.Count(), NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public virtual void TestBarCode()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.JI_BarCode = "013";
			NUnit.Framework.Assert.That(commodity.BarCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.BarCode should be");
		}

		[ExpectNoExceptions]
		public virtual void TestCommodity_CommodityRelatedPackaging()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.JI_InnerPackType = "1";
			invoiceLine.JI_InnerPackingMaterial = "456";
			invoiceLine.JI_InnerPackDescription = new ZString('A', 200);
			NUnit.Framework.Assert.That(commodity.CommodityRelatedPackaging, NUnit.Framework.Is.EqualTo(default(ICommodityRelatedPackaging)));
		}

		[ExpectNoExceptions]
		public virtual void TestGoodsGroupNameCode()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.JI_GoodsType = "333";
			NUnit.Framework.Assert.That(commodity.GoodsGroupNameCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.GoodsGroupNameCode should be");
		}

		[ExpectNoExceptions]
		public virtual void TestCommodity_HandlingInstructionsCodes()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var invLineRefs = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
			invLineRefs.JG_ReferenceNumber = "1";
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.HandlingInstructionsCodes, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ZString>)));
		}

		[ExpectNoExceptions]
		public virtual void TestTariffCodeExtensionCode()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			invoiceLine.JI_TariffExtensionCode = "1";
			NUnit.Framework.Assert.That(commodity.TariffCodeExtensionCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "Commodity.TariffCodeExtensionCode should be");
		}

		[ExpectNoExceptions]
		public virtual void TestCommodity_Quarantine()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var entryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = entryLine.PK;
			var commodity = GetCommodity(entryLine, entryLine.RandomLine);
			NUnit.Framework.Assert.That(commodity.Quarantine, NUnit.Framework.Is.EqualTo(default(IQuarantine)));
		}

		[ExpectNoExceptions]
		public virtual void TestCheckNotApplicableProperties()
		{
			var header = Factory.New<CusEntryHeader>();
			var line = header.MergedLines.AddNew();
			line.InvoiceLines.AddNew();
			var commodity = GetCommodity(line, line.RandomLine);
			NUnit.Framework.Assert.That(commodity.Food, NUnit.Framework.Is.EqualTo(default(IFood)));
			NUnit.Framework.Assert.That(commodity.CargoDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.BondedNoteCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.VehicleIDs, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ZString>)));
			NUnit.Framework.Assert.That(commodity.Classification, NUnit.Framework.Is.EqualTo(default(IClassification)));
		}

		protected abstract ICommodity GetCommodity(CusEntryLine entryLine, JobComInvoiceLine invoiceLine);
	}
}
