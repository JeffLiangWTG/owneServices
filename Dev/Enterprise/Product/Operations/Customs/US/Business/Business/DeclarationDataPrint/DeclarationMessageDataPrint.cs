using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public class DeclarationMessageDataPrint : DeclarationDataPrint
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public DeclarationMessageDataPrint(JobDeclaration declaration, IEnumerable<(ICusEntryLine entryLine, List<MessageBlock> messages)> entryLineMessageBlockCollection)
			: base(declaration)
		{
			EntryLineMessageBlockCollection = Argument.NotNull(entryLineMessageBlockCollection, "Entry Line Message Block Collection");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<(ICusEntryLine entryLine, List<MessageBlock> messages)> EntryLineMessageBlockCollection { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ZString[] PGARecapLines
		{
			get
			{
				if (pGARecapLinesCached == null)
				{
					pGARecapLinesCached = new CachedProperty<ZString[]>(Factory, () =>
					{
						var pagWithEntryLines = PGARecapPrinting.PGARecapLineGenerator.GenerateLines(EntryLineMessageBlockCollection, Declaration.Factory).ToList();
						if (pagWithEntryLines.Count > 1 && pagWithEntryLines.LastOrDefault().IsEmpty)
						{
							pagWithEntryLines.RemoveAt(pagWithEntryLines.Count - 1);
						}
						return pagWithEntryLines.ToArray();
					});
				}
				return pGARecapLinesCached.Value;
			}
		}
		CachedProperty<ZString[]> pGARecapLinesCached;
	}
}
