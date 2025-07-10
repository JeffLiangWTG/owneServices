using System.Linq;
using CargoWise.Integration;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public class CodeDescriptionPairInfoCollection : DataObjectInfoCollection<CodeDescriptionPairInfo>
	{
		public CodeDescriptionPairInfoCollection()
		{
		}

		public CodeDescriptionPairInfoCollection(ICodeDescriptionPairList codeDescriptions)
		{
			AddRange(codeDescriptions);
		}

		#region AddRange

		public void AddRange(ICodeDescriptionPairList codeDescriptions)
		{
			AddRange(
				codeDescriptions
				.OfType<ICodeDescription>()
				.Select(codeDescription
					=> new CodeDescriptionPairInfo(codeDescription)));
		}

		#endregion
	}
}
