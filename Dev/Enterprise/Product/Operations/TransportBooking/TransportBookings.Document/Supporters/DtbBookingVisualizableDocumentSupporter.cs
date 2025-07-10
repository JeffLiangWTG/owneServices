using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Document
{
	public sealed class DtbBookingVisualizableDocumentSupporter : TransportBookingsVisualizableDocumentSupporter<DtbBooking>
	{
		public DtbBookingVisualizableDocumentSupporter(DtbBooking dtbBooking)
			: base(dtbBooking)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.DtbBookingCustomiseForms;

		public override Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem)
		{
			var contextTypes = GetContextTypes(menuItem);

			if (contextTypes == null)
			{
				return (object)null;
			}

			if (contextTypes.Contains(DataContext.CMRConsignmentNote))
			{
				return GetCMRConsignmentNoteAdditionalData();
			}

			return (object)null;
		}

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;

		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;

		public override IMessageLogCreator GetMessageLogCreator(IDocument document) => null;

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => null;

		protected override string GetDataStoreNameFromDocumentName(string documentName, EventParameters eventParameters) => documentName;

		public override string GetMessageBroker() => EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface;

		public override bool ShouldUseDraftWatermark(IDocument document) => false;

		ZString[] GetContextTypes(IStmMenuItem menuItem)
		{
			return menuItem
						?.Documents
						.OfType<IStmMenuTemplatePivot>()
						.Select(p => p.Template.SO_DataContext)
						.ToArray();
		}

		Either<string, object> GetCMRConsignmentNoteAdditionalData()
		{
			if (parent?.Instructions == null || !parent.Instructions.Any())
			{
				return NoPickupOrDeliveryInstructionsMessage;
			}

			var instructions = parent.Instructions.OfType<DtbBookingInstruction>().ToArray();

			var pickupInstructions = instructions.Where(i => i.IsPickUp).ToArray();
			var deliveryInstructions = instructions.Where(i => i.IsDelivery).ToArray();

			if (!pickupInstructions.Any() || !deliveryInstructions.Any())
			{
				return NoPickupOrDeliveryInstructionsMessage;
			}

			if (pickupInstructions.Length == 1 && deliveryInstructions.Length == 1)
			{
				return new DtbBookingInstruction[] { pickupInstructions.First(), deliveryInstructions.First() };
			}

			if (pickupInstructions.Length > 1 && deliveryInstructions.Length > 1)
			{
				return MultiplePickupAndDeliveryInstructionsMessage;
			}

			var singleInstruction = pickupInstructions.Length == 1 ? pickupInstructions.First() : deliveryInstructions.First();
			var multipleInstructions = pickupInstructions.Length == 1 ? deliveryInstructions : pickupInstructions;

			var selector = ObjectFactory.Get<IInstructionSelector>();
			var selectedInstructions = selector.SelectInstruction(multipleInstructions);

			if (selectedInstructions.IsLeft)
			{
				return selectedInstructions.Left;
			}

			if (selectedInstructions.Right == null)
			{
				return NoApplicableInstructionsMessage;
			}

			return new DtbBookingInstruction[] { singleInstruction, selectedInstructions.Right };
		}

		string NoApplicableInstructionsMessage => Res.GetString("2edfc237-fd72-11ef-b702-973f438b1141", "There are no applicable instructions.");
		string NoPickupOrDeliveryInstructionsMessage => Res.GetString("54f15696-fe17-11ef-9696-b44340dddbda", "Transport Booking does not contain PIC or DLV Instructions.");
		string MultiplePickupAndDeliveryInstructionsMessage => Res.GetString("323a8b5e-fe19-11ef-9ecb-0430d634265a", "Transport Booking contains excessive PIC and DLV Instructions.");
	}
}
