using System;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(SumARegisterFormLayoutProvider))]
sealed class SumARegisterFormLayoutProviderTest : EFTA.TemporaryStorageRegister.GUI.Testing.SumARegisterFormLayoutProviderAbstractTest<SumARegisterFormLayoutProvider>
{
	protected override Type ExpectedDetailsHeaderLayoutType => typeof(SumARegisterDetailsHeaderLayout);
	protected override Type ExpectedLinesDetailsLayoutType => typeof(EFTA.TemporaryStorageRegister.GUI.LinesDetailsLayout);
}
