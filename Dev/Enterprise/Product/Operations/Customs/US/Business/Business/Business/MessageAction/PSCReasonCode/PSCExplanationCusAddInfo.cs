using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class PSCExplanationCusAddInfo : CusAddInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : CusAddInfo.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public PSCExplanationCusAddInfo Load(CusEntryHeader entry)
			{
				var query = new ZQuery();
				query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, CusEntryHeaderSchema.Constants.Prefix);
				query.AddToFilter(CusAddInfoSchema.B7_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
				query.AddToFilter(CusAddInfoSchema.B7_ParentID, entry.PK);
				return Factory.LoadTop1<PSCExplanationCusAddInfo>(query);
			}

			protected override System.Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(PSCExplanationCusAddInfo);
			}
		}

		public PSCExplanationCusAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B7_Type = CusCodeDataTypeList.Codes.PSCReasonCodes;
		}
	}
}
