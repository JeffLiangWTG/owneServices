<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExamSurveyUserControl.ascx.cs" Inherits="Enterprise.MarketingManager.WebVoting.ExamSurveyUserControl" %>
<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
    Namespace="System.Web.UI" TagPrefix="asp" %>
<%@ Register Assembly="Enterprise.ZArchitecture.Web.GUI" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" TagPrefix="edi" %>


<div class="questionsContainer">
    <edi:ZRepeater ID="QuestionsRepeater" runat="server" BindTo="PagedAnswerWrappers" OnItemDataBound="QuestionsRepeater_ItemDataBound">
    </edi:ZRepeater>
</div>
<div class="pageNavigator">
    <div class="pageNavigatonNextPrevious">
        <edi:ZButton runat="server" id="PreviousButton" Text="Previous" OnClick="PreviousButton_Click"/>
        <edi:ZButton runat="server" id="NextButton" Text="Next" OnClick="NextButton_Click"/>
    </div>
    <div class="pageNavigatorSubmit">
        <edi:ZButton runat="server" id="SubmitButton" Text="Submit" OnClick="SubmitButton_Click" />
    </div>
    
     <div class="jumpToPage" id="JumpToPageDiv" runat="server">
        <div class="jumpToPageCaption">
	        <edi:ZTextLabel id="JumpToPageLabel" runat="server" />:&nbsp;
	        <a href="javascript:showLegendWindow();">            
            <img style="border:0px;" src="Images/helpicon.png" width="15" height="15" />
           </a>
            <script type ="text/javascript">
                function showLegendWindow() {
                    window.open("Images/legend.jpg", "_blank", "width=428,height=232,resizable=no,scrollbars=no");
                }
            </script>
        </div>
        <asp:ImageMap runat="server" ID="PageNavigator" ImageUrl="~/PageNavigator.aspx" HotSpotMode="PostBack" OnClick="PageNavigator_Click" />               
    </div>
    
   
</div>
