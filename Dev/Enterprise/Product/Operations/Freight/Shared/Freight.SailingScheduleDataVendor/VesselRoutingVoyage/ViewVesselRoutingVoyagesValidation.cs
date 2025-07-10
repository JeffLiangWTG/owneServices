//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewVesselRoutingVoyagesValidation
//
//    This class should be used for overriding validation in AutoViewVesselRoutingVoyagesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Freight.SailingScheduleDataVendor.Res;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class ViewVesselRoutingVoyagesValidation : AutoViewVesselRoutingVoyagesValidation
	{
		public ViewVesselRoutingVoyagesValidation(AutoViewVesselRoutingVoyages parent)
			: base(parent)
		{
		}

		#region E8_OH_LineOperator

		public void ValidateE8_OH_LineOperator()
		{
			ValidateCalculatedProperty(Parent.E8_OH_LineOperatorInfo);
		}

		protected void CheckE8_OH_LineOperator()
		{
			Parent.E8_OH_LineOperatorInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(Parent.E8_OH_LineOperatorInfo, Parent.E8_OH_LineOperator_List);

			if (!Parent.E8_OH_LineOperatorInfo.HasErrors() && !Parent.E8_LineOperator.IsEmpty && !Parent.LineOperatorOrgMatchesExternalCode())
			{
				Parent.E8_OH_LineOperatorInfo.AddError(Res.GetString("15ebdc9a-cdcd-4b08-b358-464d00bacf1a", @"This Carrier does not match this Operator Code against this Data Source provider.
Add the Operator Code as Registration Number on the Carrier Organization > Details > Config tab based on the Data Source provider."));
			}
		}

		#endregion

		#region E8_ForeignPortToAdd

		public void ValidateE8_ForeignPortToAdd()
		{
			ValidateCalculatedProperty(Parent.E8_ForeignPortToAddInfo);
		}

		protected void CheckE8_ForeignPortToAdd()
		{
			Parent.E8_ForeignPortToAddInfo.ClearAllNotifications();
			if (Parent.E8_ForeignPortToAdd.StartsWith("AU"))
			{
				Parent.E8_ForeignPortToAddInfo.AddError(Res.GetString("eb827fb9-a987-4a6f-b20b-859d92dd8ca6", "Enter a foreign port"));
			}
		}

		#endregion

		#region E8_LloydsNumber

		protected override void CheckE8_LloydsNumber()
		{
			base.CheckE8_LloydsNumber();

			if (Parent.E8_IsSelected)
			{
				RefVessel anyVesselWithMatchingLloyds = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, Parent.E8_LloydsNumber));
				if (anyVesselWithMatchingLloyds == null)
				{
					Parent.E8_LloydsNumberInfo.AddWarning(Res.GetString("ace0fb22-3396-4ce2-8c8f-164e8f6935ca", "This vessel is not registered. A vessel will be created with this Vessel Name and Lloyds number."));
				}
			}
		}

		#endregion

		#region Implementation

		BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		new VesselRoutingVoyage Parent
		{
			get { return (VesselRoutingVoyage)base.Parent; }
		}

		#endregion

		protected override void CheckE8_DataProviderIsNotEmpty()
		{
		}

		protected override void CheckE8_LineOperatorIsNotEmpty()
		{
		}

		protected override void CheckE8_LloydsNumberIsNotEmpty()
		{
		}

		protected override void CheckE8_OperatorsDescriptionIsNotEmpty()
		{
		}

		protected override void CheckE8_VoyageIsNotEmpty()
		{
		}
	}
}
