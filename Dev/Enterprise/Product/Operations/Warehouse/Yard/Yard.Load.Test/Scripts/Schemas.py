# schemas.py
from dataclasses import dataclass, field
from datetime import datetime
from Helper import *
from uuid import uuid4
from enum import Enum

class Table(Enum):
    JobDocAddress = 'JobDocAddress'
    CYDUnitLineItem = 'CYDUnitLineItem'
    CYDYardUnitState = 'CYDYardUnitState'
    CYDReceiveAdvice = 'CYDReceiveAdvice'
    CYDReceiveAdviceLine = 'CYDReceiveAdviceLine'
    CYDReleaseAdvice = 'CYDReleaseAdvice'
    CYDReleaseAdviceLine = 'CYDReleaseAdviceLine'
    GenCustomAddOnValue = 'GenCustomAddOnValue'
    CYDMovement = 'CYDMovement'

@dataclass
class GenCustomAddOnValue:
    XV_Type: str
    XV_Data: str
    XV_ParentTableCode: str
    XV_ParentID: str
    XV_Name: str
    XV_PK: str = field(default_factory=lambda: str(uuid4()))
    XV_XR_Rule: str = None
    XV_IsRuleEnabled: int = 1
    XV_AutoVersion: int = 0
    XV_SystemCreateTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    XV_SystemCreateUser: str = '~BP'
    XV_SystemLastEditTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    XV_SystemLastEditUser: str = '~BP'  

@dataclass
class CYDReleaseAdviceLine:
    YEL_YRE_ReleaseAdvice: str
    YEL_YLI_UnitLineItem: str
    YEL_PK: str = field(default_factory=lambda: str(uuid4()))
    YEL_AutoVersion: int = 0
    YEL_PickupDate: datetime = field(default_factory=lambda: datetime(2024, 7, 10))
    YEL_ReadyDate: datetime = field(default_factory=lambda: datetime(2024, 7, 1))
    YEL_OnHireDate: datetime = None
    YEL_OffHireDate: datetime = None
    YEL_SystemCreateTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YEL_SystemCreateUser: str = '~BP'
    YEL_SystemLastEditTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YEL_SystemLastEditUser: str = '~BP'  

@dataclass
class CYDReceiveAdvice:
    YRA_JobNumber: str
    YRA_AcceptanceNumber: str
    YRA_PK: str = field(default_factory=lambda: str(uuid4()))
    YRA_AutoVersion: int = 0
    YRA_WW_Yard: str = WHS_WAREHOUSE_PK
    YRA_FromDate: datetime = field(default_factory=lambda: datetime(2024, 7, 1))
    YRA_ToDate: datetime = field(default_factory=lambda: datetime(2024, 7, 8))
    YRA_Mode: str = 'EMT'
    YRA_SystemCreateTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YRA_SystemCreateUser: str = '~BP'
    YRA_SystemLastEditTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YRA_SystemLastEditUser: str = '~BP'

@dataclass
class CYDReleaseAdvice:
    YRE_JobNumber: str
    YRE_ReleaseNumber: str
    YRE_PK: str = field(default_factory=lambda: str(uuid4()))
    YRE_AutoVersion: int = 0
    YRE_WW_Yard: str = WHS_WAREHOUSE_PK
    YRE_FromDate: datetime = field(default_factory=lambda: datetime(2024, 7, 1))
    YRE_ToDate: datetime = field(default_factory=lambda: datetime(2024, 7, 8))
    YRE_Mode: str = 'EMT'
    YRE_SystemCreateTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YRE_SystemCreateUser: str = '~BP'
    YRE_SystemLastEditTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YRE_SystemLastEditUser: str = '~BP'

@dataclass
class CYDMovement:
    YML_PK: str = field(default_factory=lambda: str(uuid4()))

@dataclass
class CYDYardUnitState:
    YUS_UnitID: str
    YUS_YRL_ReceiveLine: str
    YUS_PK: str = field(default_factory=lambda: str(uuid4()))
    YUS_AutoVersion: int = 0
    YUS_YEL_ReleaseLine: str = None
    YUS_YDL_Delivery: str = None
    YUS_YPL_Pickup: str = None
    YUS_YTU_ReceiveTransportationUnit: str = None
    YUS_YTU_DispatchTransportationUnit: str = None
    YUS_WW_CurrentYard: str = WHS_WAREHOUSE_PK
    YUS_WL_CurrentYardLocation: str = None
    YUS_SystemCreateTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YUS_SystemCreateUser: str = '~BP'
    YUS_SystemLastEditTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YUS_SystemLastEditUser: str = '~BP'
    YUS_LoadTime: datetime = None
    YUS_UnloadTime: datetime = None
    YUS_GS_NKLoadUser: str = None
    YUS_GS_NKUnloadUser: str = None

@dataclass
class CYDUnitLineItem:
    YLI_PK: str = field(default_factory=lambda: str(uuid4()))
    YLI_RowVersion: str = None
    YLI_Type: str = 'CNT'
    YLI_ManufactureDate: datetime = None
    YLI_RC_ContainerType: str = '65FC688B-A8BC-49FA-A17E-159DE4ED6830'
    YLI_SealNumber: int = 1
    YLI_Quantity: int = 1
    YLI_IsEmpty: int = 0
    YLI_SystemCreateTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YLI_SystemCreateUser: str = '~BP'
    YLI_SystemLastEditTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YLI_SystemLastEditUser: str = '~BP'

@dataclass
class CYDReceiveAdviceLine:
    YRL_YRA_ReceiveAdvice: str
    YRL_YLI_UnitLineItem: str
    YRL_PK: str = field(default_factory=lambda: str(uuid4()))
    YRL_AutoVersion: int = 0
    YRL_PreviousOnHireDate: datetime = None
    YRL_OffHireDate: datetime = None
    YRL_SystemCreateTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YRL_SystemCreateUser: str = '~BP'
    YRL_SystemLastEditTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    YRL_SystemLastEditUser: str = '~BP'

@dataclass
class JobDocAddress:
    E2_AddressType: str
    E2_OA_Address: str
    E2_ParentTableCode: str
    E2_ParentID: str
    E2_PK: str = field(default_factory=lambda: str(uuid4()))
    E2_IsValid: int = 0
    E2_IsResidential: int = 0
    E2_AddressSequence: int = 0
    E2_Contact: str = None
    E2_AddressOverride: int = 0
    E2_Address1: str = None
    E2_Address2: str = None
    E2_Postcode: str = None
    E2_State: str = None
    E2_RN_NKCountryCode: str = None
    E2_Phone: str = None
    E2_Mobile: str = None
    E2_Fax: str = None
    E2_GovRegNum: str = None
    E2_GovRegNumType: str = 'DEF'
    E2_Email: str = None
    E2_ValidationStatus: str = 'NRQ'
    E2_AddressMap: str = None
    E2_City: str = None
    E2_SuppressAddressValidationError: int = 0
    E2_SystemCreateTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    E2_SystemCreateUser: str = '~BP'
    E2_SystemLastEditTimeUtc: datetime = field(default_factory=lambda: datetime(2024, 6, 1))
    E2_SystemLastEditUser: str = '~BP'
    E2_GeoLocation: str = None
    E2_ScreeningStatus: str = 'NOT'
    E2_AdditionalAddressInformation: str = None
    E2_AutoVersion: str = 0
    E2_CompanyName: str = None