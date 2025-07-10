using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(TemporaryOrgRemoverController))]
	sealed class TemporaryOrgRemoverControllerTest : ZSingletonControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(OrgHeader);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.TemporaryOrgRemover;
		}
	}
}
