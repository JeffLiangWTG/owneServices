<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Submission.aspx.cs" Inherits="Enterprise.MarketingManager.WebVoting.Submission" %>
<%@ Register Assembly="Enterprise.ZArchitecture.Web.GUI" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" TagPrefix="cc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Submission</title>
     <style type="text/css">
        .lastExamCol1, lastExamCol2 {
            text-align: left;
            vertical-align: text-center;
        }

        .lastExamCol1 {
            width: 200px;
        }

        .lastExamCol2 {
            width: 120px;
            text-align: left;
        }

        .bottomSection {
            margin-left: 20px;
            text-align: center;
            padding-bottom: 50px;
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
            text-align: left;
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

        .gridRow a, .gridAlternatingRow a
        {
            color: #666666;
        }

        .gridRow a:hover, .gridAlternatingRow a:hover
        {
            color: #00A4E4;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div id="TextDiv" style="padding-top: 60px; padding-bottom: 10px; text-align:center;">
          <span ID="SubmissionMessageLabel" runat="server">
          </span>
        </div>
    </form>
</body>
</html>
