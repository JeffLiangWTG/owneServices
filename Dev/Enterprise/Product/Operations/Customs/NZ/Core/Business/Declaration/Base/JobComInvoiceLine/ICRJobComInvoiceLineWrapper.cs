using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using ClassificationTypeList = Enterprise.Customs.NZ.TradeSingleWindow.ClassificationTypeList;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class ICRJobComInvoiceLineWrapper : IICRConsignmentItem
	{
		public ICRJobComInvoiceLineWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine cannot be null");
			dangerousGoods = this.invoiceLine.UNDGs.Count > 0 ? this.invoiceLine.UNDGs.FirstItemForBinding[0] : null;
		}
		readonly JobComInvoiceLine invoiceLine;
		readonly UNDGDataItem dangerousGoods;

		ZShort IICRConsignmentItem.SequenceNumber => invoiceLine.JI_LineNo;

		ZBool IICRConsignmentItem.IsEmptyContainer => false;

		ZString IICRConsignmentItem.GoodsDescription => invoiceLine.JI_Description;

		ZString IICRConsignmentItem.IdentityNumber => ZString.Empty;

		ZDecimal IICRConsignmentItem.Value => invoiceLine.JI_LinePrice;

		ZString IICRConsignmentItem.Currency => invoiceLine.JI_RX_NKLinePriceCurr;

		ZString IICRConsignmentItem.IdentityType => ZString.Empty;

		public IEnumerable<IClassification> Classifications
		{
			get
			{
				if (!invoiceLine.JI_Tariff.IsEmpty)
				{
					var classification = new ICRClassification(invoiceLine.JI_Tariff, ClassificationTypeList.Codes.HS);
					yield return classification;
				}

				if (dangerousGoods != null)
				{
					var classification = new ICRClassification(dangerousGoods.UNDGSubstance?.DG_Code ?? ZString.Empty, ClassificationTypeList.Codes.SSO);
					yield return classification;
				}
			}
		}

		ZBool IICRConsignmentItem.SendFlashpointTemp => dangerousGoods != null;

		ZDecimal IICRConsignmentItem.FlashpointTempInCelsius => dangerousGoods?.DI_DGFlashPoint ?? ZDecimal.Zero;

		ITemperatureRequirements IICRConsignmentItem.Temperatures
		{
			get { return invoiceLine.JI_TemperatureDetailsToBeSent ? new TemperatureWrapper(invoiceLine.JI_StorageTemp, invoiceLine.JI_MinTemp, invoiceLine.JI_MaxTemp) : null; }
		}

		ZDecimal IICRConsignmentItem.GrossWeightInKg => invoiceLine.GrossWeightInKG;

		ZString IICRConsignmentItem.GoodsOriginCountry => invoiceLine.JI_CountryOfOrigin;

		ZInt IICRConsignmentItem.PackageQty => invoiceLine.JI_InvoiceQuantity.ToZInt();

		ZString IICRConsignmentItem.PackageType => invoiceLine.JI_InvoiceUQ;

		ZString IICRConsignmentItem.ContainerNumber
		{
			get
			{
				var invoiceLineContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Where(x => x.IsForInvoiceLine).OrderBy(x => x.ContainerNumber).FirstOrDefault();
				return invoiceLineContainer?.ContainerNumber ?? ZString.Empty;
			}
		}

		ZString IICRConsignmentItem.MPIApprovedSystemNumber => ZString.Empty;
	}
}
