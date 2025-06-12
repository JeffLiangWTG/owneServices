using System;
using System.Reflection;
using System.Resources;
using System.Text;
using Microsoft.BizTalk.BaseFunctoids;

namespace CargoWise.eHub.DeveloperTools.CustomFunctoids
{
	public class TrueForAll : BaseFunctoid
	{
		ResourceManager resourceManager = new ResourceManager("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());

		public TrueForAll()
			: base()
		{
			SetupResourceAssembly("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());
			SetName("IDS_TRUEFORALL_NAME");
			SetTooltip("IDS_TRUEFORALL_TOOLTIP");
			SetDescription("IDS_TRUEFORALL_DESCRIPTION");
			SetBitmap("IDB_TRUEFORALL_BITMAP");
			this.ID = Convert.ToInt32(this.resourceManager.GetString("IDS_TRUEFORALL_ID"));

			this.Category = FunctoidCategory.Cumulative;
			this.OutputConnectionType = ConnectionType.All;
			this.RequiredGlobalHelperFunctions = InlineGlobalHelperFunction.ValToBool;

			SetScriptGlobalBuffer(ScriptType.CSharp, "public System.Collections.ArrayList trueForAllArray = new System.Collections.ArrayList();\n");
			SetScriptBuffer(ScriptType.CSharp, GetCSharpInitBuffer(), 0);
			SetScriptBuffer(ScriptType.CSharp, GetCSharpAddToBuffer(), 1);
			SetScriptBuffer(ScriptType.CSharp, GetCSharpGetBuffer(), 2);

			SetMinParams(1);
			SetMaxParams(2);

			AddInputConnectionType(ConnectionType.All);
			AddInputConnectionType(ConnectionType.AllExceptRecord);
		}

		//Init Function
		private string GetCSharpInitBuffer()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("public string InitTrueForAll(int index)\n");
			builder.Append("{\n");
			builder.Append("	if (index >= 0)\n");
			builder.Append("	{\n");
			builder.Append("		if (index >= trueForAllArray.Count)\n");
			builder.Append("		{\n");
			builder.Append("			trueForAllArray.Add(true);\n");
			builder.Append("		}\n");
			builder.Append("		else\n");
			builder.Append("		{\n");
			builder.Append("			trueForAllArray[index] = true;\n");
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

			builder.Append("public string AddToTrueForAll(int index, string val, string reserved)\n");
			builder.Append("{\n");
			builder.Append("	if (index < 0 || index >= trueForAllArray.Count)\n");
			builder.Append("	{\n");
			builder.Append("		return \"\";\n");
			builder.Append("	}\n");
			builder.Append("	if (!ValToBool(val))\n");
			builder.Append("	{\n");
			builder.Append("		trueForAllArray[index] = false;\n");
			builder.Append("	}\n");
			builder.Append("	return \"\";\n");
			builder.Append("}\n");

			return builder.ToString();
		}

		//Get Function
		private string GetCSharpGetBuffer()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("public bool GetTrueForAll(int index)\n");
			builder.Append("{\n");
			builder.Append("	if (index < 0 || index >= trueForAllArray.Count)\n");
			builder.Append("	{\n");
			builder.Append("		return false;\n");
			builder.Append("	}\n");
			builder.Append("	return (bool)trueForAllArray[index];\n");
			builder.Append("}\n");

			return builder.ToString();
		}
	}
}
