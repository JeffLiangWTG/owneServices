<%@ page language="C#" autoeventwireup="true" codebehind="Unsubscribe.aspx.cs" inherits="Enterprise.MarketingManager.WebVoting.Unsubscribe" %>

<%@ register assembly="Enterprise.ZArchitecture.Web.GUI" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" tagprefix="cc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Auto Unsubscribe</title>
    <style type="text/css">
        #UnsubscribeResultContainer {
            height: 14em;
            vertical-align: middle;
            line-height: 7em;
            font-size: large;
        }

        #UnsubscribeResultContainer table {
            margin: 0 auto;
            text-align: center;
        }

        #ResultImage {
            width: 64px;
            height: 64px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div id="UnsubscribeResultContainer">
            <table>
                <tr>
                    <td><img id="ResultImage" src="UnsubscribeImages/success.png" alt="" runat="server" /></td>
                    <td><cc1:ZTextLabel id="ResultMessageLabel" runat="server" bindto="ResultMessage" /></td>
                </tr>
                <tr>
                    <td colspan="2"><a id="Resubscribe" runat="server" href="#"></a></td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
