using System.Collections.Generic;
using CargoWise.Macros;
using Enterprise.Customs.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Business.Documents.DocDataObjects
{
	class JobDeclarationZAVisualizableDocumentSupporter : CustomsVisualizableDocumentSupporter<JobDeclaration>
	{
		public JobDeclarationZAVisualizableDocumentSupporter(JobDeclaration parent) : base(parent)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.MaintainJobDeclarationCustomiseForms;

		public override Either<string, object> GetAdditionalData(object obj, IStmMenuItem menuItem) => (object)null;

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;

		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;

		public override IMessageLogCreator GetMessageLogCreator(IDocument document) => null;

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions)
		{
			switch (document.DataContext)
			{
				case DataContext.CargoDuesBrokerage:
					return new CargoDuesMessagingExtensions(document, parent);
				default:
					return null;
			}
		}

		protected override string GetDocProviderKey()
		{
			return Core.Constants.CountryCodes.SouthAfrica;
		}

		protected override string GetDataStoreNameFromDocumentName(string documentName)
		{
			switch (documentName)
			{
				case DeclarationDocumentConstants.DocumentNames.CargoDuesImport:
					return DeclarationDocumentConstants.DocumentDataStoreNames.CargoDuesImport;
				case DeclarationDocumentConstants.DocumentNames.CargoDuesExport:
					return DeclarationDocumentConstants.DocumentDataStoreNames.CargoDuesExport;
			}

			return documentName;
		}
	}
}
