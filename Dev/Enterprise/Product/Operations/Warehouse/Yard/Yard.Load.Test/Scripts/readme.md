# Setup
1. Install python from the software center
2. python -m venv ./venv  
3. ./venv/Scripts/activate
4. Install the required modules `pip install -r requirements` (You could also make an environment if you don't want want to install them all globally)

# Configuration

Update config.json to provide required number of receive/release line numbers.

Use Config.py if you need to update the script and do advanced setup.

# Generating the data
To generate the data run `python ./Generator.py {Database} -b {Batch Size (Optional)}` this will generate csvs inside ./csv and validate the schemas used against the database specified.
Ex. python ./generator.py Odyssey -t release

# Inserting the data
To insert the data run `python ./InsertData.py {Database}` this will use the same tables in Config and bulk insert the data from the csv's into the specified database, ideally it should be the same database used in generation.
Ex. python .\InsertData.py Odyssey