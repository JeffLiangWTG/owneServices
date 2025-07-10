using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class RefCusProcedureLoader
	{
		public RefCusProcedureLoader(IRefDataLoader refDataLoader)
		{
			RefDataLoader = refDataLoader;
		}

		internal readonly IRefDataLoader RefDataLoader;

		protected IRefDataLoader GetRefDataLoader => RefDataLoader;

		public Task<IEnumerable<RefCusProcedure>> GetRefCusProcedure()
		{
			return Task.Factory.StartNew(() => RefDataLoader.LoadData<RefCusProcedure>(CreateFilterQuery()).Result);
		}

		protected static string CreateFilterQuery()
		{
			return "RefCusProcedureUpdate?$filter=ZZ6_ZZZ_NKDataGrouping eq 'FR'";
		}
	}
}
