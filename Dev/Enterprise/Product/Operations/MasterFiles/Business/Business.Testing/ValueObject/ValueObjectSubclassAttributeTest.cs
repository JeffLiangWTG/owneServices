using System;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class ValueObjectSubclassAttributeTest : TestCase
	{
		public void TestAttributeClassName()
		{
			AssertEquals(
				"You cannot change the namespace or name of this class without first changing it in CodeDescriptionPairListGenerator",
				"Enterprise.DataTransfer.Xml.ValueObjectSubclassAttribute", typeof(ValueObjectSubclassAttribute).FullName);
		}

		public void TestTypeNameConstructorParameter()
		{
			ConstructorInfo constructor = typeof(ValueObjectSubclassAttribute).GetConstructor(new Type[] { typeof(string) });
			AssertEquals(
				"Must have a constructor of type String because the generator uses EnvDTE.CodeModel to find this parameter value",
				typeof(string), constructor.GetParameters()[0].ParameterType);
		}

		public void TestTypeName()
		{
			AssertEquals("TypeName", "name", new ValueObjectSubclassAttribute("name").TypeName);
		}
	}
}
