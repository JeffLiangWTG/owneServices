using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffTelematicsBusinessObjectLookupProvider : ITelematicsBusinessObjectLookupProvider
	{
		#region ITelematicsBusinessObjectLookupProvider

		public ITelematicsBusinessObject GetBusinessObject(BusinessObjectFactory factory, ZGuid primaryKey)
		{
			var staff = factory.Load<GlbStaff>(primaryKey);
			return staff == null ? null : new StaffTelematicsBusinessObject(staff);
		}

		#endregion

		#region Implementation

		class StaffTelematicsBusinessObject : ITelematicsBusinessObject
		{
			public StaffTelematicsBusinessObject(GlbStaff staff)
			{
				if (staff == null)
				{
					throw new ArgumentNullException(nameof(staff));
				}

				this.staff = staff;
			}

			readonly GlbStaff staff;

			#region ITelematicsBusinessObject

			public string Code
			{
				get { return staff.GS_Code; }
			}

			public string DescriptionForInterface
			{
				get { return staff.GS_FullName; }
			}

			public string DescriptionInEnglish
			{
				get { return staff.GS_FullName; }
			}

			public string TypeIdentifier
			{
				get { return Res.GetString("GlbStaff", "Staff"); }
			}

			#endregion
		}

		#endregion
	}
}
