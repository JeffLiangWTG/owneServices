using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(RateAttachmentSetController))]
	public class RateAttachmentSetControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(RateAttachmentSet);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RateAttachmentSet;
		}
	}
}
