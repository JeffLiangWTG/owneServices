using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/Duplication")]
	public class DuplicationController : ApiController
	{
		public DuplicationController()
		{
		}

		[Route("findOrgDuplicates")]
		[HttpPost]
		public IHttpActionResult FindOrgDuplicates([FromBody] OrgDuplicationRequest fields)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();

				var deduplicationOrgHeader = new DeduplicationOrgHeader(fields.Name ?? string.Empty);
				deduplicationOrgHeader.OH_PK = Guid.NewGuid();
				deduplicationOrgHeader.UNLOCO = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, fields.UNLOCO);

				var address = new DeduplicationOrgAddress();
				address.OA_PK = Guid.NewGuid();
				address.OA_PostCode = fields.Postcode ?? string.Empty;
				address.OA_Address1 = fields.Address1 ?? string.Empty;
				address.OA_Address2 = fields.Address2 ?? string.Empty;
				address.OA_RN_NKCountryCode = fields.CountryRegion ?? string.Empty;
				address.OA_City = fields.City ?? string.Empty;
				address.OA_PostCode = fields.Postcode ?? string.Empty;
				address.OA_State = fields.State ?? string.Empty;
				address.OA_Phone = fields.Phone ?? string.Empty;
				address.OA_Mobile = fields.Mobile ?? string.Empty;
				address.OA_Email = fields.Email ?? string.Empty;
				address.OA_Fax = fields.Fax ?? string.Empty;
				address.OA_ValidationStatus = fields.ValidationStatus ?? string.Empty;
				deduplicationOrgHeader.OrgAddresses = new DeduplicationOrgAddress[] { address };

				if (!string.IsNullOrEmpty(fields.WebsiteURL))
				{
					var url = new DeduplicationOrgWebURL(fields.WebsiteURL);
					url.PU_PK = Guid.NewGuid();
					deduplicationOrgHeader.OrgWebURLs = new DeduplicationOrgWebURL[] { url };
				}

				var duplicationFinder = new OrgHeaderDuplicationFinder(deduplicationOrgHeader, false, false);
				var scoringResults = duplicationFinder.FindPotentialDuplicates(false);
				var results = new List<DuplicationResponse>();
				scoringResults.ForEach(r =>
				{
					results.Add(BuildDuplicationResponse(r));
				});
				return Json(results);
			}
		}

		static string GetCountryRegionCodeAndDescription(OrgAddress address)
		{
			if (address?.Country != null)
			{
				return string.Format($"{address.Country.Code} - {address.Country.Description}");
			}
			return string.Empty;
		}

		DuplicationResponse BuildDuplicationResponse(ScoringResult result)
		{
			OrgBrandOrRelatedName matchingBrand = null;
			OrgAddress matchingAddress = null;

			var brandNameChildrenResults = result.ChildResults.Where(r => r.MultiSourceTargetType == typeof(IOrgBrandOrRelatedName));
			var highestBrandNameChildrenResult = brandNameChildrenResults.OrderByDescending(r => r.Score).FirstOrDefault();
			if (highestBrandNameChildrenResult != null)
			{
				var factory = new BusinessObjectFactory();
				matchingBrand = factory.Load<OrgBrandOrRelatedName>(highestBrandNameChildrenResult.TargetPK);
			}

			var addressChildrenResults = result.ChildResults.Where(r => r.TargetType == typeof(IOrgAddress));
			var highestAddressChildrenResult = addressChildrenResults.OrderByDescending(r => r.Score).FirstOrDefault();
			if (highestAddressChildrenResult != null)
			{
				var factory = new BusinessObjectFactory();
				matchingAddress = factory.Load<OrgAddress>(highestAddressChildrenResult.TargetPK);
			}

			return new DuplicationResponse()
			{
				PK = result.TargetPK,
				Score = result.Score,
				MatchingBrandName = matchingBrand?.P1_RelatedName,
				MatchingAddress1 = matchingAddress?.Address1,
				MatchingAddress2 = matchingAddress?.Address2,
				MatchingCity = matchingAddress?.City,
				MatchingState = matchingAddress?.StateCode,
				MatchingCountryRegion = GetCountryRegionCodeAndDescription(matchingAddress),
			};
		}
	}
}
