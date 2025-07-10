<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProductImage.aspx.cs" Inherits="Enterprise.Tracking.Web.ProductImage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Product Image</title>
</head>
<body id="DefaultBody" class="HidingBGImageInPopUpWindows" runat="server">
    <form id="form1" runat="server">
		<div id="OuterContentPane" runat="server">
			<div id="Title">
				<edi:ZTextLabel ID="WhsOrderLabel" runat="server" CssClass="PageTitle">Product Image</edi:ZTextLabel>
			</div>
			<div id="UnauthorisedDiv" class="ContentSection" runat="server">
				<edi:ZTextLabel ID="UnauthorisedLabel" runat="server" />
			</div>
			<div id="AuthorisedContent" class="ContentSection" runat="server">
				<div id="NotFoundError" runat="server">
					<edi:ZTextLabel ID="NotFoundLabel" runat="server" />
			    </div> 
				<div id="ProductImageContents" runat="server">	                
					<div class="ContentSection">
						<table class="ResultsTable" id="OrderDetailsTable">
							<tbody>
                                <tr>
                                    <td class="DetailsItem" style="vertical-align: middle;">Product:</td>
                                    <td colspan="2">
										<edi:ZTextLabel ID="ProductCode" runat="server" BindTo="Part.OP_PartNum" />                                        
                                    </td>
                                    <td>&nbsp;</td>
                                    <td colspan="3" class="DetailsItem" style="vertical-align: middle;">
										<edi:ZTextLabel ID="ZTextLabel1" runat="server" BindTo="Part.OP_Desc" />                                        
                                    </td>
                                </tr>
                                <tr>
                                    <td class="DetailsItem ProductImageTable" colspan="7">
										<div class="ProductImageContainer">											
											<span class="ProductImageVAHelper"></span>
											<asp:Image id="ProductImageControl" runat="server" CssClass="ProductImageControl"/>											
										</div>
                                    </td>
				                </tr>				                                               
							</tbody>
						</table>
					</div>					
					<div class="ContentSection" id="CloseButtonDiv" runat="server">
						<asp:Button ID="CloseWindow" runat="server" Text="Close Window" OnClick="Save_Click"></asp:Button>
					</div>
				</div>
			</div>
        </div>
    </form>
</body>
</html>
