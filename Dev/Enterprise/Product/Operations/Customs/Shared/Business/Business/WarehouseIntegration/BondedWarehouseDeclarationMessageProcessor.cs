using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.MessageProcessors
{
	public abstract class BondedWarehouseDeclarationMessageProcessor : BondedWarehouseMessageProcessor
	{
		protected BondedWarehouseDeclarationMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed)
			: base(messagePK, emailReportThatHasBeenDelayed)
		{
		}

		protected BaseJobDeclaration declaration
		{
			get { return (BaseJobDeclaration)supporter; }
		}

		protected override string GetSubject(string subjectPrefix)
		{
			return Res.GetString("5cc30ae0-755e-4dac-aaee-b59f532d918f", "{0} for Declaration Reference: {1}", subjectPrefix, declaration.JE_DeclarationReference);
		}

		protected override IWarehouseIntegrationSupporter GetSupporter()
		{
			var declarationProvider = message.EM_LinkedObject as IDeclarationProvider;
			return declarationProvider == null ? null : declarationProvider.Declaration as IWarehouseIntegrationSupporter;
		}
	}
}
