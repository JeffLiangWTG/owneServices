using System;
using System.Reflection;
using System.Runtime.Loader;
using CargoWise.Blazor.Common;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace CargoWise.Blazor.Testing.Common
{
	public sealed class AssemblySetupAttribute : Attribute, IApplyToContext
	{
		public void ApplyToContext(TestExecutionContext context)
		{
			OneTimeSetUp();
		}
		public void OneTimeSetUp()
		{
			// This code ensures that the custom assembly resolver is registered ahead of all other assembly resolvers
			// When running in production our AssemblyResolver is always registered first but in tests NUnit registers other resolvers first
			// This causes unexpected behaviour to occur when loading Winzor code in test contexts so we force registration in the correct order here
			var assemblyResolve = typeof(AssemblyLoadContext).GetField("AssemblyResolve", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)
				as Delegate;
			var invocationList = assemblyResolve.GetInvocationList();

			foreach (var handler in invocationList)
			{
				AppDomain.CurrentDomain.AssemblyResolve -= handler as ResolveEventHandler;
			}

			AssemblyResolver.Setup();

			foreach (var handler in invocationList)
			{
				AppDomain.CurrentDomain.AssemblyResolve += handler as ResolveEventHandler;
			}
		}
	}
}
