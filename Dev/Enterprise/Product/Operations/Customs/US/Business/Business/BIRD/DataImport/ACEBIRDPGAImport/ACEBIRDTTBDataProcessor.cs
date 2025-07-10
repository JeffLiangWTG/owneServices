using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.US.Business
{
	[CodeAlive("This processor will be used when TTB is supported via ACE BIRD")]
	public class ACEBIRDTTBDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDTTBDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_TTBDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_TTBIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "TTB"; }
		}

		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = pg01.GovernmentAgencyProgramCode;
			ttbLine.US_ProcessingCode = pg01.GovernmentAgencyProcessingCode;

			TTBCOLAAndCertificate currentCertficateLine = null;
			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();

			foreach (var block in pgaBlocks)
			{
				var pg14Block = block as AEPAPG14;
				if (pg14Block != null)
				{
					switch (pg14Block.LPCOType)
					{
						case TTBPermitTypeList.Codes.TZ3:
							ttbLine.US_PermitNumber = pg14Block.LPCONumberorName;
							ttbLine.US_PermitExemptionCode = pg14Block.ExemptionCode;
							break;
						case TTBPermitTypeList.Codes.TZ1:
							currentCertficateLine = ttbLine.COLAAndCertificates.AddNew();
							if (!pg14Block.ExemptionCode.IsEmpty)
							{
								currentCertficateLine.US_COLAExemptionCode = pg14Block.ExemptionCode;
							}
							else if (!pg14Block.LPCONumberorName.IsEmpty)
							{
								currentCertficateLine.US_COLA = pg14Block.LPCONumberorName;
							}
							break;
						case TTBPermitTypeList.Codes.TZ5:
							ttbLine.US_IsReleaseUnderBond = true;
							ttbLine.US_NumberForIRC = pg14Block.LPCONumberorName;
							break;
					}
				}
				else
				{
					var pg13Block = block as AEPAPG13;
					if (pg13Block != null)
					{
						if (currentCertficateLine != null)
						{
							currentCertficateLine.US_ForeignCertificateCountry = pg13Block.LocationCountryStateProvinceOfIssuerOfTheLPCO;
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
									ttbLine.US_IsReleaseUnderBond = true;
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
								var pg22Block = block as AEPAPG22;
								if (pg22Block != null)
								{
									if (pg22Block.DocumentIdentifier.IsEmpty)
									{
										var cigarLine = ttbLine.Cigars.AddNew();
										var complianceDescription = pg22Block.ComplianceDescription;
										var quantityString = complianceDescription.SubstringSafe(0, 6).TrimStart('0');
										var quantity = ZInt.ParseSafe(quantityString, 0);
										cigarLine.US_Quantity = quantity;

										if (complianceDescription.SubstringSafe(7, 5) == "SMALL")
										{
											cigarLine.US_IsSmall = true;
										}
										else if (complianceDescription.SubstringSafe(7, 5) == "MAXIM")
										{
											cigarLine.US_UnitPrice = TTBCigar.MaximuSalePrice;
										}
										else
										{
											var unitPriceString = complianceDescription.SubstringSafe(8);
											var unitPriceCorrectFormat = unitPriceString.SubstringSafe(0, 2) + "." + unitPriceString.SubstringSafe(2);
											cigarLine.US_UnitPrice = ZDecimal.ParseSafe(unitPriceCorrectFormat, 0m);
										}
									}
								}
								else
								{
									var pg29Block = block as AEPAPG29;
									if (pg29Block != null)
									{
										ttbLine.US_QuantityInPCS = pg29Block.CommodityNetQuantityPGALineNet;
									}
								}
							}
						}
					}
				}
			}

			ProcessOrganizations(ttbLine, organizationsDict, notifications);
		}

		protected override ZString GetMatchedCustomsNoTypeInOrganization(ZString customsNoTypeInMessage)
		{
			var result = ZString.Empty;

			switch (customsNoTypeInMessage)
			{
				case EntityIdentificationCodesList.Codes.IRSAssigned:
					result = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
					break;
			}

			return result;
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo orgPropertyInfo = null;
			var ttb = pga as TTBLine;
			var organizationCode = ZString.Empty;
			var addressPK = ZGuid.Empty;

			if (roleCode == EntityRoleCodeList.Codes.Consignee)
			{
				orgPropertyInfo = ttb.US_OA_ConsigneeAddressInfo;
				organizationCode = "Consignee on TTB";
				addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
			}

			if (!organizationCode.IsEmpty)
			{
				SetOrganizationOrAddress(OrganizationFieldType.Address, organizationCode, orgPropertyInfo, addressPK, true, notifications);
			}
		}
	}
}
