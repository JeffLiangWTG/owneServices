<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Enterprise.MarketingManager.WebVoting.Login" %>

<%@ Register Assembly="Enterprise.ZArchitecture.Web.GUI" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" TagPrefix="cc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Auto Login</title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="campaignDetailContainer">
            <asp:Panel runat="server" ID="GroupedExamCampaignSummary">
                <div class="topSection">
                    <table>
                        <tr>
                            <td id="detailCaption" class="campaignDetailCaption">
                                <asp:Literal runat="server" ID="ExamNameLiteral"></asp:Literal>:&nbsp;</td>
                            <td class="campaignDetailText">
                                <asp:Literal runat="server" ID="ExamNameValueLiteral"></asp:Literal></td>
                        </tr>
                    </table>
                </div>
            </asp:Panel>
            <cc1:zrepeater id="CampaignDetailRepeater" bindto="VoteExamSurveyDetails" runat="server">
                <HeaderTemplate>
                    <table>
                </HeaderTemplate>
                <ItemTemplate>
                        <tr>
                            <td id="detailCaption" class="campaignDetailCaptionNonGrouped"><%# Eval("Key") %>:&nbsp;</td>
                            <td class="campaignDetailText"><%# Eval("Value") %></td>
                        </tr>
                </ItemTemplate>
                <FooterTemplate>
                        <tr>
                            <td class="campaignDetailCaptionNonGrouped"></td>
                            <td class="campaignDetailText"><br /><cc1:ZButton ID="StartButton1" Text="<%# StartButtonText %>" RunAt="server" CssClass="campaignStartButton" OnClick="StartButton_Click"/></td>
                        </tr>
                    </table>
                </FooterTemplate>
            </cc1:zrepeater>
            <div class="bottomSection">

                <div id="divWarningMessageContainer" class="warningMessageContainer warningMessageHeading textAlignCenter">
                    <br />
                    <br />
                    <cc1:ztextlabel id="WarningCaptionLabel" cssclass="warningCaption" runat="server"></cc1:ztextlabel>
                    <cc1:ztextlabel id="WarningMessageLabel" cssclass="warningMessage" runat="server"></cc1:ztextlabel>
                </div>
                <cc1:ztextlabel id="ErrorMessageLabel" runat="server" bindto="ErrorMessage"></cc1:ztextlabel>
                <div style="float: left; text-align: left;">
                    <asp:Literal id="FooterTextForExamLiteral" runat="server"></asp:Literal>
                </div>
            </div>
        </div>
    </form>
</body>
<head>
     <style type="text/css">
        .lastExamCol2, lastExamCol3, lastExamCol4, lastExamCol5 {
            text-align: center;
            vertical-align: text-center;
        }

        .lastExamCol1 {
            width: 200px;
            text-align: left;
            vertical-align: text-center;
        }

        .lastExamCol2 {
            width: 70px;
        }

        .lastExamCol3 {
            width: 180px;
        }

        .lastExamCol4 {
            width: 120px;
        }

        .lastExamCol5 {
            width: 50px;
        }

        .bottomSection {
            margin-left: 20px;
            text-align: center;
            padding-bottom: 50px;
        }

        .topSection {
            margin-left: 20px;
            text-align: center;
            padding-top: 20px;
            padding-bottom: 10px;
        }

        .previousExamsSection {
            margin-top: 20px;
        }

        .examSectionHeading {
            font-weight: bold;
            text-align: left;
            margin-bottom: 5px;
        }

        .startIndent {
            margin-left: 150px;
        }

        .gridHeader {
            font-weight: bold;
            text-align: center;
            background-color: #EBEBEB;
            font-size: 11px;
            color: black;
            border: solid 0px #DCDCDC;
        }

        .testsGrid {
            font-family: tohoma
        }

        .gridRow
        {
	        font-family: Tahoma;
	        font-size: 10px;
	        border-collapse: separate;
	        border: solid 0px #DCDCDC;
	        padding: 3px;
        }
    </style>
</head>
</html>
