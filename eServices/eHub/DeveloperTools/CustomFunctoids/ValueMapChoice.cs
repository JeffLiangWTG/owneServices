using System;
using System.Reflection;
using System.Resources;
using System.Text;
using Microsoft.BizTalk.BaseFunctoids;

namespace CargoWise.eHub.DeveloperTools.CustomFunctoids
{
	public class ValueMapChoice : BaseFunctoid
	{
		ResourceManager resourceManager = new ResourceManager("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());

		public ValueMapChoice()
			: base()
		{
			SetupResourceAssembly("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());
			SetName("IDS_VALUEMAPCHOICE_NAME");
			SetTooltip("IDS_VALUEMAPCHOICE_TOOLTIP");
			SetDescription("IDS_VALUEMAPCHOICE_DESCRIPTION");
			SetBitmap("IDB_VALUEMAPCHOICE_BITMAP");
			this.ID = Convert.ToInt32(this.resourceManager.GetString("IDS_VALUEMAPCHOICE_ID"));

			this.Category = FunctoidCategory.String;
			this.OutputConnectionType = ConnectionType.All;
			this.AddScriptTypeSupport(ScriptType.CSharp);
			this.RequiredGlobalHelperFunctions = InlineGlobalHelperFunction.ValToBool;

			this.SetMinParams(3);
			this.SetMaxParams(3);

			AddInputConnectionType(ConnectionType.FunctoidLogical);
			AddInputConnectionType(ConnectionType.AllExceptRecord);
			AddInputConnectionType(ConnectionType.AllExceptRecord);
		}

		protected override string GetInlineScriptBuffer(ScriptType scriptType, int numParams, int functionNumber)
		{
			if (ScriptType.CSharp == scriptType)
			{
				StringBuilder builder = new StringBuilder();

				builder.AppendLine("public string ValueMapChoice(string choice, string valTrue, string valFalse)");
				builder.AppendLine("{");
				builder.AppendLine("	if (ValToBool(choice))");
				builder.AppendLine("	{");
				builder.AppendLine("		return valTrue;");
				builder.AppendLine("	}");
				builder.AppendLine("	else");
				builder.AppendLine("	{");
				builder.AppendLine("		return valFalse;");
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
