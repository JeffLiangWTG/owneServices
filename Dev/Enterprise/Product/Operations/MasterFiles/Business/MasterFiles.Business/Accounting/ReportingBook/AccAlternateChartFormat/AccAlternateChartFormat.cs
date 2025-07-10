using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(AccAlternateChart), "AlternateChartFormats")]
	public class AccAlternateChartFormat : AutoAccAlternateChartFormat
	{
		public AccAlternateChartFormat(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			this.ANF_Format = "99";
			this.ANF_Description = "desc";
			this.ANF_Separator = "-";
		}

#endif
		[MaxLength(255)]
		public override ZString ANF_Description { get => base.ANF_Description; set => base.ANF_Description = value; }

		[MaxLength(20)]
		public override ZString ANF_Format
		{
			get => base.ANF_Format;
			set
			{
				base.ANF_Format = value.ToUpper();
			}
		}

		[MaxLength(1)]
		[List("Lookups.SeparatorList")]
		public override ZString ANF_Separator { get => base.ANF_Separator; set => base.ANF_Separator = value; }
	}
}
