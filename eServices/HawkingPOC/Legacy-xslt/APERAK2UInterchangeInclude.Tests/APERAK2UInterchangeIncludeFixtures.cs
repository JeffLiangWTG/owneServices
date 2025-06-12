using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace APERAK2UInterchangeInclude.Tests
{
    public class APERAK2UInterchangeIncludeFixtures
    {
        [Fact]
        public void TestCreateExtensionObject()
        {
            var assembly = Assembly.Load("APERAK2UInterchangeInclude, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            var instance = assembly.CreateInstance("eservices.ehub.products.OceanCarrierMessaging.APERAK2UInterchangeInclude");
            var members = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Assert.Contains(members, x => x.Name == "UserCSharpNamespaceName");
            Assert.Contains(members, x => x.Name == "XslContent");

            var userCSharpNamespace = members.First(x=>x.Name == "UserCSharpNamespaceName").GetValue(instance, null).ToString();
            var xslContent = members.First(x => x.Name == "XslContent").GetValue(instance, null) as Stream;
            Assert.False(string.IsNullOrEmpty(userCSharpNamespace));
            Assert.NotNull(xslContent);
        }
    }
}
