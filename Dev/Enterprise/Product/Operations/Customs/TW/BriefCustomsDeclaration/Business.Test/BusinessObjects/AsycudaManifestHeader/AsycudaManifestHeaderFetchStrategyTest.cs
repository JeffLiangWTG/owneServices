using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderFetchStrategy))]
	sealed class AsycudaManifestHeaderFetchStrategyTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderFetchStrategyTest
	{
		protected override Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteExecuteActionExpectedHitCounts;
				result[StmDocDataOverrideSchema.Constants.TableName] = 64;
				result[StmNoteSchema.Constants.TableName] = 20;
				result[StmUniversalCopySchema.Constants.TableName] = 44;
				result[TagLinkSchema.Constants.TableName] = 4;
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForUXMLExportStrategyExpectedHitCounts
		{
			get
			{
				var result = base.FetchForUXMLExportStrategyExpectedHitCounts;
				result.Add("RefDatabase_RefDataGrouping", 2);
				if (result.ContainsKey(CusRefTradeGroupViewSchema.Constants.TableName))
				{
					result[CusRefTradeGroupViewSchema.Constants.TableName] += 1;
				}
				else
				{
					result.Add(CusRefTradeGroupViewSchema.Constants.TableName, 1);
				}
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts;
				result[ProcessHeaderLinkSchema.Constants.TableName] = 2;
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateExecuteActionExpectedHitCounts;
				result.Add(OrgHeaderSchema.Constants.TableName, 6);
				result.Add("RefDatabase_RefDataGrouping", 2);
				result[CusEntryNumSchema.Constants.TableName] = 2;
				result[ZZRefCusCodeListCombinedSchema.Constants.TableName] = 33;
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForValidateUnconsumedExpectedHitCounts
		{
			get
			{
				var result = base.FetchForValidateUnconsumedExpectedHitCounts;
				result.Remove(AsycudaPackedItemSchema.Constants.TableName);
				return result;
			}
		}

		protected override ZString Message => "TW ASYCUDA Header (AsycudaManifestHeader)";
		protected override string ApplicationCode => ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
		protected override Type AsycudaManifestHeaderTypeForTest => typeof(AsycudaManifestHeader);
		protected override ZString HeaderCountry => Core.Constants.CountryCodes.Taiwan;

		protected override void PopulateTax(ASYCUDA.Business.AsycudaTax tax, ZString chargetype, ZString methodofcalculation, ZString methodofpayment)
		{
			if (tax != null)
			{
				base.PopulateTax(tax, chargetype, methodofcalculation, methodofpayment);
			}
		}
	}
}
