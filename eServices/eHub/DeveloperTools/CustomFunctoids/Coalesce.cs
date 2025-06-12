using System;
using System.Reflection;
using System.Resources;
using System.Text;
using Microsoft.BizTalk.BaseFunctoids;

namespace CargoWise.eHub.DeveloperTools.CustomFunctoids
{
	public class Coalesce : BaseFunctoid
	{
		ResourceManager resourceManager = new ResourceManager("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());

		public Coalesce()
			: base()
		{
			SetupResourceAssembly("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());
			SetName("IDS_COALESCE_NAME");
			SetTooltip("IDS_COALESCE_TOOLTIP");
			SetDescription("IDS_COALESCE_DESCRIPTION");
			SetBitmap("IDB_COALESCE_BITMAP");
			this.ID = Convert.ToInt32(this.resourceManager.GetString("IDS_COALESCE_ID"));

			this.Category = FunctoidCategory.String;
			this.OutputConnectionType = ConnectionType.AllExceptRecord;
			this.AddScriptTypeSupport(ScriptType.CSharp);
			this.HasVariableInputs = true;

			this.SetMinParams(1);
			this.SetMaxParams(100);

			AddInputConnectionType(ConnectionType.AllExceptRecord);
		}

		protected override string GetInlineScriptBuffer(ScriptType scriptType, int numParams, int functionNumber)
		{
			if (ScriptType.CSharp == scriptType)
			{
				StringBuilder builder = new StringBuilder();

				builder.Append("public string Coalesce(");
				for (int i = 0; i < numParams; i++)
				{
					if (i > 0)
					{
						builder.Append(", ");
					}
					builder.Append("string val").Append(i);
				}
				builder.AppendLine(")");
				builder.AppendLine("{");
				for (int i = 0; i < numParams; i++)
				{
					builder.Append((i == 0) ? "	" : "	else ");
					builder.Append("if (!string.IsNullOrEmpty(val").Append(i).AppendLine("))");
					builder.AppendLine("	{");
					builder.Append("		return val").Append(i).AppendLine(";");
					builder.AppendLine("	}"); 
				}
				builder.AppendLine("	else {");
				builder.AppendLine("		return string.Empty;");
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
