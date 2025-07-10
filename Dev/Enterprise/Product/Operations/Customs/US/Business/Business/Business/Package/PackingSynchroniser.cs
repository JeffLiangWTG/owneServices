using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business
{
	public class PackingSynchroniser : Customs.Business.PackingSynchroniser
	{
		public PackingSynchroniser(JobDeclarationSynchroniser parentSynchroniser, BaseJobDeclaration declaration)
			: base(parentSynchroniser, declaration)
		{
		}

		protected override ZString GetConvertedPackUQ(ZString freightPackageType)
		{
			var mappings = USCustomsDataRegistry.Instance.USPackageTypesMapping.GetValueWithoutFallback(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty);
			return mappings.GetMappedPackageType(freightPackageType);
		}

		protected override Customs.Business.Bill GetEffectiveBill(ZString houseBill, ForwardingContainer container)
		{
			Customs.Business.Bill result = null;
			if (container != null && container.HasNewMBOL)
			{
				var calculator = new BillSynchronisationDataCalculator((JobDeclaration)Declaration, container);
				result = Declaration.Bills.FindAnyBillWithHouseBillMasterBillCombination(houseBill, calculator.BillNumber) ?? Declaration.Bills.FindByBillNumberAndType(calculator.BillNumber, BillTypeList.Codes.MasterBill);
			}
			if (result == null)
			{
				result = base.GetEffectiveBill(houseBill, container);
			}
			return result;
		}
	}
}
