# Schema: testing
import datetime
import sqlite3  # local
import time
from typing import Union

import mysql.connector  # pip install mysql-connector-python
from flask_restful import (Resource, abort,  # pip install flask-restful
                           reqparse)

from QA.database import Database
from QA.key import Key

# Error types
from mysql.connector.errors import InterfaceError as TYPE_INTERFACE_ERROR

import os

class Sync(Resource):
    def __init__(self):
        """
        When a new POST request is made, an instance of Sync is spawned immediately, and automatically which connects to a MySQL database on the server, 
        and a local SQLite database. 
        """
        self.schema = "testing"
        self.table = "trans"
        try:
            # self.sqlConn, self.sqlCursor = Database().connect("localhost", "root", "8811967", "tsc_office")
            self.sqlConn, self.sqlCursor = Database.connect("localhost", "root", "YJH030412yjh_g", self.schema)
        except TYPE_INTERFACE_ERROR:
            abort(400, message="MySQL table not found. Or connection denied, check credentials")
        
        """
        These are the arguments the Sync endpoint will accept. If a request is sent through postman (Google it if you don't know what it is), it doesn't matter if
        its from the 'Params' tab or a 'Body' tab. The only difference is the parameters will show up in the url if you choose 'Params', and it won't
        shop up if you choose 'Body'
        """

        parser = reqparse.RequestParser()
        parser.add_argument("key", type=str, location='headers')
        parser.add_argument("user", type=str, location='headers')

        parsed = parser.parse_args()
        key, user = parsed["key"], parsed["user"]


        Key().verifyKey(user, key) # add key, user

        try:
            print("connecting to sqlite3")
            os.chdir("..\\env-api\\endpoints\\databases")
            PATH = os.getcwd() + "\\syncDb.db"

            self.liteCon = sqlite3.connect(PATH) 
            self.liteCursor = self.liteCon.cursor()

        except FileNotFoundError as e:
            if e == FileNotFoundError:
                error = e.strerror
                abort(400, message=f"{error}. One should be created now. Try again.")

    def __join(self, *values: list) -> Union[str, int]:
        """
        Strips all whitespace in front, and after the conditions. To make it cleaner, spaces between words will not be stripped.
        Add quotes to all values, then joins them all with ", " between each word.
        """
        strippedValues = [str(value).strip() for value in values if len(str(value).strip()) != 0]
        quotedValues = [f"'{value}'" for value in strippedValues]
        joinedValues = (", ").join(quotedValues)

        return joinedValues

    def put(self, salesperson):
        self.person = salesperson
        
        valid = self.__dataProcessing()
        list(map(self.__insertStatement, valid))
        self.__updateCounter()

        return [{201: f"Successfully synced."}]

    def __dataProcessing(self) -> Union[list, None]:
        sqlString = f"SELECT * FROM {self.schema}.{self.table} WHERE salesperson = '{self.person}' and fbydate >= CURDATE() - INTERVAL 3 day" # This causes an extra element in a tuple in a list
        self.sqlCursor.execute(sqlString) 
        resSql = self.sqlCursor.fetchall()

        try:
            liteString = f"SELECT * FROM `transaction` WHERE salesperson = '{self.person}' and fbydate >= date('now', '-3 days')"
            self.liteCursor.execute(liteString)
            resLite = self.liteCursor.fetchall()

        except Exception as e:
            print(e)
            abort(404, message = "SQLite3 table not found.")

        if len(resSql) == len(resLite) and (len(resLite) != 0):
            abort(403, message = "Cannot make duplicate entries.")

        elif len(resLite) == 0:
            abort(403, message = "You have zero entries.")

        
        resSql = Database().formatEntries(resSql)

        valid = [lite for lite in resLite if lite not in resSql]
        
        return valid

    def __updateCounter(self) -> None:
        liteQuery = f"UPDATE `transaction` SET counter = '1' WHERE counter = '0' AND salesperson = '{self.person}' AND fbydate >= date('now', '-3 day')"
        self.liteCursor.execute(liteQuery)
        self.liteCon.commit()

        sqlQuery = f"UPDATE {self.schema}.{self.table} SET counter = '1' WHERE counter = '0' AND salesperson = '{self.person}' AND fbydate >= CURDATE() - INTERVAL 3 day"
        self.sqlCursor.execute(sqlQuery)
        self.sqlConn.commit()

        liteQuery = f"UPDATE `transaction` SET counter = '2' WHERE counter = '1' AND salesperson = '{self.person}' AND fbydate >= date('now', '-3 day')"
        self.liteCursor.execute(liteQuery)
        self.liteCon.commit()

        sqlQuery = f"UPDATE {self.schema}.{self.table} SET counter = '2' WHERE counter = '1' AND salesperson = '{self.person}' AND fbydate >= CURDATE() - INTERVAL 3 day"
        self.sqlCursor.execute(sqlQuery)
        self.sqlConn.commit()

    def __insertStatement(self, values) -> None:
        columns = Database.columnNamesForInsert(self.sqlCursor)
        quotedValues = self.__join(*values)
        sqlQuery = f"INSERT INTO {self.schema}.{self.table} ({columns}) VALUES ({quotedValues})"

        self.sqlCursor.execute(sqlQuery)
        self.sqlConn.commit()
    