<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HAWBList.aspx.cs" Inherits="Enterprise.Tracking.Web.HAWBList" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>
<%@ Register TagPrefix="edi_tracking" Namespace="Enterprise.Tracking.Web" Assembly="Enterprise.Tracking.Web" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
	<head>
		<title>List of Attached HAWBs</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
						<edi:ZTextLabel id="ShipmentDetailsLabel" runat="server" CssClass="PageTitle">HAWBs Attached to </edi:ZTextLabel>
						<edi:ZTextLabel id="ZTextLabel5" runat="server" CssClass="PageTitle" BindTo="EH_WayBillNumber"></edi:ZTextLabel>
				</div>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ZTextLabel id="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
				</div>
				<div id="AuthorisedContent" runat="server" >
					<div id="JumpButtons" runat="server" class="ContentSection">
						<asp:LinkButton ID="ParentBill" runat="server" Text="" />
						<div id="ReloadGridSection" runat="server" class="WarningSection" style="display: none;">
							|
							<span class="WarningSectionText">HAWBs have been edited.</span>
							<asp:Button ID="ReloadGrid" runat="server" Text="Reload HAWB List" />
						</div>
					</div>
					<div class="ContentSection">
						<a name="HAWBSection">
                            <asp:UpdatePanel ID="HAWBsUpdatePanel" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
							        <edi:zgrid EnableViewState="false" id="RelatedHAWBsGrid" runat="server" CssClass="DetailsTable" BindTo="WebChildBills" Caption="" Collapsed="false" DisableCollapsing="true">
								        <ItemStyle CssClass="DetailsCell"></ItemStyle>
								        <HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
							        </edi:zgrid>
                                </ContentTemplate>
                            </asp:UpdatePanel>
						</a>
					</div>
				</div>
				<div id="NotFoundError" runat="server">
					<edi:ZTextLabel id="ShipmentNotFoundLabel" runat="server" CssClass="PageTitle"></edi:ZTextLabel>
				</div>
			</div>
		</form>
	</body>
</html>
