<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eDocAttachPage.aspx.cs" Inherits="Enterprise.Tracking.Web.eDocAttachPage" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
	<title>Add Document</title>
	<base target="_self" />
</head>
<body class="UploadFileDialog">
	<form id="Form1" method="post" runat="server">
		<div>
			<div id="UnauthorisedDiv" runat="server" class="ContentSection">
				<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
			</div>
			<div id="AuthorisedContent" runat="server" class="ContentSection">
				<div id="DocumentDetails" runat="server" class="ContentSection" style="white-space:nowrap;">
					<input type="hidden" runat="server" id="dangerousFileMessageHidden" />
					<input type="hidden" runat="server" id="zeroSizeFileMessageHidden" />
					<input type="hidden" runat="server" id="maximumFileSizeHidden" />
					<input type="hidden" runat="server" id="maximumFileSizeMessageHidden" />
					<edi:ZTextLabel runat="server" id="DocumentTypeLabel" style="width:117px;text-align:right;">Document Type:</edi:ZTextLabel>
					<edi:ZDropDownList runat="server" ID="DocumentType" BindTo="DocumentUploadHelper.DocType" BindToList="DocumentUploadHelper.DocTypes" DataValueField="RT_DocType" DataTextField="RT_Desc"></edi:ZDropDownList>
				</div>
				<div id="FileUploadDetails" runat="server" class="ContentSection">
				</div>
			</div>
		</div>
	</form>
</body>
</html>
