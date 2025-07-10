using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/screening")]
	public class ScreeningController : ApiController
	{
		[Route("getscreeningstatus")]
		[HttpPost]
		public IHttpActionResult GetScreeningStatus([FromBody] ScreeningRequest fields)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				try
				{
					var names = new List<DpsNameCandidate>();
					var addresses = new List<DpsAddressCandidate>();
					var countries = new List<string>();
					var dpsCandidateCreator = new DpsCandidateCreator();

					if (!string.IsNullOrWhiteSpace(fields.Name))
					{
						dpsCandidateCreator.NewSeparatedOrganizationNames(names, fields.Name, DeniedPartyConstants.ScreeningNameTypes.Organization);
					}
					if (!string.IsNullOrWhiteSpace(fields.Address1))
					{
						addresses.Add(dpsCandidateCreator.NewAddressCandidate(fields.Address1, fields.Address2, fields.City, fields.State, fields.Postcode, fields.CountryRegion, ""));
					}
					if (!string.IsNullOrWhiteSpace(fields.CountryRegion))
					{
						countries.Add(fields.CountryRegion);
					}
					if (names.Count + addresses.Count + countries.Count > 0)
					{
						var request = dpsCandidateCreator.GetDpsRequestHeader(names, addresses, new List<DpsRegistrationCodeCandidate>(), countries);
						var dpsManager = ObjectFactory.Get<IDpsManager>();
						var services = dpsManager.GetDpsServices(null);

						var result = Task.Run(async () =>
						{
							using (Db.DisposableActionForDbConnection())
							{
								var dpsResponse = await dpsManager.GetScreenResult(request, services);
								if (dpsResponse.ResponseCode != DpsResponseCode.Successful)
								{
									return BuildScreeningResponse(false, dpsResponse.ResponseCode, dpsResponse.ExtraMessage);
								}
								else
								{
									var factory = new ReadOnlyBusinessObjectFactory() { NameForDebugging = nameof(ScreeningController) };
									var parentOrg = factory.New<OrgHeader>();
									parentOrg.OH_Code = string.Empty;
									var orgHeader = factory.New<OrgHeader>();
									var screeningParty = new ScreeningParty(parentOrg, Res.GetString("8d3919a4-970b-419c-b46a-bd77939d114a", "Stand Alone Screening"), orgHeader);
									var dpsResponseWithScreeningParty = new DpsResponseWithScreeningParty(screeningParty, dpsResponse, request);
									var resultViewModel = new DpsResultWinModel(new DpsResultModel(new List<DpsResponseWithScreeningParty> { dpsResponseWithScreeningParty }, factory, false), true) as IDpsResult;
									if (resultViewModel.AllPartiesClear)
									{
										return BuildScreeningResponse(false, DpsResponseCode.Successful);
									}
									else
									{
										return BuildScreeningResponse(true, DpsResponseCode.Successful, Res.GetString("c2c104bd-123c-4bd4-8e3d-b2cf88c6a14a", "Screened as risky"));
									}
								}
							}
						}).GetAwaiter().GetResult();
						return Json(result);
					}
					else
					{
						return Json(BuildScreeningResponse(false, DpsResponseCode.Failed, Res.GetString("f9632865-7900-49c5-b3ab-0b439a7e9e07", "Need more information for screening!")));
					}
				}
				catch (Exception ex)
				{
					return Json(BuildScreeningResponse(false, DpsResponseCode.Exception, ex.Message));
				}
			}
		}

		ScreeningResponse BuildScreeningResponse(bool hasRisk, DpsResponseCode responseCode, string message = "")
		{
			return new ScreeningResponse()
			{
				HasRisk = hasRisk,
				ResponseCode = responseCode,
				ExtraMessage = message
			};
		}
	}
}
