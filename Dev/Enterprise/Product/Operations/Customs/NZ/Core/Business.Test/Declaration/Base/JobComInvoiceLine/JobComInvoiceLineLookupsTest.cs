using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System;
	using Enterprise.Customs.NZ.Business.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	public class JobComInvoiceLineLookupsTest : TestCaseWithFactory
	{
		public void TestAccessUnitListWhenDeclarationNull()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertNull(invoiceLine.Declaration);
			AssertNoExceptionThrown(() => invoiceLine.Lookups.InvoiceUQList.GetType());
		}

		public void TestGoodsTypes()
		{
			AssertNotNull(invoiceLine.Lookups.GoodsTypes);
		}

		public void TestMeasurementUQs()
		{
			AssertNotNull(invoiceLine.Lookups.MeasurementUQs);
		}

		public void TestPartsOfClassificationList()
		{
			NonDependentNZCClassificationCollection partsOfClassifications = invoiceLine.Lookups.PartsOfClassificationList;
			partsOfClassifications.Load();
			int numberOfRecordsToCheck = partsOfClassifications.Count;
			numberOfRecordsToCheck = numberOfRecordsToCheck > 10 ? 10 : numberOfRecordsToCheck;
			Assert("Precondition: NumberOfRecordsToCheck > 0", numberOfRecordsToCheck > 0);
			for (int i = 0; i < numberOfRecordsToCheck; i++)
			{
				AssertEquals("PartsOfClassifications[i].U0_IsManual", true, partsOfClassifications[i].U0_IsManual);
			}
		}

		public void TestInvoiceUQList()
		{
			AssertEquals("Base invoice UQ list", typeof(CodeDescriptionPairList), invoiceLine.Lookups.InvoiceUQList.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("For TSW formal declarations, the UQ has moved to a packaging grid, this field remains enterable for standard enterprise invoice details.", typeof(CodeDescriptionPairList), invoiceLine.Lookups.InvoiceUQList.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("For TSW writeoff (CRE) declarations, the UQ needs to come from the TSW required list.", typeof(CodeDescriptionPairList), invoiceLine.Lookups.InvoiceUQList.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			AssertEquals("For TSW Import writeoff (ICR) declarations, the UQ needs to come from the TSW required list.", typeof(CodeDescriptionPairList), invoiceLine.Lookups.InvoiceUQList.GetType());
		}

		public void TestPackageUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BBB", "BBB DESC", new ZDateTime(2012, 3, 3), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

			Factory.Save();

			Assert("BG", invoiceLine.Lookups.PackageUQList.ContainsCode("BG"));
			Assert("BAG should not appear as it is not in the list", !invoiceLine.Lookups.PackageUQList.ContainsCode("BAG"));
			Assert("BBB should not appear as it is too new", !invoiceLine.Lookups.PackageUQList.ContainsCode("BBB"));
			Assert("AAA", invoiceLine.Lookups.PackageUQList.ContainsCode("AAA"));
		}

		public void TestClassificationList()
		{
			var collection = invoiceLine.Lookups.ClassificationList as Customs.Business.BaseClassificationCollection<CusClassification>;
			collection.Load();

			int currentCount = collection.Count;

			CusClassification classification1 = Factory.New<CusClassification>();
			classification1.CC_TariffNum = "0000.00.00";
			classification1.CC_LookupCode = "Plain";

			CusClassification classification2 = Factory.New<CusClassification>();
			classification2.CC_TariffNum = "1111.11.11";
			classification2.CC_LookupCode = "Woopeeee";

			CusClassification classification3 = Factory.New<CusClassification>();
			classification3.CC_TariffNum = "1111.11.12";
			classification3.CC_LookupCode = "Yeeha";
			classification3.CC_RN_NKCountryCode = ZString.Empty;

			var collection2 = invoiceLine.Lookups.ClassificationList as Customs.Business.BaseClassificationCollection<CusClassification>;
			collection2.Load();

			AssertEquals("Now considering existing classification, 2 of the test data are valid", 2 + currentCount, collection2.Count);
		}

		public void TestTariffList()
		{
			AssertEquals(typeof(NonDependentNZCClassificationCollection), invoiceLine.Lookups.TariffList.GetType());
		}

		public void TestConcessionList()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				TestCaseHelper.ClearTable(NZCConcessionClassificationLink.Schema.TableName);

				NZCClassification tariff = NZCClassification.New(Factory);
				tariff.U0_Tariff = "0000.00.00.00Z";
				tariff.U0_DateActiveFrom = new ZDateTime(2001, 1, 1);

				NZCConcession concession1 = NZCConcession.New(Factory);
				concession1.U2_Code = "111111A";
				concession1.U2_DateActiveFrom = new ZDateTime(2001, 1, 1);
				NZCConcessionClassificationLink concessionTariffLink1 = concession1.TariffsApplicable.AddNew();
				concessionTariffLink1.U3_TariffPortion = tariff.U0_Tariff.Left(10);

				NZCConcession concession2 = NZCConcession.New(Factory);
				concession2.U2_Code = "222222B";
				concession2.U2_DateActiveFrom = new ZDateTime(2001, 1, 1);
				NZCConcessionClassificationLink concessionTariffLink2 = concession2.TariffsApplicable.AddNew();
				concessionTariffLink2.U3_TariffPortion = tariff.U0_Tariff.Left(7);

				NZCConcession concession3 = NZCConcession.New(Factory);
				concession3.U2_Code = "333333C";
				concession3.U2_DateActiveFrom = new ZDateTime(2001, 1, 1);
				NZCConcessionClassificationLink concessionTariffLink3 = concession3.TariffsApplicable.AddNew();
				concessionTariffLink3.U3_TariffPortion = "1111";

				invoiceLine.JI_Tariff = "0000.00.00.00Z";

				Factory.Save();

				var concessions = invoiceLine.Lookups.ConcessionList as NonDependentNZCConcessionCollection;
				concessions.Load();
				AssertEquals("InvoiceLine.Lookups.ConcessionList.Count", 2, concessions.Count);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);
				invoiceLine.JI_Tariff = "123456789";

				AssertEquals("InvoiceLine.Lookups.ConcessionList.Count", 2, invoiceLine.Lookups.ConcessionList.Count);
				invoiceLine.JI_CountryOfOrigin = "AU";
				AssertEquals("InvoiceLine.Lookups.ConcessionList.Count", 1, invoiceLine.Lookups.ConcessionList.Count);
				invoiceLine.JI_QualifiesForPreferentialDuty = "N";
				AssertEquals("InvoiceLine.Lookups.ConcessionList.Count", 0, invoiceLine.Lookups.ConcessionList.Count);
			}
		}

		public void TestPreferentialCountryGroupCodeList()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tradeGroup1 = helper.LoadOrCreateTradeGroup("NZ", "AU", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				var tradeGroup2 = helper.LoadOrCreateTradeGroup("NZ", "TTP", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
				helper.AddCountry(tradeGroup1, "AU");
				helper.AddCountry(tradeGroup2, "AU");

				invoiceLine.JI_CountryOfOrigin = "AU";
				AssertEquals(2, invoiceLine.Lookups.PreferentialCountryGroupCodeList.Count);
				invoiceLine.JI_QualifiesForPreferentialDuty = "N";
				AssertEquals(0, invoiceLine.Lookups.PreferentialCountryGroupCodeList.Count);
			}
		}

		public void TestSupplementaryUQList()
		{
			AssertEquals(typeof(SupplementaryUQList), invoiceLine.Lookups.SupplementaryUQList.GetType());
		}

		public void TestQualifiesForPreferentialDutyList()
		{
			AssertEquals(typeof(QualifiesForPreferentialDutyList), invoiceLine.Lookups.QualifiesForPreferentialDutyList.GetType());
		}

		public void TestPartsCollection()
		{
			CusClassification lookUp = Factory.New<CusClassification>();
			lookUp.CC_TariffNum = "1234.56.78";
			lookUp.CC_LookupCode = "LOOKUP";
			lookUp.CC_ClassificationType = CusClassification.ClassificationType.Both;

			invoiceLine.Declaration.JE_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true)).PK;
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			invoiceLine.JI_PartNo = "PARTNO";
			invoiceLine.JI_CC = lookUp.PK;
			invoiceLine.JI_Description = "DESC";
			invoiceLine.JI_InvoiceUQ = "EA";
			MasterFiles.OrgSupplierPart newPart = (MasterFiles.OrgSupplierPart)invoiceLine.Lookups.PartsList.AddNew();
			AssertEquals("New Part Defaults", "DESC", newPart.OP_Desc);
			AssertEquals("New Part Defaults", "EA", newPart.OP_StockKeepingUnit);
			AssertEquals(1, newPart.PivotsForBinding.Count);
			var newPartPivot = newPart.PivotsForBinding[0];
			AssertEquals(lookUp.PK, newPartPivot.CI_CC);
			AssertEquals(Customs.Business.ClassificationTypeList.Codes.HTI, newPartPivot.CI_ChildType);
			int orgindex = newPart.RelatedOrganisations[0].OU_Relationship == OrgPartRelation.RelationshipTypes.Owner ? 0 : 1;
			AssertEquals("Owner", invoiceLine.Declaration.JE_OH_Importer, newPart.RelatedOrganisations[orgindex].OU_OH);
		}

		public void TestIntendedUseCodeList()
		{
			AssertNotNull("Lookups.IntendedUseCodeList", invoiceLine.Lookups.IntendedUseCodeList);
			AssertEquals(typeof(IntendedUseCodeList), invoiceLine.Lookups.IntendedUseCodeList.GetType());
			AssertEquals("HC – Sale for human consumption", true, invoiceLine.Lookups.IntendedUseCodeList.ContainsCode("HC"));
			AssertEquals("PU - Personal use", true, invoiceLine.Lookups.IntendedUseCodeList.ContainsCode("PU"));
			AssertEquals("FP - Further processing for human consumption", true, invoiceLine.Lookups.IntendedUseCodeList.ContainsCode("FP"));
			AssertEquals("FN - Further processing NOT for human consumption", true, invoiceLine.Lookups.IntendedUseCodeList.ContainsCode("FN"));
			AssertEquals("OT - Other NOT for human consumption", true, invoiceLine.Lookups.IntendedUseCodeList.ContainsCode("OT"));
		}

		#region Implememtation
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		OrgHeader supplier;
		OrgHeader anotherSupplier;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			ZQuery supplierFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
			supplier = Factory.LoadTop1<OrgHeader>(supplierFilter);

			ZQuery anotherSupplierFilter = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, supplier.PK);
			anotherSupplierFilter.AddToFilter(supplierFilter, JoinCondition.And);
			anotherSupplier = Factory.LoadTop1<OrgHeader>(anotherSupplierFilter);

			ZQuery importerFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
			importerFilter.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, supplier.PK);
			importerFilter.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, anotherSupplier.PK);
		}
		#endregion
	}
}
