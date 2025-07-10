using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AppDomainWrappers.Net;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class FreightReflectionTestHelper : TestCase
	{
		public void TestOverriddenMethodsSnapshot_ByILCodeLength_OnFactorySaving()
		{
			var expectedSnapshots = new Dictionary<string, string>()
			{
				["BillOfLading"] = "55",
				["CFSLoadListConsol"] = "121",
				["CFSService"] = "96",
				["CommonWorkSheet"] = "16",
				["ExportAWBRateLine"] = "27",
				["ForwardingConsol"] = "227",
				["ForwardingShipment"] = "334",
				["GatePassDocsAndCartage"] = "16",
				["JobDocsAndCartage"] = "36",
				["JobShipmentPreplanning"] = "62",
				["JobVoyage"] = "35",
				["Order"] = "35",
				["OrderDetailsBulkUpdateBusinessObject"] = "9",
				["OrderLine"] = "50"
			};

			AssertOverriddenMethodsSnapshots("OnFactorySaving", expectedSnapshots);
		}

		public void TestOverriddenMethodsSnapshot_ByILCodeLength_OnLoaded()
		{
			var expectedSnapshots = new Dictionary<string, string>()
			{
				["AgencyBooking"] = "45",
				["AgencyShipment"] = "73",
				["BillOfLading"] = "34",
				["CFSShipment"] = "16",
				["CommonCartage"] = "33",
				["CommonConsol"] = "55",
				["CommonShipment"] = "55",
				["ForwardingConsol"] = "232",
				["GatePassDocsAndCartage"] = "90",
				["GatePassShipment"] = "30",
				["JobVoyage"] = "47",
				["Order"] = "45",
				["PackLine"] = "118",
				["PackUnpackShipment"] = "17"
			};

			AssertOverriddenMethodsSnapshots("OnLoaded", expectedSnapshots);
		}

		#region Implementation

		void AssertOverriddenMethodsSnapshots(string methodName, Dictionary<string, string> expectedSnapshots)
		{
			var actualSnapshots = CreateSnapshotsOfOverriddenMethods(methodName);

			AssertOverriddenMethodsSnaphots(
				methodName,
				expectedSnapshots,
				actualSnapshots);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")]   // WI00669071 - Do not use System.AppDomain.
		Dictionary<string, string> CreateSnapshotsOfOverriddenMethods(string methodName)
		{
			EnsureAllFreightAssembliesAreLoaded();

			var appDomainWrapper = new AppDomainWrapper();
			var allOverriddenMethodInfos = appDomainWrapper.GetAssemblies()
					.Where(assembly => assembly.FullName.Contains("Enterprise.Freight"))
					.SelectMany(assembly => assembly.GetTypes())
					.Where(type =>
					{
						return typeof(BusinessObject).IsAssignableFrom(type)
							&& !type.FullName.Contains(".Auto")
							&& !type.FullName.Contains(".Testing");
					})
					.Select(type =>
					{
						var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						return methodInfo != null && methodInfo.DeclaringType == type
							? methodInfo
							: null;
					})
					.Where(methodInfo => methodInfo != null)
					.OrderBy(methodInfo => methodInfo.DeclaringType.Name)
					.ToArray();

			var actualAffectedTypesAndSnapshots = new Dictionary<string, string>();

			foreach (var methodInfo in allOverriddenMethodInfos)
			{
				string typeName = methodInfo.DeclaringType.Name;
				string methodSnapshot = GetMethodILCodeLength(methodInfo);

				actualAffectedTypesAndSnapshots.Add(typeName, methodSnapshot);
			}

			return actualAffectedTypesAndSnapshots;
		}

		void EnsureAllFreightAssembliesAreLoaded()
		{
			if (!freightAssembliesHaveBeenForceLoaded)
			{
				string[] allFileNamesFromBaseDirectory = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
				foreach (string fileName in allFileNamesFromBaseDirectory)
				{
					var match = Regex.Match(fileName, "^.*Enterprise\\.Freight.*\\.dll$", RegexOptions.IgnoreCase);
					if (match.Success)
					{
						var loaded = Assembly.LoadFrom(fileName);
					}
				}

				freightAssembliesHaveBeenForceLoaded = true;
			}
		}
		bool freightAssembliesHaveBeenForceLoaded;

		string GetMethodILCodeLength(MethodInfo methodInfo)
		{
			var byteArray = methodInfo.GetMethodBody().GetILAsByteArray();
			return byteArray.Length.ToString();
		}

		void AssertOverriddenMethodsSnaphots(string methodName, Dictionary<string, string> expectedSnapshots, Dictionary<string, string> actualSnapshots)
		{
			foreach (var expectedTypeMethodPair in expectedSnapshots.ToArray())
			{
				if (actualSnapshots.Contains(expectedTypeMethodPair))
				{
					actualSnapshots.Remove(expectedTypeMethodPair.Key);
					expectedSnapshots.Remove(expectedTypeMethodPair.Key);
				}
			}

			var modifiedTypes = actualSnapshots.Select(keyValue => keyValue.Key)
				.Concat(expectedSnapshots.Select(keyValue => keyValue.Key))
				.Distinct()
				.ToArray();

			string extremelyImportantMessageTemplate =
@"Looks like you've modified {0}() in {1}. 

If that is false positive - sincere apologies. Could you please adjust this test and inform IL team. 

Otherwise congratulations on your bold change! However could you please double check your design decisions and ensure performance would not be affected.

There are three possible solutions:
(1) You would realise the best course of action would be to undo changes in {0}()
(2) You would add a code review task for IL senior dev
(3) You are confident in your changes, understand implications and willing to take full responsibility

Use info below to update this test with new values if you decide to proceed.";

			CombineAssertions(string.Format(extremelyImportantMessageTemplate, methodName, string.Join(", ", modifiedTypes)), () =>
			{
				AssertContainsExactElementsInAnyOrder(expectedSnapshots, actualSnapshots);
			});
		}

		#endregion
	}
}
