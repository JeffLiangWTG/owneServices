using System;
using Enterprise.Customs.NZ.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Test
{
	[TestedType(typeof(ConsolidatedDeclarationModule))]
	sealed class ConsolidatedDeclarationModuleTest : Customs.Module.Testing.ConsolidatedDeclarationModuleAbstractTest
	{
		protected override string GetExpectedApplicationCode() => Customs.Business.ConsolidatedDeclaration.ApplicationCodes.TSW;
		protected override Type ConsolidatedDeclarationType => typeof(ConsolidatedDeclaration);

		protected override bool ShouldTestFormIsFullyTranslatable => false;
	}
}
