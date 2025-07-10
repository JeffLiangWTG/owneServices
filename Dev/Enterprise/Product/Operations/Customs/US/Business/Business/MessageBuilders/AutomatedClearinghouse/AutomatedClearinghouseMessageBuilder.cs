#define CODE_ANALYSIS
using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class AutomatedClearinghouseMessageBuilder
	{
		public AutomatedClearinghouseMessageBuilder(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public MQEDIMessage Generate<TPaymentAuthorisation>(ZString applicationIdentifier, IPaymentAuthorisation statementHeader, ZString payType)
			where TPaymentAuthorisation : MessageBlock, IPaymentAuthorisation, new()
		{
			BlockControlGenerator block = null;

			var processingOfficeCode = USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			block = new ACEInputBlockControlGenerator(statementHeader.StatementFiler, statementHeader.ProcessingPortCode, processingOfficeCode);
			block.B.ApplicationIdentifier = applicationIdentifier;

			TPaymentAuthorisation paymentAuthorisation = new TPaymentAuthorisation();
			paymentAuthorisation.PayersUnitNumber = statementHeader.PayersUnitNumber;
			paymentAuthorisation.PaymentType = payType;
			paymentAuthorisation.StatementFiler = statementHeader.StatementFiler;
			paymentAuthorisation.StatementBillNumber = statementHeader.StatementBillNumber;
			paymentAuthorisation.PaymentAmount = statementHeader.PaymentAmount;
			paymentAuthorisation.NegationCode = statementHeader.NegationCode;
			paymentAuthorisation.NegationDate = statementHeader.NegationDate;
			block.MessageBlocks.Add(paymentAuthorisation);

			var result = block.CreateMessage<MQEDIMessage>(factory);
			result.EM_MessageSubType = EM_MessageSubTypeList.Codes.AutomatedClearinghouse;
			return result;
		}

		readonly BusinessObjectFactory factory;
	}
}
