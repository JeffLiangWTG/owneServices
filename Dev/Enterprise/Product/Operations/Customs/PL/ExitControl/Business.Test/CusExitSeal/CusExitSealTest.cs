using Enterprise.Customs.EU.ExitControl.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CusExitSeal))]
sealed class CusExitSealTest : CusExitSealAbstractTest<CusExitSeal, CusExitContainer, CusExitHeader>
{
}
