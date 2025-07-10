using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.TW.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sending object names")]
	public class LicensingMessageSendingObjectAdditionalMessageErrorCollector : IEnumerable<INotification>
	{
		protected LicensingMessageSendingObjectParent Parent { get; }

		protected const string ConsigneeHumanReadableName = "Consignee";
		protected const string DeclarantHumanReadableName = "Declarant";
		protected const string EntryNumberHumanReadableName = "Entry Number";
		protected const string GoodsTypeHumanReadableName = "Goods Type";
		protected const string PackagingTypeHumanReadableName = "Packaging Type";
		protected const string PackagingMaterialHumanReadableName = "Packaging Material";
		protected const string SpecificationHumanReadableName = "Specification";
		protected const string NetWeightHumanReadableName = "Net Weight";
		protected const string LicensingQuantityHumanReadableName = "Licensing Quantity";
		protected const string LicensingUQHumanReadableName = "Licensing UQ";
		protected const string StorageAndShippingConditionsHumanReadableName = "Storage and Shipping Conditions";
		protected const string ForeignManufacturerHumanReadableName = "Foreign Manufacturer";

		public LicensingMessageSendingObjectAdditionalMessageErrorCollector(LicensingMessageSendingObjectParent parent)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		protected Notification GetNotification(string humanReadableName, string message) => new Notification(NotificationType.MessageError, $"{humanReadableName}: {message}");

		List<INotification> GetDeclarationNotification(JobDeclaration declaration) => GetDeclarationNotificationCore(declaration);
		protected virtual List<INotification> GetDeclarationNotificationCore(JobDeclaration declaration)
		{
			return new List<INotification>();
		}

		List<INotification> GetInvoiceHeaderNotification(JobComInvoiceHeader invoiceHeader) => GetInvoiceHeaderNotificationCore(invoiceHeader);
		protected virtual List<INotification> GetInvoiceHeaderNotificationCore(JobComInvoiceHeader invoiceHeader)
		{
			return new List<INotification>();
		}

		List<INotification> GetInvoiceLineNotification(JobComInvoiceLine invoiceLine) => GetInvoiceLineNotificationCore(invoiceLine);
		protected virtual List<INotification> GetInvoiceLineNotificationCore(JobComInvoiceLine invoiceLine)
		{
			return new List<INotification>();
		}

		public IEnumerator<INotification> GetEnumerator()
		{
			var notifications = new List<INotification>();
			foreach (LicensingMessageSendingObject sendingObject in Parent.SendingObjectsCollection)
			{
				if (sendingObject.ShouldSend)
				{
					var header = sendingObject.Header;

					notifications.AddRange(GetDeclarationNotification(header.Declaration));
					var controllingMessageHeaderLinkInvoiceLines = header.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().Where(c => c.Link);
					var invoiceHeaders = controllingMessageHeaderLinkInvoiceLines.Select(c => c.Invoiceline.InvoiceHeader).Distinct();
					foreach (var invoiceHeader in invoiceHeaders)
					{
						notifications.AddRange(GetInvoiceHeaderNotification(invoiceHeader));
					}

					foreach (var controllingMessageHeaderLinkInvoiceLine in controllingMessageHeaderLinkInvoiceLines)
					{
						var invoiceLine = controllingMessageHeaderLinkInvoiceLine.Invoiceline;
						notifications.AddRange(GetInvoiceLineNotification(invoiceLine));
					}
				}
			}

			return notifications.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
