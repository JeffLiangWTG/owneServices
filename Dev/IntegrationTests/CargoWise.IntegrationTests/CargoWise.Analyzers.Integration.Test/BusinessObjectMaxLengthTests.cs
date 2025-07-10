using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace CargoWise.Analyzers.Integration.Test
{
	[RequiresSoftware(RequiredSoftware.VisualStudio | RequiredSoftware.DotNetSdk)]
	[FrequentlyFailing]
	public class BusinessObjectMaxLengthTests : TestCase
	{
		public void TestLoadExternalAssemblies()
		{
			const string testCode = @"
	using Enterprise.MasterFiles.Business;
	using CargoWise.Types;

	namespace MyApplication
	{
		class MyClass
		{
			void AssignFromWrappedProperty(AutoGlbStaff staff, MyOrgContact contact)
			{
				{
					staff.GS_EmailAddress = contact.MappedProperty;
					contact.MappedProperty = staff.GS_EmailAddress;
				}
			}
		}

		class MyOrgContact
		{
			public ZString MappedProperty
			{
				get;
				set;
			}
		}
	}
	";
			var devAssemblies = new List<string>()
			{
				Path.Combine("Enterprise.MasterFiles.Business.dll"),
				Path.Combine("CargoWise.Types.dll"),
			};

			ProcessRunner.AssertNoIssuesOnTest(testCode, "CargoWise.Analyzers.DoNotExceedBizOMaxLength", devAssemblies);
		}
	}
}
