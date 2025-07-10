using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderFetchStrategyTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderFetchStrategyTest
	{
		protected override ZString Message => "ZA ASYCUDA Header (AsycudaManifestHeader)";
		protected override Type AsycudaManifestHeaderTypeForTest => typeof(AsycudaManifestHeader);
		protected override ZString HeaderCountry => Core.Constants.CountryCodes.SouthAfrica;
		protected override Dictionary<string, int> FetchForValidateExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateExecuteActionExpectedHitCounts;
				result[RefDataGroupingSchema.Constants.TableName] = 2;
				result[GenAddOnColumnSchema.Constants.TableName] = 7;
				result[ZZRefCusCodeListCombinedSchema.Constants.TableName] = 25;
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteExecuteActionExpectedHitCounts;
				result[CusCodeDataSchema.Constants.TableName] = 1;
				result[StmDocDataOverrideSchema.Constants.TableName] = 70;
				result[StmNoteSchema.Constants.TableName] = 21;
				result[StmUniversalCopySchema.Constants.TableName] = 49;
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateFetchStrategyExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateFetchStrategyExpectedHitCounts;
				result[GenAddOnColumnSchema.Constants.TableName] = 1;
				result.Add(CusCodeDataSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForUXMLExportStrategyExpectedHitCounts
		{
			get
			{
				var result = base.FetchForUXMLExportStrategyExpectedHitCounts;
				result.Add(GenAddOnColumnSchema.Constants.TableName, 3);
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForDeleteFetchStrategyExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteFetchStrategyExpectedHitCounts;
				result[GenAddOnColumnSchema.Constants.TableName] = 2;
				return result;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = (AsycudaManifestHeader)base.header;
			SetupHeaderCaseNumbers(header);
			SetupBillCaseNumbers(header.Bills[0]);
			SetupBillCaseNumbers(header.Bills[1]);
			Factory.Save();
		}

		void SetupBillCaseNumbers(AsycudaBill bill)
		{
			for (var i = 1; i < 4; i++)
			{
				var caseNumber = bill.CaseNumbers.AddNew();
				caseNumber.Document_Status = ZA.Business.DocumentStatusCodes.Codes.PND;
				caseNumber.CY_Data = "CASENUM" + i;
			}
		}

		void SetupHeaderCaseNumbers(AsycudaManifestHeader header)
		{
			for (var i = 1; i < 4; i++)
			{
				var caseNumber = header.CaseNumbers.AddNew();
				caseNumber.Document_Status = ZA.Business.DocumentStatusCodes.Codes.FAL;
				caseNumber.CY_Data = "CASENUM" + i;
			}
		}
	}
}
