using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	static class XmlProcessIntegrationTestHelper
	{
		internal static IEnumerable<TestCaseData> GetTestedClasses()
		{
			var testedClassInfos = new List<TestCaseData>();

			var baseStagingPath = AppDomain.CurrentDomain.BaseDirectory;
			var baseProducerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\UniversalXMLProducers\net8.0");

			testedClassInfos.AddRange(GetTestedTypes(Assembly.GetExecutingAssembly().Location).Select(testedType =>
			{
				PathOfTypeDictionary.Add(testedType, baseStagingPath);
				return new TestCaseData(testedType).SetName(testedType.Name);
			}));

			var baseProducerDirectoryInfo = new DirectoryInfo(baseProducerPath);
			var producerTestAssemblyFiles = baseProducerDirectoryInfo.GetFiles("CargoWise.RefDbRepo*Test*.dll");
			foreach (var producerTestAssembly in producerTestAssemblyFiles)
			{
				testedClassInfos.AddRange(GetTestedTypes(producerTestAssembly.FullName).Select(testedType =>
				{
					PathOfTypeDictionary.Add(testedType, baseProducerPath);
					return new TestCaseData(testedType).SetName(testedType.Name);
				}));
			}

			return testedClassInfos;
		}

		static IEnumerable<Type> GetTestedTypes(string assemblyFile)
		{
			var types = Assembly.LoadFrom(assemblyFile).GetTypes();
			var testedTypes = types.Where(type => type.GetInterface(nameof(IXmlProcessIntegrationTest)) != null);
			return testedTypes;
		}

		internal static readonly Dictionary<Type, string> PathOfTypeDictionary = new Dictionary<Type, string>();
	}
}
