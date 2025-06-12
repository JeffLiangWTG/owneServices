using System;
using System.Reflection;
using System.Resources;
using System.Text;
using Microsoft.BizTalk.BaseFunctoids;

namespace CargoWise.eHub.DeveloperTools.CustomFunctoids
{
	public class TrueForAny : BaseFunctoid
	{
		ResourceManager resourceManager = new ResourceManager("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());

		public TrueForAny()
			: base()
		{
			SetupResourceAssembly("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());
			SetName("IDS_TRUEFORANY_NAME");
			SetTooltip("IDS_TRUEFORANY_TOOLTIP");
			SetDescription("IDS_TRUEFORANY_DESCRIPTION");
			SetBitmap("IDB_TRUEFORANY_BITMAP");
			this.ID = Convert.ToInt32(this.resourceManager.GetString("IDS_TRUEFORANY_ID"));

			this.Category = FunctoidCategory.Cumulative;
			this.OutputConnectionType = ConnectionType.All;
			this.RequiredGlobalHelperFunctions = InlineGlobalHelperFunction.ValToBool;

			SetScriptGlobalBuffer(ScriptType.CSharp, "public System.Collections.ArrayList trueForAnyArray = new System.Collections.ArrayList();\n");
			SetScriptBuffer(ScriptType.CSharp, GetCSharpInitBuffer(), 0);
			SetScriptBuffer(ScriptType.CSharp, GetCSharpAddToBuffer(), 1);
			SetScriptBuffer(ScriptType.CSharp, GetCSharpGetBuffer(), 2);

			SetMinParams(1);
			SetMaxParams(2);

			AddInputConnectionType(ConnectionType.AllExceptRecord);
			AddInputConnectionType(ConnectionType.AllExceptRecord);
		}

		//Init Function
		private string GetCSharpInitBuffer()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("public string InitTrueForAny(int index)\n");
			builder.Append("{\n");
			builder.Append("	if (index >= 0)\n");
			builder.Append("	{\n");
			builder.Append("		if (index >= trueForAnyArray.Count)\n");
			builder.Append("		{\n");
			builder.Append("			trueForAnyArray.Add(false);\n");
			builder.Append("		}\n");
			builder.Append("		else\n");
			builder.Append("		{\n");
			builder.Append("			trueForAnyArray[index] = false;\n");
			builder.Append("		}\n");
			builder.Append("	}\n");
			builder.Append("	return \"\";\n");
			builder.Append("}\n");

			return builder.ToString();
		}

		//Cumulute Function
		private string GetCSharpAddToBuffer()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("public string AddToTrueForAny(int index, string val, string reserved)\n");
			builder.Append("{\n");
			builder.Append("	if (index < 0 || index >= trueForAnyArray.Count)\n");
			builder.Append("	{\n");
			builder.Append("		return \"\";\n");
			builder.Append("	}\n");
			builder.Append("	if (ValToBool(val))\n");
			builder.Append("	{\n");
			builder.Append("		trueForAnyArray[index] = true;\n");
			builder.Append("	}\n");
			builder.Append("	return \"\";\n");
			builder.Append("}\n");

			return builder.ToString();
		}

		//Get Function
		private string GetCSharpGetBuffer()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("public bool GetTrueForAny(int index)\n");
			builder.Append("{\n");
			builder.Append("	if (index < 0 || index >= trueForAnyArray.Count)\n");
			builder.Append("	{\n");
			builder.Append("		return false;\n");
			builder.Append("	}\n");
			builder.Append("	return (bool)trueForAnyArray[index];\n");
			builder.Append("}\n");

			return builder.ToString();
		}
	}
}
