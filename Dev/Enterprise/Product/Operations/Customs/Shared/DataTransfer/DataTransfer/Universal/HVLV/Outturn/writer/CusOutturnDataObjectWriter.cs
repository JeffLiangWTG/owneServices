using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class CusOutturnDataObjectWriter<TCusOutturn> : TopLevelDataObjectWriter<TCusOutturn, UShipment>
		where TCusOutturn : CusOutturn
	{
		public CusOutturnDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
			LinkManager = new CusContainerLinkManager();
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.Outturn;

		public readonly CusContainerLinkManager LinkManager;

		protected override void PopulateDataObject(TCusOutturn sourceBO, UShipment shipment)
		{
			shipment.WayBillType = new WayBillType
			{
				Code = WayBillTypeList.Codes.House,
				Description = WayBillTypeList.Descriptions.House
			};
			shipment.WayBillNumber = sourceBO.C5_HouseBill;

			shipment.OuterPacks = sourceBO.C5_OuterPacks;
			shipment.OuterPacksPackageType = new PackageType
			{
				Code = sourceBO.C5_OuterPackUnits,
				Description = sourceBO.Lookups.PackageTypes.GetDescriptionFromCode(sourceBO.C5_OuterPackUnits)
			};

			shipment.TotalNoOfPacks = sourceBO.C5_PackagesOutturned;
			shipment.TotalNoOfPacksPackageType = new PackageType
			{
				Code = sourceBO.C5_PackagesUnits,
				Description = sourceBO.Lookups.PackageTypes.GetDescriptionFromCode(sourceBO.C5_PackagesUnits)
			};

			PopulateStatus(sourceBO, shipment);
			PopulateDateCollection(sourceBO, shipment);
			PopulateContainerCollection(sourceBO, shipment);
			PopulateAddInfoCollection(sourceBO, shipment);
			PopulateNoteCollection(sourceBO, shipment);
			PopulateAdditionalBillCollection(sourceBO, shipment);
			PopulatePackingLineCollection(sourceBO, shipment);
			PopulateCountrySpecificDetails(sourceBO, shipment);
		}

		protected virtual void PopulateCountrySpecificDetails(TCusOutturn sourceBO, UShipment shipment)
		{
		}

		static void PopulateStatus(TCusOutturn sourceBO, UShipment shipment)
		{
			shipment.EntryStatus = new EntryStatus
			{
				Code = sourceBO.CustomsStatus.Code,
				Description = sourceBO.CustomsStatus.Description
			};
			shipment.MessageStatus = new CodeDescriptionPair
			{
				Code = sourceBO.MessageStatus.Code,
				Description = sourceBO.MessageStatus.Description
			};
			shipment.OperationalStatus = new CodeDescriptionPair
			{
				Code = sourceBO.C5_CommercialStatus,
				Description = sourceBO.Lookups.CommercialStatusList.GetDescriptionFromCode(sourceBO.C5_CommercialStatus)
			};
		}

		static void PopulateDateCollection(TCusOutturn sourceBO, UShipment shipment)
		{
			shipment.SetDateCollection(() => new List<Date>
			{
				{ DateType.Unpack, ZBool.False, sourceBO.C5_CargoUnpackDate },
				{ DateType.Received, ZBool.False, sourceBO.C5_CargoReceiptDate }
			});
		}

		static void PopulateContainerCollection(TCusOutturn sourceBO, UShipment shipment)
		{
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[]
			{
				new Container
				{
					ContainerType = new ContainerType
					{
						Code = sourceBO.C5_CargoType,
						Description = sourceBO.Lookups.CargoTypes.GetDescriptionFromCode(sourceBO.C5_CargoType)
					},
					ContainerNumber = sourceBO.C5_ContainerNumber,
					Seal = sourceBO.C5_ContainerSeal,
					IsSealOk = sourceBO.C5_SealIntactIndicator
				}
			}));
		}

		static void PopulateAddInfoCollection(TCusOutturn sourceBO, UShipment shipment)
		{
			shipment.SetAddInfoCollection(() => new List<UAddInfo>
			{
				new UAddInfo
				{
					Key = Constants.AddInfoKeys.Outturn.IsDamage,
					Value = sourceBO.C5_DamageIndicator ? YesNoList.Codes.Yes : YesNoList.Codes.No,
				},
				new UAddInfo
				{
					Key = Constants.AddInfoKeys.Outturn.IsPillage,
					Value = sourceBO.C5_PillageIndicator ? YesNoList.Codes.Yes : YesNoList.Codes.No
				}
			});
		}

		static void PopulateNoteCollection(TCusOutturn sourceBO, UShipment shipment)
		{
			shipment.SetNoteCollection(() => new DataObjectList<Note>(new[]
			{
				new Note
				{
					Description = Constants.Note.Descriptions.GoodsDescription,
					IsCustomDescription = ZBool.True,
					NoteText = sourceBO.C5_GoodsDescription
				},
				new Note
				{
					Description = Constants.Note.Descriptions.MarksAndNumbersDescription,
					IsCustomDescription = ZBool.True,
					NoteText = sourceBO.C5_MarksAndNumbers
				}
			}));
		}

		static void PopulateAdditionalBillCollection(TCusOutturn sourceBO, UShipment shipment)
		{
			shipment.SetAdditionalBillCollection(() => new List<AdditionalBill>(new[]
			{
				new AdditionalBill
				{
					BillType = new WayBillType
					{
						Code =  WayBillTypeList.Codes.House,
						Description = WayBillTypeList.Descriptions.House
					},
					BillNumber = sourceBO.C5_HouseBill,
					ParentBillNumber = sourceBO.C5_MasterBill
				}
			}));
		}

		protected void PopulatePackingLineCollection(TCusOutturn sourceBO, UShipment shipment)
		{
			var containerLinkFound = LinkManager.ContainerNumberAndLink.TryGetValue(sourceBO.C5_ContainerNumber, out var containerLink);
			var packLine = new PackingLine(writeManager.WriterStrategy)
			{
				OutturnedVolume = sourceBO.C5_VolumeOutturned,
				BillType = new WayBillType
				{
					Code = WayBillTypeList.Codes.House,
					Description = WayBillTypeList.Descriptions.House
				},
				BillNumber = sourceBO.C5_HouseBill,
				ContainerLink = containerLinkFound ? containerLink : null,
				PackQty = new ZLong(sourceBO.C5_OuterPacks),
				GoodsDescription = sourceBO.C5_GoodsDescription,
				MarksAndNos = sourceBO.C5_MarksAndNumbers
			};

			PopulatePackLine(sourceBO, packLine);
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[]
			{
				packLine
			}));
		}

		protected virtual void PopulatePackLine(TCusOutturn sourceBO, PackingLine packline)
		{
		}
	}
}
