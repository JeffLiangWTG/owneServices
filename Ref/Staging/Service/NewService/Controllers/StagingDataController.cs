using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace CargoWise.RefDbRepo.Staging.NewService.Controllers
{
	public class StagingDataController<T> : ODataController where T : class
	{
		public StagingDataController(IAuthorizationHelper authorizationHelper)
		{
			this.authorizationHelper = authorizationHelper;
		}

		readonly IAuthorizationHelper authorizationHelper;

		protected IStagingRepository Staging
		{
			get
			{
				return fStaging ?? (fStaging = HttpContext.GetContext());
			}
		}
		IStagingRepository fStaging;

		[ODataEnableQuery(MaxExpansionDepth = 10)]
		public virtual IQueryable<T> Get()
		{
			return GetDataCore();
		}

		[ODataEnableQuery(MaxExpansionDepth = 10)]
		public virtual IQueryable<T> Get([FromODataUri] Guid key)
		{
			return GetDataCore(key);
		}

		protected PropertyInfo PKColumn
		{
			get
			{
				if (fPKColumn == null)
				{
					fPKColumn = typeof(T).GetProperties().FirstOrDefault(x => x.Name.EndsWith("_pk", StringComparison.OrdinalIgnoreCase) || x.Name.Equals("id", StringComparison.OrdinalIgnoreCase));
				}
				return fPKColumn;
			}
		}
		PropertyInfo fPKColumn;

		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public virtual IActionResult Post([FromBody] T data)
		{
			if (!authorizationHelper.IsAuthorized(data, HttpContext.GetUserId()))
			{
				return Unauthorized();
			}

			var pk = PKColumn.GetValue(data);
			if ((Guid)pk == Guid.Empty)
			{
				PKColumn.SetValue(data, Guid.NewGuid());
			}
			Staging.Add(data);
			Staging.SaveChanges();
			return Created<T>(data);
		}

		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public virtual IActionResult Put([FromODataUri] Guid key, [FromBody] T data)
		{
			if (!authorizationHelper.IsAuthorized(data, HttpContext.GetUserId()))
			{
				return Unauthorized();
			}

			Staging.Update(data);
			Staging.SaveChanges();
			return Updated(data);
		}

		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public virtual IActionResult Patch([FromODataUri] Guid key, [FromBody] T data)
		{
			if (!authorizationHelper.IsAuthorized(data, HttpContext.GetUserId()))
			{
				return Unauthorized();
			}

			Staging.Update(data);
			Staging.SaveChanges();
			return Updated(data);
		}

		[Authorize(AuthenticationSchemes = AuthType.TokenAuth)]
		public virtual IActionResult Delete([FromODataUri] Guid key)
		{
			var data = GetDataCore(key).FirstOrDefault();
			if (data == null)
			{
				return NotFound();
			}
			if (!authorizationHelper.IsAuthorized(data, HttpContext.GetUserId()))
			{
				return Unauthorized();
			}

			Staging.Remove(data);
			Staging.SaveChanges();
			return NoContent();
		}

		IQueryable<T> GetDataCore() => Staging.Get<T>();

		IQueryable<T> GetDataCore(Guid key)
		{
			var param = Expression.Parameter(typeof(T));
			var propertyExp = Expression.Property(param, PKColumn);
			var equalExp = Expression.Equal(propertyExp, Expression.Constant(key, typeof(Guid)));
			var whereExp = Expression.Lambda<Func<T, bool>>(equalExp, param);
			return GetDataCore().Where(whereExp);
		}
	}
}
