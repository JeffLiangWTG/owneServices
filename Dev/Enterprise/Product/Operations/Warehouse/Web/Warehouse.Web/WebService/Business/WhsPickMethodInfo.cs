using System;
using System.Linq;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsPickMethodInfo : DataObjectInfo
	{
		#region Constructors

		public WhsPickMethodInfo()
			: this("", "", false)
		{
		}

		public WhsPickMethodInfo(string code, string description, bool isDefault)
		{
			this.code = code;
			this.description = description;
			this.isDefault = isDefault;
		}

		#endregion

		#region Properties

		public string Code
		{
			get { return code; }
			set { code = value; }
		}

		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		public bool IsDefault
		{
			get { return isDefault; }
			set { isDefault = value; }
		}

		#endregion

		#region Implementation

		string code;
		string description;
		bool isDefault;

		internal static WhsPickMethodInfo GetAnyCodePickMethod(ICodeDescriptionPairListWithDefaultCode registryPickMethods)
			=> new WhsPickMethodInfo
			{
				Code = WhsAreaAndPickMethodHelper.AnyCode,
				Description = Res.GetString("1fee7209-e82b-49e5-b51c-cd3b5af85eef", "ANY"),
				IsDefault = registryPickMethods
					.Cast<ICodeDescriptionBool>()
					.SingleOrDefault(method => WhsAreaAndPickMethodHelper.IsAnyCode(method.Code))?.Bool ?? false
			};

		#endregion
	}
}
