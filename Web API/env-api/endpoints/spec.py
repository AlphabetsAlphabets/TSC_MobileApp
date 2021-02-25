from flask_restful import Resource, abort, reqparse
from typing import Union, Iterable
from QA.database import Database
from QA.key import Key

"""
Change * in all sql query to selected ones when decided in the future.
"""

class Spec(Resource):
    def __init__(self):
        self.schema = "testing"
        self.conn, self.cursor = Database.connect("localhost", "root", "YJH030412yjh_g", self.schema)
        # self.conn, self.cursor = Database.connect("localhost", "root", "8811967", self.schema)

        parser = reqparse.RequestParser()
        parser.add_argument("key", type=str, location="headers")
        parser.add_argument("user", type=str, location="headers")

        parsed = parser.parse_args()
        key, user = parsed["key"], parsed["user"]

        Key().verifyKey(user, key)

        self.BASE_QUERY = f"SELECT * FROM {self.schema}.spec"
        """
        Change spec in {self.schema}.spec to the correct table name. For example if the correct table name is invoice, modify it like so:
        {self.schema}.invoice. Do the same modification in line 71
        """

    def get(self, table: str, o1: Union[str, int], o2: Union[str, int], o3: Union[str, int], o4: Union[str, int], o5: Union[str, int]) -> str:
        """Collects all the parameters into a list. Then filters out unwanted values."""
        initialParams = [o1, o2, o3, o4, o5] 
        filteredParams = list(filter(self.removeNone, initialParams)) # removes all empty values ("n")
        noneIndices = [i for i, v in enumerate(initialParams) if v == "n"]
        noneIndices.sort(reverse=True) # Reverse it so that when deleting values from lists, via indices the program won't throw an IndexError exception
        
        """Makes a request to the SQL database which returns None intentionally, as this is required in order to get the column names"""
        silent = f"{self.BASE_QUERY} WHERE table_name = 'silent'"  
        self.cursor.execute(silent)
        _ = self.cursor.fetchall()

        """The actual query to be made, to get the associated SQL query to get data for that specific table."""
        sqlQuery = f"{self.BASE_QUERY} WHERE table_name = '{table}'"
        self.cursor.execute(sqlQuery)
        res = self.cursor.fetchall()

        desc = self.cursor.description
        """Starts from the 2nd index, because the first two column names are: table_name and sql. Which is not needed."""
        names = [des[0] for des in desc][2:]
        parameters = {k: v for k, v in zip(names, filteredParams)}

        if len(res) == 0:
            abort(404, message = f"Table with the name {table} does not exist. Check your spelling, and try again.")

        formattedRes = Database().toSerialisable(res)
        conditionals = list(filter(self.removeNone, formattedRes[0]))[2:]
        """Deletes unneeded parameters from the list so it won't be added into a SQL query."""
        for i in noneIndices:
            del conditionals[i]

        keyValuePairs = {k: v for k, v in zip(filteredParams, conditionals)}

        whereQuerySnippets = []
        for value, k in zip(filteredParams, parameters):
            con, v = keyValuePairs[value], parameters[k]
            whereQuerySnippets.append(f"{con} = '{v}'")
        
        stitchedWhereQuery= (" AND ").join(whereQuerySnippets)
        stitchedQuery = f"SELECT * FROM {self.schema}.{table} WHERE {stitchedWhereQuery}"

        """The part where the api makes another SQL query on your behalf"""
        _ , redirCursor = Database.connect("localhost", "root", "YJH030412yjh_g", "{self.schema}")
        redirCursor.execute(stitchedQuery)

        redirRes = redirCursor.fetchall()
        lengthOfRedirRes = len(redirRes)

        """Handles the part with more than one result"""
        if lengthOfRedirRes > 1:
            tempConn, tempCursor = Database.connect("localhost", "root", "YJH030412yjh_g", "{self.schema}")
            silent = tempCursor.execute(f"SELECT * FROM {self.schema}.trans LIMIT 1")
            res = tempCursor.fetchall()
            if len(res) == 0:
                abort(404, message = "Table not found.")

            serialisedRedirRes = Database().formatEntries(redirRes)

            return serialisedRedirRes

        elif lengthOfRedirRes == 0:
            abort(404, message = "No entries with the specified information found.")

        serialisedRedirRes = Database().formatEntries(redirRes)

        """Makes a key value pair to the appriroiate value. For it to be clearer."""
        keyValuePairOfSerialisedRedirRes = Database().keyValuePairing(redirCursor, serialisedRedirRes)

        return keyValuePairOfSerialisedRedirRes

    def removeNone(self, value: Union[str, int]) -> Union[str, int]:
        """Meant to use with the filter function, to remove all None values"""
        if value != "n" and value != None:
            return value 