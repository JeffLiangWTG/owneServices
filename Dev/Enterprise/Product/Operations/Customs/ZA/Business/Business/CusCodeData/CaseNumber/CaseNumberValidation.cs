using System.Linq;

namespace Enterprise.Customs.ZA.Business
{
	public class CaseNumberValidation : Customs.Business.CusCodeDataValidation
	{
		public CaseNumberValidation(CaseNumber parent)
			: base(parent)
		{
		}

		protected new CaseNumber Parent
		{
			get { return (CaseNumber)base.Parent; }
		}

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (!Parent?.CY_Data.IsEmpty ?? false)
			{
				if (Parent.ParentAsCollectionProvider?.CaseNumbers?.Cast<CaseNumber>().Any(x => x.CY_Data == Parent.CY_Data && x.PK != Parent.PK) ?? false)
				{
					Parent.CY_DataInfo.AddMessageError(Res.GetString("E52BEB8A-0A51-49FB-A062-5990475457FD", "This code is duplicated. Only one occurrence of each case number is allowed."));
				}
			}
			else
			{
				Parent.CY_DataInfo.AddMessageError(Res.GetString("F2166DD7-25FB-4B9E-96BC-76622057C039", "Please enter a case number."));
			}
		}
	}
}
