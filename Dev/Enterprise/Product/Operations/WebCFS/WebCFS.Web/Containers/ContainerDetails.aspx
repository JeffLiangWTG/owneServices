<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ContainerDetails.aspx.cs" Inherits="Enterprise.WebCFS.Web.ContainerDetails" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	<head>
        <title>Container Details</title>
    </head>
    <body>    
		<form id="Form1" runat="server">    
			<div id="OuterContentPane" runat="server">
				<div id="Title">
					<edi:ZTextLabel id="ContainerLabel" runat="server" CssClass="PageTitle" Text="CONTAINER #: "/>
					<edi:ZTextLabel id="ValueContainerLabel" runat="server" CssClass="PageTitle" BindTo="JC_ContainerNum"/>
				</div>				
				<br/>
				<edi:ztextlabel id="DateUnpackedLabel" runat="server" Text="DATE UNPACKED: "/>
				<edi:zdatetimelabel id="ValueDateUnpackedLabel" runat="server" BindTo="JC_PackUnpackDate"/>
				<br/>
				<edi:ztextlabel id="DateAvailableLabel" runat="server" Text="DATE AVAILABLE: "/>
				<edi:zdatetimelabel id="ValueDateAvailableLabel" runat="server" BindTo="JC_LCLAvailable_Readonly"/>
				<br/>
				<edi:ztextlabel id="DateStarageLabel" runat="server" Text="DATE STORAGE: "/>
				<edi:zdatetimelabel id="ValueDateStarageLabel" runat="server" BindTo="JC_LCLStorageCommences_Readonly"/>      
				<br/>
				<edi:ztextlabel id="DateArrivedLabel" runat="server" Text="DATE ARRIVED: "/>
				<edi:zdatetimelabel id="ValueDateArrivedLabel" runat="server" BindTo="JC_ArrivalTime"/>      
				<br/>
				<edi:ztextlabel id="NoPackLinesGridLabel" runat="server" Text="The pack lines for this container will be available after the container has been unpacked"/>
				<edi:zcollapsablepanel id="PackLinesPanel" runat="server" CssClass="SectionTitle" DisableCollapsing="True" Label="Pack Lines">
					<edi:ZDataGrid ID="PackLinesGrid" runat="server" AutoGenerateColumns="False" BindTo="PackLines"	CssClass="DetailsTable">
						<HeaderStyle CssClass="DetailsHeader" />
						<PagerStyle Mode="NumericPages" PageButtonCount="20" />
						<ItemStyle CssClass="DetailsCell" />
					</edi:ZDataGrid>
				</edi:zcollapsablepanel>
			</div>
        </form>
    </body>
</html>
