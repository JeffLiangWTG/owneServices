using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobShipmentPreplanningValidation : AutoJobShipmentPreplanningValidation
	{
		public JobShipmentPreplanningValidation(AutoJobShipmentPreplanning parent) : base(parent)
		{
		}

		#region EF_OA_BuyerAddress

		protected override void CheckEF_OA_BuyerAddress()
		{
			base.CheckEF_OA_BuyerAddress();
			MandatoryValidation.CheckEntered(Parent.EF_OA_BuyerAddressInfo);
		}

		#endregion

		#region EF_RL_NKPortDisch

		protected override void CheckEF_RL_NKPortDisch()
		{
			base.CheckEF_RL_NKPortDisch();
			MandatoryValidation.CheckEntered(Parent.EF_RL_NKPortDischInfo);
			ListValidation.ErrorIfInvalidCode(Parent.EF_RL_NKPortDischInfo, Parent.Lookups.PortDisches);
		}

		#endregion

		#region EF_RL_NKPortLoad

		protected override void CheckEF_RL_NKPortLoad()
		{
			base.CheckEF_RL_NKPortLoad();
			MandatoryValidation.CheckEntered(Parent.EF_RL_NKPortLoadInfo);
			ListValidation.ErrorIfInvalidCode(Parent.EF_RL_NKPortLoadInfo, Parent.Lookups.PortLoads);
		}

		#endregion

		#region EF_MasterBill

		protected override void CheckEF_MasterBill()
		{
			base.CheckEF_MasterBill();
			MandatoryValidation.WarnIfNotEntered(Parent.EF_MasterBillInfo);

			if (HasAirConsolMatchesWithSameMAWB)
			{
				Parent.EF_MasterBillInfo.AddError(Res.GetString("2d6809fa-00b6-4308-a195-cd69e2b564ee", "An air consol has already been created for this Master Bill. Same Master Bill can be used only for one air consol."));
			}
		}

		#endregion

		#region EF_UnitOfVolume

		protected override void CheckEF_UnitOfVolume()
		{
			base.CheckEF_UnitOfVolume();
			ListValidation.ErrorIfInvalidCode(Parent.EF_UnitOfVolumeInfo, Parent.Lookups.UnitVolumeList);
		}

		#endregion

		#region EF_UnitOfWeight

		protected override void CheckEF_UnitOfWeight()
		{
			base.CheckEF_UnitOfWeight();
			ListValidation.ErrorIfInvalidCode(Parent.EF_UnitOfWeightInfo, Parent.Lookups.UnitWeightList);
		}

		#endregion

		#region EF_F3_NKPackType

		protected override void CheckEF_F3_NKPackType()
		{
			base.CheckEF_F3_NKPackType();
			ListValidation.ErrorIfInvalidCode(Parent.EF_F3_NKPackTypeInfo, Parent.Lookups.UnitPackList);
		}

		#endregion

		#region EF_OH_SendingAgent

		protected override void CheckEF_OH_SendingAgent()
		{
			base.CheckEF_OH_SendingAgent();
			if (!Parent.EF_OH_SendingAgent.IsEmpty)
			{
				CompareValidation.CheckNotEqual(Parent.EF_OH_SendingAgentInfo, Parent.EF_OH_ReceivingAgentInfo);
			}
		}

		#endregion

		#region EF_OH_ReceivingAgent

		protected override void CheckEF_OH_ReceivingAgent()
		{
			base.CheckEF_OH_ReceivingAgent();
			if (!Parent.EF_OH_ReceivingAgent.IsEmpty)
			{
				CompareValidation.CheckNotEqual(Parent.EF_OH_ReceivingAgentInfo, Parent.EF_OH_SendingAgentInfo);
			}
		}

		#endregion

		#region Public Methods

		public bool HasAirConsolMatchesWithSameMAWB
		{
			get
			{
				bool result = false;
				if (!Parent.EF_MasterBill.IsEmpty)
				{
					ZQuery query = new ZQuery(JobConsolSchema.JK_MasterBillNum, Parent.EF_MasterBill);
					query.AddToFilter(JoinCondition.And, JobConsolSchema.JK_TransportMode, Constants.TransportModes.Air);
					result = Parent.Factory.ExistsInDatabase(JobConsolSchema.Constants.TableName, query);
				}
				return result;
			}
		}

		#endregion
	}
}
