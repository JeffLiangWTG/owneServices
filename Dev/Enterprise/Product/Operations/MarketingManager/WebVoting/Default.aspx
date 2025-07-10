<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Enterprise.MarketingManager.WebVoting.Default" MaintainScrollPositionOnPostBack="False" ValidateRequest="false"%>
<%@ Register Src="Base/WebVotingBanner.ascx" TagName="WebVotingBanner" TagPrefix="uc1" %>
<%@ Register Assembly="Enterprise.ZArchitecture.Web.GUI" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" TagPrefix="cc1" %>
<%@ Register TagPrefix="vot" Namespace="Enterprise.MarketingManager.WebVoting" Assembly="Enterprise.MarketingManager.WebVoting" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Campaign</title>
    <style type="text/css">
        body.noscroll {
            position: fixed;
            overflow: hidden;
        }

        #fade {
            position: absolute;
            left: 0%;
            top: 0%;
            background-color: #fff;
            -moz-opacity: 0.0;
            opacity: .0;
            filter: alpha(opacity=0);
            width: 100%;
            height: 100%;
            z-index: 90;
        }

        #overlay {
            position: fixed;
            width: 440px;
            top: 50px;
            left: 50%;
            margin-left: -220px;
            background-color: #fff;
            padding: 25px 25px 20px 25px;
            border: 1px solid #000;
            z-index: 100;
        }

            #overlay #okButton {
                background-color: #fff;
                border: 1px solid #000;
                width: 80px;
                height: 30px;
                position: relative;
                right: -350px;
                margin-top: 10px;
            }
    </style>
    <script language="javascript" type="text/javascript">
        window.onerror = function (msg, url, lineNo, columnNo, error) {
			if (!url || url.substring(0, 8).toLowerCase() != "https://" || url.endsWith("/mootools.js"))
			{
                return false;
            }
            var userAgent = '';
            var errorMessage = '';
            var errorName = '';
            var errorStack = '';
            if (error) {
                errorMessage = error.message;
                errorName = error.name;
                errorStack = error.stack;
            }
            if (navigator && navigator.userAgent) {
                userAgent = navigator.userAgent;
            }
            var message = [
                'Message: ' + msg,
                'URL: ' + url,
                'Line: ' + lineNo,
                'Column: ' + columnNo,
                'Error Name: ' + errorName,
                'Error Message: ' + errorMessage,
                'Error Stack: ' + errorStack,
                'User Agent: ' + userAgent
            ].join(' - ');
            PageMethods.ReportError(message);
            return false;
        };

        function ShowErrorMessage() {
            document.body.className = 'noscroll';
            document.getElementById('overlay').style.display = 'block';
            document.getElementById('fade').style.display = 'block';
        }

        function HideErrorMessage() {
            document.body.className = '';
            document.getElementById('overlay').style.display = 'none';
            document.getElementById('fade').style.display = 'none';
        }
	</script>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManagerErrorReporting" runat="server" EnablePageMethods="true" />
    <div>
        <cc1:ZTextLabel ID="VoteTitleLabel" CssClass="campaignTitle" runat="server"></cc1:ZTextLabel>
        <div class="campaignContent">
            <asp:PlaceHolder id="ContentPlaceHolder" runat="server"></asp:PlaceHolder>            
        </div>
        <div class="submitButtonPanel">
            <cc1:zbutton id="SubmitButton" runat="server" text="Submit" OnClick="SubmitButton_Click" ></cc1:zbutton>
        </div>

        <div id="fade" style="display:none;"></div>
        <div id="overlay" style="display:none;">
            <div id="ErrorMessageContainer" runat="server"></div>
            <input id="okButton" type="button" value="OK" onclick="HideErrorMessage();" />
        </div>
            
    </div>
    </form>
	<script type="text/javascript">
		if (typeof JSON.stringify !== "function") {
			JSON.stringify = function (value, replacer, space) { return StandardBuiltInJSON.stringify(value, replacer, space); };
		}
		if (typeof JSON.parse !== "function") {
			JSON.parse = function (text, reviver) { return StandardBuiltInJSON.parse(text, reviver); };
		}
	</script>
</body>
</html>
