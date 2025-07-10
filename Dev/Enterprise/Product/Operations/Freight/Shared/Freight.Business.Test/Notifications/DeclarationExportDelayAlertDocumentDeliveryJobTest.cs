using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DeclarationExportDelayAlertDocumentDeliveryJobTest : DelayAlertDocumentDeliveryJobTest
	{
		#region Implementation

		protected override IDocumentSupportable NewBusinessObjectToDeliver()
		{
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_OH_Supplier] = RecipientOrganisation.PK;
			declaration[JobDeclarationSchema.JE_MessageType] = "EXP";
			return (IDocumentSupportable)declaration;
		}

		protected override BusinessContext Context
		{
			get { return BusinessContext.Customs; }
		}

		protected override bool IsExportDA
		{
			get { return true; }
		}

		#endregion
	}
}
