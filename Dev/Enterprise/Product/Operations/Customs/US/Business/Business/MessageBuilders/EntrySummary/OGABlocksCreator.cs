using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	class OGABlocksCreator
	{
		int currentFDALineNumber;
		int currentDOTLineNumber;

		public IEnumerable<MessageBlock> GetDisclaimingBlocks(IOGA entryLine, bool certifyCargoRelease)
		{
			OGAOA ogaOA = GetDisclaimBlock(entryLine, certifyCargoRelease);

			if (ogaOA != null)
			{
				yield return ogaOA;
			}
		}

		public IEnumerable<MessageBlock> GetOGABlocks(IOGA entryLine, bool certifyCargoRelease, string applicationIdentifier)
		{
			foreach (MessageBlock block in CreateDOTBlocks(entryLine, certifyCargoRelease))
			{
				yield return block;
			}

			foreach (MessageBlock block in CreateFDABlocks(entryLine, certifyCargoRelease))
			{
				yield return block;
			}
		}

		public IEnumerable<MessageBlock> CreateDOTBlocks(IOGA entryLine, bool certifyCargoRelease)
		{
			if (certifyCargoRelease)
			{
				if (OGAIndicatorList.IsToBeDeclared(entryLine.DOTIndicator))
				{
					currentDOTLineNumber = 0;
					foreach (IDOT dot in entryLine.DOT)
					{
						bool dT01Created = true;

						currentDOTLineNumber++;
						yield return MakeOGAOI(dot.CommercialDescription);
						yield return MakeOGADT01(dot, currentDOTLineNumber);

						foreach (IDOTVIN dotvin in dot.VINs)
						{
							if (!dT01Created)
							{
								currentDOTLineNumber++;
								yield return MakeOGADT01(dot, currentDOTLineNumber);
							}

							OGADT02 dt02 = MakeOGADT02(dotvin);
							if (!dt02.IsEmpty)
							{
								yield return dt02;
								dT01Created = false;
							}
						}
					}
				}
			}
		}

		public IEnumerable<MessageBlock> CreateFDABlocks(IOGA entryLine, bool certifyCargoRelease)
		{
			if (certifyCargoRelease)
			{
				if (OGAIndicatorList.IsToBeDeclared(entryLine.FDAIndicator))
				{
					currentFDALineNumber = 0;

					foreach (IPriorNoticeLine fda in entryLine.FDA)
					{
						currentFDALineNumber++;
						foreach (MessageBlock fdaBlock in GetFDABlocksForOneLine(fda, currentFDALineNumber, false))
						{
							yield return fdaBlock;
						}
					}
				}
			}
		}

		OGAOA GetDisclaimBlock(IOGA entryLine, bool certifyCargoRelease)
		{
			OGAOA result = null;

			List<string> disclaims = new List<string>();

			if (certifyCargoRelease)
			{
				if (OGAIndicatorList.IsToBeDisclaimed(entryLine.DOTIndicator))
				{
					disclaims.Add("DT0");
				}

				if (OGAIndicatorList.IsToBeDisclaimed(entryLine.FDAIndicator))
				{
					disclaims.Add("FD0");
				}
			}

			if (disclaims.Count > 0)
			{
				result = new OGAOA();

				for (int index = 0; index < disclaims.Count; index++)
				{
					string disclaim = disclaims[index];
					switch (index)
					{
						case 0:
							result.OtherAgencyDeclaration = disclaim;
							break;
						case 1:
							result.OtherAgencyDeclaration1 = disclaim;
							break;
						case 2:
							result.OtherAgencyDeclaration2 = disclaim;
							break;
					}
				}
			}

			return result;
		}

		public IEnumerable<MessageBlock> GetFDABlocksForOneLine(IPriorNoticeLine priorNoticeLine, int fdaLineNumber, bool includeStandAloneDetails)
		{
			List<KeyValuePair<ZString, ZString>> affirmationCodes = GetSortedAffirmationCodes(priorNoticeLine, includeStandAloneDetails);

			List<FDAQtyUQPair> orderedQty = priorNoticeLine.OrderedQtyUQs;

			yield return MakeOGAOI(priorNoticeLine.CommercialDescription);
			yield return MakeOGAFD01(priorNoticeLine, affirmationCodes, fdaLineNumber);
			yield return MakeOGAFD02(priorNoticeLine, orderedQty);
			yield return MakeOGAFD03(priorNoticeLine);

			OGAFD04 fd4 = MakeOGAFD04(priorNoticeLine, orderedQty);
			if (fd4 != null)
			{
				yield return fd4;
			}

			for (int i = 1; i < affirmationCodes.Count; i++)
			{
				yield return MakeOGAFD05(affirmationCodes[i]);
			}
		}

		List<KeyValuePair<ZString, ZString>> GetSortedAffirmationCodes(IPriorNoticeLine priorNoticeLine, bool includeStandAloneDetails)
		{
			List<KeyValuePair<ZString, ZString>> result = new List<KeyValuePair<ZString, ZString>>();

			// If the PNC or PND affirmation of compliance codes are required, they must be first so as to be built into the FD01 segment
			// with remaining affirmation codes built into repeating FD05 segments.
			if (priorNoticeLine.RequiresPriorNotice)
			{
				if (!priorNoticeLine.ConfirmationNumber.IsEmpty)
				{
					AddIfNotExists(result, AffirmationCodeConstants.Codes.PNC, priorNoticeLine.ConfirmationNumber);
					foreach (AffirmationCode affirmationCode in priorNoticeLine.AffirmationCodes)
					{
						AddIfNotExists(result, affirmationCode.CY_Code, affirmationCode.CY_Data, true);
					}
				}
				else if (priorNoticeLine.IsDisclaimed)
				{
					Add(result, AffirmationCodeConstants.Codes.PND, "");

					foreach (AffirmationCode affirmationCode in priorNoticeLine.AffirmationCodes)
					{
						AddIfNotExists(result, affirmationCode.CY_Code, affirmationCode.CY_Data, true);
					}
				}
				else
				{
					AddLineLevelAffirmationCodes(result, priorNoticeLine, includeStandAloneDetails);
					AddHeaderLevelAffirmationCodes(result, priorNoticeLine.PriorNoticeHeader, includeStandAloneDetails);

					result.Sort(new AffirmationCodeComparer());

					AddLineLevelBillAffirmationCodes(result, priorNoticeLine);

					ZString transmitterEmail = priorNoticeLine.ContactEmail.IsEmpty ? "none" : priorNoticeLine.ContactEmail.Left(25).ToString();
					AddIfNotExists(result, AffirmationCodeConstants.Codes.TEM, transmitterEmail);
				}
			}
			else//for non-prior notice FDA reporting
			{
				foreach (AffirmationCode affirmationCode in priorNoticeLine.AffirmationCodes)
				{
					AddIfNotExists(result, affirmationCode.CY_Code, affirmationCode.CY_Data, true);
				}

				result.Sort(new AffirmationCodeComparer());
			}

			return result;
		}

		void AddLineLevelAffirmationCodes(List<KeyValuePair<ZString, ZString>> affirmationCodes, IPriorNoticeLine priorNoticeLine, bool includeStandAloneDetails)
		{
			AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.OFT, priorNoticeLine.OwnerFirmType);
			AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.CSH, priorNoticeLine.CountryOfShipping);
			AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.PFR, priorNoticeLine.FoodFacilityRegistrationNumber);
			AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.FME, priorNoticeLine.FoodFacilityRegistrationExemption);
			AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.PFT, priorNoticeLine.ProducerFirmType);
			AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.SFR, priorNoticeLine.ShipperRegistrationNumber);

			foreach (ZString railCarNumber in priorNoticeLine.RailCarNumbers)
			{
				AddIfNotEmpty(affirmationCodes, AffirmationCodeConstants.Codes.RNO, railCarNumber);
			}

			foreach (AffirmationCode affirmationCode in priorNoticeLine.AffirmationCodes)
			{
				AddIfNotExists(affirmationCodes, affirmationCode.CY_Code, affirmationCode.CY_Data, true);
			}

			foreach (ZString containerNumber in priorNoticeLine.ContainerNumbers)
			{
				AddIfNotEmpty(affirmationCodes, AffirmationCodeConstants.Codes.CNO, containerNumber.Left(14));
			}

			if (includeStandAloneDetails)
			{
				AddIfNotExists(affirmationCodes, StandAlonePriorNotice.Codes.HarmonizedTariffNumber, priorNoticeLine.HarmonizedTariffNumber);
				AddImporterAffirmationCodes(affirmationCodes, priorNoticeLine);
				AddUltimateConsigneeAffirmationCodes(affirmationCodes, priorNoticeLine);
			}
		}

		void AddLineLevelBillAffirmationCodes(List<KeyValuePair<ZString, ZString>> affirmationCodes, IPriorNoticeLine priorNoticeLine)
		{
			foreach (IMasterHouse masterHouse in priorNoticeLine.Bills)
			{
				ZString formattedMaster = masterHouse.MasterBill.KeepAlphanumericCharacters();
				ZString formattedHouse = masterHouse.HouseBill.KeepAlphanumericCharacters().Left(12);

				if (priorNoticeLine.PriorNoticeHeader.IsSurface)
				{
					if (!formattedMaster.IsEmpty)
					{
						AddIfNotExistsKeyAndValue(affirmationCodes, AffirmationCodeConstants.Codes.BOL, masterHouse.MasterBillSCAC + formattedMaster.Left(12));

						if (!formattedHouse.IsEmpty)
						{
							AddIfNotExistsKeyAndValue(affirmationCodes, AffirmationCodeConstants.Codes.NHB, masterHouse.HouseBillSCAC + formattedHouse);
						}
					}
				}
				else
				{
					if (!formattedMaster.IsEmpty)
					{
						AddIfNotExistsKeyAndValue(affirmationCodes, AffirmationCodeConstants.Codes.AWB, formattedMaster);

						if (!formattedHouse.IsEmpty)
						{
							AddIfNotExistsKeyAndValue(affirmationCodes, AffirmationCodeConstants.Codes.AWH, formattedHouse);
						}
					}
				}
			}
		}

		void AddHeaderLevelAffirmationCodes(List<KeyValuePair<ZString, ZString>> affirmationCodes, IPriorNoticeHeader priorNoticeHeader, bool includeStandAloneDetails)
		{
			AddSubmitterAffirmationCodes(affirmationCodes, priorNoticeHeader, includeStandAloneDetails);

			AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.APA, priorNoticeHeader.PortOfArrival);
			AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.ADA, priorNoticeHeader.DateOfArrival.ToString("MMddyyyy"));
			AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.APC, priorNoticeHeader.AnticipatedPortOfCrossing);
			AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.ATA, priorNoticeHeader.TimeOfArrival.ToString("HHmm"));

			if (priorNoticeHeader.EntryType == EntryTypeList.Codes.WarehouseFTZ)
			{
				AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.ETP, EntryTypeList.Codes.WarehouseFTZ);
			}

			if (TransportTypeList.IsVoyageFlightNoMandatory(priorNoticeHeader.ModeOfTransportationCode))
			{
				AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.VFT, priorNoticeHeader.VoyageFlightNumber);
			}

			if (priorNoticeHeader.CarrierType == FDACarrierTypeList.Codes.Carrier)
			{
				AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.CAN, priorNoticeHeader.CarrierName);
				AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.CCN, priorNoticeHeader.CarrierCountry);
			}
			else if (priorNoticeHeader.CarrierType == FDACarrierTypeList.Codes.PrivatelyOwnedUSVehicle)
			{
				AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.PVL, priorNoticeHeader.CarrierName);
				AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.PVS, priorNoticeHeader.CarrierCountry);
			}
			else if (priorNoticeHeader.CarrierType == FDACarrierTypeList.Codes.PrivatelyOwnedFNVehicle)
			{
				AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.PVP, priorNoticeHeader.CarrierName);
				AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.PVC, priorNoticeHeader.CarrierCountry);
			}

			if (includeStandAloneDetails && priorNoticeHeader.EntryType != EntryTypeList.Codes.WarehouseFTZ)
			{
				AddIfNotExists(affirmationCodes, StandAlonePriorNotice.Codes.EntryFilerCode, priorNoticeHeader.EntryFilerCode);
				AddIfNotExists(affirmationCodes, StandAlonePriorNotice.Codes.EntryNumberLast8Digits, priorNoticeHeader.EntryNumberLast8Digits);
			}

			if (includeStandAloneDetails)
			{
				AddIfNotExists(affirmationCodes, StandAlonePriorNotice.Codes.EntryType, priorNoticeHeader.EntryType);
				AddIfNotExists(affirmationCodes, StandAlonePriorNotice.Codes.ModeOfTransportationCode, priorNoticeHeader.ModeOfTransportationCode);
				AddIfNotExists(affirmationCodes, StandAlonePriorNotice.Codes.FTZAdmissionNumber, priorNoticeHeader.FTZAdmissionNumber);
				AddIfNotExists(affirmationCodes, StandAlonePriorNotice.Codes.LocationOfGoodsFIRMSCode, priorNoticeHeader.LocationOfGoodsFIRMSCode);

				ZString scaAffirmationCode = priorNoticeHeader.ImportingAirCarrier3LetterCode.IsEmpty ? priorNoticeHeader.ImportingCarrierSCAC : priorNoticeHeader.ImportingAirCarrier3LetterCode;
				AddIfNotExists(affirmationCodes, StandAlonePriorNotice.Codes.ImportingCarrier, scaAffirmationCode);
			}
		}

		void AddImporterAffirmationCodes(List<KeyValuePair<ZString, ZString>> affirmationCodes, IPriorNoticeLine priorNoticeLine)
		{
			var organisation = priorNoticeLine.Importer;
			if (organisation != null)
			{
				AddIfNotExists(affirmationCodes, StandAlonePriorNotice.Codes.ImporterNumber, OrgHeaderWrapper.GetCustomsRelatedCode(organisation, OrgMatchedCustomsRegNoType.EIN));
				AddOrganisationAffirmationCodes(affirmationCodes, organisation,
					StandAlonePriorNotice.Codes.ImporterFirmName,
					StandAlonePriorNotice.Codes.ImporterFirstName,
					StandAlonePriorNotice.Codes.ImporterLastName,
					StandAlonePriorNotice.Codes.ImporterAddressLine1,
					StandAlonePriorNotice.Codes.ImporterAddressLine2,
					StandAlonePriorNotice.Codes.ImporterAddressCity,
					StandAlonePriorNotice.Codes.ImporterPhoneNumber,
					StandAlonePriorNotice.Codes.ImporterAddressStateOrCanadianProvince,
					StandAlonePriorNotice.Codes.ImporterZipOrMailCode,
					StandAlonePriorNotice.Codes.ImporterISOCountryCode,
					StandAlonePriorNotice.Codes.ImporterFax,
					StandAlonePriorNotice.Codes.ImporterEmail);
			}
		}

		void AddUltimateConsigneeAffirmationCodes(List<KeyValuePair<ZString, ZString>> affirmationCodes, IPriorNoticeLine priorNoticeLine)
		{
			var organisation = priorNoticeLine.Consignee;
			if (organisation != null)
			{
				AddIfNotExists(affirmationCodes, StandAlonePriorNotice.Codes.ConsigneeNumber, OrgHeaderWrapper.GetCustomsRelatedCode(organisation, OrgMatchedCustomsRegNoType.EIN));
				AddOrganisationAffirmationCodes(affirmationCodes, organisation,
					StandAlonePriorNotice.Codes.ConsigneeFirmName,
					StandAlonePriorNotice.Codes.ConsigneeFirstName,
					StandAlonePriorNotice.Codes.ConsigneeLastName,
					StandAlonePriorNotice.Codes.ConsigneeAddressLine1,
					StandAlonePriorNotice.Codes.ConsigneeAddressLine2,
					StandAlonePriorNotice.Codes.ConsigneeAddressCity,
					StandAlonePriorNotice.Codes.ConsigneePhoneNumber,
					StandAlonePriorNotice.Codes.ConsigneeAddressStateOrCanadianProvince,
					StandAlonePriorNotice.Codes.ConsigneeZipOrMailCode,
					StandAlonePriorNotice.Codes.ConsigneeISOCountryCode,
					StandAlonePriorNotice.Codes.ConsigneeFax,
					StandAlonePriorNotice.Codes.ConsigneeEmail);
			}
		}

		void AddSubmitterAffirmationCodes(List<KeyValuePair<ZString, ZString>> affirmationCodes, IPriorNoticeHeader priorNoticeHeader, bool includeStandAloneDetails)
		{
			var organisation = priorNoticeHeader.Submitter;
			if (organisation != null)
			{
				AddOrganisationAffirmationCodes(affirmationCodes, organisation,
					AffirmationCodeConstants.Codes.SCN,
					AffirmationCodeConstants.Codes.SFN,
					AffirmationCodeConstants.Codes.SLN,
					AffirmationCodeConstants.Codes.SA1,
					AffirmationCodeConstants.Codes.SA2,
					AffirmationCodeConstants.Codes.SAC,
					AffirmationCodeConstants.Codes.SPN,
					AffirmationCodeConstants.Codes.SAS,
					AffirmationCodeConstants.Codes.SCZ,
					AffirmationCodeConstants.Codes.SCC,
					AffirmationCodeConstants.Codes.SFX,
					AffirmationCodeConstants.Codes.SEM);
				var orgWrapper = OrgHeaderWrapper.New(organisation);
				if (orgWrapper != null)
				{
					AddIfNotExists(affirmationCodes, AffirmationCodeConstants.Codes.SFT, orgWrapper.ZO_SubmitterFirmType);
					if (includeStandAloneDetails && ((IPGAContactDetails)orgWrapper).Name.GetLastName().IsEmpty)
					{
						Add(affirmationCodes, AffirmationCodeConstants.Codes.SLN, "");
					}
				}
			}
		}

		void AddOrganisationAffirmationCodes(List<KeyValuePair<ZString, ZString>> affirmationCodes, OrgHeader organisation, ZString firmNameCode, ZString firstNameCode, ZString lastNameCode, ZString address1Code, ZString address2Code, ZString cityCode, ZString phoneNumberCode, ZString stateOrProvinceCode, ZString zipOrMailCode, ZString iSOCountryCode, ZString faxCode, ZString emailCode)
		{
			if (organisation != null)
			{
				var orgWrapper = OrgHeaderWrapper.New(organisation);
				if (orgWrapper != null)
				{
					var pgaContactDetails = (IPGAContactDetails)orgWrapper;
					AddIfNotExists(affirmationCodes, lastNameCode, pgaContactDetails.Name.GetLastName());
					AddIfNotExists(affirmationCodes, firstNameCode, pgaContactDetails.Name.GetFirstName());
					AddIfNotExists(affirmationCodes, phoneNumberCode, orgWrapper.ContactPhoneNoForFDA);
					AddIfNotExists(affirmationCodes, emailCode, orgWrapper.ContactEmailForFDA.Left(25));
					AddIfNotExists(affirmationCodes, faxCode, orgWrapper.ContactFaxForFDA);
				}

				IAddressDetails organisationAdddressDetails = organisation;
				AddIfNotExists(affirmationCodes, address1Code, organisationAdddressDetails.AddressLine1);
				AddIfNotExists(affirmationCodes, address2Code, organisationAdddressDetails.AddressLine2);
				AddIfNotExists(affirmationCodes, cityCode, organisationAdddressDetails.City);

				ZString addressStateOrProvince = organisationAdddressDetails.State;

				ZBool isUS = organisationAdddressDetails.Country == Core.Constants.CountryCodes.UnitedStates;
				if (!isUS)
				{
					if (organisationAdddressDetails.Country != Core.Constants.CountryCodes.Canada)
					{
						addressStateOrProvince = "FN";
					}
				}

				AddIfNotExists(affirmationCodes, stateOrProvinceCode, addressStateOrProvince);
				AddIfNotExists(affirmationCodes, zipOrMailCode, organisationAdddressDetails.PostCode);
				AddIfNotExists(affirmationCodes, iSOCountryCode, organisationAdddressDetails.Country);
				AddIfNotExists(affirmationCodes, firmNameCode, organisationAdddressDetails.CompanyName);
			}
		}

		void AddIfNotExists(List<KeyValuePair<ZString, ZString>> affirmationCodes, ZString key, ZString value)
		{
			AddIfNotExists(affirmationCodes, key, value, false);
		}

		void AddIfNotExists(List<KeyValuePair<ZString, ZString>> affirmationCodes, ZString key, ZString value, bool allowEmptyValue)
		{
			if (!key.IsEmpty && (allowEmptyValue || !value.IsEmpty))
			{
				bool found = false;

				foreach (KeyValuePair<ZString, ZString> keyValuePair in affirmationCodes)
				{
					if (keyValuePair.Key == key)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					affirmationCodes.Add(new KeyValuePair<ZString, ZString>(key, value));
				}
			}
		}

		void AddIfNotExistsKeyAndValue(List<KeyValuePair<ZString, ZString>> affirmationCodes, ZString key, ZString value)
		{
			AddIfNotExistsKeyAndValue(affirmationCodes, key, value, false);
		}

		void AddIfNotExistsKeyAndValue(List<KeyValuePair<ZString, ZString>> affirmationCodes, ZString key, ZString value, bool allowEmptyValue)
		{
			if (!key.IsEmpty && (allowEmptyValue || !value.IsEmpty))
			{
				bool found = false;

				foreach (KeyValuePair<ZString, ZString> keyValuePair in affirmationCodes)
				{
					if (keyValuePair.Key == key && keyValuePair.Value == value)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					affirmationCodes.Add(new KeyValuePair<ZString, ZString>(key, value));
				}
			}
		}

		void AddIfNotEmpty(List<KeyValuePair<ZString, ZString>> affirmationCodes, ZString key, ZString value)
		{
			if (!key.IsEmpty && !value.IsEmpty)
			{
				affirmationCodes.Add(new KeyValuePair<ZString, ZString>(key, value));
			}
		}

		void Add(List<KeyValuePair<ZString, ZString>> affirmationCodes, ZString key, ZString value)
		{
			affirmationCodes.Add(new KeyValuePair<ZString, ZString>(key, value));
		}

		AENSOI MakeOGAOI(ZString commercialDescription)
		{
			AENSOI result = new AENSOI();
			result.CommercialDescriptionText = commercialDescription.Left(70);
			return result;
		}

		#region FCC

		#endregion

		#region FDA

		OGAFD01 MakeOGAFD01(IPriorNoticeLine fda, List<KeyValuePair<ZString, ZString>> affirmationCodes, int fdaLineNumber)
		{
			OGAFD01 result = new OGAFD01();

			if (affirmationCodes.Count > 0)
			{
				result.AffirmationOfComplianceCode = affirmationCodes[0].Key.Left(3);
				result.AffirmationOfComplianceQualifier = affirmationCodes[0].Value;
			}

			fda.FDALineNumber = fdaLineNumber;
			result.FDALineNumber = fdaLineNumber;
			result.FDAProductCode = fda.FDAProductCode;

			result.CargoStorageStatus = fda.CargoStorageStatus;
			result.FDAActualManufacturerNumber = fda.ManufacturerNumber.Left(15);
			result.FDAActualShipperSupplierNumber = fda.SupplierOrShipperNumber.Left(15);
			result.FDACountryOfProduction = fda.CountryOfProduction;

			return result;
		}

		OGAFD02 MakeOGAFD02(IPriorNoticeLine fda, List<FDAQtyUQPair> orderedQty)
		{
			OGAFD02 result = new OGAFD02();
			if (orderedQty.Count > 0)
			{
				result.Unit1Quantity = orderedQty[0].Qty.Round(USConstants.FDA.RoundToDecimalPlaces);
				result.Unit1Measure = orderedQty[0].UQ;
			}

			if (orderedQty.Count > 1)
			{
				result.Unit2Quantity = orderedQty[1].Qty.Round(USConstants.FDA.RoundToDecimalPlaces);
				result.Unit2Measure = orderedQty[1].UQ;
			}

			if (orderedQty.Count > 2)
			{
				result.Unit3Quantity = orderedQty[2].Qty.Round(USConstants.FDA.RoundToDecimalPlaces);
				result.Unit3Measure = orderedQty[2].UQ;
			}

			if (orderedQty.Count > 3)
			{
				result.Unit4Quantity = orderedQty[3].Qty.Round(USConstants.FDA.RoundToDecimalPlaces);
				result.Unit4Measure = orderedQty[3].UQ;
			}

			if (orderedQty.Count > 4)
			{
				result.Unit5Quantity = orderedQty[4].Qty.Round(USConstants.FDA.RoundToDecimalPlaces);
				result.Unit5Measure = orderedQty[4].UQ;
			}
			return result;
		}

		OGAFD03 MakeOGAFD03(IPriorNoticeLine fda)
		{
			OGAFD03 result = new OGAFD03();

			result.ContainerDimension1 = GetContainerDimension(fda.FirstDimension, fda.DimensionUQ);
			result.ContainerDimensions2 = GetContainerDimension(fda.SecondDimension, fda.DimensionUQ);
			result.ContainerDimensions3 = GetContainerDimension(fda.ThirdDimension, fda.DimensionUQ);
			result.FDAConsigneeFDAEstablishmentIndicatorFEI = fda.ConsigneeFEI;
			result.FDAValueByFDALine = fda.ValueInWholeDollars;
			result.TradeOrBrandName = fda.TradeOrBrandName;

			return result;
		}

		public ZString GetContainerDimension(decimal qty, ZString uQ)
		{
			ZString result = ZString.Empty;

			ZDecimal value = FDAMeasurementUnitList.ConvertToInchesWithOneSixteenth(qty, uQ);

			if (value > 0)
			{
				ZDecimal theLastTwoDigits = (value * 100) % 100;

				result = value.Truncate(0).ToString(0).PadLeft(2, '0') + theLastTwoDigits.Truncate(0).ToString(0).PadLeft(2, '0');
			}

			return result;
		}

		OGAFD04 MakeOGAFD04(IPriorNoticeLine fda, List<FDAQtyUQPair> orderedQty)
		{
			OGAFD04 result = null;

			if (!fda.ContactName.IsEmpty || !fda.ContactPhone.IsEmpty || orderedQty.Count > 5)
			{
				result = new OGAFD04();
				result.ContactName = fda.ContactName.Left(10);
				result.ContactTelephoneNumber = fda.ContactPhone.Left(10);

				if (orderedQty.Count > 5)
				{
					result.Unit6Quantity = orderedQty[5].Qty;
					result.Unit6Measure = orderedQty[5].UQ;
				}
			}

			return result;
		}

		OGAFD05 MakeOGAFD05(KeyValuePair<ZString, ZString> affirmationCode)
		{
			OGAFD05 result = new OGAFD05();
			result.AffirmationOfComplianceCode = affirmationCode.Key.Left(3);
			result.AffirmationOfComplianceQualifier = affirmationCode.Value.Left(25);

			return result;
		}

		#endregion

		#region DOT

		OGADT01 MakeOGADT01(IDOT dot, ZInt dOTLineNumber)
		{
			OGADT01 result = new OGADT01();
			result.BoxCertification = dot.BoxCertification;
			result.BoxNumber = dot.BoxNumber;
			result.ClarificationCode = dot.ClarificationCode;
			result.CountryISO = dot.CountryISO;
			result.DOTBondSuretyCode = dot.DOTBondSuretyCode;

			result.DOTLineNumber = dOTLineNumber;

			result.ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter = dot.ImportersSubstantiatingStatementCopyOfContractManufacturersConfirmationLetter ? "Y" : "";
			result.NHTSAPermissionLetterOfficialOrdersCertification = dot.NHTSAPermissionLetterOfficialOrdersCertification ? "Y" : "";
			result.PassportNumber = dot.PassportNumber;
			result.TireManufacturerBrandName = dot.TireManufacturerBrandName;
			result.TireManufacturerIDCode = dot.TireManufacturerIDCode;
			return result;
		}

		OGADT02 MakeOGADT02(IDOTVIN dotvin)
		{
			OGADT02 result = new OGADT02();
			result.MakeOfVehicle = dotvin.MakeOfVehicle;
			result.Model = dotvin.Model;
			result.NHTSARegisteredImporterRINumber = dotvin.NHTSARegisteredImporterRINumber;
			result.VehicleEligibilityNumber = dotvin.VehicleEligibilityNumber;
			result.VehicleIdentificationNumber = dotvin.VehicleIdentificationNumber;
			result.Year = dotvin.Year;
			return result;
		}

		#endregion
	}
}
