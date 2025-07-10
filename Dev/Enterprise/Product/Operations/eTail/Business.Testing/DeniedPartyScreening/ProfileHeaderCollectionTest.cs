using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using NUnit.Framework;
using static Enterprise.DeniedPartyScreening.Common.DeniedPartyConstants;

namespace Enterprise.eTail.Business.DeniedPartyScreening.Testing
{
	[TestedType(typeof(ProfileHeaderCollection))]
	sealed class ProfileHeaderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProfileHeaderCollection>
	{
		protected override ProfileHeaderCollection GetCollectionToTest()
		{
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), TypeOfEntity = ScreeningNameTypes.Person },
			};
			var dpsResponse = new DpsResponse { Profiles = profiles };

			return new ProfileHeaderCollection(dpsResponse, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ProfileHeader(new ProfileHeaderInfo(), new DpsResponse(), Factory);
	}
}

