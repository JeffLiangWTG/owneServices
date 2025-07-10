using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(EntryCreationStrategy))]
	sealed class EntryCreationStrategyTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetKeyForHeader()
		{
			var actualKeys = strategy.GetKeyForHeader(invoiceLine).Keys;
			var expectedKeys = new IZType[] { new ZGuid(invoiceLine.JI_CEI), };
			NUnit.Framework.Assert.That(actualKeys.Skip(actualKeys.Count - expectedKeys.Length).ToArray(), NUnit.Framework.Is.EqualTo(expectedKeys));
		}

		[ExpectNoExceptions]
		public void TestGetKeyForLine()
		{
			invoiceLine.JI_Tariff = "87149990905";
			invoiceLine.JI_CountryOfOrigin = "TW";
			invoiceLine.JI_AlcoholPercentage = 1;
			invoiceLine.JI_CusValueConvRatio = 3;
			invoiceLine.JI_EnteredUnitPrice = 5;
			invoiceLine.JI_BrandName = "Brand";
			invoiceLine.JI_Model = "Model";
			invoiceLine.JI_TariffAdditionalCode = "TA";
			invoiceLine.JI_OA_ManufacturerAddress = new ZGuid("ADD04A5B-E395-4649-BE6D-9CF37BEB5F69");
			invoiceLine.JI_ExtraInfoForClassification = "1111";
			invoiceLine.JI_Group = "Grouping";
			invoiceLine.JI_PartNo = "PartNo";
			invoiceLine.JI_PrimaryPreference = "AAA";
			invoiceLine.JI_ConcessionOrder = "QUOTA";
			var invoiceLineTax1 = invoiceLine.Taxes.AddNew();
			invoiceLineTax1.JLT_Type = "B";
			invoiceLineTax1.JLT_Tariff = "1";
			var invoiceLineTax2 = invoiceLine.Taxes.AddNew();
			invoiceLineTax2.JLT_Type = "A";
			invoiceLineTax2.JLT_Tariff = "2";
			var invoiceLineTax3 = invoiceLine.Taxes.AddNew();
			invoiceLineTax3.JLT_Type = "A";
			invoiceLineTax3.JLT_Tariff = "1";
			var expectedHashCode = strategy.GetAdditionalTariffHashCode(invoiceLine);
			declaration.JE_MergeBy = MergeByCodeList.Codes.CondensedDeclaration;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			invoiceLine.JI_PrimaryPreference = "AAA";
			invoiceLine.JI_Procedure = "P";
			invoiceLine.JI_Description = ZString.Empty;
			invoiceLine.JI_ConcessionOrder = "QUOTA";
			invoiceLine.JI_NDescription = "NDescription";
			invoiceLine.JI_Description = "Description";
			invoiceLine.JI_DtyPymntMthd = "DEF";
			invoiceLine.JI_VatPymntMthd = "VAT";
			invoiceLine.JI_TpfPymntMthd = "TPF";
			var actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			AssertDefaultMergeKey(true, expectedHashCode, actualKeys);
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("NDescription")), NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("Description")), NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("NDescription\r\nDescription\r\nDescription")), NUnit.Framework.Is.EqualTo(false));
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			invoiceLine.JI_PrimaryPreference = "AAA";
			invoiceLine.JI_Procedure = "P";
			invoiceLine.JI_ConcessionOrder = "QUOTA";
			invoiceLine.JI_DtyPymntMthd = "DEF";
			invoiceLine.JI_VatPymntMthd = "VAT";
			invoiceLine.JI_TpfPymntMthd = "TPF";
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			AssertDefaultMergeKey(true, expectedHashCode, actualKeys);
			AssertOtherMergeKey(false, actualKeys);
			NUnit.Framework.Assert.That(actualKeys.Contains(invoiceLine.PK), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("PartNo")), NUnit.Framework.Is.EqualTo(false));
			invoiceLine.JI_TariffAdditionalCode = ZString.Empty;
			invoiceLine.JI_DtyPymntMthd = "DEF";
			invoiceLine.JI_VatPymntMthd = "VAT";
			invoiceLine.JI_TpfPymntMthd = "TPF";
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			AssertDefaultMergeKey(true, expectedHashCode, actualKeys);
			AssertOtherMergeKey(false, actualKeys, "");
			NUnit.Framework.Assert.That(actualKeys.Contains(invoiceLine.PK), NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("PartNo")), NUnit.Framework.Is.EqualTo(false));
			invoiceLine.JI_TariffAdditionalCode = "TA";
			invoiceLine.JI_DtyPymntMthd = "DEF";
			invoiceLine.JI_VatPymntMthd = "VAT";
			invoiceLine.JI_TpfPymntMthd = "TPF";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			AssertDefaultMergeKey(true, expectedHashCode, actualKeys);
			AssertOtherMergeKey(true, actualKeys);
			NUnit.Framework.Assert.That(actualKeys.Contains(invoiceLine.PK), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("NDescription\r\nDescription\r\nBRAND: Brand MODEL: Model\r\n")), NUnit.Framework.Is.EqualTo(true));
			invoiceLine.JI_DtyPymntMthd = "DEF";
			invoiceLine.JI_VatPymntMthd = "VAT";
			invoiceLine.JI_TpfPymntMthd = "TPF";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			AssertDefaultMergeKey(true, expectedHashCode, actualKeys);
			AssertOtherMergeKey(true, actualKeys);
			NUnit.Framework.Assert.That(actualKeys.Contains(invoiceLine.PK), NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("PartNo")), NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("NDescription\r\nDescription\r\nBRAND: Brand MODEL: Model\r\n")), NUnit.Framework.Is.EqualTo(true));
			invoiceLine.JI_DtyPymntMthd = "DEF";
			invoiceLine.JI_VatPymntMthd = "VAT";
			invoiceLine.JI_TpfPymntMthd = "TPF";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			AssertDefaultMergeKey(true, expectedHashCode, actualKeys);
			AssertOtherMergeKey(true, actualKeys);
			NUnit.Framework.Assert.That(actualKeys.Contains(invoiceLine.PK), NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("PartNo")), NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("NDescription\r\nDescription\r\nBRAND: Brand MODEL: Model\r\n")), NUnit.Framework.Is.EqualTo(true));
			var permitCusSupporting = invoiceLine.PermitCusSupportingCollection.AddNew();
			AssertInvoiceLinePK(false);
			permitCusSupporting.CSI_LineNo = 1;
			AssertInvoiceLinePK(true);
			permitCusSupporting.CSI_LineNo = 0;
			permitCusSupporting.CSI_ReferenceNumber = "1";
			AssertInvoiceLinePK(true);
			permitCusSupporting.CSI_LineNo = 1;
			permitCusSupporting.CSI_ReferenceNumber = "1";
			AssertInvoiceLinePK(true);
			invoiceLine.PermitCusSupportingCollection.RemoveAll();
			AssertInvoiceLinePK(false);
			var link = invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX");
			link.IsLinkedCMHeader = true;
			AssertInvoiceLinePK(true);
			link.IsLinkedCMHeader = false;
			AssertInvoiceLinePK(false);
			var assignedJobComInvLineRefs = invoiceLine.AssignedJobComInvLineRefsCollection.AddNew();
			AssertInvoiceLinePK(false);
			assignedJobComInvLineRefs.JG_ReferenceNumber = "1";
			AssertInvoiceLinePK(true);
			invoiceLine.AssignedJobComInvLineRefsCollection.RemoveAll();
			AssertInvoiceLinePK(false);
			invoiceLine.JI_CarType = "A";
			NUnit.Framework.Assert.That(!invoiceLine.IsCarRelatedDataEmpty, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			AssertInvoiceLinePK(true);
			invoiceLine.JI_CarType = ZString.Empty;
			AssertInvoiceLinePK(false);
			AssertInvoiceLinePK(true, invoiceLine.CitesPermitInfo, new ZString("A"), ZString.Empty);
			AssertInvoiceLinePK(true, invoiceLine.CertificateOfOriginNumberInfo, new ZString("A"), ZString.Empty);
			AssertInvoiceLinePK(true, invoiceLine.CertificateOfOriginNumberItemNumberInfo, new ZShort(1), ZShort.Zero);
			AssertInvoiceLinePK(true, invoiceLine.HighTechLicenseInfo, new ZString("A"), ZString.Empty);
			AssertInvoiceLinePK(true, invoiceLine.JI_PreviousEntryNumberInfo, new ZString("A"), ZString.Empty);
			AssertInvoiceLinePK(true, invoiceLine.JI_PreviousEntryLineNumberInfo, new ZShort(1), ZShort.Zero);
			AssertInvoiceLinePK(true, invoiceLine.JI_CustomsOwnerPartNoInfo, new ZString("A"), ZString.Empty);
			AssertInvoiceLinePK(true, invoiceLine.JI_CustomsSupplierPartNoInfo, new ZString("A"), ZString.Empty);
			AssertInvoiceLinePK(true, invoiceLine.JI_CompositionsInfo, new ZString("A"), ZString.Empty);
			AssertInvoiceLinePK(true, invoiceLine.PreviousPermitNoInfo, new ZString("A"), ZString.Empty);
			AssertInvoiceLinePK(true, invoiceLine.JI_BondedGoodsCodeInfo, new ZString("A"), ZString.Empty);
			AssertInvoiceLinePK(true, invoiceLine.JI_ProcedureInfo, new ZString(Constants.ProcedureCodes._37), ZString.Empty);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			AssertInvoiceLinePK(true, invoiceLine.JI_AntiDumpingDutyRateInfo, new ZDecimal(1), ZDecimal.Zero);
			AssertInvoiceLinePK(true, invoiceLine.JI_CountervailingDutyRateInfo, new ZDecimal(1), ZDecimal.Zero);
			AssertInvoiceLinePK(true, invoiceLine.JI_AdditionalDutyRateInfo, new ZDecimal(1), ZDecimal.Zero);
			AssertInvoiceLinePK(true, invoiceLine.JI_RetaliatoryDutyRateInfo, new ZDecimal(1), ZDecimal.Zero);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
			invoiceLine.JI_EPTDigit1 = ContainerMaterialList.Codes.A;
			invoiceLine.JI_EPTDigit2 = ContainerCapacityList.Codes._1;
			invoiceLine.JI_EPTDigit3 = ContainerMaterialNumberList.Codes._1;
			AssertInvoiceLinePK(true);
			invoiceLine.JI_EPTDigit1 = ZString.Empty;
			AssertInvoiceLinePK(false);
			invoiceLine.JI_EPTDigit2 = ZString.Empty;
			invoiceLine.JI_EPTDigit3 = ZString.Empty;
			invoiceLine.JI_AntiDumpingDutyRate = 1m;
			AssertInvoiceLinePK(false);
			invoiceLine.JI_AntiDumpingDutyRate = ZDecimal.Zero;
			invoiceLine.JI_CountervailingDutyRate = 1m;
			AssertInvoiceLinePK(false);
			invoiceLine.JI_CountervailingDutyRate = ZDecimal.Zero;
			invoiceLine.JI_AdditionalDutyRate = 1m;
			AssertInvoiceLinePK(false);
			invoiceLine.JI_AdditionalDutyRate = ZDecimal.Zero;
			invoiceLine.JI_RetaliatoryDutyRate = 1m;
			AssertInvoiceLinePK(false);
			invoiceLine.TrademarkStorageDocsGuid = ZGuid.NewZGuid();
			AssertInvoiceLinePK(true);
			invoiceLine.TrademarkStorageDocsGuid = ZGuid.Empty;
			AssertInvoiceLinePK(false);
		}

		[ExpectNoExceptions]
		void AssertInvoiceLinePK(bool expected)
		{
			var actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			NUnit.Framework.Assert.That(actualKeys.Contains(invoiceLine.PK), NUnit.Framework.Is.EqualTo(expected));
		}

		[ExpectNoExceptions]
		void AssertInvoiceLinePK(bool expected, ZPropertyInfo propertyInfo, IZType value, IZType emptyValue)
		{
			propertyInfo.Value = value;
			var actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			NUnit.Framework.Assert.That(actualKeys.Contains(invoiceLine.PK), NUnit.Framework.Is.EqualTo(expected));
			propertyInfo.Value = emptyValue;
			actualKeys = strategy.GetKeyForLine(invoiceLine).Keys;
			NUnit.Framework.Assert.That(actualKeys.Contains(invoiceLine.PK), NUnit.Framework.Is.EqualTo(!expected));
		}

		[ExpectNoExceptions]
		void AssertDefaultMergeKey(bool expected, ZInt expectedHashCode, System.Collections.Generic.List<IZType> actualKeys)
		{
			if (expectedHashCode >= 0)
			{
				NUnit.Framework.Assert.That(actualKeys.Contains(expectedHashCode), NUnit.Framework.Is.EqualTo(expected));
			}

			var expectedWhenImport = invoiceLine.IsImport && expected;
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("TW")), NUnit.Framework.Is.EqualTo(expectedWhenImport));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("87149990905")), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("P")), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZDecimal(1)), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("AAA")), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZDecimal(3)), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("QUOTA")), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(!actualKeys.Contains(new ZString("Grouping")), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("DEF")), NUnit.Framework.Is.EqualTo(expectedWhenImport));
		}

		[ExpectNoExceptions]
		void AssertOtherMergeKey(bool expected, System.Collections.Generic.List<IZType> actualKeys, string tariffAdditionalCodeDutyReduction = "TA")
		{
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZDecimal(5)), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("Brand")), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString("Model")), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZString(tariffAdditionalCodeDutyReduction)), NUnit.Framework.Is.EqualTo(expected));
			NUnit.Framework.Assert.That(actualKeys.Contains(new ZGuid("ADD04A5B-E395-4649-BE6D-9CF37BEB5F69")), NUnit.Framework.Is.EqualTo(expected));
		}

		#region Implementation
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		EntryCreationStrategy strategy;
		CusEntryInstruction entryInstruction;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			strategy = new EntryCreationStrategy(declaration);
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			entryInstruction = declaration.CusEntryInstruction;
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			entryInstruction.CEI_ExamMode = ExamModeList.Codes.WrittenReview;
			for (var i = 0; i < 50; i++)
			{
				declaration.InvoiceLines.AddNew().JI_CEI = entryInstruction.PK;
			}

			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "XX";
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeHSN = universalTestHelper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var tariffTypeSS = universalTestHelper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
			Factory.Save();
			universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "90031", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var childTariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeSS.PK, "Forniture", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(childTariff.PK, tariffTypeHSN.PK, "90031");
			Factory.Save();
		}
		#endregion
	}
}
