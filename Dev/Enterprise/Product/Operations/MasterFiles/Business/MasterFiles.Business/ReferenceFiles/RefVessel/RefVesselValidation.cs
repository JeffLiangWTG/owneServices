//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefVesselValidation
//
//    This class should be used for overriding validation in AutoRefVesselValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Text;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefVesselValidation : AutoRefVesselValidation
	{
		public RefVesselValidation(AutoRefVessel parent) : base(parent)
		{
		}

		protected override void CheckRV_Code()
		{
			base.CheckRV_Code();
			MandatoryValidation.CheckEntered(Parent.RV_CodeInfo);
			if (!Parent.RV_Code.IsEmpty)
			{
				ZQuery duplicateFilter = new ZQuery(RefVesselSchema.RV_Code, Parent.RV_Code.Trim());
				duplicateFilter.AddToFilter(RefVesselSchema.RV_IsActive, true);
				duplicateFilter.AddToFilter(RefVesselSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var duplicateVessel = Parent.Factory.LoadTop1<RefVessel>(duplicateFilter);
				if (duplicateVessel != null)
				{
					Parent.RV_CodeInfo.AddError(VesselAlreadyExists);
				}
			}
		}

		public static string VesselAlreadyExists
		{
			get { return Res.GetString("b4bffcf8-ef10-43a0-96d3-62021d7daeed", "A Vessel with this name already exists. Please edit the existing Vessel."); }
		}

		protected override void CheckRV_RN_NKCountryOfReg()
		{
			base.CheckRV_RN_NKCountryOfReg();
			ListValidation.ErrorIfInvalidCode(Parent.RV_RN_NKCountryOfRegInfo);
		}

		protected override void CheckRV_LloydsNumber()
		{
			base.CheckRV_LloydsNumber();
			LloydsNumberValidation lloydsNumberValidation = new LloydsNumberValidation();
			lloydsNumberValidation.Validate(Parent.RV_LloydsNumber);
			if (!lloydsNumberValidation.IsValid)
			{
				Parent.RV_LloydsNumberInfo.AddWarning(lloydsNumberValidation.ErrorText);
			}

			if (Parent.RV_IsActive && !Parent.RV_LloydsNumber.IsEmpty)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(RefVesselSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.AddToFilter(JoinCondition.And, RefVesselSchema.RV_LloydsNumber, Parent.RV_LloydsNumber);
				query.AddToFilter(JoinCondition.And, RefVesselSchema.RV_IsActive, true);

				RefVesselCollection refVesselCollection = new RefVesselCollection(Parent.Factory, query);

				if (refVesselCollection.Count != 0)
				{
					StringBuilder errorMessage = new StringBuilder();
					errorMessage.AppendLine(Res.GetString("b8c8c92d-9450-4d39-b4fb-4b7ac141bee4", "All active Vessels must have a unique Lloyds Number. Either mark a Vessel as inactive, or change the Lloyds Number."));
					errorMessage.AppendLine(Res.GetString("d9669d6d-6d2a-4676-bdc7-922f1856e637", "These other Vessels have the same Lloyds Number:"));
					errorMessage.AppendLine();

					foreach (RefVessel refVessel in refVesselCollection)
					{
						errorMessage.AppendLine(refVessel.RV_Code);
					}

					Parent.RV_LloydsNumberInfo.AddError(errorMessage.ToString());
				}
			}
		}

		protected override void CheckRV_ScreeningStatus()
		{
			base.CheckRV_ScreeningStatus();
			MandatoryValidation.CheckEntered(Parent.RV_ScreeningStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RV_ScreeningStatusInfo, Parent.Lookups.ScreeningStatusesList);
		}
	}
}
