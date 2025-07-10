using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Calculate Weight And Dims For Handling Unit")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public HandlingUnitWeightAndDimensionsResponse CalculateWeightAndDimsForHandlingUnit(Guid packagePK, string refPackTypeCode)
		{
			return HandleWebServiceRequest<HandlingUnitWeightAndDimensionsResponse>(response => CalculateWeightAndDimsForHandlingUnit(response, packagePK, refPackTypeCode));
		}

		void CalculateWeightAndDimsForHandlingUnit(HandlingUnitWeightAndDimensionsResponse response, ZGuid packagePK, string refPackTypeCode)
		{
			var handlingUnit = Factory.Load<PkgPackage>(packagePK);
			if (handlingUnit == null)
			{
				response.LogBusinessValidationError(Res.GetString("576e70e6-5ac6-4a82-822c-71ebf22acde7", "Handling Unit was not found."));
			}
			else
			{
				var refPackType = Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, refPackTypeCode);
				if (refPackType == null)
				{
					response.LogBusinessValidationError(Res.GetString("bb82aba3-81ef-4873-af79-108be3c99757", "Pack Type was not found."));
				}
				else
				{
					var huDivots = handlingUnit.PackageHandlingUnitHandlingUnitDivots;

					var innerPackagesQuery = new ZQuery(PkgPackageSchema.PK, huDivots.Select(d => d.KPD_KP_Package));
					var innerPackageRows = ((IBusinessObjectFactoryInternals)Factory).RowFactory.Load(PkgPackageSchema.Constants.TableName, innerPackagesQuery); // avoid triggering type decider.
					var packageWeights = innerPackageRows.Select(row => ((decimal)row[nameof(PkgPackageSchema.KP_Weight)], (string)row[nameof(PkgPackageSchema.KP_WeightUQ)]));

					var dimensionsInfo = new PackageDimensionsInfo
					{
						PackType = refPackTypeCode,

						Length = refPackType.F3_Length,
						Width = refPackType.F3_Width,
						Height = refPackType.F3_Height,
						DimensionUQ = refPackType.F3_UnitOfDimension,

						EmptyWeight = refPackType.F3_Weight,
						Weight = CalculateTotalHandlingUnitWeight(packageWeights, refPackType),
						WeightUQ = refPackType.F3_UnitOfWeight,
					};

					response.PackageDimensions = dimensionsInfo;
				}
			}
		}

		decimal CalculateTotalHandlingUnitWeight(IEnumerable<(decimal Weight, string WeightUQ)> packageWeights, RefPackType refPackType)
		{
			var totalHandlingUnitWeight = refPackType.F3_Weight;

			var groupedPackages = packageWeights.GroupBy(pkg => pkg.WeightUQ);
			totalHandlingUnitWeight += groupedPackages.Sum(gp => Constants.Weight.Convert(gp.Sum(pkg => pkg.Weight), gp.Key, refPackType.F3_UnitOfWeight));

			return totalHandlingUnitWeight;
		}
	}
}
