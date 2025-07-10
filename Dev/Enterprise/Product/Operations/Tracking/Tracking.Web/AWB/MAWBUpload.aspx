<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MAWBUpload.aspx.cs" Inherits="Enterprise.Tracking.Web.MAWBUpload" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="upload" Namespace="Brettle.Web.NeatUpload" Assembly="Brettle.Web.NeatUpload" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
	<head>
		<title>Upload New MAWB File</title>
	</head>
	<body id="DefaultBody" runat="server" class="popup">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
						<edi:ztextlabel id="Label1" runat="server" CssClass="PageTitle">Upload New MAWB File</edi:ztextlabel>
				</div>
				<div id="UnauthorisedDiv" runat="server" class="ContentSection">
					<edi:ztextlabel id="UnauthorisedLabel" runat="server"></edi:ztextlabel>
				</div>
				<div id="AuthorisedContent" runat="server" >
					<div class="WizardPanel">
						<div id="BasicInfo" class="ContentSection" runat="server">
							<asp:Label ID="Label4" runat="server" CssClass="SectionTitle">Basic MAWB Details</asp:Label><br />
							<div runat="server" class="ImportSection ImportBasicInfo" id="Basic">
								<div style="width: 50%; float:left">
									<asp:Label runat="server" CssClass="ImportBasicInfoLabel">MAWB #:</asp:Label>
									<asp:Label runat="server" ID="BasicMAWBNumber" /><br />
								</div>
<%--								<div style="width: 50%; float:left">
									<asp:Label runat="server" CssClass="ImportBasicInfoLabel">Origin:</asp:Label>
									<asp:Label runat="server" ID="BasicOrigin" /><br />
								</div>--%>
								<div style="width: 50%; float:left">
									<asp:Label ID="Label5" runat="server" CssClass="ImportBasicInfoLabel">Flight Date:</asp:Label>
									<asp:Label runat="server" ID="BasicFlightDate" /><br />
								</div>
								<div style="width: 50%; float:left">
									<asp:Label runat="server" CssClass="ImportBasicInfoLabel">Destination:</asp:Label>
									<asp:Label runat="server" ID="BasicDestination" /><br />
								</div>
								<asp:Label style="clear: left;" runat="server" CssClass="ImportBasicInfoLabel">HAWB #'s:</asp:Label>
								<asp:Label runat="server" ID="BasicHAWBNumbers" /><br />
							</div>
						</div>
						<div id="ValidationErrors" class="ContentSection" runat="server">
							<asp:Label ID="Label2" runat="server" CssClass="ImportValidationErrorLabel">Errors</asp:Label><br />
							<div runat="server" class="ImportSection ImportValidationError" id="ImportValidationError" />
						</div>
						<div id="ValidationWarnings" class="ContentSection" runat="server">
							<asp:Label ID="Label3" runat="server" CssClass="ImportValidationWarningLabel">Warnings</asp:Label><br />
							<div runat="server" class="ImportSection ImportValidationWarning" id="ImportValidationWarning" />
						</div>
						<div id="ValidationInfo" class="ContentSection" runat="server">
							<asp:Label ID="Label6" runat="server" CssClass="ImportValidationInfoLabel">Information</asp:Label><br />
							<div runat="server" class="ImportSection ImportValidationInfo" id="ImportValidationInfo" />
						</div>
						<div id="UploadSection" class="ContentSection" runat="server">
							Upload File: <upload:InputFile ID="InputFile" runat="server" /><br />
							<upload:ProgressBar runat="server" ID="ProgressBar" Inline="true" />
						</div>
						<div id="JumpButtons" runat="server" class="ContentSection CenteredPanel">
							<edi:ZButton ID="NextButton" runat="server" Text="Validate" CssClass="WizardButton" OnClick="Next_Click" />
						</div>
					</div>
				</div>
			</div>
		</form>
	</body>
</html>
