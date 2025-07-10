using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX601MessageSendingObjectAdditionalMessageErrorCollector : LicensingMessageSendingObjectAdditionalMessageErrorCollector
	{
		public NX601MessageSendingObjectAdditionalMessageErrorCollector(LicensingMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override List<INotification> GetDeclarationNotificationCore(JobDeclaration declaration)
		{
			var notifications = base.GetDeclarationNotificationCore(declaration);
			if (declaration.DeclarationNumberDisplay.IsEmpty)
			{
				notifications.Add(GetNotification(EntryNumberHumanReadableName, (NoResString)"You have not generated Entry Number."));
			}
			return notifications;
		}

		protected override List<INotification> GetInvoiceLineNotificationCore(JobComInvoiceLine invoiceLine)
		{
			var notifications = base.GetInvoiceLineNotificationCore(invoiceLine);
			if (invoiceLine.JI_GoodsType.IsEmpty)
			{
				notifications.Add(GetNotification(GoodsTypeHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage(GoodsTypeHumanReadableName)));
			}
			if (invoiceLine.JI_InnerPackType.IsEmpty)
			{
				notifications.Add(GetNotification(PackagingTypeHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage(PackagingTypeHumanReadableName)));
			}
			if (invoiceLine.JI_InnerPackingMaterial.IsEmpty)
			{
				notifications.Add(GetNotification(PackagingMaterialHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage(PackagingMaterialHumanReadableName)));
			}
			if (invoiceLine.JI_CustomsThirdQuantity.IsEmpty)
			{
				notifications.Add(GetNotification(LicensingQuantityHumanReadableName, MandatoryValidation.ValueCannotBeZeroMessage(LicensingQuantityHumanReadableName)));
			}
			if (invoiceLine.JI_CustomsThirdUnitQty.IsEmpty)
			{
				notifications.Add(GetNotification(LicensingUQHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage(LicensingUQHumanReadableName)));
			}
			if (invoiceLine.JI_Compositions.IsEmpty)
			{
				notifications.Add(GetNotification(SpecificationHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage(SpecificationHumanReadableName)));
			}
			if (invoiceLine.JI_NetWeight <= 0m)
			{
				notifications.Add(GetNotification(NetWeightHumanReadableName, Res.GetString("06EA5B44-4159-4200-9E78-9914134D3037", "Please enter a 'Net Weight' greater than 0.")));
			}
			if (!invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.Any())
			{
				notifications.Add(GetNotification(StorageAndShippingConditionsHumanReadableName, Res.GetString("4BE34A2B-48B9-404D-BFEA-A3E7B9EAD2E1", "You have not entered at least one Storage and Shipping Conditions Code.")));
			}
			var manufacturerDocAddress = invoiceLine.ManufacturerDocAddress;
			if (manufacturerDocAddress == null || manufacturerDocAddress.CompanyName.IsEmpty)
			{
				notifications.Add(GetNotification(ForeignManufacturerHumanReadableName, MandatoryValidation.YouHaveNotEnteredMessage((NoResString)"Foreign Manufacturer Company Name")));
			}
			return notifications;
		}
	}
}
