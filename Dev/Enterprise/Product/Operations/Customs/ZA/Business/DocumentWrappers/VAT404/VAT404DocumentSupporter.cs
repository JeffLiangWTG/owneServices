using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business
{
	internal class VAT404DocumentSupporter : DocumentSupporter
	{
		#region ctor

		public VAT404DocumentSupporter(VAT404Document parent) : base(parent)
		{
		}

		#endregion

		#region Const

		internal const string VAT404ProofOfPayment = ".VAT404ProofOfPayment";

		#endregion

		#region Override

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ZAProofOfPayment; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZA404ProofOfPaymentCustomiseDocuments);

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return System.Array.Empty<DocumentWrapper>();
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Constants.DataContext.None };
		}

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(VAT404ProofOfPayment));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return new IBODocDataProvider[] { BODocDataProvider.Get(BusinessObject) };
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;
			var importer = (BusinessObject as VAT404Document)?.Importer;
			if (importer != null)
			{
				result = new DocumentEngine.OrgHeaderContact(importer, null);
			}
			return result;
		}

		public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			return new TitleCopyCountPair(parentDocumentMenuName);
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion
	}
}
