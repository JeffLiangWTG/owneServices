using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetCartonGroupUsingTrolleyJobPKAndPackageID

		[WebMethod(Description = "Get the Carton Group using Package ID")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsCartonGroupInfoWebServiceResponse GetCartonGroupUsingTrolleyJobPKAndPackageID(Guid trolleyJobPK, string packageID)
		{
			return HandleWebServiceRequest<WhsCartonGroupInfoWebServiceResponse>(r => GetCartonGroupUsingTrolleyJobPKAndPackageIDCore(r, trolleyJobPK, packageID));
		}

		void GetCartonGroupUsingTrolleyJobPKAndPackageIDCore(WhsCartonGroupInfoWebServiceResponse response, Guid trolleyJobPK, string packageID)
		{
			var trolleyJob = WebServiceHelper.GetTrolleyJobUsingTrolleyJobPK(Factory, response, trolleyJobPK);
			if (string.IsNullOrEmpty(response.ErrorMessage))
			{
				var package = WebServiceHelper.GetPackageOnTrolley(response, trolleyJob, packageID);
				if (string.IsNullOrEmpty(response.ErrorMessage))
				{
					var cartonGroups = GetAllCartonGroups(response, package);

					if (string.IsNullOrEmpty(response.ErrorMessage))
					{
						response.CartonGroups = new WhsCartonGroupInfoCollection(cartonGroups);
					}
				}
			}

			if (!string.IsNullOrEmpty(response.ErrorMessage))
			{
				response.Error = ErrorTypes.BusinessValidationError;
			}
		}

		IEnumerable<WhsCartonGroup> GetAllCartonGroups(WebServiceResponse response, PkgPackage package)
		{
			var result = new List<WhsCartonGroup>();
			var cartonGroup = GetCartonGroupUsingPackage(package);
			var cartonGroups = GetRelatedCartonGroupsUsingPackagePK(package.PK.ToGuid());

			if ((cartonGroups == null || !cartonGroups.Any()) && cartonGroup == null)
			{
				response.ErrorMessage = Res.GetString("77f66a99-9523-4e1c-b5af-bb9a0598db9f", "Package '{0}' has not available Carton Groups & Sizes.", package.KP_PackageID);
			}
			else
			{
				if (cartonGroups.Any())
				{
					result.AddRange(cartonGroups);
				}
				if (cartonGroup != null && !result.Any(cg => cg.WCG_Code == cartonGroup.WCG_Code))
				{
					result.Add(cartonGroup);
				}
			}
			return result;
		}

		WhsCartonGroup[] GetRelatedCartonGroupsUsingPackagePK(Guid packagePK)
		{
			var packageItemDivotSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_ParentID);
			packageItemDivotSubQuery.AddToFilter(PkgPackageItemDivotSchema.KI_ParentTableCode, WhsPickLineSchema.Constants.Prefix);
			packageItemDivotSubQuery.AddToFilter(PkgPackageItemDivotSchema.KI_KP_Package, packagePK);

			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddSubQuery(packageItemDivotSubQuery, JoinCondition.And);

			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_OP);
			docketLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var partSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), OrgPartRelationSchema.OU_OP);
			partSubQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

			var relationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_WCG_CartonGroup);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Owner);
			relationSubQuery.AddToFilter(JoinCondition.Or, OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Both);
			relationSubQuery.AddSubQuery(partSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsCartonGroup));
			query.AddSubQuery(relationSubQuery, JoinCondition.And);

			return Factory.Load<WhsCartonGroup>(query);
		}

		WhsCartonGroup GetCartonGroupUsingPackage(PkgPackage package)
		{
			var orderQuery = new ZDBOnlyQuery(typeof(WhsOrder));
			var subQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			subQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentTableCode, WhsDocketSchema.Constants.Prefix);
			subQuery.AddToFilter(PkgPackageJobSchema.PK, package.PackageJob.PK);
			orderQuery.AddSubQuery(subQuery, JoinCondition.And);

			var order = Factory.LoadTop1<WhsOrder>(orderQuery);
			var cartonGroupPK = order?.CartonGroup.OrgCartonGroupPK;
			return cartonGroupPK.HasValue && cartonGroupPK.Value.IsValid ? Factory.Load<WhsCartonGroup>(cartonGroupPK.Value) : null;
		}

		#endregion
	}
}
