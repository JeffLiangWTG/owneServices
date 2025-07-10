using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Constants = Enterprise.Customs.US.ISF.Business.ISFConstants;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Writer
{
	class ISFHeaderDataObjectWriter : TopLevelDataObjectWriter<CusISFHeader, Shipment>
	{
		public ISFHeaderDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override void PopulateDataObject(CusISFHeader sourceBO, Shipment dataObject)
		{
			PopulateDataObjectCore(sourceBO, dataObject);
		}

		void PopulateDataObjectCore(CusISFHeader header, Shipment shipment)
		{
			var helper = new ISFDataObjectHelper(header);
			shipment.Branch = Branch.New(header.Branch);
			shipment.OwnerRef = header.BF_OwnerReference;

			var refUNLOCOList = header.Factory.GetRefUNLOCOList();
			shipment.PortOfDischarge = ListHelper.GetWithName(header.BF_RL_NKPortOfUnload, refUNLOCOList);
			shipment.PortOfDestination = ListHelper.GetWithName(header.BF_RL_NKPlaceOfDelivery, refUNLOCOList);
			shipment.GoodsValue = header.BF_EstimatedValue;
			shipment.TotalNoOfPacks = header.BF_EstimatedQuantity;
			shipment.TotalNoOfPacksPackageType = ListHelper.GetWithDescription<PackageType>(header.BF_EstimatedQuantityUQ, header.Lookups.PackingingUnitList);
			shipment.TotalWeight = (ZDecimal)header.BF_EstimatedWeight;
			shipment.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(header.BF_EstimatedWeightUQ, header.Lookups.WeightUQList);

			PopulateTransportModeAndContainerMode(header, shipment, helper);
			PopulateEntryNumberCollection(header, shipment, helper);
			PopulateAdditionalReferenceCollection(header, shipment);
			PopulateDateCollection(header, shipment);
			PopulateAddInfoCollection(header, shipment);
			PopulateWayBillNumberAndType(header, shipment, helper);
			PopulateAdditionalBillCollection(header, shipment);
			PopulateCustomizedFieldCollection(header, shipment);
			PopulateOrganizationAddressCollection(header, shipment);
			PopulateLineCollection(header, shipment, helper);
			PopulateContainersCollection(header, shipment);
			PopulateTransportLegCollection(header, shipment);
			PopulateNotes(header, shipment);
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.USImporterSecurityFiling;
		}

		void PopulateTransportModeAndContainerMode(CusISFHeader header, Shipment shipment, ISFDataObjectHelper helper)
		{
			var containerCode = header.BF_TransportMode == TransportModeCodes.Codes.OceanVesselContainerized
								? ContainerModeList.Codes.Containerized
								: ContainerModeList.Codes.NonContainerized;

			shipment.TransportMode = new CodeDescriptionPair() { Code = TransportTypeList.Codes.Sea, Description = TransportTypeList.Codes.Sea };
			shipment.CustomsContainerMode = ListHelper.GetWithDescription<ContainerMode>(containerCode, helper.ContainerModeList);
		}

		void PopulateEntryNumberCollection(CusISFHeader header, Shipment shipment, ISFDataObjectHelper helper)
		{
			shipment.SetEntryNumberCollection(() =>
			{
				var entryNumberCollection = new List<UniversalXml.EntryNumber>();
				if (!header.BF_CustomsReference.IsEmpty)
				{
					entryNumberCollection.Add(new UniversalXml.EntryNumber()
					{
						Type = ListHelper.GetWithDescription<EntryType>(Constants.EntryNumberConstants.ISF, helper.EntryTypeList),
						Number = header.BF_CustomsReference,
						CountryOfIssue = ListHelper.GetWithName<Country>(Core.Constants.CountryCodes.UnitedStates, header.Lookups.Countries)
					});
				}

				foreach (var bill in header.ReferenceDatas)
				{
					if (bill.BB_BillType == BillTypeList.Codes.USCBPEntryNumber)
					{
						entryNumberCollection.Add(new UniversalXml.EntryNumber()
						{
							Type = ListHelper.GetWithDescription<EntryType>(Constants.EntryNumberConstants.ENS, helper.EntryTypeList),
							Number = bill.BB_BillNum,
							CountryOfIssue = ListHelper.GetWithName<Country>(Core.Constants.CountryCodes.UnitedStates, header.Lookups.Countries)
						});
					}
				}

				return entryNumberCollection.Count == 0 ? null : entryNumberCollection;
			});
		}

		void PopulateAdditionalReferenceCollection(CusISFHeader header, Shipment shipment)
		{
			shipment.SetAdditionalReferenceCollection(() =>
			{
				var referenceCollection = new DataObjectList<AdditionalReference>();
				foreach (var bill in header.ReferenceDatas)
				{
					if (bill.BB_BillType != BillTypeList.Codes.MasterBillOfLading
						&& bill.BB_BillType != BillTypeList.Codes.HouseBillOfLading
						&& bill.BB_BillType != BillTypeList.Codes.OceanBillOfLading
						&& bill.BB_BillType != BillTypeList.Codes.USCBPEntryNumber)
					{
						referenceCollection.Add(new AdditionalReference()
						{
							Type = ListHelper.GetWithDescription<EntryType>(bill.BB_BillType, bill.Lookups.BillTypes),
							ReferenceNumber = bill.BB_BillNum
						});
					}
				}

				return referenceCollection.Count == 0 ? null : referenceCollection;
			});
		}

		void PopulateAddInfoCollection(CusISFHeader header, Shipment shipment)
		{
			shipment.SetAddInfoCollection(() =>
			{
				var addInfoCollection = new List<UniversalXml.AddInfo>();
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.EntryType, header.BF_EntryType);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ISFShipmentType, header.BF_ShipmentType);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.CarrierSCAC, header.BF_SCAC);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ActionReason, header.BF_ActionReasonCode);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ImporterIDType, header.BF_ImporterCodeType);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ImporterID, header.BF_ImporterCode);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ImporterName, header.BF_ImporterFullName);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ImporterDOB, header.BF_DateOfBirth.ToShortDateString());
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ImporterIssueCountry, header.BF_CountryOfIssue);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ConsigneeIDType, header.BF_ConsigneeCodeType);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ConsigneeID, header.BF_ConsigneeCode);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ConsigneeName, header.BF_ConsigneeFullName);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ConsigneeDOB, header.BF_ConsigneeDateOfBirth.ToShortDateString());
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ConsigneeIssueCountry, header.BF_ConsigneeCountryOfIssue);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ISFBondHolder, header.BF_BondNumberOrHolder);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ISFBondActivityCode, header.BF_BondActivityCode);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ISFBondType, header.BF_BondType);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ISFSuretyCode, header.BF_SuretyCode);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ISFBondRefNo, header.BF_BondReferenceNumber);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.ISFShipmentSubType, header.BF_ShipmentSubType);
				AddToShipmentCollection(addInfoCollection, Constants.AddInfoConstants.SendEquipment, header.BF_SendEquipment);
				return addInfoCollection.Count == 0 ? null : addInfoCollection;
			});
		}

		void AddToShipmentCollection(List<UniversalXml.AddInfo> addInfoCollection, ZString field, ZString value)
		{
			if (!value.IsEmpty)
			{
				addInfoCollection.Add(UniversalXml.AddInfo.New(field, value));
			}
		}

		void PopulateDateCollection(CusISFHeader header, Shipment shipment)
		{
			if (!header.BF_LastAcceptedDate.IsEmpty)
			{
				shipment.SetDateCollection(() =>
				{
					var dates = new List<Date>();
					dates.Add(DateType.ISFLastAccepted, ZBool.True, header.BF_LastAcceptedDate);
					return dates;
				});
			}
		}

		void PopulateWayBillNumberAndType(CusISFHeader header, Shipment shipment, ISFDataObjectHelper helper)
		{
			var billType = ZString.Empty;
			var billNumber = header.BF_HouseBill;
			if (billNumber.IsEmpty)
			{
				billNumber = header.BF_OceanBill;
				if (billNumber.IsEmpty)
				{
					billNumber = header.BF_MasterBill;
					billType = WayBillTypeList.Codes.MasterHouse;
				}
				else
				{
					billType = WayBillTypeList.Codes.Master;
				}
			}
			else
			{
				billType = WayBillTypeList.Codes.House;
			}

			shipment.WayBillNumber = billNumber;
			shipment.WayBillType = ListHelper.GetWithDescription<WayBillType>(billType, helper.WayBillTypeList);
		}

		void PopulateAdditionalBillCollection(CusISFHeader header, Shipment shipment)
		{
			shipment.SetAdditionalBillCollection(() =>
			{
				var additionalBillCollection = new List<AdditionalBill>();
				var hwbs = header.ReferenceDatas.Where(x => x.BB_BillType == BillTypeList.Codes.HouseBillOfLading).ToArray();
				var mwbs = header.ReferenceDatas.Where(x => x.BB_BillType == BillTypeList.Codes.OceanBillOfLading).ToArray();
				var mhbs = header.ReferenceDatas.Where(x => x.BB_BillType == BillTypeList.Codes.MasterBillOfLading).ToArray();
				var parentBill = mhbs.Length == 1 ? mhbs[0] : null;

				hwbs.ForEach(bill => PopulateAdditionalBillCollection(additionalBillCollection, bill, parentBill));
				mwbs.ForEach(bill => PopulateAdditionalBillCollection(additionalBillCollection, bill, null));
				mhbs.ForEach(bill => PopulateAdditionalBillCollection(additionalBillCollection, bill, null));

				return additionalBillCollection.Count == 0 ? null : additionalBillCollection;
			});
		}

		void PopulateAdditionalBillCollection(List<AdditionalBill> billCollection, CusISFBill bill, CusISFBill parentBill)
		{
			var additionalBill = new AdditionalBill
			{
				BillType = new WayBillType() { Code = bill.BB_BillType, Description = bill.BB_BillTypeDescription },
				BillNumber = bill.BB_BillNum,
				ParentBillNumber = parentBill != null ? parentBill.BB_BillNum : null
			};

			if (bill.BB_BillType == BillTypeList.Codes.HouseBillOfLading || bill.BB_BillType == BillTypeList.Codes.OceanBillOfLading)
			{
				additionalBill.AddInfoCollection = new List<UniversalXml.AddInfo>()
				{
					UniversalXml.AddInfo.New(Constants.BillAddInfoConstants.BillStatus, bill.BB_CustomsStatus),
					UniversalXml.AddInfo.New(Constants.BillAddInfoConstants.MatchedDate, bill.BB_MatchDate.ToString()),
					UniversalXml.AddInfo.New(Constants.BillAddInfoConstants.FirstMatched, bill.BB_FirstMatchedDate.ToString())
				};
			}
			billCollection.Add(additionalBill);
		}

		void PopulateCustomizedFieldCollection(CusISFHeader header, Shipment shipment)
		{
			shipment.SetCustomizedFieldCollection(() =>
			{
				var customizedFieldCollection = new List<CustomizedField>();
				if (!header.CustomAttribute1.IsEmpty)
				{
					customizedFieldCollection.Add(CustomizedField.New(Constants.CustomizedFieldConstants.CustomAttribOne, header.CustomAttribute1));
				}
				if (!header.CustomAttribute2.IsEmpty)
				{
					customizedFieldCollection.Add(CustomizedField.New(Constants.CustomizedFieldConstants.CustomAttribTwo, header.CustomAttribute2));
				}
				return customizedFieldCollection.Count == 0 ? null : customizedFieldCollection;
			});
		}

		void PopulateOrganizationAddressCollection(CusISFHeader header, Shipment shipment)
		{
			shipment.SetOrganizationAddressCollection(() =>
			{
				var headerOrgAddresses = header.DocAddresses.OfType<ISFDocAddress>().Where(x => x.DocAddressType != DocAddressType.Manufacturer).OrderBy(x => x.E2_AddressSequence);
				return ProcessCollection(headerOrgAddresses, new JobDocAddressDataObjectWriter(writeManager));
			});
			shipment.AddOrgAddress(writeManager, header.Importer, DocAddressType.ImporterDocumentaryAddress);
		}

		void PopulateLineCollection(CusISFHeader header, Shipment shipment, ISFDataObjectHelper helper)
		{
			if (header.Lines.Count > 0)
			{
				shipment.CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(writeManager.WriterStrategy)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(ProcessCollection(header.Lines.OrderBy(x => x.BL_ManufacturerDocAddressPK)
								.ThenBy(x => x.BL_TextProductCode)
								.ThenBy(x => x.BL_FormattedHarmonisedNum)
								.ThenBy(x => x.BL_RN_NKGoodsOrigin)
								.ThenBy(x => x.CustomAttribute1)
								.ThenBy(x => x.CustomAttribute2)
								.ThenBy(x => x.PK), new ISFLineDataObjectWriter(writeManager, helper)))))
					})
				};
			}
		}

		void PopulateContainersCollection(CusISFHeader header, Shipment shipment)
		{
			shipment.SetContainerCollection(() => ProcessCollection(header.Equipments, new ISFContainerDataObjectWriter(writeManager), CollectionContent.Complete));
		}

		void PopulateTransportLegCollection(CusISFHeader header, Shipment shipment)
		{
			shipment.SetTransportLegCollection(() => ProcessCollection(header.Transports, new TransportLegDataObjectWriter(writeManager), CollectionContent.Complete));
		}

		void PopulateNotes(CusISFHeader header, Shipment shipment)
		{
			shipment.SetNoteCollection(() => ProcessCollection(header.Notes.GetAllNotesVisibleToCurrentCompany(), new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
		}

		protected sealed override IEnumerable<IPropertyValue> GetUserDefinedValues(CusISFHeader sourceBO)
		{
			return sourceBO.GetUserDefinedValues().Where(x => ShouldIncludeCustomField(x));
		}

		bool ShouldIncludeCustomField(IPropertyValue propertyValue)
		{
			var result = true;
			if (propertyValue.Value.GetType() == typeof(ZString))
			{
				switch (propertyValue.PropertyName)
				{
					case Constants.CustomizedFieldConstants.CustomAttribOne:
					case Constants.CustomizedFieldConstants.CustomAttribTwo:
					case CusISFHeader.Schema.CustomAttribute1:
					case CusISFHeader.Schema.CustomAttribute2:
						result = false;
						break;
				}
			}
			return result;
		}
	}
}
