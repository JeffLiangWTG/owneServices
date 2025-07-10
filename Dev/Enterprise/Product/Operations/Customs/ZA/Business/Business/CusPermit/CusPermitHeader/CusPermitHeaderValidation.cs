using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class CusPermitHeaderValidation : Customs.Business.BaseCusPermitHeaderValidation
	{
		public CusPermitHeaderValidation(CusPermitHeader parent) : base(parent)
		{
		}

		public new CusPermitHeader Parent
		{
			get { return (CusPermitHeader)base.Parent; }
		}

		protected override void CheckCPH_EndDate()
		{
			base.CheckCPH_EndDate();
			MandatoryValidation.CheckEntered(Parent.CPH_EndDateInfo);
		}

		protected override void CheckCPH_SubType()
		{
			base.CheckCPH_SubType();

			var parent = Parent;
			if (parent.CPH_Type == PermitTypeList.Codes.RCC || parent.CPH_Type == PermitTypeList.Codes.PRC || parent.CPH_Type == PermitTypeList.Codes.VALA)
			{
				MandatoryValidation.CheckEntered(Parent.CPH_SubTypeInfo);
			}
		}

		protected override void CheckCPH_Number()
		{
			base.CheckCPH_Number();

			var parent = Parent;
			if (parent.CPH_Type == PermitTypeList.Codes.PRC || parent.CPH_Type == PermitTypeList.Codes.VALA)
			{
				var subTypeList = new PermitSubTypeList();
				if (subTypeList.ContainsCode(parent.CPH_Number.Left(3)))
				{
					parent.CPH_NumberInfo.AddError(ResString.GetMultilingualString("0A085151-C0ED-4C00-BE38-16DFD169DA8E", "Permit Number cannot begin with {0}.", string.Join(" or ", subTypeList.GetAllCodes())));
				}
			}
		}

		protected override void CheckCPH_StartDate()
		{
			base.CheckCPH_StartDate();

			var parent = Parent;
			var permitLineTransaction = parent.GetTransactions()?.Select(x => x)
				.Where(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA || x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.CUS)
				.OrderBy(x => x.CPL_TransactionDate).FirstOrDefault();
			if (permitLineTransaction?.CPL_TransactionDate < parent.CPH_StartDate)
			{
				parent.CPH_StartDateInfo.AddError("Start Date Cannot be later than the earliest Transaction Date.");
			}
		}
	}
}
