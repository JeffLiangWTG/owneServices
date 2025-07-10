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
	class ACEBIRDVNEDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDVNEDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_VNEDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_VNEIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "VNE"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_VNEElectronicImage = pg01.ElectronicImageSubmitted == "Y";

			var vehicleNumberType = ZString.Empty;
			var previousEntityRole = ZString.Empty;
			VehicleDetails currentVehicleDetailsLine = null;
			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
			var pg22Block = pgaBlocks.OfType<AEPAPG22>().FirstOrDefault();
			if (pg22Block != null)
			{
				vehicleLine.US_FormType = pg22Block.DeclarationCode == "EP1" ? EPAVNEDocumentIdentifierList.Codes.EPA3520_21 : EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			}

			foreach (var block in pgaBlocks)
			{
				var pg24Block = block as AEPAPG24;
				if (pg24Block != null)
				{
					switch (pg24Block.RemarksTypeCode)
					{
						case RemarksTypeCodeList.Codes.EP1:
							vehicleLine.US_BondExemption = pg24Block.RemarksCode == RemarksCodeList.Codes.E1Y ? YesNoDefaultList.Codes.Yes : string.Empty;
							break;
						case RemarksTypeCodeList.Codes.EP2:
							vehicleLine.US_ImportCode = pg24Block.RemarksCode;
							break;
						case RemarksTypeCodeList.Codes.EP3:
							vehicleLine.US_IndustryCode = pg24Block.RemarksCode;
							break;
						case RemarksTypeCodeList.Codes.EP4:
							vehicleLine.US_ExemptionRemarks = pg24Block.RemarksText;
							break;
						case RemarksTypeCodeList.Codes.GEN:
							vehicleLine.US_Remarks = pg24Block.RemarksText;
							break;
					}
				}
				else
				{
					var pg50Block = block as AEPAPG50;
					if (pg50Block != null)
					{
						vehicleNumberType = ZString.Empty;
					}
					else
					{
						var pg07Block = block as AEPAPG07;
						if (pg07Block != null)
						{
							if (vehicleNumberType.IsEmpty || vehicleNumberType == pg07Block.ItemIdentityNumberQualifier)
							{
								currentVehicleDetailsLine = vehicleLine.VehicleAndEngineDetails.AddNew();
							}

							vehicleNumberType = pg07Block.ItemIdentityNumberQualifier;

							if (pg07Block.ItemIdentityNumberQualifier == ItemIdentityNumberQualifierList.Codes.EngineNumber)
							{
								currentVehicleDetailsLine.US_EngineModel = pg07Block.Model;
								ZDateTime engineBuildDate;
								ZDateTime.TryParseExact(pg07Block.ManufactureMonthAndYear, out engineBuildDate, "MMyyyy");
								currentVehicleDetailsLine.US_EngineBuildDate = engineBuildDate.IsValid ? engineBuildDate : ZDateTime.Empty;
								currentVehicleDetailsLine.US_EngineNumber = pg07Block.ItemIdentityNumber;
							}
							else
							{
								vehicleLine.US_VehicleModel = pg07Block.Model;
								currentVehicleDetailsLine.US_BuildMonth = pg07Block.ManufactureMonthAndYear.Length > 4 ? pg07Block.ManufactureMonthAndYear.Substring(0, 2) : ZString.Empty;
								currentVehicleDetailsLine.US_BuildYear = pg07Block.ManufactureMonthAndYear.Length > 4 ? pg07Block.ManufactureMonthAndYear.Substring(2) : pg07Block.ManufactureMonthAndYear;
								currentVehicleDetailsLine.US_IdentityNumberQualifier = pg07Block.ItemIdentityNumberQualifier == "AKG" ? ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN : (string)pg07Block.ItemIdentityNumberQualifier;
								currentVehicleDetailsLine.US_IdentityNumber = pg07Block.ItemIdentityNumber;
							}
						}
						else
						{
							var pg08Block = block as AEPAPG08;
							if (pg08Block != null)
							{
								CreateAdditionalNumberFromPG08(pg08Block.ItemIdentityNumber, vehicleNumberType, currentVehicleDetailsLine);
								CreateAdditionalNumberFromPG08(pg08Block.ItemIdentityNumber1, vehicleNumberType, currentVehicleDetailsLine);
								CreateAdditionalNumberFromPG08(pg08Block.ItemIdentityNumber2, vehicleNumberType, currentVehicleDetailsLine);
								CreateAdditionalNumberFromPG08(pg08Block.ItemIdentityNumber3, vehicleNumberType, currentVehicleDetailsLine);
							}
							else
							{
								var pg10Block = block as AEPAPG10;
								if (pg10Block != null)
								{
									switch (pg10Block.CommodityQualifierCode)
									{
										case CommodityVehicleQualifierCodesList.Codes.V05:
											if (currentVehicleDetailsLine != null)
											{
												currentVehicleDetailsLine.US_BuildDateExplanation = pg10Block.CommodityCharacteristicDescription;
												currentVehicleDetailsLine.US_MfrDateType = pg10Block.CommodityCharacteristicQualifier;
											}
											break;
										case CommodityVehicleQualifierCodesList.Codes.V03:
											var maxEnginePower = pg10Block.CommodityCharacteristicDescription;
											var maxEnginePowweUQ = pg10Block.CommodityCharacteristicQualifier;
											var enginePowerUQ = maxEnginePowweUQ.IsEmpty ? maxEnginePower.SubstringSafe(maxEnginePower.Length - 2) : maxEnginePowweUQ;
											var enginePower = !enginePowerUQ.IsNumbersOnlyOrEmpty ? maxEnginePower.SubstringSafe(0, maxEnginePower.Length - 2) : maxEnginePower;
											vehicleLine.US_EnginePower = ZDecimal.ParseSafe(enginePower, 0m);
											vehicleLine.US_EnginePowerUQ = !enginePowerUQ.IsNumbersOnlyOrEmpty ? enginePowerUQ : ZString.Empty;
											break;
										case CommodityVehicleQualifierCodesList.Codes.V00:
										case CommodityVehicleQualifierCodesList.Codes.V02:
											vehicleLine.US_BodyType = pg10Block.CommodityQualifierCode;
											vehicleLine.US_BodyDescription = pg10Block.CommodityCharacteristicDescription;
											vehicleLine.US_BodyCode = pg10Block.CommodityCharacteristicQualifier;
											break;
										case CommodityVehicleQualifierCodesList.Codes.V06:
											vehicleLine.US_ModelYear = pg10Block.CommodityCharacteristicDescription;
											break;
										case CommodityVehicleQualifierCodesList.Codes.V01:
											vehicleLine.US_DrvSide = pg10Block.CommodityCharacteristicDescription;
											break;
										case CommodityVehicleQualifierCodesList.Codes.V04:
											vehicleLine.US_MilitaryEq = pg10Block.CommodityCharacteristicDescription == "Y";
											break;
									}
								}
								else
								{
									var pg51Block = block as AEPAPG51;
									if (pg51Block != null)
									{
										vehicleNumberType = ZString.Empty;
									}
									else
									{
										var pg14Block = block as AEPAPG14;
										if (pg14Block != null)
										{
											switch (pg14Block.LPCOTransactionType)
											{
												case LPCOTransactionTypeList.Codes.General:
													if (pg14Block.LPCOType == LPCOTypeList.Codes.EP4)
													{
														if (pg14Block.LPCODateQualifier.IsEmpty && vehicleLine.US_CBPBondNumber.IsEmpty)
														{
															vehicleLine.US_CBPBondNumber = pg14Block.LPCONumberorName;
														}
														else
														{
															vehicleLine.US_CertOfConformity = pg14Block.LPCONumberorName;
															vehicleLine.US_CertOfConformityExpiryDate = pg14Block.LPCODate;
														}
													}
													else if (pg14Block.LPCOType == LPCOTypeList.Codes.EP7)
													{
														vehicleLine.US_BondPolicyNo = pg14Block.LPCONumberorName;
													}
													break;
												case LPCOTransactionTypeList.Codes.SingleUse:
													if (pg14Block.LPCOType == LPCOTypeList.Codes.EP9)
													{
														vehicleLine.US_VehicleExemptionNumber = pg14Block.LPCONumberorName;
													}
													else if (pg14Block.LPCOType == LPCOTypeList.Codes.EP4)
													{
														vehicleLine.US_EPARegNumber = pg14Block.LPCONumberorName;
													}
													break;
												default:
													if (pg14Block.LPCOType == LPCOTypeList.Codes.EP4)
													{
														if (pg14Block.LPCODateQualifier.IsEmpty && vehicleLine.US_CBPBondNumber.IsEmpty)
														{
															vehicleLine.US_CBPBondNumber = pg14Block.LPCONumberorName;
														}
														else if (pg14Block.LPCODateQualifier.IsEmpty && vehicleLine.US_EPARegNumber.IsEmpty)
														{
															vehicleLine.US_EPARegNumber = pg14Block.LPCONumberorName;
														}
														else if (vehicleLine.US_CertOfConformity.IsEmpty)
														{
															vehicleLine.US_CertOfConformity = pg14Block.LPCONumberorName;
															vehicleLine.US_CertOfConformityExpiryDate = pg14Block.LPCODate;
														}
													}
													else if (pg14Block.LPCOType == LPCOTypeList.Codes.EP9)
													{
														vehicleLine.US_VehicleExemptionNumber = pg14Block.LPCONumberorName;
													}
													else if (pg14Block.LPCOType == LPCOTypeList.Codes.EP7)
													{
														vehicleLine.US_BondPolicyNo = pg14Block.LPCONumberorName;
													}
													break;
											}
										}
										else
										{
											var pg19Block = block as AEPAPG19;
											if (pg19Block != null)
											{
												switch (pg19Block.EntityRoleCode)
												{
													case EntityRoleCodeList.Codes.ManufacturerOfGoods:
														if (!vehicleNumberType.IsEmpty)
														{
															if (vehicleNumberType == ItemIdentityNumberQualifierList.Codes.EngineNumber)
															{
																foreach (VehicleDetails vehicleEngineDetail in vehicleLine.VehicleAndEngineDetails)
																{
																	vehicleEngineDetail.US_EngineManufacturer = pg19Block.EntityName;
																}
															}
															else
															{
																foreach (VehicleDetails vehicleEngineDetail in vehicleLine.VehicleAndEngineDetails)
																{
																	vehicleEngineDetail.US_VehicleManufacturer = pg19Block.EntityName;
																}
															}
														}
														break;
													case EntityRoleCodeList.Codes.NAICBondIssuer:
														vehicleLine.US_NAICNo = pg19Block.EntityNumber;
														break;
													case EntityRoleCodeList.Codes.Owner:
													case EntityRoleCodeList.Codes.Importer:
													case EntityRoleCodeList.Codes.StorageLocation:
														previousCompanyRecord = pg19Block;
														organizationsDict[pg19Block] = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(pg19Block, null, null);
														break;
												}

												previousEntityRole = pg19Block.EntityRoleCode;
											}
											else
											{
												var pg20Block = block as AEPAPG20;
												if (pg20Block != null)
												{
													if (previousEntityRole == EntityRoleCodeList.Codes.NAICBondIssuer)
													{
														vehicleLine.US_StateOfIssue = pg20Block.EntityStateProvince;
													}
													else if (previousCompanyRecord != null)
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
														if (pg21Block.IndividualQualifier == EntityRoleCodeList.Codes.CertifyingIndividual)
														{
															SetBrokerDetails(pg21Block.IndividualName, pg21Block.TelephoneNumberOfTheIndividual, pg21Block.EmailAddressOrFaxNumberForTheIndividual, notifications);
															vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
														}
													}
													else
													{
														var pg55Block = block as AEPAPG55;
														if (pg55Block != null)
														{
															if (previousEntityRole == EntityRoleCodeList.Codes.Owner)
															{
																vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.Owner;
															}
															else if (previousEntityRole == EntityRoleCodeList.Codes.Importer)
															{
																vehicleLine.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
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

			ProcessOrganizations(vehicleLine, organizationsDict, notifications);
		}

		void CreateAdditionalNumberFromPG08(ZString itemIdentityNumber, ZString vehicleNumberType, VehicleDetails currentVehicleDetailsLine)
		{
			if (!itemIdentityNumber.IsEmpty && currentVehicleDetailsLine != null)
			{
				var additionalNumber = currentVehicleDetailsLine.AdditionalNumbers.AddNew();
				additionalNumber.CY_Code = vehicleNumberType;
				additionalNumber.CY_Data = itemIdentityNumber;
			}
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo orgPropertyInfo = null;
			var vehicle = pga as Vehicle;
			var organizationCode = ZString.Empty;
			var addressPK = ZGuid.Empty;

			if (roleCode == EntityRoleCodeList.Codes.Importer)
			{
				if (invoiceLine.Declaration != null && invoiceLine.Declaration.IOROrgPK.IsEmpty)
				{
					notifications.AddWarning("Importer of record should have populated from SE10 or ENS10 record. Please supply EIN/CBN/SSN of an importer of record in one of the records.");
				}
			}
			else if (roleCode == EntityRoleCodeList.Codes.Owner)
			{
				orgPropertyInfo = vehicle.US_OA_OwnerInfo;
				organizationCode = "Owner";
			}
			else if (roleCode == EntityRoleCodeList.Codes.StorageLocation)
			{
				orgPropertyInfo = vehicle.US_OA_StorageLocationInfo;
				organizationCode = "Storage Location";
			}

			if (!organizationCode.IsEmpty)
			{
				addressPK = FindMatchedOrgAddressPK(organizationCode, ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
				SetOrganizationOrAddress(OrganizationFieldType.Address, organizationCode, orgPropertyInfo, addressPK, true, notifications);
			}
		}
	}
}
