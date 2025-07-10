using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobDeclarationDocumentSupporter : Customs.Business.BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		JobDeclaration Declaration
		{
			get { return BaseJobDeclaration as JobDeclaration; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			Core.Constants.DataContext[] supportedDataContexts = base.GetSupportedDataContexts();
			Core.Constants.DataContext[] additionalDataContexts = new Core.Constants.DataContext[]
			{
				Core.Constants.DataContext.SGPrintPermit,
				Core.Constants.DataContext.SGRefundInfo,
			};

			Core.Constants.DataContext[] result = new Core.Constants.DataContext[supportedDataContexts.Length + additionalDataContexts.Length];
			supportedDataContexts.CopyTo(result, 0);
			additionalDataContexts.CopyTo(result, result.Length - additionalDataContexts.Length);

			return result;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			if (dataContext == Core.Constants.DataContext.SGPrintPermit)
			{
				if (MostRecentPermit != null)
				{
					var permit = PrintPermitProcessor.GetPermit(MostRecentPermit, Factory);
					result = new DocumentWrapper[] { DocSGPrintPermit.New(permit, Factory) };
				}
			}
			else if (dataContext == Core.Constants.DataContext.SGRefundInfo)
			{
				if (MostRecentRefund != null)
				{
					var refund = PrintPermitProcessor.GetRefund(MostRecentRefund, Factory);
					result = new DocumentWrapper[] { DocSGRefundInfo.New(refund, Factory) };
				}
			}
			else if (dataContext == Core.Constants.DataContext.Declaration)
			{
				result = new DocumentWrapper[] { DocDeclaration.New(Declaration, Factory) };
			}
			else
			{
				result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}

			return result;
		}

		protected override DocumentSupporterDataState GetDataStateBeforeRunCore(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState result = base.GetDataStateBeforeRunCore(commandAboutToBeRun);

			if (commandAboutToBeRun.SU_MenuName.Contains(SGConstants.Permit))
			{
				StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Permit"));
				SecurityCheckpoint checkpoint = Env.Security.FindOrCreateDocumentCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.Customs.JobDeclaration, Env.Security.Operations);

				if (!checkpoint.IsAllowed)
				{
					result = new DocumentSupporterDataState(false, checkpoint.ErrorMessageForNotAllowed);
				}
				else if (MostRecentPermit == null)
				{
					result = new DocumentSupporterDataState(false, "Permit cannot be printed because no permits have been received.");
				}
			}

			if (commandAboutToBeRun.SU_MenuName.Contains(SGConstants.Refund) && MostRecentRefund == null)
			{
				result = new DocumentSupporterDataState(false, "Refund cannot be printed because no refund information have been received.");
			}

			return result;
		}

		EDIMessage MostRecentRefund => mostRecentRefund ?? (mostRecentRefund = GetMostRecentMessage(true));
		EDIMessage mostRecentRefund;

		EDIMessage MostRecentPermit => mostRecentPermit ?? (mostRecentPermit = GetMostRecentMessage(false));
		EDIMessage mostRecentPermit;

		EDIMessage GetMostRecentMessage(bool isGeneralDocument)
		{
			return Declaration.ActiveEntryHeaders
				.Cast<CusEntryHeader>()
				.SelectMany(c => c.Messages)
				.Cast<EDIMessage>()
				.Where(c => c.EM_MessageType == Cuspmt09bMessageProcessor.MessageType)
				.OrderByDescending(c => c.EM_SystemCreateTimeUtc)
				.FirstOrDefault(c => PrintPermitProcessor.IsGeneralDocument(c) == isGeneralDocument);
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);

			if (dataContextValue.DataContext == Core.Constants.DataContext.SGPrintPermit)
			{
				result = Res.GetString("561B3210-13B2-40A2-B4A0-199ECF8ECCE1", "Permit message cannot be found.");
			}
			else if (dataContextValue.DataContext == Core.Constants.DataContext.SGRefundInfo)
			{
				result = Res.GetString("20CD012A-D21D-49CA-8F32-F5D23505BE93", "Refund message cannot be found.");
			}

			return result;
		}
	}
}
