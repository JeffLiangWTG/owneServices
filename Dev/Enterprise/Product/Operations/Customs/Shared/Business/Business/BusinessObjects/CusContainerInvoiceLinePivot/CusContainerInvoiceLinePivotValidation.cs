using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusContainerInvoiceLinePivotValidation : AutoCusContainerInvoiceLinePivotValidation
	{
		public CusContainerInvoiceLinePivotValidation(AutoCusContainerInvoiceLinePivot parent)
			: base(parent)
		{
		}

		protected virtual bool IsCusContainerInvoiceLineValidationRequired
		{
			get { return false; }
		}

		protected override void CheckC2_NetWeight()
		{
			base.CheckC2_NetWeight();

			if (IsCusContainerInvoiceLineValidationRequired)
			{
				var invoiceLine = Parent.InvoiceLine;
				var container = Parent.Container;
				if (invoiceLine != null && container != null && invoiceLine.ContainersPivot.TotalNetWeightInKG != invoiceLine.NetWeightInKG)
				{
					string message = Res.GetString("6d18fc63-a6ca-448c-95b3-62541c8309ef", "Total of these values do not add up to invoice line net weight.");
					Parent.C2_NetWeightInfo.AddWarning(message);
				}
			}
		}

		protected override void CheckC2_SplitValue()
		{
			base.CheckC2_SplitValue();

			if (IsCusContainerInvoiceLineValidationRequired)
			{
				var invoiceLine = Parent.InvoiceLine;
				var container = Parent.Container;
				if (invoiceLine != null && container != null && invoiceLine.ContainersPivot.TotalSplitValue != invoiceLine.JI_LinePrice)
				{
					string message = Res.GetString("76e4aebf-eb58-47b2-84be-8b8de08038ca", "Total of these values do not add up to invoice line price.");
					Parent.C2_SplitValueInfo.AddWarning(message);
				}
			}
		}

		protected override void CheckC2_CO()
		{
			base.CheckC2_CO();
			var invoiceLine = Parent.InvoiceLine;
			var container = Parent.Container;
			if (invoiceLine != null && container != null)
			{
				if (IsCusContainerInvoiceLineValidationRequired && ValidationTypeForBillContainerPivotMatching != NotificationTypes.None)
				{
					var invoiceHeader = invoiceLine.InvoiceHeader;
					var bill = invoiceHeader == null ? null : invoiceHeader.Bill;

					if (bill != null && bill.PackingGroups.GetElementWithContainer(container) == null)
					{
						string containerNumbers = bill.PackingGroups.ContainerNumbersLinked;

						string message = string.Format(InvoiceLineLinkedToContainerWhichIsNotLinkedToInvoiceHeaderBill, string.IsNullOrEmpty(containerNumbers) ? Res.GetString("b4a33a84-0473-49a6-a7f8-705edbb2346d", "none") : containerNumbers);

						if (ValidationTypeForBillContainerPivotMatching == NotificationTypes.Warning)
						{
							Parent.C2_COInfo.AddWarning(message);
						}
						else if (ValidationTypeForBillContainerPivotMatching == NotificationTypes.MessageError)
						{
							Parent.C2_COInfo.AddMessageError(message);
						}
						else if (ValidationTypeForBillContainerPivotMatching == NotificationTypes.Error)
						{
							Parent.C2_COInfo.AddError(message);
						}
					}
				}

				if (invoiceLine != null && invoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking)
				{
					// Ensure this container has a line in the invoice line package grid or the invoice package gird
					var foundPackagePivotForMyContainer = false;

					foreach (var linePackagePivot in invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>())
					{
						if (linePackagePivot.Package != null && !Parent.ContainerNumber.IsEmpty && linePackagePivot.Package.CW_ContainerNoOrEquipmentNo == Parent.ContainerNumber)
						{
							foundPackagePivotForMyContainer = true;
							break;
						}
					}
					foundPackagePivotForMyContainer = foundPackagePivotForMyContainer || invoiceLine.GetInvoicePackPivotsFromContainer(container).Any();
					if (!foundPackagePivotForMyContainer)
					{
						Parent.C2_COInfo.AddMessageError(Res.GetString("EE490BE9-6E36-4334-ADAD-AABD96D3B3CB", "This container is not selected in the 'Invoice Headers/Invoice Lines - Packages' grid. Please untick 'Is For Invoice Line' on or select a package or packages for this container."));
					}
				}
			}
		}

		public static string InvoiceLineLinkedToContainerWhichIsNotLinkedToInvoiceHeaderBill
		{
			get { return Res.GetString("f56b24cf-c6cf-4dc2-934a-f3d349376ac0", "This container is not linked to a house bill this invoice line's invoice header is associated with. The containers that are currently associated with the house bill are {0:G}."); }
		}

		protected virtual NotificationTypes ValidationTypeForBillContainerPivotMatching
		{
			get { return NotificationTypes.None; }
		}

		protected new CusContainerInvoiceLinePivot Parent
		{
			get { return (CusContainerInvoiceLinePivot)base.Parent; }
		}
	}
}
