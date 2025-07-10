using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.NewService.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class DbUpgraderController : ControllerBase
	{
		readonly IDbUpgraderHelper helper;
		public DbUpgraderController(IDbUpgraderHelper helper)
		{
			Argument.NotNull(helper, nameof(helper));
			this.helper = helper;
		}

		[HttpGet]
		public IEnumerable<UpgradeWrapper> GetDeltaScriptsAfterVersion([FromQuery] string version)
		{
			var versionNumber = Convert.ToInt32(version, CultureInfo.InvariantCulture);
			if (versionNumber < 0 || versionNumber > helper.LatestVersion)
			{
				throw new ArgumentOutOfRangeException($"Delta Script Version {version} is not applicable");
			}
			else
			{
				return helper.GetDeltaScriptsAfterVersion(versionNumber);
			}
		}

		[HttpGet]
#pragma warning disable CA1024 // Use properties where appropriate
		public int GetLatestVersion()
#pragma warning restore CA1024 // Use properties where appropriate
		{
			return helper.LatestVersion;
		}

		[HttpGet]
#pragma warning disable CA1024 // Use properties where appropriate
		public int GetRequiredVersion()
#pragma warning restore CA1024 // Use properties where appropriate
		{
			return helper.RequiredVersion;
		}
	}
}
