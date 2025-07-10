using System;
using CargoWise.Integration;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class CodeDescriptionPairInfo : DataObjectInfo
	{
		#region Constructors

		public CodeDescriptionPairInfo()
			: this("", "")
		{
		}

		public CodeDescriptionPairInfo(ICodeDescription codeDescription)
			: this(codeDescription.Code, codeDescription.Description)
		{
		}

		CodeDescriptionPairInfo(string code, string description)
		{
			Code = code;
			Description = description;
			CodeAndDescription = string.IsNullOrEmpty(code) ? string.Empty : code + " - " + description;
		}

		#endregion

		#region Properties

		public string Code { get; set; }
		public string Description { get; set; }
		public string CodeAndDescription { get; set; }

		#endregion
	}
}
