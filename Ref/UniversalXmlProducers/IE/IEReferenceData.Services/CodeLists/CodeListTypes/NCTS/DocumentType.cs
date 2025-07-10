using CargoWise.RefDbRepo.IEReferenceData.Services;
using System;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS
{
    public class DocumentType : RevenueCodeListDetails, IRevenueCodeListDetails
	{
		public ApplicationType ApplicationType => ApplicationType.NCTS;

		public string Code => Constants.NctsCodeTypes.DocumentTypeType;

		public string NameInFile => "CL215 – CL Document Type";

		public string TableTitleInFile => "Code Name / description ";

		public string CodeFormattingRegularExpression => "Y([0-9]{3})";
	}
}
