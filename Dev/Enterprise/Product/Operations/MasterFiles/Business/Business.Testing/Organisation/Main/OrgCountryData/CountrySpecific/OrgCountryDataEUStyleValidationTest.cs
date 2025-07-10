using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal abstract class OrgCountryDataEUStyleValidationTest<T> : OrgCountryDataValidationTest where T : OrgCountryDataEUStyle
	{
		#region Implementation

		protected abstract IEnumerable<string> CountriesToTest { get; }

		protected abstract int MaximumApprovalValidityInYearsForKnownConsignors { get; }

		protected abstract int MaximumApprovalValidityInYearsForAccountConsignors { get; }

		#endregion

		#region Implementation

		protected override OrgCountryData GetNewBusinessObjectForTest()
		{
			return Factory.New<OrgCountryDataEU>();
		}

		protected override string ApprovedCodeForTest
		{
			get { return AviationSecuritySchemeMembership.Codes.KnownConsignor; }
		}

		protected override ZString CountryCodeForTest
		{
			get { return CountriesToTest.First(); }
		}

		protected override string ExpectedErrorForMissingRequiredDocument
		{
			get { return "Before flagging this organization as approved, attach a document to eDocs using type \"KCA\" and record the details for the document within the Document Tracking grid."; }
		}

		#endregion
	}
}
