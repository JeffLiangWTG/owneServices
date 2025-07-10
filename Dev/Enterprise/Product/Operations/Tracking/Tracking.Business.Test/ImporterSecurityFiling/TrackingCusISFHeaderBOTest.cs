using System;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingCusISFHeader))]
	sealed class TrackingCusISFHeaderBOTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType
		{
			get { return typeof(Metadata.Business.USCusISFHeader); }
		}
	}
}
