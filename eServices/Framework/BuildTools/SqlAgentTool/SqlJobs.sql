SELECT CAST(CONCAT('<SqlJobs>',
(SELECT [name], [enabled], [description], [start_step_id], [notify_level_eventlog], [notify_level_email]
		,(SELECT [syscategories].[name] FROM [msdb].[dbo].[syscategories] WHERE [syscategories].[category_id] = [sysjobs].[category_id]) [category_name]
		,SUSER_SNAME([owner_sid]) [owner_login_name]
		,(SELECT [name] FROM [msdb].[dbo].[sysoperators] WHERE [id] = [notify_email_operator_id]) [notify_email_operator_name]
		,(SELECT [step_id], [step_name], [subsystem], [command], [flags], [additional_parameters], [cmdexec_success_code], [on_success_action], [on_success_step_id], [on_fail_action], [on_fail_step_id], [server], [database_name], [database_user_name], [retry_attempts], [retry_interval], [os_run_priority], [output_file_name]
			,(SELECT [name] FROM [msdb].[dbo].[sysproxies] WHERE [sysproxies].[proxy_id] = [sysjobsteps].[proxy_id]) [proxy_name]
			FROM [msdb].[dbo].[sysjobsteps] 
			WHERE [sysjobsteps].[job_id] = [sysjobs].[job_id] 
			ORDER BY [sysjobsteps].[step_id]
			FOR XML PATH('sysjobstep'), ELEMENTS, TYPE
		)
		,(SELECT [name], [enabled], [freq_type], [freq_interval], [freq_subday_type], [freq_subday_interval], [freq_relative_interval], [freq_recurrence_factor], [active_start_date], [active_end_date], [active_start_time], [active_end_time], SUSER_SNAME([owner_sid]) [owner_login_name]
			FROM [msdb].[dbo].[sysjobschedules]
			LEFT JOIN [msdb].[dbo].[sysschedules] ON [sysschedules].[schedule_id] = [sysjobschedules].[schedule_id]
			WHERE [sysjobschedules].[job_id] = [sysjobs].[job_id] 
			ORDER BY [sysschedules].[schedule_id]
			FOR XML PATH('sysjobschedule'), ELEMENTS, TYPE
		)
		,(SELECT [id], [name], [event_source], [event_category_id], [event_id], [message_id], [severity], [enabled], [delay_between_responses], [notification_message], [include_event_description], [database_name], [event_description_keyword], [has_notification], [flags], [performance_condition], [category_id]
			FROM [msdb].[dbo].[sysalerts]
			WHERE [sysalerts].[job_id] = [sysjobs].[job_id] 
			FOR XML PATH('sysalert'), ELEMENTS, TYPE
		)
	FROM [msdb].[dbo].[sysjobs]
	ORDER BY [sysjobs].[name]
	FOR XML PATH('sysjob'), ELEMENTS
),'</SqlJobs>') AS xml) AS Config
