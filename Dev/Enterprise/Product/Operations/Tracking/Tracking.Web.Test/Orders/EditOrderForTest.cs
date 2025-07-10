using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class EditOrderForTest : EditOrder
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public EditOrderForTest()
		{
			IsCreateNewAppInstanceIfNullForTest = true;

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			OrderLabel = new ZTextLabel();
			SaveOrder = new Button();
			CancelOrder = new Button();

			OrderLinesGrid = new ZDataGrid();
			OrderLinesGrid.BindTo = "OrderLines";
			OrderLinesGrid.AllowEdit = true;
			OrderLinesGrid.AllowDelete = true;
			Controls.Add(OrderLinesGrid);
			SetupOrderLinesGrid();
		}

		public new void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		public void SelectedIndexChangedPostBackForTest()
		{
			this.WeightVolumeDropDown_SelectedIndexChanged(null, EventArgs.Empty);
		}

		public ZDataGrid OrderLinesGridForTest
		{
			get { return OrderLinesGrid; }
		}

		protected override void OrderLinesGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			ItemDataBoundWasCalled = true;
			base.OrderLinesGrid_ItemDataBound(sender, e);
		}

		public void SaveButtonClick()
		{
			this.SaveOrder_Click(null, EventArgs.Empty);
		}

		protected override BusinessObject GetNewDataSource()
		{
			return OrderForTest;
		}

		public TrackingOrder OrderForTest
		{
			get
			{
				if (fOrderForTest == null)
				{
					fOrderForTest = Factory.New<TrackingOrder>();
				}
				return fOrderForTest;
			}
			set
			{
				fOrderForTest = value;
			}
		}
		TrackingOrder fOrderForTest;

		public int GetProductColumnIndexExposed()
		{
			return GetProductColumnIndex();
		}

		public bool ItemDataBoundWasCalled;
	}
}
