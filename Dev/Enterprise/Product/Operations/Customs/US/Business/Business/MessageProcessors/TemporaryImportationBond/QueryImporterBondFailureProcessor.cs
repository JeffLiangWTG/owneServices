using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.QueryImporterBond)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.QueryImporterBondResponse)]
	[TopLevel(typeof(AABIX0), typeof(AABIOutputX1))]
	public class QueryImporterBondFailureProcessor : AllACEMessageFailureProcessor
	{
		protected override void SetFailStatus(BusinessObject bizObj)
		{
			if (bizObj.TablePrefix != JobDeclarationSchema.Constants.Prefix && bizObj.TablePrefix != OrgHeaderSchema.Constants.Prefix)
			{
				base.SetFailStatus(bizObj);
			}
		}
	}
}
