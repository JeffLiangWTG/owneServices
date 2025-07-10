using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class QuotaExhaustedExceptionSuppressorTest : TestCase
	{
#if !WINZOR
		public void TestSuppressAndThrow_Handled()
		{
			using (QuotaExhaustedExceptionSuppressor.SuppressException())
			{
				var topLevelHandler = new TopLevelExceptionHandler();
				var exception = new Win32Exception(1816);
				try
				{
					throw exception;
				}
				catch (Win32Exception e)
				{
					var wasHandled = topLevelHandler.HandleSpecificExceptions(e);
					Assert(wasHandled);
				}
				catch (Exception)
				{
					Fail("Unexpected exception seen");
				}

				AssertRegistered();
			}

			var expectedMessage = "Not enough quota is available to process this command. Try closing and restarting programs and rebooting if problems persist. You can also try increasing the size of your paging file.";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotRegistered();
		}

		public void TestNotSuppressAndThrow_Unhandled()
		{
			var topLevelHandler = new TopLevelExceptionHandler();
			var exception = new Win32Exception(1816);
			try
			{
				throw exception;
			}
			catch (Win32Exception e)
			{
				var wasHandled = topLevelHandler.HandleSpecificExceptions(e);
				Assert(!wasHandled);
			}
			catch (Exception)
			{
				Fail("Unexpected exception seen");
			}

			var expectedMessage = "Not enough quota is available to process this command. Try closing and restarting programs and rebooting if problems persist. You can also try increasing the size of your paging file.";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotRegistered();
		}

		static void AssertRegistered()
		{
			if (ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName) is ArrayList extraExceptionHandlers)
			{
				Assert(extraExceptionHandlers.OfType<QuotaExhaustedExceptionSuppressor>().Any());
			}
		}

		static void AssertNotRegistered()
		{
			if (ObjectFactory.Get(TopLevelExceptionHandler.ExtraExceptionHandlersListName) is ArrayList extraExceptionHandlers)
			{
				Assert(!extraExceptionHandlers.OfType<QuotaExhaustedExceptionSuppressor>().Any());
			}
		}
#endif
	}
}
