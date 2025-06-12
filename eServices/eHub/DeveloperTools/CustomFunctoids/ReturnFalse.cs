using System;
using System.Reflection;
using System.Text;
using Microsoft.BizTalk.BaseFunctoids;
using System.Resources;

namespace CargoWise.eHub.DeveloperTools.CustomFunctoids
{
	public class ReturnFalse : BaseFunctoid
	{
		ResourceManager resourceManager = new ResourceManager("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());

		public ReturnFalse()
			: base()
		{
			SetupResourceAssembly("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());
			SetName("IDS_RETURNFALSE_NAME");
			SetTooltip("IDS_RETURNFALSE_TOOLTIP");
			SetDescription("IDS_RETURNFALSE_DESCRIPTION");
			SetBitmap("IDB_RETURNFALSE_BITMAP");
			this.ID = Convert.ToInt32(resourceManager.GetString("IDS_RETURNFALSE_ID"));

			this.Category = FunctoidCategory.Logical;
			this.OutputConnectionType = ConnectionType.All;
			this.AddScriptTypeSupport(ScriptType.CSharp);

			this.SetMinParams(0);
			this.SetMaxParams(0);
		}

		protected override string GetInlineScriptBuffer(ScriptType scriptType, int numParams, int functionNumber)
		{
			if (ScriptType.CSharp == scriptType)
			{
				StringBuilder builder = new StringBuilder();

				builder.Append("public bool ReturnFalse()\n");
				builder.Append("{\n");
				builder.Append("	return false;\n");
				builder.Append("}\n");

				return builder.ToString();
			}
			else
			{
				return String.Empty;
			}
		}
	}
}
