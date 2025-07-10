from pyodbc import Cursor
import random
import string

GLB_COMPANY_PK = "c07ec8db-15ea-42e7-84cf-1323dce11bfa"
GLB_BRANCH_PK = "09678196-86ef-466b-b719-ca17d35da158"
ORG_HEADER_PK = "eef9a4d8-a1dc-4df4-834a-097f5799638a"
ORG_ADDRESS_PK = "41fb9b4f-946c-47b8-a6f5-ef2d42ab9881"
WHS_WAREHOUSE_PK = "6F74949B-947B-4B3C-B4DD-A131B514530B"
CONTAINER_PK = "65FC688B-A8BC-49FA-A17E-159DE4ED6830"

def create_org_header(cursor: Cursor) -> None:
    cursor.execute(f"""
    INSERT INTO OrgHeader (OH_PK, OH_Code, OH_FullName, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser, OH_SystemCreateTimeUtc, OH_SystemCreateUser) 
    VALUES (?, 'ABC', '', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
    """, (ORG_HEADER_PK))

def create_org_address(cursor: Cursor) -> None:
    cursor.execute(f"""
    INSERT INTO OrgAddress (
        OA_PK,
        OA_Code,
        OA_Address1,
        OA_OH,
        OA_City,
        OA_SystemCreateTimeUtc,
        OA_SystemCreateUser,
        OA_SystemLastEditTimeUtc,
        OA_SystemLastEditUser
    ) VALUES (
        '{ORG_ADDRESS_PK}',
        'NAME1',
        'NAME1',
        '{ORG_HEADER_PK}',
        'City',
        GETUTCDATE(), 
        '~BP',
        GETUTCDATE(),
        '~BP'
    )
    """)

def create_glb_company(cursor: Cursor) -> None:
    cursor.execute(f"""
    INSERT INTO GlbCompany (
        GC_PK,
        GC_Code,
        GC_Name,
        GC_RN_NKCountryCode,
        GC_RX_NKLocalCurrency,
        GC_SystemCreateTimeUtc,
        GC_SystemCreateUser,
        GC_SystemLastEditTimeUtc,
        GC_SystemLastEditUser
    ) VALUES (
        '{GLB_COMPANY_PK}',
        'XX1',
        'AU company',
        'AU',
        'AUD',
        GETUTCDATE(),
        '~BP',
        GETUTCDATE(),
        '~BP'
    )
    """)

def create_glb_branch(cursor: Cursor) -> None:
    cursor.execute(f"""
    INSERT INTO GlbBranch (
        GB_PK,
        GB_Code,
        GB_GC,
        GB_RL_NKHomePort,
        GB_SystemCreateTimeUtc,
        GB_SystemCreateUser,
        GB_SystemLastEditTimeUtc,
        GB_SystemLastEditUser
    ) VALUES (
        '{GLB_BRANCH_PK}',
        'XX1',
        '{GLB_COMPANY_PK}',
        'AUSYD',
        GETUTCDATE(),
        '~BP',
        GETUTCDATE(),
        '~BP'
    )
    """)

def create_whs_location_type_if_not_exists(cursor: Cursor) -> str:

    cursor.execute("""
    SELECT WLT_PK
    FROM WhsLocationType
    WHERE WLT_Code = 'DOC' AND WLT_Description = 'Dock Door'          
    """)
    whs_type = cursor.fetchone()
    if whs_type:
        return whs_type[0]

    cursor.execute(f"""    
    INSERT INTO WhsLocationType (
        WLT_PK,
        WLT_Code,
        WLT_Description,
        WLT_MaximumNumberOfProducts,
        WLT_LocationClass,
        WLT_DefaultCycleCountGranularity,
        WLT_SystemCreateTimeUtc,
        WLT_SystemCreateUser,
        WLT_SystemLastEditTimeUtc,
        WLT_SystemLastEditUser
    ) 
    OUTPUT INSERTED.WLT_PK
    VALUES (
        NEWID(),
        'DOC',
        'Dock Door',
        0,
        'DDL',
        'PWA',
        GETUTCDATE(), 
        '~BP',
        GETUTCDATE(),
        '~BP'
    )
    """)
    return cursor.fetchone()[0]

def setup_warehouse(cursor: Cursor) -> None:

    # Check if the warehouse already exists
    cursor.execute(f"""
    SELECT * FROM WhsWarehouse WHERE WW_PK = '{WHS_WAREHOUSE_PK}'               
    """)
    if cursor.fetchone():
        return

    create_org_header(cursor)
    create_org_address(cursor)
    create_glb_company(cursor)
    create_glb_branch(cursor)
    whs_location_type_pk = create_whs_location_type_if_not_exists(cursor)
    cursor.execute(f"""
    
    INSERT INTO WhsWarehouse (
        WW_PK,
        WW_WarehouseName,
        WW_OA_WarehouseAddress,
        WW_GB_RelatedCompanyBranch,
        WW_WarehouseCode,
        WW_WarehouseType,
        WW_WLT_DefaultLocationType,
        WW_SystemCreateTimeUtc,
        WW_SystemCreateUser,
        WW_SystemLastEditTimeUtc,
        WW_SystemLastEditUser
    ) VALUES (
        '{WHS_WAREHOUSE_PK}',
        'Warehouse Name',
        '{ORG_ADDRESS_PK}',
        '{GLB_BRANCH_PK}',
        'XX1',
        'CYD',
        '{whs_location_type_pk}',
        GETUTCDATE(), 
        '~BP',
        GETUTCDATE(),
        '~BP'
    )
    """)

def calculate_check_digit(container_number):
    result = 0
 
    for i in range(4):
        c = container_number[i]
        char_val = ord(c) - 55 if c.isalpha() else ord(c) - 87
        if char_val > 35 or char_val < 10:
            return "Invalid container number format"
 
        if char_val > 30:
            char_val += 3
        elif char_val > 20:
            char_val += 2
        elif char_val > 10:
            char_val += 1
 
        result += char_val << i
 
    for j in range(4, 10):
        char_val = ord(container_number[j]) - 48
 
        if char_val < 0 or char_val > 9:
            return "Invalid container number format"
        else:
            result += char_val << j
 
    result %= 11
 
    if result == 10:
        result = 0
 
    return str(result)
 
def generate_container_number():
    letters = ''.join(random.choices(string.ascii_uppercase, k=4))
    digits = ''.join(random.choices(string.digits, k=6))
    container_number_without_check_digit = letters + digits
    check_digit = calculate_check_digit(container_number_without_check_digit)
    if check_digit == "Invalid container number format":
        return "Error generating container number"
    return container_number_without_check_digit + check_digit

# Prints red error message
def print_error(message: str):
    print(f"\033[91mERROR: {message}\033[0m")
