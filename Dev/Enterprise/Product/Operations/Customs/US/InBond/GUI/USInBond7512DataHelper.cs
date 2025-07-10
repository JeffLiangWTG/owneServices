using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.InBond.GUI
{
	internal class USInBond7512DataHelper
	{
		public static DeliveryInstructionDestination Print7512DepartureBulk(IEnumerable<CusInBondMoveHeader> selectedMoveHeaders)
		{
			var destination = DeliveryInstructionDestination.None;
			if (selectedMoveHeaders.Any())
			{
				var docPack = new DocumentPack();
				var factory = new BusinessObjectFactory();
				var template = GetTemplate(factory);
				if (template != null)
				{
					foreach (var moveHeader in selectedMoveHeaders)
					{
						docPack.Add(GetReport(docPack, template, factory.Load<CusInBondMoveHeader>(moveHeader.PK)));
					}
					using var printTask = new PrintTask();
					printTask.Add(docPack);
					destination = printTask.Run(Env.Security.USInBondEdit);
				}
			}
			else
			{
				Globals.Message.ShowError("Please select at least one movement header to print.", "No Movement Selected");
			}
			return destination;
		}

		static Report GetReport(DocumentPack docPack, ExcelTemplate template, CusInBondMoveHeader moveHeader)
		{
			return new Report(docPack, template, BODocDataProvider.Get(new CBP7512Document(moveHeader)), "7512 Departure", null, DocumentDirection.ANY, false);
		}

		static ExcelTemplate GetTemplate(BusinessObjectFactory factory)
		{
			return ExcelTemplateRetriever.GetTemplate(CusInBondHeaderDocumentSupporter.CBPForm7512TemplateName, CusInBondHeaderDocumentSupporter.CBPForm7512DataContextValue, factory) ?? throw new ZException("Template(s) used for this document has been deleted from the system. Documents not printed.");
		}
	}
}
