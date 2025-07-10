using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions.Testing
{
	sealed class DummCusContainerWithAddInfo : BaseCusContainer
	{
		public DummCusContainerWithAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		DummyBusinessObject data;
		public DummyBusinessObject Data => data ?? (data = Factory.New<DummyBusinessObject>());

		public ZString Z0_Code
		{
			get => Data.Z0_Code;
			set => Data.Z0_Code = value;
		}

		public ZPropertyInfo Z0_CodeInfo => GetWrappedZPropertyInfo(DummyBizoSchema.Z0_Code.Name, x => Data.Z0_CodeInfo);

		public ZBool Z0_Bool
		{
			get => Data.Z0_Bool;
			set => Data.Z0_Bool = value;
		}

		public ZPropertyInfo Z0_BoolInfo => GetWrappedZPropertyInfo(DummyBizoSchema.Z0_Bool.Name, x => Data.Z0_BoolInfo);

		public ZDecimal Z0_Decimal
		{
			get => Data.Z0_Decimal;
			set => Data.Z0_Decimal = value;
		}

		public ZPropertyInfo Z0_DecimalInfo => GetWrappedZPropertyInfo(DummyBizoSchema.Z0_Decimal.Name, x => Data.Z0_DecimalInfo);

		public ZInt Z0_Number
		{
			get => Data.Z0_Number;
			set => Data.Z0_Number = value;
		}

		public ZPropertyInfo Z0_NumberInfo => GetWrappedZPropertyInfo(DummyBizoSchema.Z0_Number.Name, x => Data.Z0_NumberInfo);

		public ZShort Z0_Short
		{
			get => Data.Z0_Short;
			set => Data.Z0_Short = value;
		}

		public ZPropertyInfo Z0_ShortInfo => GetWrappedZPropertyInfo(DummyBizoSchema.Z0_Short.Name, x => Data.Z0_ShortInfo);

		public ZGuid Z0_Guid
		{
			get => Data.Z0_Guid;
			set => Data.Z0_Guid = value;
		}

		public ZPropertyInfo Z0_GuidInfo => GetWrappedZPropertyInfo(DummyBizoSchema.Z0_Guid.Name, x => Data.Z0_GuidInfo);

		public ZDateTime Z0_Date
		{
			get => Data.Z0_Date;
			set => Data.Z0_Date = value;
		}

		public ZPropertyInfo Z0_DateInfo => GetWrappedZPropertyInfo(DummyBizoSchema.Z0_Date.Name, x => Data.Z0_DateInfo);
	}
}
