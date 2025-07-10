using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GUI
{
	public class AttachedToDeclarationFilterOptions : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string AttachedToDeclaration = "YES";
			public const string NotAttachedToDeclaration = InvoiceHeaderWithNoDeclarationCollection.NotAttachedToDeclarationCode;
			public const string All = "ALL";
		}

		public AttachedToDeclarationFilterOptions()
		{
			AddPair(Codes.AttachedToDeclaration, Res.GetString("AttachedToDeclarationFilterOptions|AttachedToDeclaration", "Attached to a Declaration"));
			AddPair(Codes.NotAttachedToDeclaration, Res.GetString("AttachedToDeclarationFilterOptions|NotAttachedToDeclaration", "Not Attached to a Declaration"));
			AddPair(Codes.All, Res.GetString("AttachedToDeclarationFilterOptions|All", "All"));
		}
	}
}
