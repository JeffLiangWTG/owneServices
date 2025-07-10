using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgRelatedPartiesModuleFilterValidation : ModuleTextFilterValidation
	{
		public OrgRelatedPartiesModuleFilterValidation(OrgRelatedPartiesModuleFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly OrgRelatedPartiesModuleFilter parent;

		public void ValidatePartyType()
		{
			ValidateCalculatedProperty(parent.PartyTypeInfo);
		}

		protected void CheckPartyType()
		{
			ListValidation.ErrorIfInvalidCode(parent.PartyTypeInfo);
		}

		public void ValidateTransportMode()
		{
			ValidateCalculatedProperty(parent.TransportModeInfo);
		}

		protected void CheckTransportMode()
		{
			ListValidation.ErrorIfInvalidCode(parent.TransportModeInfo);
		}

		public void ValidateContainerMode()
		{
			ValidateCalculatedProperty(parent.ContainerModeInfo);
		}

		protected void CheckContainerMode()
		{
			ListValidation.ErrorIfInvalidCode(parent.ContainerModeInfo);
		}

		public void ValidateDirection()
		{
			ValidateCalculatedProperty(parent.DirectionInfo);
		}

		protected void CheckDirection()
		{
			ListValidation.ErrorIfInvalidCode(parent.DirectionInfo);
		}

		public void ValidateRelatedParty()
		{
			ValidateCalculatedProperty(parent.RelatedPartyInfo);
		}

		protected void CheckRelatedParty()
		{
			TypeValidation.CheckValidGuid(parent.RelatedPartyInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePartyType();
			ValidateTransportMode();
			ValidateContainerMode();
			ValidateDirection();
			ValidateRelatedParty();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}
	}
}
