using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class ENS42 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.ENS42, IBIRDLineRecord
	{
		#region IBIRDLineRecord Members

		void IBIRDLineRecord.Update(JobComInvoiceLine invoiceLine, INotifications notifications)
		{
			if (!SupplierIDCode.IsEmpty)
			{
				var supplierAddress = BIRDOrganisationMatching.GetOrganisationAddress(invoiceLine.Factory, OrgMatchedCustomsRegNoType.MID, SupplierIDCode);

				if (supplierAddress == null)
				{
					OrgAddress address = OrganisationCreator.CreateManufacturerAndSendNameAddressQueryMessage(invoiceLine.Factory, SupplierIDCode);
					if (address != null)
					{
						supplierAddress = address;

						notifications.AddWarning("Supplier:" + ZString.Format(OrganisationCreator.ManufacturerCreated, SupplierIDCode));
					}
					else if (!SupplierIDCode.IsLettersAndNumbersOnlyOrEmpty)
					{
						notifications.AddWarning("Supplier:" + ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, SupplierIDCode));
					}
				}

				invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.InvoiceHeader, JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress, supplierAddress?.PK ?? ZGuid.Empty, SupplierIDCode, "Supplier", notifications);
			}

			invoiceLine.InvoiceHeader.JZ_InvoiceNumber = InvoiceNumber;

			invoiceLine.Declaration.US_EnableAII = true;

			invoiceLine.InvoiceHeader.US_IsLineGrouping = true;

			AddLineGroupingIfNecessary(invoiceLine, BeginningInvoiceLineNumberA, EndingInvoiceLineNumberA);

			AddLineGroupingIfNecessary(invoiceLine, BeginningInvoiceLineNumberB, EndingInvoiceLineNumberB);

			AddLineGroupingIfNecessary(invoiceLine, BeginningInvoiceLineNumberC, EndingInvoiceLineNumberC);

			AddLineGroupingIfNecessary(invoiceLine, BeginningInvoiceLineNumberD, EndingInvoiceLineNumberD);

			AddLineGroupingIfNecessary(invoiceLine, BeginningInvoiceLineNumberE, EndingInvoiceLineNumberE);
		}

		void AddLineGroupingIfNecessary(JobComInvoiceLine invoiceLine, int startNo, int endNo)
		{
			if (startNo > 0)
			{
				InvoiceLineGroupingRange result = invoiceLine.LineGroupingRanges.AddNew();
				result.US_StartSequenceNo = (short)startNo;

				if (endNo > 0)
				{
					result.US_EndSequenceNo = (short)endNo;
				}
			}
		}

		ZString IBIRDTariffRecord.Tariff
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
