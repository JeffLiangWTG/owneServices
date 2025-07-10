using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ACEQuotaQueryMessageBuilder
	{
		internal MQEDIMessage GenerateMessage(BusinessObjectFactory factory, ZString queryType, ZString tariffOrCategoryNumber, ZString secondTariffNumber, ZString countryOfOrigin)
		{
			var generator = GenerateMessageBlocks(queryType, tariffOrCategoryNumber, secondTariffNumber, countryOfOrigin);
			var message = generator.CreateMessage<MQEDIMessage>(factory);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.QuotaVisaQuery;

			return message;
		}

		internal ACEInputBlockControlGenerator GenerateMessageBlocks(ZString queryType, ZString tariffOrCategoryNumber, ZString secondTariffNumber, ZString countryOfOrigin)
		{
			var q1 = new AQTAQ1();

			q1.QuotaQueryIDTypeCode = queryType;
			q1.QuotaQueryID = tariffOrCategoryNumber;
			q1.SecondTariffNumber = secondTariffNumber;
			q1.CountryOfOrigin = countryOfOrigin;

			var processingPortCode = ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(GlbBranch.CurrentBranch);
			var officeCode = USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var generator = new ACEInputBlockControlGenerator(GlbCompany.CurrentCompany.PK.ToGuid(), processingPortCode, officeCode);
			generator.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.QuotaQuery;

			generator.AddMessageBlock(q1);
			return generator;
		}
	}
}
