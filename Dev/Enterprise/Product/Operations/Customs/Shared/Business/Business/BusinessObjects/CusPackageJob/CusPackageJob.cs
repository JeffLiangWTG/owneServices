using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackageJob : PkgPackageJob, Integration.Customs.ICusPackageJob
	{
		public CusPackageJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		public new CusPackageCollection Packages => (CusPackageCollection)base.Packages;

		protected override PkgPackageCollection GetPackageCollectionCore() => new CusPackageCollection(this);

		public CusPackingList PackingList => Factory.Load<CusPackingList>(KJ_ParentID);

		protected override ZArchitecture.Environment.INumberFountainProxy NumberFountainForJobID => Env.NumberFountains.PackingListID;

		public static new readonly CusPackageJobTypeDecider TypeDecider = new CusPackageJobTypeDecider();

		protected override bool SupportsCloneCore() => true;

		public override bool IsSavedByFactory => base.IsSavedByFactory || Packages.Any(x => x.IsSavedByFactory);
	}
}
