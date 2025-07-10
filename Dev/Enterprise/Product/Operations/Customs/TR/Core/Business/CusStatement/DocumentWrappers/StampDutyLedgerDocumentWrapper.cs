using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business
{
	public class StampDutyLedgerDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, IVisualizerNoteSupporter
	{
		public StampDutyLedgerDocumentWrapper(CusStatementHeader statementHeader)
		{
			this.stampDutyLedgerPrint = statementHeader;
		}

		readonly CusStatementHeader stampDutyLedgerPrint;

		public ZString PeriodStartDate => stampDutyLedgerPrint.B2_PeriodStartDate.ToString("dd/MM/yyyy");

		public ZString PeriodEndDate => stampDutyLedgerPrint.B2_PeriodEndDate.ToString("dd/MM/yyyy");

		public ZString PrintDate => stampDutyLedgerPrint.B2_PrintDate.ToString("dd/MM/yyyy");

		public BusinessObjectCollectionWrapper<StampDutyLedgerChargesWrapper> Charges
		{
			get
			{
				var chargesWrapper = new List<StampDutyLedgerChargesWrapper>();
				foreach (CusStatementLine line in stampDutyLedgerPrint.StatementLines)
				{
					foreach (CusStatementLineCharge charge in line.Charges)
					{
						chargesWrapper.Add(new StampDutyLedgerChargesWrapper(charge));
					}
				}
				return new BusinessObjectCollectionWrapper<StampDutyLedgerChargesWrapper>(chargesWrapper);
			}
		}

		ZGuid IVisualizerNoteSupporter.PK => stampDutyLedgerPrint.PK;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => CusStatementHeaderSchema.Constants.Prefix;
	}
}
