<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProductDetails.aspx.cs" Inherits="Enterprise.Tracking.Web.ProductDetails" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Warehouse Order Details</title>
</head>
<body id="DefaultBody" runat="server">
	<form id="Form1" method="post" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ZTextLabel ID="WhsOrderLabel" CssClass="PageTitle" runat="server">Product Profile</edi:ZTextLabel></div>
			<div id="UnauthorisedDiv" runat="server" class="ContentSection">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
			</div>
			<div id="AuthorisedContent" runat="server">
				<div id="NotFoundError" runat="server" class="ContentSection">
					<edi:ZTextLabel ID="NotFoundLabel" runat="server" CssClass="DetailsItem"></edi:ZTextLabel>
				</div>				
				<div id="ProductContents" class="ProductContents" runat="server">
					<div id="ProductContentsInfo" class="ProductContentsInfo">
						<table class="ResultsTable">
							<tr>
								<td>
									<div class="ContentSection" id="Details" runat="server">
										<table class="ResultsTable" id="Table1">
											<tbody>
												<tr id="CodeRow">
													<td>Code
													</td>
													<td>
														<edi:ZTextLabel ID="Code" runat="server" BindTo="Part.OP_PartNum" />
													</td>
												</tr>
												<tr id="DescriptionRow">
													<td>Description
													</td>
													<td>
														<edi:ZTextLabel ID="Description" runat="server" BindTo="Part.OP_Desc" />
													</td>
												</tr>
												<tr id="CommodityRow">
													<td>Commodity
													</td>
													<td>
														<edi:ZCodeFindBoxLabel ID="Commodity" runat="server" BindTo="Part.OP_RH_NKCommodityCode" BindToList="Part.Lookups.CommodityCodes"
															DisplayStyle="CodeAndDescription" />
													</td>
												</tr>
												<tr id="StockUnitRow">
													<td>Stock Unit
													</td>
													<td>
														<edi:ZCodeLookupLabel ID="StockUnit" runat="server" BindTo="Part.OP_StockKeepingUnit" BindToList="Part.Lookups.OP_ProductUQ_List" DisplayStyle="CodeAndDescription" />
													</td>
												</tr>
												<tr id="DecimalPlacesRow">
													<td>Decimal Places
													</td>
													<td>
														<edi:ZNumericLabel ID="DecimalPlaces" runat="server" BindTo="Part.OP_CountDecimalPlaces" />
													</td>
												</tr>
												<tr id="LastCostRow">
													<td>Last Cost</td>
													<td>
														<edi:ZNumericLabel ID="ZNumericLabel1" runat="server" BindTo="Part.OP_LastCost" />
														<edi:ZTextLabel ID="ZNumericLabelCurrency" runat="server" BindTo="Part.OP_RX_NKLastWeightedCostCurr" />
													</td>
												</tr>
												<tr id="IsActiveRow">
													<td>Is Active
													</td>
													<td>
														<edi:ZTextLabel ID="IsActive" runat="server" BindTo="Part.OP_IsActive" />
													</td>
												</tr>
											</tbody>
										</table>
									</div>
								</td>
								<td>&nbsp;&nbsp;</td>
								<td>
									<div class="ContentSection" id="DimensionsAndWeight" runat="server">
										<table class="ResultsTable" id="Table2">
											<tbody>
												<tr>
													<td nowrap="nowrap" colspan="5">
														<edi:ZTextLabel ID="DimensionsAndWeightLabel" runat="server" CssClass="DetailsItem">Dimensions and Weight</edi:ZTextLabel></td>
												</tr>
												<tr>
													<td></td>
													<td>Depth&nbsp;
													</td>
													<td>Height&nbsp;
													</td>
													<td>Width&nbsp;
													</td>
													<td>Unit
													</td>
												</tr>
												<tr id="DimensionRow">
													<td>Dimension
													</td>
													<td>
														<edi:ZNumericLabel ID="Depth" runat="server" BindTo="Part.OP_Depth" />
													</td>
													<td>
														<edi:ZNumericLabel ID="Height" runat="server" BindTo="Part.OP_Height" />
													</td>
													<td>
														<edi:ZNumericLabel ID="Width" runat="server" BindTo="Part.OP_Width" />
													</td>
													<td>
														<edi:ZCodeLookupLabel ID="Unit" runat="server" BindTo="Part.OP_MeasureUQ" BindToList="Part.Lookups.OP_MeasureUQ_List" DisplayStyle="CodeOnly" />
													</td>
												</tr>
												<tr id="WeightRow">
													<td>Weight
													</td>
													<td>
														<edi:ZNumericLabel ID="Weight" runat="server" BindTo="Part.OP_Weight" />
													</td>
													<td colspan="3">
														<edi:ZCodeLookupLabel ID="WeightUnit" runat="server" BindTo="Part.OP_WeightUQ" BindToList="Part.Lookups.OP_WeightUQ_List" DisplayStyle="CodeOnly" />&nbsp; per &nbsp;
					                                <edi:ZCodeLookupLabel ID="StockUnit1" runat="server" BindTo="Part.OP_StockKeepingUnit" BindToList="Part.Lookups.OP_ProductUQ_List" DisplayStyle="CodeOnly" />
													</td>
												</tr>
												<tr id="CubeRow">
													<td>Cube
													</td>
													<td>
														<edi:ZNumericLabel ID="Cubic" runat="server" BindTo="Part.OP_Cubic" />
													</td>
													<td colspan="3">
														<edi:ZCodeLookupLabel ID="CubicUnit" runat="server" BindTo="Part.OP_CubicUQ" BindToList="Part.Lookups.OP_CubicUQ_List" DisplayStyle="CodeOnly" />&nbsp; per &nbsp;
					                                <edi:ZCodeLookupLabel ID="StockUnit2" runat="server" BindTo="Part.OP_StockKeepingUnit" BindToList="Part.Lookups.OP_ProductUQ_List" DisplayStyle="CodeOnly" />
													</td>
												</tr>
												<tr id="PalletSizeRow">
													<td>Pallet Size
													</td>
													<td>
														<edi:ZNumericLabel ID="StockKeepingUnitPerPallet" runat="server" BindTo="Part.OP_StockKeepingUnitPerPallet" />
													</td>
													<td colspan="4">
														<edi:ZCodeLookupLabel ID="StockUnit3" runat="server" BindTo="Part.OP_StockKeepingUnit" BindToList="Part.Lookups.OP_ProductUQ_List" DisplayStyle="DescriptionOnly" />
													</td>
												</tr>
											</tbody>
										</table>
									</div>
								</td>
							</tr>
						</table>
						<edi:ZGrid ID="UnitConversionsGrid" runat="server" CssClass="DetailsTable"
							BindTo="Part.PartUnits" AllowAdd="False" AllowDelete="False" AllowEdit="False" ShowFooter="False"
							DisableCollapsing="true" Caption="Unit Conversions">
							<PagerStyle Mode="NumericPages"></PagerStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:ZGrid>
						<edi:ZGrid ID="ParamsByWhsAndClientGrid" runat="server" CssClass="DetailsTable" BindTo="Product.ParamsByWhsAndClient"
							DisableCollapsing="true" Caption="Warehouses">
							<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:ZGrid>
						<edi:ZGrid ID="DocumentsGrid" runat="server" Caption="Documents" CssClass="DetailsTable" BindTo="DocumentHelper.Documents" DisableCollapsing="true">
							<PagerStyle NextPageText="Next" PrevPageText="Prev" Mode="NumericPages"></PagerStyle>
							<ItemStyle CssClass="DetailsCell"></ItemStyle>
							<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
						</edi:ZGrid>
					</div>				
					<div id="ProductImageDiv" class="ProductImage"><asp:Image id="ProductImageControl" runat="server" CssClass="ProductImageControl ProductImageBoxBorder" /></div>
				</div>
				<div id="Clearing" class="Clearing"></div>				
            </div>
        </div>
    </form>
</body>
</html>
