#if NETFRAMEWORK
using System;
using NUnit.Framework;
namespace Enterprise.Services.Scim.Business.Test
{
	class IPSafelistHelperTests
	{
		[Test]
		public void TestValidateURL_ThrowsWhenInvalidURL()
		{
			var helper = new IPSafelistHelper();
			System.Configuration.ConfigurationManager.AppSettings["WhitelistURL"] = "https://blackhaturl.com";
#pragma warning disable IDE0001
			var ex = Assert.ThrowsAsync<System.Security.SecurityException>(() => helper.GetSafelistedIps());
			Assert.That(ex?.ToString(), Contains.Substring("URL domain not allowed"));
#pragma warning restore IDE0001
		}
		
		[Test]
		public void TestExtractJsonUrlSafely_RejectsHackedUrls()
		{
			var helper = new IPSafelistHelper();
			var hackedHtml = @"
		<html>
			<body>
				<p>Some content</p>
				<a href=""https://malicious-site.com/payload.json"">Download JSON</a>
				<a href=""https://another-bad-domain.com/file.json"">Another link</a>
			</body>
		</html>";
			
			Assert.Throws<InvalidOperationException>(() => helper.ExtractJsonUrlSafely(hackedHtml));
		}
		
		[Test]
		public void TestExtractJsonUrlSafely_RejectsDomainSpoofing()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a href=""https://download.microsoft.com.evil.com/fake.json"">Link</a></body></html>";
			
			Assert.Throws<InvalidOperationException>(() => helper.ExtractJsonUrlSafely(html));
		}
		
		[Test]
		public void TestExtractJsonUrlSafely_RejectsJavaScriptURL()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a href=""javascript:alert('hacked')"">JavaScript Link</a></body></html>";
			
			Assert.Throws<InvalidOperationException>(() => helper.ExtractJsonUrlSafely(html));
		}
		
		[Test]
		public void TestExtractJsonUrlSafely_RejectsWrongFileExtension()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a href=""https://download.microsoft.com/malicious.exe"">Link</a></body></html>";
			
			Assert.Throws<InvalidOperationException>(() => helper.ExtractJsonUrlSafely(html));
		}
		
		[Test]
		public void TestExtractJsonUrlSafely_RejectsNonHttpsProtocol()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a href=""http://download.microsoft.com/file.json"">Link</a></body></html>";
			
			Assert.Throws<InvalidOperationException>(() => helper.ExtractJsonUrlSafely(html));
		}
		
		[Test]
		public void TestExtractJsonUrlSafely_RejectsUrlEncoding()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a href=""https://download.microsoft.com/%2e%2e/%2e%2e/etc/passwd"">Link</a></body></html>";
			
			Assert.Throws<InvalidOperationException>(() => helper.ExtractJsonUrlSafely(html));
		}
		
		[Test]
		public void TestExtractJsonUrlSafely_HandlesEmptyHtml()
		{
			var helper = new IPSafelistHelper();
			
			Assert.Throws<ArgumentException>(() => helper.ExtractJsonUrlSafely(string.Empty));
		}
		
		[Test]
		public void TestExtractJsonUrlSafely_HandlesNullHtml()
		{
			var helper = new IPSafelistHelper();
			
			Assert.Throws<ArgumentException>(() => helper.ExtractJsonUrlSafely(null));
		}
		
		[Test]
		public void TestExtractJsonUrlSafely_ExtractsValidMicrosoftJsonUrl()
		{
			var helper = new IPSafelistHelper();
			var validHtml = @"
				<html>
					<body>
						<h1>Download Service Tags</h1>
						<p>Please use the link below to download the latest service tags.</p>
						<a href=""https://download.microsoft.com/download/8/3/f/83f2aa92-5c1f-4e6a-b9f2-12b12c8d8238/ServiceTags_Public_20250228.json"" class=""download-button"">Download JSON</a>
						<p>Additional information can be found on our documentation page.</p>
					</body>
				</html>";
			var result = helper.ExtractJsonUrlSafely(validHtml);
	
			Assert.That(result, Is.EqualTo("https://download.microsoft.com/download/8/3/f/83f2aa92-5c1f-4e6a-b9f2-12b12c8d8238/ServiceTags_Public_20250228.json"));
		}
		
		[Test]
		public void TestExtractJsonUrlSafely_AttributesBeforeHref()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a class=""link"" id=""x"" href=""https://download.microsoft.com/path/sneaky.json"">Link</a></body></html>";
			
			var result = helper.ExtractJsonUrlSafely(html);
			Assert.That(result, Is.EqualTo("https://download.microsoft.com/path/sneaky.json"));
		}

		[Test]
		public void TestExtractJsonUrlSafely_ExtraSpacesAroundEquals()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a href = ""https://download.microsoft.com/path/file.json"">Link</a></body></html>";
			
			var result = helper.ExtractJsonUrlSafely(html);
			Assert.That(result, Is.EqualTo("https://download.microsoft.com/path/file.json"));
		}

		[Test]
		public void TestExtractJsonUrlSafely_ExtraSpacesAroundAttributeName()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a   href=""https://download.microsoft.com/path/file.json""  >Link</a></body></html>";
			
			var result = helper.ExtractJsonUrlSafely(html);
			Assert.That(result, Is.EqualTo("https://download.microsoft.com/path/file.json"));
		}

		[Test]
		public void TestExtractJsonUrlSafely_SingleQuotes()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a href='https://download.microsoft.com/path/file.json'>Link</a></body></html>";
			
			var result = helper.ExtractJsonUrlSafely(html);
			Assert.That(result, Is.EqualTo("https://download.microsoft.com/path/file.json"));
		}

		[Test]
		public void TestExtractJsonUrlSafely_MixedQuotesInTag()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a class='some-class' id=""unique"" href=""https://download.microsoft.com/path/file.json"">Link</a></body></html>";
			
			var result = helper.ExtractJsonUrlSafely(html);
			Assert.That(result, Is.EqualTo("https://download.microsoft.com/path/file.json"));
		}

		[Test]
		public void TestExtractJsonUrlSafely_LineBreaksInTag()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a 
