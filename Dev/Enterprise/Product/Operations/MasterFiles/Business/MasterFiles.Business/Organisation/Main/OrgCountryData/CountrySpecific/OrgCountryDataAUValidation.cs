using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCountryDataAUValidation : OrgCountryDataValidation
	{
		public OrgCountryDataAUValidation(OrgCountryData parent)
			: base(parent)
		{
		}

		#region Validation

		protected override void CheckOV_EXApprovedOrMajorExporter()
		{
			if (!IsValidationRequired)
			{
				return;
			}

			if (!Parent.OV_OA_ApprovedLocation.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.OV_EXApprovedOrMajorExporterInfo);
			}

			base.CheckOV_EXApprovedOrMajorExporter();
		}

		protected override void CheckOV_OA_ApprovedLocation()
		{
			if (!IsValidationRequired)
			{
				return;
			}

			switch (Parent.OV_EXApprovedOrMajorExporter)
			{
				case AviationSecuritySchemeMembership.Codes.AccreditedAirCargoAgent:
					if (Parent.OV_OA_ApprovedLocation.IsValid)
					{
						var orgAddress = Parent.Factory.Load<OrgAddress>(Parent.OV_OA_ApprovedLocation);
						if (orgAddress != null && !orgAddress.IsMainAddress)
						{
							Parent.OV_OA_ApprovedLocationInfo.AddError(Res.GetString("4d4f5e7e-d919-4422-8ee9-95477cafb2a9", "The Main Address must be selected for {0} of '{1}'.", Parent.OV_EXApprovedOrMajorExporterInfo.HumanReadableName, Parent.OV_EXApprovedOrMajorExporter));
						}
					}

					break;
			}

			base.CheckOV_OA_ApprovedLocation();
		}

		#endregion

		#region Implementation

		new OrgCountryDataAU Parent
		{
			get { return (OrgCountryDataAU)base.Parent; }
		}

		#endregion
	}
}
