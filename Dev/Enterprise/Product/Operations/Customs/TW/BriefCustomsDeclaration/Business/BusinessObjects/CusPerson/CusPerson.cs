using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class CusPerson : ASYCUDA.Business.CusPerson
	{
		public CusPerson(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Validation

		protected override Customs.Business.CusPersonValidation GetNewValidation() => new CusPersonValidation(this);

		public new CusPersonValidation Validation => (CusPersonValidation)base.Validation;

		#endregion

		public ZString PersonDescription
		{
			get
			{
				var identificationNumber = PersonIdentificationNumber;
				return identificationNumber.IsEmpty ? PersonPassport : identificationNumber;
			}
		}
	}
}
