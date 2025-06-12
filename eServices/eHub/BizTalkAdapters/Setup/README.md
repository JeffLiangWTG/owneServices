# Developer Install
1. Build the [eHub repo](https://devops.wisetechglobal.com/wtg/_git/eServices?version=GBmaster&path=%2FeHub) with QGL.
1. Execute script `.\BizTalkAdapters\Setup\Deploy-BizTalkAdapters-Install.ps1` from Powershell as Administrator.
1. Add to `"C:\Program Files (x86)\Microsoft BizTalk Server\BTSNTSvc64.exe.config"`:
```xml
<configSections>
  <sectionGroup name="common">
    <section name="logging" type="Common.Logging.ConfigurationSectionHandler, Common.Logging, Version=3.4.1.0, Culture=neutral, PublicKeyToken=af08829b84f0328e, processorArchitecture=MSIL"/>
  </sectionGroup>
  <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net, Version=2.0.8.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL"/>
  <section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false"/>
</configSections>
<common>
  <logging>
    <factoryAdapter type="Common.Logging.Log4Net.Log4NetLoggerFactoryAdapter, Common.Logging.Log4Net208, Version=3.4.1.0, Culture=neutral, PublicKeyToken=af08829b84f0328e, processorArchitecture=MSIL">
      <arg key="configType" value="INLINE"/>
    </factoryAdapter>
  </logging>
</common>
<log4net>
  <root>
    <appender-ref ref="WindowsEventLog"/>
  </root>
  <appender name="WindowsEventLog" type="log4net.Appender.EventLogAppender">
    <applicationName value="eHub BizTalk"/>
    <layout type="log4net.Layout.PatternLayout"/>
    <threshold value="WARN"/>
  </appender>
</log4net>
<entityFramework>
  <providers>
    <provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, processorArchitecture=MSIL"/>
  </providers>
</entityFramework>
<connectionStrings>
  <add name="eHubTransactionsContext" connectionString="Server=.;Database=eHubTransactions;Trusted_Connection=True;Encrypt=false;" providerName="System.Data.SqlClient"/>
</connectionStrings>
<uri>
  <schemeSettings>
    <add name="http" genericUriParserOptions="DontUnescapePathDotsAndSlashes"/>
    <add name="https" genericUriParserOptions="DontUnescapePathDotsAndSlashes"/>
  </schemeSettings>
</uri>
```
