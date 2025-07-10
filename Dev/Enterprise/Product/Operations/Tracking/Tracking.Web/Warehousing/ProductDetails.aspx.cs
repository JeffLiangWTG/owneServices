using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Web
{
	public partial class ProductDetails : BasePageWithAuthorisation
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		#region Overrides

		protected override string GetPageName()
		{
			return WebTracker.Pages.ProductDetails;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ProductProfileDetailsPage;
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupPage();
		}

		protected void SetupPage()
		{
			bool productIsValid = (Product != null);

			ProductContents.Visible = productIsValid;
			NotFoundError.Visible = !productIsValid;
			NotFoundLabel.Text = Res.GetString("7d217ab0-e81e-42fe-a71d-a1551df6b869", "Product was not found in the database or you don't have rights to view it.");
			DocumentsGrid.Visible = SiteUser.CanViewDocuments;

			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_PartNum);
			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_Desc);
			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_RH_NKCommodityCode);
			ZBindToChecker.CheckBindTo((OrgSupplierPartLookups)((TrackingSupplierPart)null).Part.Lookups);
			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_StockKeepingUnit);
			ZBindToChecker.CheckBindTo((ZByte)((TrackingSupplierPart)null).Part.OP_CountDecimalPlaces);
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingSupplierPart)null).Part.OP_LastCost);
			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_RX_NKLastWeightedCostCurr);
			ZBindToChecker.CheckBindTo((ZBool)((TrackingSupplierPart)null).Part.OP_IsActive);
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingSupplierPart)null).Part.OP_Depth);
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingSupplierPart)null).Part.OP_Height);
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingSupplierPart)null).Part.OP_Width);
			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_MeasureUQ);
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingSupplierPart)null).Part.OP_Weight);
			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_WeightUQ);
			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_StockKeepingUnit);
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingSupplierPart)null).Part.OP_Cubic);
			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_CubicUQ);
			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_MeasureUQ);
			ZBindToChecker.CheckBindTo((ZString)((TrackingSupplierPart)null).Part.OP_StockKeepingUnit);
			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingSupplierPart)null).Part.OP_StockKeepingUnitPerPallet);
			ZBindToChecker.CheckBindTo((OrgPartUnitCollection)((TrackingSupplierPart)null).Part.PartUnits);
			ZBindToChecker.CheckBindTo((WhsProductParamsByWhsAndClientCollection)((TrackingSupplierPart)null).Product.ParamsByWhsAndClient);
			ZBindToChecker.CheckBindTo((DocumentViewCollection)((TrackingSupplierPart)null).DocumentHelper.Documents);

			var eDocImage = Product != null ? Product.ProductImage : null;
			if (eDocImage != null)
			{
				SetProductImageIfAny(eDocImage);
			}
			else
			{
				ProductImageControl.Style["border"] = (NoResString)"0px"; // numeric value
			}
		}

		void SetProductImageIfAny(IeDoc productImage)
		{
			this.ProductImageControl.ImageUrl = string.Format((NoResString)"{0}?Ref={1}&Doc={2}", Enterprise.DocumentScanning.Web.eDocsRequestHandler.RequestHelper.BaseUrl, Product.Part.PK.ToString(), productImage.UniqueKey.ToString()); // String formater
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return true; }
		}

		#endregion Overrides

		#region BusinessObject

		protected override BusinessObject GetNewDataSource()
		{
			TrackingSupplierPart fProduct = null;

			if (ProductPK.IsValid)
			{
				fProduct = TrackingSupplierPart.FromPKFilteredByContact(Factory, ProductPK, SiteUser);

				if (fProduct == null && OrderPK.IsValid)
				{
					fProduct = TrackingSupplierPart.FromPKFilteredByRelatedOrder(Factory, ProductPK, OrderPK, SiteUser);
				}
			}

			return fProduct;
		}

		protected
#if DEBUG
 virtual
#endif
 TrackingSupplierPart Product
		{
			get { return DataSource as TrackingSupplierPart; }
		}

		protected ZGuid OrderPK
		{
			get { return GetGuidFromParameter("OrderRef"); }
		}

		protected ZGuid ProductPK
		{
			get { return GetGuidFromParameter(RefParameterName); }
		}

		#endregion BusinessObject

		#region SetupGrids

		protected override void SetupGrids()
		{
			base.SetupGrids();
			SetupUnitConversionsGrid();
			SetupParamsByWhsAndClientGrid();
		}

		#endregion

		#region OnPreBind

		protected override void OnPreBind()
		{
			base.OnPreBind();
			SetupDocumentsGrid(DocumentsGrid);
		}

		#endregion

		#region UnitConversionsGrid

		void SetupUnitConversionsGrid()
		{
			UnitConversionsGrid.ColumnProvider = new UnitConversionColumnProvider();
		}

		#endregion

		#region ParamsByWhsAndClientGrid

		void SetupParamsByWhsAndClientGrid()
		{
			ParamsByWhsAndClientGrid.ColumnProvider = new ParamsByWarehouseAndClientColumnProvider();
		}

		#endregion
	}
}
