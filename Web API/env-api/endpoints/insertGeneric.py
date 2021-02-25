# Schema: testing
from flask_restful import Resource, abort, reqparse
from QA.database import Database
from QA.key import Key

from typing import NoReturn, Union

from mysql.connector.cursor import MySQLCursor as TYPE_CURSOR
"""
This is for many tables. Pivot conditionals must change accordingly. 
As of 15/2/2021 this can't actually function because the responsible table does not exist.
"""

"""
IMPORTANT!

Prerequisites:
1. At line 26 change self.schema to the appropriate schema/database name
2. go to line 107 and read the doc-string
"""

class InsertGeneric(Resource):
    def __init__(self):
        self.schema = "testing"

        parser = reqparse.RequestParser()
        parser.add_argument("con1", type=str)
        parser.add_argument("key", type=str, location="headers")
        parser.add_argument("user", type=str, location="headers")

        self.parsed = parser.parse_args()
        key, user = self.parsed["key"], self.parsed["user"]

        if key == None or user == None:
            abort(406, message = "Either the key, or user field is left blank.")

        Key().verifyKey(user, key)

        """A redirect table of some sort is needed. Refer to testing.redirect for a guide."""
        self.conn, self.cursor = Database.connect("localhost", "root", "YJH030412yjh_g", "testing")
        # self.conn, self.cursor = Database.connect("localhost", "root", "8811967", "tsc_office")

    def __validateDatabase(self, cursor: TYPE_CURSOR, table: str) -> str:
        """
        Checks whether the table you are trying to add a new entry to exists, and has the 'insert' type. 
        If it doesn't exist a 404 not found error will be thrown. If it doesn't have the 'insert' type,
        a 403 forbidden error will be thrown instead.
        """
        sqlQuery = f"SELECT `sql_query` FROM testing.redirect WHERE table_name = '{table}'"
        self.cursor.execute(sqlQuery)
        res = cursor.fetchall()

        if len(res) == 0:
            abort(404, message = f"Table with the name of {table} does not exist, check your spelling, and try again.")

        return res[0][0]
 
    def __join(self, *values: list) -> Union[str, int]:
        """
        Strips all whitespace in front, and after the conditions. To make it cleaner, spaces between words will not be stripped.
        Add quotes to all values, then joins them all with ", " between each word.
        """
        strippedValues = [value.strip() for value in values if len(value.strip()) != 0]
        quotedValues = [f"'{value}'" for value in strippedValues]
        joinedValues = (", ").join(quotedValues)

        return len(quotedValues), joinedValues

    def put(self, table: str, invoiceno: str) -> dict:
        """Takes your role into account. If your role is not within the dictionary, a forbidden error is thrown.
        Provided that the checks provided by __validateDatabase has succeded. A new connection to the table you
        requested edit will be made. If the table does not exists a not found error will be thrown, otherwise
        it will continue as expected, and insert the row into the table."""
        wheres = {"tinvoicehistory": "`finvoiceno`", "tdebtordetail": "`debtorcode`"}
        if table not in wheres:
            abort(404, message=f"table with the name of {table} not found.")

        con1 = self.parsed["con1"]

        conditionals = con1.split("|")
        values = self.__validateDatabase(self.cursor, table)
        columnNames = values.split("(")[1].split(",")
        numberOfColumns = len(columnNames)

        formattedColumnNames = [columnName.strip(")").strip() for columnName in columnNames]
        joinedColumnNames = (", ").join(formattedColumnNames)

        redirConn, redirCursor = Database.connect("localhost", "root", "YJH030412yjh_g", self.schema)# Change database
        # redirConn, redirCursor = Database.connect("localhost", "root", "8811967", self.schema)

        silentQuery = f"SELECT * FROM testing.{table} LIMIT 1"
        redirCursor.execute(silentQuery)
        res = redirCursor.fetchall()
        if len(res) == 0:
            abort(404, message = f"The table {table} does not exist, check your spelling, and try again.")

        lengthOfGivenValues, quotedValues = self.__join(*conditionals)
        if numberOfColumns > lengthOfGivenValues:
            abort(406, message = f"You are required to fill a total of {numberOfColumns} but you have only entered in {lengthOfGivenValues}. Please fill up all the fields and then try again.")

        if invoiceno != "n":
            formattedColumnNamesForUpdate = Database.forUpdate(joinedColumnNames, quotedValues)
            sqlQuery = f"UPDATE {self.schema}.{table} SET {formattedColumnNamesForUpdate} WHERE {wheres[table]} = '{invoiceno}'"
        else:
            sqlQuery = f"INSERT INTO {self.schema}.{table} ({joinedColumnNames}) VALUES ({quotedValues})"

        """Uncomment (remove the hashtags from) the next two lines when the prerequisites have been setup."""
        # redirCursor.execute(sqlQuery)
        # redirConn.commit()

        return {201: "successfully added a new entry.", "query" : sqlQuery}

