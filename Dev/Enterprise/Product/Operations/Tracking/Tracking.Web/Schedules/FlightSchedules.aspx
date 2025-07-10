<%@ Page language="c#" Codebehind="FlightSchedules.aspx.cs" AutoEventWireup="True" Inherits="Enterprise.Tracking.Web.Schedules.FlightSchedules" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
		<title>Flight Schedules</title>
		<meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1"/>
		<meta name="CODE_LANGUAGE" content="C#"/>
		<meta name="vs_defaultClientScript" content="JavaScript"/>
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5"/>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server">
				<div id="Title">
					<edi:ZTextLabel id="TitleLabel" runat="server" CssClass="PageTitle">Flight Schedules</edi:ZTextLabel>
				</div>
				<div id="SearchControlHolder" runat="server"></div>
			</div>
		</form>
	</body>
</html>
