import json
import uuid
from Generator import CSVManager
from Schemas import CYDMovement, CYDReleaseAdvice, CYDReleaseAdviceLine, Table, CYDReceiveAdvice, JobDocAddress, CYDUnitLineItem, CYDReceiveAdviceLine, CYDYardUnitState, GenCustomAddOnValue
from Helper import setup_warehouse
from pyodbc import Cursor
from Helper import generate_container_number

### PLEASE SPECIFY THE TABLES YOU WILL BE USING AND THE SCHEMA
### This is used to perform schema validation and correctly order the columns in the csv
TABLES = {
        Table.CYDReceiveAdvice: CYDReceiveAdvice,
        Table.CYDReleaseAdvice: CYDReleaseAdvice,
        Table.CYDUnitLineItem: CYDUnitLineItem,
        Table.CYDReceiveAdviceLine: CYDReceiveAdviceLine,
        Table.CYDReleaseAdviceLine: CYDReleaseAdviceLine,
        Table.CYDYardUnitState: CYDYardUnitState,
        Table.GenCustomAddOnValue: GenCustomAddOnValue,
        Table.JobDocAddress: JobDocAddress
}

CLEAR_TABLES = {
        Table.CYDMovement: CYDMovement,
        Table.CYDYardUnitState: CYDYardUnitState,
        Table.CYDReceiveAdviceLine: CYDReceiveAdviceLine,
        Table.CYDReceiveAdvice: CYDReceiveAdvice,
        Table.CYDReleaseAdviceLine: CYDReleaseAdviceLine,
        Table.CYDReleaseAdvice: CYDReleaseAdvice,
        Table.CYDUnitLineItem: CYDUnitLineItem,
        Table.GenCustomAddOnValue: GenCustomAddOnValue,
        Table.JobDocAddress: JobDocAddress
}

def load_config(config_name="receive"):
    with open("config.json", "r") as file:
        config = json.load(file)
        if config_name in config:
            return config[config_name]
        else:
            raise ValueError(f"Config '{config_name}' not found.")

def get_tables_and_clear_tables(config_name="receive"):
    return load_config(config_name)

### IMPLEMENT THIS
### This is called by Generator.py and is used to generate csv files. The CSVManager
### class handles batching and creation of files as well as conversion from
### dataclass
def generate_data(csv_manager: CSVManager, object_type: str):
    config = get_tables_and_clear_tables(object_type)
    num_single_line = config['num_single_line'] 
    num_multi_line = config['num_multi_line'] 
    lines_per_multi = config['lines_per_multi'] 

    if object_type == 'receive' :
        print("Generating data for 'receieve' type.")
        index = 0
        while num_single_line + num_multi_line > 0:
            if num_single_line > 0:
                num_lines = 1
                num_single_line -= 1
            else:
                num_lines = lines_per_multi
                num_multi_line -= 1

            receive_advice = CYDReceiveAdvice(f"JOBV{index}", f"ACCV{index}")
            csv_manager.append_to_file(Table.CYDReceiveAdvice, receive_advice)
            csv_manager.append_to_file(Table.JobDocAddress, JobDocAddress('BKD', '9861CEFC-2F62-4AA0-9416-004FC37AD06B', 'YRA', receive_advice.YRA_PK))

            # Add lines
            for line in range(num_lines):
                unit = generate_container_number()
                shared_pk = str(uuid.uuid4())
                csv_manager.append_to_file(Table.CYDUnitLineItem, CYDUnitLineItem(YLI_PK=shared_pk))
                csv_manager.append_to_file(Table.CYDReceiveAdviceLine, CYDReceiveAdviceLine(receive_advice.YRA_PK, shared_pk, YRL_PK=shared_pk))
                csv_manager.append_to_file(Table.GenCustomAddOnValue, GenCustomAddOnValue("STR", unit, "YRL", shared_pk, "CYDReceiveAdviceLineUnitID", XV_PK=shared_pk))
                csv_manager.append_to_file(Table.CYDYardUnitState, CYDYardUnitState(unit, shared_pk, YUS_PK=shared_pk))
            index += 1
    elif object_type == 'release':
        print("Generating data for 'release' type.")
        index = 0
        while num_single_line + num_multi_line > 0:
            if num_single_line > 0:
                num_lines = 1
                num_single_line -= 1
            else:
                num_lines = lines_per_multi
                num_multi_line -= 1

            release_advice = CYDReleaseAdvice(f"JOBRelease{index}", f"RELEASE{index}")
            csv_manager.append_to_file(Table.CYDReleaseAdvice, release_advice)
            csv_manager.append_to_file(Table.JobDocAddress, JobDocAddress('BKD', '9861CEFC-2F62-4AA0-9416-004FC37AD06B', 'YRE', release_advice.YRE_PK))

            # Add lines
            for line in range(num_lines):
                unit = generate_container_number()
                shared_pk = str(uuid.uuid4())
                csv_manager.append_to_file(Table.CYDUnitLineItem, CYDUnitLineItem(YLI_PK=shared_pk))
                csv_manager.append_to_file(Table.CYDReleaseAdviceLine, CYDReleaseAdviceLine(release_advice.YRE_PK, shared_pk, YEL_PK=shared_pk))
                csv_manager.append_to_file(Table.GenCustomAddOnValue, GenCustomAddOnValue("STR", unit, "YEL", shared_pk, "CYDReleaseAdviceLineUnitID", XV_PK=shared_pk))
                csv_manager.append_to_file(Table.CYDYardUnitState, CYDYardUnitState(unit, shared_pk, YUS_PK=shared_pk))
            index += 1
    else:
        print(f"Unknown object type: {object_type}")
### IMPLEMENT THIS
### This is called in InsertData.py right before inserting your data from the
### generated csv's. Use it to setup anything you might need present in
### the database
def setup_database(cursor: Cursor):
    setup_warehouse(cursor)