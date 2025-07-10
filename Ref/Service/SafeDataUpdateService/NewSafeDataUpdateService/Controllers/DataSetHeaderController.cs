using System;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Common.Web;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Controllers
{
	public class DataSetHeaderController<T> : SafeDataController<T> where T : class
	{
		public DataSetHeaderController(IAuthorizationHelper userAuthorizationHelper) : base(userAuthorizationHelper)
		{
		}

		[HttpGet]
		public DateTime GetLastestCreatedTimeUTC()
		{
			return GetRefDbVersionControlWithFilter(v => v.RVC_ParentCode == TablePrefix)
				.Select(x => x.RVC_CreatedTimeUTC).Max();
		}

		[HttpGet]
		public DateTime GetLatestUpdatedTimeUTC()
		{
			return GetRefDbVersionControlWithFilter(x => x.RVC_ParentCode == TablePrefix && x.RVC_IsPublished)
				.Select(x => x.RVC_LastUpdatedUTC).Max() ?? DateTime.MinValue;
		}

		[HttpGet]
		[ODataEnableQuery]
		public IQueryable<T> GetCreatedBetween([FromODataUri] DateTime? afterCreatedTimeUTC, [FromODataUri] DateTime beforeOrEqualCreatedTimeUTC)
		{
			var pkExp = ExpressionHelper.GetPKExpression<T>();
			return base.GetCore().Join(GetRefDbVersionControlWithFilter(v => (!afterCreatedTimeUTC.HasValue || afterCreatedTimeUTC < v.RVC_CreatedTimeUTC) && v.RVC_CreatedTimeUTC <= beforeOrEqualCreatedTimeUTC), pkExp,
						v => v.RVC_ParentPK, (r, _) => r);
		}

		[HttpGet]
		[ODataEnableQuery]
		public IQueryable<T> GetModifiedBetween([FromODataUri] DateTime? afterModifiedTimeUTC, [FromODataUri] DateTime beforeOrEqualModifiedTimeUTC)
		{
			var pkExp = ExpressionHelper.GetPKExpression<T>();
			return base.GetCore().Join(GetRefDbVersionControlWithFilter(v => (!afterModifiedTimeUTC.HasValue || afterModifiedTimeUTC < v.RVC_LastUpdatedUTC) && v.RVC_LastUpdatedUTC <= beforeOrEqualModifiedTimeUTC), pkExp,
						v => v.RVC_ParentPK, (r, _) => r);
		}

		IQueryable<RefDbVersionControl> GetRefDbVersionControlWithFilter(Expression<Func<RefDbVersionControl, bool>> refDbVersionControlFunc)
		{
			var shouldGetDeleted = typeof(T).Name.EndsWith("UserView", StringComparison.OrdinalIgnoreCase);
			var result = Repository.Get<RefDbVersionControl>()?.Where(v => !v.RVC_Deleted || shouldGetDeleted);
			if (refDbVersionControlFunc != null)
			{
				result = result.Where(refDbVersionControlFunc);
			}
			return result;
		}

		protected override IQueryable<T> GetCore()
		{
			var result = base.GetCore();
			var pkExp = ExpressionHelper.GetPKExpression<T>();
			return result.Join(GetRefDbVersionControlWithFilter(null), pkExp, v => v.RVC_ParentPK, (r, _) => r);
		}

		string TablePrefix
		{
			get
			{
				if (fTablePrefix == null)
				{
					fTablePrefix = typeof(T).GetTablePrefix();
				}
				return fTablePrefix;
			}
		}
		string fTablePrefix;

		protected override bool IsDataHeader => true;
	}
}
