<%@ Control Language="c#" AutoEventWireup="false" CodeBehind="WebVotingBanner.ascx.cs" Inherits="Enterprise.MarketingManager.WebVoting.WebVotingBanner" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<script type="text/javascript">
    var countdownPanel;
    var countdownLabel;
    var countdownRef1;
    var countdownRef2;

    var lastCountdownValue;
    var originalTop;

    var shouldAutoSubmit = true;

    function getVarsAndStartCountdown(panelID, labelID, fieldID) {
        countdownPanel = document.getElementById(panelID);
        countdownLabel = document.getElementById(labelID);
        countdownRef1 = document.getElementById(fieldID);
        countdownRef2 = new Date(new Date().valueOf() + (countdownRef1.value * 1000));

        lastCountdownValue = countdownRef1.value * 1;
        setTimeout("countdown()", 1000);

        window.onscroll = adjustCountdownPanel;
        window.onresize = adjustCountdownPanel;

        setCountdownLabel(lastCountdownValue);
    }

    function countdown() {
        var countdownValue = Math.ceil((countdownRef2 - new Date()) / 1000);
        if (countdownValue > lastCountdownValue) {
            alert("The system clock has been compromised. Your answers will now be submitted automatically");
            autoSubmit();
        }
        else {
            lastCountdownValue = countdownValue;
            setCountdownLabel(countdownValue);

            if (countdownValue > 0) {
                setTimeout("countdown()", 1000);
            }
            else if (shouldAutoSubmit) {
                autoSubmit();
            }
        }
    }

    function setCountdownLabel(value) {
        var hoursText = ("0" + Math.floor(value / 3600).toString());
        hoursText = hoursText.substring(hoursText.length - 2);
        var minsText = ("0" + Math.floor((value % 3600) / 60).toString());
        minsText = minsText.substring(minsText.length - 2);
        var secsText = ("0" + Math.floor(value % 60).toString());
        secsText = secsText.substring(secsText.length - 2);

        countdownLabel.innerHTML = hoursText + ":" + minsText + ":" + secsText;
    }

    function adjustCountdownPanel() {
        if (originalTop == null) {
            originalTop = countdownPanel.offsetTop;
        }
        countdownPanel.style.top = (document.documentElement.scrollTop > originalTop)
            ? document.documentElement.scrollTop - originalTop : 0;
    }

    function SubmitAnswers(message) {
        shouldAutoSubmit = false;
        var result = window.confirm(message);
        if (!result) {
            shouldAutoSubmit = true;
        }
        return result;
    }

    function HandleAutoSubmit() {
        if (window.confirm('<%=TimeExpiredConfirmationText%>')) {
            manualSubmit();
        }
        else {
            cancelSubmit();
        }
    }
</script>

<div id="divWebVotingControlContainer">
    <div id="divLogo">
        <asp:HyperLink ID="LogoImage" RunAt="server"/>
    </div>
    <div id="divWarningContainer" class="warningMessageContainer textAlignRight">
        <asp:Label ID="WarningCaptionLabel" RunAt="server" CssClass="warningCaption" Visible="false"></asp:Label>
        <asp:Label ID="WarningMessageLabel" RunAt="server" CssClass="warningMessage" Visible="false"></asp:Label>
    </div>
	<div id="divCountdownPanel" class="countdownPanel">
		<asp:Panel ID="CountdownPanel" RunAt="server" Visible="false">
            Time Remaining: <asp:Label ID="CountdownLabel" CssClass="countdownLabel" RunAt="server"></asp:Label>
		</asp:Panel>
    </div>
</div>