//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageLookups
//
//    This class should be used for overriding collections in AutoPkgPackageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageLookups : AutoPkgPackageLookups
	{
		public PkgPackageLookups(AutoPkgPackage parent)
			: base(parent)
		{
		}

		new PkgPackage Parent
		{
			get { return (PkgPackage)base.Parent; }
		}

		#region WeightUQs

		public CodeDescriptionPairList WeightUQs
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region VolumeUQs

		public CodeDescriptionPairList VolumeUQs
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region DimensionUQs

		public CodeDescriptionPairList DimensionUQs
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length); }
		}

		#endregion

		#region PackTypes

		public override RefPackTypeCollection PackTypes
		{
			get { return GetPackTypes(Factory, Parent.KP_KJ_ParentPackageJob); }
		}

		public static RefPackTypeCollection GetPackTypes(BusinessObjectFactory factory, ZGuid packageJobPK)
		{
			return factory.GetCachedValue(GetPackTypesKey(packageJobPK), () => GetPackTypesCore(factory, factory.Load<PkgPackageJob>(packageJobPK)));
		}

		public static RefPackTypeCollection GetPackTypes(BusinessObjectFactory factory, PkgPackageJob packageJob)
		{
			return factory.GetCachedValue(GetPackTypesKey(packageJob?.PK ?? ZGuid.Empty), () => GetPackTypesCore(factory, packageJob));
		}

		static string GetPackTypesKey(ZGuid packageJobPK) => "PkgPackageLookups|PackTypes|" + packageJobPK;

		static RefPackTypeCollection GetPackTypesCore(BusinessObjectFactory factory, PkgPackageJob packageJob)
		{
			var result = new RefPackTypeCollection(factory, false);
			if (packageJob != null && packageJob.ParentJob is IPackingParentCustomPackTypes parentJob && parentJob.PackTypesToExclude.Any())
			{
				result.AdditionalFilter = new ZQuery(RefPackTypeSchema.F3_Code, SQLComparisonOperator.NotEqual, parentJob.PackTypesToExclude);
			}

			return result;
		}

		#endregion

		#region TemperatureUnits

		public CodeDescriptionPairList TemperatureUnits => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.TemperatureTypes);

		#endregion

		#region Printers

		public IBusinessObjectCollection Printers => WhsCommonLookups.GetPrintersList(Factory);

		#endregion

		#region DamagedReasons

		public CodeDescriptionPairList DamagedReasons => PackingRegistry.Instance.DamagedReasons.Value.GetCodeDescriptionPairList();

		#endregion
	}
}

