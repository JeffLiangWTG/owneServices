using System;

namespace Enterprise.MasterFiles.Business
{
	public interface IEDocsParsingSupport
	{
		string UtilityData { get; }

		bool DenySendForParsing(Guid docPK, string docType, string fileName);
	}
}
