using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	class WhsCartonGroupFilterControlTest : WhsEnvFilterControlDBHitsTestCase<WhsCartonGroupCollection, WhsCartonGroupFilterBusinessObject>
	{
		#region SetupData

		protected override void SetupData()
		{
			for (int i = 0; i < 10; i++)
			{
				var group = Helper.CreateWhsCartonGroup(i.ToString(), i.ToString());

				for (int j = 0; j < i; j++)
				{
					var uniqueCode = i.ToString() + j;
					var client = Helper.CreateClient(uniqueCode);
					client.MiscServ.OM_WCG_CartonGroup = group.PK;
					group.CartonSizes.Add(Helper.CreateWhsCartonSize(uniqueCode));
				}
			}
		}

		#endregion

		protected override bool ShouldCheckForUnusedFetchHints(string columnName) => false;

		#region GetExpectedHitsDictionary

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var hitsDictionary = new Dictionary<string, Dictionary<string, int>>();

			var attachedOrganisationHits = new Dictionary<string, int>(GetBaseHits());
			attachedOrganisationHits.Add(OrgMiscServSchema.Constants.TableName, 1);
			attachedOrganisationHits.Add(OrgHeaderSchema.Constants.TableName, 1);

			hitsDictionary.Add("AttachedOrganisationsCodes", attachedOrganisationHits);

			return hitsDictionary;
		}

		#endregion

		#region GetBaseHits

		protected override Dictionary<string, int> GetBaseHits()
		{
			var baseHits = new Dictionary<string, int>();
			baseHits.Add(WhsCartonGroupSchema.Constants.TableName, 1);
			return baseHits;
		}

		#endregion

		#region GetNewCollection

		protected override WhsCartonGroupCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsCartonGroupCollection(factory);
		}

		#endregion

		#region GetNewFilterBusinessObject

		protected override WhsCartonGroupFilterBusinessObject GetNewFilterBusinessObject() => new WhsCartonGroupFilterBusinessObject();

		#endregion

		#region GetNewFilterControl

		protected override ZFilterStripControl GetNewFilterControl(WhsCartonGroupCollection collection, WhsCartonGroupFilterBusinessObject filterBizO)
		{
			return new WhsCartonGroupFilterControl(collection, filterBizO);
		}

		#endregion
	}
}
