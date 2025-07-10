using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectReaderTest
	{
		public void TestElementsNotSupportedInBillIfAllowUpdateOfCustomsDeclarationAfterCommencement()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.SaveForTesting();

			var headerDataObject = SetupInBondHeader();
			headerDataObject.AllowUpdateOfCustomsDeclarationAfterCommencement = true;
			var dataTarget = headerDataObject.DataContext.DataTargetCollection.FirstOrDefault();
			dataTarget.Key = header.BH_JobReference;
			var billDataObject = SetupInBondBill("APLU", "HB1234", 200m, 1);
			billDataObject.CustomsReferenceCollection = new List<CustomsReference>()
			{
				SetupCustomsReferenceData(Constants.AdditionalReference.Type, BillReferenceList.Codes.CUB, "11111111")
			};
			billDataObject.SetAddInfoGroupCollection(() => new List<AddInfoGroup>()
			{
				new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.USDisposition },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo()
						{
							Key = USDispositionDataAddInfoSchema.US_Code.Name.Substring(3),
							Value = "69"
						}
					}
				}
			});
			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { billDataObject });

			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw", @"The following elements are not supported when 'AllowUpdateOfCustomsDeclarationAfterCommencement' is flagged as true. 
· <AddInfoGroupCollection><AddInfoGroup><Type><Code>UDP</Code></Type></AddInfoGroup></AddInfoGroupCollection>
", () => reader.ReadIntoBusinessObject());
		}

		AdditionalBill SetupInBondBill(ZString issuerCode, ZString billNumber, ZDecimal manifestQty, ZInt link)
		{
			return SetupInBondBill(issuerCode, billNumber, BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard, "1101", "15213", "AAAAAAA", "11", "AUSYD", "60267", "USCHI", "3901", manifestQty,
				"BAG", 100m, "KG", 200m, "M3", "CAS", "AUARA", "8277", new ZDateTime(2018, 03, 19), "Y", "W001", "R", "05", link);
		}

		AdditionalBill SetupInBondBill(ZString issuerCode, ZString billNumber, ZString billStatus, ZString ladingPort, ZString ladingPortSchK, ZString placeOfReceiptSchD, ZString transportMode,
			ZString lastForeignPort, ZString lastForeignPortSchK, ZString foreignPortOfContract, ZString foreignPortOfContractSchK, ZDecimal manifestQty, ZString manifestUQ, ZDecimal weight, ZString weightUQ,
			ZDecimal volumn, ZString volumnUQ, ZString paymentMethod, ZString portOfUnlading, ZString portOfUnladingSchD, ZDateTime estimatedUnloadDate, ZString masterInBondInd, ZString firms, ZString actionCode, ZString amendmentCode, ZInt link)
		{
			var result = SetupInBondBill(billNumber, WayBillTypeList.Codes.Master, ZString.Empty, manifestQty, link);
			result.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			result.PackType = new PackageType() { Code = manifestUQ };
			result.AddInfoCollection = new List<AddInfo>()
			{
				new AddInfo() { Key = Constants.Bill.AddInfo.IssuerCode, Value = issuerCode },
				new AddInfo() { Key = Constants.Bill.AddInfo.BillStatus, Value = billStatus },
				new AddInfo() { Key = Constants.Bill.AddInfo.PortOfLading, Value = ladingPort },
				new AddInfo() { Key = Constants.Bill.AddInfo.PortOfLadingScheduleK, Value = ladingPortSchK },
				new AddInfo() { Key = Constants.Bill.AddInfo.PlaceOfReceiptScheduleD, Value = placeOfReceiptSchD },
				new AddInfo() { Key = Constants.Bill.AddInfo.TransportModeToPortOfLading, Value = transportMode },
				new AddInfo() { Key = Constants.Bill.AddInfo.LastForeignPort, Value = lastForeignPort },
				new AddInfo() { Key = Constants.Bill.AddInfo.LastForeignPortScheduleK, Value = lastForeignPortSchK },
				new AddInfo() { Key = Constants.Bill.AddInfo.ForeignPortOfContract, Value = foreignPortOfContract },
				new AddInfo() { Key = Constants.Bill.AddInfo.ForeignPortOfContractScheduleK, Value = foreignPortOfContractSchK },
				new AddInfo() { Key = Constants.Bill.AddInfo.Weight, Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(weight) },
				new AddInfo() { Key = Constants.Bill.AddInfo.WeightUnit, Value = weightUQ },
				new AddInfo() { Key = Constants.Bill.AddInfo.Volume, Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(volumn) },
				new AddInfo() { Key = Constants.Bill.AddInfo.VolumeUnit, Value = volumnUQ },
				new AddInfo() { Key = Constants.Bill.AddInfo.PaymentMethod, Value = paymentMethod },
				new AddInfo() { Key = Constants.Bill.AddInfo.PortOfUnlading, Value = portOfUnlading },
				new AddInfo() { Key = Constants.Bill.AddInfo.PortOfUnladingScheduleD, Value = portOfUnladingSchD },
				new AddInfo() { Key = Constants.Bill.AddInfo.EstimatedUnloadDate, Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(estimatedUnloadDate) },
				new AddInfo() { Key = Constants.Bill.AddInfo.MasterInBondIndicator, Value = masterInBondInd },
				new AddInfo() { Key = Constants.Bill.AddInfo.FIRMS, Value = firms },
				new AddInfo() { Key = Constants.Bill.AddInfo.ActionCode, Value = actionCode },
				new AddInfo() { Key = Constants.Bill.AddInfo.AmendmentCode, Value = amendmentCode }
			};
			return result;
		}

		void AssertCusInBondBillContents(CusInBondBill billBO, ZString issuerCode, ZString billNumber, ZInt manifestQty)
		{
			AssertCusInBondBillContents(billBO, issuerCode, billNumber, BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard, "1101", "15213", "AAAAAAA", "11", "AUSYD", "60267", "USCHI", "3901", manifestQty,
				"BAG", 100m, "KG", 200m, "M3", "CAS", "AUARA", "8277", new ZDateTime(2018, 03, 19), true, "W001", "R", "05");
		}

		void AssertCusInBondBillContents(CusInBondBill billBO, ZString issuerCode, ZString billNumber, ZString billStatus, ZString ladingPort, ZString ladingPortSchK, ZString placeOfReceiptSchD, ZString transportMode,
			ZString lastForeignPort, ZString lastForeignPortSchK, ZString foreignPortOfContract, ZString foreignPortOfContractSchK, ZInt manifestQty, ZString manifestUQ, ZDecimal weight, ZString weightUQ,
			ZDecimal volumn, ZString volumnUQ, ZString paymentMethod, ZString portOfUnlading, ZString portOfUnladingSchD, ZDateTime estimatedUnloadDate, ZBool imMasterInBond, ZString firms, ZString actionCode, ZString amendmentCode)
		{
			AssertEquals("billBO.B0_IssuerCode", issuerCode, billBO.B0_IssuerCode);
			AssertEquals("billBO.B0_MasterBillNumber", billNumber, billBO.B0_MasterBillNumber);
			AssertEquals("billBO.B0_BillStatus", billStatus, billBO.B0_BillStatus);
			AssertEquals("billBO.B0_RL_NKPortOfLading", ladingPort, billBO.B0_RL_NKPortOfLading);
			AssertEquals("billBO.B0_PortOfLadingKCode", ladingPortSchK, billBO.B0_PortOfLadingKCode);
			AssertEquals("billBO.B0_PlaceOfReceipt", placeOfReceiptSchD, billBO.B0_PlaceOfReceipt);
			AssertEquals("billBO.B0_TransportModeToPortOfLading", transportMode, billBO.B0_TransportModeToPortOfLading);
			AssertEquals("billBO.B0_RL_NKLastForeignPort", lastForeignPort, billBO.B0_RL_NKLastForeignPort);
			AssertEquals("billBO.B0_LastForeignPortKCode", lastForeignPortSchK, billBO.B0_LastForeignPortKCode);
			AssertEquals("billBO.B0_RL_NKForeignPortOfContract", foreignPortOfContract, billBO.B0_RL_NKForeignPortOfContract);
			AssertEquals("billBO.B0_ForeignPortOfContractKCode", foreignPortOfContractSchK, billBO.B0_ForeignPortOfContractKCode);
			AssertEquals("billBO.B0_ManifestQty", manifestQty, billBO.B0_ManifestQty);
			AssertEquals("billBO.B0_ManifestUQ", manifestUQ, billBO.B0_ManifestUQ);
			AssertEquals("billBO.B0_Weight", weight, billBO.B0_Weight);
			AssertEquals("billBO.B0_WeightUQ", weightUQ, billBO.B0_WeightUQ);
			AssertEquals("billBO.B0_Volumn", volumn, billBO.B0_Volume);
			AssertEquals("billBO.B0_VolumeUQ", volumnUQ, billBO.B0_VolumeUQ);
			AssertEquals("billBO.B0_TransportPaymentMethod", paymentMethod, billBO.B0_TransportPaymentMethod);
			AssertEquals("billBO.B0_RL_NKInBondPortOfDest", portOfUnlading, billBO.B0_RL_NKInBondPortOfDest);
			AssertEquals("billBO.B0_InBondPortOfDestDCode", portOfUnladingSchD, billBO.B0_InBondPortOfDestDCode);
			AssertEquals("billBO.B0_DateOfDischarge", estimatedUnloadDate, billBO.B0_DateOfDischarge);
			AssertEquals("billBO.B0_MasterInBondIndicator", imMasterInBond, billBO.B0_MasterInBondIndicator);
			AssertEquals("billBO.B0_Firms", firms, billBO.B0_Firms);
			AssertEquals("billBO.B0_BillActionCode", actionCode, billBO.B0_BillActionCode);
			AssertEquals("billBO.B0_BillAmendmentCode", amendmentCode, billBO.B0_BillAmendmentCode);
		}

		void AssertAdditionalReferenceContains(CusInbondBillAddRefCollection collection, ZString qualifier, ZString reference)
		{
			AssertNotNull(string.Format("Additional Reference (Q:{0}, R:{1}) exists.", qualifier, reference), collection.FirstOrDefault(x => x.BR_Qualifier == qualifier && x.BR_ReferenceNum == reference));
		}

		void AssertSecondaryNotifyPartyContains(SecondaryNotifyPartyCollection collection, ZString code, ZShort order)
		{
			AssertNotNull(string.Format("Secondary Notify Party (Code:{0}, Order:{1} exists.)", code, order), collection.FirstOrDefault(x => x.CY_Data == code && x.CY_Order == order));
		}
	}
}
