using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.GUI.Testing
{
	public static class CheckCreditTestHelper
	{
		public static IDisposable WithCreditCheck(BusinessObjectFactory factory, BaseJobDeclaration jobDeclaration, bool isCheckPass)
		{
			jobDeclaration.JE_OH_Importer = factory.NewWithValidTestData<OrgHeader>().PK;
			jobDeclaration.Importer.MiscServ.OM_ARCreditLimit = -1M;

			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection
			{
				new AmountOrPercentageBasedThreeLevelAuthorisationRequirement
				{
					Amount = 1,
					Percentage = 0,
					Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo,
					AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly
				},
				new AmountOrPercentageBasedThreeLevelAuthorisationRequirement
				{
					Amount = 1,
					Percentage = 0,
					Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above,
					AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly
				}
			};
			var temporaryCreditControllerOverrideThresholdValue = AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var temporaryCreditCheckOnMessageSendValue = CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, !isCheckPass);

			return new CompositeDisposable(temporaryCreditCheckOnMessageSendValue, temporaryCreditControllerOverrideThresholdValue);
		}

		class CompositeDisposable : IDisposable
		{
			readonly IEnumerable<IDisposable> disposables;

			public CompositeDisposable(params IDisposable[] disposables) => this.disposables = disposables;

			public void Dispose()
			{
				foreach (var disposable in disposables)
				{
					disposable.Dispose();
				}
			}
		}
	}
}
