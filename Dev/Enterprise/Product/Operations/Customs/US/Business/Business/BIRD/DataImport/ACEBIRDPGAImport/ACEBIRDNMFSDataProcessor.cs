using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public abstract class ACEBIRDNMFSDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		protected ACEBIRDNMFSDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var nmfsHeader = invoiceLine.NMFSLines.AddNew();
			nmfsHeader.US_ProgramType = pg01.GovernmentAgencyProgramCode;
			nmfsHeader.HarvestingDetails.RemoveAndDeleteAll();

			NMFSHarvestingDetail currentHarvestingDetail = null;
			NMFSVessels currentHarvestingVessel = null;

			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();

			if (IsNMFSAMR && !pg01.GovernmentAgencyProcessingCode.IsEmpty)
			{
				nmfsHeader.US_Commodity = pg01.GovernmentAgencyProcessingCode;
			}

			if (IsNMFSSIMP && !pg01.ConfidentialInformationIndicator.IsEmpty)
			{
				nmfsHeader.US_Confidential = pg01.ConfidentialInformationIndicator == "Y";
			}

			foreach (var block in pgaBlocks)
			{
				var pg14Block = block as AEPAPG14;
				if (pg14Block != null)
				{
					switch (pg14Block.LPCOType)
					{
						case "NM4":
							nmfsHeader.US_IFTPPermitNumber = pg14Block.LPCONumberorName;
							break;
						case "NM2":
							nmfsHeader.US_PreApprovalIssuedNumber = pg14Block.LPCONumberorName;
							nmfsHeader.US_PreApprovalIssuedQuantity = pg14Block.LPCOQuantity;
							nmfsHeader.US_PreApprovalIssuedQuantityUQ = Core.Constants.Weight.Kilograms;
							break;
						case "NM5":
							nmfsHeader.US_EBCDNumber = pg14Block.LPCONumberorName;
							break;
						case "NM6":
							nmfsHeader.US_OtherAuthorizationNumber = pg14Block.LPCONumberorName;
							nmfsHeader.US_AuthorizationType = pg14Block.LPCOTransactionType;
							break;
					}
				}
				else
				{
					var pg22Block = block as AEPAPG22;
					if (pg22Block != null)
					{
						switch (pg22Block.DocumentIdentifier)
						{
							case NMFS370DocumentIdentifierList.Codes.NOAAForm370:
								nmfsHeader.US_DolphinSafeStatus = pg22Block.ConformanceDeclaration;
								break;
							case NMFS370DocumentIdentifierList.Codes.CaptainStatement:
								nmfsHeader.US_CaptainStatement = true;
								break;
							case NMFS370DocumentIdentifierList.Codes.ObserverStatement:
								nmfsHeader.US_ObserverStatement = true;
								break;
							case NMFS370DocumentIdentifierList.Codes.IDCPMemberNationCertification:
								nmfsHeader.US_IDCPMemberCertification = true;
								break;
							default:
								var nmfsDocument = nmfsHeader.DocumentDetails.AddNew();
								nmfsDocument.CY_Code = pg22Block.DocumentIdentifier;
								nmfsDocument.CY_Data = pg22Block.ComplianceDescription;
								break;
						}
					}
					else
					{
						var pg05Block = block as AEPAPG05;
						if (pg05Block != null)
						{
							if (IsNMFSSIMP)
							{
								currentHarvestingDetail = nmfsHeader.HarvestingDetails.AddNew();
								nmfsHeader.US_SpeciesCode = pg05Block.ScientificSpeciesCode;
							}
						}
						else
						{
							var pg50Block = block as AEPAPG50;
							if (pg50Block != null)
							{
								if (IsNMFSSIMP)
								{
									currentHarvestingVessel = currentHarvestingDetail?.HarvestingVessles.AddNew();
								}
								else
								{
									currentHarvestingDetail = nmfsHeader.HarvestingDetails.AddNew();
								}
							}
							else
							{
								var pg06Block = block as AEPAPG06;
								if (pg06Block != null)
								{
									nmfsHeader.US_SourceType = pg06Block.SourceTypeCode;
									if (currentHarvestingDetail != null)
									{
										currentHarvestingDetail.US_GearStartDate = pg06Block.ProcessingStartDate;
										currentHarvestingDetail.US_GearDescription = pg06Block.ProcessingDescription;
										currentHarvestingDetail.US_HarvestedCountry = pg06Block.CountryCode;
										currentHarvestingDetail.US_GearType = pg06Block.ProcessingTypeCode;
										if (nmfsHeader.US_SourceType == SourceTypeCodesList.Codes.HatcheryBasedAquaculture)
										{
											currentHarvestingDetail.US_GeographicLocation = pg06Block.GeographicLocation;
										}
										else
										{
											currentHarvestingDetail.US_OceanAreaOfCatch = pg06Block.GeographicLocation;
										}
									}
								}
								else
								{
									var pg10Block = block as AEPAPG10;
									if (pg10Block != null)
									{
										if (currentHarvestingDetail != null)
										{
											currentHarvestingDetail.US_ContainsYellowfinTuna = pg10Block.CategoryCode == NMFSProductCategoryCodeList.Codes.YellowfinTuna;
										}
									}
									else
									{
										var pg31Block = block as AEPAPG31;
										if (pg31Block != null)
										{
											if (IsNMFSSIMP)
											{
												if (currentHarvestingVessel != null)
												{
													switch (pg31Block.CommodityHarvestingVesselCharacteristicTypeCode)
													{
														case "VCR":
															currentHarvestingVessel.US_HarvestedCountry = pg31Block.CommodityHarvestingVesselCharacteristic;
															currentHarvestingVessel.US_NetWeight = pg31Block.HarvestedCommodityNetWeight;
															currentHarvestingVessel.US_NetWeightUQ = pg31Block.UnitOfMeasureconveyance;
															break;
														case "VNM":
															currentHarvestingVessel.US_HarvestedVessel = pg31Block.CommodityHarvestingVesselCharacteristic;
															break;
													}
												}
											}
											else
											{
												if (currentHarvestingDetail != null)
												{
													if (currentHarvestingDetail.US_VesselCountry.IsEmpty)
													{
														currentHarvestingDetail.US_VesselCountry = pg31Block.CommodityHarvestingVesselCharacteristic;
													}
													else
													{
														var clonedHarvestingDetail = (NMFSHarvestingDetail)currentHarvestingDetail.Clone();
														clonedHarvestingDetail.US_VesselCountry = pg31Block.CommodityHarvestingVesselCharacteristic;
														nmfsHeader.HarvestingDetails.Add(clonedHarvestingDetail);
													}
												}
											}
										}
										else
										{
											var pg32Block = block as AEPAPG32;
											if (pg32Block != null)
											{
												if (IsNMFSSIMP && currentHarvestingDetail != null)
												{
													var sourceType = nmfsHeader.US_SourceType;
													if (!sourceType.IsEmpty)
													{
														if (sourceType == SourceTypeCodesList.Codes.HarvestOfCaptureFisheries)
														{
															if (currentHarvestingVessel != null)
															{
																switch (pg32Block.CommodityRoutingTypeCode)
																{
																	case "11":
																		currentHarvestingVessel.US_FirstLandingCountry = pg32Block.CommodityRoutingCountryCode;
																		break;
																	case "13":
																		currentHarvestingVessel.US_TranshipmentPlace = pg32Block.CommodityRoutingCountryCode;
																		break;
																}
															}
														}
														else
														{
															switch (pg32Block.CommodityRoutingTypeCode)
															{
																case "11":
																	currentHarvestingDetail.US_FirstLandingCountry = pg32Block.CommodityRoutingCountryCode;
																	break;
															}
														}
													}
												}
											}
											else
											{
												var pg19Block = block as AEPAPG19;
												if (pg19Block != null)
												{
													if (IsNMFSSIMP)
													{
														currentHarvestingDetail.US_ContactPartyType = pg19Block.EntityRoleCode;

														switch (pg19Block.EntityRoleCode)
														{
															case EntityRoleCodeList.Codes.AquacultureFacility:
															case EntityRoleCodeList.Codes.Producer:
															case EntityRoleCodeList.Codes.Buyer:
															case EntityRoleCodeList.Codes.Consignee:
															case EntityRoleCodeList.Codes.Exporter:
															case EntityRoleCodeList.Codes.Consignor:
																previousCompanyRecord = pg19Block;
																organizationsDict[pg19Block] = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(pg19Block, null, null);
																break;
														}
													}
												}
												else
												{
													var pg20Block = block as AEPAPG20;
													if (pg20Block != null)
													{
														if (IsNMFSSIMP)
														{
															if (previousCompanyRecord != null)
															{
																var tupleValue = organizationsDict[previousCompanyRecord];
																var value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(tupleValue.Item1, pg20Block, pg20Block);
																organizationsDict[previousCompanyRecord] = value;
																previousCompanyRecord = null;
															}
														}
													}
													else
													{
														var pg29Block = block as AEPAPG29;
														if (pg29Block != null)
														{
															if (IsNMFSSIMP)
															{
																nmfsHeader.US_NetWeight = pg29Block.CommodityNetQuantityPGALineNet;
																nmfsHeader.US_NetWeightUQ = pg29Block.UnitOfMeasurePGALineNet;
															}
														}
														else
														{
															var pg24Block = block as AEPAPG24;
															if (pg24Block != null)
															{
																if (IsNMFSSIMP)
																{
																	if (!pg24Block.RemarksText.IsEmpty)
																	{
																		currentHarvestingDetail.US_NoSmallVessels = ZInt.ParseSafe(pg24Block.RemarksText, ZInt.Zero);
																	}
																}
															}
															else
															{
																var pg51Block = block as AEPAPG51;
																if (pg51Block != null)
																{
																	if (IsNMFSSIMP)
																	{
																		currentHarvestingVessel = null;
																	}
																	else
																	{
																		currentHarvestingDetail = null;
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
			ProcessOrganizations(currentHarvestingDetail, organizationsDict, notifications);
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo orgPropertyInfo = null;
			var nmfs = pga as NMFSHarvestingDetail;
			var addressPK = ZGuid.Empty;

			addressPK = FindMatchedOrgAddressPK("ORG", ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
			orgPropertyInfo = nmfs.US_OA_ContactPartyInfo;
			SetOrganizationOrAddress(OrganizationFieldType.Address, "ORG", orgPropertyInfo, addressPK, true, notifications);
		}

		protected abstract ZBool IsNMFS370 { get; }
		protected abstract ZBool IsNMFSAMR { get; }
		protected abstract ZBool IsNMFSHMS { get; }
		protected abstract ZBool IsNMFSSIMP { get; }
	}

	public class ACEBIRDNMFS370DataProcessor : ACEBIRDNMFSDataProcessor
	{
		public ACEBIRDNMFS370DataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_NMFS370DisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_NMFS370IndInfo; }
		}

		public override ZString PGAName
		{
			get { return "NMFS 370"; }
		}

		protected override ZBool IsNMFS370
		{
			get { return true; }
		}

		protected override ZBool IsNMFSAMR
		{
			get { return false; }
		}

		protected override ZBool IsNMFSHMS
		{
			get { return false; }
		}

		protected override ZBool IsNMFSSIMP
		{
			get { return false; }
		}
	}

	public class ACEBIRDNMFSAMRDataProcessor : ACEBIRDNMFSDataProcessor
	{
		public ACEBIRDNMFSAMRDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_NMFSAMRDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_NMFSAMRIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "NMFS AMR"; }
		}

		protected override ZBool IsNMFS370
		{
			get { return false; }
		}

		protected override ZBool IsNMFSAMR
		{
			get { return true; }
		}

		protected override ZBool IsNMFSHMS
		{
			get { return false; }
		}

		protected override ZBool IsNMFSSIMP
		{
			get { return false; }
		}
	}

	public class ACEBIRDNMFSHMSDataProcessor : ACEBIRDNMFSDataProcessor
	{
		public ACEBIRDNMFSHMSDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_NMFSHMSDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_NMFSHMSIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "NMFS HMS"; }
		}

		protected override ZBool IsNMFS370
		{
			get { return false; }
		}

		protected override ZBool IsNMFSAMR
		{
			get { return false; }
		}

		protected override ZBool IsNMFSHMS
		{
			get { return true; }
		}

		protected override ZBool IsNMFSSIMP
		{
			get { return false; }
		}
	}

	public class ACEBIRDNMFSSIMPDataProcessor : ACEBIRDNMFSDataProcessor
	{
		public ACEBIRDNMFSSIMPDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return null; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_NMFSSIMPIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "NMFS SIMP"; }
		}

		protected override ZBool IsNMFS370
		{
			get { return false; }
		}

		protected override ZBool IsNMFSAMR
		{
			get { return false; }
		}

		protected override ZBool IsNMFSHMS
		{
			get { return false; }
		}

		protected override ZBool IsNMFSSIMP
		{
			get { return true; }
		}
	}
}
