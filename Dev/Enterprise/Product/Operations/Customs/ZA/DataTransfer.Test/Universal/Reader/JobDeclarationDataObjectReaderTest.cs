using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class JobDeclarationDataObjectReaderTest : Customs.DataTransfer.Universal.Testing.DataObjectReaderTest
	{
		public void TestCusEntryHeaderImportedForITF()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declarationDataObject = SetupDeclaration(new CodeDescriptionPair() { Code = ZAJobMessageTypeList.Codes.Import, Description = ZAJobMessageTypeList.Descriptions.Import.ToString() },
				new CodeDescriptionPair() { Code = "ST1", Description = "STANDARD" },
				new ContainerMode() { Code = Core.Constants.ContainerModes.Containerised, Description = "Containerized" },
				"RAT HATS",
				new UNLOCO() { Code = "NZDUD", Name = "Dunedin" },
				new UNLOCO() { Code = "NZCHC", Name = "Christchurch" },
				null,
				new UNLOCO() { Code = "AUSYD", Name = "Sydney" },
				new UNLOCO() { Code = "AUBDG", Name = "Bendigo" },
				"BUNGA DELIMA",
				"343L",
				0,
				45,
				new PackageType() { Code = "KEG", Description = "Keg" },
				23.45m,
				new UnitOfVolume() { Code = "CF", Description = "Cubic Feet" },
				34.56m,
				new UnitOfWeight() { Code = "KT", Description = "Kilotons" },
				new CodeDescriptionPair() { Code = "SEA", Description = "Sea Freight" },
				null,
				"MYMASTER",
				new WayBillType() { Code = "MWB", Description = "Master Waybill" },
				null,
				new ServiceLevel() { Code = "STD", Description = "Standard" },
				ZBool.True, null, new CodeDescriptionPair() { Code = "TRF", Description = "Tariff" }, 112, 306, "LLD234", null, "AGREF123", "F234",
				new CodeDescriptionPair() { Code = "DEF", Description = "DEFAULT" },
				new CodeDescriptionPair() { Code = MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, Description = MasterFiles.Business.Customs.PaidByCodeList.Descriptions.BRK },
				new UniversalDataBuss.DataObjects.Universal.IncoTerm() { Code = "FOB", Description = "FREE ON BOARD" }, 789.012m);

				var entryHeaderDataObject1 = SetupEntryHeader("EX$", "MS2", "ES3", "BG32423", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5));
				entryHeaderDataObject1.EntryLineCollection = new List<UniversalCustoms.EntryLine>(new[] { SetupEntryLine(1, "1020304050", 2585.87m, 0.84m, 800.96m, new CodeDescriptionPair() { Code = "NO", Description = "NO DESC" }, "DESC 1", null) });
				entryHeaderDataObject1.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber() });

				var entryHeaderDataObject2 = SetupEntryHeader("IM$", "MS1", "ES1", "BG89756", 869.54m, new ZDateTime(2012, 4, 4), new ZDateTime(2012, 4, 5));
				entryHeaderDataObject2.CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[]
				{
				SetupCustomsReference(new CodeDescriptionPair() { Code = "ABC" }, null, "HELLO", null, null),
				SetupCustomsReference(new CodeDescriptionPair() { Code = "DEF" }, null, "BYE", null, null)
			});
				entryHeaderDataObject2.AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>(new[]
				{
				SetupAddInfoGroup(new CodeDescriptionPair() { Code = "ABC" }),
				SetupAddInfoGroup(new CodeDescriptionPair() { Code = "DEF" })
			});
				entryHeaderDataObject2.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber2(), SetupEntryNumber() });
				var entryLineDataObject = SetupEntryLine(2, "2010304040", 1502.53m, 0.23m, 1200.40m, new CodeDescriptionPair() { Code = "KG", Description = "KG DESC" }, "DESC 2", null);
				entryHeaderDataObject2.EntryLineCollection = new List<UniversalCustoms.EntryLine>(new[]
				{
				entryLineDataObject,
				SetupEntryLine(1, "1020305060", 2585.87m, 0.84m, 800.96m, new CodeDescriptionPair() { Code = "NO", Description = "NO DESC" }, "DESC 1", null)
			});
				entryHeaderDataObject2.EntryHeaderChargeCollection = new List<UniversalCustoms.EntryHeaderCharge>(new[] { SetupEntryHeaderCharge2(), SetupEntryHeaderCharge() });
				declarationDataObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>(new[] { entryHeaderDataObject1, entryHeaderDataObject2 }));

				entryLineDataObject.CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[]
				{
				SetupCustomsReference(new CodeDescriptionPair() { Code = "ABC" }, null, "HELLO", null, null),
				SetupCustomsReference(new CodeDescriptionPair() { Code = "DEF" }, null, "BYE", null, null)
			});

				entryLineDataObject.EntryLineChargeCollection = new List<UniversalCustoms.EntryLineCharge>(new[] { SetupEntryLineCharge2(), SetupEntryLineCharge() });

				declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group",
					null,
					new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
					SetupCommercialInvoiceHeaderData("INV1", null, null, 10000m, new Currency() { Code = Core.Constants.CurrencyCodes.UnitedStates }, new ZDateTime(2012, 4, 3), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.FreeOnBoard }, 10m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 1000m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, ZDecimal.Zero, ZDecimal.Zero, 100m, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, null, null, null,
						new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] {
							SetupCommercialInvoiceLine(1, "GOODS 1", 1.1m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Bag }, 1500m, ZString.Empty, 1.5m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 150m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
								new List<UniversalCustoms.EntryReference>(new [] {
									SetupEntryReference(1, new EntryType() { Code = "EX$" }, "BG32423"),
									SetupEntryReference(2, new EntryType() { Code = "IM$" }, "BG89756")
								})),
							SetupCommercialInvoiceLine(2, "GOODS 2", 2.2m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Bundle }, 3500m, ZString.Empty, 3.5m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 350m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
								new List<UniversalCustoms.EntryReference>(new [] {
									SetupEntryReference(2, new EntryType() { Code = "IM$" }, "BG89756")
								})),
							SetupCommercialInvoiceLine(3, "GOODS 3", 3.3m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Dozen }, 2000m, ZString.Empty, 2m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 200m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
								new List<UniversalCustoms.EntryReference>(new [] {
									SetupEntryReference(2, new EntryType() { Code = "EX$" }, "BG32423"), // should be ignore by the importation as line no 2 doesn't exist
									SetupEntryReference(1, new EntryType() { Code = "IM$" }, "BG89756")
								})),
							SetupCommercialInvoiceLine(4, "GOODS 4", 4.4m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Envelope }, 3000m, ZString.Empty, 3m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 300m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
								new List<UniversalCustoms.EntryReference>(new [] {
									SetupEntryReference(1, new EntryType() { Code = "IM$" }, "BG89756"),
									SetupEntryReference(1, new EntryType() { Code = "EX$" }, "BG32423")
								}))
						}))
					})
				);

				entryHeaderDataObject1.RelatedEntryHeaderCollection = new List<UniversalCustoms.EntryHeader>(new[]
					{
					SetupEntryHeader("GM#", "MS3", "ES3", "BGKG323", 1500m, new ZDateTime(2012, 5, 4), new ZDateTime(2012, 5, 5))
				});

				var declarationBOToLoad = Factory.New<JobDeclaration>();
				declarationBOToLoad.JE_MasterBill = "MYMASTER";
				declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
				var entryHeaderToUpdate = declarationBOToLoad.CustomsEntryHeaders.AddNew();
				entryHeaderToUpdate.CH_BGMReference = "BG89756";
				entryHeaderToUpdate.CH_MessageType = "IM$";
				var entryHeaderToDelete = declarationBOToLoad.CustomsEntryHeaders.AddNew();
				entryHeaderToDelete.CH_BGMReference = "BG32423";
				entryHeaderToDelete.CH_MessageType = "IM$";

				var entryHeaderChargeBOThatShouldBeMatched = entryHeaderToUpdate.Charges.AddNew("WCH", 150m);
				var entryHeaderChargeBOThatShouldBeDeleted = entryHeaderToUpdate.Charges.AddNew("GCH", 850m);

				var entryNumberBOThatShouldBeMatched = CreateCusEntryNumber(entryHeaderToUpdate.TableName, entryHeaderToUpdate.PK, Core.Constants.CountryCodes.SouthAfrica, "W89", "W986548", ZBool.True);
				var entryNumberBOThatShouldBeDeleted = CreateCusEntryNumber(entryHeaderToUpdate.TableName, entryHeaderToUpdate.PK, Core.Constants.CountryCodes.SouthAfrica, "!34", "ZBD234", ZBool.True);

				Factory.SaveForTesting();

				var entryLineBOThatShouldBeDeleted = entryHeaderToUpdate.AllEntryLines.AddNew();
				entryLineBOThatShouldBeDeleted.CL_LineNumber = 1;

				var masterBillDataObject = SetupAdditionalBill("MYMASTER", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, SetupAddInfos("ABC=SD"), 10m, Core.Constants.PkgUnit.Box, "BOX");
				declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject }));
				var declarationBO = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory).ReadIntoBusinessObject();
				Factory.SaveAtEndOfImport(logger);

				#region Check Contents of Business Object

				CombineAssertions(delegate
				{
					AssertEquals("Should have matched", declarationBO, declarationBOToLoad);
					AssertContents(declarationBO,
					ZAJobMessageTypeList.Codes.Import,
					Core.Constants.ContainerModes.Containerised,
					"RAT HATS", "NZDUD", "NZCHC", ZString.Empty, "AUSYD", "AUBDG", "BUNGA DELIMA",
					"343L", 0, 45, "KEG", 23.45m, "CF", 34.56m, "KT", Core.Constants.TransportModes.Sea,
					ZString.Empty, "MYMASTER", ZString.Empty, ZString.Empty,
					"STD", ZBool.True, ZString.Empty, "TRF", 112, 306,
					"LLD234", ZString.Empty, "AGREF123", "F234", "DEF", MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, "FOB", 789.012m);
					AssertEquals(MasterFiles.Business.GlbCompany.CurrentCompany.GC_OH_OrgProxy, declarationBO.JE_OH_AgentOverride);

					declarationBO.CustomsEntryHeaders.Load();
					AssertEquals("declarationBO.CustomsEntryHeaders.Count", 4, declarationBO.CustomsEntryHeaders.Count);
					var entryHeaderBO1 = declarationBO.CustomsEntryHeaders.OfType<CusEntryHeader>().FirstOrDefault(x => x.CH_BGMReference == "BG32423" && x.CH_MessageType == "EX$");
					var entryHeaderBO2 = declarationBO.CustomsEntryHeaders.OfType<CusEntryHeader>().FirstOrDefault(x => x.CH_BGMReference == "BG89756");
					var entryHeaderBO3 = declarationBO.CustomsEntryHeaders.OfType<CusEntryHeader>().FirstOrDefault(x => x.CH_BGMReference == "BGKG323");
					AssertCusEntryHeaderContents(entryHeaderBO1, declarationBO.PK, "EX$", "MS2", "ES3", "BG32423", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), ZString.Empty, ZGuid.Empty);
					entryHeaderBO1.AllEntryLines.Load();
					AssertEquals("entryHeaderBO1.AllEntryLines.Count", 1, entryHeaderBO1.AllEntryLines.Count);
					var entryHeaderBO1Line = entryHeaderBO1.AllEntryLines[0];
					AssertCusEntryLineContents(entryHeaderBO1Line, entryHeaderBO1.PK, 1, "1020304050", 2585.87m, 0.84m, 800.96m, "NO", "DESC 1", ZString.Empty);
					entryHeaderBO1.Charges.Load();
					AssertEquals("entryHeaderBO1.Charges.Count", 0, entryHeaderBO1.Charges.Count);
					var childCusEntryNumBOs = LoadCusEntryNum(entryHeaderBO1.TableName, entryHeaderBO1.PK);
					AssertEquals("Child CusEntryNum", 1, childCusEntryNumBOs.Length);
					var childCusCodeDataBOs = LoadCusCodeData(entryHeaderBO1.TablePrefix, entryHeaderBO1.PK);
					AssertEquals("Child CusCusCodeData", 0, childCusCodeDataBOs.Length);
					var childCusAddInfoBOs = LoadCusAddInfo(entryHeaderBO1.TablePrefix, entryHeaderBO1.PK);
					AssertEquals("Child CusAddInfo", 0, childCusAddInfoBOs.Length);

					AssertEquals("Should have matched", entryHeaderToUpdate, entryHeaderBO2);
					AssertCusEntryHeaderContents(entryHeaderBO2, declarationBO.PK, "IM$", "MS1", "ES1", "BG89756", 869.54m, new ZDateTime(2012, 4, 4), new ZDateTime(2012, 4, 5), ZString.Empty, ZGuid.Empty);

					entryHeaderBO2.AllEntryLines.Load();
					AssertEquals("entryHeaderBO2.AllEntryLines.Count", 2, entryHeaderBO2.AllEntryLines.Count);
					AssertEquals("Should have been deleted", true, entryLineBOThatShouldBeDeleted.IsDeleted);
					var entryHeaderBO2Line1 = entryHeaderBO2.AllEntryLines.OfType<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff == "1020305060");
					var entryHeaderBO2Line2 = entryHeaderBO2.AllEntryLines.OfType<CusEntryLine>().FirstOrDefault(x => x.CL_AdValoremTariff != "1020305060");
					AssertCusEntryLineContents(entryHeaderBO2Line1, entryHeaderBO2.PK, 1, "1020305060", 2585.87m, 0.84m, 800.96m, "NO", "DESC 1", ZString.Empty);
					AssertCusEntryLineContents(entryHeaderBO2Line2, entryHeaderBO2.PK, 2, "2010304040", 1502.53m, 0.23m, 1200.40m, "KG", "DESC 2", ZString.Empty);

					entryHeaderBO2.Charges.Load();
					AssertEquals("entryHeaderBO2.Charges.Count", 2, entryHeaderBO2.Charges.Count);
					var entryHeaderChargeBO1 = entryHeaderBO2.Charges.OfType<Customs.Business.CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == "WCH");
					var entryHeaderChargeBO2 = entryHeaderBO2.Charges.OfType<Customs.Business.CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType != "WCH");
					AssertEquals("Should have been deleted", true, entryHeaderChargeBOThatShouldBeDeleted.IsDeleted);
					AssertEquals("Should have not been deleted", false, entryHeaderChargeBOThatShouldBeMatched.IsDeleted);
					AssertEquals("Should have matched", entryHeaderChargeBO1, entryHeaderChargeBOThatShouldBeMatched);
					AssertCusEntryHeaderChargeContents2(entryHeaderChargeBO1, entryHeaderBO2.PK);
					AssertCusEntryHeaderChargeContents(entryHeaderChargeBO2, entryHeaderBO2.PK);

					childCusEntryNumBOs = LoadCusEntryNum(entryHeaderBO2.TableName, entryHeaderBO2.PK);
					AssertEquals("Child CusEntryNum", 2, childCusEntryNumBOs.Length);
					AssertEquals("Should have been deleted", true, entryNumberBOThatShouldBeDeleted.IsDeleted);
					var entryNumberBO1 = childCusEntryNumBOs.FirstOrDefault(x => x.CE_EntryType == "W89" && x.CE_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica);
					var entryNumberBO2 = childCusEntryNumBOs.FirstOrDefault(x => x.CE_EntryType != "W89" && x.CE_RN_NKCountryCode == Core.Constants.CountryCodes.SouthAfrica);
					AssertEquals(entryNumberBO1, entryNumberBOThatShouldBeMatched);
					AssertCusEntryNumberContents2(entryNumberBO1, entryHeaderBO2.TableName, entryHeaderBO2.PK, CurrentCompanyHelper.TargetCountryCode);
					AssertCusEntryNumberContents(entryNumberBO2, entryHeaderBO2.TableName, entryHeaderBO2.PK, CurrentCompanyHelper.TargetCountryCode);

					childCusCodeDataBOs = LoadCusCodeData(entryHeaderBO2.TablePrefix, entryHeaderBO2.PK);
					AssertEquals("Child CusCusCodeData is not supported against entryHeader", 0, childCusCodeDataBOs.Length);

					childCusAddInfoBOs = LoadCusAddInfo(entryHeaderBO2.TablePrefix, entryHeaderBO2.PK);
					AssertEquals("Child CusAddInfo is not supported against entryHeader", 0, childCusAddInfoBOs.Length);

					// Check Entry Line Details
					entryHeaderBO2Line2.Fees.Load();
					AssertEquals(2, entryHeaderBO2Line2.Fees.Count);
					var entryLineFeeBO1 = entryHeaderBO2Line2.Fees.OfType<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "WCL");
					var entryLineFeeBO2 = entryHeaderBO2Line2.Fees.OfType<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType != "WCL");
					AssertCusEntryLineFeeContents2(entryLineFeeBO1, entryHeaderBO2Line2.PK);
					AssertCusEntryLineFeeContents(entryLineFeeBO2, entryHeaderBO2Line2.PK);

					childCusCodeDataBOs = LoadCusCodeData(entryHeaderBO2Line2.TablePrefix, entryHeaderBO2Line2.PK);
					AssertEquals("Child CusCusCodeData is not supported against entryLine", 0, childCusCodeDataBOs.Length);

					AssertEquals("declarationBO.Invoices.Count", 1, declarationBO.Invoices.Count);
					var invoiceBO = declarationBO.Invoices[0];
					AssertContents(invoiceBO, "INV1", ZGuid.Empty, ZGuid.Empty, 10000m, Core.Constants.CurrencyCodes.UnitedStates, new ZDateTime(2012, 4, 3), Core.Constants.IncoTerms.FreeOnBoard, 10m, Core.Constants.Volume.CubicMetres, 1000m, Core.Constants.Weight.Kilograms, 11.11m, Core.Constants.Weight.Kilograms, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, 100m, ZString.Empty);

					AssertEquals("invoiceBO.JobComInvoiceLines.Count", 4, invoiceBO.JobComInvoiceLines.Count);
					var invoiceLineBO1 = invoiceBO.JobComInvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "GOODS 1");
					AssertContents(invoiceLineBO1, 1, "GOODS 1", 1.1m, Core.Constants.PkgUnit.Bag, 1500m, ZString.Empty, 1.5m, Core.Constants.Volume.CubicMetres, 150m, Core.Constants.Weight.Kilograms, "NewUsed=N*TakeUpInTradeStatistics=Y");
					AssertEquals("invoiceLineBO1.JI_CL", ZGuid.Empty, invoiceLineBO1.JI_CL);
					AssertEquals("invoiceLineBO1.AdditionalEntryLineLinks.Count", 0, invoiceLineBO1.AdditionalEntryLineLinks.Count);

					var invoiceLineBO2 = invoiceBO.JobComInvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "GOODS 2");
					AssertContents(invoiceLineBO2, 2, "GOODS 2", 2.2m, Core.Constants.PkgUnit.Bundle, 3500m, ZString.Empty, 3.5m, Core.Constants.Volume.CubicMetres, 350m, Core.Constants.Weight.Kilograms, "NewUsed=N*TakeUpInTradeStatistics=Y");
					AssertEquals("invoiceLineBO2.JI_CL", ZGuid.Empty, invoiceLineBO2.JI_CL);
					AssertEquals("invoiceLineBO2.AdditionalEntryLineLinks.Count", 0, invoiceLineBO2.AdditionalEntryLineLinks.Count);

					var invoiceLineBO3 = invoiceBO.JobComInvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "GOODS 3");
					AssertContents(invoiceLineBO3, 3, "GOODS 3", 3.3m, Core.Constants.PkgUnit.Dozen, 2000m, ZString.Empty, 2m, Core.Constants.Volume.CubicMetres, 200m, Core.Constants.Weight.Kilograms, "NewUsed=N*TakeUpInTradeStatistics=Y");
					AssertEquals("invoiceLineBO3.JI_CL", ZGuid.Empty, invoiceLineBO3.JI_CL);
					AssertEquals("invoiceLineBO3.AdditionalEntryLineLinks.Count", 0, invoiceLineBO3.AdditionalEntryLineLinks.Count);

					var invoiceLineBO4 = invoiceBO.JobComInvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault(x => x.JI_Description == "GOODS 4");
					AssertContents(invoiceLineBO4, 4, "GOODS 4", 4.4m, Core.Constants.PkgUnit.Envelope, 3000m, ZString.Empty, 3m, Core.Constants.Volume.CubicMetres, 300m, Core.Constants.Weight.Kilograms, "NewUsed=N*TakeUpInTradeStatistics=Y");
					AssertEquals("invoiceLineBO4.JI_CL", ZGuid.Empty, invoiceLineBO4.JI_CL);
					AssertEquals("invoiceLineBO4.AdditionalEntryLineLinks.Count", 0, invoiceLineBO4.AdditionalEntryLineLinks.Count);

					AssertCusEntryHeaderContents(entryHeaderBO3, declarationBO.PK, "GM#", "MS3", "ES3", "BGKG323", 1500m, new ZDateTime(2012, 5, 4), new ZDateTime(2012, 5, 5), ZString.Empty, entryHeaderBO1.PK);

					AssertMultilineASCIIEquals("logger.Logs", @"Information - Successfully loaded matching JobDeclaration.
Information - Populating JobDeclaration...
Information - Successfully loaded matching Bill.
Information - Populating Bill...
Information - No matching CusEntryHeader found, creating new CusEntryHeader.
Information - Populating CusEntryHeader...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryLine found, creating new CusEntryLine.
Information - Populating CusEntryLine...
Information - No matching CusEntryHeader found, creating new CusEntryHeader.
Information - Populating CusEntryHeader...
Information - Successfully loaded matching CusEntryHeader.
Information - Populating CusEntryHeader...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryHeaderCharges.
Information - Populating CusEntryHeaderCharges...
Information - No matching CusEntryHeaderCharges found, creating new CusEntryHeaderCharges.
Information - Populating CusEntryHeaderCharges...
Warning - Customs Reference was not processed as there is no support for Table with code 'CH'.
Information - No matching CusEntryLine found, creating new CusEntryLine.
Information - Populating CusEntryLine...
Information - No matching CusEntryLineFee found, creating new CusEntryLineFee.
Information - Populating CusEntryLineFee...
Information - No matching CusEntryLineFee found, creating new CusEntryLineFee.
Information - Populating CusEntryLineFee...
Information - No matching CusEntryLine found, creating new CusEntryLine.
Information - Populating CusEntryLine...
Information - Updated Declaration B00001000 from UniversalShipment.
Information - Successfully saved Declaration B00001000 with 1 x Bill, 3 x CusEntryNumber, 3 x CusEntryLine, 3 x CusEntryHeader, 2 x CusEntryHeaderCharges, 2 x CusEntryLineFee.".Trim(), logger.Logs);
				});

				if (ErrorReporter.LastKeyReported == string.Format("{0} does not support B7_Type 'WEN'", typeof(CusEntryHeader).FullName))
				{
					ErrorReporter.Clear();
				}

				#endregion
			}
		}

		public void TestCusEntryHeaderNotImportedForBLT()
		{
			var declarationDataObject = SetupDeclaration(new CodeDescriptionPair() { Code = ZAJobMessageTypeList.Codes.Import, Description = ZAJobMessageTypeList.Descriptions.Import.ToString() },
				new CodeDescriptionPair() { Code = "ST1", Description = "STANDARD" },
				new ContainerMode() { Code = Core.Constants.ContainerModes.Containerised, Description = "Containerized" },
				"RAT HATS",
				new UNLOCO() { Code = "NZDUD", Name = "Dunedin" },
				new UNLOCO() { Code = "NZCHC", Name = "Christchurch" },
				null,
				new UNLOCO() { Code = "AUSYD", Name = "Sydney" },
				new UNLOCO() { Code = "AUBDG", Name = "Bendigo" },
				"BUNGA DELIMA",
				"343L",
				0,
				45,
				new PackageType() { Code = "KEG", Description = "Keg" },
				23.45m,
				new UnitOfVolume() { Code = "CF", Description = "Cubic Feet" },
				34.56m,
				new UnitOfWeight() { Code = "KT", Description = "Kilotons" },
				new CodeDescriptionPair() { Code = "SEA", Description = "Sea Freight" },
				null,
				"MYMASTER",
				new WayBillType() { Code = "MWB", Description = "Master Waybill" },
				null,
				new ServiceLevel() { Code = "STD", Description = "Standard" },
				ZBool.True, null, new CodeDescriptionPair() { Code = "TRF", Description = "Tariff" }, 112, 306, "LLD234", null, "AGREF123", "F234",
				new CodeDescriptionPair() { Code = "DEF", Description = "DEFAULT" },
				new CodeDescriptionPair() { Code = MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, Description = MasterFiles.Business.Customs.PaidByCodeList.Descriptions.BRK },
				new UniversalDataBuss.DataObjects.Universal.IncoTerm() { Code = "FOB", Description = "FREE ON BOARD" }, 789.012m);

			declarationDataObject.MessagingApplicationCode = new CodeDescriptionPair() { Code = "BLT" };

			var entryHeaderDataObject1 = SetupEntryHeader("EX$", "MS2", "ES3", "BG32423", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5));
			entryHeaderDataObject1.EntryLineCollection = new List<UniversalCustoms.EntryLine>(new[] { SetupEntryLine(1, "1020304050", 2585.87m, 0.84m, 800.96m, new CodeDescriptionPair() { Code = "NO", Description = "NO DESC" }, "DESC 1", null) });
			entryHeaderDataObject1.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber() });

			var entryHeaderDataObject2 = SetupEntryHeader("IM$", "MS1", "ES1", "BG89756", 869.54m, new ZDateTime(2012, 4, 4), new ZDateTime(2012, 4, 5));
			entryHeaderDataObject2.CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[]
			{
				SetupCustomsReference(new CodeDescriptionPair() { Code = "ABC" }, null, "HELLO", null, null),
				SetupCustomsReference(new CodeDescriptionPair() { Code = "DEF" }, null, "BYE", null, null)
			});
			entryHeaderDataObject2.AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>(new[]
			{
				SetupAddInfoGroup(new CodeDescriptionPair() { Code = "ABC" }),
				SetupAddInfoGroup(new CodeDescriptionPair() { Code = "DEF" })
			});
			entryHeaderDataObject2.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber2(), SetupEntryNumber() });
			var entryLineDataObject = SetupEntryLine(2, "2010304040", 1502.53m, 0.23m, 1200.40m, new CodeDescriptionPair() { Code = "KG", Description = "KG DESC" }, "DESC 2", null);
			entryHeaderDataObject2.EntryLineCollection = new List<UniversalCustoms.EntryLine>(new[]
			{
				entryLineDataObject,
				SetupEntryLine(1, "1020305060", 2585.87m, 0.84m, 800.96m, new CodeDescriptionPair() { Code = "NO", Description = "NO DESC" }, "DESC 1", null)
			});
			entryHeaderDataObject2.EntryHeaderChargeCollection = new List<UniversalCustoms.EntryHeaderCharge>(new[] { SetupEntryHeaderCharge2(), SetupEntryHeaderCharge() });
			declarationDataObject.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>(new[] { entryHeaderDataObject1, entryHeaderDataObject2 }));

			entryLineDataObject.CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[]
			{
				SetupCustomsReference(new CodeDescriptionPair() { Code = "ABC" }, null, "HELLO", null, null),
				SetupCustomsReference(new CodeDescriptionPair() { Code = "DEF" }, null, "BYE", null, null)
			});

			entryLineDataObject.EntryLineChargeCollection = new List<UniversalCustoms.EntryLineCharge>(new[] { SetupEntryLineCharge2(), SetupEntryLineCharge() });

			declarationDataObject.CommercialInfo = SetupCommercialInfo("Top Group",
				null,
				new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
				{
					SetupCommercialInvoiceHeaderData("INV1", null, null, 10000m, new Currency() { Code = Core.Constants.CurrencyCodes.UnitedStates }, new ZDateTime(2012, 4, 3), new CodeDescriptionPair() { Code = Core.Constants.IncoTerms.FreeOnBoard }, 10m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 1000m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, 11.11m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, ZDecimal.Zero, ZDecimal.Zero, 100m, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZDateTime.Empty, null, null, null,
						new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] {
							SetupCommercialInvoiceLine(1, "GOODS 1", 1.1m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Bag }, 1500m, ZString.Empty, 1.5m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 150m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
								new List<UniversalCustoms.EntryReference>(new [] {
									SetupEntryReference(1, new EntryType() { Code = "EX$" }, "BG32423"),
									SetupEntryReference(2, new EntryType() { Code = "IM$" }, "BG89756")
								})),
							SetupCommercialInvoiceLine(2, "GOODS 2", 2.2m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Bundle }, 3500m, ZString.Empty, 3.5m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 350m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
								new List<UniversalCustoms.EntryReference>(new [] {
									SetupEntryReference(2, new EntryType() { Code = "IM$" }, "BG89756")
								})),
							SetupCommercialInvoiceLine(3, "GOODS 3", 3.3m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Dozen }, 2000m, ZString.Empty, 2m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 200m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
								new List<UniversalCustoms.EntryReference>(new [] {
									SetupEntryReference(2, new EntryType() { Code = "EX$" }, "BG32423"), // should be ignore by the importation as line no 2 doesn't exist
									SetupEntryReference(1, new EntryType() { Code = "IM$" }, "BG89756")
								})),
							SetupCommercialInvoiceLine(4, "GOODS 4", 4.4m, new CodeDescriptionPair() { Code = Core.Constants.PkgUnit.Envelope }, 3000m, ZString.Empty, 3m, new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres }, 300m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, null, null, null, null,
								new List<UniversalCustoms.EntryReference>(new [] {
									SetupEntryReference(1, new EntryType() { Code = "IM$" }, "BG89756"),
									SetupEntryReference(1, new EntryType() { Code = "EX$" }, "BG32423")
								}))
						}))
				})
			);

			entryHeaderDataObject1.RelatedEntryHeaderCollection = new List<UniversalCustoms.EntryHeader>(new[]
				{
					SetupEntryHeader("GM#", "MS3", "ES3", "BGKG323", 1500m, new ZDateTime(2012, 5, 4), new ZDateTime(2012, 5, 5))
				});

			var declarationBOToLoad = Factory.New<JobDeclaration>();
			declarationBOToLoad.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			Factory.SaveForTesting();

			var masterBillDataObject = SetupAdditionalBill("MYMASTER", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 3, 1), null, SetupAddInfos("ABC=SD"), 10m, Core.Constants.PkgUnit.Box, "BOX");
			declarationDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[] { masterBillDataObject }));
			var declarationBO = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveAtEndOfImport(logger);

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				AssertEquals("Should have matched", declarationBO, declarationBOToLoad);
				AssertContents(declarationBO,
				ZAJobMessageTypeList.Codes.Import,
				Core.Constants.ContainerModes.Containerised,
				"RAT HATS", "NZDUD", "NZCHC", ZString.Empty, "AUSYD", "AUBDG", "BUNGA DELIMA",
				"343L", 0, 45, "KEG", 23.45m, "CF", 34.56m, "KT", Core.Constants.TransportModes.Sea,
				ZString.Empty, "MYMASTER", ZString.Empty, ZString.Empty,
				"STD", ZBool.True, ZString.Empty, "TRF", 112, 306,
				"LLD234", ZString.Empty, "AGREF123", "F234", "DEF", MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK, "FOB", 789.012m);

				AssertEquals(MasterFiles.Business.GlbCompany.CurrentCompany.GC_OH_OrgProxy, declarationBO.JE_OH_AgentOverride);

				declarationBO.CustomsEntryHeaders.Load();
				AssertEquals("declarationBO.CustomsEntryHeaders.Count", 0, declarationBO.CustomsEntryHeaders.Count);
			});

			if (ErrorReporter.LastKeyReported == string.Format("{0} does not support B7_Type 'WEN'", typeof(CusEntryHeader).FullName))
			{
				ErrorReporter.Clear();
			}

			#endregion
		}

		public void TestUniversalXmlMessageImport_CustomsOffice()
		{
			using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
			{
				var helper = new ZAUniversalReferenceTestDataHelper(Factory.BOFactory, false);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
				helper.CreateZACusCodeListEntry(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "4A", "4A DESC");
				Factory.SaveForTesting();

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_DeclarationReference = "BOB123456";
				declaration.JE_LocationOfGoods = "4A";
				var manager = (IShipmentDataContextManager)declaration.GetUniversalDataContextManager();
				var declarationData = (UniversalShipment)manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration))).GetDataObject(declaration);
				AssertEquals("declarationData.LocationAtClearance", "4A", declarationData.LocationAtClearance.GetCodeAsUpperCase());
				AssertEquals("declarationData.LocationAtClearance.Description", "4A DESC", declarationData.LocationAtClearance.Description);
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany);
				dataContext.CodesMappedToTarget = false;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);
				declarationData.DataContext = dataContext;

				declaration.JE_LocationOfGoods = ZString.Empty;
				Factory.SaveForTesting();

				var declarationBO = new JobDeclarationDataObjectReader(declarationData, logger, Factory).ReadIntoBusinessObject();
				AssertEquals("Should be the same", declaration, declarationBO);
				AssertEquals("declarationBO.JE_LocationOfGoods", "4A", declarationBO.JE_LocationOfGoods);
			}
		}

		public void TestUniversalXmlMessageImport_CustomsCommenced()
		{
			var factory = Factory.BOFactory;

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "BOB123456";

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.Messages.Add(Factory.NewWithValidTestData<CUSDECEDIMessage>());

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany);
			dataContext.CodesMappedToTarget = false;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "BOB123456",
				GoodsDescription = "HELLO WORLD",
			};

			Factory.SaveForTesting();

			JobDeclaration declarationBO;

			AssertNoExceptionThrown(() =>
			{
				declarationBO = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory).ReadIntoBusinessObject();
			});
			AssertContains(ValidationConstants.Declaration.CustomsHasCommenced(declaration.JE_DeclarationReference), logger.Logs);
		}

		public void TestSettingOrder_ByUsingSpecificAddInfoReader()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				Enterprise.Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var boFactory = new BusinessObjectFactory();
				var testImporter = boFactory.NewWithValidTestData<OrgHeader>() as MasterFiles.Integration.IOrgHeader;
				boFactory.Save();

				var testDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					MessagingApplicationCode = new CodeDescriptionPair() { Code = "BLT" }, // this should be ignored
					MessageType = new CodeDescriptionPair() { Code = "IMP" },
				};
				testDataObject.SetAddInfoCollection(() => new List<AddInfo>(new[]
				{
					new AddInfo() { Key = "VATClaimBackIndicator", Value = "Y" }
				}));
				testDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = testImporter.OH_Code, Address1 = testImporter.Address1, Address2 = testImporter.Address2, AddressType = "ImporterDocumentaryAddress" }
				});

				var declarationBO = new JobDeclarationDataObjectReader(testDataObject, logger, Factory).ReadIntoBusinessObject();

				CombineAssertions(() =>
				{
					AssertEquals("BLT", (declarationBO).JE_ApplicationCode);
					AssertEquals("IMP", (declarationBO).JE_MessageType);
					AssertEquals(testImporter.PK, (declarationBO).Importer.PK);
					AssertEquals("Y", declarationBO.JE_VATClaimBackIndicator);
				});
			}
		}

		public void TestAddingFetchHintsWhichRelateToEntryInstructionDoesNotCauseExceptionWhenInvoiceLineHasNoEntryInstruction()
		{
			using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "INV1";
				var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
				invoice1Line1.JI_Description = "INV1LINE1";
				invoice1Line1.JI_MatchingKey = "MATCH001";

				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceNumber = "INV2";
				var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
				invoice2Line1.JI_Description = "INV2LINE1";
				invoice2Line1.JI_MatchingKey = "MATCH002";

				declaration.JE_AutoWeightApportion = true;
				Factory.SaveForTesting();
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

				var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine()
				{
					LineNo = 1,
					Description = "LINE 1",
					DataImportMatchingKey = "MATCH001",
					LinePrice = 200,
					Weight = 0m
				};

				var invoiceDataObject1 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INV1",
					Weight = 300,
					WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms },
				}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1 })));

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = Customs.Business.JobMessageTypeList.Codes.Import },
					CommercialInfo = new UniversalCustoms.CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject1 }) { Content = CollectionContent.Partial }
					}
				};

				declarationDataObject.SetEntryInstructionCollection(() => new List<UniversalCustoms.EntryInstruction>()
					{
						new UniversalCustoms.EntryInstruction()
						{
							Style = "AAA",
							SubStyle = new CodeDescriptionPair() { Code = "S", Description = "Sub Style" },
							Link = 9
						}
					});

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
			}
		}

		public void TestAddingFetchHintsWhichRelateToEntryInstructionDoesNotCauseExceptionWhenEntryInstructionHasNoStyle()
		{
			using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice1 = declaration.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "INV1";
				var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
				invoice1Line1.JI_Description = "INV1LINE1";
				invoice1Line1.JI_MatchingKey = "MATCH001";

				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceNumber = "INV2";
				var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
				invoice2Line1.JI_Description = "INV2LINE1";
				invoice2Line1.JI_MatchingKey = "MATCH002";

				declaration.JE_AutoWeightApportion = true;
				Factory.SaveForTesting();
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

				var invoiceLineDataObject1 = new UniversalCustoms.CommercialInvoiceLine()
				{
					LineNo = 1,
					Description = "LINE 1",
					DataImportMatchingKey = "MATCH001",
					LinePrice = 200,
					Weight = 0m
				};

				var invoiceDataObject1 = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				{
					InvoiceNumber = "INV1",
					Weight = 300,
					WeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms },
				}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLineDataObject1 })));

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = Customs.Business.JobMessageTypeList.Codes.Import },
					CommercialInfo = new UniversalCustoms.CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceDataObject1 }) { Content = CollectionContent.Partial }
					}
				};

				declarationDataObject.SetEntryInstructionCollection(() => new List<UniversalCustoms.EntryInstruction>()
					{
						new UniversalCustoms.EntryInstruction()
						{
							SubStyle = new CodeDescriptionPair() { Code = "S", Description = "Sub Style" },
							Link = 9
						}
					});

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
				AssertNoExceptionThrown(() => reader.ReadIntoBusinessObject());
			}
		}
	}
}
