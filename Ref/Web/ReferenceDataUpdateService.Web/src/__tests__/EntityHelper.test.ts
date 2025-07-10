import { Mock } from "typemoq";
import { RefCusCodeListUserView } from "../models/RefCusCodeListUserView";
import { EntityHelper } from "../EntityHelper";

describe("EntityHelper", () => {
    it("getPk", () => {
        let code = Mock.ofType<RefCusCodeListUserView>();
        code.setup(x => x.ZZD_PK).returns(() => "1");
        expect(EntityHelper.getPK(code.object)).toEqual("1");
    });

    it("isSystem", () => {
        let code = Mock.ofType<RefCusCodeListUserView>();
        code.setup(x => x.ZZD_IsSystem).returns(() => true);
        expect(EntityHelper.isSystem(code.object)).toBeTruthy();
    });

    it("isEditable", () => {
        let code = Mock.ofType<RefCusCodeListUserView>();
        code.setup(x => x.ZZD_IsEditable).returns(() => true);
        expect(EntityHelper.isEditable(code.object)).toBeTruthy();
    });
});