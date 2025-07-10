using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public class PGABlocksCreator
	{
		int ePALineNumberCounter;
		int fSISNumberCounter;
		int nMFSNumberCounter;
		int fDALineNumberCounter;
		int tTBNumberCounter;
		int nHTSANumberCounter;
		int aMSLineNumberCounter;
		int aPHLineNumberCounter;
		int fWSNumberCounter;
		int aTFNumberCounter;
		int cPSCNumberCounter;
		int oMCNumberCounter;
		int dEANumberCounter;

		public static IEnumerable<MessageBlock> BuildPGABlocks(IGovernmentAgenciesCommon line, IAcknowledgeAndSign signed, bool enablePGATracking, bool isPGACorrection = false, bool buildOI = true, bool buildAllPGAs = false)
		{
			var result = new List<MessageBlock>();
			if (line != null && signed != null)
			{
				var isCertified = signed.US_CertifyCargoRelease;
				var pgaIncludedInTrackingList = new List<Tuple<IPGADataCorrection, bool>>();
				var creator = new PGABlocksCreator();
				creator.GetDefaultPGAStartLineNumbers(line);
				result.AddRange(creator.GenerateEPA_ODSOrTSCAData(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetVNEBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetPSTBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetFSISBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetNMFSBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetDDTCBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetFDABlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetTTBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetNHTSABlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetAMSBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetAPHISBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetOMCBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetACE_LaceyActBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetFWSBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GenerateATFData(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetCPSCBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetDEABlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				result.AddRange(creator.GetHFCBlocks(line, pgaIncludedInTrackingList, isCertified, isPGACorrection, buildAllPGAs));
				creator.SetPGAStartLineNumbers(line);

				if (result.Count > 0 && buildOI)
				{
					result.Insert(0, AEPAPGBlockHelper.MakePGAOI(line.CommercialDescription));
				}

				if (enablePGATracking)
				{
					foreach (var pgaLine in pgaIncludedInTrackingList)
					{
						if (pgaLine.Item1 != null)
						{
							pgaLine.Item1.MarkStatusAsSubmittingToCustoms(pgaLine.Item2);
						}
					}
				}
			}

			return result;
		}

		void GetDefaultPGAStartLineNumbers(IGovernmentAgenciesCommon line)
		{
			ePALineNumberCounter = line.EPAStartLineNumber;
			fSISNumberCounter = line.FSISStartLineNumber;
			nMFSNumberCounter = line.NMFSStartLineNumber;
			fDALineNumberCounter = line.FDAStartLineNumber;
			tTBNumberCounter = line.TTBStartLineNumber;
			nHTSANumberCounter = line.NHTSAStartLineNumber;
			aMSLineNumberCounter = line.AMSStartLineNumber;
			aPHLineNumberCounter = line.APHStartLineNumber;
			fWSNumberCounter = line.FWSStartLineNumber;
			aTFNumberCounter = line.ATFStartLineNumber;
			cPSCNumberCounter = line.CPSCStartLineNumber;
			dEANumberCounter = line.DEAStartLineNumber;
		}

		void SetPGAStartLineNumbers(IGovernmentAgenciesCommon line)
		{
			line.EPAStartLineNumber = ePALineNumberCounter;
			line.FSISStartLineNumber = fSISNumberCounter;
			line.NMFSStartLineNumber = nMFSNumberCounter;
			line.FDAStartLineNumber = fDALineNumberCounter;
			line.TTBStartLineNumber = tTBNumberCounter;
			line.NHTSAStartLineNumber = nHTSANumberCounter;
			line.AMSStartLineNumber = aMSLineNumberCounter;
			line.APHStartLineNumber = aPHLineNumberCounter;
			line.FWSStartLineNumber = fWSNumberCounter;
			line.ATFStartLineNumber = aTFNumberCounter;
			line.CPSCStartLineNumber = cPSCNumberCounter;
			line.DEAStartLineNumber = dEANumberCounter;
		}

		bool ShouldSendPGA(IGovernmentAgencies entryLine, ZBool isCertified, ZBool isPGACorrection, ZBool buildAllPGAs, ZString pgaCode)
		{
			var result = isPGACorrection || buildAllPGAs;
			if (!result)
			{
				result = entryLine.ShouldIncludePGAInMessage(isCertified, pgaCode);
			}
			return result;
		}

		IEnumerable<MessageBlock> GenerateBlocks<T>(ZString indicator, IEnumerable<T> pgaLines, IEnumerable<IPGADataCorrection> allPGALinesPerPGA, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, Func<T, IEnumerable<MessageBlock>> getBlocks, Func<IEnumerable<MessageBlock>> getDislaimerBlocks = null, bool isPGACorrection = false)
			where T : IPGADataCorrection
		{
			var shouldBeDeclared = OGAIndicatorList.IsToBeDeclared(indicator);
			var shouldBeDisclaimed = ShouldPGABeDisclaimed(indicator, isPGACorrection, pgaLines.Cast<IPGADataCorrection>(), allPGALinesPerPGA);

			if (shouldBeDisclaimed && getDislaimerBlocks != null)
			{
				foreach (var dislaimBlock in getDislaimerBlocks())
				{
					yield return dislaimBlock;
				}
			}

			foreach (T pga in pgaLines)
			{
				if (pgaIncludedList != null)
				{
					pgaIncludedList.Add(new Tuple<IPGADataCorrection, bool>(pga, shouldBeDeclared));
				}

				if (shouldBeDeclared && pga.IncludedInMessage())
				{
					foreach (var block in getBlocks(pga))
					{
						yield return block;
					}
				}
			}
		}

		ZBool ShouldPGABeDisclaimed(ZString indicator, bool isPGACorrection, IEnumerable<IPGADataCorrection> pgaLines, IEnumerable<IPGADataCorrection> allPGALinesPerPGA)
		{
			var result = OGAIndicatorList.IsToBeDisclaimed(indicator);

			if (isPGACorrection)
			{
				result = pgaLines.AreAllLinesToBeDeleted() || ((OGAIndicatorList.IsToBeDisclaimed(indicator) && !ZZCustomsFunctionality.IsPGADataCorrection2ndPhaseffective && allPGALinesPerPGA.HasLineToBeAmended()));
			}

			return result;
		}

		#region OMC blocks

		internal IEnumerable<MessageBlock> GetOMCBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.OMC))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[] { AEPAPGBlockHelper.MakePG01ForDisclaimer(1, ACEGovernmentAgenciesCodeList.Codes.OMC, "OMC", entryLine.OMCDisclaimReason) };
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.OMCIndicator, entryLine.OMCHeaders, null, pgaIncludedList, GetOMCDataForOneLine, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetOMCDataForOneLine(IOMCHeader omcHeader)
		{
			oMCNumberCounter++;
			omcHeader.LineNo = oMCNumberCounter;
			yield return AEPAPGBlockHelper.MakePG01(omcHeader, oMCNumberCounter);
			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
			yield return AEPAPGBlockHelper.MakePG06(SourceTypeCodesList.Codes.Harvested, omcHeader.SourceCountry, omcHeader.DepartureDate);

			if (omcHeader.AquacultureFacilities != null)
			{
				foreach (var aquacultureFacility in omcHeader.AquacultureFacilities)
				{
					foreach (var block in AEPAPGBlockHelper.MakeEntityBlocks(EntityRoleCodeList.Codes.AquacultureFacility, aquacultureFacility?.CompanyAddress))
					{
						yield return block;
					}
				}
			}

			foreach (var block in AEPAPGBlockHelper.OMCMakeContactBlocks(EntityRoleCodeList.Codes.Exporter, omcHeader.Exporter, omcHeader.ExporterPGAContactInformation))
			{
				yield return block;
			}

			if (omcHeader.Exporter != null && (!omcHeader.ConformanceDeclaration.IsEmpty || omcHeader.ExporterCertificationDate.IsValid))
			{
				yield return AEPAPGBlockHelper.MakePG22(omcHeader.ConformanceDeclaration, EntityRoleCodeList.Codes.Exporter, "", omcHeader.ExporterCertificationDate);
			}

			foreach (var block in AEPAPGBlockHelper.OMCMakeContactBlocks(EntityRoleCodeList.Codes.ResponsibleGovernmentOfficial, omcHeader.ResponsibleGovernmentOfficial, omcHeader.GovOfficialPGAContactInformation))
			{
				yield return block;
			}

			if (omcHeader.OfficialCertificationDate.IsValid && !omcHeader.ConformanceDeclaration.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG22("8", EntityRoleCodeList.Codes.ResponsibleGovernmentOfficial, "DS1", omcHeader.OfficialCertificationDate);
			}

			if (!omcHeader.NetWeight.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG29(omcHeader.NetWeight, omcHeader.NetWeightUQ);
			}
		}

		#endregion

		#region ATF blocks

		IEnumerable<MessageBlock> GenerateATFData(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.ATF))
			{
				foreach (MessageBlock block in GenerateBlocks(entryLine.ATFIndicator, entryLine.ATFLines, null, pgaIncludedList, GetATFData, GetATFDisclaimedData, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetATFDisclaimedData()
		{
			return new[] { AEPAPGBlockHelper.MakePG01ForDataCorrectionDeleteAll("ATF") };
		}

		IEnumerable<MessageBlock> GetATFData(IATFData commonData)
		{
			aTFNumberCounter++;
			commonData.LineNumber = aTFNumberCounter;
			var pg01 = AEPAPGBlockHelper.MakePG01(aTFNumberCounter, "ATF", "ATF", "");
			pg01.ConfidentialInformationIndicator = "Y";
			yield return pg01;

			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
			yield return AEPAPGBlockHelper.MakePG50();

			if (!commonData.CaliberGaugeSize.IsEmpty || !commonData.Model.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG07(commonData);
			}
			yield return AEPAPGBlockHelper.MakePG10(commonData);

			var manufacturerAddress = commonData.ManufacturerAddress;
			if (manufacturerAddress != null)
			{
				var entityName = manufacturerAddress.CompanyName;
				if (!entityName.IsEmpty)
				{
					foreach (var block in AEPAPGBlockHelper.MakePG19WithName("MF", entityName))
					{
						yield return block;
					}
				}

				var countryCode = manufacturerAddress.Country;
				if (!countryCode.IsEmpty)
				{
					yield return new AEPAPG20() { EntityCountry = countryCode };
				}
			}

			yield return AEPAPGBlockHelper.MakePG26(1, commonData.Quantity, "");

			if (!commonData.BarrelLength.IsEmpty || !commonData.OverallLength.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG29(commonData);
			}

			yield return AEPAPGBlockHelper.MakePG51();

			if (!commonData.FFLNumber.IsEmpty || !commonData.FFLExemptionCode.IsEmpty)
			{
				var pg14 = AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.AT2, commonData.FFLNumber);
				pg14.ExemptionCode = commonData.FFLExemptionCode;
				yield return pg14;
			}

			if (!commonData.FELNumber.IsEmpty || !commonData.FELExemptionCode.IsEmpty)
			{
				var pg14 = AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.AT3, commonData.FELNumber);
				pg14.ExemptionCode = commonData.FELExemptionCode;
				yield return pg14;
			}

			if (!commonData.PermitNumber.IsEmpty || !commonData.PermitExemptionCode.IsEmpty)
			{
				var pg14 = AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.AT4, commonData.PermitNumber);
				pg14.ExemptionCode = commonData.PermitExemptionCode;
				yield return pg14;
			}

			if (!commonData.AECANumber.IsEmpty || !commonData.AECAExemptionCode.IsEmpty)
			{
				var pg14 = AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.AT5, commonData.AECANumber);
				pg14.ExemptionCode = commonData.AECAExemptionCode;
				yield return pg14;
			}

			yield return AEPAPGBlockHelper.MakePG30(commonData);
			yield return AEPAPGBlockHelper.MakePG32("198", commonData.ExportCountry);
		}

		#endregion

		#region Lacey Act blocks

		public IEnumerable<MessageBlock> GetLaceyActBlocks(IGovernmentAgencies entryLine, bool cargoRelease)
		{
			if (cargoRelease)
			{
				foreach (MessageBlock block in GetLaceyActCommonData(entryLine))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetLaceyActCommonData(IGovernmentAgencies entryLine)
		{
			int currentLineNumber = 0;
			foreach (ILaceyActCommon pGACommon in entryLine.LaceyActData)
			{
				currentLineNumber++;
				if (!pGACommon.CommercialDescription.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePGAOI(pGACommon.CommercialDescription);
				}

				yield return MakeLaceyActPG01(pGACommon, currentLineNumber);

				foreach (MessageBlock block in GetPG04ConstituentElementData(pGACommon))
				{
					yield return block;
				}

				if (ShouldCreatePG25Block(pGACommon))
				{
					yield return MakePGA25(pGACommon);
				}

				foreach (MessageBlock block in GetContainersBlocks(pGACommon))
				{
					yield return block;
				}
			}
		}

		PGAPG01 MakeLaceyActPG01(ILaceyActCommon commonData, int currentLineNumber)
		{
			var pg01 = new PGAPG01();

			pg01.AgencyQualifier1 = GovernmentAgenciesCodeList.Codes.AP;
			pg01.PGALineItemNumber = currentLineNumber;
			commonData.PGALineItemNumber = currentLineNumber;

			return pg01;
		}

		IEnumerable<MessageBlock> GetPG04ConstituentElementData(ILaceyActCommon commonData)
		{
			foreach (IConstituentElement pG4 in commonData.ConstituentElements)
			{
				yield return MakePG04ConstituentElementData(pG4);

				foreach (MessageBlock block in GetPG05PG15Data(pG4.ScientificData, commonData.ShouldSendPG15PG16Records))
				{
					yield return block;
				}
			}
		}

		PGAPG04 MakePG04ConstituentElementData(IConstituentElement pG4)
		{
			PGAPG04 pg04 = new PGAPG04();

			pg04.NameOfTheConstituentElement = pG4.Name.Left(51);
			if (!pG4.Percent.IsEmpty)
			{
				pg04.PercentOfConstituentElement = pG4.Percent;
			}
			pg04.QuantityOfConstituentElement = pG4.Quantity;
			pg04.UnitOfMeasure = pG4.UnitOfMeasure;

			return pg04;
		}

		IEnumerable<MessageBlock> GetPG05PG15Data(PGAScientificDataCollection pG05PG15Data, bool shouldSendPG15PG16Records)
		{
			ZString genusName = ZString.Empty;
			ZString speciesName = ZString.Empty;
			ZString countryCode = ZString.Empty;

			foreach (IScientificData scientificData in pG05PG15Data.OfType<ScientificData>().OrderByDescending(x => x.US_PGACountryCode))
			{
				if (!countryCode.IsEmpty && countryCode != scientificData.CountryCode)
				{
					yield return MakePG06Data(countryCode, shouldSendPG15PG16Records);
				}

				countryCode = scientificData.CountryCode;

				bool genusNameIsDifferentFromPreviousLine = genusName.IsEmpty || genusName != scientificData.GenusName;
				bool speciesNameIsDifferentFromPreviousLine = speciesName.IsEmpty || speciesName != scientificData.SpeciesName;

				bool shouldPG05PG15BeSent = ((!scientificData.GenusName.IsEmpty || !scientificData.SpeciesName.IsEmpty) &&
					(genusNameIsDifferentFromPreviousLine || speciesNameIsDifferentFromPreviousLine));

				if (shouldPG05PG15BeSent)
				{
					yield return MakePG05PG15Data(scientificData, shouldSendPG15PG16Records);
				}

				genusName = scientificData.GenusName;
				speciesName = scientificData.SpeciesName;
			}
			if (pG05PG15Data.Count > 0 && !countryCode.IsEmpty)
			{
				yield return MakePG06Data(countryCode, shouldSendPG15PG16Records);
			}
		}

		MessageBlock MakePG05PG15Data(IScientificData scientificData, bool shouldSendPG15PG16Records)
		{
			if (shouldSendPG15PG16Records)
			{
				PGAPG15 block = new PGAPG15();

				block.ScientificGenusName = scientificData.GenusName;
				block.ScientificSpeciesName = scientificData.SpeciesName;

				return block;
			}
			else
			{
				PGAPG05 block = new PGAPG05();

				block.ScientificGenusName = scientificData.GenusName;
				block.ScientificSpeciesName = scientificData.SpeciesName;

				return block;
			}
		}

		MessageBlock MakePG06Data(ZString countryCode, bool shouldSendPG15PG16Records)
		{
			if (shouldSendPG15PG16Records)
			{
				PGAPG16 block = new PGAPG16();

				block.CountryCode = countryCode;
				block.SourceTypeCode = LaceyActSourceTypeCode;

				return block;
			}
			else
			{
				PGAPG06 block = new PGAPG06();

				block.CountryCode = countryCode;
				block.SourceTypeCode = LaceyActSourceTypeCode;

				return block;
			}
		}

		PGAPG25 MakePGA25(ILaceyActCommon pGACommon)
		{
			PGAPG25 pg25 = new PGAPG25();
			pg25.PGALineValue = pGACommon.PGALineValue;
			return pg25;
		}

		bool ShouldCreatePG25Block(ILaceyActCommon pGAData)
		{
			return !pGAData.PGALineValue.IsEmpty;
		}

		IEnumerable<MessageBlock> GetContainersBlocks(ILaceyActCommon commonData, bool isACE = false)
		{
			ZString number1 = ZString.Empty;
			ZString number2 = ZString.Empty;
			ZString number3 = ZString.Empty;

			foreach (IContainerNumber number in commonData.ContainerNumbers)
			{
				if (number1.IsEmpty)
				{
					number1 = number.ContainerEquipmentID;
				}
				else if (number2.IsEmpty)
				{
					number2 = number.ContainerEquipmentID;
				}
				else if (number3.IsEmpty)
				{
					number3 = number.ContainerEquipmentID;
				}
				else
				{
					yield return MakePGA27(number1, number2, number3, isACE);

					number1 = number.ContainerEquipmentID;
					number2 = ZString.Empty;
					number3 = ZString.Empty;
				}
			}
			if (!number1.IsEmpty || !number2.IsEmpty || !number3.IsEmpty)
			{
				yield return MakePGA27(number1, number2, number3, isACE);
			}
		}

		MessageBlock MakePGA27(ZString number1, ZString number2, ZString number3, bool isACE)
		{
			ILaceyActContainer containerBlock = isACE ? new AEPAPG27() : new PGAPG27();
			containerBlock.ContainerEquipmentID = number1;
			containerBlock.ContainerEquipmentID1 = number2;
			containerBlock.ContainerEquipmentID2 = number3;
			return (MessageBlock)containerBlock;
		}

		#endregion

		#region EPA ODS/TSCA blocks

		IEnumerable<MessageBlock> GenerateEPA_ODSOrTSCAData(IGovernmentAgencies governmentAgenciesData, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			var shouldSendODS = ShouldSendPGA(governmentAgenciesData, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.ODS);
			var shouldSendTSCA = ShouldSendPGA(governmentAgenciesData, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.TSCA);
			var odsIndicator = shouldSendODS ? governmentAgenciesData.ODSIndicator : ZString.Empty;
			var tscaIndicator = shouldSendTSCA ? governmentAgenciesData.TSCAIndicator : ZString.Empty;
			if ((shouldSendODS && OGAIndicatorList.IsToBeDeclaredOrDisclaimed(odsIndicator)) || (shouldSendTSCA && OGAIndicatorList.IsToBeDeclaredOrDisclaimed(tscaIndicator)))
			{
				IPGADataCorrection oDSDataCorrection = null;
				IPGADataCorrection tSCADataCorrection = null;
				var tscaData = governmentAgenciesData.EPA_TSCAData;
				if (shouldSendODS)
				{
					oDSDataCorrection = governmentAgenciesData.ODSDataCorrection;
					foreach (var block in GenerateAEPAPG01ForODSOrTSCA(odsIndicator, "ODS", governmentAgenciesData.ODSDisclaimReason, oDSDataCorrection, GetAllEPALines(governmentAgenciesData), pgaIncludedList, isPGACorrection: isPGACorrection))
					{
						if (tscaData != null)
						{
							tscaData.ODSLineNumber = ePALineNumberCounter;
						}
						yield return block;
					}
				}

				var isToBeDeclaredODS = shouldSendODS && OGAIndicatorList.IsToBeDeclared(odsIndicator);
				if (shouldSendTSCA)
				{
					tSCADataCorrection = governmentAgenciesData.TSCADataCorrection;
					foreach (var block in GenerateAEPAPG01ForODSOrTSCA(tscaIndicator, "TS1", governmentAgenciesData.TSCADisclaimReason, tSCADataCorrection, GetAllEPALines(governmentAgenciesData), pgaIncludedList, isPGACorrection: isPGACorrection))
					{
						if (tscaData != null)
						{
							tscaData.TSCALineNumber = ePALineNumberCounter;
						}
						yield return block;
					}
				}

				if ((isToBeDeclaredODS && oDSDataCorrection != null && oDSDataCorrection.IncludedInMessage()) || (shouldSendTSCA && OGAIndicatorList.IsToBeDeclared(tscaIndicator) && tSCADataCorrection != null && tSCADataCorrection.IncludedInMessage()))
				{
					yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);

					if (tscaData != null)
					{
						if (tscaData.CertifySignatureDate.IsEmpty)
						{
							tscaData.CertifySignatureDate = ZDate.Today;
						}

						yield return AEPAPGBlockHelper.MakePG22EPATSCA("CI", tscaData.TSCACertificationCode, tscaData.DeclarationCertificate, tscaData.CertifySignatureDate);

						foreach (var block in AEPAPGBlockHelper.MakePG21("CI", tscaData.ContactName, tscaData.ContactPhone, tscaData.ContactEmail, ZString.Empty))
						{
							yield return block;
						}
					}
				}
			}
		}

		IEnumerable<MessageBlock> GenerateAEPAPG01ForODSOrTSCA(ZString indicator, ZString programCode, ZString disclaimReason, IPGADataCorrection dataCorrection, IEnumerable<IPGADataCorrection> allPGALinesPerPGA, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, bool isPGACorrection = false)
		{
			var shouldBeDeclared = OGAIndicatorList.IsToBeDeclared(indicator);
			var shouldBeDisclaimed = ShouldPGABeDisclaimed(indicator, isPGACorrection, dataCorrection != null ? new IPGADataCorrection[] { dataCorrection } : Array.Empty<IPGADataCorrection>(), allPGALinesPerPGA);

			if (dataCorrection != null && pgaIncludedList != null)
			{
				pgaIncludedList.Add(new Tuple<IPGADataCorrection, bool>(dataCorrection, shouldBeDeclared));
			}

			if (dataCorrection != null && shouldBeDeclared)
			{
				if (dataCorrection.IncludedInMessage())
				{
					yield return AEPAPGBlockHelper.MakePG01(++ePALineNumberCounter, ACEGovernmentAgenciesCodeList.Codes.EPA, programCode);
				}
			}

			if (shouldBeDisclaimed)
			{
				yield return AEPAPGBlockHelper.MakePG01ForDisclaimer(++ePALineNumberCounter, ACEGovernmentAgenciesCodeList.Codes.EPA, programCode, disclaimReason);
			}
		}

		#endregion

		#region EPA Vehicles and Engines blocks

		IEnumerable<MessageBlock> GetVNEBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.VNE))
			{
				Func<IEnumerable<MessageBlock>> getVNEDisclaimedBlocks = () =>
				{
					return new[]
					{
						new AEPAPG01()
						{
							PGALineNumber = ++ePALineNumberCounter,
							GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.EPA,
							GovernmentAgencyProgramCode = GovernmentAgencyProgramCodeList.Codes.VNE,
							Disclaimer = entryLine.VNEDisclaimReason
						}
					};
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.VNEIndicator, entryLine.EPA_VNELines, GetAllEPALines(entryLine), pgaIncludedList, GetVNEData, getVNEDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetVNEData(IVNEData commonData)
		{
			commonData.LineNo = ++ePALineNumberCounter;
			yield return AEPAPGBlockHelper.MakePG01(commonData, ePALineNumberCounter);
			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);

			foreach (MessageBlock block in AEPAPGBlockHelper.MakePG24(commonData))
			{
				yield return block;
			}

			foreach (MessageBlock block in GetVNE_PG07Blocks(commonData))
			{
				yield return block;
			}

			foreach (MessageBlock block in GetVNE19_22PartyDetailsBlocks(commonData))
			{
				yield return block;
			}
		}

		#region Vehicles and Engines

		IEnumerable<MessageBlock> GetVNE_PG07Blocks(IVNEData commonData)
		{
			var list = new List<IVNEDetails>();
			list.AddRange(commonData.VNEDetails);

			if (list.Count > 1)
			{
				foreach (MessageBlock block in GetVNE_OtherPG07_BlocksSet(list))
				{
					yield return block;
				}
			}

			if (list.Count > 0)
			{
				foreach (MessageBlock block in GetFirstVehiclePG07_Blocks(list[0], commonData))
				{
					yield return block;
				}

				foreach (MessageBlock block in GetFirstEnginePG07_Blocks(list[0], commonData))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetFirstVehiclePG07_Blocks(IVNEDetails engineDetails, IVNEData commonData)
		{
			if (!engineDetails.BuildMonth.IsEmpty || !engineDetails.BuildYear.IsEmpty || !engineDetails.IdentityNumber.IsEmpty || !engineDetails.VehicleManufacturer.IsEmpty)
			{
				yield return MakeVNE_PG07_Vehicle(engineDetails);
				foreach (MessageBlock block in MaleVNE_PG08(engineDetails.VehicleAdditionalNumbers))
				{
					yield return block;
				}

				if (engineDetails.IsVehicleOnly() || engineDetails.IsVehicleAndEngine())
				{
					if (!commonData.MaxEnginePower.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG10(CommodityVehicleQualifierCodesList.Codes.V03, commonData.MaxEnginePower, commonData.MaxEnginePowerUQ);
					}

					if (!commonData.BodyType.IsEmpty || !commonData.BodyDescription.IsEmpty || !commonData.BodyCode.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG10(commonData.BodyType, commonData.BodyDescription, commonData.BodyCode);
					}

					if (!commonData.ModelYear.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG10(CommodityVehicleQualifierCodesList.Codes.V06, commonData.ModelYear, ZString.Empty);
					}

					if (!commonData.DriverSide.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG10(CommodityVehicleQualifierCodesList.Codes.V01, commonData.DriverSide, ZString.Empty);
					}

					if (!commonData.MilitaryEq.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG10(CommodityVehicleQualifierCodesList.Codes.V04, commonData.MilitaryEq, ZString.Empty);
					}

					if ((!commonData.CertOfConformity.IsEmpty || !commonData.CertOfConformityExpiryDate.IsEmpty))
					{
						yield return MakeVNE_PG14_Certificate(commonData.CertOfConformity, commonData.CertOfConformityExpiryDate);
					}
					if (!commonData.BondPolicyNo.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.EP7, commonData.BondPolicyNo);
					}
					if (!commonData.CBPBondNumber.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.EP4, commonData.CBPBondNumber);
					}
					if (!commonData.VehiclesExemptionNumber.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.EP9, commonData.VehiclesExemptionNumber);
					}
					if (!commonData.EPARegistrationNumber.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.EP4, commonData.EPARegistrationNumber);
					}
				}

				var vehicleManufacturer = engineDetails.VehicleManufacturer;
				if (!vehicleManufacturer.IsEmpty)
				{
					foreach (var block in AEPAPGBlockHelper.MakePG19WithName(EntityRoleCodeList.Codes.ManufacturerOfGoods, vehicleManufacturer))
					{
						yield return block;
					}
				}
			}
		}

		IEnumerable<MessageBlock> GetFirstEnginePG07_Blocks(IVNEDetails engineDetails, IVNEData commonData)
		{
			var engineDetailsAreNotEmpty = !engineDetails.EngineNumber.IsEmpty || !engineDetails.EngineModel.IsEmpty || !engineDetails.EngineBuildDate.IsEmpty || !engineDetails.EngineManufacturer.IsEmpty;
			if (engineDetailsAreNotEmpty)
			{
				var itemType = !engineDetails.IdentityNumber.IsEmpty && !engineDetails.EngineNumber.IsEmpty ? PG02ItemTypeList.Codes.Component : PG02ItemTypeList.Codes.Product;
				if (itemType == PG02ItemTypeList.Codes.Component)
				{
					yield return AEPAPGBlockHelper.MakePG02(itemType);
				}
				yield return MakeVNE_PG07_Engine(engineDetails);
				foreach (MessageBlock block in MaleVNE_PG08(engineDetails.EngineAdditionalNumbers))
				{
					yield return block;
				}

				var certOfConformityExists = !commonData.CertOfConformity.IsEmpty || !commonData.CertOfConformityExpiryDate.IsEmpty;
				if (engineDetails.IsEngineOnly())
				{
					if ((!commonData.BodyType.IsEmpty || !commonData.BodyDescription.IsEmpty || !commonData.BodyCode.IsEmpty))
					{
						yield return AEPAPGBlockHelper.MakePG10(commonData.BodyType, commonData.BodyDescription, commonData.BodyCode);
					}

					if (!commonData.MaxEnginePower.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG10(CommodityVehicleQualifierCodesList.Codes.V03, commonData.MaxEnginePower, commonData.MaxEnginePowerUQ);
					}

					if (!engineDetails.ManufactureDateType.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG10(CommodityVehicleQualifierCodesList.Codes.V05, engineDetails.BuildDateExplanation, engineDetails.ManufactureDateType);
					}

					if (certOfConformityExists)
					{
						yield return MakeVNE_PG14_Certificate(commonData.CertOfConformity, commonData.CertOfConformityExpiryDate);
					}
					if (!commonData.BondPolicyNo.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.EP7, commonData.BondPolicyNo);
					}
					if (!commonData.CBPBondNumber.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.EP4, commonData.CBPBondNumber);
					}
					if (!commonData.VehiclesExemptionNumber.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.EP9, commonData.VehiclesExemptionNumber);
					}
				}
				else if (engineDetails.IsVehicleAndEngine())
				{
					if (certOfConformityExists)
					{
						yield return MakeVNE_PG14_Certificate(commonData.CertOfConformity, commonData.CertOfConformityExpiryDate);
					}

					if (!engineDetails.ManufactureDateType.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG10(CommodityVehicleQualifierCodesList.Codes.V05, engineDetails.BuildDateExplanation, engineDetails.ManufactureDateType);
					}
				}

				var engineManufacturer = engineDetails.EngineManufacturer;
				if (!engineManufacturer.IsEmpty)
				{
					foreach (var block in AEPAPGBlockHelper.MakePG19WithName(EntityRoleCodeList.Codes.ManufacturerOfGoods, engineManufacturer))
					{
						yield return block;
					}
				}
			}
		}

		IEnumerable<MessageBlock> GetVNE_OtherPG07_BlocksSet(List<IVNEDetails> details)
		{
			var rowsCount = details.Count;

			for (int i = 1; i < rowsCount; i++)
			{
				var engineDetails = details[i];
				var groupRequired = engineDetails.IsVehicleAndEngine();

				if (groupRequired)
				{
					yield return new AEPAPG50();
				}

				if (engineDetails.IsVehicleAndEngine() || engineDetails.IsVehicleOnly())
				{
					yield return MakeVNE_PG07_Vehicle(engineDetails);
					foreach (MessageBlock block in MaleVNE_PG08(engineDetails.VehicleAdditionalNumbers))
					{
						yield return block;
					}
				}

				if (engineDetails.IsVehicleAndEngine() || engineDetails.IsEngineOnly())
				{
					yield return MakeVNE_PG07_Engine(engineDetails);
					foreach (MessageBlock block in MaleVNE_PG08(engineDetails.EngineAdditionalNumbers))
					{
						yield return block;
					}
				}

				if (!engineDetails.ManufactureDateType.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG10(CommodityVehicleQualifierCodesList.Codes.V05, engineDetails.BuildDateExplanation, engineDetails.ManufactureDateType);
				}

				if (groupRequired)
				{
					yield return new AEPAPG51();
				}
			}
		}

		IEnumerable<MessageBlock> MaleVNE_PG08(IEnumerable<VNEAdditionalNumbers> additionalNumbers)
		{
			AEPAPG08 pg08 = null;
			var count = 0;
			foreach (var element in additionalNumbers)
			{
				switch (count++)
				{
					case 0:
						pg08 = new AEPAPG08() { ItemIdentityNumber = element.Number };
						break;
					case 1:
						pg08.ItemIdentityNumber1 = element.Number;
						break;
					case 2:
						pg08.ItemIdentityNumber2 = element.Number;
						break;
					default:
						pg08.ItemIdentityNumber3 = element.Number;
						yield return pg08;
						pg08 = null;
						count = 0;
						break;
				}
			}
			if (pg08 != null)
			{
				yield return pg08;
			}
		}

		AEPAPG07 MakeVNE_PG07_Engine(IVNEDetails details)
		{
			var pg07 = new AEPAPG07();
			pg07.Model = details.EngineModel;
			pg07.ManufactureMonthAndYear = details.EngineBuildDate;
			pg07.ItemIdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			pg07.ItemIdentityNumber = details.EngineNumber;
			return pg07;
		}

		AEPAPG07 MakeVNE_PG07_Vehicle(IVNEDetails details)
		{
			var pg07 = new AEPAPG07();
			pg07.Model = details.Model;
			pg07.ManufactureMonthAndYear = details.BuildMonth + details.BuildYear;
			pg07.ItemIdentityNumberQualifier = details.IdentityNumberQualifier;
			pg07.ItemIdentityNumber = details.IdentityNumber;
			return pg07;
		}

		#endregion

		#region LPCO blocks PG14

		AEPAPG14 MakeVNE_PG14_Certificate(ZString certificateNo, ZDate expiryDate)
		{
			AEPAPG14 result;
			result = AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.EP4, certificateNo);
			if (!expiryDate.IsEmpty)
			{
				result.LPCODateQualifier = "1";
				result.LPCODate = expiryDate;
			}
			return result;
		}

		#endregion

		#region Entities

		IEnumerable<MessageBlock> GetVNE19_22PartyDetailsBlocks(IVNEData commonData)
		{
			var rolesForPG55 = new List<ZString> { EntityRoleCodeList.Codes.CertifyingIndividual };
			if (!commonData.NAICNo.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG19WithNumber(EntityRoleCodeList.Codes.NAICBondIssuer, commonData.NAICNo);
				yield return MakePG20_BondIssuer(commonData.StateOfIssue);
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Owner, commonData.OwnerContactDetails, commonData.CertifyingIndividual == PartyTypeList.Codes.Owner ? commonData.ContactDetails : null))
			{
				yield return block;
			}
			if (commonData.CertifyingIndividual == PartyTypeList.Codes.Owner && commonData.OwnerContactDetails != null)
			{
				yield return AEPAPGBlockHelper.MakePG55(rolesForPG55);
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Importer, commonData.ImporterContactDetails, commonData.CertifyingIndividual == PartyTypeList.Codes.Importer ? commonData.ContactDetails : null))
			{
				yield return block;
			}
			if (commonData.CertifyingIndividual == PartyTypeList.Codes.Importer && commonData.ImporterContactDetails != null)
			{
				yield return AEPAPGBlockHelper.MakePG55(rolesForPG55);
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.StorageLocation, commonData.StorageLocationContactDetails))
			{
				yield return block;
			}
			if (commonData.CertifySignatureDate.IsEmpty)
			{
				commonData.CertifySignatureDate = ZDate.Today;
			}

			yield return AEPAPGBlockHelper.MakePG22(commonData);

			if (commonData.CertifyingIndividual == PartyTypeList.Codes.CustomsBroker)
			{
				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.CertifyingIndividual, commonData.ContactDetails))
				{
					yield return block;
				}
			}
		}

		AEPAPG20 MakePG20_BondIssuer(ZString stateOfIssue)
		{
			var pg20 = new AEPAPG20();
			pg20.EntityStateProvince = stateOfIssue;
			return pg20;
		}

		#endregion

		#endregion

		#region FSIS

		IEnumerable<MessageBlock> GetFSISBlocks(IGovernmentAgenciesCommon entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.FSIS))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[]
					{
						new AEPAPG01()
						{
							PGALineNumber = 1,
							GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.FSI,
							GovernmentAgencyProgramCode = ACEGovernmentAgenciesCodeList.Codes.FSI,
							Disclaimer = entryLine.FSISDisclaimReason
						}
					};
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.FSISIndicator, entryLine.FSISLines, null, pgaIncludedList, GetFSISData, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}
		IEnumerable<MessageBlock> GetFSISData(IFSISLine cer)
		{
			var rolesForPG55 = new List<ZString> { EntityRoleCodeList.Codes.CertifyingIndividual };
			fSISNumberCounter++;
			cer.LineNumber = fSISNumberCounter;
			yield return AEPAPGBlockHelper.MakePG01(cer, fSISNumberCounter);
			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
			yield return AEPAPGBlockHelper.MakePG06(SourceTypeCodesList.Codes.CountryOfProduction, cer.CountryOfOrigin);
			yield return AEPAPGBlockHelper.MakePG13(cer.CertificateIssuerCountry);
			yield return AEPAPGBlockHelper.MakePG14("FS7", cer.HealthCertifcateNumber);

			if (!cer.IsElectronicallyCertificated)
			{
				foreach (var lot in cer.Lots)
				{
					yield return new AEPAPG50();

					yield return AEPAPGBlockHelper.MakePG10(lot);
					yield return AEPAPGBlockHelper.MakePG19WithNumber(EntityRoleCodeList.Codes.ExportingEstablishment, cer.ExportingEstNo);
					if (!lot.ProducingEstNo.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG19WithNumber(EntityRoleCodeList.Codes.ProducingEstablishment, lot.ProducingEstNo);
					}

					if (!lot.SourceEstNo.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG19WithNumber(EntityRoleCodeList.Codes.SourceEstablishment, lot.SourceEstNo);
					}

					if (!lot.SourceCountry.IsEmpty)
					{
						yield return new AEPAPG06() { SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSource, CountryCode = lot.SourceCountry };
					}

					yield return AEPAPGBlockHelper.MakePG25(lot);
					foreach (var pg26 in AEPAPGBlockHelper.MakePG26(lot))
					{
						yield return pg26;
					}
					yield return AEPAPGBlockHelper.MakePG29(lot.NetWeightInLB, Core.Constants.Weight.Pounds);

					yield return new AEPAPG51();
				}
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Importer, cer.Importer, cer.CertifyingIndividual == EntityRoleCodeList.Codes.Importer ? cer.ContactDetails : null))
			{
				yield return block;
			}

			if (cer.CertifyingIndividual == PartyTypeList.Codes.Importer && cer.Importer != null)
			{
				yield return AEPAPGBlockHelper.MakePG55(rolesForPG55);
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Consignee, cer.Consignee))
			{
				yield return block;
			}
			if (cer.CertifyingIndividual == PartyTypeList.Codes.CustomsBroker)
			{
				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.CustomsBroker, cer.ContactDetails))
				{
					yield return block;
				}
				yield return AEPAPGBlockHelper.MakePG55(rolesForPG55);
			}
			else
			{
				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.CustomsBroker, cer.Broker))
				{
					yield return block;
				}
			}
			if (cer.CertifySignatureDate.IsEmpty)
			{
				cer.CertifySignatureDate = ZDate.Today;
			}

			yield return AEPAPGBlockHelper.MakePG22(cer);

			foreach (var pg24 in AEPAPGBlockHelper.MakePG24(cer))
			{
				yield return pg24;
			}

			yield return AEPAPGBlockHelper.MakePG30(cer);
		}

		#endregion

		#region EPA PST blocks

		IEnumerable<MessageBlock> GetPSTBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.PST))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[]
					{
						new AEPAPG01()
						{
							PGALineNumber = ++ePALineNumberCounter,
							GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.EPA,
							GovernmentAgencyProgramCode = entryLine.PSTDisclaimProgram,
							Disclaimer = entryLine.PSTDisclaimReason
						}
					};
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.PSTIndicator, entryLine.EPA_PSTLines, GetAllEPALines(entryLine), pgaIncludedList, GetPSTData, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetPSTData(IPSTData commonData)
		{
			commonData.LineNo = ++ePALineNumberCounter;
			yield return AEPAPGBlockHelper.MakePG01(commonData, ePALineNumberCounter);

			foreach (var block in GetPSTPG02_04DetailsBlocks(commonData))
			{
				yield return block;
			}

			foreach (var block in AEPAPGBlockHelper.MakePG24(commonData))
			{
				yield return block;
			}
			yield return AEPAPGBlockHelper.MakePG07(commonData.BrandName);
			if (!commonData.LPCONumber.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG14(LPCOTypeList.Codes.EP8, commonData.LPCONumber);
			}

			foreach (var block in AEPAPGBlockHelper.MakePG19(commonData))
			{
				yield return block;
			}

			foreach (MessageBlock block in GetPST19_21PartyDetailsBlocks(commonData))
			{
				yield return block;
			}
			if (commonData.CertifySignatureDate.IsEmpty)
			{
				commonData.CertifySignatureDate = ZDate.Today;
			}

			yield return AEPAPGBlockHelper.MakePG22WithDocId("944", "EP3", commonData.DeclarationCertificate, commonData.CertifySignatureDate);

			var packagingLevel = 1;
			yield return AEPAPGBlockHelper.MakePG26(packagingLevel++, commonData.Quantity1, commonData.UQ1);
			yield return AEPAPGBlockHelper.MakePG26(packagingLevel++, commonData.Quantity2, commonData.UQ2);

			if (commonData.Quantity3 > 0)
			{
				yield return AEPAPGBlockHelper.MakePG26(packagingLevel++, commonData.Quantity3, commonData.UQ3);
			}
			if (commonData.Quantity4 > 0)
			{
				yield return AEPAPGBlockHelper.MakePG26(packagingLevel++, commonData.Quantity4, commonData.UQ4);
			}
			if (commonData.Quantity5 > 0)
			{
				yield return AEPAPGBlockHelper.MakePG26(packagingLevel++, commonData.Quantity5, commonData.UQ5);
			}
			if (commonData.Quantity6 > 0)
			{
				yield return AEPAPGBlockHelper.MakePG26(packagingLevel++, commonData.Quantity6, commonData.UQ6);
			}

			yield return AEPAPGBlockHelper.MakePG29(commonData.NetWeight, commonData.WeightUQ);

			foreach (var block in GetPSTPG02_04SupplementalDetailsBlocks(commonData))
			{
				yield return block;
			}
		}

		IEnumerable<MessageBlock> GetPSTPG02_04DetailsBlocks(IPSTData commonData)
		{
			var pg02 = AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
			if (!DoesRequireComponentBlocks(commonData))
			{
				var detail = commonData.Lines.FirstOrDefault();
				if (detail != null)
				{
					pg02.ProductCodeQualifier = detail.LPCOType;
					pg02.ProductCodeNumber = detail.LPCONumber;

					yield return pg02;
					if (!detail.NameOfActiveIngredient.IsEmpty || !detail.ActiveIngredientPercentage.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG04(detail);
					}
				}
				else
				{
					yield return pg02;
				}
			}
			else
			{
				yield return pg02;
			}
		}

		IEnumerable<MessageBlock> GetPSTPG02_04SupplementalDetailsBlocks(IPSTData commonData)
		{
			if (DoesRequireComponentBlocks(commonData))
			{
				foreach (var line in commonData.Lines)
				{
					yield return AEPAPGBlockHelper.MakePG02(line);
					if (!line.NameOfActiveIngredient.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG04(line);
					}
				}
			}
		}

		IEnumerable<MessageBlock> GetPST19_21PartyDetailsBlocks(IPSTData commonData)
		{
			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.CustomsBroker, commonData.BrokerDetails))
			{
				yield return block;
			}
			var rolesForPG55 = ConstructRolesCollectionForPG55_PST(EntityRoleCodeList.Codes.CustomsBroker, commonData);
			if (rolesForPG55.Any() && commonData.BrokerDetails != null)
			{
				yield return AEPAPGBlockHelper.MakePG55(rolesForPG55);
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Importer, commonData.ImporterDetails))
			{
				yield return block;
			}
			rolesForPG55 = ConstructRolesCollectionForPG55_PST(EntityRoleCodeList.Codes.Importer, commonData);
			if (rolesForPG55.Any() && commonData.ImporterDetails != null)
			{
				yield return AEPAPGBlockHelper.MakePG55(rolesForPG55);
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Carrier, commonData.CarrierDetails))
			{
				yield return block;
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Shipper, commonData.ShipperDetails))
			{
				yield return block;
			}
			rolesForPG55 = ConstructRolesCollectionForPG55_PST(EntityRoleCodeList.Codes.Shipper, commonData);
			if (rolesForPG55.Any() && commonData.ShipperDetails != null)
			{
				yield return AEPAPGBlockHelper.MakePG55(rolesForPG55);
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.LocationOfGoodsImmediatelyAfterEntryRelease, commonData.ExaminationLocationDetails))
			{
				yield return block;
			}
		}

		IEnumerable<ZString> ConstructRolesCollectionForPG55_PST(ZString blockRoleType, IPSTData commonData)
		{
			if (commonData.CertifyingIndividual == blockRoleType)
			{
				yield return (ZString)EntityRoleCodeList.Codes.CertifyingIndividual;
			}

			if (commonData.NotifyParty == blockRoleType)
			{
				yield return (ZString)EntityRoleCodeList.Codes.NotifyParty;
			}
		}

		bool DoesRequireComponentBlocks(IPSTData commonData)
		{
			return (commonData.ProductType == PSTProductTypeList.Codes.PS1 || commonData.ProductType == PSTProductTypeList.Codes.PS3) && commonData.Lines.Count() > 1;
		}

		#endregion

		#region APHIS blocks

		IEnumerable<MessageBlock> GetAPHISBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.APHIS))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[]
					{
						new AEPAPG01()
						{
							PGALineNumber = ++aPHLineNumberCounter,
							GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.APH,
							GovernmentAgencyProgramCode = APHISProgramCodeList.Codes.AVS, // TODO APHIS: Determine what to send for program code when we disclaim.
							Disclaimer = entryLine.APHISDisclaimReason
						}
					};
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.APHISIndicator, entryLine.APHISHeaders, GetAllAPHISLines(entryLine), pgaIncludedList, GetAPHISData, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetAPHISData(IAPHISHeader aphisHeader)
		{
			aphisHeader.LineNo = ++aPHLineNumberCounter;
			var pg01 = AEPAPGBlockHelper.MakePG01(aphisHeader.LineNo, ACEGovernmentAgenciesCodeList.Codes.APH, aphisHeader.ProgramType, aphisHeader.ProcessingCode, aphisHeader.IsDocSubmitted, true);
			pg01.IntendedUseCode = aphisHeader.IntendedUseCode;
			pg01.IntendedUseDescription = aphisHeader.IntendedUseDescription;
			yield return pg01;

			var pg02 = AEPAPGBlockHelper.MakePG02(aphisHeader.ProductCodeQualifier, aphisHeader.ProductCodeNumber);
			var skuNumber = aphisHeader.StockKeepingUnitNumber;
			if (!skuNumber.IsEmpty)
			{
				pg02.ProductCodeQualifier1 = ProductCodeQualifiersList.Codes.StockKeepingUnit;
				pg02.ProductCodeNumber1 = skuNumber;
			}
			yield return pg02;

			AEPAPG17 pg17 = null;
			var commoditySpecificName = aphisHeader.CommoditySpecificName;
			if (!commoditySpecificName.IsEmpty)
			{
				pg17 = new AEPAPG17()
				{
					CommonNameSpecific = commoditySpecificName
				};
			}
			var productCharacteristics = aphisHeader.ProductCharacteristics.ToArray();
			var productComponents = aphisHeader.ProductComponents.ToArray();
			var hasComponents = productComponents.Length > 0;
			IAPHISProductCharacteristic productCharacteristic = null;
			if (productCharacteristics.Length > 1)
			{
				foreach (var product in productCharacteristics)
				{
					foreach (var block in GetAPHISProductCharacteristic(product, aphisHeader.CategoryTypeCode, aphisHeader.CategoryCode).ToArray())
					{
						yield return block;
					}
				}
			}
			else if (productCharacteristics.Length == 1)
			{
				productCharacteristic = productCharacteristics[0];
			}

			var scientificGenusName = aphisHeader.ScientificGenusName;
			var scientificSpeciesName = aphisHeader.ScientificSpeciesName;
			var scientificSubSpeciesName = aphisHeader.ScientificSubSpeciesName;
			if (!scientificGenusName.IsEmpty || !scientificSpeciesName.IsEmpty || !scientificSubSpeciesName.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG05(scientificGenusName, scientificSpeciesName, scientificSubSpeciesName);
			}

			foreach (var sourceDetail in aphisHeader.Sources)
			{
				yield return AEPAPGBlockHelper.MakePG06(sourceDetail.SourceTypeCode, sourceDetail.CountryCode, sourceDetail.GeographicLocation, sourceDetail.ProcessingStartDate, sourceDetail.ProcessingEndDate, sourceDetail.ProcessingTypeCode, sourceDetail.ProcessingDescription);
			}

			foreach (var block in GetAPHISProductCharacteristic(productCharacteristic, aphisHeader.CategoryTypeCode, aphisHeader.CategoryCode).ToArray())
			{
				yield return block;
			}

			if (hasComponents && aphisHeader.ProgramType != APHISProgramCodeList.Codes.APQ)
			{
				if (pg17 != null)
				{
					yield return pg17;
					pg17 = null;
				}

				foreach (var productComponent in productComponents)
				{
					yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Component);
					var origin = productComponent.Origin;
					if (!origin.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG06(SourceTypeCodesList.Codes.CountryOfSpeciesOrigin, origin);
					}
					var component = productComponent.Component;
					if (component != null)
					{
						var pg10Block = AEPAPGBlockHelper.MakePG10(component);
						pg10Block.CategoryCode = aphisHeader.CategoryCode;
						pg10Block.CategoryTypeCode = aphisHeader.CategoryTypeCode;
						yield return pg10Block;
					}

					var generalName = productComponent.GeneralName;
					var specificName = productComponent.SpecificName;
					if (!generalName.IsEmpty || !specificName.IsEmpty)
					{
						yield return new AEPAPG17()
						{
							CommonNameGeneral = generalName,
							CommonNameSpecific = specificName
						};
					}
				}
			}

			foreach (var block in GetAPHISLicenceDetailsBlocks(aphisHeader.Licenses))
			{
				yield return block;
			}

			if (pg17 != null)
			{
				yield return pg17;
			}

			foreach (var block in GetAPHIS19_21PartyDetailsBlocks(aphisHeader))
			{
				yield return block;
			}

			if (!aphisHeader.ReMarks.IsEmpty)
			{
				yield return new AEPAPG24()
				{
					RemarksText = aphisHeader.ReMarks
				};
			}

			foreach (var block in GetAPHISPackagingDetailsBlocks(aphisHeader.OrderedQtyUQs.Take(6).ToArray()))
			{
				yield return block;
			}

			foreach (var block in GetContainerDetailsBlocks(aphisHeader.Containers))
			{
				yield return block;
			}

			foreach (var block in GetAPHISInspectionDetailsBlocks(aphisHeader.Inspections, aphisHeader.ArrivalDate.Date, aphisHeader.ArrivalLocation))
			{
				yield return block;
			}

			foreach (var block in GetAPHISRoutingDetailsBlocks(aphisHeader.Routings))
			{
				yield return block;
			}

			if (hasComponents && aphisHeader.ProgramType == APHISProgramCodeList.Codes.APQ)
			{
				foreach (var block in GetAPHISProductsBlocks(productComponents))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetAPHISProductsBlocks(IAPHISProductComponent[] productComponents)
		{
			foreach (var productComponent in productComponents)
			{
				var genus = productComponent.Genus;
				var species = productComponent.Species;
				var variety = productComponent.Variety;
				yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Component);
				if (!genus.IsEmpty || !species.IsEmpty || !variety.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG05(genus, species, variety);
				}

				var sourceTypeCode = productComponent.SourceType;
				var countryCode = productComponent.CountryCode;
				var location = productComponent.GeographicLocation;
				var processingStartDate = productComponent.ProcessingStartDate;
				var processingEndDate = productComponent.ProcessingEndDate;
				var processingTypeCode = productComponent.ProcessingTypeCode;
				var processingDescription = productComponent.ProcessingDescription;

				if (!sourceTypeCode.IsEmpty || !countryCode.IsEmpty || location.IsEmpty ||
					!processingStartDate.IsEmpty || !processingEndDate.IsEmpty || !processingDescription.IsEmpty || !processingTypeCode.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG06(sourceTypeCode, countryCode, location, processingStartDate, processingEndDate, processingTypeCode, processingDescription);
				}

				var specific = productComponent.SpecificName;
				if (!specific.IsEmpty)
				{
					yield return new AEPAPG17() { CommonNameSpecific = specific };
				}
			}
		}

		IEnumerable<MessageBlock> GetAPHISProductCharacteristic(IAPHISProductCharacteristic productCharacteristics, ZString categoryType, ZString categoryCode)
		{
			if (productCharacteristics != null)
			{
				foreach (var block in GetAPHISIdentityNumberRangeDetailsBlocks(productCharacteristics.Identities))
				{
					yield return block;
				}
			}

			if (productCharacteristics == null || !productCharacteristics.Characteristics.Any())
			{
				var pg10 = AEPAPGBlockHelper.MakePG10(ZString.Empty, ZString.Empty, ZString.Empty);
				pg10.CategoryTypeCode = categoryType;
				pg10.CategoryCode = categoryCode;
				yield return pg10;
			}
			else
			{
				foreach (var characteristic in productCharacteristics.Characteristics)
				{
					var pg10 = AEPAPGBlockHelper.MakePG10(characteristic);
					pg10.CategoryTypeCode = categoryType;
					pg10.CategoryCode = categoryCode;
					yield return pg10;
				}
			}
		}

		IEnumerable<MessageBlock> GetAPHISRoutingDetailsBlocks(IEnumerable<IAPHISRouting> routings)
		{
			foreach (var routing in routings)
			{
				yield return new AEPAPG32()
				{
					CommodityRoutingTypeCode = routing.RoutingType,
					CommodityRoutingCountryCode = routing.RoutingCountry,
					CommodityPoliticalSubunitOfRoutingName = routing.RoutingState
				};
			}
		}

		IEnumerable<MessageBlock> GetAPHISInspectionDetailsBlocks(IEnumerable<IAPHISInspection> inspections, ZDate arrivalDate, ZString arrivalLocation)
		{
			if (!inspections.Any() && (arrivalDate.IsValid || !arrivalLocation.IsEmpty))
			{
				yield return new AEPAPG30()
				{
					InspectionLaboratoryTestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation,
					AnticipatedArrivalDate = arrivalDate,
					AnticipatedArrivalLocationCode = !arrivalLocation.IsEmpty ? "2" : string.Empty,
					ArrivalLocation = arrivalLocation
				};
			}
			else
			{
				foreach (var inspection in inspections)
				{
					yield return new AEPAPG30()
					{
						InspectionLaboratoryTestingStatus = inspection.InspectionTestingStatus,
						AnticipatedArrivalDate = inspection.InspectionDate,
						AnticipatedArrivalLocationCode = inspection.InspectionLocationQualifier,
						ArrivalLocation = inspection.InspectionLocation
					};
				}
			}
		}

		IEnumerable<MessageBlock> GetContainerDetailsBlocks(IEnumerable<IContainerDetail> containerDetails)
		{
			if (containerDetails == null)
			{
				yield break;
			}

			var count = 0;
			AEPAPG27 pg27 = null;
			foreach (var containerDetail in containerDetails)
			{
				switch (count++)
				{
					case 0:
						pg27 = new AEPAPG27()
						{
							ContainerNumberEquipmentID = containerDetail.ContainerEquipmentID,
							ContainerLength = containerDetail.ContainerLength,
							TypeOfContainer = containerDetail.IsRefrigerated ? Refrigerated : NonRefrigerated
						};
						break;
					case 1:
						pg27.ContainerNumberEquipmentID1 = containerDetail.ContainerEquipmentID;
						pg27.ContainerLength1 = containerDetail.ContainerLength;
						pg27.TypeOfContainer1 = containerDetail.IsRefrigerated ? Refrigerated : NonRefrigerated;
						break;
					case 2:
						pg27.ContainerNumberEquipmentID2 = containerDetail.ContainerEquipmentID;
						pg27.ContainerLength2 = containerDetail.ContainerLength;
						pg27.TypeOfContainer2 = containerDetail.IsRefrigerated ? Refrigerated : NonRefrigerated;
						yield return pg27;
						pg27 = null;
						count = 0;
						break;
				}
			}
			if (pg27 != null)
			{
				yield return pg27;
			}
		}
		const string Refrigerated = "1";
		const string NonRefrigerated = "2";

		IEnumerable<MessageBlock> GetAPHISPackagingDetailsBlocks(FDAQtyUQPair[] orderedQtyUQs)
		{
			var number = orderedQtyUQs.Length;
			if (number > 0)
			{
				foreach (var pair in orderedQtyUQs)
				{
					yield return AEPAPGBlockHelper.MakePG26(number, pair.Qty, pair.UQ);
					number--;
				}
			}
		}

		IEnumerable<MessageBlock> GetAPHISLicenceDetailsBlocks(IEnumerable<IAPHISLicense> licences)
		{
			var authorizedParties = new List<Tuple<IAddressDetails, ZString>>();
			foreach (var licence in licences)
			{
				yield return new AEPAPG13()
				{
					LPCOIssuerGovernmentGeographicCodeQualifier = LPCOIssuerLocationTypeList.Codes.ISOCountryCode,
					LocationCountryStateProvinceOfIssuerOfTheLPCO = licence.Location,
					RegionalDescriptionOfLocationOfAgencyIssuingTheLPCO = licence.LocationDescription
				};

				yield return new AEPAPG14()
				{
					LPCOTransactionType = licence.TransactionType,
					LPCOType = licence.Type,
					LPCONumberorName = licence.Number,
					LPCODateQualifier = licence.DateQualifier,
					LPCODate = licence.Date,
					LPCOQuantity = licence.Quantity,
					LPCOUnitOfMeasure = licence.UnitOfMeasure,
				};
			}
		}

		IEnumerable<MessageBlock> GetAPHIS19_21PartyDetailsBlocks(IAPHISHeader aphisHeader)
		{
			var cropGrowerDetailsRegistrationNumber = aphisHeader.CropGrowerDetailsRegistrationNumber;
			foreach (var block in AEPAPGBlockHelper.MakeEntityBlocks(EntityRoleCodeList.Codes.CropGrower, aphisHeader.CropGrowerDetails, cropGrowerDetailsRegistrationNumber.NumberType, cropGrowerDetailsRegistrationNumber.Number, isCompanyNameRequiredAlways: true))
			{
				yield return block;
			}

			var applicantDetails = aphisHeader.ApplicantDetails;
			if (applicantDetails == null)
			{
				var entityDetailsShipper = aphisHeader.ShipperCBPAssignedNumber;
				foreach (var block in AEPAPGBlockHelper.MakeEntityBlocks(EntityRoleCodeList.Codes.Shipper, aphisHeader.ShipperDetails, entityDetailsShipper.EntityIdentificationCode, entityDetailsShipper.EntityNumber, isCompanyNameRequiredAlways: true))
				{
					yield return block;
				}
			}
			else
			{
				var entityDetailsApplicant = aphisHeader.ApplicantAPHISAssignedNumber;
				foreach (var block in AEPAPGBlockHelper.MakeEntityBlocks(EntityRoleCodeList.Codes.LPCOAuthorizedParty, aphisHeader.ApplicantDetails, entityDetailsApplicant.EntityIdentificationCode, entityDetailsApplicant.EntityNumber, isCompanyNameRequiredAlways: true))
				{
					yield return block;
				}
			}
			var entityDetailsPermitted = aphisHeader.PermittedAPHISAssignedNumber;
			foreach (var block in AEPAPGBlockHelper.MakeEntityBlocks(EntityRoleCodeList.Codes.PermittedDestination, aphisHeader.PermittedDetails, entityDetailsPermitted.EntityIdentificationCode, entityDetailsPermitted.EntityNumber, isCompanyNameRequiredAlways: true))
			{
				yield return block;
			}

			var ultimateCosigneeRegistrationNumber = aphisHeader.UltimateCosigneeRegistrationNumber;
			foreach (var block in AEPAPGBlockHelper.MakeEntityBlocks(EntityRoleCodeList.Codes.UltimateConsignee, aphisHeader.UltimateCosigneeDetails, ultimateCosigneeRegistrationNumber.NumberType, ultimateCosigneeRegistrationNumber.Number, isCompanyNameRequiredAlways: true))
			{
				yield return block;
			}

			var brokerDetails = aphisHeader.BrokerDetails;
			if (brokerDetails == null)
			{
				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Importer, aphisHeader.ImporterDetails, isCompanyNameRequiredAlways: true))
				{
					yield return block;
				}
			}
			else
			{
				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.CustomsBroker, brokerDetails, EntityIdentificationCodesList.Codes.CBPAssigned, aphisHeader.BrokerFilerCode, isCompanyNameRequiredAlways: true))
				{
					yield return block;
				}
			}

			var grower = aphisHeader.USDAAPHISGrower;
			if (grower != null)
			{
				foreach (var block in AEPAPGBlockHelper.MakeEntityBlocks(EntityRoleCodeList.Codes.USDAAPHISGrower, aphisHeader.USDAAPHISGrower, isCompanyNameRequiredAlways: true))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetAPHISIdentityNumberRangeDetailsBlocks(IEnumerable<IAPHISIdentity> identities)
		{
			foreach (var identity in identities)
			{
				var individualNumbers = new List<ZString>();
				var rangeNumbers = new List<INumberRange>();
				foreach (var numberRange in identity.Numbers)
				{
					var endNumber = numberRange.EndNumber;
					if (endNumber.IsEmpty)
					{
						individualNumbers.Add(numberRange.StartNumber);
					}
					else
					{
						rangeNumbers.Add(numberRange);
					}
				}
				var hasSingleNumber = individualNumbers.Count == 1 && rangeNumbers.Count == 0;
				var firstNumber = hasSingleNumber ? individualNumbers[0] : ZString.Empty;
				yield return new AEPAPG07() { ItemIdentityNumberQualifier = identity.IdentityType, ItemIdentityNumber = firstNumber };
				if (!hasSingleNumber)
				{
					AEPAPG08 pg08 = null;
					foreach (var numberRange in rangeNumbers.OrderBy(x => x.StartNumber + "|" + x.EndNumber))
					{
						if (pg08 != null)
						{
							pg08.ItemIdentityNumber2 = RangeStartIdentifier + numberRange.StartNumber;
							pg08.ItemIdentityNumber3 = RangeEndIdentifier + numberRange.EndNumber;
							yield return pg08;
							pg08 = null;
						}
						else
						{
							pg08 = new AEPAPG08() { ItemIdentityNumber = RangeStartIdentifier + numberRange.StartNumber, ItemIdentityNumber1 = RangeEndIdentifier + numberRange.EndNumber };
						}
					}

					var count = pg08 == null ? 0 : 2;
					foreach (var number in individualNumbers.OrderBy(x => x))
					{
						switch (count++)
						{
							case 0:
								pg08 = new AEPAPG08() { ItemIdentityNumber = number };
								break;
							case 1:
								pg08.ItemIdentityNumber1 = number;
								break;
							case 2:
								pg08.ItemIdentityNumber2 = number;
								break;
							default:
								pg08.ItemIdentityNumber3 = number;
								yield return pg08;
								pg08 = null;
								count = 0;
								break;
						}
					}
					if (pg08 != null)
					{
						yield return pg08;
					}
				}
			}
		}
		const string RangeStartIdentifier = "#RS";
		const string RangeEndIdentifier = "#RE";

		#endregion

		#region TTB Blocks

		IEnumerable<MessageBlock> GetTTBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.TTB))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[]
					{
						new AEPAPG01()
						{
							PGALineNumber = 1,
							GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.TTB,
							GovernmentAgencyProgramCode = TTBProgramCodeList.Codes.Tobacco,
							Disclaimer = entryLine.TTBDisclaimReason
						}
					};
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.TTBIndicator, entryLine.TTBLines, null, pgaIncludedList, GetTTBData, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetTTBData(ITTBLine ttbLine)
		{
			ttbLine.LineNo = ++tTBNumberCounter;
			var programCode = ttbLine.ProgramCode;
			var isWine = programCode == TTBProgramCodeList.Codes.Wine;
			var isDistilledSpirits = programCode == TTBProgramCodeList.Codes.DistilledSpirits;
			var isTobacco = programCode == TTBProgramCodeList.Codes.Tobacco;
			yield return AEPAPGBlockHelper.MakePG01(ttbLine.LineNo, ACEGovernmentAgenciesCodeList.Codes.TTB, programCode, ttbLine.ProcessingCode);
			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
			var permitNumber = ttbLine.PermitNumber;
			var exemptionCode = ttbLine.ExemptionCode;
			if (!permitNumber.IsEmpty || !exemptionCode.IsEmpty)
			{
				var permitDetail = AEPAPGBlockHelper.MakePG14(TTBPermitTypeList.Codes.TZ3, permitNumber);
				permitDetail.ExemptionCode = exemptionCode;
				yield return permitDetail;
			}
			var numberForIRC = ttbLine.NumberForIRC;
			if (numberForIRC.IsEmpty)
			{
				foreach (var block in GetTTBNonBondedBlocks(ttbLine, numberForIRC, isTobacco, isWine, isDistilledSpirits))
				{
					yield return block;
				}
			}
			else
			{
				foreach (var block in GetTTBBondedBlocks(ttbLine, numberForIRC))
				{
					yield return block;
				}
			}
			var cigars = ttbLine.Cigars.Select(x => string.Format(CultureInfo.InvariantCulture, "{0}{1}", (x.Quantity > 999999 ? "******" : x.Quantity.ToString().PadLeft(6, '0')), (x.IsSmall ? SmallCigars : "@" + (x.UnitPrice == TTBCigar.MaximuSalePrice ? LargeCigarMaximumRate : FormatCigarUnitPrice(x.UnitPrice.ToStringTrimZeros()))))).ToArray();
			if (cigars.Length > 0)
			{
				yield return new AEPAPG50();
				foreach (var cigar in cigars)
				{
					yield return new AEPAPG22() { ComplianceDescription = cigar };
				}
				yield return new AEPAPG51();
			}
			var quantityInPCS = ttbLine.QuantityInPCS;
			if (!quantityInPCS.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG29(quantityInPCS, ABIUnitOfMeasureList.Codes.Pieces);
			}
		}

		IEnumerable<MessageBlock> GetTTBBondedBlocks(ITTBLine ttbLine, ZString numberForIRC)
		{
			yield return AEPAPGBlockHelper.MakePG14(TTBPermitTypeList.Codes.TZ5, numberForIRC);
			yield return new AEPAPG50();
			var consignee = ttbLine.Consignee;
			if (consignee != null)
			{
				foreach (var block in AEPAPGBlockHelper.MakePG19(EntityRoleCodeList.Codes.Consignee, EntityIdentificationCodesList.Codes.IRSAssigned, ttbLine.ConsigneeEIN, consignee.CompanyName, consignee.AddressLine1))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakePG20(consignee))
				{
					yield return block;
				}
			}
			yield return new AEPAPG22() { ImportersSubstantiatingSignedDocumentSignedConfirmationLetter = "Y", DocumentIdentifier = TTBDocumentIdentifierList.Codes.PaymentOrPerformanceBond, ComplianceDescription = InternalRevenueCodeCertification };
			yield return new AEPAPG51();
		}

		IEnumerable<MessageBlock> GetTTBNonBondedBlocks(ITTBLine ttbLine, ZString numberForIRC, bool isTobacco, bool isWine, bool isDistilledSpirits)
		{
			if (!isTobacco)
			{
				foreach (var colaAndCertificates in ttbLine.COLAAndCertificates.Where(x => !x.COLA.IsEmpty || !x.ExemptionCode.IsEmpty || !x.ForeignCertificateCountry.IsEmpty).GroupBy(x => x.ForeignCertificateCountry).OrderBy(x => x.Key))
				{
					var country = colaAndCertificates.Key;
					var isCountrySpecified = !country.IsEmpty && (isWine || isDistilledSpirits);
					foreach (var colaAndCertificate in colaAndCertificates.OrderBy(x => x.COLA))
					{
						var cola = colaAndCertificate.COLA;
						var exemptionCode = colaAndCertificate.ExemptionCode;
						var isCOLASpecified = !cola.IsEmpty;
						if (isCOLASpecified)
						{
							yield return AEPAPGBlockHelper.MakePG14(TTBPermitTypeList.Codes.TZ1, cola);
						}
						else if (!exemptionCode.IsEmpty)
						{
							yield return new AEPAPG14() { LPCOType = TTBPermitTypeList.Codes.TZ1, ExemptionCode = exemptionCode };
						}

						if (isCountrySpecified || isCOLASpecified)
						{
							yield return new AEPAPG50();
							if (isCOLASpecified)
							{
								yield return new AEPAPG22() { ImportersSubstantiatingSignedDocumentSignedConfirmationLetter = "Y", DocumentIdentifier = TTBDocumentIdentifierList.Codes.COLA };
							}
							if (isCountrySpecified)
							{
								yield return new AEPAPG22() { ImportersSubstantiatingSignedDocumentSignedConfirmationLetter = "Y", DocumentIdentifier = isDistilledSpirits ? TTBDocumentIdentifierList.Codes.DistilledSpiritsCertificate : TTBDocumentIdentifierList.Codes.WineCertificate };
							}
							yield return new AEPAPG51();
						}
					}
					if (isCountrySpecified)
					{
						yield return AEPAPGBlockHelper.MakePG13(country);
						yield return new AEPAPG50();
						yield return AEPAPGBlockHelper.MakePG14(TTBPermitTypeList.Codes.TZ4, ZString.Empty);
						yield return new AEPAPG51();
					}
				}
			}
		}

		string FormatCigarUnitPrice(ZString unitPrice)
		{
			var numbers = unitPrice.Split('.');
			return "." + numbers[0].PadLeft(2, '0').Right(2) + (numbers.Length > 1 ? numbers[1].PadRight(3, '0').Left(3) : (ZString)"000");
		}
		const string InternalRevenueCodeCertification = "IRC";
		const string LargeCigarMaximumRate = "MAXIMUM RATE";
		const string SmallCigars = " SMALL CIGARS";

		#endregion

		#region NMFS blocks

		IEnumerable<MessageBlock> GetNMFSBlocks(IGovernmentAgencies governmentAgenciesData, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(governmentAgenciesData, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes._370))
			{
				foreach (MessageBlock block in GetNMFSLine(governmentAgenciesData, NMFSProgramCodeList.Codes._370, governmentAgenciesData.NMFS370Indicator, governmentAgenciesData.NMFS370DisclaimReason, governmentAgenciesData.NMFS370Lines, pgaIncludedList, isPGACorrection))
				{
					yield return block;
				}
			}
			if (ShouldSendPGA(governmentAgenciesData, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.AMR))
			{
				foreach (MessageBlock block in GetNMFSLine(governmentAgenciesData, NMFSProgramCodeList.Codes.AMR, governmentAgenciesData.NMFSAMRIndicator, governmentAgenciesData.NMFSAMRDisclaimReason, governmentAgenciesData.NMFSAMRLines, pgaIncludedList, isPGACorrection))
				{
					yield return block;
				}
			}
			if (ShouldSendPGA(governmentAgenciesData, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.HMS))
			{
				foreach (MessageBlock block in GetNMFSLine(governmentAgenciesData, NMFSProgramCodeList.Codes.HMS, governmentAgenciesData.NMFSHMSIndicator, governmentAgenciesData.NMFSHMSDisclaimReason, governmentAgenciesData.NMFSHMSLines, pgaIncludedList, isPGACorrection))
				{
					yield return block;
				}
			}
			if (ShouldSendPGA(governmentAgenciesData, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.SIMP))
			{
				foreach (MessageBlock block in GetNMFSLine(governmentAgenciesData, NMFSProgramCodeList.Codes.SIM, governmentAgenciesData.NMFSSIMIndicator, "", governmentAgenciesData.NMFSSIMLines, pgaIncludedList, isPGACorrection))
				{
					yield return block;
				}
			}
			if (ShouldSendPGA(governmentAgenciesData, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.COA))
			{
				foreach (MessageBlock block in GetNMFSLine(governmentAgenciesData, NMFSProgramCodeList.Codes.COA, governmentAgenciesData.NMFSCOAIndicator, "", governmentAgenciesData.NMFSCOALines, pgaIncludedList, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetNMFSLine(IGovernmentAgencies governmentAgenciesData, ZString programCode, ZString indicator, ZString disclaimReason, IEnumerable<INMFSLine> nmfsLines, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, bool isPGACorrection)
		{
			Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
			{
				if (programCode == NMFSProgramCodeList.Codes.SIM && isPGACorrection)
				{
					return new[] { AEPAPGBlockHelper.MakePG01ForDataCorrectionDeleteAll(ACEGovernmentAgenciesCodeList.Codes.NMF) };
				}
				else
				{
					return new[]
					{
						new AEPAPG01()
						{
							PGALineNumber = ++nMFSNumberCounter,
							GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.NMF,
							GovernmentAgencyProgramCode = programCode,
							Disclaimer = disclaimReason
						}
					};
				}
			};

			foreach (MessageBlock block in GenerateBlocks(indicator, nmfsLines, GetAllNMFSLines(governmentAgenciesData), pgaIncludedList, GetNMFSData, getDisclaimedBlocks, isPGACorrection))
			{
				yield return block;
			}
		}

		IEnumerable<MessageBlock> GetNMFSData(INMFSLine nMFSLine)
		{
			nMFSLine.LineNo = ++nMFSNumberCounter;
			var programCode = nMFSLine.ProgramCode;
			var is370 = programCode == NMFSProgramCodeList.Codes._370;
			var isAMR = programCode == NMFSProgramCodeList.Codes.AMR;
			var isHMS = programCode == NMFSProgramCodeList.Codes.HMS;
			var isSIM = programCode == NMFSProgramCodeList.Codes.SIM;
			var isCOA = programCode == NMFSProgramCodeList.Codes.COA;
			var processingCode = ZString.Empty;
			var processingCodeDetail = ZString.Empty;

			if (is370)
			{
				processingCode = nMFSLine.ContainsYellowfinTuna ? NMFSProductCategoryCodeList.Codes.YellowfinTuna : NMFSProductCategoryCodeList.Codes.Other;
			}
			else if (isAMR)
			{
				processingCodeDetail = nMFSLine.ToothfishState;
				if (processingCodeDetail == FishStateList.Codes.FreshKrill || processingCodeDetail == FishStateList.Codes.FreshToothfish)
				{
					processingCode = new ZString("FRE");
				}
				else if (processingCodeDetail == FishStateList.Codes.FrozenKrill || processingCodeDetail == FishStateList.Codes.FrozenToothfish)
				{
					processingCode = new ZString("FRZ");
				}
			}

			if (isSIM)
			{
				yield return AEPAPGBlockHelper.MakePG01(nMFSLine.LineNo, ACEGovernmentAgenciesCodeList.Codes.NMF, nMFSLine.ProgramCode, processingCode, nMFSLine.Confidential);
			}
			else if (isCOA)
			{
				yield return AEPAPGBlockHelper.MakePG01(nMFSLine.LineNo, ACEGovernmentAgenciesCodeList.Codes.NMF, nMFSLine.ProgramCode, ZString.Empty, nMFSLine.Confidential);
			}
			else
			{
				yield return AEPAPGBlockHelper.MakePG01(nMFSLine.LineNo, ACEGovernmentAgenciesCodeList.Codes.NMF, nMFSLine.ProgramCode, processingCode, nMFSLine.ElectronicImageSubmitted, true);
			}

			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);

			if (isAMR)
			{
				yield return new AEPAPG10() { CommodityQualifierCode = processingCode };

				yield return AEPAPGBlockHelper.MakePG14("2", "NM4", nMFSLine.IFTPPermitNumber);

				if (processingCodeDetail == FishStateList.Codes.FrozenToothfish)
				{
					yield return AEPAPGBlockHelper.MakePG14("1", "NM2", nMFSLine.PreApprovalIssuedNumber, nMFSLine.PreApprovalIssuedQuantity, "KG");
				}
				else
				{
					foreach (var documentDetail in nMFSLine.DocumentDetails)
					{
						yield return AEPAPGBlockHelper.MakePG22(documentDetail, is370);
					}
				}
			}
			else if (isSIM)
			{
				yield return AEPAPGBlockHelper.MakePG05(nMFSLine.SpeciesCode);

				var firstHarvestingDetail = nMFSLine.HarvestingDetails.FirstOrDefault();
				if (firstHarvestingDetail != null)
				{
					yield return AEPAPGBlockHelper.MakePG06(firstHarvestingDetail.SourceTypeCode, firstHarvestingDetail.CountryCode,
						firstHarvestingDetail.GeographicLocation, firstHarvestingDetail.ProcessingStartDate, firstHarvestingDetail.ProcessingTypeCode, firstHarvestingDetail.ProcessingDescription);

					yield return AEPAPGBlockHelper.MakePG14("2", "NM4", nMFSLine.IFTPPermitNumber);

					if (!nMFSLine.OtherAuthorizationNumber.IsEmpty)
					{
						yield return AEPAPGBlockHelper.MakePG14(nMFSLine.AuthorizationType, "NM6", nMFSLine.OtherAuthorizationNumber);
					}

					foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(firstHarvestingDetail.ContactPartyType, firstHarvestingDetail.ContactPartyDetails))
					{
						yield return block;
					}

					if (firstHarvestingDetail.SourceTypeCode == SourceTypeCodesList.Codes.HatcheryBasedAquaculture)
					{
						yield return AEPAPGBlockHelper.MakePG29(nMFSLine.NetWeight, nMFSLine.NetWeightUQ);
					}
					else
					{
						if (firstHarvestingDetail.SourceTypeCode == SourceTypeCodesList.Codes.HarvestOfCaptureFisheries)
						{
							foreach (var vessel in firstHarvestingDetail.Vessels)
							{
								yield return new AEPAPG50();
								yield return new AEPAPG31()
								{
									CommodityHarvestingVesselCharacteristicTypeCode = "VCR",
									CommodityHarvestingVesselCharacteristic = vessel.HarvestedCountry,
									HarvestedCommodityNetWeight = vessel.NetWeight,
									UnitOfMeasureconveyance = vessel.NetWeightUQ
								};

								yield return new AEPAPG31()
								{
									CommodityHarvestingVesselCharacteristicTypeCode = "VNM",
									CommodityHarvestingVesselCharacteristic = vessel.HarvestedVessel,
								};

								if (!vessel.TranshipmentPlace.IsEmpty)
								{
									yield return AEPAPGBlockHelper.MakePG32("13", vessel.TranshipmentPlace);
								}

								if (!vessel.FirstLandingCountry.IsEmpty)
								{
									yield return AEPAPGBlockHelper.MakePG32("11", vessel.FirstLandingCountry);
								}

								yield return new AEPAPG51();
							}
						}
						else if (firstHarvestingDetail.SourceTypeCode == SourceTypeCodesList.Codes.SmallVesselHarvest)
						{
							yield return AEPAPGBlockHelper.MakePG24WithRemarkText(ZString.Empty, firstHarvestingDetail.NumberOfVessels.ToString());
							yield return AEPAPGBlockHelper.MakePG29(nMFSLine.NetWeight, nMFSLine.NetWeightUQ);
							yield return AEPAPGBlockHelper.MakePG32("11", firstHarvestingDetail.FirstLandingCountry);
						}
					}
				}
			}
			else if (isCOA)
			{
				yield return AEPAPGBlockHelper.MakePG05(nMFSLine.SpeciesCode);

				foreach (var harvestingDetailData in nMFSLine.HarvestingDetails)
				{
					yield return AEPAPGBlockHelper.MakePG06(harvestingDetailData.SourceTypeCode, harvestingDetailData.CountryCode,
						harvestingDetailData.GeographicLocation, harvestingDetailData.ProcessingStartDate, harvestingDetailData.ProcessingTypeCode, ZString.Empty);
				}

				yield return AEPAPGBlockHelper.MakePG14("2", "NM4", nMFSLine.IFTPPermitNumber);
				yield return AEPAPGBlockHelper.MakePG22COA();
			}
			else
			{
				yield return AEPAPGBlockHelper.MakePG14("2", "NM4", nMFSLine.IFTPPermitNumber);
				if (isHMS && !nMFSLine.eBCDNumber.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG14("1", "NM5", nMFSLine.eBCDNumber);
				}

				foreach (var harvestingDetailData in nMFSLine.HarvestingDetails)
				{
					yield return new AEPAPG50();
					yield return AEPAPGBlockHelper.MakePG06(SourceTypeCodesList.Codes.Harvested, harvestingDetailData.CountryCode,
						harvestingDetailData.GeographicLocation, harvestingDetailData.ProcessingTypeCode);
					if (is370)
					{
						var pg10 = new AEPAPG10() { CategoryTypeCode = PGACategoryTypeCodeList.Codes.NMFSProductDescription };
						if (harvestingDetailData.ContainsYellowfinTuna)
						{
							pg10.CommodityCharacteristicDescription = ContainsYellowfinTuna;
							pg10.CategoryCode = NMFSProductCategoryCodeList.Codes.YellowfinTuna;
						}
						else
						{
							pg10.CategoryCode = NMFSProductCategoryCodeList.Codes.Other;
						}
						yield return pg10;
					}

					foreach (var documentDetail in nMFSLine.DocumentDetails)
					{
						yield return AEPAPGBlockHelper.MakePG22(documentDetail, is370);
					}

					foreach (var harvestingVessel in harvestingDetailData.HarvestingVessels)
					{
						yield return new AEPAPG31()
						{
							CommodityHarvestingVesselCharacteristicTypeCode = "VCR",
							CommodityHarvestingVesselCharacteristic = harvestingVessel
						};
					}
					yield return new AEPAPG51();
				}
			}
		}
		const string ContainsYellowfinTuna = "CONTAINS YELLOWFIN TUNA";

		#endregion

		#region DDTC blocks

		IEnumerable<MessageBlock> GetDDTCBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			var ddtcData = entryLine.DDTCData;
			if (ddtcData != null && ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.DDTC))
			{
				foreach (MessageBlock block in GenerateBlocks(entryLine.DDTCIndicator, new[] { ddtcData }, null, pgaIncludedList, GetDDTCData, GetDDTCDisclaimedData, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetDDTCDisclaimedData()
		{
			yield return AEPAPGBlockHelper.MakePG01ForDataCorrectionDeleteAll(ACEGovernmentAgenciesCodeList.Codes.DTC);
		}

		IEnumerable<MessageBlock> GetDDTCData(IDDTCData ddtcData)
		{
			yield return AEPAPGBlockHelper.MakePG01(1, ACEGovernmentAgenciesCodeList.Codes.DTC, ACEGovernmentAgenciesCodeList.Codes.DTC, ZString.Empty);
			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
			var pga14 = AEPAPGBlockHelper.MakePG14(ddtcData.LicenseType, ddtcData.LicenseNumber);
			pga14.ExemptionCode = ddtcData.ExemptionCode;
			yield return pga14;
			if (!ddtcData.RegistrationNumber.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG14("DD1", ddtcData.RegistrationNumber);
			}
			var pg30 = new AEPAPG30() { InspectionLaboratoryTestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation };
			var anticipatedArrivalDate = ddtcData.AnticipatedArrivalDate;
			if (anticipatedArrivalDate.IsValid)
			{
				pg30.AnticipatedArrivalDate = anticipatedArrivalDate.Date;
				pg30.ArrivalTime = anticipatedArrivalDate.ToString("HHmm");
			}
			yield return pg30;
		}

		#endregion

		#region FDA blocks

		IEnumerable<MessageBlock> GetFDABlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.FDA))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[] { AEPAPGBlockHelper.MakePG01ForDisclaimer(1, ACEGovernmentAgenciesCodeList.Codes.FDA, "FDA", entryLine.ACEFDADisclaimReason) };
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.ACEFDAIndicator, entryLine.FDALines, null, pgaIncludedList, GetFDABlocksForOneLineNotStandalonePN, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		internal IEnumerable<MessageBlock> GetFDABlocksForOneLineNotStandalonePN(IFDAData data)
		{
			return GetFDABlocksForOneLine(data, false);
		}

		internal IEnumerable<MessageBlock> GetFDABlocksForOneLine(IFDAData data, bool isStandalonePN = false)
		{
			fDALineNumberCounter++;
			data.LineNo = fDALineNumberCounter;
			yield return new AEPAPG01()
			{
				PGALineNumber = fDALineNumberCounter,
				GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.FDA,
				GovernmentAgencyProgramCode = data.ProgramCode,
				GovernmentAgencyProcessingCode = data.ProcessingCode,
				IntendedUseCode = data.IntendedUseCode,
				IntendedUseDescription = data.IntendedUseDescription
			};

			yield return new AEPAPG02()
			{
				ItemType = PG02ItemTypeList.Codes.Product,
				ProductCodeQualifier = ProductCodeQualifiersList.Codes.FDAProductCode,
				ProductCodeNumber = data.ProductCode
			};

			if (!data.Remarks.IsEmpty)
			{
				yield return new AEPAPG24() { RemarksTypeCode = RemarksTypeCodeList.Codes.GEN, RemarksText = data.Remarks };
			}

			if (!data.SourceCountry.IsEmpty)
			{
				yield return new AEPAPG06() { SourceTypeCode = SourceTypeCodesList.Codes.CountryOfSource, CountryCode = data.SourceCountry };
			}
			if (!data.ProductionGrowthCountry.IsEmpty)
			{
				yield return new AEPAPG06() { SourceTypeCode = data.ProductionGrowthCountryQualifier, CountryCode = data.ProductionGrowthCountry };
			}

			if (!data.ShipmentCountry.IsEmpty)
			{
				yield return new AEPAPG06() { SourceTypeCode = SourceTypeCodesList.Codes.CountryOfShipment, CountryCode = data.ShipmentCountry };
			}

			if (!data.RefusalCountry.IsEmpty)
			{
				yield return new AEPAPG06() { SourceTypeCode = SourceTypeCodesList.Codes.CountryOfRefusal, CountryCode = data.RefusalCountry };
			}

			if (!data.BrandName.IsEmpty)
			{
				var pg07 = AEPAPGBlockHelper.MakePG07(data.BrandName);
				pg07.ItemIdentityNumber = data.ItemIdentityNumber;
				pg07.ItemIdentityNumberQualifier = data.ItemIdentityNumberQualifier;
				yield return pg07;
			}
			if (!data.Description.IsEmpty)
			{
				yield return new AEPAPG10()
				{
					CommodityCharacteristicDescription = data.Description
				};
			}

			foreach (var block in GetProductConstituentElements(data.ProductConstituentElements))
			{
				yield return block;
			}

			foreach (var block in GetLicenses(data.Licenses))
			{
				yield return block;
			}

			if (!data.PNConfirmationNumber.IsEmpty)
			{
				yield return new AEPAPG14()
				{
					LPCOTransactionType = "1",
					LPCOType = "PNC",
					LPCONumberorName = data.PNConfirmationNumber
				};
			}

			#region Parties

			foreach (var block in AEPAPGBlockHelper.MakeFDAPGAContactBlocks(data.FirmType, data.ManufacturerNumber, data.ManufacturerContact))
			{
				yield return block;
			}

			foreach (var block in AEPAPGBlockHelper.MakeFDAPGAContactBlocks(EntityRoleCodeList.Codes.Shipper, data.ShipperNumber, data.ShipperContact))
			{
				yield return block;
			}

			foreach (var block in AEPAPGBlockHelper.MakeFDAPGAContactBlocks(EntityRoleCodeList.Codes.FDAImporter1, data.FDAImporterNumber, data.FDAImporterContact))
			{
				yield return block;
			}

			foreach (var block in AEPAPGBlockHelper.MakeFDAPGAContactBlocks(data.DeliveryPartyRoleCode, data.DeliveryPartyNumber, data.DeliveryPartyContact))
			{
				yield return block;
			}

			if (data.ProcessingCode != FDAProcessingCodeList.Codes.FOO_CCW)
			{
				foreach (var block in AEPAPGBlockHelper.MakeFDAPGAContactBlocks(data.InitialImporterRoleCode, data.ProducerNumber, data.ProducerContact))
				{
					yield return block;
				}

				if (!isStandalonePN)
				{
					foreach (var block in AEPAPGBlockHelper.MakeFDAPGAContactBlocks(EntityRoleCodeList.Codes.FSVPImporter, data.FSVPImporterNumber, data.FSVPImporterContact))
					{
						yield return block;
					}
				}

				if (data.IsSubmitterRelevant)
				{
					foreach (var block in AEPAPGBlockHelper.MakeFDAPGAContactBlocks(data.SubmitterType, data.SubmitterNumber, data.SubmitterContact))
					{
						yield return block;
					}
				}

				if (data.IsTransmitterRelevant)
				{
					foreach (var block in AEPAPGBlockHelper.MakeFDAIndividualBlocks(EntityRoleCodeList.Codes.PNTransmitter, data.TransmitterNumber, data.Transmitter))
					{
						yield return block;
					}
				}

				foreach (var block in AEPAPGBlockHelper.MakeFDAPGAContactBlocks(EntityRoleCodeList.Codes.Owner, data.OwnerNumber, data.OwnerContact))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakeFDAPGAContactBlocks(EntityRoleCodeList.Codes.LocationOfGoodsImmediatelyAfterEntryRelease, data.LocationOfGoodsNumber, data.LocationOfGoodsContact))
				{
					yield return block;
				}

				foreach (var pair in data.ActiveIngredientProducers)
				{
					foreach (var block in AEPAPGBlockHelper.MakeFDAPGAContactBlocks(EntityRoleCodeList.Codes.Producer, pair.Value, pair.Key))
					{
						yield return block;
					}
				}
			}

			var transmitter = data.Transmitter;
			if (transmitter != null)//For prior notice, it is sent twice as PNT and PK
			{
				foreach (var block in AEPAPGBlockHelper.MakeFDAIndividualBlocks(EntityRoleCodeList.Codes.PointOfContact, data.TransmitterNumber, transmitter))
				{
					yield return block;
				}
			}

			#endregion

			foreach (var block in GetAffirmationOfCompliance(data.AffirmationOfCompliance, isStandalonePN))
			{
				yield return block;
			}

			if (!data.Lots.Any() && (data.PGALineValue > 0m || data.UnitValue > 0m))
			{
				yield return new AEPAPG25()
				{
					TemperatureQualifier = ZString.Empty,
					DegreeType = ZString.Empty,
					LocationOfTemperatureRecording = ZString.Empty,
					LotNumber = ZString.Empty,
					LotNumberQualifier = ZString.Empty,
					PGALineValue = data.PGALineValue,
					PGAUnitValue = data.UnitValue,
					ProductionEndDateOfTheLot = ZDate.Empty,
					ProductionStartDateOfTheLot = ZDate.Empty,
					ActualTemperature = ZString.Empty,
					NegativeNumber = ZString.Empty
				};
			}
			else
			{
				foreach (var block in GetLots(data.PGALineValue, data.UnitValue, data.Lots))
				{
					yield return block;
				}
			}

			foreach (var block in GetPackagingDetails(data))
			{
				yield return block;
			}

			foreach (MessageBlock block in GetContainers(data))
			{
				yield return block;
			}

			if (!data.CanDimensions1.IsEmpty || !data.CanDimensions2.IsEmpty || !data.CanDimensions3.IsEmpty || !data.PackageTrackingNumber.IsEmpty)
			{
				yield return new AEPAPG28()
				{
					CanDimensions1 = data.CanDimensions1,
					CanDimensions2 = data.CanDimensions2,
					CanDimension3 = data.CanDimensions3,
					PackageTrackingNumberDetails = data.PackageTrackingNumberCode.PadRight(4, ' ') + data.PackageTrackingNumber
				};
			}

			if (data.InspectionDate.IsValid || !data.ArrivalLocation.IsEmpty)
			{
				yield return new AEPAPG30()
				{
					InspectionLaboratoryTestingStatus = InspectionStatusList.Codes.BTAAnticipatedArrivalInformation,
					AnticipatedArrivalDate = data.InspectionDate,
					ArrivalTime = data.InspectionTime,
					AnticipatedArrivalLocationCode = !data.ArrivalLocation.IsEmpty ? "2" : string.Empty,
					ArrivalLocation = data.ArrivalLocation
				};
			}

			if (!data.GoodsFromFTZ.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG30(InspectionStatusList.Codes.ForeignTradeZone, "4", data.GoodsFromFTZ);
			}
		}

		IEnumerable<MessageBlock> GetProductConstituentElements(IEnumerable<IConstituentElement> productConstituentElements)
		{
			foreach (var element in productConstituentElements)
			{
				yield return new AEPAPG04()
				{
					ConstituentActiveIngredientQualifier = YesNoDefaultList.Codes.Yes,
					NameOfTheConstituentElement = element.Name.Left(51),
					QuantityOfConstituentElement = element.Quantity,
					UnitOfMeasureConstituentElement = element.UnitOfMeasure,
					PercentOfConstituentElement = element.Percent
				};
			}
		}

		IEnumerable<MessageBlock> GetAffirmationOfCompliance(IEnumerable<KeyValuePair<ZString, ZString>> affirmationOfCompliance, bool isStandalonePN)
		{
			foreach (var aoc in affirmationOfCompliance)
			{
				if (!isStandalonePN || (isStandalonePN && ACE_AffirmationOfComplianceList.CanBeSentWithPNS(aoc.Key)))
				{
					yield return new AEPAPG23()
					{
						AffirmationOfComplianceCode = aoc.Key,
						AffirmationOfComplianceDescription = aoc.Value.Left(ACEAffirmationCode.Schema.DescriptionMaxLength)
					};
				}
			}
		}

		IEnumerable<MessageBlock> GetLots(ZDecimal pGALineValue, ZDecimal unitValue, IEnumerable<IFDALot> lots)
		{
			var pgaLineValueHasBeenSent = false;

			foreach (var lot in lots)
			{
				yield return new AEPAPG25()
				{
					TemperatureQualifier = lot.TemperatureQualifier,
					DegreeType = lot.DegreeType,
					LocationOfTemperatureRecording = lot.LocationOfTemperatureRecording,
					LotNumber = lot.Number,
					LotNumberQualifier = lot.NumberQualifier,
					PGALineValue = !pgaLineValueHasBeenSent ? pGALineValue : ZDecimal.Zero,
					PGAUnitValue = !pgaLineValueHasBeenSent ? unitValue : ZDecimal.Zero,
					ProductionEndDateOfTheLot = lot.ProductionEndDate,
					ProductionStartDateOfTheLot = lot.ProductionStartDate,
					ActualTemperature = lot.Temperature,
					NegativeNumber = lot.NegativeTemperatureIndicator
				};

				pgaLineValueHasBeenSent = true;
			}
		}

		IEnumerable<MessageBlock> GetLicenses(IEnumerable<IFDALicense> licenses)
		{
			foreach (var license in licenses)
			{
				yield return new AEPAPG13()
				{
					IssuerOfLPCO = license.Issuer,
					LPCOIssuerGovernmentGeographicCodeQualifier = license.CountryCode,
					LocationCountryStateProvinceOfIssuerOfTheLPCO = license.StateCode,
					RegionalDescriptionOfLocationOfAgencyIssuingTheLPCO = license.StateDescription
				};

				yield return new AEPAPG14()
				{
					LPCOTransactionType = "1",
					LPCOType = "POV",
					LPCONumberorName = license.Number
				};
			}
		}

		IEnumerable<MessageBlock> GetPackagingDetails(IFDAData data)
		{
			var number = 1;

			foreach (FDAQtyUQPair pair in data.OrderedQtyUQs)
			{
				yield return AEPAPGBlockHelper.MakePG26(number, pair.Qty, pair.UQ);
				number++;
			}
		}

		IEnumerable<MessageBlock> GetContainers(IFDAData data)
		{
			foreach (ZString number in data.ContainerNumbers)
			{
				yield return new AEPAPG27()
				{
					ContainerNumberEquipmentID = number
				};
			}
		}

		#endregion

		#region NHTSA blocks

		IEnumerable<MessageBlock> GetNHTSABlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.NHTSA))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[]
					{
						new AEPAPG01()
						{
							PGALineNumber = 1,
							GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.NHT,
							GovernmentAgencyProgramCode = NHTSAProgramCodeList.Codes.OFF,
							Disclaimer = entryLine.NHTSADisclaimReason
						}
					};
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.NHTSAIndicator, entryLine.NHTSALines, null, pgaIncludedList, GetNHTSAData, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetNHTSAData(INHTSAHeader commonData)
		{
			nHTSANumberCounter++;
			commonData.LineNumber = nHTSANumberCounter;
			yield return AEPAPGBlockHelper.MakePG01(commonData, nHTSANumberCounter);
			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);

			foreach (var block in GetNHTSAPG07_14DetailsGroupBlocks(commonData))
			{
				yield return block;
			}

			foreach (var block in GetNHTSAContactBlocks(commonData))
			{
				yield return block;
			}

			var shouldPG24BeSent = commonData.BoxNumber == DepartmentOfTransportBoxNumberList.Codes._06 && !commonData.EmbassyNationality.IsEmpty;
			if (shouldPG24BeSent)
			{
				yield return AEPAPGBlockHelper.MakePG24(commonData);
			}

			var shouldPG35BeSent = !commonData.DOTSuretyCode.IsEmpty || !commonData.DOTBondSerialNumber.IsEmpty ||
									 !commonData.DOTBondType.IsEmpty || !commonData.DOTBondAmount.IsEmpty;
			if (shouldPG35BeSent)
			{
				yield return AEPAPGBlockHelper.MakePG35(commonData);
			}
		}

		#region PG07 - PG14

		IEnumerable<MessageBlock> GetNHTSAPG07_14DetailsGroupBlocks(INHTSAHeader commonData)
		{
			var detailsList = commonData.Details.ToList();
			if (detailsList.Count > 0)
			{
				foreach (var detailsLine in detailsList)
				{
					var requireGroupingIndicator = detailsList.Count > 1 || detailsLine.OtherAdditionalNumbers.Any();
					foreach (var block in GetNHTSAPG07_14DetailsBlocks(detailsLine, detailsLine.NumberType, detailsLine.Number, detailsLine.AdditionalNumbers, requireGroupingIndicator, true))
					{
						yield return block;
					}
				}
			}
		}

		IEnumerable<MessageBlock> GetNHTSAPG07_14DetailsBlocks(INHTSADetails commonData, ZString numberType, ZString number, IEnumerable<INHTSAAdditionalNumber> additionalNumbers, ZBool requireGroupingIndicator, bool shouldGenerateOtherAdditionalNumbers = false)
		{
			if (requireGroupingIndicator)
			{
				yield return AEPAPGBlockHelper.MakePG50();
			}

			yield return AEPAPGBlockHelper.MakePG07(commonData, numberType, number);

			var additionalNumbersArray = additionalNumbers.ToArray();
			if (additionalNumbersArray.Length > 0)
			{
				ZString[] additionNumberString = null;
				for (int i = 0; i < additionalNumbersArray.Length; i++)
				{
					var remainder = i % 4;
					if (remainder == 0)
					{
						additionNumberString = new[] { ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty };
					}

					additionNumberString[remainder] = additionalNumbersArray[i].Number;

					if (i % 4 - 3 == 0 || i == additionalNumbersArray.Length - 1)
					{
						yield return AEPAPGBlockHelper.MakePG08(additionNumberString);
					}
				}
			}

			foreach (var block in GenerateNHTSACategoryBlocks(commonData))
			{
				yield return block;
			}

			foreach (var permitAndLicense in commonData.PermitAndLicenses)
			{
				var shouldPG14BeSent = !permitAndLicense.TransactionType.IsEmpty || !permitAndLicense.LPCOType.IsEmpty || !permitAndLicense.LPCONumber.IsEmpty
									|| !permitAndLicense.DateType.IsEmpty || !permitAndLicense.LPCODate.IsEmpty || !permitAndLicense.LPCOQuantity.IsEmpty;

				if (shouldPG14BeSent)
				{
					yield return AEPAPGBlockHelper.MakePG14(permitAndLicense);
				}
			}

			if (requireGroupingIndicator)
			{
				yield return AEPAPGBlockHelper.MakePG51();
			}

			if (shouldGenerateOtherAdditionalNumbers)
			{
				foreach (var otherAdditionalNumbers in commonData.OtherAdditionalNumbers)
				{
					var firstOtherNumberTypeAndNum = otherAdditionalNumbers.FirstOrDefault();
					if (firstOtherNumberTypeAndNum != null)
					{
						var otherAdditionalNumbersList = otherAdditionalNumbers.Where(x => x.Number != firstOtherNumberTypeAndNum.Number);
						foreach (var block in GetNHTSAPG07_14DetailsBlocks(commonData, firstOtherNumberTypeAndNum.NumberType, firstOtherNumberTypeAndNum.Number, otherAdditionalNumbersList, true))
						{
							yield return block;
						}
					}
				}
			}
		}

		#region Category Blocks

		IEnumerable<MessageBlock> GenerateNHTSACategoryBlocks(INHTSADetails commonData)
		{
			if (!commonData.DriveSide.IsEmpty || !commonData.ModelYear.IsEmpty)
			{
				if (!commonData.DriveSide.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG10(commonData, CommodityVehicleQualifierCodesList.Codes.V01, commonData.DriveSide);
				}

				if (!commonData.ModelYear.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG10(commonData, CommodityVehicleQualifierCodesList.Codes.V06, commonData.ModelYear);
				}
			}
			else
			{
				yield return AEPAPGBlockHelper.MakePG10(commonData, ZString.Empty, ZString.Empty);
			}
		}

		#endregion

		#region Contact Blocks

		IEnumerable<MessageBlock> GetNHTSAContactBlocks(INHTSAHeader commonData)
		{
			var rolesForPG55 = new List<ZString> { EntityRoleCodeList.Codes.CertifyingIndividual };

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Consignee, commonData.ConsigneeDetails))
			{
				yield return block;
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithConditionalPG21(EntityRoleCodeList.Codes.Owner, commonData.OwnerDetails, commonData.CertifyingIndividual == PartyTypeList.Codes.Owner ? commonData.ContactDetails : null))
			{
				yield return block;
			}
			if (commonData.CertifyingIndividual == PartyTypeList.Codes.Owner && commonData.OwnerDetails != null)
			{
				yield return AEPAPGBlockHelper.MakePG55(rolesForPG55);
			}

			foreach (var block in AEPAPGBlockHelper.MakeNHTSAContactBlocks(EntityRoleCodeList.Codes.FabricatingManufacturer, commonData.FabricatingManufacturerDetails))
			{
				yield return block;
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Importer, commonData.ImporterDetails, commonData.CertifyingIndividual == PartyTypeList.Codes.Importer ? commonData.ContactDetails : null))
			{
				yield return block;
			}
			if (commonData.CertifyingIndividual == PartyTypeList.Codes.Importer && commonData.ImporterDetails != null)
			{
				yield return AEPAPGBlockHelper.MakePG55(rolesForPG55);
			}

			foreach (var block in AEPAPGBlockHelper.MakeNHTSAContactBlocksWithConditionalPG21(EntityRoleCodeList.Codes.OriginalVehicleManufacturer, commonData.OriginalVehicleMFRDetails))
			{
				yield return block;
			}

			foreach (var block in AEPAPGBlockHelper.MakeNHTSAContactBlocksWithConditionalPG21(EntityRoleCodeList.Codes.RetailerDistributor, commonData.RetailerDetails))
			{
				yield return block;
			}

			var shouldPG34BeSent = !commonData.DocumentType.IsEmpty || !commonData.DocumentNationality.IsEmpty || !commonData.DocumentNumber.IsEmpty;
			if (shouldPG34BeSent)
			{
				yield return AEPAPGBlockHelper.MakePG34(commonData);
			}
			if (commonData.CertifySignatureDate.IsEmpty)
			{
				commonData.CertifySignatureDate = ZDate.Today;
			}

			foreach (var block in GenerateNHTSADocumentBlocks(commonData))
			{
				yield return block;
			}

			if (commonData.CertifyingIndividual == PartyTypeList.Codes.CustomsBroker)
			{
				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.CertifyingIndividual, commonData.ContactDetails))
				{
					yield return block;
				}
			}
		}

		#endregion

		#region Document Blocks

		IEnumerable<MessageBlock> GenerateNHTSADocumentBlocks(INHTSAHeader commonData)
		{
			foreach (var document in commonData.Documents)
			{
				yield return AEPAPGBlockHelper.MakePG22(document, commonData.DeclarationCertificate, commonData.CertifySignatureDate, commonData.BoxNumber);
			}
		}

		#endregion

		#endregion

		#endregion

		#region PGA CPSC block

		internal IEnumerable<MessageBlock> GetCPSCBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.CPSC))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					var pg01 = AEPAPGBlockHelper.MakePG01ForDisclaimer(1, ACEGovernmentAgenciesCodeList.Codes.CPS, "CPS", entryLine.CPSCDisclaimReason);

					if (entryLine.CPSCDisclaimReason == PGADisclaimReasonList.Codes.A && entryLine.CPSCHeaders.Any())
					{
						var cpscHeader = entryLine.CPSCHeaders.First();
						pg01.IntendedUseCode = cpscHeader.IntendedUseCode;
						pg01.IntendedUseDescription = cpscHeader.IntendedUseDescription;
					}
					else if (entryLine.CPSCDisclaimReason == PGADisclaimReasonList.Codes.B)
					{
						pg01.IntendedUseCode = IntendedUseCodesList.Codes.ConsumerProductIntendedForPeopleAged13YearsOrOlder;
					}

					return new[] { pg01 };
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.CPSCIndicator, entryLine.CPSCHeaders, null, pgaIncludedList, GetCPSCDataForOneLine, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetCPSCDataForOneLine(ICPSCHeader cpscHeader)
		{
			cPSCNumberCounter++;
			cpscHeader.LineNo = cPSCNumberCounter;
			yield return AEPAPGBlockHelper.MakePG01(cpscHeader, cPSCNumberCounter);

			yield return AEPAPGBlockHelper.MakePG02(cpscHeader);

			foreach (var pg07Block in GetCPSCPG07(cpscHeader))
			{
				yield return pg07Block;
			}

			foreach (var pg10Block in GetCPSCPG10(cpscHeader))
			{
				yield return pg10Block;
			}

			if (!cpscHeader.ReferenceNumber.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG14(ZString.Empty, cpscHeader.ReferenceNumber);
			}

			foreach (var contactBlock in GetCPSCContactBlocks(cpscHeader))
			{
				yield return contactBlock;
			}

			if (!cpscHeader.CertificateExists.IsEmpty)
			{
				yield return new AEPAPG22() { DeclarationCode = cpscHeader.CertificateExists };
			}

			foreach (var lot in cpscHeader.Lots)
			{
				yield return new AEPAPG25()
				{
					LotNumberQualifier = lot.NumberType,
					LotNumber = lot.Number,
					ProductionStartDateOfTheLot = lot.StartDate.Date,
					ProductionEndDateOfTheLot = lot.EndDate.Date
				};
			}
		}

		IEnumerable<MessageBlock> GetCPSCPG07(ICPSCHeader cpscHeader)
		{
			if (cpscHeader.SerialNumber.IsEmpty && cpscHeader.ModelNumber.IsEmpty && cpscHeader.RegisteredNumber.IsEmpty && cpscHeader.AltenateID.IsEmpty)
			{
				if (!cpscHeader.BrandName.IsEmpty || !cpscHeader.ProductName.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG07(cpscHeader, ZString.Empty, ZString.Empty);
				}
			}
			else
			{
				if (!cpscHeader.ModelNumber.IsEmpty)
				{
					foreach (var block in GetPG07Blocks(cpscHeader, ItemIdentityNumberQualifierList.Codes.ModelNumber, cpscHeader.ModelNumber))
					{
						yield return block;
					}
				}

				if (!cpscHeader.SerialNumber.IsEmpty)
				{
					foreach (var block in GetPG07Blocks(cpscHeader, ItemIdentityNumberQualifierList.Codes.SerialNumber, cpscHeader.SerialNumber))
					{
						yield return block;
					}
				}

				if (!cpscHeader.RegisteredNumber.IsEmpty)
				{
					foreach (var block in GetPG07Blocks(cpscHeader, ItemIdentityNumberQualifierList.Codes.RegisteredNumber, cpscHeader.RegisteredNumber))
					{
						yield return block;
					}
				}

				if (!cpscHeader.AltenateID.IsEmpty)
				{
					foreach (var block in GetPG07Blocks(cpscHeader, ItemIdentityNumberQualifierList.Codes.AlternateIdentifier, cpscHeader.AltenateID))
					{
						yield return block;
					}
				}
			}
		}

		IEnumerable<MessageBlock> GetPG07Blocks(ICPSCHeader cpscHeader, ZString numberType, ZString number)
		{
			var numbers = number.Split(',').Select(x => x.Trim()).Where(x => !x.IsEmpty);
			foreach (var num in numbers)
			{
				yield return AEPAPGBlockHelper.MakePG07(cpscHeader, numberType, num);
			}
		}

		IEnumerable<MessageBlock> GetCPSCPG10(ICPSCHeader cpscHeader)
		{
			if (!cpscHeader.ModelColor.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG10("PC9", cpscHeader.ModelColor, CPSCCommodityCharacteristicQualifiersList.Codes.ModelColor);
			}

			if (!cpscHeader.ModelDescription.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG10("PC9", cpscHeader.ModelDescription, CPSCCommodityCharacteristicQualifiersList.Codes.ModelDescription);
			}

			if (!cpscHeader.ModelStyle.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG10("PC9", cpscHeader.ModelStyle, CPSCCommodityCharacteristicQualifiersList.Codes.ModelStyle);
			}
		}

		IEnumerable<MessageBlock> GetCPSCContactBlocks(ICPSCHeader cpscHeader)
		{
			if (cpscHeader.Manufacturer != null)
			{
				foreach (var block in AEPAPGBlockHelper.MakeEntityBlocks(EntityRoleCodeList.Codes.ManufacturerOfGoods, cpscHeader.Manufacturer.CompanyAddress, cpscHeader.ManufacturerEntityIdentificationCode, cpscHeader.ManufacturerRegistryID, isCompanyNameRequiredAlways: true))
				{
					yield return block;
				}
			}

			if (cpscHeader.CertifyingEntity != null)
			{
				foreach (var block in AEPAPGBlockHelper.MakeEntityBlocks(EntityRoleCodeList.Codes.CertifyingEntity, cpscHeader.CertifyingEntity.CompanyAddress, isCompanyNameRequiredAlways: true))
				{
					yield return block;
				}
			}

			if (cpscHeader.ContactPoint != null)
			{
				foreach (var block in AEPAPGBlockHelper.MakeEntityBlocks(EntityRoleCodeList.Codes.PointOfContact, cpscHeader.ContactPoint.CompanyAddress, isCompanyNameRequiredAlways: true))
				{
					yield return block;
				}
			}

			if (cpscHeader.NoLabTestingRequired)
			{
				yield return AEPAPGBlockHelper.MakePG19(EntityRoleCodeList.Codes.NoLabTestingRequired);
				foreach (var block in GetCPSCPG60(cpscHeader.RuleCodes))
				{
					yield return block;
				}
			}

			foreach (var rulesAndLabs in cpscHeader.RulesAndLabs)
			{
				if (!rulesAndLabs.CPSCAccreditedLabID.IsEmpty)
				{
					yield return new AEPAPG30() { InspectionLaboratoryTestingStatus = InspectionStatusList.Codes.LabTestingPreviouslyPerformed, AnticipatedArrivalDate = rulesAndLabs.PreviousInspectionDate.Date };
					yield return AEPAPGBlockHelper.MakePG19WithNumber(EntityRoleCodeList.Codes.IndependentThirdPartyLaboratory, rulesAndLabs.CPSCAccreditedLabID);
				}
				else if (rulesAndLabs.SafetyTestLocation != null)
				{
					yield return new AEPAPG30() { InspectionLaboratoryTestingStatus = InspectionStatusList.Codes.LabTestingPreviouslyPerformed, AnticipatedArrivalDate = rulesAndLabs.PreviousInspectionDate.Date };
					foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithoutIndividualQualifierAndName(EntityRoleCodeList.Codes.Laboratory, rulesAndLabs.SafetyTestLocation))
					{
						yield return block;
					}
				}

				foreach (var pg60Block in GetCPSCPG60(rulesAndLabs))
				{
					yield return pg60Block;
				}
			}
		}

		IEnumerable<MessageBlock> GetCPSCPG60(ICPSCRulesAndLabs rulesAndLabs)
		{
			foreach (var block in GetCPSCPG60(rulesAndLabs.RuleCodes))
			{
				yield return block;
			}

			foreach (CPSCReport report in rulesAndLabs.ReportAndLabs)
			{
				yield return new AEPAPG60() { AdditionalInformationQualifierCode = report.US_RemarksType, AdditionalInformation = report.US_RemarksText };
			}
		}

		IEnumerable<MessageBlock> GetCPSCPG60(ZString ruleCodes)
		{
			if (!ruleCodes.IsEmpty)
			{
				var codeList = ruleCodes.Split(',').Select(x => x.Trim()).Where(x => !x.IsEmpty).ToList();
				foreach (var code in codeList)
				{
					yield return new AEPAPG60() { AdditionalInformationQualifierCode = AdditionalInformationQualifierList.Codes.CitationCode, AdditionalInformation = code };
				}
			}
		}

		#endregion

		#region PGA AMS blocks

		IEnumerable<MessageBlock> GetAMSBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.AMS))
			{
				Func<IEnumerable<MessageBlock>> getAMSDisclaimedBlock = () =>
				{
					var result = new List<MessageBlock>();
					var programCode = entryLine.AMSDisclaimProgram.SubstringSafe(0, 2);
					var processingCode = entryLine.AMSDisclaimProgram.SubstringSafe(2, 1);
					var pg01 = AEPAPGBlockHelper.MakePG01(++aMSLineNumberCounter, ACEGovernmentAgenciesCodeList.Codes.AMS, programCode, processingCode);
					pg01.Disclaimer = entryLine.AMSDisclaimReason;
					result.Add(pg01);
					return result;
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.AMSIndicator, entryLine.AMSLines, null, pgaIncludedList, GetAMSData, getAMSDisclaimedBlock, isPGACorrection))
				{
					yield return block;
				}

				Func<IEnumerable<MessageBlock>> getNOPDisclaimedBlock = () =>
				{
					return new[]
					{
						new AEPAPG01()
						{
							PGALineNumber = 1,
							GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.AMS,
							GovernmentAgencyProgramCode = AMSProgramList.Codes.OR1.Substring(0, 2),
							GovernmentAgencyProcessingCode = AMSProgramList.Codes.OR1.Substring(2, 1),
							Disclaimer = entryLine.NOPDisclaimReason
						}
					};
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.NOPIndicator, entryLine.AMSLines, null, pgaIncludedList, GetNOPData, getNOPDisclaimedBlock, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetAMSData(IAMSData commonData)
		{
			var programCode = commonData.Program;
			switch (programCode)
			{
				case AMSProgramList.Codes.MO1:
					foreach (MessageBlock block in GetAMSMO1Data(commonData))
					{
						yield return block;
					}
					break;
				case AMSProgramList.Codes.MO2:
					foreach (MessageBlock block in GetAMSMO2Data(commonData))
					{
						yield return block;
					}
					break;
				case AMSProgramList.Codes.MO3:
					foreach (MessageBlock block in GetAMSMO3Data(commonData))
					{
						yield return block;
					}
					break;
				case AMSProgramList.Codes.MO4:
					foreach (MessageBlock block in GetAMSMO4Data(commonData))
					{
						yield return block;
					}
					break;
				case AMSProgramList.Codes.MO5:
				case AMSProgramList.Codes.PN1:
					foreach (MessageBlock block in GetAMSMO5Data(commonData))
					{
						yield return block;
					}
					break;
				case AMSProgramList.Codes.MO6:
					foreach (MessageBlock block in GetAMSMO6Data(commonData))
					{
						yield return block;
					}
					break;
				case AMSProgramList.Codes.MO7:
					foreach (MessageBlock block in GetAMSMO7Data(commonData))
					{
						yield return block;
					}
					break;
				case AMSProgramList.Codes.MO8:
					foreach (MessageBlock block in GetAMSMO8Data(commonData))
					{
						yield return block;
					}
					break;
				case AMSProgramList.Codes.EG1:
					foreach (MessageBlock block in GetAMSEG1Data(commonData))
					{
						yield return block;
					}
					break;
				case AMSProgramList.Codes.EG2:
					foreach (MessageBlock block in GetAMSEG2Data(commonData))
					{
						yield return block;
					}
					break;
			}
		}

		IEnumerable<MessageBlock> GetNOPData(IAMSData commonData)
		{
			var programCode = commonData.Program;
			switch (programCode)
			{
				case AMSProgramList.Codes.OR1:
					foreach (MessageBlock block in GetAMSOR1Data(commonData))
					{
						yield return block;
					}
					break;
				case AMSProgramList.Codes.OR2:
					foreach (MessageBlock block in GetAMSOR2Data(commonData))
					{
						yield return block;
					}
					break;
			}
		}

		IEnumerable<MessageBlock> GetAMSMO1Data(IAMSData commonData)
		{
			foreach (IAMSLine mo1Data in commonData.AMSLinesDetails)
			{
				aMSLineNumberCounter++;
				commonData.LineNumber = aMSLineNumberCounter;
				yield return AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
				yield return AEPAPGBlockHelper.MakePG02(ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode, mo1Data.ProductNumber.Left(19));
				yield return AEPAPGBlockHelper.MakePG10(ZString.Empty, commonData.CommercialDescription, ZString.Empty);

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.AMSApplicant, EntityRoleCodeList.Codes.AMSApplicant, mo1Data.Applicant))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.LocationOfGoodsImmediatelyAfterEntryRelease, EntityRoleCodeList.Codes.GoodsCustodian, mo1Data.GoodsLocation))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Importer, EntityRoleCodeList.Codes.Importer, commonData.Importer))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.CustomsBroker, commonData.Broker))
				{
					yield return block;
				}

				if (!mo1Data.Packages.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG26(1, mo1Data.Packages, mo1Data.PackagesUQ);
				}

				if (!mo1Data.PackageWeight.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG26(2, mo1Data.PackageWeight, mo1Data.PackageWeightUQ);
				}

				if (!mo1Data.QtyPerPackage.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG26(2, mo1Data.QtyPerPackage, mo1Data.QtyPerPackageUQ);
				}

				foreach (var block in GetContainerDetailsBlocks(commonData.Containers))
				{
					yield return block;
				}

				if (!mo1Data.NetWeight.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG29(mo1Data.NetWeight, mo1Data.NetWeightUQ);
				}
				yield return AEPAPGBlockHelper.MakePG30(InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection, mo1Data.InspecDateTime, mo1Data.InspecRemarks);
				yield return AEPAPGBlockHelper.MakePG30(InspectionStatusList.Codes.BTAAnticipatedArrivalInformation, commonData.EstimatedDate, ZString.Empty);
			}
		}

		IEnumerable<MessageBlock> GetAMSMO2Data(IAMSData commonData)
		{
			foreach (IAMSLine mo2Data in commonData.AMSLinesDetails)
			{
				aMSLineNumberCounter++;
				commonData.LineNumber = aMSLineNumberCounter;

				var pg01 = AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
				pg01.ElectronicImageSubmitted = mo2Data.IsDocSubmitted ? "Y" : "";
				yield return pg01;

				yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
				yield return AEPAPGBlockHelper.MakePG10("", commonData.CommercialDescription, "");
				yield return AEPAPGBlockHelper.MakePG13("PR", mo2Data.InspectionLocation, mo2Data.Party);
				var pg14 = AEPAPGBlockHelper.MakePG14(LPCOTransactionTypeList.Codes.SingleUse, mo2Data.CertType, mo2Data.CertNumber, mo2Data.Weight, mo2Data.WeightUQ);
				pg14.LPCODate = mo2Data.IssueDate;
				yield return pg14;

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithoutOverflowingBlock(EntityRoleCodeList.Codes.Importer, EntityRoleCodeList.Codes.Importer, commonData.Importer))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithoutOverflowingBlock(EntityRoleCodeList.Codes.CustomsBroker, commonData.Broker))
				{
					yield return block;
				}

				if (!mo2Data.NetWeight.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG29(mo2Data.NetWeight, mo2Data.NetWeightUQ);
				}
				if (!commonData.EstimatedDate.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG30(InspectionStatusList.Codes.BTAAnticipatedArrivalInformation, commonData.EstimatedDate, ZString.Empty, onlyEstimatedDate: true);
				}
			}
		}

		IEnumerable<MessageBlock> GetAMSMO3Data(IAMSData commonData)
		{
			foreach (IAMSLine mo3Data in commonData.AMSLinesDetails)
			{
				aMSLineNumberCounter++;
				commonData.LineNumber = aMSLineNumberCounter;
				yield return AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
				yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
				yield return AEPAPGBlockHelper.MakePG10(ZString.Empty, commonData.CommercialDescription, ZString.Empty);
				yield return AEPAPGBlockHelper.MakePG14(LPCOTransactionTypeList.Codes.SingleUse, LPCOTypeList.Codes.AM2, mo3Data.AuthorizationNumber);

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithoutOverflowingBlock(EntityRoleCodeList.Codes.Importer, EntityRoleCodeList.Codes.Importer, commonData.Importer))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithoutOverflowingBlock(EntityRoleCodeList.Codes.CustomsBroker, commonData.Broker))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetAMSMO4Data(IAMSData commonData)
		{
			foreach (IAMSLine mo4Data in commonData.AMSLinesDetails)
			{
				aMSLineNumberCounter++;
				commonData.LineNumber = aMSLineNumberCounter;

				yield return AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
				yield return AEPAPGBlockHelper.MakePG02(ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode, mo4Data.ProductNumber);
				yield return AEPAPGBlockHelper.MakePG10(ZString.Empty, commonData.CommercialDescription, ZString.Empty);
				yield return AEPAPGBlockHelper.MakePG14(LPCOTransactionTypeList.Codes.SingleUse, LPCOTypeList.Codes.AM2, "", mo4Data.NetWeight, mo4Data.NetWeightUQ);
				if (commonData.Importer != null)
				{
					var block = AEPAPGBlockHelper.MakePG19WithoutOverflowingBlock(EntityRoleCodeList.Codes.Importer, commonData.Importer);
					if (block != null)
					{
						yield return block;
					}
				}

				if (commonData.Consignee != null)
				{
					var block = AEPAPGBlockHelper.MakePG19WithoutOverflowingBlock(EntityRoleCodeList.Codes.Consignee, commonData.Consignee);
					if (block != null)
					{
						yield return block;
					}
				}
			}
		}

		IEnumerable<MessageBlock> GetAMSMO5Data(IAMSData commonData)
		{
			foreach (IAMSLine mo5Data in commonData.AMSLinesDetails)
			{
				aMSLineNumberCounter++;
				commonData.LineNumber = aMSLineNumberCounter;

				yield return AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
				yield return AEPAPGBlockHelper.MakePG02(ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode, mo5Data.ProductNumber);
				yield return AEPAPGBlockHelper.MakePG10(ZString.Empty, commonData.CommercialDescription, ZString.Empty);

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithoutOverflowingBlock(EntityRoleCodeList.Codes.AMSApplicant, EntityRoleCodeList.Codes.AMSApplicant, mo5Data.Applicant))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithoutOverflowingBlock(EntityRoleCodeList.Codes.LocationOfGoodsImmediatelyAfterEntryRelease, EntityRoleCodeList.Codes.GoodsCustodian, mo5Data.GoodsLocation))
				{
					yield return block;
				}
				if (commonData.Importer != null)
				{
					yield return AEPAPGBlockHelper.MakePG21WithoutOverflowingBlock(EntityRoleCodeList.Codes.Importer, commonData.Importer);
				}

				if (commonData.Broker != null)
				{
					yield return AEPAPGBlockHelper.MakePG21WithoutOverflowingBlock(EntityRoleCodeList.Codes.CustomsBroker, commonData.Broker);
				}

				foreach (ILotCode element in mo5Data.AMSLineLotCodes)
				{
					yield return AEPAPGBlockHelper.MakePG25(element);
				}

				if (!mo5Data.Packages.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG26(1, mo5Data.Packages, mo5Data.PackagesUQ);
				}
				if (!mo5Data.PackageWeight.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG26(2, mo5Data.PackageWeight, mo5Data.PackageWeightUQ);
				}

				foreach (var block in GetContainerDetailsBlocks(commonData.Containers))
				{
					yield return block;
				}

				if (!mo5Data.NetWeight.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG29(mo5Data.NetWeight, mo5Data.NetWeightUQ);
				}
				yield return AEPAPGBlockHelper.MakePG30(InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection, mo5Data.InspecDateTime, mo5Data.InspecRemarks);
			}
		}

		IEnumerable<MessageBlock> GetAMSMO6Data(IAMSData commonData)
		{
			foreach (IAMSLine mo6Data in commonData.AMSLinesDetails)
			{
				aMSLineNumberCounter++;
				commonData.LineNumber = aMSLineNumberCounter;

				yield return AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
				yield return AEPAPGBlockHelper.MakePG02(ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode, mo6Data.ProductNumber);
				yield return AEPAPGBlockHelper.MakePG10(ZString.Empty, commonData.CommercialDescription, ZString.Empty);
				if (!mo6Data.NetWeight.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG29(mo6Data.NetWeight, mo6Data.NetWeightUQ);
				}
			}
		}

		IEnumerable<MessageBlock> GetAMSMO7Data(IAMSData commonData)
		{
			aMSLineNumberCounter++;
			commonData.LineNumber = aMSLineNumberCounter;
			yield return AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
			yield return AEPAPGBlockHelper.MakePG30(InspectionStatusList.Codes.BTAAnticipatedArrivalInformation, commonData.EstimatedDate, ZString.Empty);
		}

		IEnumerable<MessageBlock> GetAMSMO8Data(IAMSData commonData)
		{
			aMSLineNumberCounter++;
			commonData.LineNumber = aMSLineNumberCounter;
			yield return AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
			yield return AEPAPGBlockHelper.MakePG29(commonData.NetWeight, commonData.NetWeightUQ);
		}

		IEnumerable<MessageBlock> GetAMSEG1Data(IAMSData commonData)
		{
			foreach (IAMSLine eg1Data in commonData.AMSLinesDetails)
			{
				aMSLineNumberCounter++;
				commonData.LineNumber = aMSLineNumberCounter;

				var pg01 = AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
				pg01.ElectronicImageSubmitted = eg1Data.IsDocSubmitted ? "Y" : "";
				yield return pg01;

				yield return AEPAPGBlockHelper.MakePG02(ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode, eg1Data.ProductNumber);
				yield return AEPAPGBlockHelper.MakePG10(ZString.Empty, commonData.CommercialDescription, ZString.Empty);

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithoutOverflowingBlock(EntityRoleCodeList.Codes.Applicant, EntityRoleCodeList.Codes.AMSApplicant, eg1Data.Applicant))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithoutOverflowingBlock(EntityRoleCodeList.Codes.LocationOfGoodsImmediatelyAfterEntryRelease, EntityRoleCodeList.Codes.GoodsCustodian, eg1Data.GoodsLocation))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksWithoutOverflowingBlock(EntityRoleCodeList.Codes.Importer, EntityRoleCodeList.Codes.Importer, commonData.Importer))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakePG26(eg1Data))
				{
					yield return block;
				}

				foreach (var block in GetContainerDetailsBlocks(commonData.Containers))
				{
					yield return block;
				}

				if (!eg1Data.TotalQuantity.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG29(eg1Data.TotalQuantity, eg1Data.TotalQuantityUQ);
				}
				else if (!eg1Data.TotalWeight.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG29(eg1Data.TotalWeight, eg1Data.TotalWeightUQ);
				}
				yield return AEPAPGBlockHelper.MakePG30(InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection, eg1Data.InspecDateTime, eg1Data.InspecRemarks);
			}
		}

		IEnumerable<MessageBlock> GetAMSEG2Data(IAMSData commonData)
		{
			foreach (IAMSLine eg2Data in commonData.AMSLinesDetails)
			{
				aMSLineNumberCounter++;
				commonData.LineNumber = aMSLineNumberCounter;

				var pg01 = AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
				pg01.ElectronicImageSubmitted = eg2Data.IsDocSubmitted ? "Y" : "";
				yield return pg01;

				yield return AEPAPGBlockHelper.MakePG02(ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode, eg2Data.ProductNumber);
				yield return AEPAPGBlockHelper.MakePG10(ZString.Empty, commonData.CommercialDescription, ZString.Empty);
				yield return AEPAPGBlockHelper.MakePG14(LPCOTransactionTypeList.Codes.SingleUse, LPCOTypeList.Codes.AM3, eg2Data.PermitNumber);
			}
		}

		IEnumerable<MessageBlock> GetAMSOR1Data(IAMSData commonData)
		{
			aMSLineNumberCounter++;
			commonData.LineNumber = aMSLineNumberCounter;
			var amsLines = commonData.AMSLinesDetails;

			var pg01 = AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
			pg01.ElectronicImageSubmitted = commonData.IsElecImageSubmitted ? "Y" : "";
			yield return pg01;

			var pg02 = AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
			yield return pg02;

			foreach (IAMSLine or1Data in amsLines)
			{
				yield return AEPAPGBlockHelper.MakePG10(ZString.Empty, or1Data.ProductLabel, ZString.Empty);
			}

			var pg14 = AEPAPGBlockHelper.MakePG14(LPCOTransactionTypeList.Codes.SingleUse, commonData.CerType, commonData.CerNumber);
			pg14.LPCODateQualifier = commonData.DateType;
			pg14.LPCODate = new ZDate(commonData.Date);
			yield return pg14;

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocksForOR1(EntityRoleCodeList.Codes.Exporter, ZString.Empty, commonData.Exporter))
			{
				yield return block;
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocksForOR1(EntityRoleCodeList.Codes.CertifyingBodyIssuingCertificate, ZString.Empty, commonData.CertifyingBody))
			{
				yield return block;
			}

			foreach (var block in AEPAPGBlockHelper.MakeContactBlocksForOR1(EntityRoleCodeList.Codes.UltimateConsignee, ZString.Empty, commonData.UltimateConsignee))
			{
				yield return block;
			}

			foreach (IAMSLine or1Data in amsLines)
			{
				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksForOR1(EntityRoleCodeList.Codes.CertifyingBodyOfFinalHandler, ZString.Empty, or1Data.CertifyingFinalHandler))
				{
					yield return block;
				}

				foreach (var block in AEPAPGBlockHelper.MakeContactBlocksForOR1(EntityRoleCodeList.Codes.CertifiedOrganicPacker, ZString.Empty, or1Data.FinalHandler))
				{
					yield return block;
				}
			}

			yield return AEPAPGBlockHelper.MakePG22WithDocId(DocumentIdentifierList.Codes.Certificate, DeclarationCodeList.Codes.AM4, YesNoDefaultList.Codes.Yes, ZDateTime.Empty);
			if (commonData.USDAOrganicStandard)
			{
				yield return new AEPAPG24() { RemarksTypeCode = RemarksTypeCodeList.Codes.AM1, RemarksCode = RemarksCodeList.Codes.A10, RemarksText = commonData.RemarkText };
			}
			if (commonData.EquivalentOrganicStandard)
			{
				yield return new AEPAPG24() { RemarksTypeCode = RemarksTypeCodeList.Codes.AM1, RemarksCode = RemarksCodeList.Codes.A11, RemarksText = (commonData.USDAOrganicStandard ? ZString.Empty : commonData.RemarkText) };
			}

			foreach (IAMSLine or1Data in amsLines)
			{
				if (!or1Data.LotNumberQualifier.IsEmpty || !or1Data.LotNumber.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG25(or1Data.LotNumberQualifier, or1Data.LotNumber);
				}
			}

			if (commonData.InvoiceLine.IsContainerisedMode)
			{
				var containers = commonData.Containers;

				foreach (var block in GetContainerDetailsBlocks(containers))
				{
					yield return block;
				}
			}

			yield return AEPAPGBlockHelper.MakePG29(commonData.NetWeight, commonData.NetWeightUQ);
		}

		IEnumerable<MessageBlock> GetAMSOR2Data(IAMSData commonData)
		{
			aMSLineNumberCounter++;
			commonData.LineNumber = aMSLineNumberCounter;

			var pg01 = AEPAPGBlockHelper.MakePG01(commonData, commonData.LineNumber);
			pg01.ElectronicImageSubmitted = commonData.IsElecImageSubmitted ? "Y" : "";
			yield return pg01;

			var pg02 = AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);
			yield return pg02;

			foreach (var certificateData in commonData.AMSLinesDetails)
			{
				yield return AEPAPGBlockHelper.MakePG14(certificateData.CertType, AMSCertTypeList.Codes.AM1, certificateData.CertNumber);
			}

			foreach (var lotCode in commonData.AMSLotCodes)
			{
				yield return AEPAPGBlockHelper.MakePG25(lotCode.Code, lotCode.Value);
			}

			yield return AEPAPGBlockHelper.MakePG29(commonData.NetWeight, commonData.NetWeightUQ);
		}

		#endregion

		#region FWS blocks

		IEnumerable<MessageBlock> GetFWSBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.FWS))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[]
					{
						new AEPAPG01()
						{
							PGALineNumber = 1,
							GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.FWS,
							GovernmentAgencyProgramCode = ACEGovernmentAgenciesCodeList.Codes.FWS,
							Disclaimer = entryLine.FWSDisclaimReason
						}
					};
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.FWSIndicator, entryLine.FWSHeaders, null, pgaIncludedList, GetFWSData, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetFWSData(IFWSHeader fwsHeader)
		{
			var isLDS = FWSProcessingCodeList.IsLDS(fwsHeader.ProcessingCode);
			fwsHeader.LineNo = ++fWSNumberCounter;
			var pg01 = AEPAPGBlockHelper.MakePG01(fwsHeader.LineNo, ACEGovernmentAgenciesCodeList.Codes.FWS, ACEGovernmentAgenciesCodeList.Codes.FWS, fwsHeader.ProcessingCode, fwsHeader.IsDocSubmitted, false);
			pg01.GloballyUniqueProductIdentificationCodeQualifier = fwsHeader.ProductType;
			pg01.GloballyUniqueProductIdentificationCode = fwsHeader.ProductNumber;
			pg01.IntendedUseCode = fwsHeader.IntendedUseCode;
			yield return pg01;

			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);

			if (!isLDS)
			{
				foreach (var block in GetFWSScientificDetails(fwsHeader))
				{
					yield return block;
				}

				var sourceCountryCode = fwsHeader.SourceCountryCode;

				if (!sourceCountryCode.IsEmpty)
				{
					yield return AEPAPGBlockHelper.MakePG06(SourceTypeCodesList.Codes.CountryOfSpeciesOrigin, sourceCountryCode);
				}

				var commodityQualifierCode = fwsHeader.CommodityQualifierCode;
				if (!commodityQualifierCode.IsEmpty)
				{
					yield return new AEPAPG10() { CommodityQualifierCode = commodityQualifierCode };
				}
			}

			foreach (var block in GetFWSLicenceDetailsBlocks(fwsHeader.Licenses))
			{
				yield return block;
			}

			if (!isLDS)
			{
				var commodityGeneralName = fwsHeader.CommodityGeneralName;
				var commoditySpecificName = fwsHeader.CommoditySpecificName;
				var cartonQty = fwsHeader.CartonQty;
				if (!commodityGeneralName.IsEmpty || !commoditySpecificName.IsEmpty || !cartonQty.IsEmpty)
				{
					yield return new AEPAPG17()
					{
						CommonNameSpecific = commoditySpecificName,
						CommonNameGeneral = commodityGeneralName,
						LiveVenomousWildlifeCode = fwsHeader.IsLiveVenomous,
						CartonsContainingWildlife = cartonQty
					};
				}

				if (fwsHeader.CertifySignatureDate.IsEmpty)
				{
					fwsHeader.CertifySignatureDate = ZDate.Today;
				}

				foreach (var block in GetFWS19_22PartyDetailsBlocks(fwsHeader))
				{
					yield return block;
				}

				var remarksTexts = fwsHeader.RemarksText.Split(68);
				if (remarksTexts.Length > 0)
				{
					foreach (var remarksText in remarksTexts)
					{
						yield return new AEPAPG24() { RemarksTypeCode = RemarksTypeCodeList.Codes.GEN, RemarksText = remarksText };
					}
				}

				var pgaLineValue = fwsHeader.PGALineValue;
				if (!pgaLineValue.IsEmpty)
				{
					yield return new AEPAPG25() { PGALineValue = pgaLineValue };
				}

				foreach (var block in GetFWSContainerDetailsBlocks(fwsHeader.ContainerNumbers))
				{
					yield return block;
				}

				var netCommodityQty = fwsHeader.NetCommodityQty;
				if (!netCommodityQty.IsEmpty)
				{
					yield return new AEPAPG29() { UnitOfMeasurePGALineNet = fwsHeader.NetCommodityUQ, CommodityNetQuantityPGALineNet = netCommodityQty };
				}

				var firms = fwsHeader.FIRMS;
				if (!firms.IsEmpty || !fwsHeader.ArrivalDate.IsEmpty)
				{
					yield return new AEPAPG30() { InspectionLaboratoryTestingStatus = InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection, AnticipatedArrivalLocationCode = InspectionLocationCodeList.Codes.FIRMS, ArrivalLocation = firms, AnticipatedArrivalDate = fwsHeader.ArrivalDate };
				}
			}
		}

		IEnumerable<MessageBlock> GetFWSScientificDetails(IFWSHeader fwsHeader)
		{
			var scientificGenusName = fwsHeader.ScientificGenusName;
			var scientificSpeciesName = fwsHeader.ScientificSpeciesName;
			var scientificSubSpeciesName = fwsHeader.ScientificSubSpeciesName;
			var scientificSpeciesCode = fwsHeader.ScientificSpeciesCode;
			var fwsDescriptionCode = fwsHeader.FWSDescriptionCode + fwsHeader.Hybrid;
			if (!scientificGenusName.IsEmpty || !scientificSpeciesName.IsEmpty || !scientificSubSpeciesName.IsEmpty)
			{
				var pg05 = AEPAPGBlockHelper.MakePG05(scientificGenusName, scientificSpeciesName, scientificSubSpeciesName);
				pg05.ScientificSpeciesCode = scientificSpeciesCode;
				pg05.FWSDescriptionCode = fwsDescriptionCode;
				yield return pg05;
			}

			var scientific2GenusName = fwsHeader.Scientific2GenusName;
			var scientific2SpeciesName = fwsHeader.Scientific2SpeciesName;
			var scientific2SubSpeciesName = fwsHeader.Scientific2SubSpeciesName;
			if (!scientific2GenusName.IsEmpty || !scientific2SpeciesName.IsEmpty || !scientific2SubSpeciesName.IsEmpty)
			{
				var pg05 = AEPAPGBlockHelper.MakePG05(scientific2GenusName, scientific2SpeciesName, scientific2SubSpeciesName);
				pg05.ScientificSpeciesCode = scientificSpeciesCode;
				pg05.FWSDescriptionCode = fwsDescriptionCode;
				yield return pg05;
			}
		}

		IEnumerable<MessageBlock> GetFWSContainerDetailsBlocks(IEnumerable<ZString> containerNumbers)
		{
			var count = 0;
			AEPAPG27 pg27 = null;
			foreach (var containerNumber in containerNumbers)
			{
				switch (count++)
				{
					case 0:
						pg27 = new AEPAPG27() { ContainerNumberEquipmentID = containerNumber };
						break;
					case 1:
						pg27.ContainerNumberEquipmentID1 = containerNumber;
						break;
					case 2:
						pg27.ContainerNumberEquipmentID2 = containerNumber;
						yield return pg27;
						pg27 = null;
						count = 0;
						break;
				}
			}
			if (pg27 != null)
			{
				yield return pg27;
			}
		}

		IEnumerable<MessageBlock> GetFWSLicenceDetailsBlocks(IEnumerable<IFWSLicense> licences)
		{
			foreach (var licence in licences)
			{
				yield return new AEPAPG14()
				{
					LPCOType = licence.Type,
					LPCONumberorName = licence.Number
				};
			}
		}

		IEnumerable<MessageBlock> GetFWS19_22PartyDetailsBlocks(IFWSHeader fwsHeader)
		{
			var brokerIdType = ZString.Empty;
			var importerIdType = ZString.Empty;
			if (fwsHeader.FWSImporterFWE.IsEmpty)
			{
				brokerIdType = EntityIdentificationCodesList.Codes.FWSAssigned;
			}
			else
			{
				importerIdType = EntityIdentificationCodesList.Codes.FWSAssigned;
			}

			foreach (var block in AEPAPGBlockHelper.FWSMakeContactBlocks(EntityRoleCodeList.Codes.FWSForeignExporter, fwsHeader.FWSForeignExporter, ZString.Empty, ZString.Empty, fwsHeader.DeclarationCode, fwsHeader.CertifySignatureDate))
			{
				yield return block;
			}

			var brokerDetails = fwsHeader.BrokerDetails;
			if (brokerDetails != null)
			{
				foreach (var block in AEPAPGBlockHelper.FWSMakeContactBlocks(EntityRoleCodeList.Codes.CustomsBroker, brokerDetails, brokerIdType, brokerIdType.IsEmpty ? ZString.Empty : fwsHeader.FilerAccountNumber, fwsHeader.DeclarationCode, fwsHeader.CertifySignatureDate))
				{
					yield return block;
				}
			}

			foreach (var block in AEPAPGBlockHelper.FWSMakeContactBlocks(EntityRoleCodeList.Codes.FWSImporter, fwsHeader.FWSImporter, importerIdType, fwsHeader.FWSImporterFWE, fwsHeader.DeclarationCode, fwsHeader.CertifySignatureDate))
			{
				yield return block;
			}

			yield return AEPAPGBlockHelper.MakePG22FWS(fwsHeader.DeclarationCode, fwsHeader.CertifySignatureDate);
		}

		#endregion

		#region ACE Lacey Act blocks

		IEnumerable<MessageBlock> GetACE_LaceyActBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.Lacey))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[]
					{
						AEPAPGBlockHelper.MakePG01ForDisclaimer(++aPHLineNumberCounter, ACEGovernmentAgenciesCodeList.Codes.APH, GovernmentAgencyProgramCode, entryLine.LaceyActDisclaimReason)
					};
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.LaceyActIndicator, entryLine.LaceyActData, GetAllAPHISLines(entryLine), pgaIncludedList, GetACE_LaceyActCommonData, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}
		const string GovernmentAgencyProgramCode = "APL";

		IEnumerable<MessageBlock> GetACE_LaceyActCommonData(ILaceyActCommon laceyActCommon)
		{
			laceyActCommon.PGALineItemNumber = ++aPHLineNumberCounter;
			yield return new AEPAPG01() { PGALineNumber = aPHLineNumberCounter, GovernmentAgencyCode = ACEGovernmentAgenciesCodeList.Codes.APH, GovernmentAgencyProgramCode = GovernmentAgencyProgramCode };
			yield return new AEPAPG02() { ItemType = PG02ItemTypeList.Codes.Product };
			yield return new AEPAPG10() { CommodityCharacteristicDescription = laceyActCommon.CommercialDescription };

			if (!laceyActCommon.UnknownBreakdownTotal)
			{
				foreach (MessageBlock block in GetLaceyActConstituentData(laceyActCommon))
				{
					yield return block;
				}
			}

			if (laceyActCommon.CertifySignatureDate.IsEmpty)
			{
				laceyActCommon.CertifySignatureDate = ZDate.Today;
			}

			if (laceyActCommon.CertifyingIndividual == PartyTypeList.Codes.Importer)
			{
				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.Importer, laceyActCommon.ImporterContactDetails, laceyActCommon.ContactDetails))
				{
					yield return block;
				}
				yield return AEPAPGBlockHelper.MakePG22WithDocId(ZString.Empty, LaceyActDeclarationCode, laceyActCommon.DeclarationCertificate, laceyActCommon.CertifySignatureDate, EntityRoleCodeList.Codes.Importer);
			}
			else if (laceyActCommon.CertifyingIndividual == PartyTypeList.Codes.CustomsBroker)
			{
				foreach (var block in AEPAPGBlockHelper.MakeContactBlocks(EntityRoleCodeList.Codes.CustomsBroker, laceyActCommon.ContactDetails))
				{
					yield return block;
				}
				yield return AEPAPGBlockHelper.MakePG22WithDocId(ZString.Empty, LaceyActDeclarationCode, laceyActCommon.DeclarationCertificate, laceyActCommon.CertifySignatureDate, EntityRoleCodeList.Codes.CustomsBroker);
			}

			yield return new AEPAPG25() { PGALineValue = laceyActCommon.PGALineValue };

			if (laceyActCommon.UnknownBreakdownTotal)
			{
				foreach (MessageBlock block in GetLaceyActConstituentDataInGroup(laceyActCommon))
				{
					yield return block;
				}
			}

			foreach (MessageBlock block in GetContainersBlocks(laceyActCommon, true))
			{
				yield return block;
			}
		}
		const string LaceyActDeclarationCode = "AP6";

		IEnumerable<MessageBlock> GetLaceyActConstituentData(ILaceyActCommon commonData)
		{
			foreach (IConstituentElement constElement in commonData.ConstituentElements)
			{
				yield return new AEPAPG04()
				{
					NameOfTheConstituentElement = constElement.Name.Left(51),
					QuantityOfConstituentElement = constElement.Quantity,
					UnitOfMeasureConstituentElement = constElement.UnitOfMeasure,
					PercentOfConstituentElement = constElement.Percent,
					ConstituentActiveIngredientQualifier = commonData.UnknownBreakdown ? "Y" : ""
				};
				yield return new PGAPG05() { ScientificGenusName = constElement.GenusName, ScientificSpeciesName = constElement.SpeciesName };

				if (!constElement.CountryCode.IsEmpty)
				{
					yield return new AEPAPG06() { SourceTypeCode = LaceyActSourceTypeCode, CountryCode = constElement.CountryCode };
				}
			}
		}

		IEnumerable<MessageBlock> GetLaceyActConstituentDataInGroup(ILaceyActCommon commonData)
		{
			yield return new AEPAPG04() { NameOfTheConstituentElement = commonData.NameOfConstituent };
			yield return new AEPAPG50();

			foreach (var constElement in commonData.ConstituentElements)
			{
				yield return new PGAPG05() { ScientificGenusName = constElement.GenusName, ScientificSpeciesName = constElement.SpeciesName };
			}

			foreach (var country in commonData.CountryCodes)
			{
				yield return new AEPAPG06() { SourceTypeCode = LaceyActSourceTypeCode, CountryCode = country.CountryCode };
			}

			yield return AEPAPGBlockHelper.MakePG29(commonData.QuantityOfConstituent, commonData.UnitOfMeasure);
			yield return new AEPAPG51();
		}
		const string LaceyActSourceTypeCode = "HRV";

		#endregion

		#region PGA DEA block

		internal IEnumerable<MessageBlock> GetDEABlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.DEA))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[] { AEPAPGBlockHelper.MakePG01ForDisclaimer(1, ACEGovernmentAgenciesCodeList.Codes.DEA, "DEA", entryLine.DEADisclaimReason) };
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.DEAIndicator, entryLine.DEAHeaders, null, pgaIncludedList, GetDEADataForOneLine, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetDEADataForOneLine(IDEAHeader deaHeader)
		{
			dEANumberCounter++;
			deaHeader.LineNo = dEANumberCounter;

			yield return AEPAPGBlockHelper.MakePG01(dEANumberCounter);
			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);

			foreach (IDEAConstituent constituent in deaHeader.Constituents)
			{
				yield return AEPAPGBlockHelper.MakePG02(constituent);
				yield return AEPAPGBlockHelper.MakePG04(constituent);
			}

			yield return AEPAPGBlockHelper.MakePG06(SourceTypeCodesList.Codes.CountryOfShipment, deaHeader.CountryOfShipment);
			yield return AEPAPGBlockHelper.MakePG14(deaHeader);
			yield return AEPAPGBlockHelper.MakePG19(deaHeader);
			yield return AEPAPGBlockHelper.MakePG22(deaHeader);
			yield return AEPAPGBlockHelper.MakePG30(deaHeader);
		}

		#endregion

		#region PGA HFC block

		internal IEnumerable<MessageBlock> GetHFCBlocks(IGovernmentAgencies entryLine, List<Tuple<IPGADataCorrection, bool>> pgaIncludedList, ZBool isCertified, bool isPGACorrection, bool buildAllPGAs)
		{
			if (ShouldSendPGA(entryLine, isCertified, isPGACorrection, buildAllPGAs, GovernmentAgencyProgramCodeList.Codes.HFC))
			{
				Func<IEnumerable<MessageBlock>> getDisclaimedBlocks = () =>
				{
					return new[] { AEPAPGBlockHelper.MakePG01ForDisclaimer(1, ACEGovernmentAgenciesCodeList.Codes.EPA, GovernmentAgencyProgramCodeList.Codes.HFC, entryLine.HFCDisclaimReason) };
				};

				foreach (MessageBlock block in GenerateBlocks(entryLine.HFCIndicator, entryLine.EPA_HFCHeaders, null, pgaIncludedList, GetHFCDataForOneLine, getDisclaimedBlocks, isPGACorrection))
				{
					yield return block;
				}
			}
		}

		IEnumerable<MessageBlock> GetHFCDataForOneLine(IHFCHeader hfcHeader)
		{
			hfcHeader.LineNo = ++ePALineNumberCounter;

			yield return AEPAPGBlockHelper.MakePG01(ePALineNumberCounter, ACEGovernmentAgenciesCodeList.Codes.EPA, GovernmentAgencyProgramCodeList.Codes.HFC, ZString.Empty, hfcHeader.ElectronicImageSubmitted, false);
			yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Product);

			if (!hfcHeader.ASHRAENumber.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG07(ItemIdentityNumberQualifierList.Codes.ASHRAENumber, hfcHeader.ASHRAENumber);
			}

			if (hfcHeader.Importer is IPGAContactDetails importer)
			{
				yield return AEPAPGBlockHelper.MakePG19WithoutOverflowingBlock(EntityRoleCodeList.Codes.Importer, ZString.Empty, ZString.Empty, importer.CompanyAddress.CompanyName, importer.CompanyAddress.AddressLine1);
				yield return AEPAPGBlockHelper.MakePG20WithoutOverflowingBlock(importer.CompanyAddress);
				yield return AEPAPGBlockHelper.MakePG21WithoutOverflowingBlock(EntityRoleCodeList.Codes.Importer, importer);
				if (hfcHeader.CertifyingIndividual == EntityRoleCodeList.Codes.Importer)
				{
					yield return AEPAPGBlockHelper.MakePG55(new List<ZString>() { EntityRoleCodeList.Codes.CertifyingIndividual });
				}
			}

			if (hfcHeader.Consignee is IPGAContactDetails consignee)
			{
				yield return AEPAPGBlockHelper.MakePG19WithoutOverflowingBlock(EntityRoleCodeList.Codes.Consignee, ZString.Empty, ZString.Empty, consignee.CompanyAddress.CompanyName, ZString.Empty);
				yield return AEPAPGBlockHelper.MakePG21BlockWithoutOverflowingBlock(EntityRoleCodeList.Codes.Consignee, consignee.Name, ZString.Empty, ZString.Empty, ZString.Empty);
				if (hfcHeader.CertifyingIndividual == EntityRoleCodeList.Codes.Consignee)
				{
					yield return AEPAPGBlockHelper.MakePG55(new List<ZString>() { EntityRoleCodeList.Codes.CertifyingIndividual });
				}
			}

			if (hfcHeader.CertifyingIndividual == EntityRoleCodeList.Codes.CustomsBroker && hfcHeader.CustomsBroker is ICustomsBrokerDetails customsBroker)
			{
				yield return AEPAPGBlockHelper.MakePG19WithoutOverflowingBlock(EntityRoleCodeList.Codes.CertifyingIndividual, customsBroker);
				yield return AEPAPGBlockHelper.MakePG21WithoutOverflowingBlock(EntityRoleCodeList.Codes.CertifyingIndividual, customsBroker);
			}

			yield return AEPAPGBlockHelper.MakePG22(EntityRoleCodeList.Codes.CertifyingIndividual, DeclarationCodeList.Codes.EP4, "Y");

			foreach (var block in GetContainerDetailsBlocks(hfcHeader.CusContainers))
			{
				yield return block;
			}

			if (!hfcHeader.NetWeight.IsEmpty)
			{
				yield return AEPAPGBlockHelper.MakePG29(hfcHeader.NetWeight, Core.Constants.Weight.Kilograms);
			}

			if (hfcHeader.ASHRAENumber.IsEmpty)
			{
				foreach (IHFCDetail detail in hfcHeader.HFCDetails)
				{
					yield return AEPAPGBlockHelper.MakePG02(PG02ItemTypeList.Codes.Component, ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber, detail.ProductCode);
					yield return AEPAPGBlockHelper.MakePG04("Y", detail.NameOfActiveIngredient, detail.ActiveIngredientPercentage);
				}
			}
		}

		#endregion

		#region PGA Lines Per PGA

		IEnumerable<IPGADataCorrection> GetAllEPALines(IGovernmentAgencies line)
		{
			return allEPALines ?? (allEPALines = GetPGALinesPerPGA(line, PGALinesForPGACorrection.GetAllEPALines));
		}
		IEnumerable<IPGADataCorrection> allEPALines;

		IEnumerable<IPGADataCorrection> GetAllAPHISLines(IGovernmentAgencies line)
		{
			return allAPHISLines ?? (allAPHISLines = GetPGALinesPerPGA(line, PGALinesForPGACorrection.GetAllAPHISLines));
		}
		IEnumerable<IPGADataCorrection> allAPHISLines;

		IEnumerable<IPGADataCorrection> GetAllNMFSLines(IGovernmentAgencies line)
		{
			return allNMFSLines ?? (allNMFSLines = GetPGALinesPerPGA(line, PGALinesForPGACorrection.GetAllNMFSLines));
		}
		IEnumerable<IPGADataCorrection> allNMFSLines;

		IEnumerable<IPGADataCorrection> GetPGALinesPerPGA(IGovernmentAgencies line, Func<IPGAGovernmentAgenciesCommon, IEnumerable<IPGADataCorrection>> getAllLines)
		{
			IEnumerable<IPGADataCorrection> result = Array.Empty<IPGADataCorrection>();

			var pgaCorrectionLine = line as IPGAGovernmentAgenciesCommon;
			if (pgaCorrectionLine != null)
			{
				result = getAllLines(pgaCorrectionLine);
			}

			return result;
		}

		#endregion
	}
}
