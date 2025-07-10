from pyodbc import connect, Cursor
import argparse
import time
import os
from Helper import print_error

def execute_query(cursor: Cursor, query: str) -> None:
    print(f"\t{query}")
    cursor.execute(query)

def disable_triggers(cursor: Cursor, tables: list[str]):
    for table in tables:
        execute_query(cursor, f"ALTER TABLE {table} DISABLE TRIGGER ALL;")

def enable_triggers(cursor: Cursor, tables: list[str]):
    for table in tables:
        execute_query(cursor, f"ALTER TABLE {table} ENABLE TRIGGER ALL;")

def disable_indexes(cursor: Cursor, tables: list[str]):
    cursor.execute("""
    SELECT sys.indexes.name, sys.objects.name
    FROM 
        sys.indexes
    JOIN 
        sys.objects 
        ON sys.indexes.object_id = sys.objects.object_id
    LEFT JOIN 
        sys.foreign_keys AS fk 
        ON fk.parent_object_id = sys.indexes.object_id AND fk.key_index_id = sys.indexes.index_id
    WHERE sys.indexes.type_desc = 'NONCLUSTERED'
    AND sys.objects.type_desc = 'USER_TABLE'
    AND sys.objects.name IN ('{}')
    AND fk.object_id IS NULL;
    """.format("', '".join(tables)))

    # Log each disabled index
    sql = cursor.fetchall()
    for index, table in sql:
        try:
            execute_query(cursor, f'ALTER INDEX {index} ON {table} DISABLE;')
        except:
            print(f'\tFailed to disabled index: {index}')

def clear_tables(cursor: Cursor, tables: list[str]):
    for table in tables:
        execute_query(cursor, f"DELETE FROM {table};")

def rebuild_indexes(cursor: Cursor, tables: list[str]):
    for table in tables:
        execute_query(cursor, f"ALTER INDEX ALL ON {table} REBUILD;")

def bulk_insert(cursor: Cursor, file_path: str, table_name: str) -> None:
    query = f"""
    BULK INSERT {table_name}
    FROM '{file_path}'
    WITH (
        FIRSTROW = 2,
        FIELDTERMINATOR = ',',  
        ROWTERMINATOR = '\\n',
        TABLOCK
    );
    """
    execute_query(cursor, query)

if __name__ == "__main__":

    # Setup argument parser
    parser = argparse.ArgumentParser(description='Connect to a SQL Server database.')
    parser.add_argument('database', type=str, help='Name of the database to connect to.')
    args = parser.parse_args()

    if not args.database:
        print_error("The database argument is required.")
        exit(1)

    base_dir = os.path.dirname(os.path.abspath(__file__))

    from Config import TABLES,CLEAR_TABLES
    tables = [table.value for table in TABLES]
    clearTables = [table.value for table in CLEAR_TABLES]

    # Establish connection
    connection_string = f'DRIVER={{ODBC Driver 17 for SQL Server}};SERVER=localhost;DATABASE={args.database};Trusted_Connection=yes;'
    with connect(connection_string, autocommit=True) as connection:

        # Create a cursor object to interact with the database
        cursor = connection.cursor()

        # Setup function
        print("Inserting Setup Data")
        from Config import setup_database
        setup_database(cursor)

        try:
            print("Switching database to BULK logging mode:")
            execute_query(cursor, f"ALTER DATABASE {args.database} SET RECOVERY BULK_LOGGED;")
            cursor.commit()
            connection.autocommit = False

            print("--- STARTING TRANSACTION")
            start_time = time.time()
            cursor.execute("BEGIN TRANSACTION;")

            print("Disabling triggers:")
            disable_triggers(cursor, tables)

            print("Disabling unnecessary indexes:")
            disable_indexes(cursor, tables)

            print("Clearing tables")
            clear_tables(cursor, clearTables)

            print("Starting BULK inserts")
            for table in tables:
                bulk_insert(cursor, os.path.join(base_dir, f'csv/{table}.csv'), table)

            print("Rebuilding indexes:")
            rebuild_indexes(cursor, tables)

            print("Enabling triggers:")
            enable_triggers(cursor, tables)

            print("--- COMMITTING TRANSACTION")
            cursor.execute("COMMIT;")
            
            end_time = time.time()
            print(f"Transaction completed in {end_time - start_time:.2f} seconds")
        except Exception as e:
            cursor.execute("ROLLBACK TRANSACTION;")
            print(f"Transaction failed and was rolled back: {e}")
        finally:
            print("Switching database to Simple logging mode:")
            connection.autocommit = True
            execute_query(cursor, f"ALTER DATABASE {args.database} SET RECOVERY SIMPLE;")
            cursor.commit()
