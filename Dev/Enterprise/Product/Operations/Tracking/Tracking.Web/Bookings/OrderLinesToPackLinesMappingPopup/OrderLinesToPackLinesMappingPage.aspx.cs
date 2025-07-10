using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Bookings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public partial class OrderLinesToPackLinesMappingPage : BasePageWithAuthorisation
	{
		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			var nonLinkedOrders = HttpContext.Current.Session["nonLinkedOrders"] as List<Order>;
			if (nonLinkedOrders != null)
			{
				nonLinkedOrders = Factory.Load<Order>(new ZQuery(JobOrderHeaderSchema.PK, nonLinkedOrders.Select(x => x.PK))).ToList();
			}

			return new OrderLineToPackLineConversionHelper(Factory, Booking.QuotedBooking.Booking, nonLinkedOrders);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			NotificationFlags.DisplayAll = true;
			DefaultBody.Style.Add("background-image", (NoResString)"none");// css fix in popup

			if (Booking == null || ConversionHelper == null)
			{
				OrderLinesLabel.Text = Res.GetString("fbb4819d-58b6-4285-94d7-ff5216aae332", "Order Lines Not Found");
				NotFoundLabel.Text = Res.GetString("0f9112de-39b6-49a1-917f-a5132109fe02", "Order lines were not found in the database or you don't have rights to attach them.");
				LineContents.Visible = false;
			}
			else
			{
				ConversionHelper.ClearAllNotifications();
				OrderLinesGrid.ClearMultiLineSelection();
				DummyPackLinesGrid.ClearMultiLineSelection();
				NotFoundError.Visible = false;
				LineContents.Visible = true;
			}
		}

		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			base.OnInit(e);
			InitializeComponent();
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			NotFoundLabel.BindTo = null;
		}

		protected OrderLineToPackLineConversionHelper ConversionHelper
		{
			get { return DataSource as OrderLineToPackLineConversionHelper; }
		}

		protected string BookingKey
		{
			get { return GetStringFromParameter("Ref"); }
		}

		protected TrackingBooking Booking
		{
			get
			{
				if (booking == null)
				{
					if (HttpContext.Current.Session[BookingKey] != null)
					{
						booking = (TrackingBooking)HttpContext.Current.Session[BookingKey];
					}
					else if (ZGuid.TryParse(BookingKey, out var bookingPK))
					{
						var forwardingShipment = Factory.Load<ForwardingShipment>(bookingPK);
						if (forwardingShipment != null)
						{
							booking = new TrackingBooking(forwardingShipment.PK, Factory, SiteUser);
						}
					}
				}

				return booking;
			}
		}
		TrackingBooking booking;

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser != null && SiteUser.CanViewOrders; }
		}

		#region Overrides to hide page header and menu

		protected override string PageHeaderControlPath
		{
			get { return string.Empty; }
		}

		protected override bool ShowLoginStatus
		{
			get { return false; }
		}

		protected override bool ShowCloseWindowInPopup
		{
			get { return false; }
		}

		protected override bool ShowFooter
		{
			get { return false; }
		}

		#endregion

		protected override string GetSaveButtonClientID()
		{
			return Finish.ClientID;
		}

		protected void RedirectToEditBooking()
		{
			ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "ClosePopupScript", RedirectScript, false);// Redirect path
		}

		string RedirectScript
		{
			get
			{
				return FormattableString.Invariant($@"<SCRIPT TYPE=""text/javascript"">
					try{{
						if (window.opener && !window.opener.closed){{
							window.opener.location = ""{AppInstance.EditBookingPage}?Ref={Booking.PK}&{DataSourceInSessionParameterName}={DataSourceInSessionParameterValue}"";
						}}
					}} catch(err) {{ }}
					window.close();
				</SCRIPT>"); // Javascript segment
			}
		}

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();
			if (!DataSource.HasErrors && !DataSource.HasMessageErrors)
			{
				RedirectToEditBooking();
			}
			else
			{
				NotificationFlags.DisplayAll = true;
				NotificationFlags.DisplayWarnings = false;
			}
		}

		#region Page Confirmations

		protected override void AddPageConfirmations(List<ZPageConfirmation> confirmations)
		{
			base.AddPageConfirmations(confirmations);
			confirmations.Add(new SplitOrderConfirmation(this));
		}

		#endregion

		#region Grid setup

		protected override void SetupGrids()
		{
			SetupOrderLinesGrid();
			SetupDummyPackLinesGrid();
		}

		protected void SetupOrderLinesGrid()
		{
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(ResString.GetMultilingualString("3690e69f-850a-4532-9d38-b5da3d882318", "Line#"), OrderLine.Schema.JO_LineNo) { Decimals = 0, ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZCodeFindBoxColumn(ResString.GetMultilingualString("fded6d9f-18ad-48d4-870d-2ad0a73a691b", "Part#"), OrderLine.Schema.JO_Partno) { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZTextEditColumn(ResString.GetMultilingualString("32d1d592-f99e-4a5f-9bb5-ef5e055347a6", "Order No"), "Order+JD_OrderNumber") { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZTextEditColumn(ResString.GetMultilingualString("cf64294b-322e-4fbd-97d3-febc5503b7cb", "Buyer"), "Order+Buyer+OH_FullName") { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZTextEditColumn(ResString.GetMultilingualString("6a131c83-6307-4df3-ba56-a376f6c5f50b", "Supplier"), "Order+Supplier+OH_FullName") { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZTextEditColumn(ResString.GetMultilingualString("43d01ff1-edb6-43b3-b9e4-bb246b278a22", "Description"), OrderLine.Schema.JO_Description) { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(ResString.GetMultilingualString("f34b3f31-d6aa-4032-8099-a51867a6f16b", "Inner Packs"), OrderLine.Schema.JO_InnerPacks) { Decimals = 0, ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(ResString.GetMultilingualString("c15307e9-8d7a-4539-8f88-f79957c542ba", "Outer Packs"), OrderLine.Schema.JO_OuterPacks) { Decimals = 0, ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(ResString.GetMultilingualString("aaf0e8f6-b8b4-4c32-ab87-9771b6069707", "Qty Ordered"), OrderLine.Schema.JO_Quantity) { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(ResString.GetMultilingualString("d8a9cda3-dadf-48b3-9d28-c85906d7c909", "Qty Invoiced"), OrderLine.Schema.JO_QtyInvoiced) { ReadOnly = Enterprise.Registry.Business.OrdersDataRegistry.Instance.OrderLineQtyRemainingManagement.Value, AutoPostBack = true });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(ResString.GetMultilingualString("2a4c6a46-5eeb-4eb0-a25b-63a98adc166d", "Qty Received"), OrderLine.Schema.JO_QtyReceived) { ReadOnly = !Enterprise.Registry.Business.OrdersDataRegistry.Instance.OrderLineQtyRemainingManagement.Value, AutoPostBack = true });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(ResString.GetMultilingualString("2d619696-b474-41f7-884a-ab1f3647e5f5", "Qty Remaining"), OrderLine.Schema.JO_QuantityRemaining) { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZDropDownListColumn(ResString.GetMultilingualString("cbad40f4-65a2-46cf-aee6-05ec507d4726", "Unit of Qty"), OrderLine.Schema.JO_F3_NKPackType) { BindToList = "JO_F3_NKPackType_List", EditorWidth = 100, ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(ResString.GetMultilingualString("af6ef4d7-16bc-4d7b-b6fc-6adf6d25e04a", "Item Price"), OrderLine.Schema.JO_ItemPrice) { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZCalcEditColumn(ResString.GetMultilingualString("c4d33ce0-d646-41b5-8fcb-e1a31edc80b4", "Total Price"), OrderLine.Schema.JO_LinePrice) { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZDateTimeColumn(ResString.GetMultilingualString("4b572dfc-7e2f-4aeb-a034-ce1f7658a9d1", "Required In Store Date"), OrderLine.Schema.JO_LineDropDate, ZDateTimePickerFormat.Short) { ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZDropEditColumn(ResString.GetMultilingualString("de7ac4a8-d051-40b6-4e1d-4b33b5dd4394", "Incoterm"), OrderLine.Schema.JO_INCO) { BindToList = "JO_INCO_List", EditorWidth = 100, DisplayStyle = OComboBoxDropDownStyle.CodeAndDescription, ReadOnly = true });
			OrderLinesGrid.Columns.Add(new ZTextEditColumn(ResString.GetMultilingualString("ad6d01ae-c960-474c-8352-406a4e5746b8", "Additional Terms"), OrderLine.Schema.JO_AdditionalTerms) { EditorWidth = 200, ReadOnly = true });

			ZBindToChecker.CheckBindTo(((ICodeDescriptionPairList)((OrderLine)null).JO_F3_NKPackType_List));
			ZBindToChecker.CheckBindTo((ZDateTime)((OrderLine)null).JO_LineDropDate);
			ZBindToChecker.CheckBindTo(((ICodeDescriptionPairList)((OrderLine)null).JO_INCO_List));
		}

		protected void SetupDummyPackLinesGrid()
		{
			DummyPackLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("040f6f91-dfaf-46db-bf47-9bb84ddc9287", "Container Number"), "ContainerNumber") { ReadOnly = true });
			DummyPackLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("02ac2ecb-aef1-4693-a203-714a531057df", "Volume"), Res.GetString("02ac2ecb-aef1-4693-a203-714a531057df", "Volume")) { Decimals = 0, ReadOnly = true });
			DummyPackLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("22471017-fbcd-48f5-ab56-dd28618f2920", "Unit of Volume"), "VolumeUnit") { ReadOnly = true });
			DummyPackLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("25272f75-c19b-49ff-97e6-e9465599f7f2", "Weight"), Res.GetString("25272f75-c19b-49ff-97e6-e9465599f7f2", "Weight")) { Decimals = 0, ReadOnly = true });
			DummyPackLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("4310565d-0484-4cac-87c6-a84e55618c65", "Unit of Weight"), "WeightUnit") { ReadOnly = true });
			DummyPackLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("9174d248-d6c2-4911-891b-fc4ca05d7411", "Products"), Res.GetString("9174d248-d6c2-4911-891b-fc4ca05d7411", "Products")) { ReadOnly = true });
			DummyPackLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("518e73c2-b318-4eb1-b6b7-ac4ecaeacd59", "Description"), Res.GetString("518e73c2-b318-4eb1-b6b7-ac4ecaeacd59", "Description")) { ReadOnly = true });
			DummyPackLinesGrid.Columns.Add(new ZTextEditColumn(Res.GetString("4bfb7d92-b09c-4e8b-abb8-b2bdea65e41d", "Origin"), Res.GetString("4bfb7d92-b09c-4e8b-abb8-b2bdea65e41d", "Origin")) { ReadOnly = true });
			DummyPackLinesGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("3fda1839-748c-4569-85c5-cf1da6e30a2a", "Line Price"), "LinePrice") { ReadOnly = true });
		}

		#endregion

		#region EventHandlers

		protected void Cancel_Click(object sender, EventArgs e)
		{
			HttpContext.Current.Session["nonLinkedOrders"] = null;
			RedirectToEditBooking();
		}

		protected void Finish_Click(object sender, EventArgs e)
		{
			NotificationFlags.DisplayAll = true;
			SuppressErrorDialog = false;

			SaveRequest = true;

			foreach (Order orgOrder in ConversionHelper.OriginalOrders)
			{
				ZGuid[] pks = ConversionHelper.OrderLines.Select(o => o.PK).ToArray();
				foreach (OrderLine orgOrderLine in orgOrder.OrderLines)
				{
					if (pks.Contains(orgOrderLine.PK))
					{
						OrderLine tempOrderLine = ConversionHelper.OrderLines.First(o => o.PK.Equals(orgOrderLine.PK));
						orgOrderLine.JO_QtyReceived = tempOrderLine.JO_QtyReceived;
					}
				}
			}

			HandleSaveConfirmations();
			if (!HasOutstandingRequiredConfirmations())
			{
				var newOrders = new List<Order>();

				newOrders = ConversionHelper.GetNewOrSplitOrdersFromOriginalOrdersList(CreateOrderType.Split);

				if (newOrders.Count > 0)
				{
					var stringBuilder = new ZStringBuilder();
					foreach (Order order in newOrders)
					{
						stringBuilder.Append(order.JD_OrderNumberAndSplit);
					}

					var message = Res.GetString("a1b99b75-c6b2-4037-af02-d5d6543b856e", "The following orders have been split and saved successfully: ");
					message += stringBuilder.ToStringWithDelimiterBetweenAppends(", ");

					ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "MessageAlert", (NoResString)"alert('" + message + (NoResString)"')", true);// Redirect path
				}

				SaveDataSourceFactory();

				ConversionHelper.CreatePackLines();
				HttpContext.Current.Session[BookingKey] = Booking;
				HttpContext.Current.Session["nonLinkedOrders"] = null;

				RedirectToEditBooking();
			}
		}

		protected void Merge_Click(object sender, EventArgs e)
		{
			ZGuid[] selectedOrderLines = OrderLinesGrid.GetSelectedPKs();

			if (selectedOrderLines.Length > 1)
			{
				OrderLine[] toBeMerged = ConversionHelper.OrderLines.Where(n => selectedOrderLines.Contains(n.PK)).ToArray();
				ConversionHelper.CreateDummyPackLine(toBeMerged);
				Bind();
			}
			else
			{
				errorMessage = Res.GetString("c9dcf55a-c582-4442-b9e0-6b09a2dc7a37", "Please select order lines to merge or press 'Finish' to create pack line for every non-merged order line.");
			}
		}

		protected void Undo_Click(object sender, EventArgs e)
		{
			ZGuid[] selectedDummyPackLines = DummyPackLinesGrid.GetSelectedPKs();
			if (selectedDummyPackLines.Length > 0)
			{
				DummyPackLine[] toBeUndone = ConversionHelper.DummyPackLines.ToArray<DummyPackLine>().Where(n => selectedDummyPackLines.Contains(n.PK)).ToArray();
				ConversionHelper.UndoDummyPackLines(toBeUndone);
				Bind();
			}
			else
			{
				errorMessage = Res.GetString("4b5b57e3-a545-4e1f-84ea-28b1ce4f8e49", "Please select combined pack line(s) to undo.");
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (!string.IsNullOrEmpty(errorMessage))
			{
				ZStringBuilder message = new ZStringBuilder();
				message.Append((NoResString)"Please fix the following errors before proceeding:\\n\\n"); // Client Message
				message.Append(errorMessage);

				ConversionHelper.AddRowError(errorMessage);
			}
			base.OnPreRender(e);
		}

		string errorMessage = string.Empty;

		#endregion

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LinesMappingPage;
		}

		protected override string GetPageName()
		{
			return (NoResString)"Lines Mapping Page";// Redirection path
		}
	}
}
