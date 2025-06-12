using System;
using System.Reflection;
using System.Resources;
using System.Text;
using Microsoft.BizTalk.BaseFunctoids;

namespace CargoWise.eHub.DeveloperTools.CustomFunctoids
{
	public class ConvertToBoolean : BaseFunctoid
	{
		ResourceManager resourceManager = new ResourceManager("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());

		public ConvertToBoolean()
			: base()
		{
			SetupResourceAssembly("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());
			SetName("IDS_CONVERTTOBOOLEAN_NAME");
			SetTooltip("IDS_CONVERTTOBOOLEAN_TOOLTIP");
			SetDescription("IDS_CONVERTTOBOOLEAN_DESCRIPTION");
			SetBitmap("IDB_CONVERTTOBOOLEAN_BITMAP");
			this.ID = Convert.ToInt32(this.resourceManager.GetString("IDS_CONVERTTOBOOLEAN_ID"));

			this.Category = FunctoidCategory.Logical;
			this.OutputConnectionType = ConnectionType.All;
			this.AddScriptTypeSupport(ScriptType.CSharp);
			this.RequiredGlobalHelperFunctions = InlineGlobalHelperFunction.ValToBool;
			this.RequiredGlobalHelperFunctions = InlineGlobalHelperFunction.IsNumeric;

			this.SetMinParams(1);
			this.SetMaxParams(1);

			AddInputConnectionType(ConnectionType.AllExceptRecord);
		}

		protected override string GetInlineScriptBuffer(ScriptType scriptType, int numParams, int functionNumber)
		{
			if (ScriptType.CSharp == scriptType)
			{
				StringBuilder builder = new StringBuilder();

				builder.AppendLine("public bool ConvertToBoolean(string val)");
				builder.AppendLine("{");
				builder.AppendLine("	if(ValToBool(val))");
				builder.AppendLine("	{");
				builder.AppendLine("		return true;");
				builder.AppendLine("	}");
				builder.AppendLine("	else");
				builder.AppendLine("	{");
				builder.AppendLine("		return false;");
				builder.AppendLine("	}");
				builder.AppendLine("}");

				return builder.ToString();
			}
			else
			{
				return String.Empty;
			}
		}
	}
}
