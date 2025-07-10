import { shallow } from "enzyme";
import React from "react";
import { DateTimeRangeInput } from "../DateTimeRangeInput";
import { Filter, FilterOps } from "../Filter";
import { act } from "react-dom/test-utils";
import { setImmediate } from "timers";

declare var global: any;

describe("DateTimeInput", () => {
	it("render", async () => {
		const div = global.document.createElement("div");
		global.document.body.appendChild(div);

		const filter = new Filter(
			"value",
			FilterOps.DateRange,
			"" as any,
			"datetime"
		);
		const wrapper = shallow(
			<DateTimeRangeInput
				format="yyyy-MM-DD"
				filter={filter}
				onValueChange={jest.fn}
			/>
		);

		await act(() => new Promise(setImmediate));
		wrapper.update();

		expect(wrapper.find("input[type='text']").length).toBe(2);
		global.document.body.removeChild(div);
	});
});
