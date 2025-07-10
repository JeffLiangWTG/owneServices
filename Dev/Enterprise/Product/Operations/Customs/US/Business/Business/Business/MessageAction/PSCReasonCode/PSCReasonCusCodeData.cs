using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class PSCReasonCusCodeData : CusCodeData
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : CusCodeData.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public PSCReasonCusCodeData Load(CusEntryHeader entry)
			{
				var query = new ZQuery();
				query.AddToFilter(CusCodeDataSchema.CY_ParentID, entry.PK);
				query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
				query.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, CusEntryHeaderSchema.Constants.Prefix);
				return Factory.LoadTop1<PSCReasonCusCodeData>(query);
			}

			public PSCReasonCusCodeData Load(CusEntryLine entryLine)
			{
				var query = new ZQuery();
				query.AddToFilter(CusCodeDataSchema.CY_ParentID, entryLine.PK);
				query.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
				query.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, CusEntryLineSchema.Constants.Prefix);
				return Factory.LoadTop1<PSCReasonCusCodeData>(query);
			}

			protected override System.Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(PSCReasonCusCodeData);
			}
		}

		public PSCReasonCusCodeData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.PSCReasonCodes;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (CY_Data.IsEmpty)
			{
				Delete();
			}
		}

		#region Implementation

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusEntryHeader), typeof(CusEntryLine)); }
		}

		#endregion
	}
}
