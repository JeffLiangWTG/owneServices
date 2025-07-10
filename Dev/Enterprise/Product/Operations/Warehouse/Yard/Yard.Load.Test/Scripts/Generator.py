import pandas as pd
import argparse
from pyodbc import Cursor, connect
from dataclasses import fields
import time
import os
from Schemas import Table
from Helper import print_error

class CSVManager():
    batch_count = 0
    batch_data: dict[Table, list] = {}
    batch_size = 500000
    table_columns: dict[Table, list[str]] = {}

    def __init__(self, batch_size=500000) -> None:
        self.batch_size = batch_size

    def create_file(self, table: Table, columns: list[str]) -> None:
        self.table_columns[table] = columns
        os.makedirs('./csv', exist_ok=True)
        pd.DataFrame(columns=columns).to_csv(f'./csv/{table.value}.csv', index=False)

    def append_to_file(self, table: Table, record) -> None:

        # Append record to the list
        if table not in self.batch_data:
            self.batch_data[table] = []
        self.batch_data[table].append(record)
        
        self.batch_count += 1
        if self.batch_count >= self.batch_size:
            self.save_batch()

    def save_batch(self):
        for table, records in self.batch_data.items():
            if table not in self.table_columns:
                print_error(f"Table '{table.value}' has not been validated, please add it to the tables variable in __main__.")
                exit(1)
            df = pd.DataFrame([vars(record) for record in records], columns=self.table_columns[table])
            df.to_csv(f'./csv/{table.value}.csv', mode='a', index=False, header=False)
        self.batch_data.clear()
        self.batch_count = 0

def fetch_column_order(cursor: Cursor, table: Table):
    cursor.execute(f"""
    SELECT 
        COLUMN_NAME 
    FROM 
        INFORMATION_SCHEMA.COLUMNS 
    WHERE 
        TABLE_NAME = '{table.value}' 
    ORDER BY 
        ORDINAL_POSITION;
    """)
    result = cursor.fetchall()
    return [column[0] for column in result]

def check_table_exists(cursor: Cursor, table: Table) -> bool:
    cursor.execute(f"""
    SELECT 
        1 
    FROM 
        INFORMATION_SCHEMA.TABLES 
    WHERE 
        TABLE_NAME = '{table.value}';
    """)
    result = cursor.fetchone()
    return result is not None

def compare_columns(cursor: Cursor, table: Table, schema) -> tuple[set[str], set[str]]:
    db_columns = set(fetch_column_order(cursor, table))
    schema_columns = set([field.name for field in fields(schema)])
    missing_in_db = schema_columns - db_columns
    missing_in_schema = db_columns - schema_columns
    return missing_in_db, missing_in_schema

def validate_schema(csv_manager: CSVManager, database: str, tables: dict[Table, any]):

    # Establish connection
    connection_string = f'DRIVER={{ODBC Driver 17 for SQL Server}};SERVER=localhost;DATABASE={args.database};Trusted_Connection=yes;'
    with connect(connection_string, autocommit=True) as connection:
        cursor = connection.cursor()

        # Validate schemas
        print("Validating schemas...")
        for table, schema in tables.items():

            if not check_table_exists(cursor, table):
                print_error(f"Table '{table.value}' does not exist in the database.")
                exit(1)

            # Check for any missing columns in schema or db
            missing_in_db, missing_in_schema = compare_columns(cursor, table, schema)
            if missing_in_db or missing_in_schema:
                print_error(f"Discrepancies found for table '{table.value}':")
                if missing_in_db:
                    print_error(f"Columns missing in {database}: {missing_in_db}")
                if missing_in_schema:
                    print_error(f"Columns missing in Schema.py: {missing_in_schema}")
                exit(1)
            else:
                print(f"Schema for table {table.value} matches perfectly with the database schema.")
                csv_manager.create_file(table, fetch_column_order(cursor, table))
                
        print("Schemas Validated")

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Connect to a SQL Server database.')
    parser.add_argument('database', type=str, help='Name of the database to validate against.')
    parser.add_argument('-b', '--batch_size', type=int, default=500000, help='Batch size for writing to files.')
    parser.add_argument('-t', '--type', type=str, default='receive', help='Generate object type')

    args = parser.parse_args()

    if not args.database:
        print_error("The database argument is required.")
        exit(1)

    object_type = args.type
    csv_manager = CSVManager(batch_size=args.batch_size)

    from Config import TABLES
    validate_schema(csv_manager, args.database, TABLES)

    print("Generating Data")
    start_time = time.time()

    from Config import generate_data
    generate_data(csv_manager, object_type)
    csv_manager.save_batch()

    end_time = time.time()
    print(f"Data Generated in {end_time-start_time:.2f} seconds")
