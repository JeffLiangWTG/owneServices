<%@ page language="C#" autoeventwireup="true" codebehind="SubscribePreference.aspx.cs" inherits="Enterprise.MarketingManager.WebVoting.SubscribePreference" %>

<%@ register assembly="Enterprise.ZArchitecture.Web.GUI" namespace="Enterprise.ZArchitecture.Web.GUI.WebControls" tagprefix="cc1" %>
<%@ register assembly="CargoWise.Types" namespace="CargoWise.Types" tagprefix="cc2" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <title>Subscription Preference</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <style type="text/css">
        @import url('https://fonts.googleapis.com/css?family=Noto+Sans|Open+Sans:400,300');

        #divLogo { 
          margin: 20px; 
          text-align: center; 
        }
        html, body, table, td, th, h1, h2, h3, h4, h5, h6, p, a, span, div, pre, input, select, #LanguageList { 
          font-family: 'Noto Sans', sans-serif !important; 
          font-size: 11pt; 
        }
        #subscription-preferences { 
            margin: 0 auto 20px auto; 
            max-width: 700px; 
            background-color:#fff; 
            overflow: hidden;
        }
        #subscription-preferences h2 { 
            background-color: #00a8e1;
            color: #fff;
            font-weight: bold;
            padding: 15px;
            margin: 0;
        }
        #subscription-preferences h2, #subscription-preferences h2 span { 
            font-size: 16pt;
        }
        #subscription-preferences p {
            margin: 30px 15px 15px 15px;
        }
        .subscription-summary {
            color: #a9a9a9;
            padding-top: 2px;
        }
        #subscription-preferences table { 
            width: 100%;
            border-spacing: 0;
            border-collapse: collapse;
            margin-top: 25px;
        }
        #subscription-preferences table tr:nth-child(2n+1) {
            background-color: #f5f5f5;
        }
        #subscription-preferences table td { 
            padding: 10px 15px;
            margin: 0;
        }
        #subscription-preferences table tr td:nth-child(2) { 
            text-align: center;
            width: 40px;
        }
        @media only screen and (min-width: 800px) {
            #subscription-preferences table tr td:nth-child(2) { 
                width: 105px;
            }
        }
        #subscription-preferences .button {
            display: block;
            text-transform: uppercase;
            padding: 12px 21px;
            margin: 20px auto 10px auto;
            text-align: center;
            color: #fff;
            border: none;
            border-radius: 2px;
            background-color: #00a8e1;
            box-shadow: 0 0 0 0 #8fd400 inset;
            -webkit-transition: .4s ease-in-out;
            transition: .4s ease-in-out;
            font-size: 11pt; 
            font-weight: bold; 
            cursor: pointer;
        }
        #subscription-preferences input[type=submit]:hover { 
            box-shadow: 0 48px 0 0 #8fd400 inset;
        }
        #SubscriptionResultContainer {
            margin: 0 auto 30px auto; 
            padding: 20px 0; 
            max-width: 500px;
        }
        #big-tick {
            float: left;
            margin: -40px 15px 0 0;
            font-size: 60pt;
        }
    </style>
    
</head>
<body>
    <section id="section-subscription">
        <form id="form1" runat="server">
            <div id="subscription-preferences">
              <div id="SubscriptionListContainer" runat="server">
                <h2>
                    <cc1:ztextlabel id="SubscriptionPreferenceLabel" runat="server" />
                </h2>
                <p><cc1:ztextlabelnoencode id="SubscriptionPreferenceDescLabel" runat="server" /></p>
                <div id="SubscriptionPreferences">
                      <table>
                        <tbody>
                        <cc1:zrepeater id="SubscriptionPreferencesRepeater" runat="server" bindto="SubscriptionList">
                          <HeaderTemplate>
                          </HeaderTemplate>
                          <ItemTemplate>
                            <tr>
                              <td>
                                  <strong>
                                <asp:Label id="lblSubscription" runat="server" Text='<%# Eval("PublishedDescription") %>'></asp:Label> 
                                  </strong>
                                  <br />
                                <div class="subscription-summary">
                                  <asp:Label id="lblSubscritionSummary" runat="server" Text='<%# Eval("PublishedSummary") %>'></asp:Label>
                                </div>
                              </td>
                              <td>
                                <cc1:ZSelectionCheckBox id="cbSubscription" Enabled="true" runat="server" Checked='<%# GetChechBoxStatus(Eval("IsSubscribed")) %>'></cc1:ZSelectionCheckBox>
                              </td>
                            </tr>

                          </ItemTemplate>
                          <FooterTemplate>
                          </FooterTemplate>
                        </cc1:zrepeater>
                        </tbody>
                      </table>
                </div>
                <cc1:zbutton id="SubmitButton" runat="server" text="SUBMIT" onclick="SubmitButton_Click" class="button"></cc1:zbutton>
              </div>
            </div>
            <div id="SubscriptionResultContainer" runat="server">
              <p>
                 <img id="ResultImage" src="UnsubscribeImages/success.png" alt="" runat="server" /><cc1:ztextlabel id="ResultMessageLabel" runat="server" bindto="ResultMessage" />
              </p>
            </div>

          </form>

    </section>
</body>
</html>
