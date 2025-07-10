using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDFDADataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDFDADataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_FDADisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_FDAIndicatorInfo; }
		}

		public override ZString PGAName
		{
			get { return "ACE FDA"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = pg01.GovernmentAgencyProgramCode;
			fda.US_ProcessingCode = pg01.GovernmentAgencyProcessingCode;
			fda.US_IntendedUseCode = pg01.IntendedUseCode;
			fda.US_IntendedUseDescr = pg01.IntendedUseDescription;
			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.Lots.RemoveAndDeleteAll();
			fda.Licenses.RemoveAndDeleteAll();

			var hasPreviousScientific = false;
			FDALicense previousLicense = null;
			ZString previousPG19CustomsNoType = ZString.Empty;
			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
			var quantityUQsList = new List<Tuple<ZDecimal, ZString>>();

			foreach (var block in pgaBlocks)
			{
				var pg02Block = block as AEPAPG02;
				if (pg02Block != null)
				{
					fda.US_ProductCode = pg02Block.ProductCodeNumber;
				}
				else
				{
					var pg06Block = block as AEPAPG06;
					if (pg06Block != null)
					{
						switch (pg06Block.SourceTypeCode)
						{
							case SourceTypeCodesList.Codes.CountryOfSource:
								if (!hasPreviousScientific)
								{
									fda.US_SourceCountry = pg06Block.CountryCode;
								}
								break;
							case SourceTypeCodesList.Codes.CountryOfProduction:
							case SourceTypeCodesList.Codes.PlaceOfGrowth:
								fda.US_ProdCountry = pg06Block.CountryCode;
								DefaultProducerType(fda, pg06Block.SourceTypeCode);
								break;
							case SourceTypeCodesList.Codes.CountryOfShipment:
								fda.US_ShipmentCountry = pg06Block.CountryCode;
								break;
							case SourceTypeCodesList.Codes.CountryOfRefusal:
								fda.US_RefusedCountry = pg06Block.CountryCode;
								break;
						}
					}
					else
					{
						var pg05Block = block as AEPAPG05;
						if (pg05Block != null)
						{
							hasPreviousScientific = true;
						}
						else
						{
							var pg07Block = block as AEPAPG07;
							if (pg07Block != null)
							{
								fda.US_BrandName = pg07Block.TradeNameBrandName;
								fda.US_ItemIdentityNumber = pg07Block.ItemIdentityNumber;
								fda.US_ItemIdentityNumberQualifier = pg07Block.ItemIdentityNumberQualifier;
							}
							else
							{
								var pg10Block = block as AEPAPG10;
								if (pg10Block != null)
								{
									fda.US_Description = pg10Block.CommodityCharacteristicDescription;
								}
								else
								{
									var pg04Block = block as AEPAPG04;
									if (pg04Block != null)
									{
										var constituent = fda.ProductConstituentElements.AddNew();
										constituent.US_PGANameOfTheConstituentElement = pg04Block.NameOfTheConstituentElement;
										constituent.US_PGAQuantityOfConstituentElement = pg04Block.QuantityOfConstituentElement;
										constituent.US_PGAUnitOfMeasure = pg04Block.UnitOfMeasureConstituentElement;
										constituent.US_PGAPercentOfConstituentElement = pg04Block.PercentOfConstituentElement;
									}
									else
									{
										var pg13Block = block as AEPAPG13;
										if (pg13Block != null)
										{
											var license = fda.Licenses.AddNew();
											license.US_CountryCode = pg13Block.LPCOIssuerGovernmentGeographicCodeQualifier;
											license.US_StateCode = pg13Block.LocationCountryStateProvinceOfIssuerOfTheLPCO;
											license.US_StateDescription = pg13Block.RegionalDescriptionOfLocationOfAgencyIssuingTheLPCO;
											previousLicense = license;
										}
										else
										{
											var pg14Block = block as AEPAPG14;
											if (pg14Block != null)
											{
												switch (pg14Block.LPCOType)
												{
													case "POV":
														if (previousLicense != null)
														{
															var matchedLicense = fda.Licenses.OfType<FDALicense>().FirstOrDefault(x => x == previousLicense);
															if (matchedLicense != null)
															{
																matchedLicense.US_Number = pg14Block.LPCONumberorName;
																previousLicense = null;
															}
														}
														break;
													case "PNC":
														fda.US_PNC = pg14Block.LPCONumberorName;
														break;
												}
											}
											else
											{
												var pg19Block = block as AEPAPG19;
												if (pg19Block != null)
												{
													DefaultProducerType(fda, pg19Block);

													if (pg19Block.EntityRoleCode != EntityRoleCodeList.Codes.PointOfContact)
													{
														previousCompanyRecord = pg19Block;
														organizationsDict[pg19Block] = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(pg19Block, null, null);
													}

													previousPG19CustomsNoType = pg19Block.EntityRoleCode;
												}
												else
												{
													var pg20Block = block as AEPAPG20;
													if (pg20Block != null)
													{
														if (previousCompanyRecord != null)
														{
															var tupleValue = organizationsDict[previousCompanyRecord];
															var value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(tupleValue.Item1, pg20Block, pg20Block);
															organizationsDict[previousCompanyRecord] = value;
															previousCompanyRecord = null;
														}
													}
													else
													{
														var pg21Block = block as AEPAPG21;
														if (pg21Block != null)
														{
															if (previousPG19CustomsNoType == EntityRoleCodeList.Codes.PointOfContact)
															{
																SetBrokerDetails(pg21Block.IndividualName, pg21Block.TelephoneNumberOfTheIndividual, pg21Block.EmailAddressOrFaxNumberForTheIndividual, notifications);
															}
														}
														else
														{
															var pg60Block = block as AEPAPG60;
															if (pg60Block != null)
															{
																if (previousPG19CustomsNoType == EntityRoleCodeList.Codes.PointOfContact)
																{
																	invoiceLine.US_FDAContactName += pg60Block.AdditionalInformation;
																}
															}
															else
															{
																var pg23Block = block as AEPAPG23;
																if (pg23Block != null)
																{
																	var affirmationCode = pg23Block.AffirmationOfComplianceCode;
																	var aocDescription = pg23Block.AffirmationOfComplianceDescription;
																	switch (affirmationCode)
																	{
																		case ACE_AffirmationOfComplianceList.Codes.VES:
																			invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.Declaration, JobDeclaration.Schema.JE_VesselName, aocDescription, aocDescription, "Vessel", notifications);
																			break;
																		case ACE_AffirmationOfComplianceList.Codes.VFT:
																			invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.Declaration, JobDeclaration.Schema.JE_VoyageFlightNo, aocDescription, aocDescription, "Voyage/Flight", notifications);
																			break;
																		case ACE_AffirmationOfComplianceList.Codes.PFR:
																		case ACE_AffirmationOfComplianceList.Codes.CFR:
																		case ACE_AffirmationOfComplianceList.Codes.GFR:
																			fda.US_PFR = aocDescription;
																			break;
																		case ACE_AffirmationOfComplianceList.Codes.FME:
																			fda.US_FME = aocDescription;
																			break;
																		default:
																			var aoc = fda.AffirmationCodes.AddNew();
																			aoc.CY_Code = affirmationCode;
																			aoc.CY_Data = aocDescription;
																			break;
																	}
																}
																else
																{
																	var pg24Block = block as AEPAPG24;
																	if (pg24Block != null)
																	{
																		if (pg24Block.RemarksTypeCode == "GEN")
																		{
																			fda.US_Remarks = pg24Block.RemarksText;
																		}
																	}
																	else
																	{
																		var pg25Block = block as AEPAPG25;
																		if (pg25Block != null)
																		{
																			if (!pg25Block.TemperatureQualifier.IsEmpty || !pg25Block.DegreeType.IsEmpty || !pg25Block.LocationOfTemperatureRecording.IsEmpty || !pg25Block.LotNumber.IsEmpty
																				|| !pg25Block.ProductionEndDateOfTheLot.IsEmpty || !pg25Block.ProductionStartDateOfTheLot.IsEmpty || !pg25Block.ActualTemperature.IsEmpty)
																			{
																				var lot = fda.Lots.AddNew();
																				lot.US_TemperatureQualifier = pg25Block.TemperatureQualifier;
																				lot.US_DegreeType = pg25Block.DegreeType;
																				lot.US_LocationOfTemp = pg25Block.LocationOfTemperatureRecording;
																				lot.US_LotNumber = pg25Block.LotNumber;
																				lot.US_EndDate = pg25Block.ProductionEndDateOfTheLot;
																				lot.US_StartDate = pg25Block.ProductionStartDateOfTheLot;

																				if (!pg25Block.ActualTemperature.IsEmpty)
																				{
																					var actualTemperature = pg25Block.ActualTemperature.PadLeft(6, '0');
																					var temperatureDecimalString = actualTemperature.Substring(0, 4) + "." + actualTemperature.SubstringSafe(4);
																					var temperature = ZDecimal.ParseSafe(temperatureDecimalString, 0m);
																					if (pg25Block.NegativeNumber == "X")
																					{
																						lot.US_Temperature = decimal.Negate(temperature);
																					}
																					else
																					{
																						lot.US_Temperature = temperature;
																					}
																				}
																			}

																			if (!pg25Block.PGALineValue.IsEmpty)
																			{
																				fda.US_InvCurrValue = pg25Block.PGALineValue;
																			}

																			if (!pg25Block.PGAUnitValue.IsEmpty)
																			{
																				fda.US_UnitValue = pg25Block.PGAUnitValue;
																			}
																		}
																		else
																		{
																			var pg26Block = block as AEPAPG26;
																			if (pg26Block != null)
																			{
																				quantityUQsList.Add(new Tuple<ZDecimal, ZString>(pg26Block.Quantity, pg26Block.UnitOfMeasurePackagingLevel));
																			}
																			else
																			{
																				var pg27Block = block as AEPAPG27;
																				if (pg27Block != null)
																				{
																					var containerNumber = pg27Block.ContainerNumberEquipmentID;
																					if (!containerNumber.IsEmpty && !invoiceLine.Declaration.CusContainers.OfType<CusContainer>().Any(x => x.CO_ContainerNumber == containerNumber))
																					{
																						invoiceLine.Declaration.CusContainers.AddNew().CO_ContainerNumber = containerNumber;
																					}
																				}
																				else
																				{
																					var pg28Block = block as AEPAPG28;
																					if (pg28Block != null)
																					{
																						SetContainerDimensions(fda.US_CanDim1Info, pg28Block.CanDimensions1);
																						SetContainerDimensions(fda.US_CanDim2Info, pg28Block.CanDimensions2);
																						SetContainerDimensions(fda.US_CanDim3Info, pg28Block.CanDimension3);

																						if (!pg28Block.PackageTrackingNumberDetails.IsEmpty)
																						{
																							var packageTrackingNumberCode = pg28Block.PackageTrackingNumberDetails.SubstringSafe(0, 4).TrimEnd();
																							fda.US_PackageTrackCode = packageTrackingNumberCode;

																							var packageTrackingNumber = pg28Block.PackageTrackingNumberDetails.SubstringSafe(4);
																							fda.US_PackageTrackNumber = packageTrackingNumber;
																						}
																					}
																					else
																					{
																						var pg30Block = block as AEPAPG30;
																						if (pg30Block != null)
																						{
																							if (pg30Block.InspectionLaboratoryTestingStatus == InspectionStatusList.Codes.BTAAnticipatedArrivalInformation)
																							{
																								var dateTime = DateTimeParser.GetDateTimeFromZDateAndStringTime(pg30Block.AnticipatedArrivalDate, pg30Block.ArrivalTime);
																								invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.Declaration, JobDeclaration.Schema.US_FDAADTA, dateTime, dateTime.ToLongTimeString(), "PGA Arrival Date/Time", notifications);
																							}
																							else if (pg30Block.InspectionLaboratoryTestingStatus == InspectionStatusList.Codes.ForeignTradeZone)
																							{
																								var goodsFromFTZ = pg30Block.ArrivalLocation;
																								if (!goodsFromFTZ.IsEmpty)
																								{
																									invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.Declaration, JobDeclaration.Schema.US_GoodsFromFTZ, goodsFromFTZ, goodsFromFTZ, "Goods From FTZ", notifications);
																								}
																							}
																						}
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}

			ProcessOrganizations(fda, organizationsDict, notifications);
			ProcessQuantityAndUQs(fda, quantityUQsList);
		}

		void DefaultProducerType(ACEFDA fda, ZString productionGrowthCountry)
		{
			if (fda.US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_FEE && productionGrowthCountry == SourceTypeCodesList.Codes.CountryOfProduction)
			{
				fda.US_ProducerType = ProducerFirmTypeList.Codes.M;
			}
		}

		void DefaultProducerType(ACEFDA fda, AEPAPG19 pg19)
		{
			if (fda.US_ProgramCode == FDAProgramCodeList.Codes.TOB)
			{
				if (pg19.EntityRoleCode == EntityRoleCodeList.Codes.IndependentThirdPartyLaboratory)
				{
					fda.US_ProducerType = "I";
				}
				else if (pg19.EntityRoleCode == EntityRoleCodeList.Codes.Laboratory)
				{
					fda.US_ProducerType = "L";
				}
			}
			else if (pg19.EntityRoleCode == EntityRoleCodeList.Codes.FDAConsolidator)
			{
				fda.US_ProducerType = ProducerFirmTypeList.Codes.C;
			}
			else if (pg19.EntityRoleCode == EntityRoleCodeList.Codes.CropGrower)
			{
				fda.US_ProducerType = ProducerFirmTypeList.Codes.G;
			}
		}

		protected override ZString GetMatchedCustomsNoTypeInOrganization(ZString customsNoTypeInMessage)
		{
			var result = ZString.Empty;

			switch (customsNoTypeInMessage)
			{
				case EntityIdentificationCodesList.Codes.FDAAssigned:
					result = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
					break;
				case EntityIdentificationCodesList.Codes.DUNSNumber:
					result = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
					break;
			}

			return result;
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString organizationType, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo addressPropertyInfo = null;
			var fda = pga as ACEFDA;
			ZGuid addressPK = ZGuid.Empty;
			ZString organizationCode = ZString.Empty;
			var fieldType = OrganizationFieldType.Address;

			var overrideExistingValue = true;
			var matchCustomsNumberOnly = false;

			if (organizationType == EntityRoleCodeList.Codes.FDAConsolidator || organizationType == EntityRoleCodeList.Codes.CropGrower || organizationType == EntityRoleCodeList.Codes.IndependentThirdPartyLaboratory || organizationType == EntityRoleCodeList.Codes.Laboratory)
			{
				addressPropertyInfo = fda.US_ManufacturerAddressInfo;
				organizationCode = "Manufacturer";
			}
			else if (organizationType == EntityRoleCodeList.Codes.Shipper)
			{
				addressPropertyInfo = fda.US_OA_ShipperAddressInfo;
				organizationCode = "Shipper";
			}
			else if (organizationType == EntityRoleCodeList.Codes.FDAImporter1)
			{
				addressPropertyInfo = fda.US_FDAImporterAddressInfo;
				organizationCode = "FDA Importer";
			}
			else if (organizationType == EntityRoleCodeList.Codes.UltimateConsignee || organizationType == EntityRoleCodeList.Codes.DeliveryParty)
			{
				addressPropertyInfo = fda.US_DeliverToPartyAddressInfo;
				organizationCode = "Delivery To Party";
			}
			else if (organizationType == EntityRoleCodeList.Codes.DeviceInitialImporter || organizationType == EntityRoleCodeList.Codes.Sponsor)
			{
				addressPropertyInfo = fda.US_ProducerAddressInfo;
				organizationCode = @"Initial Importer\Sponsor";
			}
			else if (organizationType == EntityRoleCodeList.Codes.Submitter || organizationType == EntityRoleCodeList.Codes.PNSubmitter)
			{
				addressPropertyInfo = fda.InvoiceLine.Declaration.JE_OH_FDASubmitterInfo;
				organizationCode = "Submitter";
				matchCustomsNumberOnly = true;
				overrideExistingValue = false;
				fieldType = OrganizationFieldType.Header;
			}
			else if (organizationType == EntityRoleCodeList.Codes.ManufacturerOfGoods)
			{
				if (fda.US_ProgramCode == FDAProgramCodeList.Codes.TOB && !fda.US_ManufacturerAddress.IsEmpty)
				{
					addressPropertyInfo = fda.US_ProducerAddressInfo;
				}
				else
				{
					addressPropertyInfo = fda.US_ManufacturerAddressInfo;
				}

				organizationCode = "Manufacturer";
			}
			else if (organizationType == EntityRoleCodeList.Codes.Producer)
			{
				var firstEmptyConstituent = fda.ProductConstituentElements.OfType<ConstituentElement>().FirstOrDefault(x => x.US_OA_ProducerAddress.IsEmpty);
				if (firstEmptyConstituent != null)
				{
					addressPropertyInfo = firstEmptyConstituent.US_OA_ProducerAddressInfo;
					organizationCode = "Producer";
				}
			}
			else if (organizationType == EntityRoleCodeList.Codes.Owner)
			{
				addressPropertyInfo = fda.US_OwnerAddressInfo;
				organizationCode = "Owner";
			}
			else if (organizationType == EntityRoleCodeList.Codes.LocationOfGoodsImmediatelyAfterEntryRelease)
			{
				addressPropertyInfo = fda.US_LocationOfGoodsAddressInfo;
				organizationCode = "Goods Location";
			}
			else if (organizationType == EntityRoleCodeList.Codes.FSVPImporter)
			{
				addressPropertyInfo = fda.US_FSVPImporterAddressInfo;
				organizationCode = "FSVP Importer";
			}

			if (!organizationCode.IsEmpty)
			{
				if (matchCustomsNumberOnly)
				{
					addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, notifications);
				}
				else
				{
					addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
				}

				SetOrganizationOrAddress(fieldType, organizationCode, addressPropertyInfo, addressPK, overrideExistingValue, notifications);
			}
		}

		void ProcessQuantityAndUQs(ACEFDA fda, List<Tuple<ZDecimal, ZString>> quantityUQs)
		{
			while (quantityUQs.Count < 6)
			{
				quantityUQs.Insert(0, new Tuple<ZDecimal, ZString>(0m, ZString.Empty));
			}

			var quantityAndUQ6 = quantityUQs[0];
			fda.US_Qty6 = quantityAndUQ6.Item1;
			fda.US_UQ6 = quantityAndUQ6.Item2;

			var quantityAndUQ5 = quantityUQs[1];
			fda.US_Qty5 = quantityAndUQ5.Item1;
			fda.US_UQ5 = quantityAndUQ5.Item2;

			var quantityAndUQ4 = quantityUQs[2];
			fda.US_Qty4 = quantityAndUQ4.Item1;
			fda.US_UQ4 = quantityAndUQ4.Item2;

			var quantityAndUQ3 = quantityUQs[3];
			fda.US_Qty3 = quantityAndUQ3.Item1;
			fda.US_UQ3 = quantityAndUQ3.Item2;

			var quantityAndUQ2 = quantityUQs[4];
			fda.US_Qty2 = quantityAndUQ2.Item1;
			fda.US_UQ2 = quantityAndUQ2.Item2;

			var quantityAndUQ1 = quantityUQs[5];
			fda.US_Qty1 = quantityAndUQ1.Item1;
			fda.US_UQ1 = quantityAndUQ1.Item2;
		}

		void SetContainerDimensions(ZPropertyInfo propertyInfo, ZString dimensionString)
		{
			if (!dimensionString.IsEmpty)
			{
				var firstValue = dimensionString.SubstringSafe(0, dimensionString.Length - 2).TrimStart();
				firstValue = firstValue.IsEmpty ? "0" : (string)firstValue;
				var secondValue = dimensionString.SubstringSafe(dimensionString.Length - 2);
				secondValue = secondValue.IsEmpty ? "00" : (string)secondValue;
				var formattedString = firstValue + "." + secondValue;
				propertyInfo.Value = ZDecimal.ParseSafe(formattedString, 0m);
			}
		}
	}
}
