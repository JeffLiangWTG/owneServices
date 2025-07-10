using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(RefCusMapType.Schema.ZZP_MapType), DescriptionProperty(RefCusMapType.Schema.ZZP_Description)]
	public sealed class RefCusMapType : AutoRefCusMapType
	{
		public RefCusMapType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.MapDirectionList")]
		public override ZString ZZP_Direction
		{
			get { return base.ZZP_Direction; }
			set { base.ZZP_Direction = value; }
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZP_IsReadonly = ZBool.False;
		}
	}
}
