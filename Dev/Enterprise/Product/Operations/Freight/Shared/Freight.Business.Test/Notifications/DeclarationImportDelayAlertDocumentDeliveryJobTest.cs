using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DeclarationImportDelayAlertDocumentDeliveryJobTest : DelayAlertDocumentDeliveryJobTest
	{
		#region Implementation

		protected override IDocumentSupportable NewBusinessObjectToDeliver()
		{
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_OH_Importer] = RecipientOrganisation.PK;
			declaration[JobDeclarationSchema.JE_MessageType] = "IMP";
			return (IDocumentSupportable)declaration;
		}

		protected override BusinessContext Context
		{
			get { return BusinessContext.Customs; }
		}

		protected override bool IsExportDA
		{
			get { return false; }
		}

		#endregion
	}
}
