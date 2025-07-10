using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	public abstract class OutturnDataObjectReaderTestHelper<TCusOutturnHeader, TCusOutturn> : DataObjectReaderTestHelper
			where TCusOutturnHeader : CusOutturnHeader
			where TCusOutturn : CusOutturn
	{
		protected TCusOutturn CreateOuttrun(ZString cargoType, ZString containerNumber, ZString masterBill, ZString houseBill, CusOutturnHeader outturnHeader, ZString? status = null)
		{
			var newBOFactory = new BusinessObjectFactory();
			var existOutturn = newBOFactory.New<TCusOutturn>();
			existOutturn.C5_C6 = outturnHeader.PK;
			existOutturn.C5_CargoType = cargoType;
			existOutturn.C5_ContainerNumber = containerNumber;
			existOutturn.C5_MasterBill = masterBill;
			existOutturn.C5_HouseBill = houseBill;
			existOutturn.C5_MessageStatus = status ?? ZString.Empty;
			newBOFactory.Save();
			return existOutturn;
		}

		protected Shipment CreateTestOutturnShipment(ZString cargoType, ZString containerNumber, ZString masterBill, ZString houseBill)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.Outturn, null);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				ShipmentType = new CodeDescriptionPair
				{
					Code = "NIL",
					Description = "NIL Description"
				},
				OuterPacks = 10,
				OuterPacksPackageType = new PackageType
				{
					Code = "BAG",
					Description = "BAG Description"
				},
				TotalNoOfPacks = 5,
				TotalNoOfPacksPackageType = new PackageType
				{
					Code = "BAG",
					Description = "BAG Description"
				},
			};
			shipment.SetDateCollection(() => new List<Date>
				{
					{ DateType.Unpack, ZBool.False, new ZDateTime(2019, 08, 08) },
					{ DateType.Received, ZBool.False, new ZDateTime(2019, 08, 09) }
				});

			shipment.SetAddInfoCollection(() => new List<AddInfo>
				{
					new AddInfo
					{
						Key = "IsDamage",
						Value = "Y"
					},
					new AddInfo
					{
						Key = "IsPillage",
						Value = "N"
					}
				});

			shipment.SetNoteCollection(() => new DataObjectList<Note>
				{
					new Note
					{
						Description = Constants.Note.Descriptions.GoodsDescription,
						IsCustomDescription = true,
						NoteText = "GoodsDescription TEXT"
					},
					new Note
					{
						Description = Constants.Note.Descriptions.MarksAndNumbersDescription,
						IsCustomDescription = true,
						NoteText = "MarksAndNumbersDescription TEXT"
					}
				});
			shipment.SetAdditionalBillCollection(() => new List<AdditionalBill>
				{
					new AdditionalBill
					{
						BillType = new WayBillType
						{
							Code =  WayBillTypeList.Codes.House,
							Description = WayBillTypeList.Descriptions.House
						},
						BillNumber = houseBill,
						ParentBillNumber = masterBill
					}
				});
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						OutturnedVolume = 10.00M,
						BillType = new WayBillType
						{
							Code = WayBillTypeList.Codes.House,
							Description = WayBillTypeList.Descriptions.House
						},
						BillNumber = "HB123"
					}
				});
			shipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						ContainerType = new ContainerType
						{
							Code = cargoType,
							Description = "FCL Description"
						},
						ContainerNumber = containerNumber,
						Seal = "ContainerSeal",
						IsSealOk =  true
					}
				});
			return shipment;
		}

		protected CusOutturn CreateOutturn(ZString cargoType, ZString containerNumber, ZString masterBill, ZString houseBill, CusOutturnHeader outturnHeader, ZDateTime? receiptDate = null)
		{
			var existOutturn = Factory.BOFactory.New<CusOutturn>();
			existOutturn.C5_C6 = outturnHeader.PK;
			existOutturn.C5_CargoType = cargoType;
			existOutturn.C5_ContainerNumber = containerNumber;
			existOutturn.C5_MasterBill = masterBill;
			existOutturn.C5_HouseBill = houseBill;
			existOutturn.C5_CargoReceiptDate = receiptDate ?? ZDateTime.Empty;
			return existOutturn;
		}

		protected RefVessel CreateRefVesselForTest()
		{
			var aplVessel = Factory.New<RefVessel>();
			aplVessel.RV_Code = "VESSEL";
			aplVessel.RV_LloydsNumber = "9832343";
			aplVessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.China;
			aplVessel.RV_RadioCallSign = "CALLME";

			return aplVessel;
		}

		protected OrgHeader CreatePremiseAddressForTest()
		{
			var primiseAddress = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			primiseAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "CP123", GlbCompany.CurrentCompany.Country.Code);
			return primiseAddress;
		}

		protected TCusOutturnHeader CreateOuttrunHeader(RefVessel refVessel, OrgHeader premise, ZString voyage, ZString reference)
		{
			var newBOFactory = new BusinessObjectFactory();
			var existOutturnHeader = newBOFactory.New<TCusOutturnHeader>();
			existOutturnHeader.C6_VesselName = refVessel.RV_Code;
			existOutturnHeader.C6_LloydsIMO = refVessel.RV_LloydsNumber;
			existOutturnHeader.C6_VoyageNum = voyage;
			existOutturnHeader.C6_OA_OutturningPremise = premise.MainAddress.PK;
			existOutturnHeader.C6_OutturningPremiseID = premise.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID);
			existOutturnHeader.C6_SendersMessageReference = reference;
			newBOFactory.Save();

			return existOutturnHeader;
		}

		protected Shipment CreateTestOutturnHeaderShipment(RefVessel refVessel, OrgHeader premise, ZString voyage, string countryCode = Core.Constants.CountryCodes.Australia, ZString? reference = null, ZString? forwardingShipmentUniqueConsignRef = null)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.SeaCargoOutturn, reference);
			if (forwardingShipmentUniqueConsignRef.HasValue)
			{
				dataContext.AddDataSource(DataContextType.ForwardingShipment, forwardingShipmentUniqueConsignRef.ToString());
			}

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				TransportMode = new CodeDescriptionPair
				{
					Code = Core.Constants.TransportModes.Sea,
					Description = Core.Constants.TransportModeDescriptions.Sea
				},
				VoyageFlightNo = voyage,
				VesselName = refVessel.RV_Code,
				LloydsIMO = refVessel.RV_LloydsNumber,
			};

			shipment.SetDateCollection(() => new List<Date>
				{
					{ DateType.Arrival, ZBool.False, new ZDateTime(2019, 08, 07) }
				});

			shipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new EntryType
						{
							Code = Constants.AdditionalReference.EntryType.Codes.ControlledPremiseID,
							Description = Constants.AdditionalReference.EntryType.Descriptions.ControlledPremiseID
						},
						ContextInformation = countryCode,
						ReferenceNumber = premise.CustomsCodes.GetCustomsRegNo(Constants.AdditionalReference.EntryType.Codes.ControlledPremiseID)
					},
					new AdditionalReference
					{
						Type = new EntryType
						{
							Code = Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
							Description = Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID
						},
						ContextInformation = countryCode,
						ReferenceNumber = "RE123"
					}
				});

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			shipment.AddOrgAddress(writeManager, premise, AddressTypes.ArrivalCFSAddress);

			return shipment;
		}
	}
}
