using System.Text;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	static class ZPropertyInfoHelpers
	{
		public static void AddIsNotRequiredMessageError(this ZPropertyInfo info, string because = null)
		{
			var sb = new StringBuilder();
			sb.Append(string.IsNullOrWhiteSpace(because) ? (NoResString)"You" : because);
			sb.Append((NoResString)" do not require the ");
			sb.Append(info.HumanReadableName);
			sb.Append(".");
			info.AddMessageError(sb.ToString());
		}
	}
}
