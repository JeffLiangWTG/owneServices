using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class RecursiveDeletion(SafeDbContext entities)
	{
		readonly SafeDbContext _entities = entities;

		public async Task RecursiveDeleteAsync<TE>(IQueryable<TE> data) where TE : class
		{
			foreach (var relatedProperty in typeof(TE).GetCollectionNavigationPropertyInfos())
			{
				var relatedType = relatedProperty.PropertyType.GetElementTypeOfCollection();
				//calls DbContext.Set<T> method.
				var methodSet = typeof(DbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(m => m.Name == nameof(DbContext.Set) && !m.GetParameters().Any());
				var methodGeneric = methodSet.MakeGenericMethod(relatedType);
				var relatedData = methodGeneric.Invoke(_entities, null) as IQueryable;

				//calls AsNoTracking<T> method
				var asNoTrackingMethod = typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking), BindingFlags.Static | BindingFlags.Public);
				var asNoTrackingGeneric = asNoTrackingMethod.MakeGenericMethod(relatedType);
				relatedData = asNoTrackingGeneric.Invoke(null, new[] { relatedData }) as IQueryable;

				var filteredData = typeof(FilterDataHelper).InvokeStaticGenericMethod(nameof(FilterDataHelper.FilterWithParentData), new[] { relatedType, typeof(TE) }, new[] { relatedData, data });
				await (this.InvokeGenericMethod(nameof(RecursiveDeleteAsync), relatedType, [filteredData]) as Task);
			}
			await data.DeleteAsync();
		}
	}
}
