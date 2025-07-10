using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using CommodityQualifier = Enterprise.Customs.US.Business.APHIS.CommodityQualifier;

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDAPHISDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDAPHISDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_APHISDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_APHISIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "APHIS"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var aphis = invoiceLine.APHISHeaders.AddNew();
			aphis.US_ProgramType = pg01.GovernmentAgencyProgramCode;
			aphis.US_ProcessingCode = pg01.GovernmentAgencyProcessingCode;
			aphis.US_IsDocSubmitted = pg01.ElectronicImageSubmitted == "Y";
			aphis.US_IntendedUseCode = pg01.IntendedUseCode;
			aphis.US_IntendedUseDescription = pg01.IntendedUseDescription;

			var firstPG10Record = pgaBlocks.OfType<AEPAPG10>().FirstOrDefault();
			if (firstPG10Record != null)
			{
				aphis.US_CategoryType = firstPG10Record.CategoryTypeCode;
				aphis.US_CategoryCode = firstPG10Record.CategoryCode;
			}

			aphis.Sources.RemoveAndDeleteAll();
			aphis.Routings.RemoveAndDeleteAll();
			aphis.Inspections.RemoveAndDeleteAll();
			aphis.Licenses.RemoveAndDeleteAll();

			ZBool isComponent = false;
			APHISProduct currentProduct = null;
			APHISIdentity currentIdentity = null;
			APHISLicense currentLicense = null;
			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
			var licenseOriganizationDict = new Dictionary<APHISLicense, Tuple<IACEBIRDOrgCompanyRecord, IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
			var quantityUQsList = new List<Tuple<ZDecimal, ZString>>();

			foreach (var block in pgaBlocks)
			{
				var pg02Block = block as AEPAPG02;
				if (pg02Block != null)
				{
					aphis.US_ProductType = pg02Block.ProductCodeQualifier;
					aphis.US_ProductNumber = pg02Block.ProductCodeNumber;

					if (pg02Block.ProductCodeQualifier1 == ProductCodeQualifiersList.Codes.StockKeepingUnit)
					{
						aphis.US_StockKeepingUnitNumber = pg02Block.ProductCodeNumber1;
					}

					isComponent = pg02Block.ItemType == PG02ItemTypeList.Codes.Component;
				}
				else
				{
					var pg17Block = block as AEPAPG17;
					if (pg17Block != null)
					{
						if (isComponent && currentProduct != null)
						{
							currentProduct.US_SpecificName = pg17Block.CommonNameSpecific;
							currentProduct.US_GeneralName = pg17Block.CommonNameGeneral;
						}
						else
						{
							aphis.US_CommoditySpecificName = pg17Block.CommonNameSpecific;
						}
					}
					else
					{
						var pg07Block = block as AEPAPG07;
						if (pg07Block != null)
						{
							if (pg07Block.ItemIdentityNumberQualifier == APHISItemIdentityNumberQualifierList.Codes.BQG)
							{
								aphis.US_BouquetGroupingNumber = pg07Block.ItemIdentityNumber;
							}
							else
							{
								currentProduct = aphis.Products.AddNew();
								currentProduct.Identities.RemoveAndDeleteAll();

								currentIdentity = currentProduct.Identities.AddNew();
								currentIdentity.CY_Code = pg07Block.ItemIdentityNumberQualifier;
								currentIdentity.CY_Data = pg07Block.ItemIdentityNumber;
							}
						}
						else
						{
							var pg08Block = block as AEPAPG08;
							if (pg08Block != null)
							{
								var startNumber = ZString.Empty;
								var endNumber = ZString.Empty;

								if (!pg08Block.ItemIdentityNumber.StartsWith("#RS", System.StringComparison.OrdinalIgnoreCase))
								{
									AddProductNumberRange(currentIdentity, pg08Block.ItemIdentityNumber, ZString.Empty);
								}
								else
								{
									startNumber = pg08Block.ItemIdentityNumber.SubstringSafe(3);
								}

								if (!pg08Block.ItemIdentityNumber1.StartsWith("#RE", System.StringComparison.OrdinalIgnoreCase))
								{
									AddProductNumberRange(currentIdentity, pg08Block.ItemIdentityNumber1, ZString.Empty);
								}
								else
								{
									endNumber = pg08Block.ItemIdentityNumber1.SubstringSafe(3);
								}

								if (!startNumber.IsEmpty || !endNumber.IsEmpty)
								{
									AddProductNumberRange(currentIdentity, startNumber, endNumber);
									startNumber = ZString.Empty;
									endNumber = ZString.Empty;
								}

								if (!pg08Block.ItemIdentityNumber2.StartsWith("#RS", System.StringComparison.OrdinalIgnoreCase))
								{
									AddProductNumberRange(currentIdentity, pg08Block.ItemIdentityNumber2, ZString.Empty);
								}
								else
								{
									startNumber = pg08Block.ItemIdentityNumber2.SubstringSafe(3);
								}

								if (!pg08Block.ItemIdentityNumber3.StartsWith("#RE", System.StringComparison.OrdinalIgnoreCase))
								{
									AddProductNumberRange(currentIdentity, pg08Block.ItemIdentityNumber3, ZString.Empty);
								}
								else
								{
									endNumber = pg08Block.ItemIdentityNumber3.SubstringSafe(3);
								}

								if (!startNumber.IsEmpty || !endNumber.IsEmpty)
								{
									AddProductNumberRange(currentIdentity, startNumber, endNumber);
									startNumber = ZString.Empty;
									endNumber = ZString.Empty;
								}
							}
							else
							{
								var pg10Block = block as AEPAPG10;
								if (pg10Block != null)
								{
									ProcessProductCharacteristics(aphis, currentProduct, pg10Block.CommodityQualifierCode, pg10Block.CommodityCharacteristicQualifier, pg10Block.CommodityCharacteristicDescription);
								}
								else
								{
									var pg05Block = block as AEPAPG05;
									if (pg05Block != null)
									{
										aphis.US_ScientificGenusName = pg05Block.ScientificGenusName;
										aphis.US_ScientificSpeciesName = pg05Block.ScientificSpeciesName;
										aphis.US_ScientificSubSpeciesName = pg05Block.ScientificSubSpeciesName;
									}
									else
									{
										var pg06Block = block as AEPAPG06;
										if (pg06Block != null)
										{
											var source = aphis.Sources.AddNew();
											source.US_SourceTypeCode = pg06Block.SourceTypeCode;
											source.US_CountryCode = pg06Block.CountryCode;
											source.US_GeographicLocation = pg06Block.GeographicLocation;
											source.US_ProcessingTypeCode = pg06Block.ProcessingTypeCode;
											source.US_ProcessingStartDate = pg06Block.ProcessingStartDate;
											source.US_ProcessingEndDate = pg06Block.ProcessingEndDate;
											source.US_ProcessingDescription = pg06Block.ProcessingDescription;
										}
										else
										{
											var pg13Block = block as AEPAPG13;
											if (pg13Block != null)
											{
												currentLicense = aphis.Licenses.AddNew();
												currentLicense.US_RN_CountryCode = pg13Block.LocationCountryStateProvinceOfIssuerOfTheLPCO;
												currentLicense.US_StateDescription = pg13Block.RegionalDescriptionOfLocationOfAgencyIssuingTheLPCO;

												licenseOriganizationDict.Add(currentLicense, new Tuple<IACEBIRDOrgCompanyRecord, IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(null, null, null, null));
											}
											else
											{
												var pg14Block = block as AEPAPG14;
												if (pg14Block != null)
												{
													if (currentLicense != null)
													{
														currentLicense.US_Type = pg14Block.LPCOType;
														currentLicense.US_Number = pg14Block.LPCONumberorName;
														currentLicense.US_DateQualifier = pg14Block.LPCODateQualifier;
														currentLicense.US_Date = pg14Block.LPCODate;
														currentLicense.US_Quantity = pg14Block.LPCOQuantity;
														currentLicense.US_UnitOfMeasure = pg14Block.LPCOUnitOfMeasure;
													}
												}
												else
												{
													var pg19Block = block as AEPAPG19;
													if (pg19Block != null)
													{
														switch (pg19Block.EntityRoleCode)
														{
															case EntityRoleCodeList.Codes.CropGrower:
															case EntityRoleCodeList.Codes.Shipper:
															case EntityRoleCodeList.Codes.LPCOAuthorizedParty:
															case EntityRoleCodeList.Codes.UltimateConsignee:
															case EntityRoleCodeList.Codes.PermittedDestination:
															case EntityRoleCodeList.Codes.Importer:
															case EntityRoleCodeList.Codes.USDAAPHISGrower:
																previousCompanyRecord = pg19Block;
																organizationsDict[pg19Block] = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(pg19Block, null, null);
																break;
														}
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
																if (pg21Block.IndividualQualifier == EntityRoleCodeList.Codes.CustomsBroker)
																{
																	SetBrokerDetails(pg21Block.IndividualName, pg21Block.TelephoneNumberOfTheIndividual, pg21Block.EmailAddressOrFaxNumberForTheIndividual, notifications);
																}
															}
															var pg24Block = block as AEPAPG24;
															if (pg24Block != null)
															{
																aphis.US_ReMarks = pg24Block.RemarksText;
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
																		if (pg27Block.ContainerNumberEquipmentID1.IsEmpty && pg27Block.TypeOfContainer == "2" && !pg27Block.ContainerLength.IsEmpty && aphis.US_VehicleNumber.IsEmpty)
																		{
																			aphis.US_VehicleNumber = pg27Block.ContainerNumberEquipmentID;
																			aphis.US_VehicleLength = (ZShort)pg27Block.ContainerLength;
																		}
																		else
																		{
																			CreatContainersOnDeclaration(pg27Block.ContainerNumberEquipmentID);
																			CreatContainersOnDeclaration(pg27Block.ContainerNumberEquipmentID1);
																			CreatContainersOnDeclaration(pg27Block.ContainerNumberEquipmentID2);
																		}
																	}
																	else
																	{
																		var pg30Block = block as AEPAPG30;
																		if (pg30Block != null)
																		{
																			var inspection = aphis.Inspections.AddNew();
																			inspection.US_TestingStatus = pg30Block.InspectionLaboratoryTestingStatus;
																			inspection.US_Date = pg30Block.AnticipatedArrivalDate;
																			inspection.US_Location = pg30Block.ArrivalLocation;
																		}
																		else
																		{
																			var pg32Block = block as AEPAPG32;
																			if (pg32Block != null)
																			{
																				var routing = aphis.Routings.AddNew();
																				routing.US_Type = pg32Block.CommodityRoutingTypeCode;
																				routing.US_Country = pg32Block.CommodityRoutingCountryCode;
																				routing.US_State = pg32Block.CommodityPoliticalSubunitOfRoutingName;
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

			ProcessOrganizations(aphis, organizationsDict, notifications);
			ProcessQuantityAndUQs(aphis, quantityUQsList);

			foreach (var licenseOrganization in licenseOriganizationDict)
			{
				if (licenseOrganization.Value != null && licenseOrganization.Value.Item1 != null)
				{
					var orgDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
					orgDict.Add(licenseOrganization.Value.Item1, new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(licenseOrganization.Value.Item2, licenseOrganization.Value.Item3, licenseOrganization.Value.Item4));
					ProcessOrganizations(licenseOrganization.Key, orgDict, notifications);
				}
			}
		}

		void AddProductNumberRange(APHISIdentity identity, ZString startNumber, ZString endNumber)
		{
			if (!identity.NumberRanges.OfType<APHISIdentityNumberRange>().Any(x => x.US_StartNumber == startNumber && x.US_EndNumber == endNumber))
			{
				var numberRange = identity.NumberRanges.AddNew();
				numberRange.US_StartNumber = startNumber;
				numberRange.US_EndNumber = endNumber;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ProcessProductCharacteristics(APHISHeader header, APHISProduct product, ZString qualifierCode, ZString qualifier, ZString description)
		{
			var foundMatched = false;
			ZPropertyInfo propertyInfo = null;
			ZPropertyInfo descriptionPropertyInfo = null;

			if (product != null)
			{
				switch (qualifierCode)
				{
					case CommodityQualifier.AnimalProductsAndByProductsList.Codes.SpeciesComposition:
						foundMatched = true;
						propertyInfo = product.US_TypeInfo;
						break;
					case CommodityQualifier.LiveAnimalsList.Codes.Age:
						foundMatched = true;
						propertyInfo = product.US_AgeInfo;
						descriptionPropertyInfo = product.US_AgeRangeDescInfo;
						break;
					case CommodityQualifier.LiveAnimalsList.Codes.BreedVariety:
						foundMatched = true;
						propertyInfo = product.US_BreedVarietyInfo;
						break;
					case CommodityQualifier.LiveAnimalsList.Codes.Color:
						foundMatched = true;
						propertyInfo = product.US_ColorInfo;
						break;
					case CommodityQualifier.LiveAnimalsList.Codes.Gender:
						foundMatched = true;
						propertyInfo = product.US_GenderInfo;
						break;
					case CommodityQualifier.LiveAnimalsList.Codes.FertilizedPregnantGestating:
						foundMatched = true;
						propertyInfo = product.US_IsFertilizedPregnantGestatingInfo;
						break;
					case CommodityQualifier.LiveAnimalsList.Codes.GestationalAgeIfPregnant:
						foundMatched = true;
						propertyInfo = product.US_GestationalAgeIfPregnantInfo;
						break;
					case CommodityQualifier.LiveAnimalsList.Codes.ProtectedSpecies:
						foundMatched = true;
						propertyInfo = product.US_IsProtectedSpeciesInfo;
						break;
				}
			}

			if (!foundMatched)
			{
				switch (qualifierCode)
				{
					case CommodityQualifier.AnimalProductsAndByProductsList.Codes.Condition:
					case CommodityQualifier.CutFlowersAndGreeneryList.Codes.TypesOfCutFlowerAndGreenery:
					case CommodityQualifier.GeneticallyEngineeredOrganismsList.Codes.Type:
					case CommodityQualifier.MiscellaneousAndProcessedProductsList.Codes.Condition:
					case CommodityQualifier.RelatedAnimalProductsList.Codes.Condition:
						propertyInfo = header.US_ProductConditionInfo;
						break;
					case CommodityQualifier.AnimalProductsAndByProductsList.Codes.PhysicalStateFormArrangementOrMode:
					case CommodityQualifier.CutFlowersAndGreeneryList.Codes.PhysicalStateFormArrangementOrMode:
					case CommodityQualifier.FruitsAndVegetablesList.Codes.PhysicalStateFormArrangementOrMode:
					case CommodityQualifier.GeneticallyEngineeredOrganismsList.Codes.LifeStage:
					case CommodityQualifier.MiscellaneousAndProcessedProductsList.Codes.PhysicalStateFormArrangementOrMode:
					case CommodityQualifier.PropagativeMaterialList.Codes.PhysicalStateFormArrangementOrMode:
					case CommodityQualifier.RelatedAnimalProductsList.Codes.PhysicalStateFormArrangementOrMode:
					case CommodityQualifier.SeedsNotForPlantingList.Codes.PhysicalStateFormArrangementOrMode:
						propertyInfo = header.US_ProductPhysicalStateInfo;
						break;
					case CommodityQualifier.AnimalProductsAndByProductsList.Codes.SpeciesComposition:
					case CommodityQualifier.GeneticallyEngineeredOrganismsList.Codes.IntergenericYesNo:
						propertyInfo = header.US_ProductComponentInfo;
						break;
					case CommodityQualifier.PropagativeMaterialList.Codes.EndangeredSpeciesStatus:
					case CommodityQualifier.CutFlowersAndGreeneryList.Codes.EndangeredSpeciesStatus:
						propertyInfo = header.US_ProductStatusInfo;
						break;
					case CommodityQualifier.PropagativeMaterialList.Codes.GrowingMedia:
						propertyInfo = header.US_GrowingMediaInfo;
						break;
				}
			}

			if (propertyInfo != null)
			{
				propertyInfo.Value = qualifier;
			}

			if (descriptionPropertyInfo != null)
			{
				descriptionPropertyInfo.Value = description;
			}
		}

		protected override ZString GetMatchedCustomsNoTypeInOrganization(ZString customsNoTypeInMessage)
		{
			var result = ZString.Empty;

			switch (customsNoTypeInMessage)
			{
				case EntityIdentificationCodesList.Codes.APHISAssigned:
					result = OrgCusCode.USACodeTypes.APHISAssignedNumber;
					break;
				case EntityIdentificationCodesList.Codes.IRSAssigned:
					result = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
					break;
			}

			return result;
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo orgPropertyInfo = null;
			var aphis = pga as APHISHeader;

			var organizationCode = ZString.Empty;
			var overrideExisting = true;
			var addressPK = ZGuid.Empty;
			var fieldType = OrganizationFieldType.Address;

			if (aphis != null)
			{
				if (roleCode == EntityRoleCodeList.Codes.CropGrower)
				{
					orgPropertyInfo = aphis.US_OA_CropGrowerAddressInfo;
					organizationCode = "Crop Grower";
					addressPK = FindMatchedOrgAddressPK(organizationCode, ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
				}
				else if (roleCode == EntityRoleCodeList.Codes.Shipper)
				{
					orgPropertyInfo = aphis.US_OA_ShipperAddressInfo;
					organizationCode = "Shipper";
					addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
				}
				else if (roleCode == EntityRoleCodeList.Codes.LPCOAuthorizedParty)
				{
					orgPropertyInfo = aphis.US_OA_ApplicantAddressInfo;
					organizationCode = "Authorized License Holder";
					addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
				}
				else if (roleCode == EntityRoleCodeList.Codes.UltimateConsignee)
				{
					orgPropertyInfo = invoiceLine.JI_OA_ConsigneeAddressInfo;
					organizationCode = "Consignee";
					overrideExisting = false;
					addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
				}
				else if (roleCode == EntityRoleCodeList.Codes.PermittedDestination)
				{
					orgPropertyInfo = aphis.US_OA_PermittedAddressInfo;
					organizationCode = "Permitted Dest.";
					addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
				}
				else if (roleCode == EntityRoleCodeList.Codes.Importer)
				{
					orgPropertyInfo = invoiceLine.InvoiceHeader.JZ_OH_BuyerInfo;
					fieldType = OrganizationFieldType.Header;
					organizationCode = "Buyer";
					overrideExisting = false;
					addressPK = FindMatchedOrgAddressPK(organizationCode, ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
				}
				else if (roleCode == EntityRoleCodeList.Codes.USDAAPHISGrower)
				{
					orgPropertyInfo = aphis.US_OA_USDAAPHISGrowerAddressInfo;
					organizationCode = "USDA APHIS Grower";
					addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
				}
			}

			if (!organizationCode.IsEmpty)
			{
				SetOrganizationOrAddress(fieldType, organizationCode, orgPropertyInfo, addressPK, overrideExisting, notifications);
			}
		}

		void ProcessQuantityAndUQs(APHISHeader aphis, List<Tuple<ZDecimal, ZString>> quantityUQs)
		{
			while (quantityUQs.Count < 3)
			{
				quantityUQs.Insert(0, new Tuple<ZDecimal, ZString>(0m, ZString.Empty));
			}

			var quantityAndUQ3 = quantityUQs[0];
			aphis.US_Qty3 = quantityAndUQ3.Item1;
			aphis.US_UQ3 = quantityAndUQ3.Item2;

			var quantityAndUQ2 = quantityUQs[1];
			aphis.US_Qty2 = quantityAndUQ2.Item1;
			aphis.US_UQ2 = quantityAndUQ2.Item2;

			var quantityAndUQ1 = quantityUQs[2];
			aphis.US_Qty1 = quantityAndUQ1.Item1;
			aphis.US_UQ1 = quantityAndUQ1.Item2;
		}

		void CreatContainersOnDeclaration(ZString containerNumber)
		{
			if (!containerNumber.IsEmpty && !invoiceLine.Declaration.CusContainers.OfType<CusContainer>().Any(x => x.CO_ContainerNumber == containerNumber))
			{
				invoiceLine.Declaration.CusContainers.AddNew().CO_ContainerNumber = containerNumber;
			}
		}
	}
}
