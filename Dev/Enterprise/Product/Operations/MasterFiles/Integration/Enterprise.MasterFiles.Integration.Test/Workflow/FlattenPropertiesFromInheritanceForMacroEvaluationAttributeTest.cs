using System;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Integration.Testing
{
	public class FlattenPropertiesFromInheritanceForMacroEvaluationAttributeTest : TestCase
	{
		public void TestInterfaceName()
		{
			var name = typeof(IClass).GetCustomAttribute<FlattenPropertiesFromInheritanceForMacroEvaluationAttribute>().InterfaceName;
			AssertEquals(nameof(IClass2), name);
		}

		public void TestConstructorExceptionIfArgumentNullorEmpty()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new FlattenPropertiesFromInheritanceForMacroEvaluationAttribute(null));
			AssertExceptionThrown<ArgumentException>(() => new FlattenPropertiesFromInheritanceForMacroEvaluationAttribute(string.Empty));
		}

		[FlattenPropertiesFromInheritanceForMacroEvaluation(nameof(IClass2))]
		interface IClass : IClass2 { }

		interface IClass2 { }
	}
}
