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
	public class ACEBIRDAMSNOPDataProcessor : ACEBIRDCommonPGADataProcessor
	{
		public ACEBIRDAMSNOPDataProcessor(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override ZPropertyInfo IndicatorInfo
		{
			get { return invoiceLine.US_NOPIndInfo; }
		}

		public override ZPropertyInfo DisclaimReasonInfo
		{
			get { return invoiceLine.US_NOPDisclaimReasonInfo; }
		}

		public override ZString PGAName
		{
			get { return "AMS NOP"; }
		}

		protected override void ProcessDeclaredPGABlocks(AEPAPG01 pg01, List<IPGABlock> pgaBlocks, INotifications notifications)
		{
			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_LineNo = invoiceLine.AMSLines.Count;
			amsHeader.US_Program = pg01.GovernmentAgencyProgramCode + pg01.GovernmentAgencyProcessingCode;
			amsHeader.US_IntendedUseCode = pg01.IntendedUseCode;
			amsHeader.US_IsElecImageSubmitted = pg01.ElectronicImageSubmitted == "Y";

			if (amsHeader.US_Program == AMSProgramList.Codes.OR2)
			{
				foreach (var block in pgaBlocks)
				{
					if (block is AEPAPG14 pg14)
					{
						var amsLine = amsHeader.AMSLines.AddNew();
						amsLine.US_CertType = pg14.LPCOTransactionType;
						amsLine.US_CertNumber = pg14.LPCONumberorName;
					}
					else
					{
						if (block is AEPAPG25 pg25)
						{
							var lotCode = amsHeader.LotCodes.AddNew();
							lotCode.CY_Code = pg25.LotNumberQualifier;
							lotCode.CY_Data = pg25.LotNumber;
						}
						else
						{
							if (block is AEPAPG29 pg29)
							{
								amsHeader.US_NetWeight = pg29.CommodityNetQuantityPGALineNet;
								amsHeader.US_NetWeightUQ = pg29.UnitOfMeasurePGALineNet;
							}
						}
					}
				}
			}
			else
			{
				var amsLines = new List<AMSLineProvider_ACEBIRDAMSNOPData>();
				var pg19Role = "";
				IACEBIRDOrgCompanyRecord previousCompanyRecord = null;
				var organizationsDict = new Dictionary<IACEBIRDOrgCompanyRecord, Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>>();
				foreach (var block in pgaBlocks)
				{
					if (block is AEPAPG10 pg10)
					{
						amsLines.Add(new AMSLineProvider_ACEBIRDAMSNOPData() { PG10 = pg10 });
					}
					else if (block is AEPAPG14 pg14)
					{
						amsHeader.US_CerNumber = pg14.LPCONumberorName;
						amsHeader.US_Date = pg14.LPCODate;
					}
					else if (block is AEPAPG19 pg19)
					{
						pg19Role = pg19.EntityRoleCode;
						var tuple = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(pg19, null, null);
						previousCompanyRecord = pg19;
						switch (pg19Role)
						{
							case EntityRoleCodeList.Codes.Exporter:
							case EntityRoleCodeList.Codes.CertifyingBodyIssuingCertificate:
							case EntityRoleCodeList.Codes.UltimateConsignee:
								organizationsDict[pg19] = tuple;
								break;
							case EntityRoleCodeList.Codes.CertifyingBodyOfFinalHandler:
								var amsLineForORC = amsLines.FirstOrDefault(x => !x.ORCHaveBeenAssigned);
								if (amsLineForORC != null)
								{
									amsLineForORC.OrganizationsDict[pg19] = tuple;
								}
								break;
							case EntityRoleCodeList.Codes.CertifiedOrganicPacker:
								var amsLineForORP = amsLines.FirstOrDefault(x => !x.ORPHaveBeenAssigned);
								if (amsLineForORP != null)
								{
									amsLineForORP.OrganizationsDict[pg19] = tuple;
								}
								break;
						}
					}
					else if (previousCompanyRecord != null && block is AEPAPG20 pg20)
					{
						Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord> tupleValue = null;
						Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord> value = null;
						switch (pg19Role)
						{
							case EntityRoleCodeList.Codes.Exporter:
							case EntityRoleCodeList.Codes.CertifyingBodyIssuingCertificate:
							case EntityRoleCodeList.Codes.UltimateConsignee:
								tupleValue = organizationsDict[previousCompanyRecord];
								value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(tupleValue.Item1, pg20, pg20);
								organizationsDict[previousCompanyRecord] = value;
								break;
							case EntityRoleCodeList.Codes.CertifyingBodyOfFinalHandler:
								var amsLineForORC = amsLines.FirstOrDefault(x => !x.ORCHaveBeenAssigned);
								if (amsLineForORC != null)
								{
									tupleValue = amsLineForORC.OrganizationsDict[previousCompanyRecord];
									value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(tupleValue.Item1, pg20, pg20);
									amsLineForORC.OrganizationsDict[previousCompanyRecord] = value;
									amsLineForORC.ORCHaveBeenAssigned = true;
								}
								break;
							case EntityRoleCodeList.Codes.CertifiedOrganicPacker:
								var amsLineForORP = amsLines.FirstOrDefault(x => !x.ORPHaveBeenAssigned);
								if (amsLineForORP != null)
								{
									tupleValue = amsLineForORP.OrganizationsDict[previousCompanyRecord];
									value = new Tuple<IACEBIRDOrgAddressRecord, IACEBIRDOrgAddress2Record, IACEBIRDOrgCountryRecord>(tupleValue.Item1, pg20, pg20);
									amsLineForORP.OrganizationsDict[previousCompanyRecord] = value;
									amsLineForORP.ORCHaveBeenAssigned = true;
									amsLineForORP.ORPHaveBeenAssigned = true;
								}
								break;
						}
						previousCompanyRecord = null;
					}
					else if (block is AEPAPG24 pg24)
					{
						if (pg24.RemarksCode == RemarksCodeList.Codes.A10)
						{
							amsHeader.US_USDAOrganicStandard = true;
							amsHeader.US_Remarks = pg24.RemarksText;
						}
						else if (pg24.RemarksCode == RemarksCodeList.Codes.A11)
						{
							amsHeader.US_EquivalentOrganicStandard = true;
							if (pg24.RemarksText != "")
							{
								amsHeader.US_Remarks = pg24.RemarksText;
							}
						}
					}
					else if (block is AEPAPG25 pg25)
					{
						var amsLine = amsLines.FirstOrDefault(x => x.PG25 == null);
						if (amsLine != null)
						{
							amsLine.PG25 = pg25;
						}
					}
					else if (block is AEPAPG27 pg27)
					{
						AddContainerToDeclaration(pg27.ContainerNumberEquipmentID);
						AddContainerToDeclaration(pg27.ContainerNumberEquipmentID1);
						AddContainerToDeclaration(pg27.ContainerNumberEquipmentID2);
					}
					else if (block is AEPAPG29 pg29)
					{
						amsHeader.US_NetWeight = pg29.CommodityNetQuantityPGALineNet;
						amsHeader.US_NetWeightUQ = pg29.UnitOfMeasurePGALineNet;
					}
				}

				ProcessOrganizations(amsHeader, organizationsDict, notifications);
				GenerateAMSLines(amsHeader, amsLines, notifications);
			}
		}

		void GenerateAMSLines(AMS amsHeader, List<AMSLineProvider_ACEBIRDAMSNOPData> amsLinesProvider, INotifications notifications)
		{
			foreach (var amsLineProvider in amsLinesProvider)
			{
				var amsLine = amsHeader.AMSLines.AddNew();
				amsLine.US_ProductLabel = amsLineProvider.PG10.CommodityCharacteristicDescription;

				var pg25 = amsLineProvider.PG25;
				if (pg25 != null)
				{
					amsLine.US_LotEntity = pg25.LotNumberQualifier;
					amsLine.US_LotNumber = pg25.LotNumber;
				}

				ProcessOrganizations(amsLine, amsLineProvider.OrganizationsDict, notifications);
			}
		}

		protected override void SetOrganizationOrAddressDetails(BusinessObject pga, ZString roleCode, ZString customsNoType, ZString customsNumber, ZString companyName, ZString address1, ZString address2, ZString countryCode, ZString city, ZString postCode, INotifications notifications)
		{
			ZPropertyInfo orgPropertyInfo = null;
			var organizationCode = ZString.Empty;
			var addressPK = ZGuid.Empty;
			var overrideExisting = true;

			if (roleCode == EntityRoleCodeList.Codes.Exporter)
			{
				orgPropertyInfo = invoiceLine.JI_OA_ExporterAddressInfo;
				organizationCode = "Exporter";
			}
			else if (roleCode == EntityRoleCodeList.Codes.CertifyingBodyIssuingCertificate)
			{
				var amsHeader = pga as AMS;
				orgPropertyInfo = amsHeader.US_OA_CertifyingBodyInfo;
				organizationCode = "AMS NOP Certifying Body Address";
			}
			else if (roleCode == EntityRoleCodeList.Codes.UltimateConsignee)
			{
				var amsHeader = pga as AMS;
				orgPropertyInfo = amsHeader.US_OA_RecipientInfo;
				organizationCode = "AMS NOP Recipient Address";
			}
			else if (roleCode == EntityRoleCodeList.Codes.CertifyingBodyOfFinalHandler)
			{
				var amsLine = pga as AMSLine;
				orgPropertyInfo = amsLine.US_OA_CerFinalHandlerInfo;
				organizationCode = "AMS NOP Line Cer Final Handler Address";
			}
			else if (roleCode == EntityRoleCodeList.Codes.CertifiedOrganicPacker)
			{
				var amsLine = pga as AMSLine;
				orgPropertyInfo = amsLine.US_OA_FinalHandlerInfo;
				organizationCode = "AMS NOP Line Final Handler Address";
			}

			if (!organizationCode.IsEmpty)
			{
				addressPK = FindMatchedOrgAddressPK(organizationCode, customsNoType, customsNumber, companyName, address1, address2, countryCode, city, postCode, notifications);
				SetOrganizationOrAddress(OrganizationFieldType.Address, organizationCode, orgPropertyInfo, addressPK, overrideExisting, notifications);
			}
		}

		void AddContainerToDeclaration(ZString containerNumber)
		{
			if (!containerNumber.IsEmpty && !invoiceLine.Declaration.CusContainers.OfType<CusContainer>().Any(x => x.CO_ContainerNumber == containerNumber))
			{
				invoiceLine.Declaration.CusContainers.AddNew().CO_ContainerNumber = containerNumber;
			}
		}
	}
}
