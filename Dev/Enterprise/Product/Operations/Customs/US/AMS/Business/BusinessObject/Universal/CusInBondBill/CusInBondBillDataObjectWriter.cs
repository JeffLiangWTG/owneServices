using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class CusInBondBillDataObjectWriter : DataTransfer.Universal.CusInBondBillDataObjectWriter
	{
		public CusInBondBillDataObjectWriter(IDataWritingManager writeManager, InBondDataObjectWriterHelper helper)
			: base(writeManager, helper)
		{
		}

		protected new InBondDataObjectWriterHelper Helper
		{
			get { return (InBondDataObjectWriterHelper)base.Helper; }
		}

		protected override void PopulateInBondSpecificData(Customs.Business.CusInBondBill billBO, AdditionalBill billData)
		{
			base.PopulateInBondSpecificData(billBO, billData);
			var amsBillBO = (CusInBondBill)billBO;
			billData.BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			billData.BillNumber = amsBillBO.B0_MasterBillNumber;
			billData.PackType = ListHelper.GetWithDescription<PackageType>(billBO.B0_ManifestUQ, amsBillBO.Lookups.ManifestUnitList);
			Helper.PopulateDispositions(amsBillBO, billData);
		}

		protected override List<AddInfo> PopulateAddInfosData(Customs.Business.CusInBondBill billBO, AdditionalBill billData)
		{
			var amsBillBO = (CusInBondBill)billBO;
			var addInfoCollection = base.PopulateAddInfosData(billBO, billData) ?? new List<AddInfo>();
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.ShipmentType, Value = amsBillBO.B0_ShipmentType });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.IssuerCode, Value = amsBillBO.B0_IssuerCode });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.BillStatus, Value = amsBillBO.B0_BillStatus });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.PortOfLading, Value = amsBillBO.B0_RL_NKPortOfLading });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.PortOfLadingScheduleK, Value = amsBillBO.B0_PortOfLadingKCode });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.PlaceOfReceiptScheduleD, Value = amsBillBO.B0_PlaceOfReceipt });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.TransportModeToPortOfLading, Value = amsBillBO.B0_TransportModeToPortOfLading });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.LastForeignPort, Value = amsBillBO.B0_RL_NKLastForeignPort });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.LastForeignPortScheduleK, Value = amsBillBO.B0_LastForeignPortKCode });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.ForeignPortOfContract, Value = amsBillBO.B0_RL_NKForeignPortOfContract });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.ForeignPortOfContractScheduleK, Value = amsBillBO.B0_ForeignPortOfContractKCode });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.Weight, Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(amsBillBO.B0_Weight) });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.WeightUnit, Value = amsBillBO.B0_WeightUQ });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.Volume, Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(amsBillBO.B0_Volume) });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.VolumeUnit, Value = amsBillBO.B0_VolumeUQ });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.PaymentMethod, Value = amsBillBO.B0_TransportPaymentMethod });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.PortOfUnlading, Value = amsBillBO.B0_RL_NKInBondPortOfDest });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.PortOfUnladingScheduleD, Value = amsBillBO.B0_InBondPortOfDestDCode });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.EstimatedUnloadDate, Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(amsBillBO.B0_DateOfDischarge) });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.MasterInBondIndicator, Value = amsBillBO.B0_MasterInBondIndicator ? YesNoDefaultList.Codes.Yes : string.Empty });
			addInfoCollection.Add(new AddInfo() { Key = Constants.Bill.AddInfo.FIRMS, Value = amsBillBO.B0_Firms });
			return addInfoCollection;
		}

		protected override List<CustomsReference> PopulateCustomsReferenceData(Customs.Business.CusInBondBill billBO, AdditionalBill billData)
		{
			var amsBillBO = (CusInBondBill)billBO;
			var customsReferenceCollection = base.PopulateCustomsReferenceData(billBO, billData) ?? new List<CustomsReference>();
			foreach (var referenceDetailBO in Helper.Load<CusInbondBillAddRef>(amsBillBO.ShipmentReferenceDetails.CompleteFilter).OrderBy(x => x.BR_Qualifier + x.BR_ReferenceNum))
			{
				customsReferenceCollection.Add(new CustomsReference()
				{
					Type = new CodeDescriptionPair() { Code = Constants.AdditionalReference.Type, Description = Constants.AdditionalReference.TypeDescription },
					SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(referenceDetailBO.BR_Qualifier, referenceDetailBO.Lookups.ReferenceList),
					Reference = referenceDetailBO.BR_ReferenceNum
				});
			}

			foreach (var secondaryNotificationBO in Helper.Load<SecondaryNotifyParty>(amsBillBO.SecondaryNotifyParties.CompleteFilter))
			{
				customsReferenceCollection.Add(new CustomsReference()
				{
					Type = new CodeDescriptionPair() { Code = Constants.SecondaryNotifyParty.Type, Description = Constants.SecondaryNotifyParty.TypeDescription },
					SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(secondaryNotificationBO.CY_Data, secondaryNotificationBO.Lookups.SCACOrFIRMSList),
					Reference = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(secondaryNotificationBO.CY_Order)
				});
			}

			return customsReferenceCollection;
		}

		protected override List<OrganizationAddress> PopulateOrganizations(Customs.Business.CusInBondBill billBO, AdditionalBill billData)
		{
			var organizationCollection = base.PopulateOrganizations(billBO, billData) ?? new List<OrganizationAddress>();
			var amsBillBO = (CusInBondBill)billBO;
			organizationCollection = ProcessCollection(amsBillBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager));
			return organizationCollection;
		}
	}
}
