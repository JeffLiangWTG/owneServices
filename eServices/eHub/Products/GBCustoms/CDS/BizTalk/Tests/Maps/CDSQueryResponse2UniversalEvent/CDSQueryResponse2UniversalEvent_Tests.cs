using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.GBCustoms.CDS.BT.Tests.Maps.CDSQueryResponse2UniversalEvent
{
    [TestClass]
    public class CDSQueryResponse2UniversalEvent_Tests
    {
        const string filePath = "Maps.CDSQueryResponse2UniversalEvent.TestFiles.";

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void Test_CDSQueryResponse2UniversalEvent()
        {
            var input = filePath + "Test1 - Success Input.xml";
            var expectedOutput = filePath + "Test1 - Success Output.xml";
            AssertMapping(input, expectedOutput, "CDSQueryResponse2UniversalEvent");
        }

        static void AssertMapping(string input, string expectedOutput, string OverrideFileName)
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

            mockDateMapper.Expect(x => x.CurrentDateTimeUTC("s")).Return("2018-09-17T14:21:01").Repeat.Once();
            mockContextAccessor.Stub(_ => _.GetContextProperty("jobNumber", "")).Return("JOB1234");
            mockContextAccessor.Stub(_ => _.GetContextProperty("eHubTrackingID", "")).Return("093b255c-8a20-44bc-bfb1-596bfa9eae13");
            mockContextAccessor.Stub(_ => _.GetContextProperty("OriginalMessage", "")).Return("PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4NCjxwOkRlY2xhcmF0aW9uU3RhdHVzUmVzcG9uc2UNCiAgeG1sbnM6cD0iaHR0cDovL2dvdi51ay9jdXN0b21zL2RlY2xhcmF0aW9uSW5mb3JtYXRpb25SZXRyaWV2YWwvc3RhdHVzL3YyIg0KICB4bWxuczpwMT0idXJuOndjbzpkYXRhbW9kZWw6V0NPOlJlc3BvbnNlX0RTOkRNUzoyIg0KICB4bWxuczpwMj0idXJuOndjbzpkYXRhbW9kZWw6V0NPOkRFQy1ETVM6MiINCiAgeG1sbnM6cDM9InVybjp3Y286ZGF0YW1vZGVsOldDTzpEZWNsYXJhdGlvbl9EUzpETVM6MiINCiAgeG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSINCnhzaTpzY2hlbWFMb2NhdGlvbj0iaHR0cDovL2dvdi51ay9jdXN0b21zL2RlY2xhcmF0aW9uSW5mb3JtYXRpb25SZXRyaWV2YWwvc3RhdHVzL3YyIC4uL3NjaGVtYXMvd2NvL2RlY2xhcmF0aW9uL0RlY2xhcmF0aW9uSW5mb3JtYXRpb25SZXRyaWV2YWxTdGF0dXNSZXNwb25zZS54c2QgIj4NCgk8cDpEZWNsYXJhdGlvblN0YXR1c0RldGFpbHM+DQoJCTxwOkRlY2xhcmF0aW9uPg0KCQkJPHA6QWNjZXB0YW5jZURhdGVUaW1lPg0KCQkJCTxwMTpEYXRlVGltZVN0cmluZyBmb3JtYXRDb2RlPSIzMDQiPjIwMTkwNzAyMTEwNzU3WjwvcDE6RGF0ZVRpbWVTdHJpbmc+DQoJCQk8L3A6QWNjZXB0YW5jZURhdGVUaW1lPg0KCQkJPHA6SUQ+MTlHQkw0NTkyTkNPSTIxTlI5PC9wOklEPg0KCQkJPHA6VmVyc2lvbklEPjE8L3A6VmVyc2lvbklEPg0KCQkJPHA6UmVjZWl2ZWREYXRlVGltZT4NCgkJCQk8cDpEYXRlVGltZVN0cmluZyBmb3JtYXRDb2RlPSIzMDQiPjIwMTkwNzAyMTEwNzU3WjwvcDpEYXRlVGltZVN0cmluZz4NCgkJCTwvcDpSZWNlaXZlZERhdGVUaW1lPg0KCQkJPHA6R29vZHNSZWxlYXNlZERhdGVUaW1lPg0KCQkJCTxwOkRhdGVUaW1lU3RyaW5nIGZvcm1hdENvZGU9IjMwNCI+MjAxOTA3MDIxMTA3NTdaPC9wOkRhdGVUaW1lU3RyaW5nPg0KCQkJPC9wOkdvb2RzUmVsZWFzZWREYXRlVGltZT4NCgkJCTxwOlJPRT42PC9wOlJPRT4NCgkJCTxwOklDUz4xNTwvcDpJQ1M+DQoJCQk8cDpJUkM+MDAwPC9wOklSQz4NCgkJPC9wOkRlY2xhcmF0aW9uPg0KCQk8cDI6RGVjbGFyYXRpb24+DQoJCQk8cDI6RnVuY3Rpb25Db2RlPjk8L3AyOkZ1bmN0aW9uQ29kZT4NCgkJCTxwMjpUeXBlQ29kZT5JTVo8L3AyOlR5cGVDb2RlPg0KCQkJPHAyOkdvb2RzSXRlbVF1YW50aXR5PjEwMDwvcDI6R29vZHNJdGVtUXVhbnRpdHk+DQoJCQk8cDI6VG90YWxQYWNrYWdlUXVhbnRpdHk+MTA8L3AyOlRvdGFsUGFja2FnZVF1YW50aXR5Pg0KCQkJPHAyOlN1Ym1pdHRlcj4NCgkJCQk8cDI6SUQ+R0IxMjM0NTY3ODkwMTIwMDA8L3AyOklEPg0KCQkJPC9wMjpTdWJtaXR0ZXI+DQoJCQk8cDI6R29vZHNTaGlwbWVudD4NCgkJCQk8cDI6UHJldmlvdXNEb2N1bWVudD4NCgkJCQkJPHAyOklEPjE4R0JBS1o4MUVRSjJGR1ZSPC9wMjpJRD4NCgkJCQkJPHAyOlR5cGVDb2RlPkRDUjwvcDI6VHlwZUNvZGU+DQoJCQkJPC9wMjpQcmV2aW91c0RvY3VtZW50Pg0KCQkJCTxwMjpQcmV2aW91c0RvY3VtZW50Pg0KCQkJCQk8cDI6SUQ+MThHQkFLWjgxRVFKMkZHVkE8L3AyOklEPg0KCQkJCQk8cDI6VHlwZUNvZGU+TUNSPC9wMjpUeXBlQ29kZT4NCgkJCQk8L3AyOlByZXZpb3VzRG9jdW1lbnQ+DQoJCQkJPHAyOlVDUj4NCgkJCQkJPHAyOlRyYWRlckFzc2lnbmVkUmVmZXJlbmNlSUQ+MjBHQkFLWjgxRVFKMldYWVo8L3AyOlRyYWRlckFzc2lnbmVkUmVmZXJlbmNlSUQ+DQoJCQkJPC9wMjpVQ1I+DQoJCQk8L3AyOkdvb2RzU2hpcG1lbnQ+DQoJCTwvcDI6RGVjbGFyYXRpb24+DQoJPC9wOkRlY2xhcmF0aW9uU3RhdHVzRGV0YWlscz4NCjwvcDpEZWNsYXJhdGlvblN0YXR1c1Jlc3BvbnNlPg0K");

            var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper },
			};

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<BT.Transforms.CDSQueryResponse2UniversalEvent.CDSQueryResponse2UniversalEvent>(input, expectedOutput);
        }
    }
}
