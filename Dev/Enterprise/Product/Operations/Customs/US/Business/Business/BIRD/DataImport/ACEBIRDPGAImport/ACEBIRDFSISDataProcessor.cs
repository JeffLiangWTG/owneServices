using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDFSISDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDFSISDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_FSISDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_FSISIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "FSIS"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var fsis = invoiceLine.FSISLines.AddNew();
			fsis.Lots.RemoveAndDeleteAll();

			USFSISLot currentLot = null;
			ZString previousRoleType = ZString.Empty;
			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
			var sealNumbers = ZString.Empty;

			var pg14Block = pgaBlocks.OfType<AEPAPG14>().FirstOrDefault();
			if (pg14Block != null && pg14Block.LPCOType == "FS7")
			{
				fsis.US_HealthCertificateNumber = pg14Block.LPCONumberorName;
			}

			SetInvoiceLineFSISValues(fsis, USInvoiceLineFSISLine.Schema.US_ProductIDQualifier, pg01.GloballyUniqueProductIdentificationCodeQualifier, "Qualifier on FSIS", notifications);
			SetInvoiceLineFSISValues(fsis, USInvoiceLineFSISLine.Schema.US_ProductID, pg01.GloballyUniqueProductIdentificationCode, "Product ID on FSIS", notifications);
			SetInvoiceLineFSISValues(fsis, USInvoiceLineFSISLine.Schema.US_IntendedUseCode, pg01.IntendedUseCode, "Intended Use Code on FSIS", notifications);

			foreach (var block in pgaBlocks)
			{
				var pg06Block = block as AEPAPG06;
				if (pg06Block != null)
				{
					switch (pg06Block.SourceTypeCode)
					{
						case SourceTypeCodesList.Codes.CountryOfProduction:
							SetInvoiceLineFSISValues(fsis, USInvoiceLineFSISLine.Schema.US_UC_NKCountryOfOrigin, pg06Block.CountryCode, "Origin Country on FSIS", notifications);
							break;
						case SourceTypeCodesList.Codes.CountryOfSource:
							if (currentLot != null)
							{
								currentLot.US_SourceCountry = pg06Block.CountryCode;
							}
							break;
					}
				}
				else
				{
					var pg13Block = block as AEPAPG13;
					if (pg13Block != null)
					{
						if (pg13Block.LPCOIssuerGovernmentGeographicCodeQualifier == "ISO")
						{
							SetInvoiceLineFSISValues(fsis, USInvoiceLineFSISLine.Schema.US_UC_NKCertificateIssuerCountry, pg13Block.LocationCountryStateProvinceOfIssuerOfTheLPCO, "Issued Country on FSIS", notifications);
						}
					}
					else
					{
						var pg50Block = block as AEPAPG50;
						if (pg50Block != null)
						{
							currentLot = fsis.Lots.AddNew();
						}
						else
						{
							var pg10Block = block as AEPAPG10;
							if (pg10Block != null)
							{
								if (currentLot != null && pg10Block.CategoryTypeCode == "FS1")
								{
									currentLot.US_Species = pg10Block.CategoryCode;
									currentLot.US_ProductQualifierCode = pg10Block.CommodityQualifierCode;
									currentLot.US_ProductCharacteristicQualifier = pg10Block.CommodityCharacteristicQualifier;
								}
							}
							else
							{
								var pg19Block = block as AEPAPG19;
								if (pg19Block != null)
								{
									switch (pg19Block.EntityRoleCode)
									{
										case EntityRoleCodeList.Codes.ExportingEstablishment:
											SetInvoiceLineFSISValues(fsis, USInvoiceLineFSISLine.Schema.US_ExportingEstNo, pg19Block.EntityNumber, "Export Est. Number on FSIS", notifications);
											break;
										case EntityRoleCodeList.Codes.ProducingEstablishment:
											if (currentLot != null)
											{
												currentLot.US_ProducingEstNo = pg19Block.EntityNumber;
											}

											break;
										case EntityRoleCodeList.Codes.SourceEstablishment:
											if (currentLot != null)
											{
												currentLot.US_SourceEstNo = pg19Block.EntityNumber;
											}

											break;
										case EntityRoleCodeList.Codes.Importer:
										case EntityRoleCodeList.Codes.Consignee:
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
										var pg25Block = block as AEPAPG25;
										if (pg25Block != null)
										{
											if (currentLot != null)
											{
												currentLot.US_LotNumber = pg25Block.LotNumber;
												currentLot.US_StartDate = pg25Block.ProductionStartDateOfTheLot;
												currentLot.US_EndDate = pg25Block.ProductionEndDateOfTheLot;
											}
										}
										else
										{
											var pg26Block = block as AEPAPG26;
											if (pg26Block != null)
											{
												if (currentLot != null)
												{
													if (pg26Block.PackagingQualifier == 1)
													{
														currentLot.US_NoOfUnit1 = pg26Block.Quantity.ToZInt();
														currentLot.US_UQ1 = pg26Block.UnitOfMeasurePackagingLevel;
														currentLot.US_ShippingMarks = pg26Block.PackageIdentifier;
													}
													else if (pg26Block.PackagingQualifier == 2)
													{
														currentLot.US_NoOfUnit2 = pg26Block.Quantity.ToZInt();
														currentLot.US_UQ2 = pg26Block.UnitOfMeasurePackagingLevel;
													}
												}
											}
											else
											{
												var pg29Block = block as AEPAPG29;
												if (pg29Block != null)
												{
													if (currentLot != null)
													{
														currentLot.US_NetWeight = pg29Block.CommodityNetQuantityPGALineNet;
														currentLot.US_WeightUQ = Core.Constants.Weight.Pounds;
													}
												}
												else
												{
													var pg51Block = block as AEPAPG51;
													if (pg51Block != null)
													{
														currentLot = null;
													}
													else
													{
														var pg21Block = block as AEPAPG21;
														if (pg21Block != null)
														{
															if (previousRoleType == EntityRoleCodeList.Codes.CustomsBroker || previousRoleType == EntityRoleCodeList.Codes.Importer)
															{
																SetBrokerDetails(pg21Block.IndividualName, pg21Block.TelephoneNumberOfTheIndividual, pg21Block.EmailAddressOrFaxNumberForTheIndividual, notifications);
																fsis.US_CertifyingIndividual = pg21Block.IndividualQualifier;
															}
														}
														else
														{
															var pg60Block = block as AEPAPG60;
															if (pg60Block != null)
															{
																if (previousRoleType == EntityRoleCodeList.Codes.CustomsBroker)
																{
																	invoiceLine.US_FDAContactName += pg60Block.AdditionalInformation;
																}
															}
															else
															{
																var pg24Block = block as AEPAPG24;
																if (pg24Block != null)
																{
																	if (pg24Block.RemarksTypeCode == RemarksTypeCodeList.Codes.GEN)
																	{
																		sealNumbers += pg24Block.RemarksText + ",";
																	}
																}
																else
																{
																	var pg30Block = block as AEPAPG30;
																	if (pg30Block != null)
																	{
																		if (pg30Block.InspectionLaboratoryTestingStatus == InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection)
																		{
																			SetInvoiceLineFSISValues(fsis, USInvoiceLineFSISLine.Schema.US_ImportingEstNo, pg30Block.ArrivalLocation, "Import Est. on FSIS", notifications);
																			SetInvoiceLineFSISValues(fsis, USInvoiceLineFSISLine.Schema.US_DateOfInspection, pg30Block.AnticipatedArrivalDate, "Date of Inspection on FSIS", notifications);
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

			SetInvoiceLineFSISValues(fsis, USInvoiceLineFSISLine.Schema.US_SealNumbers, sealNumbers.TrimEnd(','), "Seals on FSIS", notifications);
			ProcessOrganizations(fsis, organizationsDict, notifications);
		}

		void SetInvoiceLineFSISValues(USInvoiceLineFSISLine invoiceFSIS, ZString fieldName, ZString value, ZString humanReadableName, INotifications notifications)
		{
			invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceFSIS, fieldName, value, value, humanReadableName, notifications);
		}

		void SetInvoiceLineFSISValues(USInvoiceLineFSISLine invoiceFSIS, ZString fieldName, ZDateTime value, ZString humanReadableName, INotifications notifications)
		{
			invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceFSIS, fieldName, value, value.ToLongTimeString(), humanReadableName, notifications);
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			if (roleCode == EntityRoleCodeList.Codes.Importer)
			{
				if (invoiceLine.Declaration != null && invoiceLine.Declaration.IOROrgPK.IsEmpty)
				{
					notifications.AddWarning("Importer of record should have populated from SE10 or ENS10 record. Please supply EIN/CBN/SSN of an importer of record in one of the records.");
				}
			}
			else if (roleCode == EntityRoleCodeList.Codes.Consignee)
			{
				var addressPK = FindMatchedOrgAddressPK("Importer", ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
				SetOrganizationOrAddress(OrganizationFieldType.Header, "Importer", invoiceLine.InvoiceHeader.JZ_OH_BuyerInfo, addressPK, false, notifications);
			}
		}
	}
}
