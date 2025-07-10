#if NETFRAMEWORK
using System.Web.Http;
using IActionResult = System.Web.Http.IHttpActionResult;
using Route = System.Web.Http.RouteAttribute;
#endif
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost.NetCore
{
#if NET
	[Route("api/Customs/CustomsLookup")]
#elif NETFRAMEWORK
	[RoutePrefix("api/Customs/CustomsLookup")]
#endif
	[GlowTicketAuthentication]

	public class CustomsLookupController : ControllerBase
	{
		/// <summary>
		/// This function is not currently in use, it will be used in the phase 2 of the Contain Mode Lookup development
		/// </summary>
		/// <param name="countryCode"></param>
		/// <returns></returns>
		[Route("ContainerModeByCountry")]
		[HttpGet]
		public IActionResult GetCustomsContainerModesByCurrentyCountry()
		{
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var jobDeclaration = factory.New<BaseJobDeclaration>();
				var containerModes = jobDeclaration.Lookups.CargoIdTypeList;
				return Ok(containerModes);
			}
		}

		[Route("ContainerMode")]
		[HttpGet]
		public IActionResult GetCustomsFullContainerModes()
		{
			var containerModes = new CodeDescriptionPairList();

			var type = typeof(ContainerModes);

			//Get all the contant values defined in Core.Constants.ContainerModes
			foreach (var code in from fi in type.GetFields(BindingFlags.Static | BindingFlags.Public)
				   where fi.IsLiteral && !fi.IsInitOnly
				   select (string)fi.GetValue(null))
			{
				if (code != "ALL")
				{
					var description = Core.Constants.ContainerModeDescriptions.GetDescription(code);
					var pair = new CodeDescriptionPair(code, description);
					containerModes.Add(pair);
				}
			}
			containerModes.Sort();

			return Ok(containerModes);
		}
	}
}
