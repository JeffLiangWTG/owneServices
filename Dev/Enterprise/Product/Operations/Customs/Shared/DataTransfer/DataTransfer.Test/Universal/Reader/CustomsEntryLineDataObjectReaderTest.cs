using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestCusEntryLineFieldCL_AddINfoMapping()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var expectedEntryLineAddInfo = string.Format("{0}=56.29NZD", AUAddInfoSchema.ZA_TILV.Name.Substring(3));
				var entryLineAddInfo = "SDH2@#$=HELLO*" + expectedEntryLineAddInfo;
				var entryLineDataObject = SetupEntryLine(AddInfoCollectionCreator.CreateCollection(entryLineAddInfo));
				var entryLineBO =
					new CustomsEntryLineDataObjectReader(entryLineDataObject, logger, CurrentCompanyHelper, entryHeader)
						.ReadIntoBusinessObject();
				AssertEquals("entryLineBO.CL_AddInfo", expectedEntryLineAddInfo, entryLineBO.CL_AddInfo);
			}
		}

		public void TestCusEntryLineFieldMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLineDataObject = SetupEntryLine();
			var reader = new CustomsEntryLineDataObjectReader(entryLineDataObject, logger, CurrentCompanyHelper, entryHeader);
			var entryLineBO = reader.ReadIntoBusinessObject();
			AssertCusEntryLineContents(entryLineBO, entryHeader.PK, "");
			AssertEquals(0, entryLineBO.Fees.Count);
			var childCusCodeDataBOs = LoadCusCodeData(entryLineBO.TablePrefix, entryLineBO.PK);
			AssertEquals("Child CusCusCodeData", 0, childCusCodeDataBOs.Length);

			AssertNotEquals("PreCondition", 1000.10m, entryLineBO.CL_CustomsValue);
			entryLineBO.CL_CustomsValue = 1000.10m;
			var entryLineFeeBOThatShouldBeDeleted = entryLineBO.Fees.AddOrUpdate("WCL", 150m);

			entryLineDataObject.EntryLineChargeCollection = new List<UniversalCustoms.EntryLineCharge>(new[] { SetupEntryLineCharge2(), SetupEntryLineCharge() });
			logger.ClearLogs();
			var entryLineBO2 = reader.ReadIntoBusinessObject();
			CombineAssertions(delegate
			{
				AssertNotEquals("A new one should be created as there should be no matching", entryLineBO, entryLineBO2);
				AssertEquals("Existing fees should not be touched", false, entryLineFeeBOThatShouldBeDeleted.IsDeleted);
				AssertCusEntryLineContents(entryLineBO2, entryHeader.PK);
				entryLineBO2.Fees.Load();
				AssertEquals(2, entryLineBO2.Fees.Count);
				var entryLineFeeBO1 = entryLineBO2.Fees.OfType<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == "WCL");
				var entryLineFeeBO2 = entryLineBO2.Fees.OfType<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType != "WCL");
				AssertCusEntryLineFeeContents2(entryLineFeeBO1, entryLineBO2.PK);
				AssertCusEntryLineFeeContents(entryLineFeeBO2, entryLineBO2.PK);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusEntryLine found, creating new CusEntryLine.
Information - Populating CusEntryLine...
Information - No matching CusEntryLineFee found, creating new CusEntryLineFee.
Information - Populating CusEntryLineFee...
Information - No matching CusEntryLineFee found, creating new CusEntryLineFee.
Information - Populating CusEntryLineFee...
".Trim(), logger.Logs);
			});
		}
	}
}
