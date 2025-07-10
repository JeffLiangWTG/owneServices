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

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDNHTSADataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDNHTSADataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_NHTDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_NHTSAIndicatorInfo; }
		}

		public override ZString PGAName
		{
			get { return "NHTSA"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var nhtsaHeader = invoiceLine.NHTSALines.AddNew();
			nhtsaHeader.US_NHTProgramCode = pg01.GovernmentAgencyProgramCode;
			nhtsaHeader.US_NHTElectronicImage = pg01.ElectronicImageSubmitted == "Y";
			nhtsaHeader.US_IntendedUseCode = pg01.IntendedUseCode;
			nhtsaHeader.US_IntendedUseDesc = pg01.IntendedUseDescription;
			nhtsaHeader.NHTSADocuments.RemoveAndDeleteAll();

			NHTSADetails previousDetailsLine = null;
			ZString previousNumberType = ZString.Empty;
			ZString previousRoleType = ZString.Empty;
			AEPAPG21 previousPG21Block = null;
			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();

			foreach (var block in pgaBlocks)
			{
				var pg07Block = block as AEPAPG07;
				if (pg07Block != null)
				{
					var brandName = pg07Block.TradeNameBrandName;
					var model = pg07Block.Model;
					var monthOfMfr = pg07Block.ManufactureMonthAndYear.Length == 6 ? pg07Block.ManufactureMonthAndYear.SubstringSafe(0, 2) : ZString.Empty;
					var yearOfMfr = pg07Block.ManufactureMonthAndYear.Length == 6 ? pg07Block.ManufactureMonthAndYear.SubstringSafe(2) : pg07Block.ManufactureMonthAndYear.SubstringSafe(0, 4);
					var numberType = pg07Block.ItemIdentityNumberQualifier;
					var number = pg07Block.ItemIdentityNumber;

					var matchedNHTSALine = nhtsaHeader.NHTSADetails.OfType<NHTSADetails>().FirstOrDefault(x => x.US_NHTBrandName == brandName && x.US_NHTModel == model && x.US_NHTMonthOfMFR == monthOfMfr && x.US_NHTYearOfMFR == yearOfMfr);
					if (matchedNHTSALine == null)
					{
						matchedNHTSALine = nhtsaHeader.NHTSADetails.AddNew();
						matchedNHTSALine.US_NHTBrandName = brandName;
						matchedNHTSALine.US_NHTModel = model;
						matchedNHTSALine.US_NHTMonthOfMFR = monthOfMfr;
						matchedNHTSALine.US_NHTYearOfMFR = yearOfMfr;
						matchedNHTSALine.AdditionalNumbers.RemoveAndDeleteAll();
						matchedNHTSALine.PermitAndLicenses.RemoveAndDeleteAll();
					}

					if (!matchedNHTSALine.AdditionalNumbers.OfType<NHTSAAdditionalNum>().Any(x => x.US_NHTAdditionalIdentityNumQualifier == numberType && x.US_NHTAdditionalIdentityNumber == number))
					{
						AddNumberToDetailsLine(matchedNHTSALine, numberType, number);
					}

					previousNumberType = numberType;
					previousDetailsLine = matchedNHTSALine;
				}
				else
				{
					var pg08Block = block as AEPAPG08;
					if (pg08Block != null)
					{
						if (previousDetailsLine != null)
						{
							if (!pg08Block.ItemIdentityNumber.IsEmpty)
							{
								AddNumberToDetailsLine(previousDetailsLine, previousNumberType, pg08Block.ItemIdentityNumber);
							}
							if (!pg08Block.ItemIdentityNumber1.IsEmpty)
							{
								AddNumberToDetailsLine(previousDetailsLine, previousNumberType, pg08Block.ItemIdentityNumber1);
							}
							if (!pg08Block.ItemIdentityNumber2.IsEmpty)
							{
								AddNumberToDetailsLine(previousDetailsLine, previousNumberType, pg08Block.ItemIdentityNumber2);
							}
							if (!pg08Block.ItemIdentityNumber3.IsEmpty)
							{
								AddNumberToDetailsLine(previousDetailsLine, previousNumberType, pg08Block.ItemIdentityNumber3);
							}
						}
					}
					else
					{
						var pg10Block = block as AEPAPG10;
						if (pg10Block != null)
						{
							if (previousDetailsLine != null)
							{
								previousDetailsLine.US_NHTCategoryCode = pg10Block.CategoryCode;

								switch (pg10Block.CommodityQualifierCode)
								{
									case CommodityVehicleQualifierCodesList.Codes.V01:
										previousDetailsLine.US_NHTDriveSide = pg10Block.CommodityCharacteristicQualifier;
										break;
									case CommodityVehicleQualifierCodesList.Codes.V06:
										previousDetailsLine.US_NHTModelYear = pg10Block.CommodityCharacteristicQualifier;
										break;
								}
							}
						}
						else
						{
							var pg14Block = block as AEPAPG14;
							if (pg14Block != null)
							{
								if (previousDetailsLine != null)
								{
									var lpcoType = pg14Block.LPCOType;
									var lpcoNumber = pg14Block.LPCONumberorName;
									var dateType = pg14Block.LPCODateQualifier;
									var date = pg14Block.LPCODate;
									var quantity = pg14Block.LPCOQuantity;

									if (!previousDetailsLine.PermitAndLicenses.OfType<NHTSAPermitAndLicenses>().Any(x => x.US_NHTLPCOType == lpcoType && x.US_NHTLPCONumber == lpcoNumber && x.US_NHTLPCODateType == dateType && x.US_NHTLPCODate.Date == date && x.US_NHTLPCOQuantity == quantity))
									{
										AddPermitAndLicenseToDetailsLine(previousDetailsLine, lpcoType, lpcoNumber, dateType, date, quantity);
									}
								}
							}
							else
							{
								var pg19Block = block as AEPAPG19;
								if (pg19Block != null)
								{
									switch (pg19Block.EntityRoleCode)
									{
										case EntityRoleCodeList.Codes.Consignee:
										case EntityRoleCodeList.Codes.Owner:
										case EntityRoleCodeList.Codes.FabricatingManufacturer:
										case EntityRoleCodeList.Codes.Importer:
										case EntityRoleCodeList.Codes.OriginalVehicleManufacturer:
										case EntityRoleCodeList.Codes.RetailerDistributor:
											previousCompanyRecord = pg19Block;
											organizationsDict[pg19Block] = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(pg19Block, null, null);
											break;
									}

									previousRoleType = pg19Block.EntityRoleCode;
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
											if (previousRoleType == EntityRoleCodeList.Codes.CertifyingIndividual)
											{
												SetBrokerDetails(pg21Block.IndividualName, pg21Block.TelephoneNumberOfTheIndividual, pg21Block.EmailAddressOrFaxNumberForTheIndividual, notifications);
												nhtsaHeader.US_CertifyingIndividual = EntityRoleCodeList.Codes.CustomsBroker;
											}

											previousPG21Block = pg21Block;
										}
										else
										{
											var pg60Block = block as AEPAPG60;
											if (pg60Block != null)
											{
												if (previousRoleType == EntityRoleCodeList.Codes.CertifyingIndividual)
												{
													invoiceLine.US_FDAContactName += pg60Block.AdditionalInformation;
												}
											}
											else
											{
												var pg34Block = block as AEPAPG34;
												if (pg34Block != null)
												{
													nhtsaHeader.US_NHTTravelDocType = pg34Block.TravelDocumentTypeCode;
													nhtsaHeader.US_NHTTravelDocNationality = pg34Block.TravelDocumentNationality;
													nhtsaHeader.US_NHTTravelDocNumber = pg34Block.TravelDocumentIdentifier;
												}
												else
												{
													var pg22Block = block as AEPAPG22;
													if (pg22Block != null)
													{
														var document = nhtsaHeader.NHTSADocuments.AddNew();
														document.US_NHTDocumentType = pg22Block.DocumentIdentifier;
														document.US_NHTDocumentOwner = pg22Block.EntityRoleCode;

														if (!pg22Block.ConformanceDeclaration.IsEmpty)
														{
															string boxNumber = pg22Block.ConformanceDeclaration.Trim('0').Trim();
															nhtsaHeader.US_NHTBoxNumber = boxNumber.Length == 1 ? "0" + boxNumber : boxNumber;
														}
													}
													else
													{
														var pg24Block = block as AEPAPG24;
														if (pg24Block != null)
														{
															if (pg24Block.RemarksTypeCode == "NHE" && pg24Block.RemarksCode == "NEM")
															{
																nhtsaHeader.US_NHTEmbassyNationality = pg24Block.RemarksText;
															}
														}
														else
														{
															var pg35Block = block as AEPAPG35;
															if (pg35Block != null)
															{
																nhtsaHeader.US_NHTDOTSuretyCode = pg35Block.DOTSuretyCode;
																nhtsaHeader.US_NHTDOTBondNumber = pg35Block.DOTBondSerialNumber;
																nhtsaHeader.US_NHTDOTBondType = pg35Block.DOTBondQualifier;
																nhtsaHeader.US_NHTDOTBondAmount = pg35Block.DOTBondAmount;
															}
															else
															{
																var pg55Block = block as AEPAPG55;
																if (pg55Block != null)
																{
																	if (previousRoleType == EntityRoleCodeList.Codes.Importer || previousRoleType == EntityRoleCodeList.Codes.Owner)
																	{
																		nhtsaHeader.US_CertifyingIndividual = previousRoleType;

																		if (previousPG21Block != null)
																		{
																			nhtsaHeader.US_PGAContactName = previousPG21Block.IndividualName;
																			nhtsaHeader.US_PGAContactPhoneNo = previousPG21Block.TelephoneNumberOfTheIndividual;
																			nhtsaHeader.US_PGAContactEmail = previousPG21Block.EmailAddressOrFaxNumberForTheIndividual;
																		}
																	}

																	previousPG21Block = null;
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

			ProcessOrganizations(nhtsaHeader, organizationsDict, notifications);
		}

		void AddNumberToDetailsLine(NHTSADetails detailsLine, ZString numberType, ZString number)
		{
			var additionalNumber = detailsLine.AdditionalNumbers.AddNew();
			additionalNumber.US_NHTAdditionalIdentityNumber = number;
			additionalNumber.US_NHTAdditionalIdentityNumQualifier = numberType;
		}

		void AddPermitAndLicenseToDetailsLine(NHTSADetails detailsLine, ZString lpcoType, ZString lpcoNumber, ZString dateType, ZDateTime date, ZDecimal quantity)
		{
			var permitAndLicense = detailsLine.PermitAndLicenses.AddNew();
			permitAndLicense.US_NHTLPCOType = lpcoType;
			permitAndLicense.US_NHTLPCONumber = lpcoNumber;
			permitAndLicense.US_NHTLPCODateType = dateType;
			permitAndLicense.US_NHTLPCODate = date;
			permitAndLicense.US_NHTLPCOQuantity = quantity;
		}

		protected override ZString GetMatchedCustomsNoTypeInOrganization(ZString customsNoTypeInMessage)
		{
			var result = ZString.Empty;

			switch (customsNoTypeInMessage)
			{
				case EntityIdentificationCodesList.Codes.TireManufacturerCode:
					result = OrgCusCode.USACodeTypes.TireManufacturerCode;
					break;
				case EntityIdentificationCodesList.Codes.GlazingManufacturerCode:
					result = OrgCusCode.USACodeTypes.GlazingManufacturerCode;
					break;
			}

			return result;
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo orgPropertyInfo = null;
			var nhtsa = pga as NHTSAHeader;
			var organizationCode = ZString.Empty;
			var overrideExisting = true;
			var addressPK = ZGuid.Empty;

			if (roleCode == EntityRoleCodeList.Codes.Consignee)
			{
				orgPropertyInfo = invoiceLine.JI_OA_ConsigneeAddressInfo;
				organizationCode = "Consignee";
				overrideExisting = false;
				addressPK = FindMatchedOrgAddressPK("Consignee", ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
			}
			else if (roleCode == EntityRoleCodeList.Codes.Owner)
			{
				orgPropertyInfo = nhtsa.US_OA_NHTOwnerInfo;
				organizationCode = "Owner";
				addressPK = FindMatchedOrgAddressPK("Owner", ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
			}
			else if (roleCode == EntityRoleCodeList.Codes.FabricatingManufacturer)
			{
				orgPropertyInfo = nhtsa.US_NHTFabricatingMFRAddressInfo;
				organizationCode = "Fabricating Manufacturer";
				addressPK = FindMatchedOrgAddressPK("Fabricating Manufacturer", customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
			}
			else if (roleCode == EntityRoleCodeList.Codes.Importer)
			{
				if (invoiceLine.Declaration != null && invoiceLine.Declaration.IOROrgPK.IsEmpty)
				{
					notifications.AddWarning("Importer of record should have populated from SE10 or ENS10 record. Please supply EIN/CBN/SSN of an importer of record in one of the records.");
				}
			}
			else if (roleCode == EntityRoleCodeList.Codes.OriginalVehicleManufacturer)
			{
				orgPropertyInfo = nhtsa.US_NHTOriginalMFRAddressInfo;
				organizationCode = "Original Vehicle Manufacturer";
				addressPK = FindMatchedOrgAddressPK("Original Vehicle Manufacturer", customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
			}
			else if (roleCode == EntityRoleCodeList.Codes.RetailerDistributor)
			{
				orgPropertyInfo = nhtsa.US_OA_NHTRetailerInfo;
				organizationCode = "Retailer Distributor";
				addressPK = FindMatchedOrgAddressPK("Retailer Distributor", ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
			}

			if (!organizationCode.IsEmpty)
			{
				SetOrganizationOrAddress(OrganizationFieldType.Address, organizationCode, orgPropertyInfo, addressPK, overrideExisting, notifications);
			}
		}
	}
}
