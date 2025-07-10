using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class TWJobDeclarationDataObjectWriter : DeclarationDataObjectWriter
	{
		public TWJobDeclarationDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
		}

		IEnumerable<ZGuid> cAHeaderPKsToPopulate;
		public void SetCAHeaderPKsToPopulate(IEnumerable<ZGuid> pksToPopulate)
		{
			cAHeaderPKsToPopulate = pksToPopulate;
		}

		protected override CustomsEntryInstructionDataObjectWriter GetNewCustomsEntryInstructionDataObjectWriter()
		{
			return new TWEntryInstructionDataObjectWriter(writeManager, helper, cAHeaderPKsToPopulate);
		}

		protected override CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.Business.CusEntryHeader relatedEntry)
		{
			return new TWInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		protected override UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
		{
			return new TWDataObjectWriterHelper(declarationBO.Factory);
		}

		protected override IEnumerable<IFetchHint> GetEntryInstructionRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetEntryInstructionRelatedFetchHints(row))
			{
				yield return hint;
			}
			var pk = row.GetValue(CusEntryInstructionSchema.PK);
			yield return new FetchHint(StmNoteSchema.ST_ParentID, pk);
		}

		protected override void PopulateExtraOrganisation(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			base.PopulateExtraOrganisation(declarationBO, declarationData, keepExistingData);
			var declarationTW = declarationBO as JobDeclaration;
			if (declarationTW != null)
			{
				foreach (var org in declarationTW.BondedFactories)
				{
					var addr = org.Address;
					if (addr != null)
					{
						declarationData.AddOrgAddress(writeManager, addr, Constants.AddressTypes.PreviousBondedFactory);
					}
				}
				var notifyParty = declarationTW.NotifyParty;
				if (notifyParty != null)
				{
					declarationData.AddOrgAddress(writeManager, notifyParty, Constants.AddressTypes.NotifyParty);
				}
			}
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var hint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return hint;
			}

			var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(JobComInvLineRefsSchema.JG_JI, invoiceLinePK);
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter()
		{
			return new CustomsEntryHeaderDataObjectWriter(writeManager, helper);
		}

		protected override List<EntryNumber> PopulateCusDisposition(BaseJobDeclaration declarationBO)
		{
			var result = base.PopulateCusDisposition(declarationBO);
			if (declarationBO is JobDeclaration declaration && declaration.CusEntryInstruction is Business.CusEntryInstruction entryInstruction)
			{
				result.Add(new EntryNumber()
				{
					Number = entryInstruction.UCRNumber,
					Type = new EntryType() { Code = CusEntryNumberTypes.Standard.UniqueConsignementReference },
					EntryIsSystemGenerated = !entryInstruction.UCROverride
				});
			}
			return result;
		}
	}
}
