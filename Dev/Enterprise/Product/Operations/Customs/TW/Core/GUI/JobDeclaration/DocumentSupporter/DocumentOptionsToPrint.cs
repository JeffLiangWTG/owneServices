using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.TW.Business;
using Enterprise.DocumentEngine.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public class DocumentOptionsToPrint : Integration.Customs.TW.IDocumentOptionsToPrint
	{
		bool Integration.Customs.TW.IDocumentOptionsToPrint.GetDocumentOptionsToPrintCustomsDeclaration(Integration.Customs.TW.IJobDeclaration declaration, ICollection documents)
		{
			var result = true;
			if (declaration is JobDeclaration decl && IsPerformOnGetDocumentOptionsToPrintCustomsDeclarationRequired(documents))
			{
				var declDcoumentAddressConfig = decl.DocumentSupporter.JobDeclarationDocumentAddressConfig;
				result = declDcoumentAddressConfig.Declarations.Count > 0 ? CheckMultipleDeclarations(decl, declDcoumentAddressConfig)
					: CheckSingleDeclaration(decl, declDcoumentAddressConfig);
			}
			return result;
		}

		bool CheckSingleDeclaration(JobDeclaration decl, JobDeclarationDocumentAddressConfig config)
		{
			if (!decl.HasMessageInitiator)
			{
				decl.MessageInitiator = new Customs.GUI.SendsMessagesToCustomsGUI();
			}
			return Business.MessageManagers.MessageManager.CheckEntries(decl, new MessageNotificationCollector()) && ShowDeclarationDocumentOptionsForm(decl, config);
		}

		bool CheckMultipleDeclarations(JobDeclaration decl, JobDeclarationDocumentAddressConfig config)
		{
			var result = ShowDeclarationDocumentOptionsForm(decl, config);
			if (result)
			{
				config.Declarations.Except(decl).ForEach(x => x.DocumentSupporter.JobDeclarationDocumentAddressConfig.CopyConfigParams(config));
			}

			return result;
		}

		bool ShowDeclarationDocumentOptionsForm(JobDeclaration decl, JobDeclarationDocumentAddressConfig config)
		{
			var result = true;
			config.SetDefaultJobDeclarationDocumentOptions();
			using (var documentOptionsForm = new JobDeclarationDocumentOptionsForm(decl.IsExport, config))
			{
				result = ZFormModaliser.ShowDialogWithoutDispose(documentOptionsForm) == DialogResult.OK;
			}
			return result;
		}

		bool IsPerformOnGetDocumentOptionsToPrintCustomsDeclarationRequired(ICollection documents)
		{
			return documents.Cast<StmMenuTemplatePivotBase>().Any(c => ShouldDisplayOptionDialog.Contains(c.Template?.SO_DataContext ?? ZString.Empty));
		}

		internal HashSet<ZString> ShouldDisplayOptionDialog => shouldDisplayOptionDialog ??= new HashSet<ZString>
		{
			CusEntryHeaderDocumentSupporter.ExportCustomsDeclarationDocument,
			JobDeclarationDocumentSupporter.ExportNonCondensedDeclarationDocument,
			CusEntryHeaderDocumentSupporter.ImportCustomsDeclarationDocument,
			CusEntryHeaderDocumentSupporter.ImportCustomsNonCondensedDeclarationDocument
		};
		HashSet<ZString> shouldDisplayOptionDialog;
	}
}
