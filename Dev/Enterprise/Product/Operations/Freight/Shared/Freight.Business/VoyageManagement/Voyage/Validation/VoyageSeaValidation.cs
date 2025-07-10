using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class VoyageSeaValidation : BaseJobVoyageValidation
	{
		public VoyageSeaValidation(JobVoyage voyage)
			: base(voyage)
		{
		}

		protected override void CheckJV_RV_NKVessel()
		{
			base.CheckJV_RV_NKVessel();

			var info = Voyage.JV_RV_NKVesselInfo;
			MandatoryValidation.CheckEntered(info);
			info.ValidateVesselIsValid(() => Parent.Vessel);

			if (!info.HasErrors() && !info.HasWarnings())
			{
				CheckVesselVoyageCarrierCombinationIsUnique(info);
				CheckLloydsNumber();
			}
		}

		void CheckLloydsNumber()
		{
			var vessel = RefVessel.LookupVesselByFK(Voyage, JobVoyageSchema.JV_RV_NKVessel);

			if (vessel == null)
			{
				return;
			}

			if (vessel.RV_LloydsNumber.IsEmpty)
			{
				var message = Res.GetString("0D6C124B-4E7E-4477-A5A6-C28D631459F1", "The IMO number for vessel {0} is empty. You will not be able to receive automatic schedule updates.", Voyage.JV_RV_NKVessel);
				Voyage.JV_RV_NKVesselInfo.AddWarning(message);
			}
			else
			{
				var lloydsNumberValidation = new LloydsNumberValidation();
				lloydsNumberValidation.Validate(vessel.RV_LloydsNumber);

				if (vessel.RV_LloydsNumber.Length != 7 || !lloydsNumberValidation.IsValid)
				{
					var message = Res.GetString("58610EA8-8D28-4071-B05A-5E98AD977143", "The IMO number {0} for vessel {1} is not compliant with IMO requirements. You will not be able to receive automatic schedule updates.", vessel.RV_LloydsNumber, Voyage.JV_RV_NKVessel);
					Voyage.JV_RV_NKVesselInfo.AddWarning(message);
				}
			}
		}

		protected override void CheckJV_OH_Line()
		{
			base.CheckJV_OH_Line();
			MandatoryValidation.CheckEntered(Voyage.JV_OH_LineInfo);

			if (!Voyage.JV_OH_LineInfo.HasErrors())
			{
				CheckVesselVoyageCarrierCombinationIsUnique(Voyage.JV_OH_LineInfo);
			}
		}

		protected override void CheckJV_VoyageFlight()
		{
			base.CheckJV_VoyageFlight();
			MandatoryValidation.CheckEntered(Voyage.JV_VoyageFlightInfo);

			if (!Voyage.IsCharter)
			{
				string pattern = (NoResString)"^([Vv][.,\'\" ]*[0-9]*)$";
				bool isInvalidNumber = Regex.IsMatch(Voyage.JV_VoyageFlight, pattern);

				if (isInvalidNumber)
				{
					Voyage.JV_VoyageFlightInfo.AddWarning(Res.GetString("86763540-f07b-438c-af38-eaa495220c6c", "Voyage number should not start with a 'V' followed by numbers or punctuation characters. The system will add this 'V' automatically."));
				}
			}

			if (!Voyage.JV_VoyageFlightInfo.HasErrors())
			{
				CheckVesselVoyageCarrierCombinationIsUnique(Voyage.JV_VoyageFlightInfo);
			}

			if (!Voyage.JV_VoyageFlightInfo.HasErrors()
				&& !Voyage.JV_VoyageFlight.IsEmpty
				&& !Voyage.JV_RV_NKVessel.IsEmpty
				&& FindOtherVoyagesWithSameVesselVoyageCombination().Any())
			{
				Voyage.JV_VoyageFlightInfo.AddWarning(Res.GetString("c4b71bd7-52bb-426e-bb19-6037070cd0a5", "Another Sailing Schedule already exists for the given Vessel/Voyage Number combination."));
			}
		}

		protected override void CheckIsArchived()
		{
			base.CheckIsArchived();

			if (!Voyage.IsArchived)
			{
				CheckVesselVoyageCarrierCombinationIsUnique(Voyage.IsArchivedInfo);
			}
		}

		void CheckVesselVoyageCarrierCombinationIsUnique(ZPropertyInfo propertyInfo)
		{
			if (Voyage.JV_IsActive
				&& !Voyage.JV_VoyageFlight.IsEmpty
				&& !Voyage.JV_RV_NKVessel.IsEmpty
				&& !Voyage.JV_OH_Line.IsEmpty
				&& FindOtherVoyagesWithSameVesselVoyageCombination().Any(v => v.JV_OH_Line == Voyage.JV_OH_Line))
			{
				propertyInfo.AddError(Res.GetString("b5e30501-c182-4a97-bdc7-b80f39d9f44e", "Vessel/Voyage Number/Carrier combination must be unique and cannot be repeated for non-archived sailings."));
			}
		}

		protected override void CheckJV_VoyageType()
		{
			base.CheckJV_VoyageType();
			ListValidation.ErrorIfInvalidCode(Voyage.JV_VoyageTypeInfo);

			if (Voyage.JV_IsActive
				&& Voyage.IsMainVoyage
				&& !Voyage.JV_VoyageTypeInfo.HasErrors()
				&& FindOtherVoyagesWithSameVesselVoyageCombination().Any(v => v.IsMainVoyage))
			{
				Voyage.JV_VoyageTypeInfo.AddError(Res.GetString("4cc0c6af-2653-4d34-b6c2-3a20549d4a8c", "Main Sailing Schedule already exists for the given Vessel/Voyage Number combination."));
			}
		}
	}
}
