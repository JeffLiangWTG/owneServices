using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondVehicleCtrlValidation : Customs.Business.CusInBondVehicleCtrlValidation
	{
		public CusInBondVehicleCtrlValidation(CusInBondVehicleCtrl parent)
			: base(parent)
		{
		}

		new CusInBondVehicleCtrl Parent
		{
			get { return (CusInBondVehicleCtrl)base.Parent; }
		}

		protected override void CheckBV_VIN()
		{
			base.CheckBV_VIN();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BV_VINInfo);

			if (!Parent.BV_VIN.IsEmpty)
			{
				var vehicles = Parent.Container != null ?
					Parent.Container.Vehicles.Find(x => x.BV_VIN == Parent.BV_VIN) :
					null;
				if (vehicles != null && vehicles.Count() > 1)
				{
					Parent.BV_VINInfo.AddMessageError(DuplicateVINIsNotAllowed);
				}
			}

			if (Parent.HasSailingLinkage && ((ISailingSynchronisationTarget<AgencyShipmentContainer>)Parent).Source == null)
			{
				Parent.BV_VINInfo.AddWarning(ValidationConstants.SailingSynchronisation.VehicleMightBeIncorectlyAdded);
			}
		}
		internal const string DuplicateVINIsNotAllowed = "You have entered duplicate VIN for this container.";
	}
}
