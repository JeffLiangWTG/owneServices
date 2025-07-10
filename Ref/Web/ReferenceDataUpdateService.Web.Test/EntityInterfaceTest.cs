using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace ReferenceDataUpdateService.Web.Test
{
	[TestFixture]
	public class EntityInterfaceTest
	{
		[Test]
		public void TestEntityInterface()
		{
			var rootPath = TestSourcePathHelper.DATTestSupplementaryContentPath;
			var modelsPath = Path.Combine(rootPath, @"Web\ReferenceDataUpdateService.Web\src\models");
			var filesArray = Directory.GetFiles(modelsPath, "*.ts");
			var pattern = @"interface (?<name>(\w)+) extends IEntity(\s)*{(?<properties>.+?)}";
			Regex regex = new Regex(pattern);
			foreach (var file in filesArray)
			{
				var contents = File.ReadAllText(file).Replace("\r\n", "\t").Replace("\n", "\t");
				if (regex.IsMatch(contents))
				{
					var match = regex.Match(contents);
					var interfaceName = match.Groups["name"].Value;
					var propertyContents = match.Groups["properties"].Value;
					if (IgnoreInterfaceList.Contains(interfaceName))
					{
						continue;
					}
					AssertInterfaceShouldBeSynced(interfaceName, propertyContents);
				}
			}
		}

		void AssertInterfaceShouldBeSynced(string interfaceName, string propertyContents)
		{
			var propertyList = GetInterfacePropertyList(propertyContents);
			var properties = GetClassProperties(interfaceName);
			Assert.True(properties.Count > 0, $"Cannot find the class corresponding to interface {interfaceName}");
			foreach (var property in properties)
			{
				Assert.Contains(property, propertyList, $"Interface {interfaceName} does not contain {property}.");
			}
		}

		List<string> GetInterfacePropertyList(string contents)
		{
			var propertyList = new List<string>();
			var lines = contents.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				var property = line.Substring(0, line.IndexOf(":")).Trim();
				propertyList.Add(property);
			}
			return propertyList;
		}

		List<string> GetClassProperties(string className)
		{
			var properties = new List<string>();
			if (className.StartsWith("I"))
			{
				className = className.Substring(1);
			}
			var assemblyName = "CargoWise.RefDbRepo.Service.Schema_0_9_New";
			Type type = Assembly.Load(assemblyName).GetType($"{assemblyName}.{className}");
			if (type != null)
			{
				var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				foreach (var propertyInfo in propertyInfos)
				{
					var propertyTypeName = propertyInfo.PropertyType.FullName;
					var propertyName = propertyInfo.Name;
					if (propertyTypeName.StartsWith("System") && !propertyTypeName.StartsWith("System.Collections") && !propertyName.EndsWith("SysStartTime") && !propertyName.EndsWith("SysEndTime"))
					{
						properties.Add(propertyInfo.Name);
					}
				}
			}
			return properties;
		}

		readonly List<string> IgnoreInterfaceList = new List<string>() { "IQrtzJobDetails", "IRefApplicationAttribute", "IRefApplicationAttributeType", "ISourceDataUserView", "IProcessorStatus" };
	}
}
