using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryInstructionLookups : AutoZACusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(CusEntryInstruction parent)
			: base(parent)
		{
		}

		public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

		public CodeDescriptionPairList MRNsForReplacing
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var declaration = Parent.JobDeclaration;
				if (declaration != null)
				{
					foreach (var header in declaration.ActiveEntryHeaders.Cast<CusEntryHeader>()
						.Where(x => !x.MovementReferenceNumber.IsEmpty))
					{
						result.AddPair(header.MovementReferenceNumber, ZString.Format("CPC:{0} LRN:{1}", header.CustomsProcedureCode, header.CH_BGMReference));
					}
				}
				return result;
			}
		}

		public ZZRefCarrierCombinedCollection CargoCarrierCodeList
		{
			get
			{
				var declaration = Parent.JobDeclaration;
				return ZZRefCarrierCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, RefCarrierTypeList.Codes.CargoCarrier, declaration.JE_TransportMode);
			}
		}

		public CodeDescriptionPairList CreditTerms
		{
			get
			{
				var addInvDetailsToCusDec = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);

				var declaration = Parent.JobDeclaration;
				if (addInvDetailsToCusDec && !declaration.IsExport)
				{
					return Factory.GetCachedValue<CodeDescriptionPairList>();
				}
				else
				{
					return Factory.GetCachedValue<CreditTermsCodeListCompleteList>();
				}
			}
		}

		public ICodeDescriptionPairList CustomsOfficeList => Parent.JobDeclaration?.Lookups.CustomsOfficeList ?? new CodeDescriptionPairList();

		public ICodeDescriptionPairList BankCodes => ZARefCusCodeListTypes.GetBankCodeList(Factory);

		public CodeDescriptionPairList EntityTypeList => Factory.GetCachedValue<EntityTypeList>();

		public ICodeDescriptionPairList PortsOfExit => ZARefCusCodeListTypes.GetCustomsOfficeList(Factory);

		public HeaderLevelProvisionalPayments ProvisionalPaymentTypes => Factory.GetCachedValue<HeaderLevelProvisionalPayments>();

		public CodeDescriptionPairList RefTypeList => Factory.GetCachedValue<RefTypeList>();

		public CodeDescriptionPairList ScopeList => Factory.GetCachedValue<ScopeList>();
	}
}