class=""link"" 
id=""x"" 
href=""https://download.microsoft.com/path/file.json""
>Link</a></body></html>";
			
			var result = helper.ExtractJsonUrlSafely(html);
			Assert.That(result, Is.EqualTo("https://download.microsoft.com/path/file.json"));
		}

		[Test]
		public void TestExtractJsonUrlSafely_TabsInTag()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a	class=""link""	id=""x""	href=""https://download.microsoft.com/path/file.json"">Link</a></body></html>";
			
			var result = helper.ExtractJsonUrlSafely(html);
			Assert.That(result, Is.EqualTo("https://download.microsoft.com/path/file.json"));
		}

		[Test]
		public void TestExtractJsonUrlSafely_AttributesAfterHref()
		{
			var helper = new IPSafelistHelper();
			var html = @"<html><body><a href=""https://download.microsoft.com/path/file.json"" target=""_blank"" rel=""noopener"">Link</a></body></html>";
			
			var result = helper.ExtractJsonUrlSafely(html);
			Assert.That(result, Is.EqualTo("https://download.microsoft.com/path/file.json"));
		}

		[Test]
		public void TestExtractJsonUrlSafely_MultipleHrefsInPage()
		{
			var helper = new IPSafelistHelper();
			var html = @"
<html>
	<body>
		<a href=""https://example.com/not-json.html"">Not JSON</a>
		<a href=""https://bad-domain.com/file.json"">Wrong domain</a>
		<a href=""https://download.microsoft.com/path/file.json"">Correct link</a>
		<a href=""https://download.microsoft.com/path/another.json"">Another correct link</a>
	</body>
</html>";
			
			var result = helper.ExtractJsonUrlSafely(html);
			Assert.That(result, Is.EqualTo("https://download.microsoft.com/path/file.json"));
		}
	}
}
#endif
