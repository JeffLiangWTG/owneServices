using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.TW.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class PrintBatchDocumentsOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		public PrintBatchDocumentsOperationalActionMethodApplicator(string name) : base(name)
		{
			ReMergeAndCalculate = false;
			SuppressNotificationPopout = true;
			IgnoreMessageWarnings = false;
		}

		string MessageType
		{
			get
			{
				switch (Name)
				{
					case JobDeclarationDocumentSupporter.DocumentName.ExportCustomsDeclarationInformal:
					case JobDeclarationDocumentSupporter.DocumentName.ExportCustomsDeclarationFormal:
					case JobDeclarationDocumentSupporter.DocumentName.ExportCustomsDeclarationProof:
					case JobDeclarationDocumentSupporter.DocumentName.ExportCustomsDeclarationEnglish:
						return JobMessageTypeList.Codes.Export;
					default:
						return JobMessageTypeList.Codes.Import;
				}
			}
		}

		string GetJobWord(int count) => count > 1 ? (NoResString)"jobs" : (NoResString)"job";

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var printsuccessfully = false;
			var notificationCollector = new MessageNotificationCollector();
			var logNotificationWrapper = new OperationalActionLogAndUserNotificationWrapper(notificationCollector, notificationCollector, log, IgnoreMessageWarnings, SuppressNotificationPopout);
			if (targets.Length > 0)
			{
				logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("B13A7DDE-2CD2-4F02-BFA2-79A0F2FE1BDB", "{0} {1} has been selected.", targets.Length, GetJobWord(targets.Length)));
				var messageType = MessageType;
				var printDocumentsGUI = new OperationalActionMessageNotificationCollector(log) { SuppressUserInteraction = SuppressNotificationPopout };
				var declarations = targets.OfType<JobDeclaration>().Where(x => x.JE_MessageType == messageType).ToList();
				var result = BeforePrintDocuments(declarations, logNotificationWrapper, printDocumentsGUI);
				if (result || IgnoreMessageWarnings)
				{
					var validatedDeclarations = declarations.Where(x => x.EntryHeader != null).ToList();
					var validatedDeclarationCount = validatedDeclarations.Count;
					if (validatedDeclarationCount > 0)
					{
						var jobWord = GetJobWord(validatedDeclarationCount);
						logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("905F4BD1-0A45-4332-BBBC-25055FFAE5C4", "Prepare to print {0} {1}.", validatedDeclarationCount, jobWord));
						PrintDocuments(validatedDeclarations, logNotificationWrapper);
						logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("CA0EF999-D3C3-4246-8469-F2DD0CD3832F", "{0} {1} successfully printed.", validatedDeclarationCount, jobWord));
						printsuccessfully = true;
					}
				}
			}
			if (!printsuccessfully)
			{
				logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("6411BA3D-B467-448C-B9A0-5535B9EEE96D", "No jobs have been printed."));
			}
		}

		void AddWarningForNoEntryDeclarationIfNeeded(IEnumerable<JobDeclaration> declarations, OperationalActionLogAndUserNotificationWrapper logNotificationWrapper)
		{
			var jobs = declarations.Where(x => x.EntryHeader == null).Select(x => x.JobNumber);
			if (jobs.Any())
			{
				logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("PrintBatchDocumentsOperationalActionMethodApplicator|noEntryDeclarations|Line1", @"There are no entry to print customs declaration for Job Number : {0}, and so on.", string.Join(", ", jobs.ToArray())));
				if (!ReMergeAndCalculate && !IgnoreMessageWarnings)
				{
					logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("PrintBatchDocumentsOperationalActionMethodApplicator|noEntryDeclarations|Line2", @"*Please generate entry before printing customs declaration documents.*"));
				}
			}
		}

		void PrintDocuments(List<JobDeclaration> validatedDeclarations, OperationalActionLogAndUserNotificationWrapper logNotificationWrapper)
		{
			try
			{
				var declaration = validatedDeclarations.FirstOrDefault();
				if (declaration != null)
				{
					var declarationDocsToBePrinted = declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig.Declarations;
					validatedDeclarations.ForEach(x => declarationDocsToBePrinted.Add(x));
					if (declarationDocsToBePrinted.Count > 0)
					{
						var documentRunner = new DocumentRunner();
						var documentSupportable = declaration as DocumentEngineCore.DocumentSupport.IDocumentSupportable;
						var documentCommand = DocumentCommand.GetDocumentCommand(declaration.Factory, documentSupportable, Name, true, DocumentsDataRegistry.Instance.UseNewDocBuilderOrganizationDocumentsOnly.Value);
						if (documentCommand != null)
						{
							documentCommand.Parent = documentSupportable;
							documentRunner.Run(documentCommand);
						}
						else
						{
							logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("FCA92D5A-030D-4E03-8C3D-4D6757B7A552", "Document cannot be generated because system has not found the {0} Document.", Name));
						}
					}
					else
					{
						logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("6458B4E5-5F0F-4B58-94D7-CDD2FE520092", "Document to be printed list is empty."));
					}
				}
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("PrintBatchDocumentsOperationalActionMethodApplicator", ex);
			}
		}

		bool BeforePrintDocuments(List<JobDeclaration> declarations, OperationalActionLogAndUserNotificationWrapper logNotificationWrapper, ISendsMessagesToCustoms sender)
		{
			var result = true;
			if (declarations.Count == 0)
			{
				result = false;
			}
			else
			{
				var needToMergeJobs = declarations.Where(x => x.EntryHeader == null).ToList();
				if (needToMergeJobs.Count > 0 && !ReMergeAndCalculate)
				{
					result = false;
				}
				else
				{
					foreach (var decl in needToMergeJobs)
					{
						var mergeResult = RunReMerge(decl, logNotificationWrapper, sender);
						if (!mergeResult)
						{
							result = false;
						}
					}
				}

				AddWarningForNoEntryDeclarationIfNeeded(needToMergeJobs, logNotificationWrapper);
			}
			return result;
		}

		bool RunReMerge(JobDeclaration decl, OperationalActionLogAndUserNotificationWrapper logNotificationWrapper, ISendsMessagesToCustoms sender)
		{
			var result = true;
			if (decl.JE_ApplicationCode != DeclarationApplicationCodeList.Codes.Builtin)
			{
				result = false;
				logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("4EFFF50F-4EB9-4493-AB22-7756EB2F4521", "The job {0} does not have 'Submit Type: BLT - Submit entry using built-in messaging system' selected."), decl.JobNumber);
			}
			else
			{
				if (decl.ActiveEntryHeaders.Count <= 0)
				{
					var jobNumber = decl.JobNumber;
					logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("05DBC064-3082-46C7-9B03-4A06DD01629A", "Job Number: {0}, Merging ..."), jobNumber);
					var mergeResult = decl.DoMerge(sender);
					decl.Factory.Save();
					if (mergeResult)
					{
						logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("AD2C0044-2FCF-4906-9924-E975C1EC160D", "Job Number: {0}, Merged."), jobNumber);
					}
					else
					{
						result = false;
						logNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("13BE166B-8335-413C-B88F-852247369C8E", "Job Number: {0}, Merge failed."), jobNumber);
					}
				}
			}
			return result;
		}

		#region User Action Configuration Section
		public bool ReMergeAndCalculate { get; set; }
		public bool SuppressNotificationPopout { get; set; }
		public bool IgnoreMessageWarnings { get; set; }
		#endregion
	}
}
