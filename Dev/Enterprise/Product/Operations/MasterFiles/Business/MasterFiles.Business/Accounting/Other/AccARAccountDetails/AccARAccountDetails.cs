using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccARAccountDetails : AccAPAccountDetails
	{
		public AccARAccountDetails(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DataRow row = ((IBusinessObjectInternals)this).Row;
			row[AccAPAccountDetailsSchema.Constants.A1_PaymentMethod] = ARBankAccPayment;
		}

		[List("A1_PaymentMethodList")]
		public override ZString A1_PaymentMethod
		{
			get
			{
				return base.A1_PaymentMethod;
			}
			set
			{
				base.A1_PaymentMethod = value;
			}
		}

		public CodeDescriptionPairList A1_PaymentMethodList
		{
			get
			{
				if (fA1_PaymentMethod_List == null)
				{
					fA1_PaymentMethod_List = new CodeDescriptionPairList();
					fA1_PaymentMethod_List.Insert(0, new CodeDescriptionPair(ARBankAccPayment, Res.GetString("a1e320ca-6fe9-4809-6d19-b023064a96fc", "Tax Invoice's Bank Details")));
					fA1_PaymentMethod_List.Insert(1, new CodeDescriptionPair(ARCollectionRequest, Res.GetString("36aead92-0503-4a54-9472-68deb25d42e4", "Collection Request")));
					if (AccountingMasterFilesRegistry.Instance.EnableNetting.Value)
					{
						fA1_PaymentMethod_List.Insert(2, new CodeDescriptionPair(ARNettingBankAccount, Res.GetString("59e18c64-1d67-4b60-ad38-84b3f054f561", "Netting Bank Account")));
					}
				}
				return fA1_PaymentMethod_List;
			}
		}

		CodeDescriptionPairList fA1_PaymentMethod_List;
		public const string ARBankAccPayment = "TAX";
		public const string ARCollectionRequest = "CRQ";
		public const string ARNettingBankAccount = "NET";

		protected override AccAPAccountDetailsValidation GetNewValidation()
		{
			return new AccARAccountDetailsValidation(this);
		}

		protected override bool ModifyAccountDetailsSecurity
		{
			get { return CompanyData.Header.SecurityProvider.HasModifyReceivablesAccountDetailsSecurity; }
		}
	}
}
