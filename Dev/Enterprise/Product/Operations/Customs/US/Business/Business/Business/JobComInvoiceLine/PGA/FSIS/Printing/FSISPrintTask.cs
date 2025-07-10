using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class FSISPrintTask : IDisposable
	{
		public FSISPrintTask(JobDeclaration declaration)
		{
			this.declaration = declaration;
			DocumentCommand command = declaration.IsStandAlone ? GetCertificatePrintCommand("Customs") : GetCertificatePrintCommand("Shipment");
			if (command != null)
			{
				command.Parent = declaration;
				task = new DocumentPrintSet(command, null);
			}
		}
		readonly JobDeclaration declaration;
		readonly DocumentPrintSet task;

		DocumentCommand GetCertificatePrintCommand(string businessContext)
		{
			DocumentCommand command;

			ZQuery queryCustoms = new ZQuery(StmMenuItemSchema.SU_MenuName, DocumentNames.FSISForm9540);
			queryCustoms.AddToFilter(StmMenuItemSchema.SU_BusinessContext, businessContext);
			queryCustoms.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.DoesNotStartWith, Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments);
			command = declaration.FSISLinesForPrint[0].Factory.LoadTop1<DocumentCommand>(queryCustoms);

			return command;
		}

		public void Run()
		{
			if (task != null)
			{
				task.Run(Env.Security.None);
			}
		}

		public void Dispose()
		{
			if (task != null)
			{
				task.Dispose();
			}
		}

#if DEBUG
		public bool IsTaskCreated
		{
			get { return task != null; }
		}
#endif
	}
}
