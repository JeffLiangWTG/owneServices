using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceAssignmentDivot : AutoGlbDeviceAssignmentDivot, IDeviceAssignmentDivot
	{
		public GlbDeviceAssignmentDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public GlbDevice Device
		{
			get { return Factory.Load<GlbDevice>(V7_V3_Device); }
		}

		[RelatedBusinessObject("Device")]
		public override ZGuid V7_V3_Device
		{
			get { return base.V7_V3_Device; }
			set { base.V7_V3_Device = value; }
		}

		[ResourceStringData("GlbDeviceAssignmentDivot|ParentObjectName", Caption = "Assigned To")]
		public ZString ParentObjectName
		{
			get { return string.Format("{0} - {1} ({2})", AssignedBusinessObject.Code, AssignedBusinessObject.DescriptionForInterface, AssignedBusinessObject.TypeIdentifier); }
		}

		ITelematicsBusinessObject AssignedBusinessObject
		{
			get
			{
				if (assignedBusinessObject == null)
				{
					assignedBusinessObject = GetAssignedBusinessObject();
				}
				return assignedBusinessObject;
			}
		}
		ITelematicsBusinessObject assignedBusinessObject;

		public override ZGuid V7_ParentID
		{
			get { return base.V7_ParentID; }
			set
			{
				base.V7_ParentID = value;
				assignedBusinessObject = null;
			}
		}

		public override ZString V7_ParentTableCode
		{
			get { return base.V7_ParentTableCode; }
			set
			{
				base.V7_ParentTableCode = value;
				assignedBusinessObject = null;
			}
		}

		#region Implementation

		ITelematicsBusinessObject GetAssignedBusinessObject()
		{
			var lookupName = string.Format("{0}_{1}", nameof(ITelematicsBusinessObjectLookupProvider), V7_ParentTableCode);
			var provider = ObjectFactory.Get<ITelematicsBusinessObjectLookupProvider>(lookupName);
			return provider.GetBusinessObject(Factory, V7_ParentID);
		}

		#endregion
	}
}
