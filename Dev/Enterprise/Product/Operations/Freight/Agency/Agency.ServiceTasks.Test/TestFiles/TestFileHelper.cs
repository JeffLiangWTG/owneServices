using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Tests that consume the helper have SOURCE_CODE")]
	static class TestFileHelper
	{
		public static class COARRI
		{
			const string prefix = "COARRI\\";
			public static string GetResultBody1(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody1.htm"), mappings);
			}

			public static string GetResultBody2_ACK(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody2_ACK.htm"), mappings);
			}

			public static string GetResultBody2_DSC(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody2_DSC.htm"), mappings);
			}

			public static string GetResultBody3(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody3.htm"), mappings);
			}
		}

		public static class CODECO
		{
			const string prefix = "CODECO\\";
			public static string GetInboundEmail()
			{
				return GetText(prefix + "InboundEmail.eml");
			}

			public static string GetResultBody1(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody1.htm"), mappings);
			}

			public static string GetResultBody2(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody2.htm"), mappings);
			}

			public static string GetResultBody3(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody3.htm"), mappings);
			}

			public static string GetResultBody4(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody4.htm"), mappings);
			}

			public static string GetResultBody5(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody5.htm"), mappings);
			}

			public static string GetResultBody6(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody6.htm"), mappings);
			}

			public static string GetResultBody7(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody7.htm"), mappings);
			}

			public static string GetResultBody8(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody8.htm"), mappings);
			}

			public static string GetResultBody9(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody9.htm"), mappings);
			}

			public static string GetResultBody10(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody10.htm"), mappings);
			}

			public static string GetResultBody11(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody11.htm"), mappings);
			}

			public static string GetResultBody12_DSC(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody12_DSC.htm"), mappings);
			}

			public static string GetResultBody12_ACK(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody12_ACK.htm"), mappings);
			}

			public static string GetResultBody13(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody13.htm"), mappings);
			}

			public static string GetResultBody14(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultBody14.htm"), mappings);
			}

			public static string GetResultDscVesselNull(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultDscVesselNull.htm"), mappings);
			}

			public static string GetResultDscVoyageNull(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultDscVoyageNull.htm"), mappings);
			}

			public static string GetResultDscVoyageVesselIsNullOrEmpty(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultDscVoyageVesselIsNullOrEmpty.htm"), mappings);
			}

			public static string GetResultUpdateSeals_Ack(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultUpdateSeals_Ack.htm"), mappings);
			}

			public static string GetResultUpdateSeals_Dsc(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultUpdateSeals_Dsc.htm"), mappings);
			}

			public static string GetResultVerifySeals(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultVerifySeals.htm"), mappings);
			}

			public static string GetResultVerifySealsDbEmpty(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultVerifySealsDbEmpty.htm"), mappings);
			}

			public static string GetResultVerifySealsMissMatch(IDictionary<string, string> mappings)
			{
				return MappingReplacement(GetText(prefix + "ResultVerifySealsMissMatch.htm"), mappings);
			}
		}

		public static class EIDO
		{
			const string prefix = "E-IDO\\";
			public static string GetFailure1()
			{
				return GetText(prefix + "Failure1.htm");
			}
		}

		public static class FailedMessageHtmlBuilder
		{
			const string prefix = "FailedMessageHtmlBuilder\\";
			public static string GetSample()
			{
				return GetText(prefix + "Sample.htm");
			}
		}

		public static string GetCMMErrorResultBody()
		{
			return GetText("CMMErrorResultBody.txt");
		}

		#region Internal
		static string GetText(string filename)
		{
			string absolutePath = Path.Combine(Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Operations\Freight\Agency\Agency.ServiceTasks\TestFiles"), filename);
			if (!File.Exists(absolutePath))
			{
				throw new FileNotFoundException(string.Format("cannot find the test file '{0}'", absolutePath), absolutePath);
			}

			using (StreamReader reader = new StreamReader(absolutePath))
			{
				return reader.ReadToEnd();
			}
		}

		static string MappingReplacement(string template, IDictionary<string, string> mappings)
		{
			if (mappings == null)
			{
				throw new ArgumentNullException(nameof(mappings));
			}

			return placeholder.Replace(template, delegate(Match match)
			{
				string key = match.ToString();
				string value;
				if (mappings.TryGetValue(key, out value))
				{
					return value;
				}
				else
				{
					throw new InvalidOperationException(string.Format("Un-mapped place holder '{0}'.", key));
				}
			});
		}

		static readonly Regex placeholder = new Regex(@"\(\*[A-Z0-9]+\*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		#endregion
	}
}
