using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Common.Business.Testing
{
	public class CartageTestHelper
	{
		public CartageTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		public BusinessObjectFactory Factory { get { return factory; } }
		readonly BusinessObjectFactory factory;

		#region Staff

		public GlbStaff CreateStaff(ZString code, ZString name)
		{
			var result = Factory.New<GlbStaff>();
			result.GS_Code = code;
			result.GS_LoginName = name;
			return result;
		}

		#endregion

		#region Truck

		public RefEquipment CreateTruck()
		{
			return CreateTruck("123");
		}

		public RefEquipment CreateTruck(ZString code)
		{
			var result = Factory.New<RefEquipment>();
			result.RQ_IsVehicle = true;
			result.RQ_ShortCode = code;
			result.RQ_Registration = code;
			return result;
		}

		#endregion

		public CommonCartageType CreateCartageType(ZString jobType)
		{
			var result = Factory.New<CommonCartageType>();
			result.E3_JobType = jobType;
			return result;
		}

		public CommonCartageOrg CreateCartageTypeOrg(CommonCartageType cartageType, ZString orgType)
		{
			var result = cartageType.CommonCartageOrganisations.AddNew();
			result.E5_OrgType = orgType;
			return result;
		}

		public CommonCartageLegType CreateCartageMoveType(CommonCartageType cartageType, ZString containerMode, CommonCartageOrg firstOrg, CommonCartageOrg secondOrg)
		{
			var move = (containerMode == "CNT") ? cartageType.ContainerizedBookedMoveTypes.AddNew() : cartageType.LooseBookedMoveTypes.AddNew();
			move.E4_ContainerMode = containerMode;
			move.E4_E5_FromOrg = firstOrg != null ? firstOrg.PK : ZGuid.Empty;
			move.E4_E5_WaitPointOrg = secondOrg != null ? secondOrg.PK : ZGuid.Empty;
			return move;
		}

		public CommonCartageLegType CreateCartageLegType(CommonCartageType cartageType, ZString containerMode, CommonCartageOrg fromOrg, CommonCartageOrg waitPOrg, CommonCartageOrg toPOrg)
		{
			var leg = (containerMode == "CNT") ? cartageType.ContainerizedCartageLegTypes.AddNew() : cartageType.LooseCartageLegTypes.AddNew();
			leg.E4_ContainerMode = containerMode;
			leg.E4_E5_FromOrg = fromOrg != null ? fromOrg.PK : ZGuid.Empty;
			leg.E4_E5_WaitPointOrg = waitPOrg != null ? waitPOrg.PK : ZGuid.Empty;
			leg.E4_E5_ToOrg = toPOrg != null ? toPOrg.PK : ZGuid.Empty;
			return leg;
		}
	}
}
