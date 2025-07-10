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
	public class ACEBIRDFWSDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDFWSDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_FWSDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_FWSIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "FWS"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var fwsHeader = invoiceLine.FWSHeaders.AddNew();
			fwsHeader.US_ProcessingCode = pg01.GovernmentAgencyProcessingCode;
			fwsHeader.US_IsDocSubmitted = pg01.ElectronicImageSubmitted == "Y";
			fwsHeader.US_ProductType = pg01.GloballyUniqueProductIdentificationCodeQualifier;
			fwsHeader.US_ProductNumber = pg01.GloballyUniqueProductIdentificationCode;
			fwsHeader.US_IntendedUseCode = pg01.IntendedUseCode;
			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();

			foreach (var block in pgaBlocks)
			{
				var pg05Block = block as AEPAPG05;
				if (pg05Block != null)
				{
					if (fwsHeader.US_ScientificGenusName.IsEmpty && fwsHeader.US_ScientificSpeciesName.IsEmpty && fwsHeader.US_ScientificSubSpeciesName.IsEmpty)
					{
						fwsHeader.US_ScientificGenusName = pg05Block.ScientificGenusName;
						fwsHeader.US_ScientificSpeciesName = pg05Block.ScientificSpeciesName;
						fwsHeader.US_ScientificSubSpeciesName = pg05Block.ScientificSubSpeciesName;
					}
					else
					{
						fwsHeader.US_Scientific2GenusName = pg05Block.ScientificGenusName;
						fwsHeader.US_Scientific2SpeciesName = pg05Block.ScientificSpeciesName;
						fwsHeader.US_Scientific2SubSpeciesName = pg05Block.ScientificSubSpeciesName;
					}

					if (!pg05Block.ScientificSpeciesCode.IsEmpty)
					{
						fwsHeader.US_WildlifeCategoryCode = pg05Block.ScientificSpeciesCode;
					}

					if (!pg05Block.FWSDescriptionCode.IsEmpty)
					{
						fwsHeader.US_WildlifeDescriptionCode = pg05Block.FWSDescriptionCode;
					}
				}
				else
				{
					var pg06Block = block as AEPAPG06;
					if (pg06Block != null)
					{
						if (pg06Block.SourceTypeCode == SourceTypeCodesList.Codes.CountryOfSpeciesOrigin)
						{
							fwsHeader.US_SpeciesOrigin = pg06Block.CountryCode;
						}
					}
					else
					{
						var pg10Block = block as AEPAPG10;
						if (pg10Block != null)
						{
							if (pg10Block.CommodityCharacteristicQualifier.IsEmpty)
							{
								if (fwsHeader.US_WildlifeSource.IsEmpty)
								{
									fwsHeader.US_WildlifeSource = pg10Block.CommodityQualifierCode;
								}
								else
								{
									fwsHeader.US_Hybrid = pg10Block.CommodityQualifierCode;
								}
							}
						}
						else
						{
							var pg14Block = block as AEPAPG14;
							if (pg14Block != null)
							{
								var currentLicense = fwsHeader.Licenses.AddNew();
								currentLicense.US_Type = pg14Block.LPCOType;
								currentLicense.US_Number = pg14Block.LPCONumberorName;
							}
							else
							{
								var pg17Block = block as AEPAPG17;
								if (pg17Block != null)
								{
									fwsHeader.US_CommoditySpecificName = pg17Block.CommonNameSpecific;
									fwsHeader.US_CommodityGeneralName = pg17Block.CommonNameGeneral;
									fwsHeader.US_IsLiveVenomous = pg17Block.LiveVenomousWildlifeCode == YesNoDefaultList.Codes.Yes;
									fwsHeader.US_CartonQty = (ZShort)pg17Block.CartonsContainingWildlife;
								}
								else
								{
									var pg19Block = block as AEPAPG19;
									if (pg19Block != null)
									{
										switch (pg19Block.EntityRoleCode)
										{
											case EntityRoleCodeList.Codes.FWSImporter:
											case EntityRoleCodeList.Codes.FWSForeignExporter:
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
											var pg24Block = block as AEPAPG24;
											if (pg24Block != null)
											{
												if (pg24Block.RemarksTypeCode == RemarksTypeCodeList.Codes.GEN)
												{
													fwsHeader.US_RemarksText += pg24Block.RemarksText;
												}
											}
											else
											{
												var pg25Block = block as AEPAPG25;
												if (pg25Block != null)
												{
													fwsHeader.US_InvCurrPGAValue = pg25Block.PGALineValue;
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
													else
													{
														var pg29Block = block as AEPAPG29;
														if (pg29Block != null)
														{
															fwsHeader.US_NetCommodityUQ = pg29Block.UnitOfMeasurePGALineNet;
															fwsHeader.US_NetCommodity = pg29Block.CommodityNetQuantityPGALineNet;
														}
														else
														{
															var pg30Block = block as AEPAPG30;
															if (pg30Block != null)
															{
																if (pg30Block.AnticipatedArrivalLocationCode == InspectionLocationCodeList.Codes.FIRMS)
																{
																	fwsHeader.US_FIRMS = pg30Block.ArrivalLocation;
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

			ProcessOrganizations(fwsHeader, organizationsDict, notifications);
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
				case EntityIdentificationCodesList.Codes.DUNSNumber:
					result = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
					break;
			}

			return result;
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo addressPropertyInfo = null;
			var fwsHeader = pga as FWSHeader;
			ZString organizationCode = ZString.Empty;

			if (roleCode == EntityRoleCodeList.Codes.FWSForeignExporter)
			{
				organizationCode = "FWS Exporter";
				addressPropertyInfo = fwsHeader.US_OA_FWSExporterAddressInfo;
			}
			else if (roleCode == EntityRoleCodeList.Codes.FWSImporter)
			{
				organizationCode = "FWS Importer";
				addressPropertyInfo = fwsHeader.US_OA_FWSImporterAddressInfo;
			}

			if (!organizationCode.IsEmpty)
			{
				var addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
				SetOrganizationOrAddress(OrganizationFieldType.Address, organizationCode, addressPropertyInfo, addressPK, true, notifications);
			}
		}
	}
}
