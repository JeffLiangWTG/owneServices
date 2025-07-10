using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ContractNumber : Customs.Business.CusCodeData, IDrawbackContractNumber
	{
		public ContractNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ContractNumber;
			CY_Code = CusCodeDataTypeList.Codes.ContractNumber;
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new ContractNumberValidation(this);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobDeclaration)); }
		}

		public ZString ContractNumberCode
		{
			get { return CY_Data; }
		}
	}
}
