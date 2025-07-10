<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TermsAndConditions.aspx.cs" Inherits="Enterprise.Tracking.Web.TermsAndConditions" %>

<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Terms and Conditions</title>
</head>
<body id="DefaultBody" runat="server">
    <form id="Form1" runat="server">
        <div id="OuterContentPane" runat="server">
            <div id="Title">
                <edi:ZTextLabel ID="TitleLabel" runat="server" CssClass="PageTitle">Terms and Conditions</edi:ZTextLabel>
            </div>
            <div id="TermsAndConditionsContent" runat="server" class="ContentSection">
                <edi:ZTextLabelNoEncode ID="TermsAndConditionsText" runat="server"></edi:ZTextLabelNoEncode>
            </div>
            <div id="ButtonsDiv" runat="server" class="ContentSection">
                <asp:Button ID="IAgreeButton" runat="server" Text="I have read and agree to the Terms and Conditions" OnClick="IAgreeButton_Click" Style="min-width: 60px;" />&nbsp;
                <asp:Button ID="IDisagreeButton" runat="server" Text="I do not agree to these Terms and Conditions" OnClick="IDisagreeButton_Click" Style="min-width: 60px;" />&nbsp;
                <asp:Button ID="PrintButton" runat="server" Text="Print these Terms and Conditions" OnClientClick="javascript:CallPrint('TermsAndConditionsContent');" Style="min-width: 60px;" />
            </div>
        </div>
    </form>
</body>
<script language="javascript">
    function CallPrint(strid) {
        var prtContent = document.getElementById(strid);
        var WinPrint = window.open('', '', 'location=0,toolbar=1,status=1,scrollbars=1,resizable=1');
        WinPrint.document.write(prtContent.innerHTML);
        WinPrint.document.close();
        WinPrint.focus();
        WinPrint.print();
        prtContent.innerHTML = strOldOne;
    }
</script>
</html>
