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
	public class ACEBIRDCPSCDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDCPSCDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_CPSCDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_CPSCIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "CPSC"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
			cpscHeader.US_ProcessingCode = pg01.GovernmentAgencyProcessingCode;
			cpscHeader.US_ProductIDType = pg01.GloballyUniqueProductIdentificationCodeQualifier;
			cpscHeader.US_ProductID = pg01.GloballyUniqueProductIdentificationCode;
			cpscHeader.US_IntendedUseCode = pg01.IntendedUseCode;
			cpscHeader.US_IntendedUseDescription = pg01.IntendedUseDescription;

			ZPropertyInfo currentNumberInfoForPG07 = null;
			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			CPSCRule currentRuleAndLab = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
			var ruleAndLabsOrganizationDict = new Dictionary<CPSCRule, Tuple<IACEBIRDOrgCompanyRecord, IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();

			foreach (var block in pgaBlocks)
			{
				var pg02Block = block as AEPAPG02;
				if (pg02Block != null)
				{
					if (pg02Block.ItemType == PG02ItemTypeList.Codes.Product && pg02Block.ProductCodeQualifier == "SKU")
					{
						cpscHeader.US_SKUProductCode = pg02Block.ProductCodeNumber;
					}
				}
				else
				{
					var pg07Block = block as AEPAPG07;
					if (pg07Block != null)
					{
						if (cpscHeader.US_TradeBrandName.IsEmpty)
						{
							cpscHeader.US_TradeBrandName = pg07Block.TradeNameBrandName;
						}

						if (cpscHeader.US_ProductName.IsEmpty)
						{
							cpscHeader.US_ProductName = pg07Block.Model;
						}

						switch (pg07Block.ItemIdentityNumberQualifier)
						{
							case ItemIdentityNumberQualifierList.Codes.ModelNumber:
								currentNumberInfoForPG07 = cpscHeader.US_ModelNumberInfo;
								break;
							case ItemIdentityNumberQualifierList.Codes.SerialNumber:
								currentNumberInfoForPG07 = cpscHeader.US_SerialNumberInfo;
								break;
							case ItemIdentityNumberQualifierList.Codes.RegisteredNumber:
								currentNumberInfoForPG07 = cpscHeader.US_RegisteredNumberInfo;
								break;
							case ItemIdentityNumberQualifierList.Codes.AlternateIdentifier:
								currentNumberInfoForPG07 = cpscHeader.US_AltenateIDInfo;
								break;
							default:
								currentNumberInfoForPG07 = null;
								break;
						}

						if (currentNumberInfoForPG07 != null)
						{
							ZString newValue = (ZString)currentNumberInfoForPG07.Value + "," + pg07Block.ItemIdentityNumber;
							currentNumberInfoForPG07.Value = newValue.TrimStart(',');
						}
					}
					else
					{
						var pg08Block = block as AEPAPG08;
						if (pg08Block != null)
						{
							if (currentNumberInfoForPG07 != null)
							{
								var numberValue = (ZString)currentNumberInfoForPG07.Value;
								if (!pg08Block.ItemIdentityNumber.IsEmpty)
								{
									numberValue += "," + pg08Block.ItemIdentityNumber;
								}

								if (!pg08Block.ItemIdentityNumber1.IsEmpty)
								{
									numberValue += "," + pg08Block.ItemIdentityNumber1;
								}

								if (!pg08Block.ItemIdentityNumber2.IsEmpty)
								{
									numberValue += "," + pg08Block.ItemIdentityNumber2;
								}

								if (!pg08Block.ItemIdentityNumber3.IsEmpty)
								{
									numberValue += "," + pg08Block.ItemIdentityNumber3;
								}

								currentNumberInfoForPG07.Value = numberValue.TrimStart(',');
							}
						}
						else
						{
							var pg10Block = block as AEPAPG10;
							if (pg10Block != null)
							{
								switch (pg10Block.CommodityCharacteristicQualifier)
								{
									case CPSCCommodityCharacteristicQualifiersList.Codes.ModelColor:
										cpscHeader.US_ModelColor = pg10Block.CommodityCharacteristicDescription;
										break;
									case CPSCCommodityCharacteristicQualifiersList.Codes.ModelDescription:
										cpscHeader.US_ModelDescription = pg10Block.CommodityCharacteristicDescription;
										break;
									case CPSCCommodityCharacteristicQualifiersList.Codes.ModelStyle:
										cpscHeader.US_ModelStyle = pg10Block.CommodityCharacteristicDescription;
										break;
								}
							}
							else
							{
								var pg14Block = block as AEPAPG14;
								if (pg14Block != null)
								{
									cpscHeader.US_ReferenceNumber = pg14Block.LPCONumberorName;
								}
								else
								{
									var pg19Block = block as AEPAPG19;
									if (pg19Block != null)
									{
										currentRuleAndLab = null;
										var roleCode = pg19Block.EntityRoleCode;
										if (roleCode == EntityRoleCodeList.Codes.NoLabTestingRequired)
										{
											cpscHeader.US_NoLabTestingRequired = true;
										}
										else if (roleCode == EntityRoleCodeList.Codes.ManufacturerOfGoods)
										{
											organizationsDict[pg19Block] = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(pg19Block, null, null);
										}
										else if (roleCode == EntityRoleCodeList.Codes.IndependentThirdPartyLaboratory)
										{
											currentRuleAndLab = cpscHeader.RuleAndLabs.AddNew();
											currentRuleAndLab.US_CPSCAccreditedLabID = pg19Block.EntityNumber;
										}
										else if (roleCode == EntityRoleCodeList.Codes.Laboratory)
										{
											currentRuleAndLab = cpscHeader.RuleAndLabs.AddNew();
											ruleAndLabsOrganizationDict[currentRuleAndLab] = new Tuple<IACEBIRDOrgCompanyRecord, IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(pg19Block, pg19Block, null, null);
										}

										previousCompanyRecord = pg19Block;
									}
									else
									{
										var pg20Block = block as AEPAPG20;
										if (pg20Block != null)
										{
											if (previousCompanyRecord != null)
											{
												var roleCode = previousCompanyRecord.OrganizationType;
												if (roleCode == EntityRoleCodeList.Codes.ManufacturerOfGoods && organizationsDict.ContainsKey(previousCompanyRecord))
												{
													var tupleValue = organizationsDict[previousCompanyRecord];
													var value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(tupleValue.Item1, pg20Block, pg20Block);
													organizationsDict[previousCompanyRecord] = value;
												}
												else if (roleCode == EntityRoleCodeList.Codes.Laboratory && ruleAndLabsOrganizationDict.ContainsKey(currentRuleAndLab))
												{
													var tupleValue = ruleAndLabsOrganizationDict[currentRuleAndLab];
													var value = new Tuple<IACEBIRDOrgCompanyRecord, IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(tupleValue.Item1, tupleValue.Item2, pg20Block, pg20Block);
													ruleAndLabsOrganizationDict[currentRuleAndLab] = value;
												}
											}
										}
										else
										{
											var pg06Block = block as AEPAPG60;
											if (pg06Block != null)
											{
												if (pg06Block.AdditionalInformationQualifierCode == "CIT")
												{
													var ruleCode = pg06Block.AdditionalInformation;
													if (previousCompanyRecord != null && previousCompanyRecord.OrganizationType == EntityRoleCodeList.Codes.NoLabTestingRequired)
													{
														currentRuleAndLab = cpscHeader.RuleAndLabs.OfType<CPSCRule>().LastOrDefault();
														if (currentRuleAndLab == null || currentRuleAndLab.US_RuleCodes.Split(',').Length > 10)
														{
															currentRuleAndLab = cpscHeader.RuleAndLabs.AddNew();
														}
													}

													if (currentRuleAndLab != null)
													{
														var currentValue = currentRuleAndLab.US_RuleCodes;
														if (!currentValue.Contains(ruleCode, StringComparison.OrdinalIgnoreCase))
														{
															currentValue += "," + ruleCode;
														}
														currentRuleAndLab.US_RuleCodes = currentValue.TrimStart(',');
													}
												}
											}
											else
											{
												var pg22Block = block as AEPAPG22;
												if (pg22Block != null)
												{
													cpscHeader.US_CertificateExists = pg22Block.DeclarationCode == "CPY" ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
												}
												else
												{
													var pg25Block = block as AEPAPG25;
													if (pg25Block != null)
													{
														var lot = cpscHeader.Lots.AddNew();
														lot.US_LotNumberType = pg25Block.LotNumberQualifier;
														lot.US_LotNumber = pg25Block.LotNumber;
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

			ProcessOrganizations(cpscHeader, organizationsDict, notifications);

			foreach (var ruleAndLabs in ruleAndLabsOrganizationDict)
			{
				var ruleAndLabOrgDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
				if (ruleAndLabs.Key != null && ruleAndLabs.Value != null)
				{
					ruleAndLabOrgDict[ruleAndLabs.Value.Item1] = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(ruleAndLabs.Value.Item2, ruleAndLabs.Value.Item3, ruleAndLabs.Value.Item4);
					ProcessOrganizations(ruleAndLabs.Key, ruleAndLabOrgDict, notifications);
				}
			}
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo orgPropertyInfo = null;
			var cpscHeader = pga as CPSCHeader;
			var cpscRule = pga as CPSCRule;
			var organizationCode = ZString.Empty;

			if (roleCode == EntityRoleCodeList.Codes.ManufacturerOfGoods && cpscHeader != null)
			{
				orgPropertyInfo = cpscHeader.US_OA_ManufacturerAddressInfo;
				organizationCode = "Manufacturer on CPSC";
			}
			else if (roleCode == EntityRoleCodeList.Codes.Laboratory && cpscRule != null)
			{
				orgPropertyInfo = cpscRule.US_OA_SafetyTestLocationAddressInfo;
				organizationCode = "Safety Test Location";
			}

			var addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
			if (!organizationCode.IsEmpty && !addressPK.IsEmpty)
			{
				SetOrganizationOrAddress(OrganizationFieldType.Address, organizationCode, orgPropertyInfo, addressPK, true, notifications);
			}
		}
	}
}
