<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HAWBs.aspx.cs" Inherits="Enterprise.Tracking.Web.HAWBs" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
	<head>
		<title>HAWB - FHL</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
	    		<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server" CssClass="PageTitle"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server">
					<div id="Title">
						<edi:ZTextLabel id="PageTitleLabel" runat="server" CssClass="PageTitle">HAWB - FHL Messages</edi:ZTextLabel>
					</div>
					<div id="SearchControlHolder" runat="server"></div>
				</div>
			</div>
		</form>
	</body>
</html>