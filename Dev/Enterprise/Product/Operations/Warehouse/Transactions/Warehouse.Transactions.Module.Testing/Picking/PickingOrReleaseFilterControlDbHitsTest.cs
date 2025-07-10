using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	abstract class PickingOrReleaseFilterControlDbHitsTest<TFilterBizO> : WhsFilterControlDBHitsTestCase<WhsPickCollection, TFilterBizO>
		where TFilterBizO : PickingFilterBusinessObject
	{
		#region Scaffolding

		protected override WhsPickCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsPickCollection(factory);
		}

		protected override Dictionary<string, int> GetBaseHits()
		{
			var expectedDbHits = new Dictionary<string, int>
			{
				{ WhsPickSchema.Constants.TableName, 1 }
			};
			return expectedDbHits;
		}

		#endregion

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var baseHits = GetBaseHits();
			var hitsDictionary = new Dictionary<string, Dictionary<string, int>>();

			#region

			var whsHits = new Dictionary<string, int>(baseHits)
			{
				{ WhsWarehouseSchema.Constants.TableName, 1 }
			};
			hitsDictionary.Add(nameof(WhsPick.WP_WW_Whs), whsHits);

			var dockDoorHits = new Dictionary<string, int>(baseHits)
			{
				{ WhsWarehouseSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 1 }
			};
			hitsDictionary.Add(nameof(WhsPick.DockDoorPK), dockDoorHits);

			var clientCodesHits = new Dictionary<string, int>(baseHits)
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 }
			};
			hitsDictionary.Add(nameof(WhsPick.ClientCodes), clientCodesHits);

			var clientNamesHits = new Dictionary<string, int>(baseHits)
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 }
			};
			hitsDictionary.Add(nameof(WhsPick.ClientNames), clientNamesHits);

			var consigneeCodesHits = new Dictionary<string, int>(baseHits)
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};
			hitsDictionary.Add(nameof(WhsPick.ConsigneeCodes), consigneeCodesHits);

			var earliestRequiredDateHits = new Dictionary<string, int>(baseHits)
			{
				{ WhsDocketSchema.Constants.TableName, 1 },
			};
			hitsDictionary.Add(nameof(WhsPick.EarliestRequiredDate), earliestRequiredDateHits);

			var consigneeNamesHits = new Dictionary<string, int>(baseHits)
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};
			hitsDictionary.Add(nameof(WhsPick.ConsigneeNames), consigneeNamesHits);

			var transportCodesHits = new Dictionary<string, int>(baseHits)
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
			};
			hitsDictionary.Add(nameof(WhsPick.TransportCodes), transportCodesHits);

			var transportReferencesHits = new Dictionary<string, int>(baseHits)
			{
				{ WhsDocketSchema.Constants.TableName, 1 },
			};
			hitsDictionary.Add(nameof(WhsPick.TransportReferences), transportReferencesHits);

			var distributionCentreCodesHits = new Dictionary<string, int>(baseHits)
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
			};
			hitsDictionary.Add(nameof(WhsPick.DistributionCentreCodes), distributionCentreCodesHits);

			var serviceLevelsHits = new Dictionary<string, int>(baseHits)
			{
				{ WhsDocketSchema.Constants.TableName, 1 },
			};
			hitsDictionary.Add(nameof(WhsPick.ServiceLevels), serviceLevelsHits);

			var pickPriorityHits = new Dictionary<string, int>(baseHits)
			{
				{ WhsDocketSchema.Constants.TableName, 1 },
			};
			hitsDictionary.Add(nameof(WhsPick.PickPriority), pickPriorityHits);

			#endregion

			var salesChannelHits = new Dictionary<string, int>(baseHits)
			{
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsSalesChannelSchema.Constants.TableName, 1 },
			};
			hitsDictionary.Add(nameof(WhsPick.SalesChannelDescriptions), salesChannelHits);

			return hitsDictionary;
		}

		#region Need validation

		protected override IEnumerable<string> GetTableNamesToIgnoreForUnusedFetchHints(string columnName)
		{
			var tableNamesToIgnore = base.GetTableNamesToIgnoreForUnusedFetchHints(columnName).ToList();
			tableNamesToIgnore.Add(GenAddOnColumnSchema.Constants.TableName);
			tableNamesToIgnore.Add(WhsDocketLineSchema.Constants.TableName);

			switch (columnName)
			{
				case nameof(WhsPick.ClientCodes):
				case nameof(WhsPick.ClientNames):
					tableNamesToIgnore.Add(OrgCompanyDataSchema.Constants.TableName);
					break;
				case nameof(WhsPick.ConsigneeCodes):
				case nameof(WhsPick.ConsigneeNames):
				case nameof(WhsPick.DistributionCentreCodes):
					tableNamesToIgnore.Add(GenCustomAddOnRuleAckSchema.Constants.TableName);
					tableNamesToIgnore.Add(OrgAddressCapabilitySchema.Constants.TableName);
					tableNamesToIgnore.Add(OrgCompanyDataSchema.Constants.TableName);
					break;
				case nameof(WhsPick.TransportCodes):
					tableNamesToIgnore.Add(OrgHeaderSchema.Constants.TableName);
					tableNamesToIgnore.Add(GenCustomAddOnRuleAckSchema.Constants.TableName);
					tableNamesToIgnore.Add(OrgAddressCapabilitySchema.Constants.TableName);
					tableNamesToIgnore.Add(OrgCompanyDataSchema.Constants.TableName);
					break;
				default:
					break;
			}

			return tableNamesToIgnore;
		}

		#endregion

		protected override void SetupData()
		{
			var numberOfPicksToCreate = 10;

			var clients = new List<OrgHeader>();
			var parts = new List<OrgSupplierPart>();
			var warehouses = new List<WhsWarehouse>();
			var salesChannels = new List<WhsSalesChannel>();
			var distributionCentres = new List<OrgHeader>();

			for (int index = 0; index < numberOfPicksToCreate; index++)
			{
				var postfix = index.ToString();
				var client = Helper.CreateClient("O" + postfix);
				clients.Add(client);

				var part = Helper.CreateProduct(client, "p1" + postfix);
				parts.Add(part);

				var warehouse = Helper.CreateWarehouse("W" + postfix, "L" + postfix);
				warehouses.Add(warehouse);

				var salesChannel = Helper.CreateWhsSalesChannel($"WS{postfix}", $"Sales Channel {postfix}");
				salesChannels.Add(salesChannel);

				var distributionCentre = Helper.CreateClient("DC" + postfix);
				distributionCentres.Add(distributionCentre);
			}
			Factory.Save();

			for (int index = 0; index < numberOfPicksToCreate; index++)
			{
				var postfix = index.ToString();

				var order = Helper.CreateWhsOrderWithOrderLine(clients[index], warehouses[index], $"Order{postfix}", parts[index], 1);
				order.WD_WSH_SalesChannel = salesChannels[index].PK;

				var transportCo = Helper.CreateClient("T" + postfix);
				order.TransportCoPK = transportCo.PK;

				order.DistributionCentreDocAddress.OrganisationPK = distributionCentres[index].PK;
				Helper.CreatePickNew(order);
			}
		}
	}
}
