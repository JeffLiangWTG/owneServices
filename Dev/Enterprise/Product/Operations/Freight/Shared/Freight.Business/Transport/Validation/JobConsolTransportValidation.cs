//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConsolTransportValidation
//
//    This class should be used for overriding validation in AutoJobConsolTransportValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class JobConsolTransportValidation : AutoJobConsolTransportValidation
	{
		// CRITICAL DANGER WARNING CRITICAL DANGER WARNING CRITICAL DANGER WARNING CRITICAL DANGER WARNING
		//
		// Do not add validation logic here or it may be run multiple times.
		//
		// If you want your validation logic to run for always then, you want TransportValidation.
		//
		// CRITICAL DANGER WARNING CRITICAL DANGER WARNING CRITICAL DANGER WARNING CRITICAL DANGER WARNING

		public JobConsolTransportValidation(AutoJobConsolTransport parent)
			: base(parent)
		{
		}

		#region JW_JX_JV_RegistrationNo

		public void ValidateJW_JX_JV_RegistrationNo()
		{
			ValidateCalculatedProperty(Parent.JW_JX_JV_RegistrationNoInfo);
		}

		protected virtual void CheckJW_JX_JV_RegistrationNo()
		{
		}

		#endregion

		#region JW_JX_IsPublished

		public void ValidateJW_JX_IsPublished()
		{
			ValidateCalculatedProperty(Parent.JW_JX_IsPublishedInfo);
		}

		protected virtual void CheckJW_JX_IsPublished()
		{
		}

		#endregion

		#region IsDomestic

		public void ValidateIsDomestic()
		{
			ValidateCalculatedProperty(Parent.IsDomesticInfo);
		}

		protected virtual void CheckIsDomestic()
		{
		}

		#endregion

		#region CreditorPK

		public void ValidateCreditorPK()
		{
			ValidateCalculatedProperty(Parent.CreditorPKInfo);
		}

		protected virtual void CheckCreditorPK()
		{
		}

		#endregion

		#region CarrierPK

		public void ValidateCarrierPK()
		{
			ValidateCalculatedProperty(Parent.CarrierPKInfo);
		}

		protected virtual void CheckCarrierPK()
		{
		}

		#endregion

		#region Ports Validation

		protected bool RoutingLegLoadsAtPort(ZString port)
		{
			return ConsolTransportValidationHelper.RoutingLegLoadsAtPort(Parent, Parent.JW_RL_NKLoadPort, port);
		}

		protected bool RoutingLegDischargesAtPort(ZString port)
		{
			return ConsolTransportValidationHelper.RoutingLegDischargesAtPort(Parent, Parent.JW_RL_NKDiscPort, port);
		}

		#endregion

		#region JW_JX_Load_ATA

		public void ValidateJW_JX_Load_ATA()
		{
			ValidateCalculatedProperty(Parent.JW_JX_Load_ATAInfo);
		}

		protected virtual void CheckJW_JX_Load_ATA()
		{
		}

		#endregion

		#region JW_JX_Load_ETA

		public void ValidateJW_JX_Load_ETA()
		{
			ValidateCalculatedProperty(Parent.JW_JX_Load_ETAInfo);
		}

		protected virtual void CheckJW_JX_Load_ETA()
		{
		}

		#endregion

		#region CO2ePerTonneInKg

		public void ValidateTotalCO2eForSorting()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForSortingInfo);
		}

		protected virtual void CheckTotalCO2eForSorting()
		{
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateJW_JX_JV_RegistrationNo();
			ValidateIsDomestic();
			ValidateJW_JX_IsPublished();
			ValidateCarrierPK();
			ValidateCreditorPK();
			ValidateJW_JX_Load_ETA();
			ValidateJW_JX_Load_ATA();
			ValidateTotalCO2eForSorting();
		}

		#endregion

		#region Implementation

		protected new Transport Parent
		{
			get { return (Transport)base.Parent; }
		}

		#endregion
	}
}
