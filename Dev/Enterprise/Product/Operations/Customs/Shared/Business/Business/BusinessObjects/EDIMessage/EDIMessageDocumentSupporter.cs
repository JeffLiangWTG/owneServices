using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class EDIMessageDocumentSupporter : DocumentSupporter
	{
		public EDIMessageDocumentSupporter(EDIMessage ediMessage)
			: base(ediMessage)
		{ }

		protected EDIMessage EdiMessage
		{
			get { return (EDIMessage)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.EDIMessage; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return Array.Empty<DocumentWrapper>();
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return Array.Empty<Core.Constants.DataContext>();
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}
	}
}
