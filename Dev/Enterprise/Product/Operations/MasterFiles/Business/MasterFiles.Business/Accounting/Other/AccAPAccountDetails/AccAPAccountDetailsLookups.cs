using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccAPAccountDetailsLookups : AutoAccAPAccountDetailsLookups
	{
		public AccAPAccountDetailsLookups(AutoAccAPAccountDetails parent) : base(parent)
		{
		}

		#region A1_EPaymentReferenceType_List

		public CodeDescriptionPairList A1_EPaymentReferenceType_List => EPaymentReferenceTypes.CodesList;

		#endregion

		#region A1_EPaymentReasonCode_List

		public CodeDescriptionPairList A1_EPaymentReasonCode_List
		{
			get
			{
				var paymentMethod = ((AccAPAccountDetails)Parent).A1_PaymentMethod;
				return EPaymentPropertyHelper.GetPaymentReasonsForEPaymentMethod(Env.CurrentCompanyPK, paymentMethod);
			}
		}

		#endregion

		#region A1_PaymentMethod_List

		public CodeDescriptionPairList A1_PaymentMethod_List
		{
			get
			{
				if (fA1_PaymentMethod_List == null)
				{
					fA1_PaymentMethod_List = new CodeDescriptionPairList(OLookUpEditType.APPaymentMethod);
					ZString defaultCode = OrganisationsDataRegistry.Instance.APPaymentMethod.Value;
					ZString defaultDesc = fA1_PaymentMethod_List.GetDescriptionFromCode(defaultCode);
					fA1_PaymentMethod_List.Insert(0, new CodeDescriptionPair(DefaultPayment, Res.GetString("b0f440ca-5fe9-4809-9d29-a923064a96fb", "Default ({0})", defaultDesc)));
				}
				return fA1_PaymentMethod_List;
			}
		}
		CodeDescriptionPairList fA1_PaymentMethod_List;
		public const string DefaultPayment = "DEF";

		#endregion

		#region AccountCurrencies

		public override RefCurrencyCollection AccountCurrencies
		{
			get
			{
				if (fAccountCurrencies == null)
				{
					fAccountCurrencies = new RefCurrencyCollection(Factory);
				}
				return fAccountCurrencies;
			}
		}
		RefCurrencyCollection fAccountCurrencies;

		#endregion
	}
}
