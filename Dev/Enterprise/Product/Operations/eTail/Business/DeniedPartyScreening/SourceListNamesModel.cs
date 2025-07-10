using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class SourceListNamesModel
	{
		public SourceListNamesModel(BusinessObjectFactory factory, string[] sourceListCodes)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(sourceListCodes, nameof(sourceListCodes));

			Factory = factory;
			SourceListCodes = sourceListCodes;

			var includedAndExcluded = DpsComplianceListHelper.GetComplianceListIncludedAndExcludedItems(factory, sourceListCodes);

			IncludedLists = includedAndExcluded.Included;
			ExcludedLists = includedAndExcluded.Excluded;
		}

		public BusinessObjectFactory Factory { get; }

		public IEnumerable<string> SourceListCodes { get; }

		public DpsComplianceListItem[] IncludedLists { get; }

		public DpsComplianceListItem[] ExcludedLists { get; } = Array.Empty<DpsComplianceListItem>();
	}
}
