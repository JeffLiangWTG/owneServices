using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class OrgSupplierPartCollection : Customs.Business.OrgSupplierPartCollection
	{
		public OrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, JobComInvoiceLine invoiceLine, bool isExport)
			: base(factory, invoiceLine, isExport)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, BaseJobComInvoiceLine invoiceLine, OrgHeader supplier, OrgHeader owner, bool isExport)
			: base(factory, invoiceLine, supplier, owner, isExport)
		{
		}

		public new OrgSupplierPart AddNew()
		{
			return (OrgSupplierPart)base.AddNew();
		}

		public new OrgSupplierPart this[int index]
		{
			get { return (OrgSupplierPart)Elements[index]; }
		}

		protected override void AddPivotWithAdditionalLineDetailsCore(Customs.Business.OrgSupplierPart part, BaseJobComInvoiceLine invoiceLine)
		{
			base.AddPivotWithAdditionalLineDetailsCore(part, invoiceLine);
			var orgSupplierPart = (OrgSupplierPart)part;
			var jobComInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var pivot = orgSupplierPart.PivotsForBinding.Cast<CusClassPartPivot>().FirstOrDefault();
			if (pivot != null)
			{
				CopyValuesFromInvoiceLine(pivot, jobComInvoiceLine);
			}
		}

		protected override void AddAdditionalLineDetails(Customs.Business.OrgSupplierPart part, BaseJobComInvoiceLine invoiceLine)
		{
			base.AddAdditionalLineDetails(part, invoiceLine);
			var orgSupplierPart = (OrgSupplierPart)part;
			var jobComInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var trademarkStorageDoc = jobComInvoiceLine.TrademarkStorageDoc;
			if (trademarkStorageDoc != null)
			{
				orgSupplierPart.DocManagerInfo()?.AddFileOrDocument(trademarkStorageDoc.ImageData, trademarkStorageDoc.FileName, trademarkStorageDoc.DocType);
			}
		}

		void CopyValuesFromInvoiceLine(CusClassPartPivot pivot, JobComInvoiceLine invoiceLine)
		{
			pivot.CI_NDescription = invoiceLine.JI_NDescription;
			pivot.CI_Description = invoiceLine.JI_Description;
			pivot.CI_PartPivotUOM = invoiceLine.JI_InvoiceUQ;

			CopyValuesFromInvoiceLineToCarInfo(pivot, invoiceLine);

			pivot.CI_Compositions = invoiceLine.JI_Compositions;
			pivot.CI_CustomsOwnerPartNo = invoiceLine.JI_CustomsOwnerPartNo;
			pivot.CI_CustomsSupplierPartNo = invoiceLine.JI_CustomsSupplierPartNo;
			pivot.CI_RN_NKCountryOfOrigin = invoiceLine.JI_CountryOfOrigin;
			pivot.CI_Price = invoiceLine.JI_EnteredUnitPrice;
			pivot.CI_PriceCurr = invoiceLine.JI_RX_NKLinePriceCurr;
			if (pivot.IsImportClassification)
			{
				pivot.CI_DutyTreatment = invoiceLine.JI_Procedure.Left(2);
				pivot.CI_TariffAdditionalCode = invoiceLine.JI_TariffAdditionalCode;
				pivot.CI_AlcoholPercentage = invoiceLine.JI_AlcoholPercentage;
			}
			else if (pivot.IsExportClassification)
			{
				pivot.CI_ModeOfStatistics = invoiceLine.JI_Procedure.Left(2);
			}

			foreach (PermitCusSupporting item in invoiceLine.PermitCusSupportingCollection)
			{
				var productPermitCusSupportingCollection = pivot.ProductPermitCusSupportingCollection.AddNew();
				productPermitCusSupportingCollection.CSI_ReferenceNumber = item.CSI_ReferenceNumber;
				productPermitCusSupportingCollection.CSI_LineNo = item.CSI_LineNo;
			}

			foreach (AssignedJobComInvLineRefs item in invoiceLine.AssignedJobComInvLineRefsCollection)
			{
				var assignedCusClassPartPivotRefCollection = pivot.AssignedCusClassPartPivotRefCollection.AddNew();
				assignedCusClassPartPivotRefCollection.CIR_ReferenceNumber = item.JG_ReferenceNumber;
			}
		}

		void CopyValuesFromInvoiceLineToCarInfo(CusClassPartPivot pivot, JobComInvoiceLine invoiceLine)
		{
			if (pivot.IsCarRelatedTariff)
			{
				pivot.CI_CarType = invoiceLine.JI_CarType;
				pivot.CI_Transmission = invoiceLine.JI_Transmission;
				pivot.CI_EngineType = invoiceLine.JI_EngineType;
				pivot.CI_LHD = invoiceLine.JI_LHD;
				pivot.CI_HasCatalystConverter = invoiceLine.JI_HasCatalystConverter;
				pivot.CI_EquipmentPrintMode = invoiceLine.JI_EquipmentPrintMode;
				pivot.CI_CarCondition = invoiceLine.JI_CarCondition;
				pivot.CI_ModelYear = invoiceLine.JI_ModelYear;
				pivot.CI_Displacement = invoiceLine.JI_Displacement;
				pivot.CI_NumberOfDoor = invoiceLine.JI_NumberOfDoor;
				pivot.CI_Seats = invoiceLine.JI_Seats;
				pivot.CI_Cylinders = invoiceLine.JI_Cylinders;
				pivot.CI_Gears = invoiceLine.JI_Gears;
			}
		}
	}
}
