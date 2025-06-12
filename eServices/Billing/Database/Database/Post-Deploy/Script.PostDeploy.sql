if not exists(select 1 from edi.ConfigFirstMessage)
begin
	insert edi.ConfigFirstMessage(FM_Category, FM_PriceItemCode, FM_IncludeRef1, FM_IncludeRef2)
	values('NZC', 'DEC', 1, 0)
		, ('NZC', 'CAR', 1, 0)
		, ('NZC', 'IM1', 1, 0)
		, ('NZC', 'EX1', 1, 0)
		, ('NZC', 'CRE', 1, 0)
		, ('NZC', 'OCR', 1, 0)
		, ('NZC', 'ICR', 1, 0)
		, ('JPC', 'AFR', 1, 1)
		, ('CMP', 'SCO', 1, 1)
		, ('CMP', 'SCS', 1, 1)
		, ('CMD', 'CMD', 1, 0)
		, ('USC', 'UJL', 1, 0)
		, ('USC', 'UPL', 1, 0)
		, ('USC', 'URB', 1, 0)
		, ('USC', 'UQT', 1, 0)
		, ('USC', 'URR', 1, 0)
		, ('USC', 'USO', 1, 0)
		, ('USC', 'UFT', 1, 0)
		, ('USC', 'UXT', 1, 0)
		, ('PMG', 'PM1', 1, 1)
		, ('CTR', 'CTR', 1, 1)
		, ('SPM', 'SPE', 1, 0)
		, ('SPM', 'SPA', 1, 0)
		, ('USC', 'ISF', 1, 0)
		, ('ASC', 'ASC', 1, 1)
		, ('RIM', 'RIM', 1, 1)

	insert edi.ConfigFirstMessageFilter(MF_Category, MF_PriceItemCode, MF_RefIndex, MF_Operator, MF_RefValue)
	values('CMP', 'SCO', 3, '!=', 'UNMATCHED ORGANISATION')
end

if not exists(select 1 from edi.ConfigReferenceSwap)
begin
	insert edi.ConfigReferenceSwap(Category, PriceItemCode, FirstVersion, LastVersion, RefIndex1, RefIndex2, RefIndex3, RefIndex4, RefIndex5)
	values
		-- client mapping CMP: Ref4 is interface name -> 1; Ref3 is Element -> 2; Ref2 = file name -> 3; Ref1 = tracking ID -> 4
		  ('CMP', 'CMP', 0, NULL, 4, 3, 2, 1, 5)

		-- client mapping SCO: Ref4 is interface name -> 1; Ref2 is Order# -> 2; Ref3 = buyer org -> 3; Ref1 = EM_MessageNum -> 4
		 ,('CMP', 'SCO', 0, NULL, 4, 2, 3, 1, 5)

		-- client mapping SCS: Ref4 is interface name -> 1; Ref2 is Shipment ID -> 2; Ref3 = null -> 3; Ref1 = EM_MessageNum -> 4
		, ('CMP', 'SCS', 0, NULL, 4, 2, 3, 1, 5)

		-- container tracking (old source from ehub): Ref2 = Container, Ref4 = MBOL, Ref3 = Event
		, ('CTR', 'CTR', 0, NULL, 2, 4, 3, 1, 5)
		-- container tracking (new source): Ref2 = Container, Ref4 = MBOL, Ref3 = Event
		-- just for consistency with old source
		, ('CTR', 'CTO', 0, NULL, 2, 4, 3, 1, 5)
		, ('CTR', 'CTV', 0, NULL, 2, 4, 3, 1, 5)

		-- shipping port messaging ALL: Ref1 = tracking ID, Ref2 = recipient; Ref3 = role
		-- shipping port messaging SPA: Ref4 = Container ReleaseNum, Ref5 = unit count
		, ('SPM', 'SPA', 0, NULL, 4, 2, 3, 1, 5)
		-- shipping port messaging SPE: Ref4 = Container Number, Ref5 = data context key
		, ('SPM', 'SPE', 0, NULL, 4, 2, 3, 1, 5)
		-- shipping port messaging SPR: Ref4 = Shipment Number , Ref5 = null
		, ('SPM', 'SPR', 0, NULL, 4, 2, 3, 1, 5)

		-- US Customs ISF: Ref2 is transaction#, Ref1 is post code, Ref3 is tracking ID, Ref 4&5 null
		, ('USC', 'ISF', 0, NULL, 2, 1, 3, 4, 5)

		-- ASYCUDA: Ref2 is consol, Ref3 is country, Ref1 is tracking ID, Ref 4&5 null
		, ('ASC', 'ASC', 0, NULL, 2, 3, 1, 4, 5)

		-- EAdapter 
		-- Warehouse lines: ref2 is the docket ID, ref3 is the warehouse, ref1 = WE_LineNo, ref4 = EI_SessionGUID
		-- Order lines: ref2 is the order number, ref3 is the org, ref1 = JO_Partno + '/' + JO_LineNo, ref4 = EI_SessionGUID)
		, ('EAD', 'ICA', 0, NULL, 2, 3, 1, 4, 5)
		, ('EAD', 'IUA', 0, NULL, 2, 3, 1, 4, 5)
		, ('EAD', 'ICJ', 0, NULL, 2, 3, 1, 4, 5)
		, ('EAD', 'ICK', 0, NULL, 2, 3, 1, 4, 5)
		, ('EAD', 'IUJ', 0, NULL, 2, 3, 1, 4, 5)
		, ('EAD', 'IUK', 0, NULL, 2, 3, 1, 4, 5)
end
