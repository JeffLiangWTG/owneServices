<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MAWBs.aspx.cs" Inherits="Enterprise.Tracking.Web.MAWBs" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
		<title>MAWB - FWB</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
	    		<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server" CssClass="PageTitle"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server">
					<div id="Title">
						<edi:ZTextLabel id="PageTitleLabel" runat="server" CssClass="PageTitle">MAWB - FWB Messages</edi:ZTextLabel>
					</div>
					<div id="SearchControlHolder" runat="server"></div>
				</div>
			</div>
		</form>
	</body>
</html>
