using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	public partial class AENS32 : Abstract.AENS32, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			ZString linkedEntryNumber = ReleaseEntryFilerCode1 + ReleaseEntryNumber1;
			AddLinkedEntryNumber(declaration, linkedEntryNumber);

			linkedEntryNumber = ReleaseEntryFilerCode2 + ReleaseEntryNumber2;
			AddLinkedEntryNumber(declaration, linkedEntryNumber);

			linkedEntryNumber = ReleaseEntryFilerCode3 + ReleaseEntryNumber3;
			AddLinkedEntryNumber(declaration, linkedEntryNumber);

			linkedEntryNumber = ReleaseEntryFilerCode4 + ReleaseEntryNumber4;
			AddLinkedEntryNumber(declaration, linkedEntryNumber);

			linkedEntryNumber = ReleaseEntryFilerCode5 + ReleaseEntryNumber5;
			AddLinkedEntryNumber(declaration, linkedEntryNumber);

			linkedEntryNumber = ReleaseEntryFilerCode6 + ReleaseEntryNumber6;
			AddLinkedEntryNumber(declaration, linkedEntryNumber);

			var uniqueEntryNumbers = entryNumbers.Distinct().ToList();

			var headers = declaration.Invoices;
			var invoiceCount = headers == null ? 0 : headers.Count;
			var entryNoCount = uniqueEntryNumbers.Count;

			if (entryNoCount <= invoiceCount)
			{
				var importedCount = 0;
				foreach (var entryNumber in uniqueEntryNumbers)
				{
					headers[importedCount].IsRetrievingInvoiceFromReleaseEntry = false;
					headers[importedCount].US_ReleaseEntryNumber = entryNumber;
					importedCount++;
				}
			}
			else
			{
				var importedCount = 0;
				var headersArray = headers.ToArray();
				foreach (JobComInvoiceHeader header in headersArray)
				{
					header.IsRetrievingInvoiceFromReleaseEntry = false;
					header.US_ReleaseEntryNumber = uniqueEntryNumbers[importedCount];
					importedCount++;
				}

				var remainCount = entryNoCount - importedCount;
				for (var i = 0; i < remainCount; i++)
				{
					var newInvoice = headers.AddNew();
					newInvoice.US_ReleaseEntryNumber = uniqueEntryNumbers[importedCount];
					importedCount++;
				}
			}
		}

		void AddLinkedEntryNumber(JobDeclaration declaration, ZString linkedEntryNumber)
		{
			if (!linkedEntryNumber.IsEmpty)
			{
				var releaseDec = USReleaseDeclarationLoader.GetReleaseDeclaration(declaration.Factory, linkedEntryNumber, declaration.JE_GC);
				if (releaseDec != null && (releaseDec.US_ConsolidatedJobNumber == ZString.Empty || releaseDec.US_ConsolidatedJobNumber == declaration.JE_DeclarationReference))
				{
					entryNumbers.Add(linkedEntryNumber);
				}
			}
		}

		readonly List<ZString> entryNumbers = new List<ZString>();
		#endregion
	}
}
