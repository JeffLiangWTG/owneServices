using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(CostsComparerController))]
	public class CostsComparerControllerTest : ZSingletonControllerBasherTest
	{
		public void TestGetDisplayModeForNew()
		{
			AssertEquals(ODisplayMode.NewSaved, new CostsComparerControllerForTest().GetDisplayModeForNew());
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(Costing);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CostsComparer;
		}
	}

	public class CostsComparerControllerForTest : CostsComparerController
	{
		public new ODisplayMode GetDisplayModeForNew()
		{
			return base.GetDisplayModeForNew();
		}
	}
}
