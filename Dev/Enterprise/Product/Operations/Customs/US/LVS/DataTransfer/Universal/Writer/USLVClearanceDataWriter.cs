using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class USLVClearanceDataWriter : TopLevelDataObjectWriter<CusUSLVClearance, Shipment>
	{
		public USLVClearanceDataWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		bool IsWritingConsolidatedDeclaration => writeManager.Action?.ActionType.Equals(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage) ?? false;

		protected override void PopulateDataObject(CusUSLVClearance sourceBO, Shipment dataObject)
		{
			PopulateWayBillNumber(sourceBO, dataObject);
			PopulateCusUSLVConsignmentCollection(sourceBO, dataObject);
			PopulateCommercialInfo(sourceBO, dataObject);
			PopulateShipmentBasicInfos(sourceBO, dataObject);
			PopulateOrganizationAddressCollection(sourceBO, dataObject);
			PopulateAdditionalBillCollection(sourceBO, dataObject);
			PopulateAddInfoCollection(sourceBO, dataObject);
			PopulateDateCollection(sourceBO, dataObject);
		}

		void PopulateCommercialInfo(CusUSLVClearance sourceBO, Shipment dataObject)
		{
			if (IsWritingConsolidatedDeclaration)
			{
				var commercialInfo = new CommercialInfo();
				commercialInfo.SetWriterStrategy(writeManager.WriterStrategy);
				commercialInfo.SetCommercialInvoiceCollection(() =>
				{
					var invoices = new DataObjectList<CommercialInvoiceHeader>();
					var toDeclarationWriter = new USLVConsignmentToDeclarationDataWriter(writeManager);
					var consignments = sourceBO.CusUSLVConsignmentBatches.CurrentBatch ?? [];
					foreach (var consignment in consignments)
					{
						invoices.AddRange(toDeclarationWriter.PopulateCommercialInvoices(consignment, null, invoices.Count + 1));
					}

					return invoices;
				});

				dataObject.CommercialInfo = commercialInfo;
			}
		}

		public void PopulateShipmentBasicInfos(CusUSLVClearance uSLVClearance, Shipment shipment)
		{
			shipment.TransportMode = new CodeDescriptionPair() { Code = uSLVClearance.ULH_TransportMode };
			shipment.CustomsContainerMode = new ContainerMode() { Code = uSLVClearance.ULH_ContainerMode };
			shipment.PortOfLoading = new UNLOCO { Code = uSLVClearance.ULH_RL_NKPortOfLoading };
			shipment.PortOfDischarge = new UNLOCO { Code = uSLVClearance.ULH_RL_NKPortOfDischarge };
			shipment.VesselName = uSLVClearance.ULH_ConveyanceName;
			shipment.VoyageFlightNo = uSLVClearance.ULH_VoyageFlightNo;
			shipment.Branch = Branch.New(uSLVClearance.Branch);
		}

		void PopulateWayBillNumber(CusUSLVClearance sourceBO, Shipment dataObject)
		{
			dataObject.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
			dataObject.WayBillNumber = sourceBO.ULH_MasterBill;
		}

		public void PopulateDateCollection(CusUSLVClearance uSLVClearance, Shipment shipment)
		{
			shipment.SetDateCollection(() => new List<Date>()
			{
				new Date
				{
					Type = DateType.LoadingDate,
					Value = uSLVClearance.ULH_DepartureDate,
					IsEstimate = ZBool.False,
				},
				new Date
				{
					Type = DateType.DischargeDate,
					Value = uSLVClearance.ULH_DischargeDate,
					IsEstimate = ZBool.False,
				}
			});
		}

		public void PopulateAdditionalBillCollection(CusUSLVClearance sourceBO, Shipment shipment)
		{
			if (shipment.AdditionalBillCollection != null || shipment.SetAdditionalBillCollection(() => new List<AdditionalBill>()))
			{
				var additionalBill = new AdditionalBill
				{
					BillNumber = sourceBO.ULH_MasterBill,
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master, },
				};

				var listAddInfo = new List<UAddInfo>
				{
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.UI_NKBillIssuerSCAC,
						Value = sourceBO.ULH_MasterBillIssuerSCAC,
					}
				};
				additionalBill.AddInfoCollection = listAddInfo;

				shipment.AdditionalBillCollection.Add(additionalBill);
			}
		}

		public void PopulateAddInfoCollection(CusUSLVClearance sourceBO, Shipment shipment)
		{
			if (shipment.AddInfoCollection != null || shipment.SetAddInfoCollection(() => new List<UAddInfo>()))
			{
				var listAddInfo = new List<UAddInfo>
				{
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.EntryFilerCode,
						Value = sourceBO.ULH_EntryFilerCode
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.US_NKLocationOfGoods,
						Value = sourceBO.ULH_US_NKLocationOfGoods
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.US_NKCentralizedExamSite,
						Value = sourceBO.ULH_US_NKCentralizedExamSite
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.MasterWayBillIssuerSCAC,
						Value = sourceBO.ULH_MasterBillIssuerSCAC
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.UI_NKCarrierSCAC,
						Value = sourceBO.ULH_CarrierSCAC
					},
					new UAddInfo
					{
						Key =  LVSConstants.AddInfoConstants.SchDLoading,
						Value = sourceBO.ULH_PortOfLoading
					},
					new UAddInfo
					{
						Key =  LVSConstants.AddInfoConstants.SchDEntry,
						Value = sourceBO.ULH_PortOfEntry
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.SchDArrival,
						Value = sourceBO.ULH_PortOfDischarge
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.EntryMode,
						Value = sourceBO.ULH_RemoteLocationFiling ? LVSConstants.AddInfoConstants.RemoteLocationFiling : string.Empty
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.EntryDate,
						Value = BaseAddInfo.GetStringRepresentation(sourceBO.ULH_EntryDate)
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.IORType,
						Value = sourceBO.ULH_IORType
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.IORReference,
						Value = sourceBO.ULH_IORReference
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.FilerName,
						Value = sourceBO.ULH_ContactName
					},
					new UAddInfo
					{
						Key = LVSConstants.AddInfoConstants.FilerPhoneNumber,
						Value = sourceBO.ULH_ContactPhone
					},
				};

				if (IsWritingConsolidatedDeclaration)
				{
					listAddInfo.Add(new UAddInfo
					{
						Key = USAddInfoSchema.Constants.US_ConsolACE.Substring(3),
						Value = ZBool.True.ToString()
					});

					listAddInfo.Add(new UAddInfo
					{
						Key = USAddInfoSchema.Constants.US_EnableENS.Substring(3),
						Value = ZBool.True.ToString()
					});

					listAddInfo.Add(new UAddInfo
					{
						Key = USAddInfoSchema.Constants.US_EntryType.Substring(3),
						Value = EntryTypeList.Codes.InformalFreeDutiable
					});

					listAddInfo.Add(new UAddInfo
					{
						Key = USAddInfoSchema.Constants.US_ConsolidatedInformalIndicator.Substring(3),
						Value = ConsolidatedInformalList.Codes.Personal
					});
				}

				shipment.AddInfoCollection.AddRange(listAddInfo);
			}
		}

		void PopulateCusUSLVConsignmentCollection(CusUSLVClearance usLVClearance, Shipment shipment)
		{
			var consignments = IsWritingConsolidatedDeclaration
				? usLVClearance.CusUSLVConsignmentBatches.CurrentBatch ?? []
				: usLVClearance.CusUSLVConsignments.AsEnumerable();

			var data = ProcessCollection(consignments, new USLVConsignmentDataWriter(writeManager));
			shipment.SetSubShipmentCollection(() => data != null ? [.. data] : null);
		}

		public void PopulateOrganizationAddressCollection(CusUSLVClearance uSLVClearance, Shipment shipment)
		{
			if (uSLVClearance.Client != null)
			{
				shipment.AddOrgAddress(writeManager, uSLVClearance.Client, AddressTypes.SendersLocalClient);
			}

			if (uSLVClearance.Importer != null)
			{
				shipment.AddOrgAddress(writeManager, uSLVClearance.Importer, DocAddressType.ImporterOfRecord);
				if (IsWritingConsolidatedDeclaration)
				{
					shipment.AddOrgAddress(writeManager, uSLVClearance.Importer, DocAddressType.ImporterDocumentaryAddress);
				}
			}
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.USCustomsLowValueEntriesClearance;
	}
}

