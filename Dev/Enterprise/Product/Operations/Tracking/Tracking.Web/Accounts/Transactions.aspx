<%@ Page Language="c#" CodeBehind="Transactions.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.Accounts.Transactions" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head>
    <title>Transactions</title>
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" method="post" runat="server">
        <div id="OuterContentPane" runat="server">

            <div id="TitleDiv" runat="server">
                <edi:ZTextLabel ID="TitleLabel" runat="server" CssClass="PageTitle">Statement of Account</edi:ZTextLabel>
            </div>
            <div id="UnauthorisedDiv" runat="server" class="ContentSection">
                <edi:ZTextLabel ID="UnauthorisedLabel" runat="server"></edi:ZTextLabel>
            </div>
            <div id="NoStatementDiv" runat="server" visible="false">
                <edi:ZTextLabel ID="NoStatementLabel" runat="server" CssClass="ErrorMessage"></edi:ZTextLabel>
            </div>
            <div id="AuthorisedContent" runat="server">
                <div id="Company" runat="server" class="ContentSection">
                    Company:
					<edi:ZGuidDropDownList ID="CompanyDropDownList" runat="server" AutoPostBack="True"
                        DataValueField="PK" DataTextField="GC_Name" BindToList="Companies" BindTo="Company"
                        descriptionfieldname="GC_Name" codefieldname="GC_Name" valuefieldname="PK" showdescription="False"
                        readonly="True">
                    </edi:ZGuidDropDownList>
                    &nbsp;&nbsp;<asp:Button ID="StatementButton" runat="server"
                        Text="View Statement of Account" OnClick="StatementButton_Click"></asp:Button>
                    <asp:Label ID="Message" runat="server" CssClass="ErrorMessage"></asp:Label>
                </div>
                <div class="ContentSection">
                    <edi:ZTextLabel ID="Ztextlabel1" runat="server" CssClass="PageTitle">Transactions</edi:ZTextLabel>
                    <div id="SearchControlHolder" runat="server">
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
