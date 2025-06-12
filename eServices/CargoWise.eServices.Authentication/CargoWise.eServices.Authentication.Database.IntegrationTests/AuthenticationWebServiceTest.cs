using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using CargoWise.eServices.Authentication.WebService;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.IntegrationTests
{

    [TestFixture]
    public class AuthenticationWebServiceTest
	{
	    [TestCase("DAU", "TST", "password")]
        public void TestLoggingWithUnknownRequesterIP(string enterpriseCode, string serverCode, string password)
		{
		    var appender = new MemoryAppender();
		    BasicConfigurator.Configure(appender);
            var controller = new AuthenticationController();
		    var param = new String[] { enterpriseCode, serverCode, password };
            var result = controller.ValidateCodeAndPassword(param);

		    var messagesList = appender.GetEvents().Where(x => x.Level == Level.Debug).ToList();
		    Assert.That(messagesList.Count, Is.EqualTo(1));

            var expectedLogEntry = $"[Code: { param[0]}-{ param[1]}] [ValidateCodeAndPassword: Start with request from IP Address/es: <UNKNOWN>]";
		    Assert.That(messagesList[0].RenderedMessage.Contains(expectedLogEntry),
		            $"The rendered message:\r\n{messagesList[0].RenderedMessage}\r\ndoes not contain the expected message:\r\n{expectedLogEntry}");
        }

	    [TestCase("DAU", "TST", "password")]
	    public void TestLoggingWithRemoteRequesterIP(string enterpriseCode, string serverCode, string password)
	    {
	        var appender = new MemoryAppender();
	        BasicConfigurator.Configure(appender);

            var testRequest = new HttpRequest("", "http://tempuri.org", "");
	        AddServerVariable(ref testRequest, "REMOTE_ADDR", "192.1.1.1");
	        HttpContext.Current = new HttpContext(
	            testRequest,
	            new HttpResponse(new StringWriter())
	        );

            var controller = new AuthenticationController();
            var param = new String[] { enterpriseCode, serverCode, password };
	        controller.ValidateCodeAndPassword(param);
            HttpContext.Current = null;

	        var messagesList = appender.GetEvents().Where(x => x.Level == Level.Debug).ToList();
	        Assert.That(messagesList.Count, Is.EqualTo(1));

            var expectedLogEntry = $"[Code: {param[0]}-{param[1]}] [ValidateCodeAndPassword: Start with request from IP Address/es: 192.1.1.1]";
	        Assert.That(messagesList[0].RenderedMessage.Contains(expectedLogEntry),
	                $"The rendered message:\r\n{messagesList[0].RenderedMessage}\r\ndoes not contain the expected partial message:\r\n{expectedLogEntry}");
	    }

        private static void AddServerVariable(ref HttpRequest request, string key, string value)
	    {
	        const BindingFlags tempFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	        MethodInfo addStatic = null;
	        MethodInfo makeReadOnly = null;
	        MethodInfo makeReadWrite = null;

	        var type = request.ServerVariables.GetType();
	        var methods = type.GetMethods(tempFlags);
	        foreach (var method in methods)
	        {
	            switch (method.Name)
	            {
	                case "MakeReadWrite":
	                    makeReadWrite = method;
	                    break;
	                case "MakeReadOnly":
	                    makeReadOnly = method;
	                    break;
	                case "AddStatic":
	                    addStatic = method;
	                    break;
	            }
	        }
	        makeReadWrite.Invoke(request.ServerVariables, null);
	        string[] values = { key, value };
	        addStatic.Invoke(request.ServerVariables, values);
	        makeReadOnly.Invoke(request.ServerVariables, null);
	    }

    }

}
