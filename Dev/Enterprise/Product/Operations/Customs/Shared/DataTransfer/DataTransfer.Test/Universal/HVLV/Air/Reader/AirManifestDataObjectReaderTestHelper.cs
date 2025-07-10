using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Currency = Enterprise.UniversalDataBuss.DataObjects.Universal.Currency;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest.Testing
{
	public abstract class AirManifestDataObjectReaderTestHelper : DataObjectReaderTestHelper
	{
		protected UniversalShipment SetupAirCargoHouse(ZString? wayBillNumber, ZBool isMasterHouse, ZString? masterHouse, UNLOCO portOfOrigin, UNLOCO portOfDestination, ZDecimal? weight, UnitOfWeight weightUQ, ZInt? piecesManifested, ZString? goodsDescription, ZDecimal? goodsValue, Currency goodsValueCurrency, ZString? responsitresponsiblePartyID)
		{
			var result = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = wayBillNumber,
				WayBillType = new WayBillType() { Code = isMasterHouse ? WayBillTypeList.Codes.MasterHouse : WayBillTypeList.Codes.House },
				PortOfOrigin = portOfOrigin,
				PortOfDestination = portOfDestination,
				TotalWeight = weight,
				TotalWeightUnit = weightUQ,
				TotalNoOfPieces = piecesManifested,
				GoodsDescription = goodsDescription,
				GoodsValue = goodsValue,
				GoodsValueCurrency = goodsValueCurrency
			};

			if (masterHouse.HasValue)
			{
				result.SetAdditionalBillCollection(() => new List<AdditionalBill>());
				result.AdditionalBillCollection.Add(SetupAdditionalBill(wayBillNumber, result.WayBillType, masterHouse));
			}

			if (responsitresponsiblePartyID.HasValue)
			{
				result.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());
				result.AdditionalReferenceCollection.Add(SetupAdditionalReference(null, null, responsitresponsiblePartyID, new EntryType() { Code = Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID }));
			}

			return result;
		}

		protected UniversalShipment SetupAirCargoHouse(ZString? wayBillNumber, ZBool isMasterHouse, ZString? masterHouse)
		{
			return SetupAirCargoHouse(wayBillNumber, isMasterHouse, masterHouse, new UNLOCO() { Code = AirForeignPort2.RL_Code }, new UNLOCO() { Code = AirLocalPort3.RL_Code },
				1500.60m, new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms }, 350, "GOODS FOR TESTING", 1304.50m, LocalCurrency, "RIP342342");
		}

		protected AdditionalBill SetupAdditionalBill(ZString? billNumber, WayBillType billType, ZString? parentBillNumber)
		{
			return new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = billNumber,
				BillType = billType,
				ParentBillNumber = parentBillNumber
			};
		}

		protected void AssertCusHAWBContents(CusHAWB hawbBO, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse)
		{
			AssertCusHAWBContents(hawbBO, wayBillNumber, isMasterHouse, masterHouse, AirForeignPort2.RL_Code, AirLocalPort3.RL_Code,
				1500.60m, Core.Constants.Weight.Kilograms, 350, "GOODS FOR TESTING", 1304.50m, GlbCompany.CurrentCompany.Country.RN_RX_NKLocalCurrency, "RIP342342");
		}

		protected virtual void AssertCusHAWBContents(CusHAWB hawbBO, ZString wayBillNumber, ZBool isMasterHouse, ZString masterHouse, ZString origin, ZString destination, ZDecimal weight, ZString weightUQ, ZShort piecesManifested, ZString goodsDescription, ZDecimal goodsValue, ZString goodsCurrency, ZString responsiblePartyID)
		{
			AssertEquals("hawbBO.CS_HAWB", wayBillNumber, hawbBO.CS_HAWB);
			AssertEquals("hawbBO.CS_IsMasterHouse", isMasterHouse, hawbBO.CS_IsMasterHouse);
			AssertEquals("hawbBO.CS_MasterHouseBill", masterHouse, hawbBO.CS_MasterHouseBill);
			AssertEquals("hawbBO.CS_RL_NKOrigin", origin, hawbBO.CS_RL_NKOrigin);
			AssertEquals("hawbBO.CS_RL_NKDestination", destination, hawbBO.CS_RL_NKDestination);
			AssertEquals("hawbBO.CS_Weight", weight, hawbBO.CS_Weight);
			AssertEquals("hawbBO.CS_WeightUQ", weightUQ, hawbBO.CS_WeightUQ);
			AssertEquals("hawbBO.CS_PiecesManifested", piecesManifested, hawbBO.CS_PiecesManifested);
			AssertEquals("hawbBO.CS_GoodsDescription", goodsDescription, hawbBO.CS_GoodsDescription);
			AssertEquals("hawbBO.CS_GoodsValue", goodsValue, hawbBO.CS_GoodsValue);
			AssertEquals("hawbBO.CS_RX_NKGoodsCurrency", goodsCurrency, hawbBO.CS_RX_NKGoodsCurrency);
			AssertEquals("hawbBO.CS_ResponsiblePartyID", responsiblePartyID, hawbBO.CS_ResponsiblePartyID);
		}

		protected virtual void AssertCusHAWBConsignor(CusHAWB hawbBO, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode, ZString contactName, ZString phone, ZGuid organisationPK)
		{
			AssertEquals("hawbBO.CS_ConsignorName", name.ToUpper(), hawbBO.CS_ConsignorName);
			AssertEquals("hawbBO.CS_ConsignorStreet", address1.ToUpper(), hawbBO.CS_ConsignorStreet);
			AssertEquals("hawbBO.CS_ConsignorStreet2", address2.ToUpper(), hawbBO.CS_ConsignorStreet2);
			AssertEquals("hawbBO.CS_ConsignorCity", city.ToUpper(), hawbBO.CS_ConsignorCity);
			AssertEquals("hawbBO.CS_ConsignorState", state.ToUpper(), hawbBO.CS_ConsignorState);
			AssertEquals("hawbBO.CS_ConsignorPostcode", postCode.ToUpper(), hawbBO.CS_ConsignorPostcode);
			AssertEquals("hawbBO.CS_RN_NKConsignorCountry", countryCode.ToUpper(), hawbBO.CS_RN_NKConsignorCountry);
			AssertEquals("hawbBO.CS_ConsignorContactName", contactName.ToUpper(), hawbBO.CS_ConsignorContactName);
			AssertEquals("hawbBO.CS_ConsignorPhone", GetFormattedPhone(phone, countryCode).ToUpper(), hawbBO.CS_ConsignorPhone);
			AssertEquals("hawbBO.CS_OH_Consignor", organisationPK, hawbBO.CS_OH_Consignor);
		}

		protected virtual void AssertCusHAWBConsignee(CusHAWB hawbBO, ZString name, ZString address1, ZString address2, ZString city, ZString state, ZString postCode, ZString countryCode, ZString contactName, ZString phone, ZGuid organisationPK)
		{
			AssertEquals("hawbBO.CS_ConsigneeName", name.ToUpper(), hawbBO.CS_ConsigneeName);
			AssertEquals("hawbBO.CS_ConsigneeStreet", address1.ToUpper(), hawbBO.CS_ConsigneeStreet);
			AssertEquals("hawbBO.CS_ConsigneeStreet2", address2.ToUpper(), hawbBO.CS_ConsigneeStreet2);
			AssertEquals("hawbBO.CS_ConsigneeCity", city.ToUpper(), hawbBO.CS_ConsigneeCity);
			AssertEquals("hawbBO.CS_ConsigneeState", state.ToUpper(), hawbBO.CS_ConsigneeState);
			AssertEquals("hawbBO.CS_ConsigneePostcode", postCode.ToUpper(), hawbBO.CS_ConsigneePostcode);
			AssertEquals("hawbBO.CS_RN_NKConsigneeCountry", countryCode.ToUpper(), hawbBO.CS_RN_NKConsigneeCountry);
			AssertEquals("hawbBO.CS_ConsigneeContactName", contactName.ToUpper(), hawbBO.CS_ConsigneeContactName);
			AssertEquals("hawbBO.CS_ConsigneePhone", GetFormattedPhone(phone, countryCode).ToUpper(), hawbBO.CS_ConsigneePhone);
			AssertEquals("hawbBO.CS_OH_Consignee", organisationPK, hawbBO.CS_OH_Consignee);
		}

		string GetFormattedPhone(string phone, string country)
		{
			if (string.IsNullOrEmpty(country))
			{
				return phone;
			}
			var formattedPhone = PhoneFormatter.FormatE164(phone, country);
			if (!string.IsNullOrEmpty(formattedPhone))
			{
				return formattedPhone;
			}
			return phone;
		}

		PhoneNumberFormatter PhoneFormatter => new PhoneNumberFormatter();

		protected UniversalShipment SetupAirCargoMaster(ZString? wayBillNumber, ZString? coloadBillNumber)
		{
			return SetupAirCargoMaster(wayBillNumber, coloadBillNumber,
				"QF344", "F34", new UNLOCO() { Code = "NZAKL" },
				new UNLOCO() { Code = "AUBNE" }, new UNLOCO() { Code = "AUSYD" });
		}

		protected UniversalShipment SetupAirCargoMaster(ZString? wayBillNumber, ZString? coloadBillNumber, ZString? flight, ZString? folio, UNLOCO portOfLoading, UNLOCO portOfDischarge, UNLOCO portOfFirstArrival)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.AirManifest, null);

			var result = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = wayBillNumber,
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Air },
				VoyageFlightNo = flight,
				Folio = folio,
				PortOfLoading = portOfLoading,
				PortOfDischarge = portOfDischarge,
				PortOfFirstArrival = portOfFirstArrival,
			};

			if (coloadBillNumber.HasValue)
			{
				result.SetAdditionalBillCollection(() => new List<AdditionalBill>());
				result.AdditionalBillCollection.Add(new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
				{
					BillNumber = coloadBillNumber.Value,
					BillType = new WayBillType()
					{
						Code = WayBillTypeList.Codes.MasterHouse,
						Description = WayBillTypeList.Descriptions.MasterHouse
					},
					ParentBillNumber = wayBillNumber
				});
			}
			return result;
		}

		protected void AssertContents(CusMAWB mawbBO, ZString masterBill, ZString coloadBill)
		{
			AssertContents(mawbBO, masterBill, coloadBill, "QF344", "F34", "NZAKL", "AUBNE", "AUSYD");
		}

		protected void AssertContents(CusMAWB mawbBO, ZString masterBill, ZString coloadBill, ZString flight, ZString folio, ZString portOfLoading, ZString portOfDischarge, ZString portOfFirstArrival)
		{
			AssertEquals("mawbBO.CM_MAWB", masterBill, mawbBO.CM_MAWB);
			AssertEquals("mawbBO.CM_MasterHouseBill", coloadBill, mawbBO.CM_MasterHouseBill);
			AssertEquals("mawbBO.CM_FlightNo", flight, mawbBO.CM_FlightNo);
			AssertEquals("mawbBO.CM_Folio", folio, mawbBO.CM_Folio);
			AssertEquals("mawbBO.CM_RL_NKLoadPort", portOfLoading, mawbBO.CM_RL_NKLoadPort);
			AssertEquals("mawbBO.CM_RL_NKDischargePort", portOfDischarge, mawbBO.CM_RL_NKDischargePort);
			AssertEquals("mawbBO.CM_RL_NKFirstArrivalPort", portOfFirstArrival, mawbBO.CM_RL_NKFirstArrivalPort);
		}

		protected RefUNLOCO AirLocalPort1
		{
			get
			{
				if (airLocalPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort1;
			}
		}

		RefUNLOCO airLocalPort1;

		protected RefUNLOCO AirLocalPort2
		{
			get
			{
				if (airLocalPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, AirLocalPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort2;
			}
		}

		RefUNLOCO airLocalPort2;

		protected RefUNLOCO AirLocalPort3
		{
			get
			{
				if (airLocalPort3 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, new[] { AirLocalPort1.PK, AirLocalPort2.PK });
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airLocalPort3 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airLocalPort3;
			}
		}

		RefUNLOCO airLocalPort3;

		protected RefUNLOCO AirForeignPort1
		{
			get
			{
				if (airForeignPort1 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airForeignPort1 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airForeignPort1;
			}
		}

		RefUNLOCO airForeignPort1;

		protected RefUNLOCO AirForeignPort2
		{
			get
			{
				if (airForeignPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, AirForeignPort1.PK);
					query.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
					airForeignPort2 = Factory.LoadTop1<RefUNLOCO>(query);
				}
				return airForeignPort2;
			}
		}

		RefUNLOCO airForeignPort2;
	}
}
