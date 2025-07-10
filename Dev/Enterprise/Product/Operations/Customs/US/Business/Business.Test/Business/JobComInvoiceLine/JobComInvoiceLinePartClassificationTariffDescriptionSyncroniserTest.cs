using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceLinePartClassificationTariffDescriptionSyncroniserTest : Customs.Business.Testing.InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest
	{
		protected override ZString TariffCode => "0000000000";

		protected override ZString TariffCode2 => "0000000001";

		protected override ZString TariffDescription => string.Empty;

		protected override ZString TariffDescription2 => string.Empty;

		protected override Type DeclarationTypeForTest => typeof(JobDeclaration);

		protected override BaseCusClassification GetNewLookup()
		{
			var result = Factory.New<BaseCusClassification>(); // BaseCusClassification.New(Factory)
			result.CC_LookupCode = LookupCode;
			result.CC_ClassificationType = BaseCusClassification.ClassificationType.EXP;
			result.CC_TariffNum = TariffCode;
			result.CC_Description = LookupDescription;
			return result;
		}

		protected override void AddLookupToPart(Customs.Business.OrgSupplierPart part, BaseCusClassification lookup)
		{
			var exportPivot = ((OrgSupplierPart)part).PivotsForBinding.AddNew();
			exportPivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			exportPivot.CI_CC = lookup.PK;
		}

		protected override void ChangeLookupOnPartInAnotherFactory(ZGuid partPK, ZGuid newClassificationPK)
		{
			var anotherNewFactory = new BusinessObjectFactory();
			var loadedPart = anotherNewFactory.Load<OrgSupplierPart>(partPK);
			var newClassInAnotherFactory = anotherNewFactory.Load<CusClassification>(newClassificationPK);
			CusClassPartPivot exportPivot = null;
			if (loadedPart.PivotsForBinding.Count > 0)
			{
				exportPivot = loadedPart.PivotsForBinding[0];
			}
			if (exportPivot == null)
			{
				exportPivot = loadedPart.PivotsForBinding.AddNew();
				exportPivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			}
			exportPivot.CI_CC = newClassInAnotherFactory.PK;
			foreach (var pivot in loadedPart.PivotsForBinding)
			{
				if (pivot != exportPivot)
				{
					pivot.Delete();
				}
			}
			anotherNewFactory.Save();
		}

		protected override string ExpectedPartExtendedCommercialDescription => PartExtendedCommercialDescription;
	}
}
