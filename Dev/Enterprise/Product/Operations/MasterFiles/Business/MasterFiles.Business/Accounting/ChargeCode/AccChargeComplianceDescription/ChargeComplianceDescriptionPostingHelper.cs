using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Accounting
{
	public interface IDescriptionSetter
	{
		ZString Description { set; }
	}

	public interface ISellComplianceDescription
	{
		ZString Description { get; set; }

		ZString SellComplianceDescription { get; }
	}

	public interface IChargeComplianceDescriptionPostingHelper
	{
		void UpdateInvoiceLineAndChargeDescriptions(IDescriptionSetter line, ISellComplianceDescription charge);
	}

	class ChargeComplianceDescriptionPostingHelper : IChargeComplianceDescriptionPostingHelper
	{
		void IChargeComplianceDescriptionPostingHelper.UpdateInvoiceLineAndChargeDescriptions(IDescriptionSetter line, ISellComplianceDescription charge)
		{
			var description = $"{charge.Description}{charge.SellComplianceDescription}";
			if (charge.Description != description)
			{
				charge.Description = description;
			}
			line.Description = description;
		}
	}
}
