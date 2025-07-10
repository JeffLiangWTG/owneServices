using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing
{
	[TestFixture]
	public class CodeListsParserTests
	{
		[Test]
		public void TestDataSourceIsUnique()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var businessAssembly = Assembly.LoadFile(Path.Combine(binPath, "CargoWise.RefDbRepo.DEReferenceData.Business.dll"));
			var types = businessAssembly.GetTypes().Where(t => !t.IsAbstract);
			var errorMessageBuilder = new StringBuilder();
			var dataSourceValueList = new List<string>();
			var parserCount = 0;
			foreach (var type in types)
			{
				if (TryGetParser(type, out var parser) && parser != null)
				{
					parserCount++;
					var dataSource = GetXMLWriterDataSource(type, parser);
					if (!dataSourceValueList.Contains(dataSource))
					{
						dataSourceValueList.Add(dataSource);
					}
					else
					{
						errorMessageBuilder.AppendLine(CultureInfo.InvariantCulture , $"{XMLWriterDataSource}: {dataSource} for type: {type} already exists");
					}
				}
			}
			Assert.AreEqual(CodeListsConstants.ValidCodeLists.Count, parserCount);
			Assert.IsEmpty(errorMessageBuilder.ToString());
		}

		bool TryGetParser(Type type, out object parser)
		{
			var parserTSVType = typeof(CodeListsParserTSV<,>);
			var parserXMLType = typeof(ManyToOneCodeListsParserXML<,>);
			var baseType = type.BaseType;
			while (baseType != null)
			{
				if (baseType.IsGenericType)
				{
					var genericTypeDefinitionName = baseType.GetGenericTypeDefinition().Name;
					if (genericTypeDefinitionName == parserTSVType.Name)
					{
						parser = Activator.CreateInstance(type, new Dictionary<string, string>());
						return true;
					}

					if (genericTypeDefinitionName == parserXMLType.Name)
					{
						parser = Activator.CreateInstance(type, new object[] { Array.Empty<string>() });
						return true;
					}
				}
				baseType = baseType.BaseType;
			}

			parser = null;
			return false;
		}

		string GetXMLWriterDataSource(Type parserType, object parser)
		{
			const BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
			return (string)parserType.GetProperty(XMLWriterDataSource, flag).GetValue(parser);
		}
		const string XMLWriterDataSource = nameof(XMLWriterDataSource);
	}
}
