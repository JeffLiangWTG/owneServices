<%@ Register TagPrefix="cc1" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
		<title>Application Error</title>
	</head>
	<body id="DefaultBody" runat="server">
		<form id="Form1" method="post" runat="server">
			<div id="OuterContentPane" runat="server" style="text-align:center;padding-top:50px">
				<div id="Title">
					<h1 runat="server" id="PageTitle">Application Error</h1>
				</div>
				<div>
					<asp:Literal id="MessageDescription" runat="server" Text="A problem has been encountered with the Web Application. Sorry for any inconvenience this may have caused."></asp:Literal>
				</div>
			</div>
		</form>
	</body>
</html>
