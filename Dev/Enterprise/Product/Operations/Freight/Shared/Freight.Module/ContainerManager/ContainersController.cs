using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Module
{
	public class ContainersController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Containers; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Containers; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CommonContainer); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ContainersForm(GetCommonContainer(businessEntity));
		}

		public override IZForm ShowEditForm(BusinessObject businessObject)
		{
			var container = GetCommonContainer(businessObject);
			return base.ShowEditForm(container);
		}

		CommonContainer GetCommonContainer(IBusiness sourceEntity)
		{
			CommonContainer result = null;

			var sourceEntityType = sourceEntity.GetType();
			var baseCustomsContainerType = ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();

			if (sourceEntityType.IsSubclassOf(baseCustomsContainerType) || sourceEntityType == baseCustomsContainerType)
			{
				result = (CommonContainer)((BusinessObject)sourceEntity)["JobContainer"];
			}
			else
			{
				result = (CommonContainer)sourceEntity;
			}
			return result;
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var container = GetCommonContainer(sourceEntity);

			var booking = container.Booking;

			Type entityType = null;

			if (booking != null && booking.JS_IsShipping)
			{
				entityType = booking.JS_ShipmentStatus == ShipmentStatusList.Codes.WebFwdInstruction || booking.JS_ShipmentStatus == ShipmentStatusList.Codes.Confirmed
					? ObjectFactory.GetType<Agency.IBillOfLadingContainer>()
					: ObjectFactory.GetType<Agency.IAgencyBookingContainer>();
			}

			return Factory.Load(entityType ?? TypeOfTopLevelBusinessObject, ((IBusiness)container).Identifier);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ForwardingContainer; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ForwardingContainer; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ForwardingContainerEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ForwardingContainer; }
		}
	}
}
