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
	public class ACEBIRDOMCDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDOMCDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_OMCDisclaimReasonInfo; }
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_OMCIndInfo; }
		}

		public override ZString PGAName
		{
			get { return "OMC"; }
		}

		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var omcHeader = invoiceLine.OMCHeaders.AddNew();
			omcHeader.US_ElectronicImageSubmitted = pg01.ElectronicImageSubmitted == "Y";

			IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
			var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
			AEPAPG21 exporterPG21Block = null;
			AEPAPG21 governmentOfficialPG21Block = null;

			foreach (var block in pgaBlocks)
			{
				var pg06Block = block as AEPAPG06;
				if (pg06Block != null)
				{
					if (pg06Block.SourceTypeCode == SourceTypeCodesList.Codes.Harvested)
					{
						omcHeader.US_SourceCountry = pg06Block.CountryCode;
						omcHeader.US_DepartureDate = pg06Block.ProcessingStartDate;
					}
				}
				else
				{
					var pg19Block = block as AEPAPG19;
					if (pg19Block != null)
					{
						switch (pg19Block.EntityRoleCode)
						{
							case EntityRoleCodeList.Codes.AquacultureFacility:
							case EntityRoleCodeList.Codes.Exporter:
							case EntityRoleCodeList.Codes.ResponsibleGovernmentOfficial:
								organizationsDict[pg19Block] = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(pg19Block, null, null);
								break;
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
								if (organizationsDict.ContainsKey(previousCompanyRecord))
								{
									var tupleValue = organizationsDict[previousCompanyRecord];
									var value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(tupleValue.Item1, pg20Block, pg20Block);
									organizationsDict[previousCompanyRecord] = value;
								}

								previousCompanyRecord = null;
							}
						}
						else
						{
							var pg21Block = block as AEPAPG21;
							if (pg21Block != null)
							{
								switch (pg21Block.IndividualQualifier)
								{
									case EntityRoleCodeList.Codes.Exporter:
										exporterPG21Block = pg21Block;
										break;
									case EntityRoleCodeList.Codes.ResponsibleGovernmentOfficial:
										governmentOfficialPG21Block = pg21Block;
										break;
								}
							}
							else
							{
								var pg22Block = block as AEPAPG22;
								if (pg22Block != null)
								{
									if (pg22Block.EntityRoleCode == EntityRoleCodeList.Codes.Exporter)
									{
										omcHeader.US_DeclarationCode = pg22Block.ConformanceDeclaration;
										omcHeader.US_ExporterCertificationDate = pg22Block.DateOfSignature;
									}
									else if (pg22Block.EntityRoleCode == EntityRoleCodeList.Codes.ResponsibleGovernmentOfficial)
									{
										omcHeader.US_OfficialCertificationDate = pg22Block.DateOfSignature;
									}
								}
								else
								{
									var pg29Block = block as AEPAPG29;
									if (pg29Block != null)
									{
										omcHeader.US_NetWeight = pg29Block.CommodityNetQuantityPGALineNet;
										omcHeader.US_NetWeightUQ = pg29Block.UnitOfMeasurePGALineNet;
									}
								}
							}
						}
					}
				}
			}

			ProcessOrganizations(omcHeader, organizationsDict, notifications);

			if (exporterPG21Block != null)
			{
				omcHeader.US_ExporterPGAContactName = exporterPG21Block.IndividualName;
				omcHeader.US_ExporterPGAContactPhoneNo = exporterPG21Block.TelephoneNumberOfTheIndividual;
				omcHeader.US_ExporterPGAContactEmail = exporterPG21Block.EmailAddressOrFaxNumberForTheIndividual;
			}

			if (governmentOfficialPG21Block != null)
			{
				omcHeader.US_OfficialPGAContactName = governmentOfficialPG21Block.IndividualName;
				omcHeader.US_OfficialPGAContactPhoneNo = governmentOfficialPG21Block.TelephoneNumberOfTheIndividual;
				omcHeader.US_OfficialPGAContactEmail = governmentOfficialPG21Block.EmailAddressOrFaxNumberForTheIndividual;
			}
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo orgPropertyInfo = null;
			var omcHeader = pga as OMCHeader;
			var organizationCode = ZString.Empty;

			if (roleCode == EntityRoleCodeList.Codes.AquacultureFacility)
			{
				if (omcHeader.US_OA_AquacultureFacility.IsEmpty)
				{
					orgPropertyInfo = omcHeader.US_OA_AquacultureFacilityInfo;
					organizationCode = "Aquaculture Facility";
				}
				else
				{
					var additionalAquaculture = omcHeader.AquacultureFacilities.AddNew();
					orgPropertyInfo = additionalAquaculture.US_OA_AquacultureFacilityInfo;
					organizationCode = "Additional Aquaculture Facility";
				}
			}
			else if (roleCode == EntityRoleCodeList.Codes.Exporter)
			{
				orgPropertyInfo = omcHeader.US_OA_ExporterInfo;
				organizationCode = "Exporter on OMC";
			}
			else if (roleCode == EntityRoleCodeList.Codes.ResponsibleGovernmentOfficial)
			{
				orgPropertyInfo = omcHeader.US_OA_ResponsibleGovernmentOfficialInfo;
				organizationCode = "Responsible Government Official";
			}

			var addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
			if (!organizationCode.IsEmpty && !addressPK.IsEmpty)
			{
				SetOrganizationOrAddress(OrganizationFieldType.Address, organizationCode, orgPropertyInfo, addressPK, true, notifications);
			}
		}
	}
}
