using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentRunSheetDocumentSupporter : DocumentSupporter
	{
		public DtbConsignmentRunSheetDocumentSupporter(DtbConsignmentRunSheet runSheet)
			: base(runSheet)
		{
		}

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.DtbConsignRunSheet; }
		}

		#endregion

		#region CustomisationSecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.DtbConsignmentRunSheetCustomiseDocuments; }
		}

		#endregion

		#region DocWrappers / DataContext

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Constants.DataContext.GenericFreightJob && commandBeingRun != null) // Document header
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
			}

			return result;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Menu name")]
		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			var result = base.GetDataStateBeforeRun(commandAboutToBeRun);

			if (result.IsValid && commandAboutToBeRun != null && commandAboutToBeRun.SU_MenuName.Equals("CMR Consignment Note") && BusinessObject is DtbConsignmentRunSheet runSheet)
			{
				var consignments = ConsignmentRunSheetHelper.GetConsignmentsFromRunSheet(runSheet);

				if (!consignments.Any())
				{
					result = new DocumentSupporterDataState();
					result.IsValid = false;
					result.ErrorMessage = Res.GetString("9d5dea4c-2fae-11f0-8c2a-6891e4b4e7ad", "Run Sheet does not contain any Consignments.");
				}
			}

			return result;
		}
	}
}
