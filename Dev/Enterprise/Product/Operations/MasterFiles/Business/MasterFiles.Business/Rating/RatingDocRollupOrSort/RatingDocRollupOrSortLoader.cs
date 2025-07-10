using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.DocRollupOrSort;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Rating
{
	public sealed class RatingDocRollupOrSortLoader : BaseDocRollupOrSortLoader<RatingDocumentsChargeGroupingOrRollup>
	{
		public RatingDocRollupOrSortLoader(BusinessObjectFactory factory, OrgHeader orgHeader, GlbBranch branch, GlbDepartment department)
			: base(factory, orgHeader, branch, department)
		{
		}

		public ZString GetDisplay(ZString serviceDirection, ZString transportMode, ZString containerMode, params ZString[] jobTypes)
			=> Get
			(
				orgField: RatingDocumentsChargeGroupingOrRollupSchema.RCG_Display,
				registryField: RatingDocRollupOrGroupRegistry.Schema.Display,
				defaultSetting: RatingDocumentsChargeGroupingOrRollup.DisplayDefaultCode,
				serviceDirection,
				transportMode,
				containerMode,
				jobTypes
			);

		public ZString GetStyle(ZString serviceDirection, ZString transportMode, ZString containerMode, params ZString[] jobTypes)
			=> Get
			(
				orgField: RatingDocumentsChargeGroupingOrRollupSchema.RCG_Style,
				registryField: RatingDocRollupOrGroupRegistry.Schema.Style,
				defaultSetting: RatingDocumentsChargeGroupingOrRollup.StyleDefaultCode,
				serviceDirection,
				transportMode,
				containerMode,
				jobTypes
			);

		#region Implementation

		protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RatingDocumentsChargeGroupingOrRollup);

		protected override SchemaColumn JobTypeSchemaColumn => RatingDocumentsChargeGroupingOrRollupSchema.RCG_JobType;

		protected override SchemaColumn ServiceDirectionSchemaColumn => null;

		protected override SchemaColumn TransportModeSchemaColumn => RatingDocumentsChargeGroupingOrRollupSchema.RCG_TransportMode;

		protected override SchemaColumn CompanyDataSchemaColumn => RatingDocumentsChargeGroupingOrRollupSchema.RCG_OB_CompanyData;

		protected override RegistryBusinessObjectCollectionTemplate GetRegistrySettings(Guid companyPK, Guid branchPK, Guid departmentPK)
			=> OrganisationRegistry.Instance.RatingDocRollupOrGroup.GetFallBackValueAtAllLevels
			(
				companyPK,
				branchPK,
				departmentPK
			);

		#endregion
	}
}
