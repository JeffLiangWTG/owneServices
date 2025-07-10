using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageOriginLandValidationTest : BaseJobVoyOriginValidationTest
	{
		#region TestValidateJA_E_DEP

		public override void TestValidateJA_E_DEP()
		{
			Origin.JA_RL_NKPortOfLoading = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Origin.JA_E_DEP = ZDateTime.Empty;
			Origin.Validation.ValidateJA_E_DEP();
			AssertHasErrors("Date of Departure is empty, error expected", Origin.JA_E_DEPInfo);

			Origin.JA_E_DEP = ZDateTime.Today;
			AssertNoErrors("Date is valid, no error expected", Origin.JA_E_DEPInfo);

			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Origin.JA_RL_NKPortOfLoading = "USLAX";
			Origin.JA_E_DEP = ZDateTime.Empty;

			AssertNoErrors("For an arrival voyage, departure date should not have errors", Origin.JA_E_DEPInfo);
			AssertHasWarnings("Expecting departure date to have a warning", Origin.JA_E_DEPInfo);

			Destination.JB_RL_NKPortOfDischarge = "NZAKL";
			Origin.Validation.ValidateJA_E_DEP();
			AssertHasErrors("Date of Departure is empty, error expected", Origin.JA_E_DEPInfo);
		}

		#endregion

		#region Implementation

		protected override ZString TransportTypeCode
		{
			get { return Constants.TransportModes.Road; }
		}

		protected override Type ValidationType
		{
			get { return typeof(VoyageOriginLandValidation); }
		}

		#endregion
	}
}
