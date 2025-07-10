namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ExportAWBSpecialHandlingCollection : Forwarding.AWB.Business.ExportAWBSpecialHandlingCollection
	{
		public ExportAWBSpecialHandlingCollection(ExportAWBHeader parent)
			: base(parent)
		{
			if (parent is ConsolExportAWBHeader)
			{
				MaxCountValidationEnable(9,
					Res.GetString("6c945224-82c6-e48e-4bc8-cb126f480c2e",
						"A maximum of nine Special Handling Codes is possible for the FWB message"));
			}
		}

		public new ExportAWBSpecialHandling this[int index]
		{
			get { return (ExportAWBSpecialHandling)(Elements[index]); }
		}

		public new ExportAWBSpecialHandling AddNew()
		{
			return (ExportAWBSpecialHandling)base.AddNew();
		}

		public void ValidateAll()
		{
			foreach (ExportAWBSpecialHandling item in this)
			{
				item.Validation.ValidateAll();
			}
		}
	}
}
