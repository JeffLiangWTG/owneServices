using System;
using System.Reflection;
using System.Resources;
using System.Text;
using Microsoft.BizTalk.BaseFunctoids;

namespace CargoWise.eHub.DeveloperTools.CustomFunctoids
{
	public class FormatDateTime : BaseFunctoid
	{
		ResourceManager resourceManager = new ResourceManager("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());

		public FormatDateTime()
			: base()
		{
			SetupResourceAssembly("CargoWise.eHub.DeveloperTools.CustomFunctoids.Properties.Resources", Assembly.GetExecutingAssembly());
			SetName("IDS_FORMATDATETIME_NAME");
			SetTooltip("IDS_FORMATDATETIME_TOOLTIP");
			SetDescription("IDS_FORMATDATETIME_DESCRIPTION");
			SetBitmap("IDB_FORMATDATETIME_BITMAP");
			this.ID = Convert.ToInt32(this.resourceManager.GetString("IDS_FORMATDATETIME_ID"));

			this.Category = FunctoidCategory.DateTime;
			this.OutputConnectionType = ConnectionType.All;
			this.AddScriptTypeSupport(ScriptType.CSharp);

			this.SetMinParams(3);
			this.SetMaxParams(3);

			AddInputConnectionType(ConnectionType.AllExceptRecord);
		}

		protected override string GetInlineScriptBuffer(ScriptType scriptType, int numParams, int functionNumber)
		{
			if (ScriptType.CSharp == scriptType)
			{
				StringBuilder builder = new StringBuilder();

				builder.Append("public string FormatDateTime(string val, string inFmts, string outFmt)\n");
				builder.Append("{\n");
				builder.Append("	DateTime parsedDate;\n");
				builder.Append("	if (DateTime.TryParseExact(val, inFmts.Split(new char[] {';'}), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))\n");
				builder.Append("	{\n");
				builder.Append("		return parsedDate.ToString(outFmt);\n");
				builder.Append("	}\n");
				builder.Append("	else");
				builder.Append("	{\n");
				builder.Append("				try\n");
				builder.Append("				{\n");
				builder.Append("					parsedDate = XmlConvert.ToDateTime(val, XmlDateTimeSerializationMode.Unspecified);\n");
				builder.Append("				}\n");
				builder.Append("				catch\n");
				builder.Append("				{\n");
				builder.Append("					return string.Empty;\n");
				builder.Append("				};\n");
				builder.Append("				return parsedDate.ToString(outFmt);\n");
				builder.Append("	}\n");
				builder.Append("}\n");
				builder.Append("\n");

				return builder.ToString();
			}
			else
			{
				return String.Empty;
			}
		}
	}
}
