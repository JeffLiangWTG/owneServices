using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.DocumentEngine.DocumentDelivery;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobDeclarationDocumentSupporter : Customs.Business.BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
			if (jobDeclaration.Shipment != null)
			{
				jobDeclaration.Shipment.DocumentSupporter.DocumentPrintRequested += DocumentPrintRequested_NZHandler;
			}
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Customs; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return base.GetSupportedDataContexts().Concat(new[] { Core.Constants.DataContext.NZCustoms }).ToArray();
		}

		protected JobDeclaration Declaration
		{
			get { return (JobDeclaration)BusinessObject; }
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);

			documentEventSource.DocumentPrintRequested += DocumentPrintRequested_NZHandler;
		}

		void DocumentPrintRequested_NZHandler(object sender, DocumentCancelEventArgs e)
		{
			var menuItem = e.MenuItem;
			if (menuItem != null && RequiresMergeBeforeCommand(menuItem))
			{
				var errorsStoppingMerge = Declaration.MergeAndSaveIfNotMergedAlreadyReturningErrors();
				if (!errorsStoppingMerge.IsEmpty)
				{
					e.Cancel = true;
				}
			}
		}

		bool CanPrintDO => Declaration.IsEntryClear || Declaration.IsInternationalTranshipmentApproved;

		protected override List<DocumentSupporterQuestion> GenerateQuestionsToAskUsersBeforeRunningDocumentCore(IStmMenuItem commandAboutToBeRun)
		{
			var result = base.GenerateQuestionsToAskUsersBeforeRunningDocumentCore(commandAboutToBeRun);
			if (commandAboutToBeRun.SU_MenuName.Contains(DocNames.DeliveryOrder) && !CanPrintDO)
			{
				const string question = @"You are trying to print a Delivery Order for a Declaration that is not currently cleared.

Please note that the document produced will not be valid for Customs Clearance processes as it will not have the required Delivery Instructions at the bottom of the document.

Are you sure you want to print a Delivery Order?";
				result.Add(new DocumentSupporterQuestion("Warning: Declaration Not Cleared", question, QuestionType.Warning));
			}

			if ((commandAboutToBeRun.SU_MenuName.Contains(DocNames.EntryPrint) || commandAboutToBeRun.SU_MenuName.Contains(DocNames.CustomsCertificate)) && Declaration.ConsolidatedDec == null)
			{
				var warning = JobDeclaration.GetWarningMessageIfCurrentEntryTotalAmountAndReturnedOneAreDifferent(Declaration.CusEntryHeader);
				if (!warning.IsEmpty)
				{
					result.Add(new DocumentSupporterQuestion("Warning", string.Format("{0}\r\n\r\nAre you sure you want to print {1}?", warning, commandAboutToBeRun.SU_MenuName), QuestionType.Warning));
				}
			}

			return result;
		}

		protected override DocumentSupporterDataState GetDataStateBeforeRunCore(IStmMenuItem commandAboutToBeRun)
		{
			if (RequiresMergeBeforeCommand(commandAboutToBeRun))
			{
				var mergeResultGetter = new Customs.Business.SendsMessagesToCustomsReturningResultsAsProperties(true);

				ZString errorsStoppingMerge = Declaration.MergeManager.CheckAndGetPrerequisiteConditions(mergeResultGetter);
				if (!errorsStoppingMerge.IsEmpty)
				{
					return new DocumentSupporterDataState(false, MessageForInvaildEntryPrintDataState(commandAboutToBeRun) + "\r\n\r\n" + errorsStoppingMerge);
				}
			}

			return base.GetDataStateBeforeRunCore(commandAboutToBeRun);
		}

		bool RequiresMergeBeforeCommand(IStmMenuItem commandAboutToBeRun)
		{
			return Declaration.ShouldMergeInvoiceLines &&
				(
					commandAboutToBeRun.SU_MenuName.Contains(DocNames.EntryPrint)
					 || commandAboutToBeRun.SU_MenuName.Contains(DocNames.CustomsCertificate)
					 || commandAboutToBeRun.SU_MenuName.Contains(DocNames.DissectionReport)
				);
		}

		public class DocNames : Freight.Business.CommonShipmentDocumentSupporter.NZCustomsDocList
		{
			// These are all defined in Enterprise.Freight.Business.ShipmentDocumentSupport.NZCustomsDocNames class so that
			// the functionality defined here that is "Special" for Customs Docs in GetDataStateBeforeRun and GenerateQuestionsToAskUsersBeforeRunningDocumentCore
			// will get run even if the document is run from a Declaration attached to a Shipment.
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			switch (dataContext)
			{
				case Core.Constants.DataContext.LandedCostHeader:
					{
						ILandedCostHeader declaration = Declaration;
						declaration.DoStuffBeforeRunningLCDistribution();
					}
					break;
				case Core.Constants.DataContext.NZCustoms:
					dataContext = Core.Constants.DataContext.Declaration;
					break;
			}

			return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}

		#region AutoPrintCustomsClearanceDocs
		public void AutoPrintCustomsClearanceDocs(CusEntryHeader entryHeader, bool printDO, bool printCCAndEP = true)
		{
			if (entryHeader == null)
			{
				throw new ArgumentNullException(nameof(entryHeader));
			}
			AutoPrintCustomsClearanceDocs(entryHeader.Messages.LastOutgoingMessage, printDO, printCCAndEP);
		}

		public void AutoPrintCustomsClearanceDocs(EDIMessage outgoingMessage, bool printDO, bool printCCAndEP = true)
		{
			if (printCCAndEP)
			{
				AutoPrintDocument(outgoingMessage, "Customs Certificate", NZCustomsDataRegistry.Instance.CustomsCertificatePrinter, NZCustomsDataRegistry.Instance.CustomsCertificateCopies, NZCustomsDataRegistry.Instance.CustomsCertificateCopyToEDocs);
				AutoPrintDocument(outgoingMessage, "Entry Print", NZCustomsDataRegistry.Instance.EntryPrintPrinter, NZCustomsDataRegistry.Instance.EntryPrintCopies, NZCustomsDataRegistry.Instance.EntryPrintCopyToEDocs);
			}

			if (printDO)
			{
				AutoPrintDocument(outgoingMessage, "Delivery Order", NZCustomsDataRegistry.Instance.DeliveryOrderPrinter, NZCustomsDataRegistry.Instance.DeliveryOrderCopies, NZCustomsDataRegistry.Instance.DeliveryOrderCopyToEDocs);
			}
		}

		void AutoPrintDocument(EDIMessage outgoingMessage, ZString documentName, GuidRegistryItem printerRegistryItem, IntRegistryItem copiesRegistryItem, BooleanRegistryItem copyToEDocsRegistryItem)
		{
			ZString menuPath = "";
			ZString filterList = "MSGBKRCTY=" + Declaration.JE_MessageType + "NZ";
			Guid branchGuid = (Declaration.JE_GB.IsEmpty ? GlbBranch.CurrentBranch.PK : Declaration.JE_GB).ToGuid();
			Guid departmentGuid = Guid.Empty;
			if (outgoingMessage != null)
			{
				GlbStaff staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, outgoingMessage.EM_SystemCreateUser);
				if (staff != null && staff.HomeDepartment != null)
				{
					departmentGuid = staff.HomeDepartment.PK.ToGuid();
				}
			}

			ZGuid printQueuePK = new ZGuid(printerRegistryItem.GetFallBackValueAtAllLevels(Guid.Empty, branchGuid, departmentGuid));
			ZInt copies = new ZInt(copiesRegistryItem.GetValueWithoutFallback(Guid.Empty, branchGuid, Guid.Empty));
			ZBool copyToEDocs = new ZBool(copyToEDocsRegistryItem.GetValueWithoutFallback(Guid.Empty, branchGuid, Guid.Empty));
			SilentDocumentPrinter documentPrinter = new SilentDocumentPrinter(Factory, Declaration, documentName, menuPath, filterList);
			documentPrinter.Print(printQueuePK, copies, false);

			if (copyToEDocs)
			{
				documentPrinter.Print(ZGuid.Empty, 0, true);
			}
		}

		public override ZBool ShowReasonForNotPrinting(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return base.ShowReasonForNotPrinting(dataContext, commandBeingRun) && dataContext != Core.Constants.DataContext.NZCustoms;
		}

		#endregion
	}
}
