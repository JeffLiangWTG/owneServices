using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.DataTransfer.Gate;
using Enterprise.Warehouse.Yard.DataTransfer.Universal;
using Enterprise.Warehouse.Yard.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer
{
	public sealed class YardDropoffValidationProvider : IYardDropoffValidationProvider
	{
		IYardValidationData IYardValidationProvider.Get(IYardValidationRequest request)
		{
			var input = (IYardDropoffRequest)request;
			var (result, errorCode) = RetrieveRelatedContainer(input.FacilityCode, input.OrgCode, input.AddressCode, input.ReferenceNumber, !input.IsLaden);

			return errorCode switch
			{
				YardDropoffData.ResponseCode.FNF or
				YardDropoffData.ResponseCode.CGI => new YardDropoffData
				{
					Result = YardDropoffData.ResultEnum.Reject,
					MessageCode = errorCode,
					Message = YardDropoffData.ErrorMessages[errorCode.Value]
				},
				YardDropoffData.ResponseCode.CNF => new YardDropoffData
				{
					Result = YardDropoffData.ResultEnum.Unavailable,
					ContainerNumber = input.ReferenceNumber
				},
				null => new YardDropoffData()
				{
					Result = YardDropoffData.ResultEnum.Accept,
					ContainerNumber = result.YUS_UnitID,
					IsoCode = result.ReceiveAdviceLine.UnitLineItem.ContainerType.RC_ISOType,
					ContainerCode = result.ReceiveAdviceLine.UnitLineItem.ContainerType.RC_Code,
#pragma warning disable EDI007 // Disable the warning Customizable Data Translation Rule for RC_Description.
					ContainerDescription = result.ReceiveAdviceLine.UnitLineItem.ContainerType.RC_Description,
#pragma warning restore EDI007 // Enable Customizable Data Translation Rule
					Owner = GetCommunityCodeFromAddress(result.ReceiveAdvice.Client),
					ReferenceNumber = result.ReceiveAdvice?.YRA_AcceptanceNumber,
					IsStoringOrderAvailable = result.ReceiveAdvice is not null,
					AvailableDateUtc = result.ReceiveAdvice?.Yard.GetWarehouseBranchLocalDateTimeOffset(result.ReceiveAdvice.YRA_FromDate).ToUtcDateTime(),
					ExpiryDateUtc = result.ReceiveAdvice?.Yard.GetWarehouseBranchLocalDateTimeOffset(result.ReceiveAdvice.YRA_ToDate).ToUtcDateTime(),
					Seal = string.Empty,
					Voyage = string.Empty,
					Vessel = string.Empty,
				},
				_ => throw new InvalidOperationException(@"Invalid error code: ${errorCode}")
			};
		}

		static (CYDYardUnitState Result, YardDropoffData.ResponseCode? Error) RetrieveRelatedContainer(string communityCode, string orgCode, string addressCode, string containerNumber, bool? empty)
		{
			var factory = new BusinessObjectFactory();
			var facility = FacilityMatchingHelper.GetFacilityFromCommunityCode(factory, communityCode, WarehouseTypes.Codes.ContainerYard)
				?? FacilityMatchingHelper.GetFacilityFromOrgCodeAndAddressCode(factory, orgCode, addressCode, WarehouseTypes.Codes.ContainerYard);
			if (facility == null)
			{
				return (Result: null, Error: YardDropoffData.ResponseCode.FNF);
			}

			if (IsUnitInYard(factory, containerNumber, facility))
			{
				return (Result: null, Error: YardDropoffData.ResponseCode.CGI);
			}

			var yardUnit = YardMatchingHelper.GetYardUnitForDropOff(factory, containerNumber, facility);
			if (yardUnit is null)
			{
				return (Result: null, Error: YardDropoffData.ResponseCode.CNF);
			}

			return (Result: yardUnit, Error: null);
		}

		static ZString GetCommunityCodeFromAddress(JobDocAddress address)
		{
			var communityCode = address.Address.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ContainerChainCommunityCode, string.Empty);
			return communityCode.IsEmpty
				? address.Organisation.OH_Code
				: communityCode;
		}

		static bool IsUnitInYard(BusinessObjectFactory factory, string containerNumber, WhsWarehouse yard)
		{
			var query = new ZDBOnlyQuery(typeof(CYDYardUnitState));
			query.AddToFilter(CYDYardUnitStateSchema.YUS_UnitID, containerNumber);
			query.AddToFilter(CYDYardUnitStateSchema.YUS_WW_CurrentYard, yard.PK);
			var yardUnits = factory.Load<CYDYardUnitState>(query);
			return yardUnits.Any(unit => unit.HasBeenGatedIn && !unit.HasBeenGatedOut && !unit.HasBeenRejected);
		}
	}
}
