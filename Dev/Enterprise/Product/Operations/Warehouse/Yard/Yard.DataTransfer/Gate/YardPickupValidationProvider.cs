using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.DataTransfer.Gate;
using Enterprise.Warehouse.Yard.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer
{
	public sealed class YardPickupValidationProvider : IYardPickupValidationProvider
	{
		IYardValidationData IYardValidationProvider.Get(IYardValidationRequest request)
		{
			var input = (IYardPickupRequest)request;
			var (releaseAdviceLines, errorCode) = RetrieveRelatedReleaseAdviceLines(input.FacilityCode, input.OrgCode, input.AddressCode, input.ReferenceNumber);
			if (errorCode is not null)
			{
				return new YardPickupData()
				{
					Result = YardPickupData.ResultEnum.Reject,
					MessageCode = errorCode,
					Message = YardPickupData.ErrorMessages[errorCode.Value]
				};
			}

			return new YardPickupData()
			{
				Result = YardPickupData.ResultEnum.Accept,
				ReleaseDetails = CreateReleaseDetails(releaseAdviceLines)
			};
		}

		(CYDReleaseAdviceLine[] Result, YardPickupData.ResponseCode? Error) RetrieveRelatedReleaseAdviceLines(string communityCode, string orgCode, string addressCode, string releaseNumber)
		{
			CYDReleaseAdviceLine[] result = null;
			YardPickupData.ResponseCode? error = null;

			var factory = new BusinessObjectFactory();
			var facility = FacilityMatchingHelper.GetFacilityFromCommunityCode(factory, communityCode, WarehouseTypes.Codes.ContainerYard)
				?? FacilityMatchingHelper.GetFacilityFromOrgCodeAndAddressCode(factory, orgCode, addressCode, WarehouseTypes.Codes.ContainerYard);

			if (facility == null)
			{
				error = YardPickupData.ResponseCode.RNF;
				return (Result: null, Error: error);
			}

			var query = new ZDBOnlyQuery(typeof(CYDReleaseAdviceLine));
			var releaseAdviceSubQuery = new ZDBOnlySubQuery(typeof(CYDReleaseAdvice), CYDReleaseAdviceLineSchema.YEL_YRE_ReleaseAdvice);
			_ = releaseAdviceSubQuery.AddToFilter(CYDReleaseAdviceSchema.YRE_ReleaseNumber, releaseNumber);
			_ = releaseAdviceSubQuery.AddToFilter(CYDReleaseAdviceSchema.YRE_WW_Yard, facility.PK);

			query.AddSubQuery(releaseAdviceSubQuery, JoinCondition.And);

			var releaseAdviceLines = factory.Load<CYDReleaseAdviceLine>(query)
				.Where(line => line.ReleaseAdvice.Client.Address is not null)
				.ToArray();

			if (releaseAdviceLines.Length == 0)
			{
				error = YardPickupData.ResponseCode.RNF;
			}
			else
			{
				var activeReleaseAdviceLines = releaseAdviceLines
					.Where(line => line.ReleaseAdvice.IsActive)
					.ToArray();

				if (activeReleaseAdviceLines.Length == 0)
				{
					error = YardPickupData.ResponseCode.RNE;
				}
				else if (activeReleaseAdviceLines.All(line => line.TotalAvailablePickupQuantity <= 0))
				{
					error = YardPickupData.ResponseCode.RNU;
				}
				else
				{
					result = activeReleaseAdviceLines;
				}
			}

			return (Result: result, Error: error);
		}

		List<YardPickupData.ReleaseDetail> CreateReleaseDetails(CYDReleaseAdviceLine[] lines)
		{
			var groupedLines = lines
				.GroupBy(line => line.ReleaseAdvice)
				.ToDictionary(
					group => group.Key,
					group => group.ToArray());

			var releaseDetails = groupedLines.Keys.Select(releaseAdvice =>
			{
				return new YardPickupData.ReleaseDetail
				{
					ClientCode = GetCommunityCodeFromAddress(releaseAdvice.Client),
					AvailableDateUtc = releaseAdvice.Yard.GetWarehouseBranchLocalDateTimeOffset(releaseAdvice.YRE_FromDate).ToUtcDateTime(),
					ExpiryDateUtc = releaseAdvice.Yard.GetWarehouseBranchLocalDateTimeOffset(releaseAdvice.YRE_ToDate).ToUtcDateTime(),
					Containers = groupedLines[releaseAdvice]
						.GroupBy(line => line.UnitLineItem.ContainerType)
						.Select(group =>
						{
							var minReadyDate = group.Min(line => line.YEL_ReadyDate);
							return new YardPickupData.ContainerDetails()
							{
								IsoCode = group.Key.RC_ISOType,
								Code = group.Key.RC_Code,
#pragma warning disable EDI007 // Disable the warning Customizable Data Translation Rule for RC_Description.
								Description = group.Key.RC_Description,
#pragma warning restore EDI007 // Enable Customizable Data Translation Rule
								ReadyDateUtc = releaseAdvice.Yard.GetWarehouseBranchLocalDateTimeOffset(minReadyDate).ToUtcDateTime(),
								TotalQuantity = group.Sum(x => x.UnitLineItem.YLI_Quantity),
								AvailableQuantity = group.Sum(x => x.TotalAvailablePickupQuantity)
							};
						})
						.ToList(),
					// TODO: to be determined
					VesselId = string.Empty,
					VesselName = string.Empty,
					VoyageNumber = string.Empty
				};
			});

			return releaseDetails.ToList();
		}

		ZString GetCommunityCodeFromAddress(JobDocAddress address)
		{
			var communityCode = address.Address.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ContainerChainCommunityCode, string.Empty);
			return communityCode.IsEmpty
				? address.Organisation.OH_Code
				: communityCode;
		}
	}
}
