using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataTransfer.Universal.Extensions;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Schema;
using DeclarationDetail = Enterprise.Customs.US.DataTransfer.Universal.WarehouseCustomsLineDetails.DeclarationDetail;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class WarehouseCustomsLineDetailsProvider : WarehouseCustomsLineDetailsProvider<JobDeclaration, JobComInvoiceHeader>
	{
		public WarehouseCustomsLineDetailsProvider(Shipment shipment)
			: base(shipment)
		{
		}

		protected override IWarehouseCustomsLineDetails GetNewLineDetail(CommercialInvoiceLine invoiceLine, CommercialInvoiceHeader invoice)
		{
			return new WarehouseCustomsLineDetails(Factory, invoiceLine, invoice, PackageDetailDictionary, (DeclarationDetail)GetFallbackDetail(invoice));
		}

		protected override ITableSchema GetDeclarationAddInfoSchema() => USAddInfoSchema.Instance;

		protected override ITableSchema GetInvoiceAddInfoSchema() => USAddInfoSchema.Instance;

		protected override IEnumerable<string> GetInvoiceLineAddInfosApplicableForInwardWarehousing() => Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing();

		IDictionary<ZString, ZInt> PackageDetailDictionary
		{
			get
			{
				if (packageDetailDictionary == null)
				{
					packageDetailDictionary = new Dictionary<ZString, ZInt>();
					if (shipment.AddInfoGroupCollection != null)
					{
						foreach (var packageDetail in shipment.AddInfoGroupCollection.Where(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USWHSPack && x.AddInfoCollection != null))
						{
							var packageQty = packageDetail.AddInfoCollection.GetZIntValue(USWHSPackAddInfo.Schema.US_PackageQty.Substring(3));
							var packageID = packageDetail.AddInfoCollection.GetZStringValue(USWHSPackAddInfo.Schema.US_PackageReference.Substring(3));
							if (packageQty.HasValue && packageID.HasValue && !packageDetailDictionary.ContainsKey(packageID.Value))
							{
								packageDetailDictionary.Add(packageID.Value, packageQty.Value);
							}
						}
					}
				}
				return packageDetailDictionary;
			}
		}
		IDictionary<ZString, ZInt> packageDetailDictionary;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override WarehouseCustomsFallbackDetail GetNewDeclarationDetail()
		{
			var messageType = shipment.MessageType.GetCodeAsUpperCase();
			var isImport = JobMessageTypeList.IsImport(messageType);
			var declarationDetail = new DeclarationDetail()
			{
				IsImportByExternalBroker = messageType == JobMessageTypeList.Codes.ImportByExternalBroker,
				IsFTZAdmission = messageType == JobMessageTypeList.Codes.FTZ,
				IsInBond = messageType == CusInBondApplicationCodeList.Codes.InBond,
				SupplierAddress = shipment.OrganizationAddressCollection.FindBestSupplierMatch(),
				InvoiceLineAddInfosApplicableForInwardWarehousing = InvoiceLineAddInfosApplicableForInwardWarehousing
			};
			if (shipment.AddInfoCollection != null)
			{
				var isENSEnabled = shipment.AddInfoCollection.GetZBoolValue(JobDeclaration.Schema.US_EnableENS.Substring(3)).GetValueOrDefault();
				var entryType = shipment.AddInfoCollection.GetZStringValue(JobDeclaration.Schema.US_EntryType.Substring(3)).GetValueOrDefault();
				if (isImport && isENSEnabled && !entryType.IsEmpty)
				{
					declarationDetail.IsExWarehouse = EntryTypeList.IsExWarehouseType(entryType);
					declarationDetail.IsConsumptionFTZ = !declarationDetail.IsExWarehouse && entryType == EntryTypeList.Codes.ConsumptionFTZ;
				}
				declarationDetail.EntryFilerCode = shipment.AddInfoCollection.GetZStringValue(JobDeclaration.Schema.US_EntryFilerCode.Substring(3));
				if (declarationDetail.IsExWarehouse)
				{
					declarationDetail.WHSEntryFilerCode = shipment.AddInfoCollection.GetZStringValue(JobDeclaration.Schema.US_WHSEntryFilerCode.Substring(3));
					declarationDetail.WHSEntryNumber = shipment.AddInfoCollection.GetZStringValue(JobDeclaration.Schema.US_WHSEntryNumber.Substring(3));
				}
				else if (declarationDetail.IsFTZAdmission)
				{
					var admissionType = shipment.AddInfoCollection.GetZStringValue(JobDeclaration.Schema.US_F_AdmissionType.Substring(3));
					if (admissionType.HasValue)
					{
						declarationDetail.FromOtherFTZ = admissionType.Value == FTZAdmissionTypeCodeList.Codes.ZoneToZone;
					}
					if (shipment.EntryNumberCollection != null)
					{
						var ftzAdmissionNumber = shipment.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone && x.CountryOfIssue.GetCodeAsUpperCase() == Core.Constants.CountryCodes.UnitedStates && x.Number.HasValue);
						if (ftzAdmissionNumber != null)
						{
							declarationDetail.FTZAdmissionNumber = FTZAdmissionNumberRetriever.FTZAdmissionNumberFormatted(ftzAdmissionNumber.Number.ToString());
						}
					}
				}
				else
				{
					if (shipment.EntryNumberCollection != null)
					{
						var entryNumber = shipment.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == CusEntryHeaderMessageTypeList.Codes.EntrySummary && x.CountryOfIssue.GetCodeAsUpperCase() == Core.Constants.CountryCodes.UnitedStates);
						if (entryNumber != null)
						{
							declarationDetail.EntryNumber = entryNumber.Number;
						}
					}
				}
			}
			if (declarationDetail.IsConsumptionFTZ)
			{
				declarationDetail.OutwardType = OutwardType.Consumption;
			}
			else if (declarationDetail.IsInBond)
			{
				if (shipment.InBondMoveHeaderCollection != null && shipment.InBondMoveHeaderCollection.Count == 1)
				{
					var inBondMoveHeader = shipment.InBondMoveHeaderCollection[0];
					if (inBondMoveHeader != null)
					{
						if (inBondMoveHeader.EntryNumberCollection != null)
						{
							var inBondEntryNumber = inBondMoveHeader.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == CusEntryHeaderMessageTypeList.Codes.InBond && (x.CountryOfIssue == null || x.CountryOfIssue.GetCodeAsUpperCase() == Core.Constants.CountryCodes.UnitedStates));
							if (inBondEntryNumber != null)
							{
								declarationDetail.InBondNumber = inBondEntryNumber.Number;
							}
						}
						var entryType = inBondMoveHeader.EntryType.GetCodeAsUpperCase();
						if (entryType == InbondCommonTypeList.Codes._2TransportandExport || entryType == InbondCommonTypeList.Codes._3ImmediateExport)
						{
							declarationDetail.OutwardType = OutwardType.Exports;
						}
						else if (inBondMoveHeader.MoveToFTZ.GetCodeAsUpperCase() == YesNoDefaultList.Codes.Yes)
						{
							declarationDetail.OutwardType = OutwardType.ToOtherFTZ;
						}
					}
				}
			}
			return declarationDetail;
		}
	}
}
