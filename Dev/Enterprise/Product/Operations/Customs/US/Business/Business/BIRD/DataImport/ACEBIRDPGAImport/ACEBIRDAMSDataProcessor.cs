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
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ACEBIRDAMSDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDAMSDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_AMSDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_AMSIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "AMS"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = pg01.GovernmentAgencyProgramCode + pg01.GovernmentAgencyProcessingCode;
			amsHeader.US_IntendedUseCode = pg01.IntendedUseCode;
			amsHeader.US_IntendedUseDescription = pg01.IntendedUseDescription;
			var amsLine = amsHeader.AMSLines.AddNew();
			amsLine.US_IsDocSubmitted = pg01.ElectronicImageSubmitted == "Y";

			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();

			foreach (var block in pgaBlocks)
			{
				var pg02Block = block as AEPAPG02;
				if (pg02Block != null)
				{
					if (pg02Block.ProductCodeQualifier == ProductCodeQualifiersList.Codes.UNStandardProductsServicesCode)
					{
						amsLine.US_ProductNumber = pg02Block.ProductCodeNumber;
					}
				}
				else
				{
					var pg10Block = block as AEPAPG10;
					if (pg10Block != null)
					{
						amsHeader.US_CommercialDescription = pg10Block.CommodityCharacteristicDescription;
					}
					else
					{
						var pg19Block = block as AEPAPG19;
						if (pg19Block != null)
						{
							switch (pg19Block.EntityRoleCode)
							{
								case EntityRoleCodeList.Codes.Applicant:
								case EntityRoleCodeList.Codes.AMSApplicant:
								case EntityRoleCodeList.Codes.LocationOfGoodsImmediatelyAfterEntryRelease:
								case EntityRoleCodeList.Codes.Importer:
								case EntityRoleCodeList.Codes.Consignee:
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
								else
								{
									var pg26Block = block as AEPAPG26;
									if (pg26Block != null)
									{
										if (amsHeader.US_Program != AMSProgramList.Codes.EG1)
										{
											switch (pg26Block.PackagingQualifier)
											{
												case 1:
													amsLine.US_Packages = pg26Block.Quantity;
													amsLine.US_PackagesUQ = pg26Block.UnitOfMeasurePackagingLevel;
													break;
												case 2:
													if (amsLine.US_PackageWeight.IsEmpty)
													{
														amsLine.US_PackageWeight = pg26Block.Quantity;
														amsLine.US_PackageWeightUQ = pg26Block.UnitOfMeasurePackagingLevel;
													}
													else
													{
														amsLine.US_QtyPerPackage = pg26Block.Quantity;
														amsLine.US_QtyPerPackageUQ = pg26Block.UnitOfMeasurePackagingLevel;
													}
													break;
											}
										}
										else
										{
											if (amsLine.US_OuterPackage.IsEmpty)
											{
												amsLine.US_OuterPackage = pg26Block.Quantity;
												amsLine.US_OuterPackageUQ = pg26Block.UnitOfMeasurePackagingLevel;
											}
											else if (amsLine.US_InnerPackage.IsEmpty)
											{
												amsLine.US_InnerPackage = pg26Block.Quantity;
												amsLine.US_InnerPackageUQ = pg26Block.UnitOfMeasurePackagingLevel;
											}
											else if (amsLine.US_InnerAmount.IsEmpty)
											{
												amsLine.US_InnerAmount = pg26Block.Quantity;
												amsLine.US_InnerAmountUQ = pg26Block.UnitOfMeasurePackagingLevel;
											}
											else if (amsLine.US_InnerWeight.IsEmpty)
											{
												amsLine.US_InnerWeight = pg26Block.Quantity;
												amsLine.US_InnerWeightUQ = pg26Block.UnitOfMeasurePackagingLevel;
											}
										}
									}
									else
									{
										var pg29Block = block as AEPAPG29;
										if (pg29Block != null)
										{
											if (amsHeader.US_Program != AMSProgramList.Codes.EG1)
											{
												amsLine.US_NetWeight = pg29Block.CommodityNetQuantityPGALineNet;
												amsLine.US_NetWeightUQ = pg29Block.UnitOfMeasurePGALineNet;
											}
											else
											{
												if (amsLine.US_TotalQuantity.IsEmpty)
												{
													amsLine.US_TotalQuantity = pg29Block.CommodityNetQuantityPGALineNet;
													amsLine.US_TotalQuantityUQ = pg29Block.UnitOfMeasurePGALineNet;
												}
												else
												{
													amsLine.US_TotalWeight = pg29Block.CommodityNetQuantityPGALineNet;
													amsLine.US_TotalWeightUQ = pg29Block.UnitOfMeasurePGALineNet;
												}
											}
										}
										else
										{
											var pg30Block = block as AEPAPG30;
											if (pg30Block != null)
											{
												var dateTime = DateTimeParser.GetDateTimeFromZDateAndStringTime(pg30Block.AnticipatedArrivalDate, pg30Block.ArrivalTime);
												switch (pg30Block.InspectionLaboratoryTestingStatus)
												{
													case InspectionStatusList.Codes.ProductLocationForRegulatoryAuthorityInspection:
														amsLine.US_InspecDateTime = dateTime.IsValid ? dateTime : ZDateTime.Empty;
														amsLine.US_InspecRemarks = pg30Block.ArrivalLocation;
														break;
													case InspectionStatusList.Codes.BTAAnticipatedArrivalInformation:
														invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.Declaration, JobDeclaration.Schema.US_FDAADTA, dateTime, dateTime.ToLongTimeString(), "PGA Arrival Date/Time", notifications);
														break;
												}
											}
											else
											{
												var pg13Block = block as AEPAPG13;
												if (pg13Block != null)
												{
													switch (pg13Block.LPCOIssuerGovernmentGeographicCodeQualifier)
													{
														case "PR":
															amsLine.US_InspectionLocation = pg13Block.LocationCountryStateProvinceOfIssuerOfTheLPCO;
															amsLine.US_Party = pg13Block.IssuerOfLPCO;
															break;
													}
												}
												else
												{
													var pg14Block = block as AEPAPG14;
													if (pg14Block != null)
													{
														if (pg14Block.LPCOType != LPCOTypeList.Codes.AM2 && pg14Block.LPCOType != LPCOTypeList.Codes.AM3)
														{
															amsLine.US_CertType = pg14Block.LPCOType;
															amsLine.US_CertNumber = pg14Block.LPCONumberorName;
															amsLine.US_Weight = pg14Block.LPCOQuantity;
															amsLine.US_WeightUQ = pg14Block.LPCOUnitOfMeasure;
															amsLine.US_IssueDate = pg14Block.LPCODate;
														}
														else if (pg14Block.LPCOType == LPCOTypeList.Codes.AM2)
														{
															amsLine.US_AuthorizationNumber = pg14Block.LPCONumberorName;
															amsLine.US_NetWeight = pg14Block.LPCOQuantity;
															amsLine.US_NetWeightUQ = pg14Block.LPCOUnitOfMeasure;
														}
														else if (pg14Block.LPCOType == LPCOTypeList.Codes.AM3)
														{
															amsLine.US_PermitNumber = pg14Block.LPCONumberorName;
														}
													}
													else
													{
														var pg25Block = block as AEPAPG25;
														if (pg25Block != null)
														{
															var lot = amsLine.LotCodes.AddNew();
															lot.CY_Code = pg25Block.LotNumberQualifier;
															lot.CY_Data = pg25Block.LotNumber;
														}
														else
														{
															var pg27Block = block as AEPAPG27;
															if (pg27Block != null)
															{
																AddContainerToDeclaration(pg27Block.ContainerNumberEquipmentID);
																AddContainerToDeclaration(pg27Block.ContainerNumberEquipmentID1);
																AddContainerToDeclaration(pg27Block.ContainerNumberEquipmentID2);
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

			ProcessOrganizations(amsLine, organizationsDict, notifications);
		}

		protected override void ProcessDisclaimedPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			base.ProcessDisclaimedPGABlocks(pg01, pgaBlocks, notifications);

			invoiceLine.US_AMSDisclaimProgram = (pg01.GovernmentAgencyProgramCode + pg01.GovernmentAgencyProcessingCode).Substring(0, USAddInfoSchema.US_AMSDisclaimProgram.MaxLength);

			foreach (var block in pgaBlocks)
			{
				var pg30Block = block as AEPAPG30;
				if (pg30Block != null)
				{
					var dateTime = DateTimeParser.GetDateTimeFromZDateAndStringTime(pg30Block.AnticipatedArrivalDate, pg30Block.ArrivalTime);
					invoiceLine.Factory.GetCachedValue<BIRDUpdateHeaderHelperTool>().UpdateOrWarn(invoiceLine.Declaration, JobDeclaration.Schema.US_FDAADTA, dateTime, dateTime.ToLongTimeString(), "PGA Arrival Date/Time", notifications);
				}
			}
		}

		void AddContainerToDeclaration(ZString containerNumber)
		{
			if (!containerNumber.IsEmpty && !invoiceLine.Declaration.CusContainers.OfType<CusContainer>().Any(x => x.CO_ContainerNumber == containerNumber))
			{
				invoiceLine.Declaration.CusContainers.AddNew().CO_ContainerNumber = containerNumber;
			}
		}

		protected override ZString GetMatchedCustomsNoTypeInOrganization(ZString customsNoTypeInMessage)
		{
			var result = ZString.Empty;

			switch (customsNoTypeInMessage)
			{
				case "331":
					result = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
					break;
			}

			return result;
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo orgPropertyInfo = null;
			var amsLine = pga as AMSLine;
			var organizationCode = ZString.Empty;
			var addressPK = ZGuid.Empty;
			var overrideExisting = true;

			if (roleCode == EntityRoleCodeList.Codes.Applicant || roleCode == EntityRoleCodeList.Codes.AMSApplicant)
			{
				orgPropertyInfo = amsLine.US_OA_ApplicantInfo;
				organizationCode = "AMS Applicant";
				addressPK = FindMatchedOrgAddressPK(organizationCode, ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
			}
			else if (roleCode == EntityRoleCodeList.Codes.LocationOfGoodsImmediatelyAfterEntryRelease)
			{
				orgPropertyInfo = amsLine.US_OA_GoodsLocationInfo;
				organizationCode = "AMS Goods Location";
				addressPK = FindMatchedOrgAddressPK(organizationCode, ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
			}
			else if (roleCode == EntityRoleCodeList.Codes.Importer)
			{
				if (invoiceLine.Declaration != null && invoiceLine.Declaration.IOROrgPK.IsEmpty)
				{
					notifications.AddWarning("Importer of record should have populated from SE10 or ENS10 record. Please supply EIN/CBN/SSN of an importer of record in one of the records.");
				}
			}
			else if (roleCode == EntityRoleCodeList.Codes.Consignee)
			{
				orgPropertyInfo = invoiceLine.JI_OA_ConsigneeAddressInfo;
				organizationCode = "Consignee";
				overrideExisting = false;
				addressPK = FindMatchedOrgAddressPK("Consignee", customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
			}

			if (!organizationCode.IsEmpty)
			{
				SetOrganizationOrAddress(OrganizationFieldType.Address, organizationCode, orgPropertyInfo, addressPK, overrideExisting, notifications);
			}
		}
	}
}
