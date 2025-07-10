//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbPortDeliveryTimeValidation
//
//    This class should be used for overriding validation in AutoGlbPortDeliveryTimeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPortDeliveryTimeValidation : AutoGlbPortDeliveryTimeValidation
	{
		public GlbPortDeliveryTimeValidation(AutoGlbPortDeliveryTime parent) : base(parent)
		{
		}

		protected override void CheckG1_DaysDelayFromArrivalToDeliver()
		{
			base.CheckG1_DaysDelayFromArrivalToDeliver();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.G1_DaysDelayFromArrivalToDeliverInfo, 0);
		}

		protected override void CheckG1_GC_Company()
		{
			base.CheckG1_GC_Company();
			ListValidation.ErrorIfInvalidPK(Parent.G1_GC_CompanyInfo);
		}

		protected override void CheckG1_RL_NKDestinationPort()
		{
			base.CheckG1_RL_NKDestinationPort();
			MandatoryValidation.CheckEntered(Parent.G1_RL_NKDestinationPortInfo);
			ListValidation.ErrorIfInvalidCode(Parent.G1_RL_NKDestinationPortInfo);
			CheckPortDeliveryTimeExists(Parent.G1_RL_NKDestinationPortInfo);
		}

		protected override void CheckG1_RL_NKDischargePort()
		{
			base.CheckG1_RL_NKDischargePort();
			MandatoryValidation.CheckEntered(Parent.G1_RL_NKDischargePortInfo);
			ListValidation.ErrorIfInvalidCode(Parent.G1_RL_NKDischargePortInfo);
			CheckPortDeliveryTimeExists(Parent.G1_RL_NKDischargePortInfo);
		}

		protected override void CheckG1_OH_ClientOverride()
		{
			base.CheckG1_OH_ClientOverride();
			CheckPortDeliveryTimeExists(Parent.G1_OH_ClientOverrideInfo);
		}

		protected override void CheckG1_FreightMode()
		{
			base.CheckG1_FreightMode();
			MandatoryValidation.CheckEntered(Parent.G1_FreightModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.G1_FreightModeInfo);
			CheckPortDeliveryTimeExists(Parent.G1_FreightModeInfo);
		}

		protected override void CheckG1_JobMode()
		{
			base.CheckG1_JobMode();
			CheckPortDeliveryTimeExists(Parent.G1_JobModeInfo);
		}

		void CheckPortDeliveryTimeExists(ZPropertyInfo propertyInfo)
		{
			if (IsGlbPortDeliveryTimeExists)
			{
				propertyInfo.AddError(Res.GetString("C2708972-80B2-4E41-A490-67B29A3EB883", "A Port Delivery Time already exists with the same Freight Mode, Job Mode, Discharge Port, Destination Port and Client."));
			}
		}

		bool IsGlbPortDeliveryTimeExists
		{
			get
			{
				var query = new ZQuery(GlbPortDeliveryTimeSchema.G1_GC_Company, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(GlbPortDeliveryTimeSchema.G1_FreightMode, Parent.G1_FreightMode);
				query.AddToFilter(GlbPortDeliveryTimeSchema.G1_JobMode, Parent.G1_JobMode);
				query.AddToFilter(GlbPortDeliveryTimeSchema.G1_RL_NKDestinationPort, Parent.G1_RL_NKDestinationPort);
				query.AddToFilter(GlbPortDeliveryTimeSchema.G1_RL_NKDischargePort, Parent.G1_RL_NKDischargePort);
				query.AddToFilter(GlbPortDeliveryTimeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				if (Parent.G1_OH_ClientOverride.IsEmpty)
				{
					query.AddToFilter(GlbPortDeliveryTimeSchema.G1_OH_ClientOverride, SQLComparisonOperator.Equal, System.DBNull.Value);
				}
				else
				{
					query.AddToFilter(GlbPortDeliveryTimeSchema.G1_OH_ClientOverride, Parent.G1_OH_ClientOverride);
				}

				return Parent.Factory.LoadTop1(Parent.GetType(), query) != null;
			}
		}
	}
}
