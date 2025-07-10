using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Printing
{
	public abstract class WhsDocumentPrinter
	{
		protected WhsDocumentPrinter(IDocumentSupportable parent, INotifications notify)
		{
			this.Parent = parent;
			this.notify = notify;
		}

		protected readonly IDocumentSupportable Parent;
		readonly INotifications notify;

		#region GetDocumentCommand

#if DEBUG
		public
#endif
 DocumentCommand GetDocumentCommand()
		{
			var commandFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, DocumentMenuName);
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_BusinessContext, DocumentMenuBusinessContext);
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsPublished, true);
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsSystemDefined, true);

			var legacyOp = UseLegacyVersion ? SQLComparisonOperator.Contains : SQLComparisonOperator.NotContains;
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, legacyOp, "Legacy");

			return new BusinessObjectFactory().LoadTop1<DocumentCommand>(commandFilter);
		}

		public abstract ZString DocumentMenuName { get; }
		protected abstract ZString DocumentMenuBusinessContext { get; }
		protected abstract ZBool UseLegacyVersion { get; }

		#endregion

		#region PrintDocument

		public ZBool PrintDocument()
		{
			var result = false;

			if (ShouldPrint)
			{
				var command = GetDocumentCommand();
				if (command != null)
				{
					command.Parent = Parent;

					var documentRunner = new DocumentRunner();

#if DEBUG
					LastPrintedDocumentName = DocumentMenuName;
					LastDocumentRunner = documentRunner;
					DocumentCommandForTest = command;
#endif
					result = documentRunner.Run(command);
				}
				else
				{
					HandleDocumentCommandNotFound();
				}
			}

			return result;
		}

		void HandleDocumentCommandNotFound()
		{
			var prefix = UseLegacyVersion ? Res.GetString("27cb47d4-8e71-4326-817a-5678f4971913", "Legacy") + " " : "";
			var message = Res.GetString("c84b99a6-7002-43a7-8b0a-49a4cb69d8eb", "Unable to find Document to print. Please make sure the {0}'{1}' Document is published.", prefix, DocumentMenuName);
			notify.Add(new ErrorNotification(ErrorType.Error, message));
		}

		#endregion

		#region ShouldPrint

		protected virtual ZBool ShouldPrint
		{
			get { return true; }
		}

		#endregion

		#region Testing
#if DEBUG
		[ThreadStatic]
		public static ZString LastPrintedDocumentName;
		public DocumentRunner LastDocumentRunner;
		public DocumentCommand DocumentCommandForTest;
#endif
		#endregion
	}
}
