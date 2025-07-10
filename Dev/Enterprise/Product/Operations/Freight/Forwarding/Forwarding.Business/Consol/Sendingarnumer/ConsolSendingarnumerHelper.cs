using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolSendingarnumerHelper
	{
		public ConsolSendingarnumerHelper(ForwardingConsol consol)
		{
			if (consol == null)
			{
				throw new ArgumentNullException(nameof(consol));
			}

			this.Consol = consol;
			Factory = consol.Factory;
			this.Sendingarnumer = new Sendingarnumer(consol.JK_CRN);
		}

		public void RefreshCRN()
		{
			Consol.JK_CRN = Sendingarnumer.Code;
		}

		public void FormatCRN(ZString code)
		{
			Sendingarnumer.FormatCode(code);
			if (Sendingarnumer.IsAir && Sendingarnumer.IsImport)
			{
				AutoPopulateConsol();
			}
		}

		public ZString Code
		{
			get
			{
				return Sendingarnumer.Code;
			}
		}

		#region CarrierNumberPrefixCharacter
		public ZString CarrierNumberPrefixCharacter
		{
			get
			{
				if (carrierNumberPrefixCharacter.IsEmpty)
				{
					if (Consol != null)
					{
						if (Consol.IsAir)
						{
							if (Consol.IsAirExpress)
							{
								carrierNumberPrefixCharacter = Sendingarnumer.AirExpressPrefix + GetConsolSequenceNumber();
							}
							else
							{
								carrierNumberPrefixCharacter = FreightDataRegistry.Instance.ForwarderSendingarnumerCodeForAirfreight.Value;
							}
						}
						else if (Consol.IsSea)
						{
							carrierNumberPrefixCharacter = FreightDataRegistry.Instance.ForwarderSendingarnumerCodeForSeafreight.Value;
						}
					}
				}
				return carrierNumberPrefixCharacter;
			}
		}
		ZString carrierNumberPrefixCharacter;

		ZString GetConsolSequenceNumber()
		{
			ZString result = "0";
			ZString sameFlightConsolCRN = Consol.JK_CRN.SubstringSafe(0, 12);

			if (!sameFlightConsolCRN.IsEmpty && sameFlightConsolCRN.Length == 12)
			{
				ZDBOnlyQuery consolFilter = new ZDBOnlyQuery(typeof(ForwardingConsol));
				ZDBOnlySubQuery cusEntryNumFilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobConsolSchema.Constants.TableName);
				cusEntryNumFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Iceland.CRN);
				cusEntryNumFilter.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, sameFlightConsolCRN);
				cusEntryNumFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				consolFilter.AddSubQuery(cusEntryNumFilter, JoinCondition.And);

				ForwardingConsol[] consols = Factory.Load<ForwardingConsol>(consolFilter);

				for (int i = 0; i < consols.Length; i++)
				{
					ZBool isTaken = ZBool.False;

					foreach (ForwardingConsol currentConsol in consols)
					{
						if (currentConsol != Consol)
						{
							foreach (ForwardingShipment shipment in currentConsol.Shipments)
							{
								if (shipment.CustomsEntryNumber.SubstringSafe(20, 2) == Sendingarnumer.AirExpressPrefix + i)
								{
									isTaken = ZBool.True;
									break;
								}
							}
						}

						if (isTaken)
						{
							break;
						}
					}

					if (!isTaken)
					{
						result = i.ToString(CultureInfo.InvariantCulture);
						break;
					}
				}
			}

			return result;
		}
		#endregion

		public void Validate()
		{
			if (!Consol.JK_CRN.IsEmpty)
			{
				if (Sendingarnumer.IsImport && Sendingarnumer.IsAir && !Sendingarnumer.HasValidLength)
				{
					Consol.JK_CRNInfo.AddError(Res.GetString("f866ddfa-846c-4b1c-b5b5-22efb0fffe1f", "You must enter {0} characters EXCLUDING the '{1}' character for Import Air.", Sendingarnumer.CodeLengthWithoutDelimiter, Sendingarnumer.Delimiter));
				}

				ValidateShipmentCRNMatch();
				ValidateNoDupliateCRNs();
				ValidateCarrierCode();
				ValidateVesselFlightNumber();
				ValidateArrivalOrDepartureDate();
				ValidateYear();
				ValidatePortOfLoading();
				ValidateCarrierNumber();
				ValidateCheckDigit();
			}
		}

		#region Implementation
		readonly ForwardingConsol Consol;
		readonly BusinessObjectFactory Factory;
		public readonly Sendingarnumer Sendingarnumer;

		#region Iceland
		RefCountry Iceland
		{
			get
			{
				if (iceland == null)
				{
					iceland = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iceland);
				}
				return iceland;
			}
		}
		RefCountry iceland;
		#endregion

		#region Validation

		void ValidateNoDupliateCRNs()
		{
			if (!Sendingarnumer.CarrierNumber.IsEmpty)
			{
				ZDBOnlyQuery crnFilter = new ZDBOnlyQuery(typeof(CusEntryNumber));
				crnFilter.AddToFilter(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.NotEqual, Consol.PK);
				crnFilter.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, Sendingarnumer.CodeWithoutCheckDigit);
				crnFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobConsolSchema.Constants.TableName);
				crnFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Iceland.CRN);
				crnFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				CusEntryNumber cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(crnFilter);
				if (cusEntryNumber != null)
				{
					ForwardingConsol consolWithSameCusEntryNumber = Factory.Load<ForwardingConsol>(cusEntryNumber.CE_ParentID);
					if (consolWithSameCusEntryNumber != null)
					{
						Consol.JK_CRNInfo.AddError(Res.GetString("b385e4d9-8a30-4516-9c36-1cf6ea5c76c9", "This Sendingarnumer is already been used by Consolidation: {0}", consolWithSameCusEntryNumber.JK_UniqueConsignRef));
					}
				}
			}
		}

		void ValidateShipmentCRNMatch()
		{
			if (Consol.Shipments.Count > 0 && !Consol.JK_CRN.IsEmpty)
			{
				ZString consolCRN = Consol.JK_CRN.Length > 19 ? Consol.JK_CRN.Substring(0, 19) : Consol.JK_CRN;
				ZBool hasShipmentWithInvalidCRN = ZBool.False;

				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					if (!shipment.CustomsEntryNumber.IsEmpty && !shipment.CustomsEntryNumber.StartsWith(consolCRN, StringComparison.Ordinal))
					{
						hasShipmentWithInvalidCRN = ZBool.True;
						break;
					}
				}

				if (hasShipmentWithInvalidCRN)
				{
					Consol.JK_CRNInfo.AddError(Res.GetString("4f6dd5ed-7c6c-4417-8123-3caaff292b87", "This Consolidation has Shipment(s) with invalid Sendingarnumer. Please regenerate the shipment's Sendingarnumer from the Actions Menu"));
				}
			}
		}

		void ValidateCarrierCode()
		{
			if (!Sendingarnumer.CarrierCode.IsEmpty)
			{
				if (!Sendingarnumer.CarrierCode.IsLettersOnlyOrEmpty)
				{
					Consol.JK_CRNInfo.AddError(Res.GetString("3705393d-fe65-4604-988a-8e1cc8c2b72a", "The Carrier Code must be Alphabetic"));
				}
				else if (Sendingarnumer.CarrierCode == "B" || Sendingarnumer.CarrierCode == "Q" || Sendingarnumer.CarrierCode == "T" || Sendingarnumer.CarrierCode == "X")
				{
					Consol.JK_CRNInfo.AddError(Res.GetString("006016a8-c929-4a79-a9fe-5b3499719be4", "The Carrier Code cannot be: B, Q, T and X"));
				}
				else
				{
					ZQuery carrierCodeFilter = new ZQuery();
					carrierCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Iceland.Code);
					carrierCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);
					carrierCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, Sendingarnumer.CarrierCode);

					if (Consol.ShippingLine == null)
					{
						Consol.JK_CRNInfo.AddError(Res.GetString("ed6f3174-89f6-491d-94c2-f9edcd76546e", "Please enter a Carrier"));
					}
					else if (Consol.ShippingLine != null && Consol.ShippingLine.CustomsCodes.Find(carrierCodeFilter).Length == 0)
					{
						Consol.JK_CRNInfo.AddError(Res.GetString("2b173c23-b972-41b8-a489-b917268e352e", "Carrier {0} does not have Carrier Code '{1}'", Consol.ShippingLine.OH_FullNameTruncated, Sendingarnumer.CarrierCode));
					}
				}
			}
		}

		void ValidateVesselFlightNumber()
		{
			if (!Sendingarnumer.VesselCodeFlightNumber.IsEmpty)
			{
				if (Sendingarnumer.IsAir)
				{
					if (Consol.Transports.MostInterestingTransport != null)
					{
						ZDBOnlyQuery transportLegFilter = new ZDBOnlyQuery(typeof(JobVoyage));
						transportLegFilter.AddToFilter(JobVoyageSchema.JV_VoyageFlight, Consol.Transports.MostInterestingTransport.JW_VoyageFlight);
						transportLegFilter.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Core.Constants.TransportModes.Air);

						if (Factory.LoadTop1<JobVoyage>(transportLegFilter) == null)
						{
							Consol.JK_CRNInfo.AddError(Res.GetString("93eedab6-031d-430c-afbf-3928929bb20a", "Unable to find Flight Number {0}", Consol.Transports.MostInterestingTransport.JW_VoyageFlight));
						}
					}

					if (Consol.JK_TransportMode != Core.Constants.TransportModes.Air)
					{
						Consol.JK_CRNInfo.AddError(Res.GetString("9fd9eb28-ef88-423c-9f3a-061299bcceb3", "This Sendingarnumer is for Air."));
					}
				}
				else if (Sendingarnumer.IsSea)
				{
					if (Consol.JK_TransportMode != Core.Constants.TransportModes.Sea)
					{
						Consol.JK_CRNInfo.AddError(Res.GetString("76ae7e43-29a6-462a-8dec-8e1d41592393", "This Sendingarnumer is for Sea."));
					}
					else
					{
						Transport currentTransport = Consol.Transports.MostInterestingTransport;

						if (currentTransport.Vessel != null && currentTransport.Vessel.RV_CarrierCode.SubstringSafe(0, 3) != Sendingarnumer.VesselCodeFlightNumber)
						{
							if (currentTransport.Vessel.RV_CarrierCode.IsEmpty)
							{
								Consol.JK_CRNInfo.AddError(Res.GetString("42fb7d5f-4da8-4e29-a992-e92c86bb6bc2", "The vessel {0} has no Vessel Code", Consol.Transports.MostInterestingTransport.Vessel.RV_Name));
							}
							else if (currentTransport.Vessel.RV_CarrierCode.Length < 3)
							{
								Consol.JK_CRNInfo.AddError(Res.GetString("ade58153-b921-428f-a971-b54abb2d5e25", "The Vessel Code of vessel {0} is less than 3 characters", Consol.Transports.MostInterestingTransport.Vessel.RV_Name));
							}
							else
							{
								Consol.JK_CRNInfo.AddError(Res.GetString("faba0938-0018-402c-b846-b36ffd612ff4", "The Vessel Code is incorrect. It should be") + " " + Consol.Transports.MostInterestingTransport.Vessel.RV_CarrierCode.SubstringSafe(0, 3));
							}
						}
					}
				}
			}
		}

		void ValidateArrivalOrDepartureDate()
		{
			if (!Sendingarnumer.ArrivalDepartureDayMonth.IsEmpty)
			{
				if (!Sendingarnumer.ArrivalDepartureDayMonth.IsNumbersOnlyOrEmpty)
				{
					Consol.JK_CRNInfo.AddError(Res.GetString("2f79ad00-7698-4930-9e2a-f1cda1b9d73a", "Arrival/Departure date must be Numeric"));
				}
				else
				{
					ZInt dayPortion = ZInt.ParseEmptyAsZero(Sendingarnumer.ArrivalDepartureDayMonth.SubstringSafe(0, 2));
					ZInt monthPortion = ZInt.ParseEmptyAsZero(Sendingarnumer.ArrivalDepartureDayMonth.SubstringSafe(2, 2));

					if (dayPortion > 31)
					{
						Consol.JK_CRNInfo.AddError(Res.GetString("6ccafbcf-9950-41c0-8cea-82e5d8b654ad", "Days cannot be more than 31"));
					}

					if (monthPortion > 12)
					{
						Consol.JK_CRNInfo.AddError(Res.GetString("9db3d3ef-a4a5-43d5-b9cf-f5390dc26f75", "Months cannot be more than 12"));
					}

					if (!Sendingarnumer.ArrivalDepartureYear.IsEmpty)
					{
						if (!Sendingarnumer.ArrivalDepartureDate.IsValid)
						{
							Consol.JK_CRNInfo.AddError(Res.GetString("85e6a260-010b-47ee-a4db-2440360a041d", "The date that you have entered is not valid"));
						}
					}
				}
			}
		}

		void ValidateYear()
		{
			if (!Sendingarnumer.ArrivalDepartureYear.IsEmpty)
			{
				if (!Sendingarnumer.ArrivalDepartureYear.IsNumbersOnlyOrEmpty)
				{
					Consol.JK_CRNInfo.AddError(Res.GetString("d7a3d34c-23c5-4ba3-8aa2-3d761410096d", "Year must be Numeric"));
				}
			}
		}

		void ValidatePortOfLoading()
		{
			if (!Sendingarnumer.PortOfLoadingCountryCode.IsEmpty)
			{
				RefCountry country = RefCountry.LoadFromCountryCode(Factory, Sendingarnumer.PortOfLoadingCountryCode);

				if (country == null)
				{
					Consol.JK_CRNInfo.AddError(Res.GetString("6605bbb0-5a97-4ca1-b61a-c8362c3669a4", "Unable to find Country/Region with Country/Region Code: {0}", Sendingarnumer.PortOfLoadingCountryCode));
				}
				else if (!Sendingarnumer.PortOfLoadingPortCode.IsEmpty)
				{
					if (Sendingarnumer.PortOfLoadingUNLOCO == null)
					{
						Consol.JK_CRNInfo.AddError(Res.GetString("9ae9c846-ad7e-420d-834d-4a1341247619", "Country/Region ({0}) {1} does not have port code {2}", Sendingarnumer.PortOfLoadingCountryCode, country.RN_DescMultilingual, Sendingarnumer.PortOfLoadingPortCode));
					}
					else if (Sendingarnumer.IsAir && Sendingarnumer.IsImport && Consol.ShippingLine != null && Consol.ShippingLine.MiscServ.Airline != null)
					{
						RefAirline airline = Consol.ShippingLine.MiscServ.Airline;

						ZDBOnlyQuery transportLegFilter = new ZDBOnlyQuery(typeof(JobVoyage));
						transportLegFilter.AddToFilter(JobVoyageSchema.JV_VoyageFlight, airline.RM_TwoCharacterCode + Sendingarnumer.VesselCodeFlightNumber);
						transportLegFilter.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Core.Constants.TransportModes.Air);

						ZDBOnlySubQuery portOfDischargeFilter = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.JB_JV);
						portOfDischargeFilter.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
						portOfDischargeFilter.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.EqualToDatePartOnly, Sendingarnumer.ArrivalDepartureDate);
						transportLegFilter.AddSubQuery(JobVoyageSchema.PK, portOfDischargeFilter, JoinCondition.And);

						ZDBOnlySubQuery portOfLoadingFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.JA_JV);
						portOfLoadingFilter.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, Sendingarnumer.PortOfLoadingUNLOCO.Code);
						transportLegFilter.AddSubQuery(JobVoyageSchema.PK, portOfLoadingFilter, JoinCondition.And);

						if (Factory.LoadTop1<JobVoyage>(transportLegFilter) == null)
						{
							Consol.JK_CRNInfo.AddError(Res.GetString("b1661fdd-a0e1-4938-9d6c-68c57cecc25e", "Unable to find Flight ({0}) with Port of Loading ({1}) and Estimated Date of Arrival ({2})", airline.RM_TwoCharacterCode + Sendingarnumer.VesselCodeFlightNumber, Sendingarnumer.PortOfLoadingUNLOCO.Code, Sendingarnumer.ArrivalDepartureDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)));
						}
					}
				}
			}
		}

		void ValidateCarrierNumber()
		{
			if (!Sendingarnumer.CarrierNumber.IsEmpty && Sendingarnumer.IsAir && !Sendingarnumer.CarrierNumber.IsNumbersOnlyOrEmpty)
			{
				Consol.JK_CRNInfo.AddError(Res.GetString("90366377-0e69-490b-9271-78808d1a04e5", "The Carrier Number for Air type should only be numeric"));
			}
		}

		void ValidateCheckDigit()
		{
			if (Sendingarnumer.HasValidLength && !Sendingarnumer.CheckDigit.IsEmpty)
			{
				ZString validCheckDigit = Sendingarnumer.GenerateCheckDigitFromCode();
				if (validCheckDigit != Sendingarnumer.CheckDigit)
				{
					Consol.JK_CRNInfo.AddError(Res.GetString("34ea06b1-c05c-4871-aac5-8b2cf986cf77", "The Check Digit should be '{0}'", validCheckDigit));
				}
			}
		}

		#endregion

		#region AutoPopulation

		void AutoPopulateConsol()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			AutoPopulateCarrier();
			AutoPopulateTransportLeg();
		}

		void AutoPopulateTransportLeg()
		{
			if (Consol.ShippingLine != null &&
				Consol.ShippingLine.MiscServ != null &&
				Consol.ShippingLine.MiscServ.Airline != null &&
				!Sendingarnumer.VesselCodeFlightNumber.IsEmpty &&
				Sendingarnumer.ArrivalDepartureDate.IsValid &&
				Sendingarnumer.PortOfLoadingUNLOCO != null)
			{
				RefAirline airline = Consol.ShippingLine.MiscServ.Airline;

				ZDBOnlySubQuery voyageFilter = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyageSchema.PK);
				voyageFilter.AddToFilter(JobVoyageSchema.JV_VoyageFlight, airline.RM_TwoCharacterCode + Sendingarnumer.VesselCodeFlightNumber);
				voyageFilter.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Core.Constants.TransportModes.Air);

				ZDBOnlySubQuery destinationFilter = new ZDBOnlySubQuery(typeof(VoyageDestination), JobVoyDestinationSchema.PK);
				destinationFilter.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, SQLComparisonOperator.StartsWith, Iceland.Code);
				destinationFilter.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.EqualToDatePartOnly, Sendingarnumer.ArrivalDepartureDate);
				destinationFilter.AddSubQuery(JobVoyDestinationSchema.JB_JV, voyageFilter, JoinCondition.And);

				ZDBOnlySubQuery originFilter = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.PK);
				originFilter.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, Sendingarnumer.PortOfLoadingUNLOCO.Code);
				originFilter.AddSubQuery(JobVoyOriginSchema.JA_JV, voyageFilter, JoinCondition.And);

				ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
				sailingFilter.AddSubQuery(JobSailingSchema.JX_JB, destinationFilter, JoinCondition.And);
				sailingFilter.AddSubQuery(JobSailingSchema.JX_JA, originFilter, JoinCondition.And);

				JobSailing firstSail = Factory.LoadTop1<JobSailing>(sailingFilter);
				if (firstSail != null)
				{
					Consol.Transports.MostInterestingTransport.JW_JX = firstSail.PK;
					Consol.Transports.MostInterestingTransport.JW_VoyageFlight = firstSail.JX_JV_VoyageFlight;
					Consol.Transports.MostInterestingTransport.JW_ETA = firstSail.JX_JB_E_ARV;
					Consol.Transports.MostInterestingTransport.JW_ATA = firstSail.JX_JB_A_ARV;
					Consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = firstSail.JX_JA_RL_NKPortOfLoading;
					Consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = firstSail.JX_JB_RL_NKPortOfDischarge;
				}
				else
				{
					ClearTransportLeg();
				}
			}
		}

		void ClearTransportLeg()
		{
			Consol.Transports.MostInterestingTransport.JW_VoyageFlight = ZString.Empty;
			Consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = ZString.Empty;
			Consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = ZString.Empty;
			Consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Empty;
			Consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Empty;
			Consol.Transports.MostInterestingTransport.JW_ATA = ZDateTime.Empty;
			Consol.Transports.MostInterestingTransport.JW_ATD = ZDateTime.Empty;
		}

		void AutoPopulateCarrier()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Iceland.Code);
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, Sendingarnumer.CarrierCode);

			OrgCusCode carrierCusCode = Factory.LoadTop1<OrgCusCode>(query);
			if (carrierCusCode != null)
			{
				Consol.SetDefaultShippingLineAddress(carrierCusCode.OK_OH);
			}
		}

		#endregion

		#endregion
	}
}
