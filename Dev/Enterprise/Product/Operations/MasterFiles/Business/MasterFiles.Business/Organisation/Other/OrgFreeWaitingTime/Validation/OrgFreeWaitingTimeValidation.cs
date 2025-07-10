//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgFreeWaitingTimeValidation
//
//    This class should be used for overriding validation in AutoOrgFreeWaitingTimeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Core;

	public class OrgFreeWaitingTimeValidation : AutoOrgFreeWaitingTimeValidation
	{
		public OrgFreeWaitingTimeValidation(AutoOrgFreeWaitingTime parent) : base(parent)
		{
		}

		protected override void CheckOY_DropMode()
		{
			if (!new CombinedEquipmentNeededList().CodesAsString.Contains(Parent.OY_DropModeInfo.Value.ToString()))
			{
				Parent.OY_DropModeInfo.AddError(Res.GetString("bc18cfcb-ASDF-4efc-1234-38177d2b6d76", "Please use a valid Drop Mode from the list."));
			}
			if (Parent.OY_DropModeInfo.Value.IsEmpty)
			{
				Parent.OY_DropModeInfo.AddError(Res.GetString("bc18cfcb-1234-4efc-ae63-38177d2b6d76", "Drop Mode cannot be empty. Use 'ANY' for a catch all fallback."));
			}
		}

		protected override void CheckOY_RC_ContainerType()
		{
			var containerTypes = ((IEnumerable<BusinessObject>)((OrgFreeWaitingTime)Parent).ContainerTypes);
			if (!containerTypes.Select(i => i.PK).Contains((ZGuid)Parent.OY_RC_ContainerTypeInfo.Value) && !Parent.OY_RC_ContainerTypeInfo.Value.IsEmpty)
			{
				Parent.OY_RC_ContainerTypeInfo.AddError(Res.GetString("6c6a5590-c510-4b64-1234-2dbff310f667", "Please enter a valid Container Type."));
			}
		}
	}
}
