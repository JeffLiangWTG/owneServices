import { Mock, It } from "typemoq";
import { IDataSetChangeHistory } from "../models/IDataSetChangeHistory";
import { IEntityManager, ServiceType } from "../EntityManager";
import { shallow } from "enzyme";
import { MainForm } from "../MainForm";
import React from "react";

describe("<MainForm />", () => {
    it("render", async () => {
        let history = Mock.ofType<IDataSetChangeHistory>();
        
        var entityManager = Mock.ofType<IEntityManager>();
        entityManager.setup(x => x.getAsync("DataSetChangeHistory", [ServiceType.Safe], It.isAny(), false, It.isAny())).returns(() => Promise.resolve([history.object]));

        let wrapper = shallow<MainForm>(<MainForm id="123" parentCode="ZZD" entityManager={entityManager.object} render={() => <div>Hello</div> } />);
        await wrapper.instance().componentDidMount();

        expect(wrapper.find("nav > div > a")).toHaveLength(1);
    });
});