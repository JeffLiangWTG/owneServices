using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	[CodeAlive("NZCSupplier has been removed - now in Global Codes - table needs to reamin temporarily for Raj. Will be removed when all EDITariff_Reference_NZ files/db is dropped")]
	[CodeProperty(NZCSupplier.Schema.U8_CustomsCode), DescriptionProperty(NZCSupplier.Schema.U8_Description)]
	public class NZCSupplier : AutoNZCSupplier
	{
		#region Schema

		public new class Schema : AutoNZCSupplier.Schema
		{
			public const string U8_Description = "U8_Description";
		}

		#endregion

		public NZCSupplier(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override ZString HumanReadableNameCore => Res.GetString("F817FBE0-B24C-4795-A2D0-1FCAAE7BF745", "Supplier {0}", U8_CustomsCode);

		#region U8_Description
		public ZString U8_Description
		{
			get { return U8_CompanyName + " (" + U8_CountryCode + ")"; }
		}

		public ZPropertyInfo U8_DescriptionInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.U8_Description);
			}
		}
		#endregion
	}
}
