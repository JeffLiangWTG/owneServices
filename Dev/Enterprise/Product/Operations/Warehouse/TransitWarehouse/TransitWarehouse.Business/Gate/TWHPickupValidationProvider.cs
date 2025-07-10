using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;

namespace Enterprise.Warehouse.Transit.Business
{
	public sealed class TWHPickupValidationProvider : ITWHPickupValidationProvider
	{
		ITWHValidationResponse ITWHValidationProvider.Get(ITWHValidationRequest request)
		{
			if (!Enum.TryParse(request.ReferenceNumberType, out ReferenceNumberTypes referenceNumberType))
			{
				if (request.ReferenceNumberType.IsNullOrEmpty())
				{
					referenceNumberType = ReferenceNumberTypes.Unknown;
				}
				else
				{
					return new TWHValidationResponse()
					{
						Result = ValidationResult.Decline,
						ReferenceNumberType = referenceNumberType.ToString(),
						MessageCode = ValidationErrorCode.INV,
						Message = TWHJobValidationHelper.GetErrorMessage(ValidationErrorCode.INV, string.Empty)
					};
				}
			}

			var trw = GateMatchingHelper.GetFacilityFromCommunityCode(Factory, request.FacilityCode, WarehouseTypes.Codes.Transit)
				?? GateMatchingHelper.GetFacilityFromOrgCodeAndAddressCode(Factory, request.OrgCode, request.AddressCode, WarehouseTypes.Codes.Transit);

			if (trw == null)
			{
				return new TWHValidationResponse()
				{
					Result = ValidationResult.Decline,
					ReferenceNumberType = referenceNumberType.ToString(),
					MessageCode = ValidationErrorCode.FNF,
					Message = TWHJobValidationHelper.GetErrorMessage(ValidationErrorCode.FNF, string.Empty)
				};
			}

			var jobMatcherResult = GetJobMatcherResult(trw, request.ReferenceNumber, referenceNumberType);

			if (jobMatcherResult.ErrorCode != null)
			{
				return new TWHValidationResponse()
				{
					Result = ValidationResult.Decline,
					MessageCode = jobMatcherResult.ErrorCode,
					Message = TWHJobValidationHelper.GetErrorMessage(jobMatcherResult.ErrorCode, jobMatcherResult.ErrorMessage)
				};
			}

			return new TWHValidationResponse()
			{
				Result = ValidationResult.Accept,
				ReferenceNumberType = jobMatcherResult.ReferenceNumberType.ToString(),
				GrossWeightValue = jobMatcherResult.GrossWeightValue,
				GrossWeightUnit = jobMatcherResult.GrossWeightUnit,
				GrossVolumeValue = jobMatcherResult.GrossVolumeValue,
				GrossVolumeUnit = jobMatcherResult.GrossVolumeUnit,
				QuantityValue = jobMatcherResult.QuantityValue
			};
		}

		TWHJobMatcherResult GetJobMatcherResult(WhsWarehouse trw, string referenceNumber, ReferenceNumberTypes referenceNumberType)
		{
			var dtuMatcher = new TWHDTUJobMatcher(Factory, trw, referenceNumber, referenceNumberType);
			var dcnMatcher = new TWHDCNJobMatcher(Factory, trw, referenceNumber, referenceNumberType);
			var dllMatcher = new TWHDLLJobMatcher(Factory, trw, referenceNumber, referenceNumberType);

			dtuMatcher.SetNextJobMatcher(dcnMatcher);
			dcnMatcher.SetNextJobMatcher(dllMatcher);

			return dtuMatcher.Process();
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
