using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Customs.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	class JobDeclarationUSVisualizableDocumentSupporter : CustomsVisualizableDocumentSupporter<JobDeclaration>
	{
		public JobDeclarationUSVisualizableDocumentSupporter(JobDeclaration parent) : base(parent)
		{
		}

		public override ISecurityCheckpoint CustomizeFormCheckpoint => Env.Security.MaintainJobDeclarationCustomiseForms;

		public override Either<string, object> GetAdditionalData(object obj, IStmMenuItem menuItem)
		{
			var contextTypes = GetContextTypes(menuItem);
			if (contextTypes.Contains(DataContext.USATF6A))
			{
				var selector = ObjectFactory.Get<Integration.Customs.US.IATF6AFormPermitNumbersSelector>();
				var selectedPermitNumbers = selector.SelectPermitNumbers(obj);

				if (selectedPermitNumbers.IsLeft)
				{
					return selectedPermitNumbers.Left;
				}

				return selectedPermitNumbers.Right;
			}
			return (object)null;
		}

		ZString[] GetContextTypes(IStmMenuItem menuItem)
		{
			return menuItem
						?.Documents
						.OfType<IStmMenuTemplatePivot>()
						.Select(p => p.Template.SO_DataContext)
						.ToArray();
		}

		public override IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;

		public override IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;

		public override IMessageLogCreator GetMessageLogCreator(IDocument document) => null;

		public override IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => null;

		protected override string GetDataStoreNameFromDocumentName(string documentName) => string.Empty;

		protected override string GetDocProviderKey()
		{
			return Core.Constants.CountryCodes.UnitedStates;
		}
	}
}
