using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.MasterFiles;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI
{
	public partial class CusClassificationForm : BaseClassificationForm
	{
		private readonly System.ComponentModel.Container components;

		public CusClassificationForm(CusClassification classification)
			: base(classification)
		{
		}

		protected override BaseClassificationUserControl GetUserControl()
		{
			return new CusClassificationUserControl();
		}

		public override string FormCaption
		{
			get { return Enterprise.Customs.NZ.GUI.Res.GetString("8640F7B9-1D30-4AD6-B9E8-16609DD4A165", "Classification Lookup"); }
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}


