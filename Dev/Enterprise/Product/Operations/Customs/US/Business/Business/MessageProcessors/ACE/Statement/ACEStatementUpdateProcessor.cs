using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse)]
	public class ACEStatementUpdateProcessor : CommonStatementDeleteAddMessageProcessor<ASTUH, ASTUH1, ASTUH3, ASTUH2, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override ZString GetLongDescription(ZString errorCode, ZString initialDescription)
		{
			return ConditionCodes.GetDescriptionFromCode(errorCode);
		}

		ConditionCodesList ConditionCodes
		{
			get { return Factory.GetCachedValue<ConditionCodesList>(); }
		}
	}
}
