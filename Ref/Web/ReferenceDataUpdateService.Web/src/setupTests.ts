import Enzyme from 'enzyme'
import Adapter from 'enzyme-adapter-react-16'
import $ from "jquery";
import moment from "moment";

declare var global : any;

Enzyme.configure({
  adapter: new Adapter(),
});

global.$ = global.jQuery = $;
global.moment = moment;

// https://github.com/AzureAD/microsoft-authentication-library-for-js/issues/6487 provides another solution to solve test failures.
jest.mock("@azure/msal-react", () => ({}));

global.BroadcastChannel = class {
  onmessage: ((event: any) => void) | null = null;
  postMessage = jest.fn();
  close = jest.fn();
  addEventListener = jest.fn();
  removeEventListener = jest.fn();
};
