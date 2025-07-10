using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public abstract class SendACASMessageMenuItem : BaseHVLVMenuItem
	{
		public SendACASMessageMenuItem(MultilingualString caption, ForwardingShipment shipment)
			: base(caption, shipment)
		{
			Name = GetType().Name;
		}

		protected override Action MenuAction => () =>
		{
			if (UserPromptCheckingHelper.CheckUSSecurityFilingsEnabled()
				&& CheckCanSendACASMessage())
			{
				var acasSender = ObjectFactory.Get<IHVLVAirCargoAdvanceScreeningMessageSender>(nameof(IHVLVAirCargoAdvanceScreeningMessageSender), shipment);
				var errorMessage = acasSender.ValidateACASReportBasicRequirments();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					Globals.Message.Show(errorMessage);
				}
				else
				{
					if (Header != null)
					{ 
						var consignmentsForACASWrapperCollection = new HVLVConsignmentForACASWrapperCollection(ConsignmentsForACAS);
						consignmentsForACASWrapperCollection.RunPreSaveValidation();

						ZFormModaliser.ShowDialogAndDispose(new HVLVConsignmentACASValidationForm(consignmentsForACASWrapperCollection, ACASAction), Form);
					}
				}
			}
		};

		public override void UpdateVisibilityAndCaption()
		{
			Visible = HVLVMenuItemHelper.IsAirShipmentWithUSDestination(shipment);
		}

		protected virtual bool CheckCanSendACASMessage() => UserPromptCheckingHelper.CheckBusinessObjectHasNoChangesOrNotify(shipment, SaveFormBeforeSendACASMessage)
			&& UserPromptCheckingHelper.CheckHasAnyActiveConsignmentOrNotify(ConsignmentsForACAS);

		public abstract ACASReportAction ACASAction { get; }

		protected abstract string SaveFormBeforeSendACASMessage { get; }

		protected virtual string ACASMessageStatus { get; }

		protected IEnumerable<HVLVConsignment> ConsignmentsForACAS => consignmentsForACAS ?? (consignmentsForACAS = Header?.Consignments.Where(x => x.HVC_ACASMessageStatus == ACASMessageStatus && x.HVC_IsActive));
		IEnumerable<HVLVConsignment> consignmentsForACAS;
	}
}
