using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BuildTools;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationCollectionEnsuresNoSubclassesHaveFactoryOnlyCtorTest : TestCaseWithFactory
	{
		// WI00072221.
		// The purpose of this WI is to hinder a country specific JobDeclarationCollection from having a Factory-only constructor as it leads to the situation that it will load all declarations as its own type, which is wrong.
		// Enumerate all the direct and indirect subclasses of BaseJobDeclarationCollection. For each subclass, pass it if the class does not have a factory-only constructor and add it to the pass list. If it does have such a ctor,
		// look inside the method body to see if that ctor DIRECTLY calls the ctor on BaseJobDeclaration that has a string or a GlbCompany. If it does, pass it and add it to the pass list.
		// Otherwise, look to see if the type's base type is already on the pass list. If not, add it to the fail list.
		// Then take the failures and re-test them against the (updated) pass list, because we may have tested a twice-derived type before its intermediate type (e.g. GB before EU).

		public void TestSubclassesOfBaseJobDecCollMustNotHaveFactoryOnlyConstructor()
		{
			var skipAssemblies = new HashSet<string>
			{
				"Enterprise.CodeAnalysis.TestCode",
				"Enterprise.ReflectionTest"
			};

			var baseTypeToCheckAgainst = typeof(BaseJobDeclarationCollection);
			var allAssemblies = BuildXml.Instance.GetAllAssembliesToBuild(false)
				.Select(asm =>
				{
					var directory = Path.GetDirectoryName(asm);
					var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(asm);
					var newPath = string.IsNullOrEmpty(directory) ? fileNameWithoutExtension : Path.Combine(directory, fileNameWithoutExtension);
					return newPath;
				})
				.Distinct()
#pragma warning disable CS0436 // Type conflicts with imported type - due to InternalsVisibleTo
				.Where(asm => !skipAssemblies.Contains(asm) && !IsNotTargetPrefix(asm))
#pragma warning restore CS0436 // Type conflicts with imported type
				.ToArray();
			var subClassRetriever = new SubClassRetriever(allAssemblies, baseTypeToCheckAgainst);
			var typesThatFailed = new List<Type>();
			var typesThatPassed = new List<Type>();
			foreach (var subClassTypeToTest in subClassRetriever.Retrieve())
			{
				CheckOneType(baseTypeToCheckAgainst, typesThatFailed, typesThatPassed, subClassTypeToTest);
			}

			if (typesThatFailed.Count > 0)
			{
				// Derived types may have been tested before their base type, e.g. GB before EU.  Check the 'failures' again.
				var typeToTestAgain = typesThatFailed.ToArray().ToList();
				typesThatFailed = new List<Type>();
				foreach (var intermediateFailure in typeToTestAgain.ToArray())
				{
					CheckOneType(baseTypeToCheckAgainst, typesThatFailed, typesThatPassed, intermediateFailure);
				}
				if (typesThatFailed.Count > 0)
				{
					var sb = new ZStringBuilder();
					foreach (var t in typesThatFailed)
					{
						sb.AppendLine(t.FullName);
					}
					Assert("The following (indirect?) subclasses of BaseJobDeclarationCollection have a factory-only constructor. The subclass or its intermediate base class should be modified to ensure it takes a country code or GlbCompany as well. \r\n"
						+ sb,
						false);
				}
			}
			Assert("No subclasses of BaseJobDeclarationCollection have a factory-only constructor; good", true);
		}

		public bool IsNotTargetPrefix(string assemblyName)
		{
#pragma warning disable CS0436 // Type conflicts with imported type - due to InternalsVisibleTo
			bool isNetCoreTargetFrameworkPrefix = assemblyName.StartsWith(CommonAssemblyInfo.CWNetCoreSubfolder, StringComparison.OrdinalIgnoreCase);
#pragma warning restore CS0436 // Type conflicts with imported type
#if NETFRAMEWORK
			return isNetCoreTargetFrameworkPrefix;
#elif NET
			return !isNetCoreTargetFrameworkPrefix;
#else
#error Unexpected target platform
#endif
		}

		static void CheckOneType(Type veryBaseTypeToCheckAgainst, List<Type> typesThatFailed, List<Type> typesThatPassed, Type subClassType)
		{
			var argumentTypesForSignature = new Type[] { typeof(BusinessObjectFactory) };
			var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
			var constructorInfoObj = subClassType.GetConstructor(flags, null, argumentTypesForSignature, null);
			if (constructorInfoObj != null)
			{
				// Factory-only ctor exists on derived type, but this is not immediatewly terrible. Check: does it call base with other parameters?
				// (This part is kinda lame, I don't know of a way of making a new Cecil.TypeDefintion from subSubClass.  So enumerate again.)
				var cecilModule = ModuleDefinition.ReadModule(subClassType.Assembly.Location);
				var cecilType = cecilModule.Types.First(x => x.FullName == subClassType.FullName);
				var constructorMethod = cecilType.Methods.First(x => x.IsConstructor);
				var passedOK = false;
				foreach (var instruction in constructorMethod.Body.Instructions)  // look at IL code...
				{
					if (instruction.OpCode == OpCodes.Call) // Calling anothe rmethod, including another ctor
					{
						MethodReference methodCall = instruction.Operand as MethodReference;
						if (methodCall != null && methodCall.Name == ".ctor")
						{
							if (methodCall.DeclaringType.FullName == veryBaseTypeToCheckAgainst.FullName)  // This class derives directly from BaseJobDeclarationCollection
							{
								foreach (var parameter in methodCall.Parameters)
								{
									if (parameter.ParameterType.FullName == typeof(ZString).FullName
										|| parameter.ParameterType.FullName == typeof(GlbCompany).FullName)
									{
										// Even though the ctor on subClassType is factory-only, it makes a call to BaseJobDeclaration with a signature that uses a ZString or a GlbCompay.  So that's OK.
										passedOK = true;
										typesThatPassed.Add(subClassType);
										break;
									}
								}
							}
							else
							{
								// Intermediate type, e.g. Type-safe or EU.  Does it derive from a type that we already know is good?
								if (typesThatPassed.Any(t => t.FullName == subClassType.BaseType.FullName))
								{
									passedOK = true;
									typesThatPassed.Add(subClassType);
									break;
								}
							}
						}
					}
				}
				if (!passedOK)
				{
					typesThatFailed.Add(subClassType);
				}
			}
			else
			{
				// OK... the factory-only ctor does not exist on this subtype. Good.
				typesThatPassed.Add(subClassType);
			}
		}
	}
}
