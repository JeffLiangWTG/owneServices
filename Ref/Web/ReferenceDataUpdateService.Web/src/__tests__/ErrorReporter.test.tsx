import { Mock } from "typemoq";
import { ErrorReporter } from "../ErrorReporter";
import axios from "axios";

it("emits error reports", () => {
	let mockFetchPromise = Promise.resolve({});
	var globalRef: any = global;
	globalRef.fetch = jest.fn().mockImplementation(() => mockFetchPromise);

	const errorReporter = new ErrorReporter(window);
	errorReporter.init();

  errorReporter.reportError = jest.fn();
  errorReporter.reportPromiseRejection = jest.fn();

  const spyReportError = jest.spyOn(errorReporter, "reportError");
  const spyReportPromiseRejection = jest.spyOn(errorReporter, "reportPromiseRejection");

	window.dispatchEvent(new ErrorEvent("error"));
	expect(spyReportError).toBeCalledTimes(1);
	expect(spyReportPromiseRejection).toBeCalledTimes(0);

	window.dispatchEvent(new ErrorEvent("unhandledrejection"));
	expect(spyReportPromiseRejection).toBeCalledTimes(1);
});

it("report error no more than 3 times", () => {
	jest.mock("axios");
	const mockedAxios = axios as jest.Mocked<typeof axios>;
	mockedAxios.post = jest.fn();

	let errorEvent = Mock.ofType<ErrorEvent>();
	errorEvent.setup((x) => x.error).returns(() => new Error("Error"));
	let rejectionEvent = Mock.ofType<PromiseRejectionEvent>();
	rejectionEvent
		.setup((x) => x.reason)
		.returns(() => new Error("Unhandled Rejection Error"));

	let errorReporter = new ErrorReporter(window);
	errorReporter.reportError(errorEvent.object);
	errorReporter.reportError(errorEvent.object);
	errorReporter.reportError(errorEvent.object);
	errorReporter.reportError(errorEvent.object);
	expect(mockedAxios.post).toBeCalledTimes(3);

	mockedAxios.post.mockClear();
	errorReporter.reportPromiseRejection(rejectionEvent.object);
	errorReporter.reportPromiseRejection(rejectionEvent.object);
	errorReporter.reportPromiseRejection(rejectionEvent.object);
	errorReporter.reportPromiseRejection(rejectionEvent.object);
	expect(mockedAxios.post).toBeCalledTimes(3);
});
