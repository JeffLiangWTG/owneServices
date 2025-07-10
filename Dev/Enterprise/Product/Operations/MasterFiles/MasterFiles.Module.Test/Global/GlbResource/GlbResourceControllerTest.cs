using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbResourceController))]
	sealed class GlbResourceControllerTest : ZControllerBasherTest
	{
		public void TestShowNewForm()
		{
			GlbResourceController controller = new GlbResourceController();
			using (GlbResourceForm form = (GlbResourceForm)controller.ShowNewForm())
			{
				AssertEquals(true, form.Resource.GS_IsResource);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbResource;
		}

		public override Type ControllerToBashType
		{
			get { return typeof(GlbResourceController); }
		}
	}
}
