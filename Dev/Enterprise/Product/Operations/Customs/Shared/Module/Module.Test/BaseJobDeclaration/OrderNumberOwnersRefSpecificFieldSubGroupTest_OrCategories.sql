(
	(
		(
			(
				JE_PK IN 
				(
					SELECT JE_PK FROM dbo.JobDeclarationOrderNumber 
						WHERE 
					(
						JE_OwnerRef like 'C%' 
						AND
						JE_OwnerRef >= 'C' 
						AND
						JE_OwnerRef <= 'þ'
					)
					AND
					(
						JE_OwnerRef like 'D%' 
						AND
						JE_OwnerRef >= 'D' 
						AND
						JE_OwnerRef <= 'þ'
					)
					AND
					(
						JE_OwnerRef like 'G%' 
						AND
						JE_OwnerRef >= 'G' 
						AND
						JE_OwnerRef <= 'þ'
					)
				)
			)
			AND
			(
				JE_PK NOT IN 
				(
					SELECT JE_PK FROM dbo.JobDeclarationOrderNumber 
						WHERE 
					(
						JE_OwnerRef like 'I%' 
						AND
						JE_OwnerRef >= 'I' 
						AND
						JE_OwnerRef <= 'þ'
					)
					OR
					(
						JE_OwnerRef like 'J%' 
						AND
						JE_OwnerRef >= 'J' 
						AND
						JE_OwnerRef <= 'þ'
					)
				)
			)
		)
		AND
		(
			(
				JE_PK IN 
				(
					SELECT JE_PK FROM dbo.JobDeclarationOrderNumber 
						WHERE 
					(
						JE_OwnerRef like 'A%' 
						AND
						JE_OwnerRef >= 'A' 
						AND
						JE_OwnerRef <= 'þ'
					)
				)
			)
			OR
			(
				JE_PK NOT IN 
				(
					SELECT JE_PK FROM dbo.JobDeclarationOrderNumber 
						WHERE 
					(
						JE_OwnerRef like 'B%' 
						AND
						JE_OwnerRef >= 'B' 
						AND
						JE_OwnerRef <= 'þ'
					)
				)
			)
		)
	)
	AND
	(
		JE_PK IN 
		(
			SELECT JE_PK FROM dbo.JobDeclarationOrderNumber 
				WHERE 
			(
				JE_OwnerRef like 'E%' 
				AND
				JE_OwnerRef >= 'E' 
				AND
				JE_OwnerRef <= 'þ'
			)
			OR
			(
				JE_OwnerRef like 'F%' 
				AND
				JE_OwnerRef >= 'F' 
				AND
				JE_OwnerRef <= 'þ'
			)
		)
	)
)
AND
(
	JE_PK IN 
	(
		SELECT JE_PK FROM dbo.JobDeclarationOrderNumber 
			WHERE 
		(
			JE_OwnerRef like 'H%' 
			AND
			JE_OwnerRef >= 'H' 
			AND
			JE_OwnerRef <= 'þ'
		)
	)
)
AND
(
	JE_PK NOT IN 
	(
		SELECT JE_PK FROM dbo.JobDeclarationOrderNumber 
			WHERE 
		(
			JE_OwnerRef like 'J%' 
			AND
			JE_OwnerRef >= 'J' 
			AND
			JE_OwnerRef <= 'þ'
		)
		AND
		(
			JE_OwnerRef like 'K%' 
			AND
			JE_OwnerRef >= 'K' 
			AND
			JE_OwnerRef <= 'þ'
		)
	)
)
