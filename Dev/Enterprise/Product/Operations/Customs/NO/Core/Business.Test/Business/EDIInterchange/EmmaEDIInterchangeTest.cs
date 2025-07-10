using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NO.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.xTMessaging.Shared.Constants;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EmmaEDIInterchange))]
sealed class EmmaEDIInterchangeTest : Messaging.Testing.EDIInterchangeTest
{
	public void TestImplementsIxTMessageAttributeProvider()
	{
		AssertCollectionContains(typeof(IxTMessageAttributeProvider), typeof(EmmaEDIInterchange).GetInterfaces());
	}

	public void TestImplementsIEmmaEDIInterchange()
	{
		AssertCollectionContains(typeof(Integration.Customs.NO.IEmmaEDIInterchange), typeof(EmmaEDIInterchange).GetInterfaces());
	}

	public void TestGetMessageAttrDictionary() => CombineAssertions(() =>
	{
		const string username1 = "Arthur Dent";
		const string username2 = "Rose Tyler";
		const string password1 = "sandwich-maker";
		const string password2 = "earthling";
		EmmaEDIInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
		IxTMessageAttributeProvider provider = EmmaEDIInterchange;

		AssertContainsExactElementsInAnyOrder("with default credentials", [
			$"{xTMsgAttributes.FtpClientUser}:",
			$"{xTMsgAttributes.FtpClientPassword}:"
		], provider.GetMessageAttrDictionary().Select(x => $"{x.Key}:{x.Value}"));

		UpdateCredentialsForCompany(username1, password1, GlbCompany.CurrentCompany);
		AssertContainsExactElementsInAnyOrder("with initial credentials", [
			$"{xTMsgAttributes.FtpClientUser}:{username1}",
			$"{xTMsgAttributes.FtpClientPassword}:{password1}"
		], provider.GetMessageAttrDictionary().Select(x => $"{x.Key}:{x.Value}"));

		UpdateCredentialsForCompany(username1, password2, GlbCompany.CurrentCompany);
		AssertContainsExactElementsInAnyOrder("with updated credentials", [
			$"{xTMsgAttributes.FtpClientUser}:{username1}",
			$"{xTMsgAttributes.FtpClientPassword}:{password2}"
		], provider.GetMessageAttrDictionary().Select(x => $"{x.Key}:{x.Value}"));

		var company2 = GetNewCompany("NOX");
		var branch2 = GetNewBranch(company2, "NOY");
		EmmaEDIInterchange.EI_GB = branch2.PK;
		UpdateCredentialsForCompany(username2, password2, company2);
		AssertContainsExactElementsInAnyOrder("with credentials for other company", [
			$"{xTMsgAttributes.FtpClientUser}:{username2}",
			$"{xTMsgAttributes.FtpClientPassword}:{password2}"
		], provider.GetMessageAttrDictionary().Select(x => $"{x.Key}:{x.Value}"));
	});

	static void UpdateCredentialsForCompany(ZString user, ZString pass, GlbCompany company)
	{
		var settings = FTPSettingsEMMADocRegistry.DefaultValues;
		settings.Username = user;
		settings.Password = pass;
		NOCustomsDataRegistry.Instance.FTPSettingsEMMADoc.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, settings);
	}

	GlbCompany GetNewCompany(string companyCode = "NOC")
	{
		var newCompany = Factory.NewWithValidTestData<GlbCompany>();
		newCompany.GC_Code = companyCode;
		newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;
		return newCompany;
	}

	static GlbBranch GetNewBranch(GlbCompany company, string branchCode = "NOB")
	{
		var newBranch = company.Branches.AddNew();
		newBranch.GB_Code = branchCode;
		return newBranch;
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewEDIInterchange();

	EmmaEDIInterchange EmmaEDIInterchange => emmaEDIInterchange ??= GetNewEDIInterchange();
	EmmaEDIInterchange emmaEDIInterchange;
	EmmaEDIInterchange GetNewEDIInterchange() => Factory.New<EmmaEDIInterchange>();
}
