//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbDeviceValidation
//
//    This class should be used for overriding validation in AutoGlbDeviceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceValidation : AutoGlbDeviceValidation
	{
		public GlbDeviceValidation(AutoGlbDevice parent)
			: base(parent)
		{
		}

		new GlbDevice Parent
		{
			get { return (GlbDevice)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateAssignedParentStaffID();
			ValidateAssignedParentEquipmentID();
		}

		#region AssignedParentStaffID

		public void ValidateAssignedParentStaffID()
		{
			ValidateCalculatedProperty(Parent.AssignedParentStaffIDInfo);
		}

		protected void CheckAssignedParentStaffID()
		{
			ListValidation.ErrorIfInvalidPK(Parent.AssignedParentStaffIDInfo);
		}

		#endregion

		#region AssignedParentEquipmentID

		public void ValidateAssignedParentEquipmentID()
		{
			ValidateCalculatedProperty(Parent.AssignedParentEquipmentIDInfo);
		}

		protected void CheckAssignedParentEquipmentID()
		{
			ListValidation.ErrorIfInvalidPK(Parent.AssignedParentEquipmentIDInfo);
			ValidateParentId(Parent.AssignedParentEquipmentID, Parent.AssignedParentEquipmentIDInfo);
		}

		#endregion

		void ValidateParentId(ZGuid guid, ZPropertyInfo propertyInfo)
		{
			var query = new ZQuery(GlbDeviceAssignmentDivotSchema.V7_ParentID, guid);
			query.AddToFilter(GlbDeviceAssignmentDivotSchema.V7_EndTimeUtc, SQLComparisonOperator.Equal, null);
			query.AddToFilter(GlbDeviceAssignmentDivotSchema.V7_V3_Device, SQLComparisonOperator.NotEqual, Parent.PK);
			var divot = Parent.Factory.LoadTop1<GlbDeviceAssignmentDivot>(query);

			if (divot != null)
			{
				var openDevice = Parent.Factory.Load<GlbDevice>(divot.V7_V3_Device);
				var message = Res.GetString("b539c861-264a-424d-b36b-904fdf8e25f4", "The Equipment you are trying to assign this device to is currently assigned to {0}", openDevice.V3_HumanReadableIdentifier);
				propertyInfo.AddError(message);
			}
		}
	}
}
