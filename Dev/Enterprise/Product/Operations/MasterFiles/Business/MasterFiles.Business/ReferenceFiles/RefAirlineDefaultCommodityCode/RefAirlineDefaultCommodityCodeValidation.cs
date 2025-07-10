using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineDefaultCommodityCodeValidation : AutoRefAirlineDefaultCommodityCodeValidation
	{
		public RefAirlineDefaultCommodityCodeValidation(AutoRefAirlineDefaultCommodityCode parent) : base(parent)
		{
		}

		protected override void CheckRDC_RAR_NKProductCode()
		{
			base.CheckRDC_RAR_NKProductCode();

			MandatoryValidation.CheckEntered(Parent.RDC_RAR_NKProductCodeInfo);
			if (!Parent.RDC_RAR_NKProductCode.IsEmpty)
			{
				CheckUniqueness(Parent.RDC_RAR_NKProductCodeInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.RDC_RAR_NKProductCodeInfo, Parent.Lookups.RDC_RAR_NKProductCode_List);
		}

		protected override void CheckRDC_RAC_NKCommodityCode()
		{
			base.CheckRDC_RAC_NKCommodityCode();

			MandatoryValidation.CheckEntered(Parent.RDC_RAC_NKCommodityCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RDC_RAC_NKCommodityCodeInfo, Parent.Lookups.RDC_RAC_NKCommodityCode_List, (NoResString)"The Commodity Code must be valid for the chosen Product.");
		}

		protected override void CheckRDC_RL_NKOrigin()
		{
			base.CheckRDC_RL_NKOrigin();
			if (!Parent.RDC_RAR_NKProductCode.IsEmpty)
			{
				CheckUniqueness(Parent.RDC_RL_NKOriginInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.RDC_RL_NKOriginInfo, Parent.Lookups.Locations);
		}

		protected override void CheckRDC_RL_NKDestination()
		{
			base.CheckRDC_RL_NKDestination();

			if (!Parent.RDC_RAR_NKProductCode.IsEmpty)
			{
				CheckUniqueness(Parent.RDC_RL_NKDestinationInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.RDC_RL_NKDestinationInfo, Parent.Lookups.Locations);
		}

		void CheckUniqueness(ZPropertyInfo property)
		{
			var query = new ZQuery();
			query.AddToFilter(RefAirlineDefaultCommodityCodeSchema.RDC_RM, Parent.RDC_RM);
			query.AddToFilter(RefAirlineDefaultCommodityCodeSchema.RDC_RL_NKOrigin, Parent.RDC_RL_NKOrigin);
			query.AddToFilter(RefAirlineDefaultCommodityCodeSchema.RDC_RL_NKDestination, Parent.RDC_RL_NKDestination);
			query.AddToFilter(RefAirlineDefaultCommodityCodeSchema.RDC_RAR_NKProductCode, Parent.RDC_RAR_NKProductCode);
			query.AddToFilter(RefAirlineDefaultCommodityCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			var match = Parent.Factory.LoadTop1<RefAirlineDefaultCommodityCode>(query);
			if (match != null)
			{
				property.AddError(Res.GetString("50E5D80C-76BB-446C-8A36-D4C66363BB00", "The same Product cannot be duplicated for the same Origin/Destination."));
			}
		}
	}
}
