using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	class CommonCusReferenceForTest : CommonCusReference
	{
		public CommonCusReferenceForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CFR_Type = CusReferenceTypeList.Codes.NctsAuthorization;
		}

		public new CommonCusReferenceLookupsForTest Lookups => (CommonCusReferenceLookupsForTest)base.Lookups;

		public ZString DataGroupingCodeExposed => DataGroupingCode;

		protected override CusReferenceLookups GetNewLookups() => new CommonCusReferenceLookupsForTest(this);

		protected override CusReferenceValidation GetNewValidation() => new CommonCusReferenceValidationForTest(this);
	}

	class CommonCusReferenceLookupsForTest : CommonCusReferenceLookups
	{
		public CommonCusReferenceLookupsForTest(CommonCusReference parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("YY");
				return result;
			}
		}
	}

	class CommonCusReferenceValidationForTest : CommonCusReferenceValidation
	{
		public CommonCusReferenceValidationForTest(CommonCusReference parent) : base(parent)
		{
		}
	}
}
