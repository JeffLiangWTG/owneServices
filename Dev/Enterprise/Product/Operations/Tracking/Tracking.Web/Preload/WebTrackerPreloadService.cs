using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Compilation;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web
{
	// This class uses the compiler and demands file write access so can not be easily unit tested.
	public class WebTrackerPreloadService : IWebTrackerPreloadService
	{
		public void PreloadModule(HttpContext context, string modulePath)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (string.IsNullOrEmpty(modulePath))
			{
				throw new ArgumentException($"{nameof(modulePath)} cannot be null or empty", nameof(modulePath));
			}

			if (context.Server != null)
			{
				context.Server.Execute(modulePath);
			}
		}

		public void PreloadWebResources(params ZWebResource[] webResources)
		{
			if (webResources == null)
			{
				throw new ArgumentNullException(nameof(webResources));
			}

			var fileNamesToCompile = new List<string>();

			foreach (var webResource in webResources)
			{
				try
				{
					webResource.Extract();
					fileNamesToCompile.Add(webResource.FileName);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}

			foreach (var fileName in fileNamesToCompile)
			{
				try
				{
					BuildManager.GetCompiledType(fileName);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}
	}
}
