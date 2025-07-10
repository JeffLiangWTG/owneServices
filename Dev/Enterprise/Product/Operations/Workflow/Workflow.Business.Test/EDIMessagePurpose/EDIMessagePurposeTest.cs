using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessagePurpose))]
	class EDIMessagePurposeTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			foreach (var p in Factory.Load<EDIMessagePurpose>(new ZQuery()))
			{
				p.Delete();
			}
			Factory.Save();
		}
	}
}
