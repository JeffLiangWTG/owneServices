using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Country = Enterprise.Core.Constants.CountryCodes;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.NZ.Business.Data
{
	public class NZDeclarationValueObjectDataAdapter : DeclarationValueObjectDataAdapter, Integration.Customs.NZ.INZDeclarationValueObjectDataAdapter
	{
		public NZDeclarationValueObjectDataAdapter()
		{
		}

		public NZDeclarationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		#region Overrides

		protected override string AddInfoPrefix
		{
			get { return "ZN_"; }
		}

		protected override InvoiceValueObjectDataAdapter GetNewInvoiceAdapter(Customs.Business.BaseJobDeclaration jobDec)
		{
			return new NZInvoiceValueObjectDataAdapter(jobDec);
		}

		protected override void SetShipmentTypeDetailsCore(Customs.Business.BaseJobDeclaration jobDec)
		{
			jobDec.JE_MessageType = jobDec.JE_RL_NKPortOfArrival.StartsWith(Country.NewZealand) ? "IMP" : "EXP";
		}

		protected override bool AllowExportOfFirstArrivalPort
		{
			get { return false; }
		}

		#endregion

		#region Import

		protected override void ImportMasterbillDetails(Customs.Business.BaseJobDeclaration jobDec, Xsd.Consol consol, IValueObjectImportContext context)
		{
			Xsd.ConsolIdentifier consolIdentifier = consol.ConsolIdentifier.FindFirst(Xsd.ConsolIdentifierType.MasterWaybill);
			if (consolIdentifier != null)
			{
				context.SetPropertyInfoValue(jobDec.JE_MasterBillInfo, consolIdentifier.Value, consolIdentifier.ValueSpecified, "Declaration Masterbill");
			}
		}

		protected override void ImportDeclarationAdditionalInfo(Customs.Business.BaseJobDeclaration jobDec, Xsd.AdditionalCustomsInformationCollection addCustomsDetails, IValueObjectImportContext context)
		{
			JobDeclaration nZJobDec = (JobDeclaration)jobDec;
			ImportAddInfos(((IHaveNZAddInfo)nZJobDec).AddInfo.ZPropertyInfoHash, addCustomsDetails, context);

			foreach (Xsd.AdditionalCustomsInformation addCustomsInfo in addCustomsDetails)
			{
				if (addCustomsInfo.CustomsDetailType == "ProcessPort")
				{
					nZJobDec.JE_RL_NKProcessingPort = addCustomsInfo.CustomsDetailValue;
				}
			}
		}

		protected override void AddHouseBillToHouseBills(Customs.Business.BaseJobDeclaration jobDec, ZString houseBillNo, ZString masterBillNo, IValueObjectImportContext context)
		{
			Customs.Business.Bill houseBill = jobDec.Bills.FindByBillNumberAndType(houseBillNo, Customs.Business.BillTypeList.Codes.HouseBill);
			if (houseBill == null)
			{
				//empty one
				houseBill = jobDec.Bills.FindByBillNumberAndType(ZString.Empty, Customs.Business.BillTypeList.Codes.HouseBill);
				if (houseBill == null)
				{
					houseBill = jobDec.Bills.AddNew();
					houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
				}
				context.SetPropertyInfoValueIfValueNotEmpty(houseBill.CU_BillNumInfo, houseBillNo);
			}
		}

		#endregion

		#region Export

		protected override void ExportMasterbillDetails(Xsd.Consol toConsol, Customs.Business.BaseJobDeclaration jobDec)
		{
			if (!jobDec.JE_MasterBill.IsEmpty)
			{
				Xsd.ConsolIdentifier consolIdentifier = toConsol.ConsolIdentifier.AddNew();
				consolIdentifier.IsSpecified = true;
				consolIdentifier.ConsolIdentifierTypeSpecified = true;
				consolIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
				consolIdentifier.Value = jobDec.JE_MasterBill;
			}
		}

		protected override Xsd.InvoiceHeaderCollection ExportInvoiceHeaders(Customs.Business.BaseJobDeclaration jobDec, IValueObjectExportContext context)
		{
			Xsd.InvoiceHeaderCollection result = base.ExportInvoiceHeaders(jobDec, context);

			GroupInvoiceValueObjectDataAdapter groupInvoiceXml = GetNewGroupInvoiceAdapter(jobDec);
			foreach (Customs.Business.BaseJobComInvoiceGroupHeader groupHeader in jobDec.JobComInvoiceGroupHeaders)
			{
				ExportInvoiceHeaders(groupInvoiceXml, groupHeader.JobComInvoiceGroupHeaders, result, context);
			}

			return result;
		}

		protected override void ExportHousebillDetails(Xsd.Shipment toShipment, Customs.Business.BaseJobDeclaration jobDec)
		{
			foreach (Bill bill in jobDec.Bills)
			{
				if (bill.CU_BillType == Customs.Business.BillTypeList.Codes.HouseBill)
				{
					Xsd.ShipmentIdentifier shipmentIdentifier = toShipment.ShipmentIdentifier.AddNew();
					shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
					shipmentIdentifier.Value = bill.CU_HouseBill;
				}
			}
		}

		protected override void ExportBillContainerPacks(Xsd.Declaration declarationXsd, Customs.Business.BaseJobDeclaration jobDec)
		{
			foreach (Package package in jobDec.Packages)
			{
				Xsd.DeclarationBillContainerPack billContPack = declarationXsd.BillContainerPacks.AddNew();
				if (package.Bill != null)
				{
					billContPack.BillNumber = package.Bill.CU_BillNum;
				}
				billContPack.ContainerNumber = package.CW_ContainerNoOrEquipmentNo;
				billContPack.PackQty.Value = (decimal)package.CW_PackQty;
				billContPack.PackQty.DimensionType = package.CW_PackType;
			}
		}

		protected override void ExportDeclarationAdditionalInfo(Xsd.AdditionalCustomsInformationCollection addCustomsDetails, Customs.Business.BaseJobDeclaration jobDec, INotifications notification)
		{
			JobDeclaration nZJobDec = jobDec as JobDeclaration;
			if (nZJobDec != null)
			{
				ExportAddInfos(addCustomsDetails, ((IHaveNZAddInfo)nZJobDec).AddInfo.ZPropertyInfoHash);

				if (!nZJobDec.JE_RL_NKProcessingPort.IsEmpty)
				{
					Xsd.AdditionalCustomsInformation addCustomsDetail = addCustomsDetails.AddNew();
					addCustomsDetail.CustomsDetailType = "ProcessPort";
					addCustomsDetail.CustomsDetailValue = nZJobDec.JE_RL_NKProcessingPort;
				}
			}
			else
			{
				// This could be an Enterprise.Customs.NZ.Business.JobDeclaration (Which doesn't have the AddInfo property) rather than a
				// Enterprise.Customs.NZ.Business.FormalEntry.JobDeclaration
				NZAddInfo nZAddInfo = new NZAddInfo(jobDec, jobDec.JE_AddInfoInfo);
				ExportAddInfos(addCustomsDetails, nZAddInfo.ZPropertyInfoHash);
			}
		}

		protected override InvoicesGeneratorFromXSD GetNewInvoicesGenerator(Customs.Business.BaseJobDeclaration jobDec)
		{
			return new NZInvoicesGeneratorFromXSD(jobDec);
		}

		#endregion

	}
}
