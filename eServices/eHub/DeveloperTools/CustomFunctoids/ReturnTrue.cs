using System;
using System.Reflection;
using System.Text;
using Microsoft.BizTalk.BaseFunctoids;
using System.Resources;

namespace CargoWise.eHub.DeveloperTools.CustomFunctoids
{
	public class ReturnTrue : BaseFunctoid
	{
		ResourceManager resourceManager = new ResourceManager("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());

		public ReturnTrue()
			: base()
		{
			SetupResourceAssembly("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());
			SetName("IDS_RETURNTRUE_NAME");
			SetTooltip("IDS_RETURNTRUE_TOOLTIP");
			SetDescription("IDS_RETURNTRUE_DESCRIPTION");
			SetBitmap("IDB_RETURNTRUE_BITMAP");
			this.ID = Convert.ToInt32(this.resourceManager.GetString("IDS_RETURNTRUE_ID"));

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

				builder.Append("public bool ReturnTrue()\n");
				builder.Append("{\n");
				builder.Append("	return true;\n");
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
