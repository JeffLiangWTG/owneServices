using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USWHSPackLine)]
	public class USWHSPackLineAddInfo : AutoUSWHSPackLineAddInfo
	{
		public USWHSPackLineAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new WHSPackLine Parent
		{
			get { return (WHSPackLine)base.Parent; }
			protected set { base.Parent = value; }
		}

		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				var parent = Parent;
				return parent == null ? null : parent.InvoiceLine;
			}
		}

		[List(nameof(Lookups) + "." + nameof(USWHSPackLineAddInfoLookups.InvoiceLineList))]
		public override ZGuid US_JI_InvoiceLine
		{
			get { return base.US_JI_InvoiceLine; }
			set { base.US_JI_InvoiceLine = value; }
		}

		[List(nameof(Lookups) + "." + nameof(USWHSPackLineAddInfoLookups.WHSPackList))]
		public override ZGuid US_B7_WHSPack
		{
			get { return base.US_B7_WHSPack; }
			set { base.US_B7_WHSPack = value; }
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get
			{
				return new SchemaColumn[]
				{
					USWHSPackLineAddInfoSchema.US_JI_InvoiceLine,
					USWHSPackLineAddInfoSchema.US_B7_WHSPack
				};
			}
		}
	}
}
