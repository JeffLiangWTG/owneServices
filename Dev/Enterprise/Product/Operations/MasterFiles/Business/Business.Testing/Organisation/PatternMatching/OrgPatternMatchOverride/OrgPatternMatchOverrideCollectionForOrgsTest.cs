using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPatternMatchOverrideCollectionForOrgs))]
	class OrgPatternMatchOverrideCollectionForOrgsTest : ActiveBusinessObjectCollectionTestCase<OrgPatternMatchOverrideCollectionForOrgs>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(OrgPatternMatchOverrideCollectionForOrgs);
		}

		protected override OrgPatternMatchOverrideCollectionForOrgs GetCollectionToTest()
		{
			return new OrgPatternMatchOverrideCollectionForOrgs(Factory, new ZQuery());
		}
	}
}
