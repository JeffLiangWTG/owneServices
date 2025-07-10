using System;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(MergeManager))]
sealed class MergeManagerTest : Customs.Business.Testing.MergeManagerTest
{
	protected override Type GetLineMergerType() => typeof(LineMerger);
}
