using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;

namespace Enterprise.Warehouse.Transit.Business
{
	public sealed class TWHDeliveryValidationProvider : ITWHDeliveryValidationProvider
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

			var validationResult = GetJobMatcherResult(trw, request.ReferenceNumber, referenceNumberType);

			if (validationResult.ErrorCode != null)
			{
				return new TWHValidationResponse()
				{
					Result = ValidationResult.Decline,
					MessageCode = validationResult.ErrorCode,
					Message = TWHJobValidationHelper.GetErrorMessage(validationResult.ErrorCode, validationResult.ErrorMessage)
				};
			}

			return new TWHValidationResponse()
			{
				Result = ValidationResult.Accept,
				ReferenceNumberType = validationResult.ReferenceNumberType.ToString(),
				GrossWeightValue = validationResult.GrossWeightValue,
				GrossWeightUnit = validationResult.GrossWeightUnit,
				GrossVolumeValue = validationResult.GrossVolumeValue,
				GrossVolumeUnit = validationResult.GrossVolumeUnit,
				QuantityValue = validationResult.QuantityValue
			};
		}

		TWHJobMatcherResult GetJobMatcherResult(WhsWarehouse trw, string referenceNumber, ReferenceNumberTypes referenceNumberType)
		{
			var rtuJobMatcher = new TWHRTUJobMatcher(Factory, trw, referenceNumber, referenceNumberType);
			var rcnJobMatcher = new TWHRCNJobMatcher(Factory, trw, referenceNumber, referenceNumberType);
			var asnJobMatcher = new TWHASNJobMatcher(Factory, trw, referenceNumber, referenceNumberType);

			rtuJobMatcher.SetNextJobMatcher(rcnJobMatcher);
			rcnJobMatcher.SetNextJobMatcher(asnJobMatcher);

			return rtuJobMatcher.Process();
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
