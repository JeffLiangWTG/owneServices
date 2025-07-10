<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>

<%@ Page Language="c#" ViewStateEncryptionMode='Always' EnableViewStateMac="false" CodeBehind="Login.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.Login" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Login</title>
    <meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1" />
    <meta name="CODE_LANGUAGE" content="C#" />
    <meta name="vs_defaultClientScript" content="JavaScript" />
    <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge;" />
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" method="post" runat="server">
        <div id="OuterContentPane" runat="server">
            <div id="loginBox" class="loginBox">
                <div id="CompanyCode" runat="server">
                    Company Code:<br />
                    <edi:ZTextBox ID="CompanyCodeTextBox" runat="server" BindTo="CompanyCode" MaxLength="12"></edi:ZTextBox>
                    <br />
                </div>
                E-mail:<br />
                <edi:ZTextBox ID="LoginNameTextBox" runat="server" BindTo="UserName" MaxLength="40"></edi:ZTextBox>
                <br />
                Password:<br />
                <edi:ZTextBox ID="PasswordTextBox" TextMode="password" runat="server" BindTo="Password" MaxLength="40"></edi:ZTextBox>
                <br />
                <edi:ZCheckBox ID="ZCheckBox1" runat="server" BindTo="RememberMe" Text="Remember me"></edi:ZCheckBox>
                <br />
                <br />
                <asp:Button ID="SigninBtn" runat="server" Text="Login" OnClick="SigninBtn_Click"></asp:Button>
                &nbsp;&nbsp;&nbsp;
					<edi:ZTextLabel ID="Message" runat="server" CssClass="ErrorMessage"
                        BindTo="LoginErrorMsg" EnableHtmlEncoding="True" />
                <br />
                <asp:HyperLink ID="PasswordReminder" runat="server" OnPreRender="PasswordReminderPrerender" NavigateUrl="~/Login/ForgotPassword.aspx">Forgot your password?</asp:HyperLink>
            </div>
            <br />
            <br />
            <div id="QuickViewDetails" class="loginBox" runat="server">
                <div id="ViewShipmentDetails" runat="server">
                    Shipment/House Bill/Direct Master Number:<br />
                    <edi:ZTextBox ID="ShipmentHousebillNumberTextbox" runat="server" BindTo="QuickViewNumber" MaxLength="40" ValidationGroup="QuickView"></edi:ZTextBox>
                    <br />
                    <br />
                </div>
                <div id="ViewContainerDetails" runat="server">
                    Container Number:<br />
                    <edi:ZTextBox ID="ContainerNumberTextBox" runat="server" BindTo="ContainerQuickViewNumber" ValidationGroup="QuickView"></edi:ZTextBox>
                    <br />
                    <br />
                </div>
                <asp:Button ID="FindBtn" runat="server" Text="Find" OnClick="FindBtn_Click" ValidationGroup="QuickView"></asp:Button>
                &nbsp;&nbsp;&nbsp;
					<edi:ZTextLabel ID="QuickViewMessage" runat="server" CssClass="ErrorMessage"
                        BindTo="QuickViewErrorMsg" EnableHtmlEncoding="True" />

            </div>
            <br />
            <br />
            <div id="LoginInstructionContent" runat="server" class="LoginInstruction">
                <edi:ZTextLabelNoEncode ID="LoginInstructionLabel" runat="server"></edi:ZTextLabelNoEncode>
            </div>
        </div>
    </form>
    <script type="text/javascript">

        function callButtonClick(e) {
            var btn = document.getElementById("FindBtn");
            var evt = e || window.event;
            if (evt.keyCode === 13) {
                btn.click();
                e.preventDefault();
                return false;
            }
        }

        function setFocusOnFindButton() {
            var txtcontainer = document.getElementById("ContainerNumberTextBox");
            var txtshipment = document.getElementById("ShipmentHousebillNumberTextbox");

            if (txtcontainer) {
                txtcontainer.addEventListener("keypress", callButtonClick);
            }

            if (txtshipment) {
                txtshipment.addEventListener("keypress", callButtonClick);
            }

            setTimeout(function () {
                document.getElementById("<%=DefaultTextBox%>").focus();
            }, 10);
        }

            window.onload = setFocusOnFindButton();

    </script>
</body>
</html>
