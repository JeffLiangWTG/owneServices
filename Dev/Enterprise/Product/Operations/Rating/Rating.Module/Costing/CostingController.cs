using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Module
{
	public class CostingController : RatingController<Costing>
	{
		#region Standard Controller Overrides

		public override ControllerID ID => ControllerIDs.Costing;

		public override ModuleIdentifier ModuleID => ModuleIDs.Costing;

		protected override IZForm GetForm(IBusiness businessEntity) =>
			new CostingForm((Costing)businessEntity);

		#endregion

#if DEBUG
		public IBusiness LoadBusinessEntity_ForTest(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return LoadBusinessEntity(factory, sourceEntityPK);
		}
#endif

		/// <summary>
		/// Called when handling a URL of the costing type, for View, Edit, etc or other modalities.
		/// When this is called, the value this controller returns here, is re-used when the form is shown.
		/// </summary>
		/// <param name="sourceEntityPK">The PK of a RatingHeader or RatingContract or RatingContractAllocation</param>
		/// <returns>Regardless of the object represent by the sourceEntityPK, it returns a Costing or null</returns>
		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var costing = base.LoadBusinessEntity(factory, sourceEntityPK);
			if (costing != null)
			{
				return costing;
			}

			var ratingContractAllocation = Factory.Load<IRatingContractAllocationLine>(sourceEntityPK);
			var ratingContractPK = ratingContractAllocation?.RCA_RCT_RatingContract ?? sourceEntityPK;

			var ratingContract = Factory.Load<IRatingContract>(ratingContractPK);
			if (ratingContract == null)
			{
				return null;
			}

			var organisationPK = ratingContract.RCT_OH;
			costing = Factory.LoadTop1<Costing>(new ZQuery(RatingHeaderSchema.TH_OH, organisationPK));
			if (costing == null)
			{
				return null;
			}

			var containerCode = string.Empty;
			var containerPK = ratingContractAllocation?.RCA_RC_ContainerType;
			if (containerPK.HasValue)
			{
				containerCode = Factory.Load<RefContainer>(containerPK.Value)?.RC_Code;
			}

			RateEntryFilterValueCache.Instance[((Costing)costing).PK] = new RateEntryFilterValue()
			{
				StartDate = ratingContractAllocation?.RCA_StartDate ?? ratingContract.RCT_StartDate,
				ExpiryDate = ratingContractAllocation?.RCA_ExpiryDate ?? ratingContract.RCT_EndDate,

				Destination = ratingContractAllocation?.RCA_Calc_DischargeLocation ?? string.Empty,
				Origin = ratingContractAllocation?.RCA_Calc_LoadLocation ?? string.Empty,
				ContainerCode = containerCode,

				ContractNumber = ratingContract.RCT_ContractNumber,
				TransportMode = ratingContract.RCT_TransportMode,
				AllowHazardousCommodity = ratingContract.RCT_AllowHazardousCommodities,
				ContainerType = ratingContract.RCT_ContainerType
			};

			return costing;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CostingRatesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CostingRatesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CostingRatesEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CostingRatesDelete;

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject) =>
			IsGlobal(bizObject)
				? Env.Security.GlobalCostingRatesView
				: base.GetCheckPointForView(bizObject);

		public override SecurityCheckpoint GetCheckPointForNew(BusinessObject bizObject) =>
			IsGlobal(bizObject)
				? Env.Security.GlobalCostingRatesNew
				: base.GetCheckPointForNew(bizObject);

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject) =>
			IsGlobal(bizObject)
				? Env.Security.GlobalCostingRatesEdit
				: base.GetCheckPointForEdit(bizObject);

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject) =>
			IsGlobal(bizObject)
				? Env.Security.GlobalCostingRatesDelete
				: base.GetCheckPointForDelete(bizObject);

		protected override CRMSecurityProvider<Costing> SecurityProvider =>
			securityProvider ?? (securityProvider = new CostingCRMSecurityProvider());
		CostingCRMSecurityProvider securityProvider;

		#endregion
	}
}

