using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.AspNetCore.Mvc;

namespace CargoWise.RefDbRepo.Staging.NewService.Controllers
{
	[ApiController]
	[Route("api/[controller]/[action]")]
	public class EntityMatcherController : ControllerBase
	{
		public EntityMatcherController(IEntityMatcher matcher)
		{
			Argument.NotNull(matcher, nameof(matcher));
			this.matcher = matcher;
		}

		readonly IEntityMatcher matcher;

		[HttpGet]
		public IEnumerable<string> GetCodes([FromQuery] string entityClass, [FromQuery] string[] names, [FromQuery] bool useRecog = true, [FromQuery] string language = "EN")
		{
			var entityClassEnum = EntityClass.COUNTRY;
			if (!string.IsNullOrEmpty(entityClass) && Enum.TryParse(entityClass.ToUpperInvariant(), out entityClassEnum))
			{
				var ignoreCase = entityClassEnum == EntityClass.COUNTRY || entityClassEnum == EntityClass.CURRENCY || entityClassEnum == EntityClass.CACUSTOMUOM;
				return matcher.GetBestMatchingCodes(entityClassEnum, language, useRecog, ignoreCase, names).Select(x => x.Result);
			}
			else
			{
				throw new NotSupportedException($"Entity Class {entityClass} is not supported.");
			}
		}
	}
}
