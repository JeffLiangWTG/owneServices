using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class AsycudaContainerFetchStrategyTest : ManifestBase.Testing.AsycudaContainerFetchStrategyTest
	{
		protected override Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteExecuteActionExpectedHitCounts;
				result.Add(GenAddOnColumnSchema.Constants.TableName, 2);
				result[StmDocDataOverrideSchema.Constants.TableName] = 20;
				result[StmUniversalCopySchema.Constants.TableName] = 11;
				return result;
			}
		}

		protected override Dictionary<string, int> FetchForDeleteUnconsumedExpectedHitCounts
		{
			get
			{
				var result = base.FetchForDeleteUnconsumedExpectedHitCounts;
				result.Add(GenCustomAddOnValueSchema.Constants.TableName, 1);
				result.Add(UNDGDataItemSchema.Constants.TableName, 1);
				return result;
			}
		}

		protected override ZString Message => "NZ Container (AsycudaContainer)";

		protected override string ApplicationCode => ApplicationCodeTypeList.Codes.Consolidator;

		protected override Type AsycudaManifestHeaderTypeForTest => typeof(AsycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var container = (AsycudaContainer)base.container;
			container.SendMCDInformation = true;
			container.HasMPIQD = true;
			container.IsContainerClean = true;
			container.IsPackingContaminated = true;
			container.IsWoodPackingUsed = true;
			container.IsWoodPackingTreated = true;
			container.HasWoodPackingTreatmentCert = true;
			container.DeliveryDestination = orgHeader.MainAddress.PK;
			var containerLergPickup = container.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ContainerLegPickupAddress);
			containerLergPickup.E2_OA_Address = orgHeader.MainAddress.PK;
			Factory.Save();
		}
	}
}
