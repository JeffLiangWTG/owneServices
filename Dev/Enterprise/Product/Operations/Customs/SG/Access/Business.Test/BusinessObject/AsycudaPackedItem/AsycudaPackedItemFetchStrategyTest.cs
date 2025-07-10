using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class AsycudaPackedItemFetchStrategyTest : ASYCUDA.Business.Testing.AsycudaPackedItemFetchStrategyTest
	{
		protected override Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts
		{
			get
			{
				var dictionary = base.FetchForDeleteExecuteActionExpectedHitCounts;
				dictionary[StmDocDataOverrideSchema.Constants.TableName] = 6;
				dictionary[StmUniversalCopySchema.Constants.TableName] = 5;
				return dictionary;
			}
		}

		protected override ZString Message => "SG Access Packed Item (AsycudaPackedItem)";

		protected override Type AsycudaManifestHeaderTypeForTest => typeof(AsycudaManifestHeader);

		protected override Dictionary<string, int> FetchForValidateExecuteActionExpectedHitCounts => new Dictionary<string, int>()
		{
			{ GenAddOnColumnSchema.Constants.TableName, 1 },
		};

		protected override void SetUp()
		{
			base.SetUp();
			var sgPackedItem = (AsycudaPackedItem)packedItem;
			sgPackedItem.GoodsType = Constants.GoodsType.NormalGoods;
			sgPackedItem.SetSystemDefinedValue("ForTestingHits", new ZString("TEST"));
			Factory.Save();
		}
	}
}
