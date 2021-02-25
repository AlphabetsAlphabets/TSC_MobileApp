import datetime
import decimal
from typing import Union

import mysql.connector
from flask_restful import abort

"""All types from third party modules will be prefixed with TYPE and will all be uppercase."""
from mysql.connector.cursor import MySQLCursor as TYPE_CURSOR
from mysql.connector.connection import MySQLConnection as TYPE_SQL_CONN
from mysql.connector.errors import ProgrammingError as E_PROGRAMMING_ERROR

class Database:
    def connect(host: str, user: str, password: str, database: str) -> Union[TYPE_SQL_CONN, TYPE_CURSOR]:
        """Connects to a MySQL database assuming all entered information is valid."""
        try:
            connection = mysql.connector.connect(
                host = host, user = user, password = password,
                database = database, auth_plugin="mysql_native_password"
            )
            cursor = connection.cursor()

        except E_PROGRAMMING_ERROR as e:
            errno = e.errno
            if errno != 1049:
                abort(404, message=f"Database '{database}' does not exist.")

        return connection, cursor

    def __json_default(self, value: Union[decimal.Decimal, datetime.date]) -> str: 
        if isinstance(value, decimal.Decimal):
            return str(float(value)) + "0"
            
        elif isinstance(value, datetime.date):
            return str(value.strftime("%Y-%m-%d"))

        elif isinstance(value, datetime.datetime):
            return str(value.strftime("%Y-%m-%d %H:%M:%S"))

        elif isinstance(value, datetime.timedelta):
            return str(value)
        

    def toSerialisable(self, data: tuple) -> list:
        data = [list(c) for c in data]

        return data

    def keyValuePairing(self, cursor: TYPE_CURSOR, res: list) -> dict:
        """
        Transforms a list of unserialisable objects, to a dictionary. Which is serialisable. 
        """
        description = cursor.description # Gets all the column names
        desc = [description[c][0] for c, _ in enumerate(description)] # Storing all the column names into a list, previously = (fid, 0, 0, 0) after formatting = fid

        keyValuePairs = {k: v for k, v in zip(desc, res[0])} # Dictionary comprehension, with column names (desc) as a key, and the values (res) as its vaue.
        for k in keyValuePairs:
            returnValue = self.__json_default(k)
            if returnValue is not None:
                keyValuePairs[k] = returnValue

        return keyValuePairs


    def formatEntries(self, result: list) -> list:
        """Converts any MySQL database types to native python types."""
        formattedRes = self.toSerialisable(result)
        for con in formattedRes:
            for n, v in enumerate(con):
                returnValue = self.__json_default(v)
                if returnValue is not None:
                    con[n] = returnValue

                elif v == None:
                    con[n] = ""

        return formattedRes
        
    def getColumnNames(cursor: TYPE_CURSOR, names: list) -> str:
        """Takes in a list of column names, and stiches them together to form a string of all the column names."""
        baseString = ""
        for name in names:
            if name == names[0]:
                baseString += name
            elif name == names[-1]:
                baseString += ", " + name
            else:
                baseString += ", " + name

        return baseString

    def columnNamesForInsert(cursor: TYPE_CURSOR, names: list = None) -> str:
        if names == None:
            description = cursor.description # Gets all the column names
            formattedName = [description[c][0] for c, _ in enumerate(description)] # Storing all the column names into a list, previously = (fid, 0, 0, 0) after formatting = fid

        elif names != None:
            names = names.split(", ")
            formattedName  = [f"`{name}`" for name in names]

        baseString = ""
        for name in formattedName:
            if name == formattedName[0]:
                baseString += name
            elif name == formattedName[-1]:
                baseString += ", " + name
            else:
                baseString += ", " + name
                
        return baseString

    def forUpdate(columnNames: str, values: list) -> str:
        valid = [f"{column} = {value}" for column, value in zip(columnNames.split(", "), values.split(", ")) if len(value.strip()) != 0]

        return (", ").join(valid)

