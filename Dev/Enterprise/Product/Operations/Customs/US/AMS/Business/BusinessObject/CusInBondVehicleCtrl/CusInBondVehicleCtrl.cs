using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	[DependentBusinessObject(typeof(CusInBondContainer), "Vehicles")]
	public class CusInBondVehicleCtrl : Customs.Business.CusInBondVehicleCtrl, IVehicleDetails, ISailingSynchronisationTarget<AgencyShipmentContainer>
	{
		public CusInBondVehicleCtrl(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CusInBondContainer Container
		{
			get { return Factory.Load<CusInBondContainer>(BV_BC); }
		}

		ZString IVehicleDetails.VIN
		{
			get { return BV_VIN; }
		}

		protected override Customs.Business.CusInBondVehicleCtrlValidation GetNewValidation()
		{
			return new CusInBondVehicleCtrlValidation(this);
		}

		public bool HasSailingLinkage
		{
			get
			{
				var container = Container;
				return container != null && container.HasSailingLinkage;
			}
		}

		#region ISailingSynchronisationTarget

		AgencyShipmentContainer ISailingSynchronisationTarget<AgencyShipmentContainer>.Source
		{
			get
			{
				if (synchronisationSource != null && IsMatched(synchronisationSource))
				{
					return synchronisationSource;
				}
				synchronisationSource = null;
				var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Container.Bill).Source;
				if (sailingBill != null && sailingBill.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff)
				{
					synchronisationSource = sailingBill.Vehicles.Cast<AgencyShipmentContainer>().FirstOrDefault(x => IsMatched(x));
				}
				return synchronisationSource;
			}
		}
		AgencyShipmentContainer synchronisationSource;

		bool ISailingSynchronisationTarget<AgencyShipmentContainer>.IsMatched(AgencyShipmentContainer sailingTarget)
		{
			return IsMatched(sailingTarget);
		}

		bool IsMatched(AgencyShipmentContainer sailingVehicle)
		{
			var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Container.Bill).Source;
			return sailingBill != null && sailingBill.Vehicles.Contains(sailingVehicle) && this.BV_VIN == sailingVehicle.JC_ContainerNum.Left(CusInBondVehicleCtrl.Schema.BV_VINMaxLength);
		}

		void ISailingSynchronisationTarget<AgencyShipmentContainer>.Set(AgencyShipmentContainer sailingTarget)
		{
			using (this.GetValidationSuspender())
			{
				this.BV_VIN = sailingTarget.JC_ContainerNum.Left(CusInBondVehicleCtrl.Schema.BV_VINMaxLength);
			}
		}

		void ISailingSynchronisationTarget<AgencyShipmentContainer>.Synchronise()
		{
		}

		#endregion
	}
}
