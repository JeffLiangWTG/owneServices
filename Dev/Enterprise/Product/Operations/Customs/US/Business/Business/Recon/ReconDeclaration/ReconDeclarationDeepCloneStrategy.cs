using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconDeclarationDeepCloneStrategy : JobDeclarationDeepCloneStrategy
	{
		public ReconDeclarationDeepCloneStrategy(ReconDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn) : base(declarationToClone.ReconWrappedJobDeclaration, cloneType, alternativeFactoryToInstantiateCloneIn)
		{ }

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedArgs = new BusinessObjectCloneArgs(
				args.AlternativeFactoryToInstantiateCloneIn,
				args.GetExcludedColumns(),
				args.TypeToCloneAs,
				args.PerformRowCopyWithoutTriggeringValidationAndSetter,
				args.CopyDecider);

			clonedArgs.AddExcludedColumns(new string[] { ReconDeclaration.Schema.TotalDutyDifference, ReconDeclaration.Schema.TotalFeeDifference, ReconDeclaration.Schema.TotalOriginalDuty, ReconDeclaration.Schema.TotalOriginalFee, ReconDeclaration.Schema.TotalOriginalTax,
				ReconDeclaration.Schema.TotalReconDuty, ReconDeclaration.Schema.TotalReconFee, ReconDeclaration.Schema.TotalReconTax, ReconDeclaration.Schema.TotalTaxDifference, ReconDeclaration.Schema.InterestPaymentAmount });

			var templateCopy = (JobDeclaration)base.CloneInternal(args);
			return templateCopy;
		}
		protected const string CusEntryHeaderPKPairsKey = "cusEntryHeaderExistedPKPairs";

		protected override void DeepCopyDeclarationCore(BaseJobDeclaration clonedResult)
		{
			var declaration = ((JobDeclaration)clonedResult);
			var declarationToClone = DeclarationToClone as JobDeclaration;

			var clonedArgs = new BusinessObjectCloneArgs(System.Array.Empty<string>());
			var cusEntryHeaderOldPK = new Dictionary<ZGuid, ZGuid>();

			foreach (CusEntryHeader entry in declarationToClone.CustomsEntryHeaders)
			{
				if (entry.CH_MessageType != CusEntryHeaderMessageTypeList.Codes.ReconEntry)
				{
					var newEntry = (CusEntryHeader)entry.Clone(clonedArgs);

					foreach (CusEntryHeaderCharges charge in entry.Charges)
					{
						var newCharge = newEntry.Charges.AddNew();
						newCharge.C1_ChargeType = charge.C1_ChargeType;
						newCharge.C1_ChargeAmount = charge.C1_ChargeAmount;
					}
					declaration.CustomsEntryHeaders.Add(newEntry);
					cusEntryHeaderOldPK.Add(entry.PK, newEntry.PK);
				}
			}
			pkPairsDictionaryCollection.Add(CusEntryHeaderPKPairsKey, cusEntryHeaderOldPK);

			using (declaration.GetValidationSuspender())
			{
				declaration.ReconDeclaration = new ReconDeclaration(declaration);
			}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			declaration.Logs.AddNew(ZArchitecture.Business.Events.AddedARecordToTheSystem, "[CreationSource = Recon-" + declarationToClone.JE_DeclarationReference + "]");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		protected override JobComInvoiceHeaderDeepCopyStrategy GetInvoiceDeepCopyStrategy(BaseJobComInvoiceHeader invoiceToClone, BaseJobDeclaration clonedDeclaration)
		{
			return new ReconJobComInvoiceHeaderDeepCloneStrategy((JobComInvoiceHeader)invoiceToClone, cloneType, (JobDeclaration)clonedDeclaration, pkPairsDictionaryCollection);
		}
	}
}
