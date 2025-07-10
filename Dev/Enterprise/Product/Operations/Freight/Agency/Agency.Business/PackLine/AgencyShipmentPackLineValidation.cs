using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentPackLineValidation : PackLineValidation
	{
		public AgencyShipmentPackLineValidation(AgencyShipmentPackLine parent)
			: base(parent)
		{
		}

		public new AgencyShipmentPackLine Parent
		{
			get { return (AgencyShipmentPackLine)base.Parent; }
		}

		#region JL_JC

		public void ValidateJL_JC()
		{
			ValidateCalculatedProperty(Parent.JL_JCInfo);
		}

		protected virtual void CheckJL_JC()
		{
			TypeValidation.CheckValidGuid(Parent.JL_JCInfo);
			if (Parent.JL_JC.IsValid)
			{
				ListValidation.ErrorIfInvalidPK(Parent.JL_JCInfo, Parent.Lookups.Containers);
			}
		}

		#endregion

		#region JL_PackageCount

		protected override void CheckJL_PackageCount()
		{
			base.CheckJL_PackageCount();

			if (Parent.JL_PackageCount < 1)
			{
				Parent.JL_PackageCountInfo.AddError(Res.GetString("68f3a3f8-0462-4aa7-b11e-04ff819e2216", "Package count must be greater than 0"));
			}
		}

		#endregion

		#region JL_HarmonisedCode

		protected override void CheckJL_HarmonisedCode()
		{
			base.CheckJL_HarmonisedCode();
			HarmonisedCodeValidator.Validate(Parent.JL_HarmonisedCodeInfo, Parent.IsInDatabase);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJL_JC();
		}
	}
}


