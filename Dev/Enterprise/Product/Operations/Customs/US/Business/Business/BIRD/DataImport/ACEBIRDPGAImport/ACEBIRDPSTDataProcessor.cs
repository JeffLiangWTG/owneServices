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
	public class ACEBIRDPSTDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDPSTDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override void ProcessDisclaimedPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			base.ProcessDisclaimedPGABlocks(pg01, pgaBlocks, notifications);
			invoiceLine.US_PSTDisclaimProgram = pg01.GovernmentAgencyProgramCode;
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_PSTDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_PSTIndicatorInfo; }
		}

		public override ZString PGAName
		{
			get { return "PST"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var pesticide = invoiceLine.PSTLines.AddNew();
			pesticide.US_ProductType = pg01.GovernmentAgencyProgramCode;
			pesticide.US_IntendedUseCode = pg01.IntendedUseCode;
			pesticide.US_PSTLabelsSent = pg01.ElectronicImageSubmitted == "Y";
			pesticide.US_CBIIndicator = pg01.ConfidentialInformationIndicator == "Y";

			PesticideLine currentPesticideLine = null;
			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();

			foreach (var block in pgaBlocks)
			{
				var pg02Block = block as AEPAPG02;
				if (pg02Block != null)
				{
					if (!pg02Block.ProductCodeQualifier.IsEmpty || !pg02Block.ProductCodeNumber.IsEmpty)
					{
						currentPesticideLine = pesticide.PesticideLines.AddNew();
						currentPesticideLine.US_LPCOType = pg02Block.ProductCodeQualifier;
						currentPesticideLine.US_LPCONumber = pg02Block.ProductCodeNumber;
					}
				}
				else
				{
					var pg04Block = block as AEPAPG04;
					if (pg04Block != null)
					{
						if (currentPesticideLine == null)
						{
							currentPesticideLine = pesticide.PesticideLines.AddNew();
						}

						currentPesticideLine.US_NameOfActiveIngredient = pg04Block.NameOfTheConstituentElement;
						currentPesticideLine.US_ActiveIngredientPercentage = pg04Block.PercentOfConstituentElement;
						currentPesticideLine = null;
					}
					else
					{
						var pg24Block = block as AEPAPG24;
						if (pg24Block != null)
						{
							if (pg24Block.RemarksTypeCode == "GEN")
							{
								pesticide.US_UnregReasonRemarks = pg24Block.RemarksText;
							}

							else
							{
								pesticide.US_UnregReasonCode = pg24Block.RemarksCode;
								pesticide.US_ConfidentialityRemarks = pg24Block.RemarksText;
							}
						}
						else
						{
							var pg07Block = block as AEPAPG07;
							if (pg07Block != null)
							{
								pesticide.US_BrandName = pg07Block.TradeNameBrandName;
							}
							else
							{
								var pg14Block = block as AEPAPG14;
								if (pg14Block != null)
								{
									if (pg14Block.LPCOType == LPCOTypeList.Codes.EP8)
									{
										pesticide.US_LPCONumber = pg14Block.LPCONumberorName;
									}
								}
								else
								{
									var pg19Block = block as AEPAPG19;
									if (pg19Block != null)
									{
										switch (pg19Block.EntityRoleCode)
										{
											case EntityRoleCodeList.Codes.EPAProducerEstablishmentNumber:
												if (pesticide.US_ProducerEstNoForeign.IsEmpty)
												{
													pesticide.US_ProducerEstNoForeign = pg19Block.EntityNumber;
												}
												else
												{
													pesticide.US_ProducerEstNo = pg19Block.EntityNumber;
												}
												break;
											case EntityRoleCodeList.Codes.Importer:
											case EntityRoleCodeList.Codes.Carrier:
											case EntityRoleCodeList.Codes.TransportMeansOwner:
											case EntityRoleCodeList.Codes.Shipper:
											case EntityRoleCodeList.Codes.LocationOfGoodsImmediatelyAfterEntryRelease:
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
												if (pg21Block.IndividualQualifier == EntityRoleCodeList.Codes.CertifyingIndividual)
												{
													SetBrokerDetails(pg21Block.IndividualName, pg21Block.TelephoneNumberOfTheIndividual, pg21Block.EmailAddressOrFaxNumberForTheIndividual, notifications);
												}
											}
											else
											{
												var pg26Block = block as AEPAPG26;
												if (pg26Block != null)
												{
													switch (pg26Block.PackagingQualifier)
													{
														case 1:
															pesticide.US_NoOfUnit1 = pg26Block.Quantity;
															pesticide.US_UQ1 = pg26Block.UnitOfMeasurePackagingLevel;
															break;
														case 2:
															pesticide.US_NoOfUnit2 = pg26Block.Quantity;
															pesticide.US_UQ2 = pg26Block.UnitOfMeasurePackagingLevel;
															break;
														case 3:
															pesticide.US_NoOfUnit3 = pg26Block.Quantity;
															pesticide.US_UQ3 = pg26Block.UnitOfMeasurePackagingLevel;
															break;
														case 4:
															pesticide.US_NoOfUnit4 = pg26Block.Quantity;
															pesticide.US_UQ4 = pg26Block.UnitOfMeasurePackagingLevel;
															break;
														case 5:
															pesticide.US_NoOfUnit5 = pg26Block.Quantity;
															pesticide.US_UQ5 = pg26Block.UnitOfMeasurePackagingLevel;
															break;
														case 6:
															pesticide.US_NoOfUnit6 = pg26Block.Quantity;
															pesticide.US_UQ6 = pg26Block.UnitOfMeasurePackagingLevel;
															break;
													}
												}
												else
												{
													var pg29Block = block as AEPAPG29;
													if (pg29Block != null)
													{
														pesticide.US_NetWeight = pg29Block.CommodityNetQuantityPGALineNet;
														pesticide.US_WeightUQ = pg29Block.UnitOfMeasurePGALineNet;
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

			ProcessOrganizations(pesticide, organizationsDict, notifications);
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo orgPropertyInfo = null;
			var pesticide = pga as Pesticide;
			var organizationCode = ZString.Empty;
			var overrideExisting = true;
			var addressPK = ZGuid.Empty;
			var fieldType = OrganizationFieldType.Address;

			if (roleCode == EntityRoleCodeList.Codes.Importer)
			{
				if (invoiceLine.Declaration != null && invoiceLine.Declaration.IOROrgPK.IsEmpty)
				{
					notifications.AddWarning("Importer of record should have populated from SE10 or ENS10 record. Please supply EIN/CBN/SSN of an importer of record in one of the records.");
				}
			}
			else if (roleCode == EntityRoleCodeList.Codes.Carrier || roleCode == EntityRoleCodeList.Codes.TransportMeansOwner)
			{
				orgPropertyInfo = invoiceLine.Declaration.JE_OH_ShippingLineInfo;
				overrideExisting = false;
				organizationCode = "Carrier";
				fieldType = OrganizationFieldType.Header;
				addressPK = FindMatchedOrgAddressPK(organizationCode, ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
			}
			else if (roleCode == EntityRoleCodeList.Codes.Shipper)
			{
				orgPropertyInfo = pesticide.US_OA_ShipperAddressInfo;
				organizationCode = "Shipper on PST";
				addressPK = FindMatchedOrgAddressPK(organizationCode, ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
			}
			else if (roleCode == EntityRoleCodeList.Codes.LocationOfGoodsImmediatelyAfterEntryRelease)
			{
				orgPropertyInfo = pesticide.US_OA_ExaminationLocationInfo;
				organizationCode = "Exam. Location";
				addressPK = FindMatchedOrgAddressPK(organizationCode, ZString.Empty, ZString.Empty, companyName, address1, address2, countryCode, city, postCode, notifications);
			}

			if (!organizationCode.IsEmpty)
			{
				SetOrganizationOrAddress(fieldType, organizationCode, orgPropertyInfo, addressPK, overrideExisting, notifications);
			}
		}
	}
}
