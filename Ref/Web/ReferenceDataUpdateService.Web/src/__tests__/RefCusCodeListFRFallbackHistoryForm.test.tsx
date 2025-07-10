import { shallow } from "enzyme";
import { Mock, It, IMock } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import React from "react";
import { RefCusCodeListFRFallbackHistoryForm } from "../RefCusCodeListFRFallbackHistoryForm";
import { RefCusCodeListUserView } from "../models/RefCusCodeListUserView";
import { RefCusCodeListAttributeUserView } from "../models/RefCusCodeListAttributeUserView";

describe("<RefCusCodeListFRFallbackRevokeForm />", () => {
	let code1: IMock<RefCusCodeListUserView>;
	let code2: IMock<RefCusCodeListUserView>;

	let attrib1: IMock<RefCusCodeListAttributeUserView>;
	let attrib2: IMock<RefCusCodeListAttributeUserView>;
	let attrib3: IMock<RefCusCodeListAttributeUserView>;
	let attrib4: IMock<RefCusCodeListAttributeUserView>;

	let entityManager: IMock<IEntityManager>;

	beforeEach(() => {
		code1 = Mock.ofType<RefCusCodeListUserView>();
		code1.setup(x => x.ZZD_PK).returns(() => "DAB4A9D0-9F29-11EA-8457-17CA8B768246");
		code1.setup(x => x.ZZD_Code).returns(() => "1590481269997,DELTAT");
		code1.setup(x => x.ZZD_Description).returns(() => "BLABLAT");
		code1.setup(x => x.ZZD_StartDate).returns(() => "2020-05-26T08:21:00Z");
		code1.setup(x => x.ZZD_EndDate).returns(() => "2079-06-06T23:59:00Z");
		code1.setup(x => x.ZZD_CodeType).returns(() => "FBK");
		code1.setup(x => x.ZZD_CountryOrGrouping).returns(() => "FR");

		attrib1 = Mock.ofType<RefCusCodeListAttributeUserView>();
		attrib1.setup(x => x.ZZE_PK).returns(() => "DAB4D0E0-9F29-11EA-8457-17CA8B768246");
		attrib1.setup(x => x.ZZE_ZZD_CodeList).returns(() => "DAB4A9D0-9F29-11EA-8457-17CA8B768246");
		attrib1.setup(x => x.ZZE_ZXE_NKName).returns(() => "CommentInvoke");
		attrib1.setup(x => x.ZZE_Value).returns(() => "BLABLAT");

		attrib2 = Mock.ofType<RefCusCodeListAttributeUserView>();
		attrib2.setup(x => x.ZZE_PK).returns(() => "DAB4D0E0-9F29-11EA-8457-17CA8B768246");
		attrib2.setup(x => x.ZZE_ZZD_CodeList).returns(() => "DAB4A9D0-9F29-11EA-8457-17CA8B768246");
		attrib2.setup(x => x.ZZE_ZXE_NKName).returns(() => "CommentRevoke");
		attrib2.setup(x => x.ZZE_Value).returns(() => "");

		code2 = Mock.ofType<RefCusCodeListUserView>();
		code2.setup(x => x.ZZD_PK).returns(() => "72881600-9F2C-11EA-8457-17CA8B768246");
		code2.setup(x => x.ZZD_Code).returns(() => "1590482383712,DELTAG");
		code2.setup(x => x.ZZD_Description).returns(() => "BLABLAG");
		code2.setup(x => x.ZZD_StartDate).returns(() => "2020-05-27T08:39:00Z");
		code2.setup(x => x.ZZD_EndDate).returns(() => "2079-06-06T23:59:00Z");
		code2.setup(x => x.ZZD_CodeType).returns(() => "FBK");
		code2.setup(x => x.ZZD_CountryOrGrouping).returns(() => "FR");

		attrib3 = Mock.ofType<RefCusCodeListAttributeUserView>();
		attrib3.setup(x => x.ZZE_PK).returns(() => "72881601-9F2C-11EA-8457-17CA8B768246");
		attrib3.setup(x => x.ZZE_ZZD_CodeList).returns(() => "72881600-9F2C-11EA-8457-17CA8B768246");
		attrib3.setup(x => x.ZZE_ZXE_NKName).returns(() => "CommentInvoke");
		attrib3.setup(x => x.ZZE_Value).returns(() => "BLABLAG");

		attrib4 = Mock.ofType<RefCusCodeListAttributeUserView>();
		attrib4.setup(x => x.ZZE_PK).returns(() => "72881601-9F2C-11EA-8457-17CA8B768246");
		attrib4.setup(x => x.ZZE_ZZD_CodeList).returns(() => "72881600-9F2C-11EA-8457-17CA8B768246");
		attrib4.setup(x => x.ZZE_ZXE_NKName).returns(() => "CommentRevoke");
		attrib4.setup(x => x.ZZE_Value).returns(() => "");

		entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync("RefCusCodeListUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([code1.object, code2.object]));
		entityManager.setup(x => x.getAsync("RefCusCodeListAttributeUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([attrib1.object, attrib2.object, attrib3.object, attrib4.object]));
	});

	it("render data", async () => {
		var wrapper = shallow<RefCusCodeListFRFallbackHistoryForm>(<RefCusCodeListFRFallbackHistoryForm entityManager={entityManager.object} />);
		await wrapper.instance().loadListRefCusCode();
		expect(wrapper.find("tbody > tr")).toHaveLength(2);
		expect(wrapper.find("tbody > tr").at(0).html()).toContain("<tr><td>1</td><td>DELTAT</td>");
	});
});
