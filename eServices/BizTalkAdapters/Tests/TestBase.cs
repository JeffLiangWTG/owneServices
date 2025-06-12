using System;
using System.IO;
using System.Reflection;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
    public class TestBase
    {
        internal MemoryStream CreatePaddedStream(int length)
        {
            var buffer = new byte[length];
            var rng = new Random();
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)rng.Next(32, 127);
            }
            return new MemoryStream(buffer);
        }

        internal class FakePropertyBag : IPropertyBag
        {
            public void Read(string propName, out object ptrVar, int errorLog)
            {
                ptrVar = this.Config;
            }

            public void Write(string propName, ref object ptrVar)
            {
                throw new NotImplementedException();
            }

            public string Config { get; set; }
        }

        protected string GetEmbeddedResource(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.eHub.BizTalkAdapters.Tests." + name))
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        protected byte[] GetEmbeddedResourceBytes(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.eHub.BizTalkAdapters.Tests." + name))
            using (var mem = new MemoryStream())
            {
                stream.CopyTo(mem);
                return mem.ToArray();
            }
        }

	    internal static void AssertException<T>(string expectedMessage, Action action) where T : Exception
	    {
		    try
		    {
			    action();
		    }
		    catch (Exception ex)
		    {
			    Assert.IsInstanceOfType(ex, typeof(T));
			    Assert.AreEqual(expectedMessage, ex.Message);
			    return;
		    }

		    Assert.Fail("Expected exception '{0}: {1}' not thrown.", typeof(T).Name, expectedMessage);
	    }

	    internal static void AssertInnerException<T>(string expectedMessage, Action action) where T : Exception
	    {
		    try
		    {
			    action();
		    }
		    catch (Exception ex)
		    {
			    Assert.IsInstanceOfType(ex.InnerException, typeof(T));
			    Assert.AreEqual(expectedMessage, ex.InnerException.Message);
			    return;
		    }

		    Assert.Fail("Expected exception '{0}: {1}' not thrown.", typeof(T).Name, expectedMessage);
	    }
    }
}
