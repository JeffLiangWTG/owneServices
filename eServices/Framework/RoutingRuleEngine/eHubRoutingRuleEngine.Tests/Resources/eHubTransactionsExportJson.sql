SELECT '{' + STRING_AGG(TRIM('{}' FROM J),',') + '}' FROM (VALUES 
((SELECT * FROM [eHubRoutingRule] FOR JSON AUTO, ROOT('eHubRoutingRules'))),
((SELECT * FROM [eHubRoutingRuleFact] FOR JSON AUTO, ROOT('eHubRoutingRuleFacts'))),
((SELECT * FROM [eHubServiceProvider] FOR JSON AUTO, ROOT('eHubServiceProviders'))),
((SELECT * FROM [eHubServiceProviderRequiredRegistration] FOR JSON AUTO, ROOT('eHubServiceProviderRequiredRegistrations'))),
((SELECT * FROM [eHubRegistrationType] FOR JSON AUTO, ROOT('eHubRegistrationTypes'))),
((SELECT * FROM [eHubClient] FOR JSON AUTO, ROOT('eHubClients'))),
((SELECT * FROM [eHubClientRegistration] FOR JSON AUTO, ROOT('eHubClientRegistrations')))
) T([J])
