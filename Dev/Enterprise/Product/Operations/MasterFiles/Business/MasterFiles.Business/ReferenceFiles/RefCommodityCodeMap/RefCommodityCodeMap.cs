using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefCommodityCodeMap : AutoRefCommodityCodeMap
	{
		public RefCommodityCodeMap(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("Country")]
		[List("Lookups.Countries")]
		public override ZString LC_RN_NKCountry
		{
			get
			{
				return base.LC_RN_NKCountry;
			}
			set
			{
				base.LC_RN_NKCountry = value;
				if (!IsValidationSuspended && !LC_LocalCode.IsEmpty)
				{
					Validation.ValidateLC_LocalCode();
				}
			}
		}

		[List("Lookups.LocalProviders")]
		public override ZString LC_LocalCodeProvider
		{
			get
			{
				return base.LC_LocalCodeProvider;
			}
			set
			{
				base.LC_LocalCodeProvider = value;
				if (!IsValidationSuspended && !LC_LocalCode.IsEmpty)
				{
					Validation.ValidateLC_LocalCode();
				}
			}
		}
	}
}
