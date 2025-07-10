using Enterprise.MasterFiles.Integration.Reference;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class IATASpecialHandlingCodesProvider : IIATASpecialHandlingCodesProvider
	{
		public CodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return new AWBSpecialHandlingCodeDescriptionPairList();
		}
	}
}
