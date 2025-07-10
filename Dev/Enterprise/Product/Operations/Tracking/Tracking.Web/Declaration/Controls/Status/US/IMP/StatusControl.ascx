<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StatusControl.ascx.cs" Inherits="Enterprise.Tracking.Web.Declaration.US.IMP.StatusControl" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI" %>
<%@ Register TagPrefix="edi" Namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" Assembly="Enterprise.ZArchitecture.Web.GUI.Ajax" %>

<table class="ResultsTable">
    <tr>
        <td colspan="2">
            <asp:Label ID="StatusHeaderLabel" runat="server" CssClass="SectionTitle">Status Summary</asp:Label></td>
    </tr>
    <tr>
        <td>
            <div id="AreaCargoReleaseStatus">
                <table id="TableCargoReleasStatus" class="ResultsTable">
                    <tr>
                        <td colspan="3">
                            <asp:Label CssClass="SectionTitle" runat="server">Cargo Release Status: </asp:Label><edi:ztextlabel runat="server" id="ReleaseStatusDescText" bindto="ReleaseStatusDesc"></edi:ztextlabel>
                            (<edi:ztextlabel runat="server" id="ReleaseStatusText" bindto="ReleaseStatus"></edi:ztextlabel>)
                            <edi:zdatetimelabel id="ReleaseDateText" runat="server" bindto="JE_EntryAuthorisationDate" datetimeformat="Long" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <edi:zgrid id="DispositionGrid" runat="server" cssclass="DetailsTable" bindto="DispositionCodesView" disablecollapsing="False" label="Release Status" orderby="StatusDate DESC">
					<ItemStyle CssClass="DetailsCell"></ItemStyle>
					<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
				</edi:zgrid>
                        </td>
                    </tr>
                </table>
            </div>
        </td>
        <td>
            <div id="Area7501Status">
                <table id="Table7501Status" class="ResultsTable">
                    <tr>
                        <td colspan="3">
                            <asp:Label ID="Status7501Header" runat="server" CssClass="SectionTitle">7501 Status: </asp:Label><edi:zcodelookuplabel id="ZCodeLookupLabel1" runat="server" bindto="EntrySummaryStatus" displaystyle="CodeAndDescription"></edi:zcodelookuplabel>
                            <edi:zdatetimelabel id="ZDateTimeLabel2" runat="server" bindto="ENSStatusDate" datetimeformat="Long" />
                        </td>
                    </tr>

                    <tr>
                        <td colspan="3">
                            <edi:zgrid id="EntrySummaryStatusGrid" runat="server" cssclass="DetailsTable" bindto="ENSE0Records" disablecollapsing="False" label="Entry Summary Status (E0)" orderby="StatusDate DESC">
					<ItemStyle CssClass="DetailsCell"></ItemStyle>
					<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
				</edi:zgrid>
                        </td>
                    </tr>
                </table>
            </div>
        </td>
    </tr>
    <tr>
        <td colspan="2">
            <edi:zgrid id="PGAStatusGrid" runat="server" cssclass="DetailsTable" bindto="EntryPGACusDispositions" disablecollapsing="False" label="Entry PGA Status">
			<ItemStyle CssClass="DetailsCell"></ItemStyle>
			<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
		</edi:zgrid>
        </td>
    </tr>
    <tr>
        <td colspan="2">
            <edi:zgrid id="PGALineStatusGrid" runat="server" cssclass="DetailsTable" bindto="OGADispositionCodes" disablecollapsing="False" label="Entry PGA Line Status">
			<ItemStyle CssClass="DetailsCell"></ItemStyle>
			<HeaderStyle CssClass="DetailsHeader"></HeaderStyle>
		</edi:zgrid>
        </td>
    </tr>
  
    <tr>
        <td>
            <div id="AreaStatementOfPayment">
                <table class="ResultsTable">
                    <tr>
                        <td colspan="3">
                            <asp:Label ID="StatementOfPaymentHeader" runat="server" CssClass="SectionTitle">Statement or Payment</asp:Label></td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="NumberLabel" runat="server" CssClass="DetailsItem">Number:</asp:Label></td>
                        <td>
                            <edi:zfindboxlabel id="Number" runat="server" bindto="RelatedStatementPK" displaystyle="DescriptionOnly" />
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="StatusLabel" runat="server" CssClass="DetailsItem">Status:</asp:Label></td>
                        <td>
                            <edi:zcodelookuplabel id="SOFStatus" runat="server" bindto="RelatedStatement.B2_Status" displaystyle="CodeAndDescription"></edi:zcodelookuplabel>
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="PaymentDateLabel" runat="server" CssClass="DetailsItem">Payment Date:</asp:Label></td>
                        <td>
                            <edi:zdatetimelabel id="PaymentAuthorizationDate" runat="server" bindto="RelatedStatement.B2_PaymentAuthorizationDate" />
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="DueDateLabel" runat="server" CssClass="DetailsItem">Due Date:</asp:Label></td>
                        <td>
                            <edi:zdatetimelabel id="DueDate" runat="server" bindto="RelatedStatement.B2_DueDate" />
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="PrintDateLabel" runat="server" CssClass="DetailsItem">Print Date:</asp:Label></td>
                        <td>
                            <edi:zdatetimelabel id="PrintDate" runat="server" bindto="RelatedStatement.B2_PrintDate" />
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="CheckNoLabel" runat="server" CssClass="DetailsItem">Check No:</asp:Label></td>
                        <td>
                            <edi:ztextlabel id="CheckNo" runat="server" bindto="US_CheckNo"></edi:ztextlabel>
                        </td>
                    </tr>
                </table>
            </div>
            <div runat="server" id="AreaITStatus">
                <table class="ResultsTable">
                    <tr>
                        <td colspan="3">
                            <asp:Label ID="ITStatusHeader" runat="server" CssClass="SectionTitle">IT Status: </asp:Label><edi:zcodelookuplabel id="ITMessageStatus" runat="server" bindto="InBondStatus" displaystyle="CodeAndDescription"></edi:zcodelookuplabel>
                            <edi:zdatetimelabel id="ITMessageStatusDate" runat="server" bindto="ITStatusDate" datetimeformat="Long" />
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="DepartureStatusLabel" runat="server" CssClass="DetailsItem">Departure Status:</asp:Label></td>
                        <td>
                            <edi:zcodelookuplabel id="DepartureStatus" runat="server" bindto="ITDepartureStatus" displaystyle="CodeAndDescription"></edi:zcodelookuplabel>
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="ArrivalStatusLabel" runat="server" CssClass="DetailsItem">Arrival Status:</asp:Label></td>
                        <td>
                            <edi:zcodelookuplabel id="ArrivalStatus" runat="server" bindto="ITArrivalStatus" displaystyle="CodeAndDescription"></edi:zcodelookuplabel>
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="ExportStatusLabel" runat="server" CssClass="DetailsItem">Export Status:</asp:Label></td>
                        <td>
                            <edi:zcodelookuplabel id="ExportStatus" runat="server" bindto="ITExportStatus" displaystyle="CodeAndDescription"></edi:zcodelookuplabel>
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="TOLStatusLabel" runat="server" CssClass="DetailsItem">TOL Status:</asp:Label></td>
                        <td>
                            <edi:zcodelookuplabel id="TOLStatus" runat="server" bindto="ITTOLStatus" displaystyle="CodeAndDescription"></edi:zcodelookuplabel>
                        </td>
                    </tr>
                </table>
            </div>
        </td>
        <td>
            <div id="AreaAIIStatus" runat="server">
                <table class="ResultsTable">
                    <tr>
                        <td colspan="3">
                            <asp:Label ID="AIIStatusHeader" runat="server" CssClass="SectionTitle">AII Status: </asp:Label><edi:zcodelookuplabel id="AIIMessageStatus" runat="server" bindto="ElectronicInvoiceStatus" displaystyle="CodeAndDescription"></edi:zcodelookuplabel></td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <asp:Label ID="URecordHeader" runat="server" CssClass="SectionTitle">(U*) Record</asp:Label></td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="InvoiceRequestedLabel" runat="server" CssClass="DetailsItem">Invoice Requested:</asp:Label></td>
                        <td>
                            <edi:ztextlabel id="InvoiceRequested" runat="server" bindto="AIIRequested"></edi:ztextlabel>
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="RejectReasonLabel" runat="server" CssClass="DetailsItem">Reject Reason:</asp:Label></td>
                        <td>
                            <edi:ztextlabel id="RejectReason" runat="server" bindto="AIIRejectedReason"></edi:ztextlabel>
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                            <asp:Label ID="StatusUDateLabel" runat="server" CssClass="DetailsItem">Status (U*) Date:</asp:Label></td>
                        <td>
                            <edi:zdatetimelabel id="StatusUDate" runat="server" bindto="ElectronicInvoiceStatus" />
                        </td>
                    </tr>
                </table>
            </div>
            <div id="AreaBillOfLadingUpdateStatus" runat="server">
                <table class="ResultsTable">
                    <tr>
                        <td colspan="3">
                            <asp:Label ID="BOLUpdateStatusHeader" runat="server" CssClass="SectionTitle">BOL Update Status: </asp:Label><edi:zcodelookuplabel id="BOLUpdateStatus" runat="server" bindto="BLUStatus" bindtolist="Lookups.BLUStatusList" displaystyle="CodeAndDescription"></edi:zcodelookuplabel>
                            <edi:zdatetimelabel id="BOLUpdateStatusDate" runat="server" bindto="BLUMessageStatusDate" datetimeformat="Long" />
                        </td>
                    </tr>
                </table>
            </div>
        </td>
    </tr>
</table>
