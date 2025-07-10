using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ACEACQueryMessageBuilder
	{
		public MQEDIMessage Generate(BusinessObjectFactory factory, IEnumerable<ZString> caseNumbers)
		{
			ZString processingPortCode = GetProcessingPortCode();
			ZString officeCode = GetOfficeCode();

			ACEInputBlockControlGenerator generator = new ACEInputBlockControlGenerator(GlbCompany.CurrentCompany.PK.ToGuid(), processingPortCode, officeCode);
			generator.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQuery;

			int index = 1;

			AADQQ1 q1 = null;
			foreach (ZString caseNumber in caseNumbers)
			{
				if (q1 == null)
				{ q1 = new AADQQ1(); }

				switch (index)
				{
					case 1:
						q1.CaseNumber1 = caseNumber.Left(7);
						q1.CaseNumber1Suffix = caseNumber.SubstringSafe(7, 3);
						break;

					case 2:
						q1.CaseNumber2 = caseNumber.Left(7);
						q1.CaseNumber2Suffix = caseNumber.SubstringSafe(7, 3);
						break;

					case 3:
						q1.CaseNumber3 = caseNumber.Left(7);
						q1.CaseNumber3Suffix = caseNumber.SubstringSafe(7, 3);
						break;

					case 4:
						q1.CaseNumber4 = caseNumber.Left(7);
						q1.CaseNumber4Suffix = caseNumber.SubstringSafe(7, 3);
						break;

					case 5:
						q1.CaseNumber5 = caseNumber.Left(7);
						q1.CaseNumber5Suffix = caseNumber.SubstringSafe(7, 3);
						break;
				}

				index++;

				if (index > 5)
				{
					index = 1;
					generator.AddMessageBlock(q1);
					q1 = null;
				}
			}

			if (q1 != null)
			{
				generator.AddMessageBlock(q1);
			}

			var result = generator.CreateMessage<MQEDIMessage>(factory);
			result.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACE_AD_CVDQuery;
			return result;
		}

		public MQEDIMessage Generate(BusinessObjectFactory factory, IACEACCaseQueryInput queryInput)
		{
			var caseNumbersList = new List<ZString>(queryInput.CaseNumbers);
			return caseNumbersList.Count > 0 ?
				Generate(factory, queryInput.CaseNumbers) :
				GenerateWithAdditionalCriteria(factory, queryInput);
		}

		public MQEDIMessage GenerateWithAdditionalCriteria(BusinessObjectFactory factory, IACEACCaseQueryInput queryInput)
		{
			var processingPortCode = GetProcessingPortCode();
			var officeCode = GetOfficeCode();

			var generator = new ACEInputBlockControlGenerator(GlbCompany.CurrentCompany.PK.ToGuid(), processingPortCode, officeCode);
			generator.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQuery;

			var q2 = new AADQQ2();
			q2.CompanyCaseStatus = queryInput.CaseStatus;
			q2.CountryCode = queryInput.CountryCode;
			q2.DateSinceLastUpdate = queryInput.DateSinceLastUpdate;
			q2.ForeignExporterIdentificationCode = queryInput.ForeignExporterMID;
			q2.HTSNumber = new TariffFormatter().Format(queryInput.HTSNumber);
			q2.ManufacturerIdentificationCode = queryInput.ManufacturerMID;
			q2.TSUSANumber = queryInput.TSUSANumber;
			generator.AddMessageBlock(q2);

			var result = generator.CreateMessage<MQEDIMessage>(factory);
			result.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACE_AD_CVDQuery;
			return result;
		}

		ZString GetProcessingPortCode()
		{
			return ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(GlbBranch.CurrentBranch);
		}

		ZString GetOfficeCode()
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			return USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}
	}

	public interface IACEACCaseQueryInput
	{
		IEnumerable<ZString> CaseNumbers { get; }
		ZString CaseStatus { get; }
		ZString CountryCode { get; }
		ZString HTSNumber { get; }
		ZString TSUSANumber { get; }
		ZString ManufacturerMID { get; }
		ZString ForeignExporterMID { get; }
		ZDate DateSinceLastUpdate { get; }
	}

	public class ACEACCaseQueryInput : IACEACCaseQueryInput
	{
		public ZString CaseStatus { get; set; }
		public ZString CountryCode { get; set; }
		public ZString HTSNumber { get; set; }
		public ZString TSUSANumber { get; set; }
		public ZString ManufacturerMID { get; set; }
		public ZString ForeignExporterMID { get; set; }
		public ZDate DateSinceLastUpdate { get; set; }

		public IEnumerable<ZString> CaseNumbers
		{
			get { return Array.Empty<ZString>(); }
		}
	}
}
