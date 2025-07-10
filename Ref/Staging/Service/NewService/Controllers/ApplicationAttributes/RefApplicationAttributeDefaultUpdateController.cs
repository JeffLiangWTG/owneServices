using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace CargoWise.RefDbRepo.Staging.NewService.Controllers
{
	public class RefApplicationAttributeDefaultUpdateController : ODataController
	{
		[ODataEnableQuery]
		[InternalDataSetActionFilter]
		public IEnumerable<RefApplicationAttribute> Get()
		{
			var rootFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			try
			{
				var file = Path.Combine(rootFolder, XmlFileName);
				return xmlToRefApplicationAttribute.GetApplicationAttributes(file);
			}
			catch (Exception ex)
			{
				var message = $"Failed to get application attributes. Root folder: {rootFolder}.";
				throw new InvalidOperationException(message, ex);
			}
		}

		public static string ControllerName => "RefApplicationAttributeDefault";
		readonly string XmlFileName = "ApplicationPathsAndDefaultConfigurations.xml";
		static readonly XmlToRefApplicationAttribute xmlToRefApplicationAttribute = new XmlToRefApplicationAttribute();
	}
}
